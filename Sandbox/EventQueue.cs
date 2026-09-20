using System;
using System.Collections.Generic;

namespace Tarotro.Sequencing
{
    /// <summary>
    /// Named lanes. Events in different lanes never block each other, so an
    /// achievement popup cannot stall the scoring sequence and vice versa.
    /// </summary>
    public static class Lane
    {
        public const string Base = "base";
        public const string Unlock = "unlock";
        public const string Achievement = "achievement";
        public const string Tutorial = "tutorial";
        public const string Other = "other";

        public static readonly string[] All = { Base, Unlock, Achievement, Tutorial, Other };
    }

    /// <summary>
    /// Port of Balatro's <c>EventManager</c> (engine/event.lua).
    ///
    /// Deliberately plain C#: no MonoBehaviour, no async, no allocation per step.
    /// Drive it from one place (see SequenceRunner) and keep all gameplay
    /// sequencing inside it. Do not replace this with UniTask or R3 for the
    /// scoring path — ordered, synchronous, single-threaded resolution is what
    /// makes a seed reproducible.
    /// </summary>
    public sealed class EventQueue
    {
        /// <summary>Queue is evaluated on a fixed 1/60 s cadence, as in the source.</summary>
        public const float StepSeconds = 1f / 60f;

        private readonly Dictionary<string, List<GameEvent>> _lanes =
            new Dictionary<string, List<GameEvent>>(StringComparer.Ordinal);

        private EventStep _step;          // reused; no per-event allocation
        private float _lastProcessed;

        /// <summary>Wall clock. Advances even while paused.</summary>
        public float RealTime { get; private set; }

        /// <summary>Game clock. Frozen while paused, scaled by <see cref="Speed"/>.</summary>
        public float TotalTime { get; private set; }

        public bool Paused { get; set; }

        /// <summary>Global sequence speed. Balatro's SPEEDFACTOR; expose it as a game setting.</summary>
        public float Speed { get; set; } = 1f;

        public EventQueue()
        {
            foreach (var lane in Lane.All)
                _lanes[lane] = new List<GameEvent>(32);
        }

        public float TimeOn(EventClock clock)
            => clock == EventClock.Real ? RealTime : TotalTime;

        // ---------------------------------------------------------------------
        // Enqueue
        // ---------------------------------------------------------------------

        public GameEvent Add(GameEvent e, string lane = Lane.Base, bool front = false)
        {
            if (e == null) return null;

            if (!_lanes.TryGetValue(lane, out var list))
            {
                list = new List<GameEvent>(16);
                _lanes[lane] = list;
            }

            e.Prime(this);

            if (front) list.Insert(0, e);
            else list.Add(e);

            return e;
        }

        /// <summary>Convenience: <c>Queue.Wait(0.4f)</c> reads like Balatro's <c>delay(0.4)</c>.</summary>
        public GameEvent Wait(float seconds, string lane = Lane.Base)
            => Add(GameEvent.Wait(seconds), lane);

        public GameEvent Do(Action action, string lane = Lane.Base)
            => Add(GameEvent.Immediate(action), lane);

        public int Count(string lane = Lane.Base)
            => _lanes.TryGetValue(lane, out var list) ? list.Count : 0;

        public bool IsBusy(string lane = Lane.Base) => Count(lane) > 0;

        // ---------------------------------------------------------------------
        // Clear
        // ---------------------------------------------------------------------

        /// <summary>Clears every lane. Events flagged NoDelete survive.</summary>
        public void ClearAll()
        {
            foreach (var kv in _lanes) Prune(kv.Value);
        }

        /// <summary>Clears every lane except <paramref name="exceptLane"/>.</summary>
        public void ClearAllExcept(string exceptLane)
        {
            foreach (var kv in _lanes)
                if (!string.Equals(kv.Key, exceptLane, StringComparison.Ordinal))
                    Prune(kv.Value);
        }

        public void Clear(string lane)
        {
            if (_lanes.TryGetValue(lane, out var list)) Prune(list);
        }

        private static void Prune(List<GameEvent> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
                if (!list[i].NoDelete) list.RemoveAt(i);
        }

        // ---------------------------------------------------------------------
        // Tick
        // ---------------------------------------------------------------------

        /// <summary>
        /// Call once per frame with unscaled delta time.
        /// </summary>
        /// <param name="forced">
        /// Bypass the fixed-step gate and process immediately. Balatro uses this when
        /// it needs the queue drained inside a single frame (state transitions, loading).
        /// </param>
        public void Tick(float unscaledDeltaTime, bool forced = false)
        {
            RealTime += unscaledDeltaTime;
            if (!Paused) TotalTime += unscaledDeltaTime * Speed;

            _lastProcessed += unscaledDeltaTime;

            // At most one pass per frame, exactly as in the source. This is a
            // throttle on how often the queue is *checked*; event timing is measured
            // against the clocks above, so a 30 fps device does not run at half speed.
            if (!forced && _lastProcessed < StepSeconds) return;
            if (!forced) _lastProcessed -= StepSeconds;
            if (_lastProcessed > StepSeconds * 4f) _lastProcessed = StepSeconds; // don't spiral after a hitch

            foreach (var kv in _lanes)
                ProcessLane(kv.Value);
        }

        private void ProcessLane(List<GameEvent> list)
        {
            bool blocked = false;
            int i = 0;

            while (i < list.Count)
            {
                var e = list[i];
                _step.Reset();

                if (!blocked || !e.Blockable)
                    e.Handle(this, ref _step);

                if (_step.PauseSkip)
                {
                    i++;
                    continue;
                }

                if (!blocked && _step.Blocking) blocked = true;

                // Removal requires BOTH: the work finished and the time elapsed.
                if (_step.Completed && _step.TimeDone) list.RemoveAt(i);
                else i++;
            }
        }
    }
}
