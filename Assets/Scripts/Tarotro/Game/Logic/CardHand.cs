using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Tarotro.Game.Core;
using Tarotro.Game.Presenters;
using Tarotro.Game.Utils;
using Tarotro.Game.View;
using Tarotro.Motion;
using Tarotro.Sequencing;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace Tarotro.Game.Logic {
    public class CardHand : IDisposable {
        public class CardSlot {
            public CardModel Model;
            public CardPresenter Presenter;
            public Moveable Moveable;
            public CardView View;
            public MoveableView MoveableView;
            public GameObject GameObject;
        }

        private readonly MotionSystem _motion;
        private readonly EventQueue _queue;
        private readonly IGameLogger _logger;

        private readonly List<CardSlot> _slots = new List<CardSlot>();
        private readonly List<Moveable> _moveables = new List<Moveable>();
        private readonly List<bool> _highlights = new List<bool>();

        private Action<MoveableView> _registerView;
        private Action<MoveableView> _unregisterView;
        private GameObject _cardPrefab;
        private AsyncOperationHandle<GameObject> _prefabHandle;
        private bool _prefabLoaded;

        private Transform2D _handArea = new Transform2D(2f, 8f, 16f, 2.5f);
        private Transform2D _deckPosition = new Transform2D(0.5f, 4f, 1.2f, 1.8f);
        private Transform2D _discardPosition = new Transform2D(18f, 4f, 1.2f, 1.8f);
        private float _cardWidth = 1.2f;
        private float _cardHeight = 1.8f;
        private int _softLimit = 8;

        public const string CardPrefabPath = "Bundles/Cards/card.prefab";
        public const string CardSpritePath = "Bundles/Cards/Sprites/{0}";

        public IReadOnlyList<CardSlot> Slots => _slots;
        public int Count => _slots.Count;

        [Inject]
        public CardHand(MotionSystem motion, EventQueue queue, IGameLogger logger) {
            _motion = motion;
            _queue = queue;
            _logger = logger;
        }

        public void SetViewCallbacks(Action<MoveableView> register, Action<MoveableView> unregister) {
            _registerView = register;
            _unregisterView = unregister;
        }

        public void Configure(
            Transform2D handArea,
            Transform2D deckPosition,
            Transform2D discardPosition,
            float cardWidth = 1.2f,
            float cardHeight = 1.8f,
            int softLimit = 8) {
            _handArea = handArea;
            _deckPosition = deckPosition;
            _discardPosition = discardPosition;
            _cardWidth = cardWidth;
            _cardHeight = cardHeight;
            _softLimit = softLimit;
        }

        public async UniTask LoadPrefab(CancellationToken ct = default) {
            if (_prefabLoaded) return;
            _prefabHandle = Addressables.LoadAssetAsync<GameObject>(CardPrefabPath);
            _cardPrefab = await _prefabHandle.ToUniTask(cancellationToken: ct);
            _prefabLoaded = true;
            _logger.Info("Card prefab loaded", "CardHand");
        }

        public async UniTask DealCards(IReadOnlyList<CardModel> cards, CancellationToken ct = default) {
            if (!_prefabLoaded) await LoadPrefab(ct);

            var pending = new List<CardSlot>(cards.Count);

            foreach (var model in cards) {
                var slot = CreateCardSlot(model);
                slot.Moveable.HardSet(_deckPosition);
                pending.Add(slot);

                if (!string.IsNullOrEmpty(model.SpritePath))
                    LoadCardSprite(slot, model.SpritePath).Forget();
            }

            for (var i = 0; i < pending.Count; i++) {
                var slot = pending[i];
                _queue.Add(GameEvent.After(0.06f, () => {
                    _slots.Add(slot);
                    _moveables.Add(slot.Moveable);
                    _highlights.Add(false);
                    slot.Moveable.JuiceUp(0.3f);
                }));
            }

            _logger.Info($"Dealing {cards.Count} cards", "CardHand");
        }

        public void DiscardAll() {
            var count = _slots.Count;
            for (var i = 0; i < count; i++) {
                var slot = _slots[i];
                var index = i;
                slot.Moveable.T = _discardPosition;

                _queue.Add(GameEvent.After(0.04f * index, () => {
                    slot.Moveable.JuiceUp(0.2f);
                }));
            }

            _queue.Add(GameEvent.After(0.04f * count + 0.5f, () => {
                RemoveAllSlots();
            }));

            _logger.Info("Discarding hand", "CardHand");
        }

        public void RemoveCard(int index) {
            if (index < 0 || index >= _slots.Count) return;

            var slot = _slots[index];
            DestroySlot(slot);
            _slots.RemoveAt(index);
            _moveables.RemoveAt(index);
            _highlights.RemoveAt(index);
        }

        public void SetHighlighted(int index, bool highlighted) {
            if (index < 0 || index >= _slots.Count) return;
            _highlights[index] = highlighted;
            _slots[index].Presenter.SetHighlighted(highlighted);
        }

        public void Tick() {
            if (_moveables.Count == 0) return;

            HandLayout.Apply(
                _handArea,
                _moveables,
                _highlights,
                _cardWidth,
                _softLimit,
                _motion.RealTime);
        }

        private CardSlot CreateCardSlot(CardModel model) {
            var go = UnityEngine.Object.Instantiate(_cardPrefab);
            var view = go.GetComponent<CardView>();
            var moveableView = go.GetComponent<MoveableView>();

            var moveable = new Moveable(Transform2D.Card(0, 0, _cardWidth, _cardHeight));
            _motion.Register(moveable);
            moveableView.Bind(moveable);

            _registerView?.Invoke(moveableView);

            var presenter = new CardPresenter(model, view);

            return new CardSlot {
                Model = model,
                Presenter = presenter,
                Moveable = moveable,
                View = view,
                MoveableView = moveableView,
                GameObject = go
            };
        }

        private async UniTaskVoid LoadCardSprite(CardSlot slot, string spritePath) {
            try {
                var path = string.Format(CardSpritePath, spritePath);
                var handle = Addressables.LoadAssetAsync<Sprite>(path);
                var sprite = await handle.ToUniTask();
                if (slot.View != null)
                    slot.View.SetSprite(sprite);
            } catch (Exception e) {
                _logger.Warning($"Failed to load card sprite '{spritePath}': {e.Message}", "CardHand");
            }
        }

        private void RemoveAllSlots() {
            foreach (var slot in _slots)
                DestroySlot(slot);

            _slots.Clear();
            _moveables.Clear();
            _highlights.Clear();
        }

        private void DestroySlot(CardSlot slot) {
            slot.Presenter.Dispose();
            _motion.Unregister(slot.Moveable);
            _unregisterView?.Invoke(slot.MoveableView);

            if (slot.GameObject != null)
                UnityEngine.Object.Destroy(slot.GameObject);
        }

        public void Dispose() {
            RemoveAllSlots();
            if (_prefabLoaded && _prefabHandle.IsValid())
                Addressables.Release(_prefabHandle);
        }
    }
}
