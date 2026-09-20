using System;
using UnityEngine;

namespace Tarotro.Motion
{
    /// <summary>
    /// A 2D transform in game units. X/Y is the top-left corner, matching the source,
    /// so W/H participate in centring maths. R is radians. Scale is a centre-adjusted
    /// multiplier applied on top of W/H.
    /// </summary>
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

    /// <summary>
    /// Per-frame smoothing coefficients. Computed once for the whole scene, not per object —
    /// they only depend on delta time. Source: game.lua:2617-2624.
    /// </summary>
    public readonly struct MotionFrame
    {
        public readonly float ExpPosition;
        public readonly float ExpScale;
        public readonly float ExpRotation;
        public readonly float MoveDelta;
        public readonly float MaxStep;     // max displacement this frame
        public readonly float RealTime;

        public MotionFrame(float unscaledDt, float realTime, MotionTuning t)
        {
            ExpPosition = Mathf.Exp(-t.PositionRate * unscaledDt);
            ExpScale    = Mathf.Exp(-t.ScaleRate * unscaledDt);
            ExpRotation = Mathf.Exp(-t.RotationRate * unscaledDt);
            MoveDelta   = Mathf.Min(t.MaxMoveDelta, unscaledDt);
            MaxStep     = t.MaxSpeed * MoveDelta;
            RealTime    = realTime;
        }
    }

    /// <summary>
    /// Port of Balatro's <c>Moveable</c> (engine/moveable.lua).
    ///
    /// The whole idea in one sentence: gameplay code writes <see cref="T"/> instantly
    /// and never animates anything; the visual transform <see cref="VT"/> chases it with
    /// framerate-independent exponential smoothing. Interruptions, re-targets and
    /// cancellations are free, because there is no tween to kill — you just assign a
    /// new target and the same integrator keeps running.
    ///
    /// Plain C# on purpose. No MonoBehaviour, no Update, no DOTween. Drive it from
    /// <see cref="MotionSystem"/>; render it with <see cref="MoveableView"/>.
    /// </summary>
    public class Moveable
    {
        public Transform2D T;   // target  — logic writes this
        public Transform2D VT;  // visual  — renderer reads this

        private Vector2 _velocity;      // per-frame displacement, not units/second
        private float _velocityScale;
        private float _velocityRotation;

        // Interaction state, fed by your input layer.
        public bool Hovered;
        public bool Dragged;

        /// <summary>Whether hover/drag inflate the scale. Off for static UI.</summary>
        public bool ZoomOnInteract = true;

        /// <summary>Eases W/H to zero when set. Balatro's pinch, used for collapsing panels.</summary>
        public bool PinchX, PinchY;

        /// <summary>True when nothing moved this frame. Use it to skip redraws.</summary>
        public bool Stationary { get; private set; }

        /// <summary>Signed -1..1 horizontal offset from screen centre; drives shadow offset.</summary>
        public float ShadowParallax { get; private set; }

        public MotionTuning Tuning = null;
        private MotionTuning Tune => Tuning != null ? Tuning : MotionTuning.Default;

        // --- juice ------------------------------------------------------------
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

        /// <summary>Teleport: set target and visual together, killing all velocity.</summary>
        public void HardSet(Transform2D t)
        {
            T = t;
            VT = t;
            _velocity = Vector2.zero;
            _velocityScale = 0f;
            _velocityRotation = 0f;
        }

        public void HardSetPosition(float x, float y)
        {
            T.X = x; T.Y = y;
            VT.X = x; VT.Y = y;
            _velocity = Vector2.zero;
        }

        // ---------------------------------------------------------------------
        // Juice — the squash-and-wobble that fires on every meaningful event.
        // ---------------------------------------------------------------------

        /// <summary>
        /// Source: Moveable:juice_up. Amplitude decays as a cubic (scale) / quadratic
        /// (rotation) envelope over 0.4 s while a fast sine rings inside it. The visual
        /// scale is squashed immediately so the pop reads as a recoil, not a grow.
        /// </summary>
        /// <param name="amount">Scale amplitude. 0.4 is the source default; 0.8-1.0 for big hits.</param>
        /// <param name="rotationAmount">
        /// Rotation amplitude. Pass null for a random ±0.6*amount, which is what makes
        /// a row of cards wobble in different directions instead of in lockstep.
        /// Pass 0 to suppress rotation (correct for text and HUD numbers).
        /// </param>
        public void JuiceUp(float? amount = null, float? rotationAmount = null, float? realTime = null)
        {
            var tune = Tune;
            if (tune.ReducedMotion) return;

            float amt = amount ?? tune.JuiceDefaultAmount;
            float now = realTime ?? _lastRealTime;

            _juicing = true;
            _juiceScaleAmount = amt;
            _juiceRotationAmount = rotationAmount
                ?? ((UnityEngine.Random.value < 0.5f ? 1f : -1f) * tune.JuiceRotationRatio * amt);
            _juiceStart = now;
            _juiceEnd = now + tune.JuiceDuration;
            _juiceScale = 0f;
            _juiceRotation = 0f;

            VT.Scale = 1f - tune.JuicePreSquash * amt;
        }

        private float _lastRealTime;

        // ---------------------------------------------------------------------
        // Tick — call once per frame, in this order. The order matters:
        // rotation reads the position velocity computed a line earlier.
        // ---------------------------------------------------------------------

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
            bool moving = T.X != VT.X || T.Y != VT.Y
                          || Mathf.Abs(_velocity.x) > Tune.SnapDistance
                          || Mathf.Abs(_velocity.y) > Tune.SnapDistance;
            if (!moving) return;

            var tune = Tune;
            float e = f.ExpPosition;
            float pull = (1f - e) * tune.Stiffness * f.MoveDelta;

            _velocity.x = e * _velocity.x + (T.X - VT.X) * pull;
            _velocity.y = e * _velocity.y + (T.Y - VT.Y) * pull;

            float sq = _velocity.x * _velocity.x + _velocity.y * _velocity.y;
            if (sq > f.MaxStep * f.MaxStep)
            {
                float mag = Mathf.Sqrt(sq);
                _velocity.x = f.MaxStep * _velocity.x / mag;
                _velocity.y = f.MaxStep * _velocity.y / mag;
            }

            Stationary = false;
            VT.X += _velocity.x;
            VT.Y += _velocity.y;

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

            // The lean: horizontal speed tilts the object. This one term is most of
            // why cards flying across the screen look like objects rather than sprites.
            float speedLean = f.MoveDelta > 0f
                ? tune.RotationFromSpeed * _velocity.x / f.MoveDelta
                : 0f;

            float desired = T.R + speedLean + _juiceRotation * tune.JuiceRotationGain;

            if (desired != VT.R || Mathf.Abs(_velocityRotation) > tune.SnapAngle)
            {
                Stationary = false;
                _velocityRotation = f.ExpRotation * _velocityRotation
                                    + (1f - f.ExpRotation) * (desired - VT.R);
                VT.R += _velocityRotation;
            }

            if (Mathf.Abs(VT.R - T.R) < tune.SnapAngle && Mathf.Abs(_velocityRotation) < tune.SnapAngle)
            {
                VT.R = T.R;
                _velocityRotation = 0f;
            }
        }

        private void MoveScale(in MotionFrame f)
        {
            var tune = Tune;

            float interaction = ZoomOnInteract
                ? (Dragged ? tune.DragScaleBonus : 0f) + (Hovered ? tune.HoverScaleBonus : 0f)
                : 0f;

            float desired = T.Scale + interaction + _juiceScale;

            if (desired != VT.Scale || Mathf.Abs(_velocityScale) > tune.SnapScale)
            {
                Stationary = false;
                _velocityScale = f.ExpScale * _velocityScale
                                 + (1f - f.ExpScale) * (desired - VT.Scale);
                VT.Scale += _velocityScale;
            }
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
