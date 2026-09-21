using System;
using System.Collections.Generic;
using Tarotro.Game.Logic.Motion;
using VContainer;

namespace Tarotro.Game.Logic {
    public class CardHand : IDisposable {
        public class CardSlot {
            public CardModel Model;
            public Moveable Moveable;
        }

        private readonly MotionSystem _motion;

        private readonly List<CardSlot> _slots = new List<CardSlot>();
        private readonly List<Moveable> _moveables = new List<Moveable>();
        private readonly List<bool> _highlights = new List<bool>();

        private Transform2D _handArea = new Transform2D(2f, 8f, 16f, 2.5f);
        private float _cardWidth = 1.2f;
        private float _cardHeight = 1.8f;
        private int _softLimit = 8;

        public IReadOnlyList<CardSlot> Slots => _slots;
        public int Count => _slots.Count;

        [Inject]
        public CardHand(MotionSystem motion) {
            _motion = motion;
        }

        public void Configure(
            Transform2D handArea,
            float cardWidth = 1.2f,
            float cardHeight = 1.8f,
            int softLimit = 8) {
            _handArea = handArea;
            _cardWidth = cardWidth;
            _cardHeight = cardHeight;
            _softLimit = softLimit;
        }

        public CardSlot CreateSlot(CardModel model) {
            var moveable = new Moveable(Transform2D.Card(0, 0, _cardWidth, _cardHeight));
            return new CardSlot {
                Model = model,
                Moveable = moveable
            };
        }

        public void CommitSlot(CardSlot slot) {
            PrepareSlot(slot);
            ActivateSlot(slot);
        }

        public void PrepareSlot(CardSlot slot) {
            _slots.Add(slot);
            _moveables.Add(slot.Moveable);
            _highlights.Add(false);
        }

        public void ActivateSlot(CardSlot slot) {
            _motion.Register(slot.Moveable);
        }

        public void RemoveCard(int index) {
            if (index < 0 || index >= _slots.Count) {
                return;
            }

            _motion.Unregister(_slots[index].Moveable);
            _slots.RemoveAt(index);
            _moveables.RemoveAt(index);
            _highlights.RemoveAt(index);
        }

        public void DetachSlot(CardSlot slot) {
            var index = _slots.IndexOf(slot);
            if (index < 0) {
                return;
            }

            _slots.RemoveAt(index);
            _moveables.RemoveAt(index);
            _highlights.RemoveAt(index);
        }

        public void UnregisterMoveable(Moveable moveable) {
            _motion.Unregister(moveable);
        }

        public void Clear() {
            foreach (var slot in _slots) {
                _motion.Unregister(slot.Moveable);
            }

            _slots.Clear();
            _moveables.Clear();
            _highlights.Clear();
        }

        public void SetHighlighted(int index, bool highlighted) {
            if (index < 0 || index >= _slots.Count) {
                return;
            }

            _highlights[index] = highlighted;
        }

        public void Tick() {
            if (_moveables.Count == 0) {
                return;
            }

            HandLayout.Apply(
                _handArea,
                _moveables,
                _highlights,
                _cardWidth,
                _softLimit,
                _motion.RealTime);
        }

        public void Dispose() {
            Clear();
        }
    }
}
