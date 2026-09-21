using System;
using System.Collections.Generic;

namespace Tarotro.Game.Logic.Sequencing
{
    public static class Lane
    {
        public const string Base = "base";
        public const string Unlock = "unlock";
        public const string Achievement = "achievement";
        public const string Tutorial = "tutorial";
        public const string Other = "other";

        public static readonly string[] All = { Base, Unlock, Achievement, Tutorial, Other };
    }

    public sealed class EventQueue
    {
        public const float StepSeconds = 1f / 60f;

        private readonly Dictionary<string, List<GameEvent>> _lanes =
            new Dictionary<string, List<GameEvent>>(StringComparer.Ordinal);

        private EventStep _step;
        private float _lastProcessed;

        public float RealTime { get; private set; }
        public float TotalTime { get; private set; }
        public bool Paused { get; set; }
        public float Speed { get; set; } = 1f;

        public EventQueue()
        {
            foreach (var lane in Lane.All)
                _lanes[lane] = new List<GameEvent>(32);
        }

        public float TimeOn(EventClock clock)
            => clock == EventClock.Real ? RealTime : TotalTime;

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

        public GameEvent Wait(float seconds, string lane = Lane.Base)
            => Add(GameEvent.Wait(seconds), lane);

        public GameEvent Do(Action action, string lane = Lane.Base)
            => Add(GameEvent.Immediate(action), lane);

        public int Count(string lane = Lane.Base)
            => _lanes.TryGetValue(lane, out var list) ? list.Count : 0;

        public bool IsBusy(string lane = Lane.Base) => Count(lane) > 0;

        public void ClearAll()
        {
            foreach (var kv in _lanes) Prune(kv.Value);
        }

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

        public void Tick(float unscaledDeltaTime, bool forced = false)
        {
            RealTime += unscaledDeltaTime;
            if (!Paused) TotalTime += unscaledDeltaTime * Speed;

            _lastProcessed += unscaledDeltaTime;

            if (!forced && _lastProcessed < StepSeconds) return;
            if (!forced) _lastProcessed -= StepSeconds;
            if (_lastProcessed > StepSeconds * 4f) _lastProcessed = StepSeconds;

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

                if (_step.Completed && _step.TimeDone) list.RemoveAt(i);
                else i++;
            }
        }
    }
}
