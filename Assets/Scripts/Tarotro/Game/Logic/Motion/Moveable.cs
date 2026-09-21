using System;
using UnityEngine;

namespace Tarotro.Game.Logic.Motion
{
    [Serializable]
    public struct Transform2D
    {
        public float X, Y, W, H, R, Scale;

        public Transform2D(float x, float y, float w, float h, float r = 0f, float scale = 1f)
        {
            X = x; Y = y; W = w; H = h; R = r; Scale = scale;
        }

        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set { X = value.x; Y = value.y; }
        }

        public Vector2 Centre => new Vector2(X + W * 0.5f, Y + H * 0.5f);

        public static Transform2D Card(float x, float y, float w, float h)
            => new Transform2D(x, y, w, h, 0f, 1f);
    }

    public readonly struct MotionFrame
    {
        public readonly float ExpPosition;
        public readonly float ExpScale;
        public readonly float ExpRotation;
        public readonly float MoveDelta;
        public readonly float MaxVelocity;
        public readonly float RealTime;

        public MotionFrame(float unscaledDt, float realTime, MotionTuning t)
        {
            float dt = Mathf.Min(t.MaxMoveDelta, unscaledDt);
            ExpPosition = Mathf.Exp(-t.PositionRate * dt);
            ExpScale    = Mathf.Exp(-t.ScaleRate * dt);
            ExpRotation = Mathf.Exp(-t.RotationRate * dt);
            MoveDelta   = dt;
            MaxVelocity = t.MaxSpeed * dt;
            RealTime    = realTime;
        }
    }

    public class Moveable
    {
        public Transform2D T;
        public Transform2D VT;

        private Vector2 _velocity;

        public bool Hovered;
        public bool Dragged;
        public bool ZoomOnInteract = true;
        public bool PinchX, PinchY;
        public bool Stationary { get; private set; }
        public float ShadowParallax { get; private set; }

        public MotionTuning Tuning = null;
        private MotionTuning Tune => Tuning != null ? Tuning : MotionTuning.Default;

        private bool _juicing;
        private float _juiceScaleAmount, _juiceRotationAmount;
        private float _juiceStart, _juiceEnd;
        private float _juiceScale, _juiceRotation;

        public Moveable() { }

        public Moveable(Transform2D t)
        {
            T = t;
            VT = t;
        }

        public void HardSet(Transform2D t)
        {
            T = t;
            VT = t;
            _velocity = Vector2.zero;
        }

        public void HardSetPosition(float x, float y)
        {
            T.X = x; T.Y = y;
            VT.X = x; VT.Y = y;
            _velocity = Vector2.zero;
        }

        public void JuiceUp(float? amount = null, float? rotationAmount = null, float? realTime = null)
        {
            var tune = Tune;
            if (tune.ReducedMotion) return;

            float amt = amount ?? tune.JuiceDefaultAmount;

            _juicing = true;
            _juiceScaleAmount = amt;
            _juiceRotationAmount = rotationAmount
                ?? ((UnityEngine.Random.value < 0.5f ? 1f : -1f) * tune.JuiceRotationRatio * amt);

            float now = realTime ?? (_lastRealTime > 0f ? _lastRealTime : -1f);
            _juiceStart = now;
            _juiceEnd = now >= 0f ? now + tune.JuiceDuration : -1f;
            _juiceScale = 0f;
            _juiceRotation = 0f;

            VT.Scale = 1f - tune.JuicePreSquash * amt;
        }

        private float _lastRealTime;

        public void Tick(in MotionFrame f, float roomWidth = 0f)
        {
            _lastRealTime = f.RealTime;
            Stationary = true;

            MoveJuice(f.RealTime);
            MovePosition(f);
            MoveRotation(f);
            MoveScale(f);
            MoveSize(f);

            if (roomWidth > 0f) CalculateParallax(roomWidth);
        }

        private void MoveJuice(float realTime)
        {
            if (!_juicing) return;

            if (_juiceStart < 0f)
            {
                _juiceStart = realTime;
                _juiceEnd = realTime + Tune.JuiceDuration;
            }

            if (_juiceEnd < realTime)
            {
                _juicing = false;
                _juiceScale = 0f;
                _juiceRotation = 0f;
                return;
            }

            var tune = Tune;
            float elapsed = realTime - _juiceStart;
            float remaining = Mathf.Max(0f, (_juiceEnd - realTime) / (_juiceEnd - _juiceStart));

            _juiceScale = _juiceScaleAmount
                          * Mathf.Sin(tune.JuiceScaleFrequency * elapsed)
                          * (remaining * remaining * remaining);

            _juiceRotation = _juiceRotationAmount
                             * Mathf.Sin(tune.JuiceRotationFrequency * elapsed)
                             * (remaining * remaining);

            Stationary = false;
        }

        private void MovePosition(in MotionFrame f)
        {
            float errorX = T.X - VT.X;
            float errorY = T.Y - VT.Y;

            bool moving = errorX != 0f || errorY != 0f
                          || Mathf.Abs(_velocity.x) > Tune.SnapDistance
                          || Mathf.Abs(_velocity.y) > Tune.SnapDistance;
            if (!moving) return;

            var tune = Tune;
            float dt = f.MoveDelta;
            float expPos = f.ExpPosition;

            _velocity.x = expPos * _velocity.x + (1f - expPos) * errorX * tune.Stiffness * dt;
            _velocity.y = expPos * _velocity.y + (1f - expPos) * errorY * tune.Stiffness * dt;

            float sq = _velocity.x * _velocity.x + _velocity.y * _velocity.y;
            float maxVel = f.MaxVelocity;
            if (sq > maxVel * maxVel)
            {
                float inv = maxVel / Mathf.Sqrt(sq);
                _velocity.x *= inv;
                _velocity.y *= inv;
            }

            VT.X += _velocity.x;
            VT.Y += _velocity.y;

            Stationary = false;

            float snap = tune.SnapDistance;
            if (Mathf.Abs(VT.X - T.X) < snap && Mathf.Abs(_velocity.x) < snap)
            {
                VT.X = T.X;
                _velocity.x = 0f;
            }
            if (Mathf.Abs(VT.Y - T.Y) < snap && Mathf.Abs(_velocity.y) < snap)
            {
                VT.Y = T.Y;
                _velocity.y = 0f;
            }
        }

        private void MoveRotation(in MotionFrame f)
        {
            var tune = Tune;

            float speedLean = f.MoveDelta > 0f
                ? Mathf.Clamp(tune.RotationFromSpeed * _velocity.x / f.MoveDelta,
                    -tune.MaxSpeedLean, tune.MaxSpeedLean)
                : 0f;
            float desired = T.R + speedLean + _juiceRotation * tune.JuiceRotationGain;

            if (Mathf.Abs(desired - VT.R) < tune.SnapAngle)
            {
                VT.R = desired;
                return;
            }

            Stationary = false;
            VT.R += (1f - f.ExpRotation) * (desired - VT.R);
        }

        private void MoveScale(in MotionFrame f)
        {
            var tune = Tune;

            float interaction = ZoomOnInteract
                ? (Dragged ? tune.DragScaleBonus : 0f) + (Hovered ? tune.HoverScaleBonus : 0f)
                : 0f;

            float desired = T.Scale + interaction + _juiceScale;

            if (Mathf.Abs(desired - VT.Scale) < tune.SnapScale)
            {
                VT.Scale = desired;
                return;
            }

            Stationary = false;
            VT.Scale += (1f - f.ExpScale) * (desired - VT.Scale);
        }

        private void MoveSize(in MotionFrame f)
        {
            var tune = Tune;

            bool changing = (T.W != VT.W && !PinchX)
                            || (T.H != VT.H && !PinchY)
                            || (VT.W > 0f && PinchX)
                            || (VT.H > 0f && PinchY);
            if (!changing) return;

            Stationary = false;
            float step = tune.SizeRate * f.MoveDelta;

            VT.W += step * (PinchX ? -1f : 1f) * T.W;
            VT.H += step * (PinchY ? -1f : 1f) * T.H;

            VT.W = Mathf.Clamp(VT.W, 0f, T.W);
            VT.H = Mathf.Clamp(VT.H, 0f, T.H);
        }

        private void CalculateParallax(float roomWidth)
        {
            float half = roomWidth * 0.5f;
            ShadowParallax = (T.X + T.W * 0.5f - half) / half * Tune.ParallaxStrength;
        }
    }
}
