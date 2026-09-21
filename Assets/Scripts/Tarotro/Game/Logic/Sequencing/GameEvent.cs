using System;

namespace Tarotro.Game.Logic.Sequencing
{
    public enum EventTrigger
    {
        Immediate,
        After,
        Before,
        Ease,
        Condition,
    }

    public enum EventClock
    {
        Total,
        Real,
    }

    public enum EaseKind { Lerp, Quad, Elastic }

    public struct EventStep
    {
        public bool Blocking;
        public bool Completed;
        public bool TimeDone;
        public bool PauseSkip;

        public void Reset()
        {
            Blocking = false;
            Completed = false;
            TimeDone = false;
            PauseSkip = false;
        }
    }

    public sealed class GameEvent
    {
        public EventTrigger Trigger = EventTrigger.Immediate;
        public bool Blocking = true;
        public bool Blockable = true;
        public bool NoDelete;
        public float Delay;
        public EventClock Clock = EventClock.Total;
        public Func<bool> Func = AlwaysTrue;
        public bool Complete { get; private set; }

        private Func<float> _get;
        private Action<float> _set;
        private EaseKind _easeKind = EaseKind.Lerp;
        private Func<float, float> _easeFilter;
        private float _easeFrom, _easeTo, _easeStart, _easeEnd;
        private bool _easeStarted;

        private float _startedAt;
        private bool _timerStarted;
        private bool _createdWhilePaused;
        private bool _clockFromEnqueue;

        private static readonly Func<bool> AlwaysTrue = () => true;

        public static GameEvent Immediate(Func<bool> func) => new GameEvent
        {
            Trigger = EventTrigger.Immediate,
            Func = func ?? AlwaysTrue,
        };

        public static GameEvent Immediate(Action action) => new GameEvent
        {
            Trigger = EventTrigger.Immediate,
            Func = () => { action(); return true; },
        };

        public static GameEvent After(float delay, Action action) => new GameEvent
        {
            Trigger = EventTrigger.After,
            Delay = delay,
            Func = () => { action?.Invoke(); return true; },
        };

        public static GameEvent After(float delay, Func<bool> func) => new GameEvent
        {
            Trigger = EventTrigger.After,
            Delay = delay,
            Func = func ?? AlwaysTrue,
        };

        public static GameEvent Wait(float seconds) => After(seconds, (Action)null);

        public static GameEvent Until(Func<bool> condition) => new GameEvent
        {
            Trigger = EventTrigger.Condition,
            Func = condition,
        };

        public static GameEvent Ease(
            Func<float> get,
            Action<float> set,
            float to,
            float duration,
            EaseKind kind = EaseKind.Lerp,
            Func<float, float> filter = null)
        {
            if (get == null) throw new ArgumentNullException(nameof(get));
            if (set == null) throw new ArgumentNullException(nameof(set));

            return new GameEvent
            {
                Trigger = EventTrigger.Ease,
                Delay = duration,
                _get = get,
                _set = set,
                _easeTo = to,
                _easeKind = kind,
                _easeFilter = filter,
                Func = AlwaysTrue,
            };
        }

        public static GameEvent EaseInt(Func<float> get, Action<float> set, float to, float duration)
            => Ease(get, set, to, duration, EaseKind.Lerp, v => (float)Math.Floor(v));

        public GameEvent ClockFromEnqueue() { _clockFromEnqueue = true; return this; }
        public GameEvent NonBlocking() { Blocking = false; return this; }
        public GameEvent Unblockable() { Blockable = false; return this; }
        public GameEvent Persistent() { NoDelete = true; return this; }
        public GameEvent OnRealClock() { Clock = EventClock.Real; return this; }
        public GameEvent WithDelay(float d) { Delay = d; return this; }

        internal void Prime(EventQueue queue)
        {
            _createdWhilePaused = queue.Paused;
            _startedAt = queue.TimeOn(Clock);
            if (_clockFromEnqueue) _timerStarted = true;
        }

        internal void Handle(EventQueue queue, ref EventStep step)
        {
            step.Blocking = Blocking;
            step.Completed = Complete;

            if (!_createdWhilePaused && queue.Paused)
            {
                step.PauseSkip = true;
                return;
            }

            float now = queue.TimeOn(Clock);

            if (!_timerStarted)
            {
                _startedAt = now;
                _timerStarted = true;
            }

            switch (Trigger)
            {
                case EventTrigger.Immediate:
                    step.Completed = Func();
                    step.TimeDone = true;
                    break;

                case EventTrigger.After:
                    if (_startedAt + Delay <= now)
                    {
                        step.TimeDone = true;
                        step.Completed = Func();
                    }
                    break;

                case EventTrigger.Before:
                    if (!Complete) step.Completed = Func();
                    if (_startedAt + Delay <= now) step.TimeDone = true;
                    break;

                case EventTrigger.Condition:
                    if (!Complete) step.Completed = Func();
                    step.TimeDone = true;
                    break;

                case EventTrigger.Ease:
                    HandleEase(now, ref step);
                    break;
            }

            if (step.Completed) Complete = true;
        }

        private void HandleEase(float now, ref EventStep step)
        {
            if (!_easeStarted)
            {
                _easeStart = now;
                _easeEnd = now + Delay;
                _easeFrom = _get();
                _easeStarted = true;
            }

            if (Complete) return;

            if (_easeEnd >= now)
            {
                float t = _easeEnd - _easeStart <= 0f
                    ? 0f
                    : (_easeEnd - now) / (_easeEnd - _easeStart);

                switch (_easeKind)
                {
                    case EaseKind.Quad:
                        t *= t;
                        break;
                    case EaseKind.Elastic:
                        t = (float)(-Math.Pow(2.0, 10.0 * t - 10.0)
                                    * Math.Sin((t * 10.0 - 10.75) * 2.0 * Math.PI / 3.0));
                        break;
                }

                float v = t * _easeFrom + (1f - t) * _easeTo;
                _set(_easeFilter != null ? _easeFilter(v) : v);
            }
            else
            {
                _set(_easeFilter != null ? _easeFilter(_easeTo) : _easeTo);
                Complete = true;
                step.Completed = true;
                step.TimeDone = true;
            }
        }
    }
}
