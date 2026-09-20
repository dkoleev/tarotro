using System.Collections.Generic;

namespace Tarotro.Motion
{
    public sealed class MotionSystem
    {
        private readonly List<Moveable> _moveables = new List<Moveable>(256);
        private readonly MotionTuning _tuning;

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

        public void Tick(float unscaledDeltaTime)
        {
            RealTime += unscaledDeltaTime;

            var frame = new MotionFrame(unscaledDeltaTime, RealTime, _tuning);
            float room = RoomWidth;

            for (int i = 0; i < _moveables.Count; i++)
                _moveables[i].Tick(in frame, room);
        }
    }
}
