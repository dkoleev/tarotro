using System.Collections.Generic;
using UnityEngine;

namespace Tarotro.Motion
{
    /// <summary>
    /// Owns every <see cref="Moveable"/> and ticks them once per frame with a single
    /// shared <see cref="MotionFrame"/>. Source: the MOVEABLES loop in game.lua:2617-2627.
    ///
    /// Register this in VContainer as a singleton and drive it from one MonoBehaviour.
    /// One loop over a flat list beats N MonoBehaviour.Update calls — which matters on
    /// WebGL, where Unity's per-component Update dispatch is comparatively expensive
    /// and a full hand plus jokers plus HUD is easily 60+ moveables.
    /// </summary>
    public sealed class MotionSystem
    {
        private readonly List<Moveable> _moveables = new List<Moveable>(256);
        private readonly MotionTuning _tuning;

        /// <summary>Playfield width in game units. Drives shadow parallax.</summary>
        public float RoomWidth { get; set; } = 20f;

        public float RealTime { get; private set; }

        public MotionSystem(MotionTuning tuning)
        {
            _tuning = tuning != null ? tuning : MotionTuning.Default;
        }

        public MotionTuning Tuning => _tuning;

        public T Register<T>(T moveable) where T : Moveable
        {
            moveable.Tuning = _tuning;
            _moveables.Add(moveable);
            return moveable;
        }

        public void Unregister(Moveable moveable) => _moveables.Remove(moveable);

        public void Clear() => _moveables.Clear();

        /// <summary>Call once per frame with Time.unscaledDeltaTime.</summary>
        public void Tick(float unscaledDeltaTime)
        {
            RealTime += unscaledDeltaTime;

            var frame = new MotionFrame(unscaledDeltaTime, RealTime, _tuning);
            float room = RoomWidth;

            // Plain indexed for-loop: no enumerator allocation, no GC spike on WebGL.
            for (int i = 0; i < _moveables.Count; i++)
                _moveables[i].Tick(in frame, room);
        }
    }
}
