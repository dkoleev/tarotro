using System;

namespace Tarotro.Sequencing
{
    public enum EventTrigger
    {
        /// <summary>Runs once, this step. Completes when Func returns true.</summary>
        Immediate,

        /// <summary>Waits Delay, then runs Func. Blocks the queue while waiting.</summary>
        After,

        /// <summary>Runs Func every step, but does not release the queue until Delay elapses.</summary>
        Before,

        /// <summary>Drives a float from its current value to EaseTo over Delay seconds.</summary>
        Ease,

        /// <summary>Runs Func every step until it returns true. No timeout.</summary>
        Condition,
    }

    public enum EventClock
    {
        /// <summary>Game time. Stops while paused, scales with game speed.</summary>
        Total,

        /// <summary>Wall clock. Keeps running while paused. Use for UI and menus.</summary>
        Real,
    }

    public enum EaseKind { Lerp, Quad, Elastic }

    /// <summary>
    /// Result of a single <see cref="GameEvent.Handle"/> call. Passed by ref so the
    /// queue allocates nothing per step.
    /// </summary>
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

    /// <summary>
    /// One unit of sequenced work. Port of Balatro's <c>Event</c> (engine/event.lua).
    ///
    /// The contract: an event is removed from its queue only when it has both
    /// completed (Func returned true) and its time is done. While a blocking event
    /// sits at the front of a queue, every blockable event behind it is frozen.
    /// That is what lets gameplay code enqueue a whole scoring sequence in one
    /// synchronous pass and have it play out over several seconds.
    /// </summary>
    public sealed class GameEvent
    {
        public EventTrigger Trigger = EventTrigger.Immediate;

        /// <summary>Holds up blockable events behind this one in the same queue.</summary>
        public bool Blocking = true;

        /// <summary>If false, this event runs even while something ahead of it is blocking.</summary>
        public bool Blockable = true;

        /// <summary>Survives <see cref="EventQueue.Clear"/>. Use sparingly.</summary>
        public bool NoDelete;

        /// <summary>Seconds. Wait for After/Before, duration for Ease.</summary>
        public float Delay;

        public EventClock Clock = EventClock.Total;

        /// <summary>Return true when the work is done. Defaults to done-immediately.</summary>
        public Func<bool> Func = AlwaysTrue;

        public bool Complete { get; private set; }

        // --- ease state -------------------------------------------------------
        private Func<float> _get;
        private Action<float> _set;
        private EaseKind _easeKind = EaseKind.Lerp;
        private Func<float, float> _easeFilter;
        private float _easeFrom, _easeTo, _easeStart, _easeEnd;
        private bool _easeStarted;

        // --- timing -----------------------------------------------------------
        private float _startedAt;
        private bool _timerStarted;
        private bool _createdWhilePaused;
        private bool _clockFromEnqueue;

        private static readonly Func<bool> AlwaysTrue = () => true;

        // ---------------------------------------------------------------------
        // Construction
        // ---------------------------------------------------------------------

        public static GameEvent Immediate(Func<bool> func) => new GameEvent
        {
            Trigger = EventTrigger.Immediate,
            Func = func ?? AlwaysTrue,
        };

        /// <summary>Fire-and-forget action that completes in one step.</summary>
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

        /// <summary>A pure pause in the queue. Balatro's <c>delay(t)</c>.</summary>
        public static GameEvent Wait(float seconds) => After(seconds, (Action)null);

        public static GameEvent Until(Func<bool> condition) => new GameEvent
        {
            Trigger = EventTrigger.Condition,
            Func = condition,
        };

        /// <summary>
        /// Eases a float from wherever it is when this event reaches the front of the
        /// queue to <paramref name="to"/>. The start value is captured on first handle,
        /// not at construction, so queued eases compose correctly.
        /// </summary>
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

        /// <summary>Integer-stepped ease. What Balatro uses for chip and dollar counters.</summary>
        public static GameEvent EaseInt(Func<float> get, Action<float> set, float to, float duration)
            => Ease(get, set, to, duration, EaseKind.Lerp, v => (float)Math.Floor(v));

        // --- fluent modifiers -------------------------------------------------

        /// <summary>
        /// Count Delay from the moment the event is enqueued rather than from the
        /// moment it reaches the front of its lane. Balatro's <c>start_timer</c>.
        /// Use for things that should expire on wall time regardless of queue backlog.
        /// </summary>
        public GameEvent ClockFromEnqueue() { _clockFromEnqueue = true; return this; }

        public GameEvent NonBlocking() { Blocking = false; return this; }
        public GameEvent Unblockable() { Blockable = false; return this; }
        public GameEvent Persistent() { NoDelete = true; return this; }
        public GameEvent OnRealClock() { Clock = EventClock.Real; return this; }
        public GameEvent WithDelay(float d) { Delay = d; return this; }

        // ---------------------------------------------------------------------
        // Runtime
        // ---------------------------------------------------------------------

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

            // An event born while unpaused must not tick during a pause.
            if (!_createdWhilePaused && queue.Paused)
            {
                step.PauseSkip = true;
                return;
            }

            float now = queue.TimeOn(Clock);

            // The clock starts when the event first gets a turn, not when it was queued.
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
                // Note the direction: t runs 1 -> 0, matching the Lua source.
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
