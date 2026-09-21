using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Tarotro.Game.Logic;
using Tarotro.Game.Logic.Motion;
using Tarotro.Game.Logic.Sequencing;
using Tarotro.Game.Utils;
using Tarotro.Game.View;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace Tarotro.Game.Presenters {
    public class CardHandPresenter : IDisposable {
        private class CardViewBinding {
            public CardHand.CardSlot Slot;
            public CardPresenter Presenter;
            public CardView View;
            public MoveableView MoveableView;
            public GameObject GameObject;
            public AsyncOperationHandle<Sprite>? SpriteHandle;
        }

        private readonly CardHand _cardHand;
        private readonly EventQueue _queue;
        private readonly IGameLogger _logger;

        private readonly List<CardViewBinding> _viewBindings = new List<CardViewBinding>();

        private Action<MoveableView> _registerView;
        private Action<MoveableView> _unregisterView;
        private GameObject _cardPrefab;
        private AsyncOperationHandle<GameObject> _prefabHandle;
        private bool _prefabLoaded;

        private Transform2D _deckPosition = new Transform2D(0.5f, 4f, 1.2f, 1.8f);
        private Transform2D _discardPosition = new Transform2D(18f, 4f, 1.2f, 1.8f);

        public const string CardPrefabPath = "Bundles/Cards/card.prefab";

        [Inject]
        public CardHandPresenter(CardHand cardHand, EventQueue queue, IGameLogger logger, CardHandConfig config = null) {
            _cardHand = cardHand;
            _queue = queue;
            _logger = logger;

            if (config != null) {
                Configure(config.HandArea, config.DeckPosition, config.DiscardPosition,
                    config.cardWidth, config.cardHeight, config.softLimit);
            }
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
            _cardHand.Configure(handArea, cardWidth, cardHeight, softLimit);
            _deckPosition = deckPosition;
            _discardPosition = discardPosition;
        }

        public async UniTask LoadPrefab(CancellationToken ct = default) {
            if (_prefabLoaded) {
                return;
            }

            _prefabHandle = Addressables.LoadAssetAsync<GameObject>(CardPrefabPath);
            _cardPrefab = await _prefabHandle.ToUniTask(cancellationToken: ct);
            _prefabLoaded = true;
            _logger.Info("Card prefab loaded", "CardHandPresenter");
        }

        public async UniTask DealCards(IReadOnlyList<CardModel> cards, CancellationToken ct = default) {
            if (!_prefabLoaded) {
                await LoadPrefab(ct);
            }

            var pending = new List<CardViewBinding>(cards.Count);

            foreach (var model in cards) {
                var slot = _cardHand.CreateSlot(model);
                var binding = CreateViewBinding(slot);
                slot.Moveable.HardSet(_deckPosition);
                _cardHand.PrepareSlot(slot);
                _viewBindings.Add(binding);
                pending.Add(binding);

                if (!string.IsNullOrEmpty(model.SpritePath)) {
                    LoadCardSprite(binding, model.SpritePath).Forget();
                }
            }

            for (var i = 0; i < pending.Count; i++) {
                var binding = pending[i];
                _queue.Add(GameEvent.After(0.06f, () => {
                    _cardHand.ActivateSlot(binding.Slot);
                    binding.Slot.Moveable.JuiceUp(0.3f);
                }));
            }

            _logger.Info($"Dealing {cards.Count} cards", "CardHandPresenter");
        }

        public void DiscardAll() {
            var count = _viewBindings.Count;
            if (count == 0) {
                return;
            }

            var discarding = new List<CardViewBinding>(_viewBindings);

            foreach (var binding in discarding) {
                _cardHand.DetachSlot(binding.Slot);
                binding.Slot.Moveable.T = _discardPosition;
            }

            for (var i = 0; i < discarding.Count; i++) {
                var binding = discarding[i];
                _queue.Add(GameEvent.After(0.04f, () => {
                    binding.Slot.Moveable.JuiceUp(0.2f);
                }));
            }

            _queue.Add(GameEvent.After(0.5f, () => {
                foreach (var binding in discarding) {
                    _cardHand.UnregisterMoveable(binding.Slot.Moveable);
                }

                DestroyAllViewBindings();
            }));

            _logger.Info("Discarding hand", "CardHandPresenter");
        }

        public void RemoveCard(int index) {
            if (index < 0 || index >= _viewBindings.Count) {
                return;
            }

            DestroyViewBinding(_viewBindings[index]);
            _viewBindings.RemoveAt(index);
            _cardHand.RemoveCard(index);
        }

        public void SetHighlighted(int index, bool highlighted) {
            if (index < 0 || index >= _viewBindings.Count) {
                return;
            }

            _cardHand.SetHighlighted(index, highlighted);
            _viewBindings[index].Presenter.SetHighlighted(highlighted);
        }

        private CardViewBinding CreateViewBinding(CardHand.CardSlot slot) {
            var go = UnityEngine.Object.Instantiate(_cardPrefab);
            var view = go.GetComponent<CardView>();
            var moveableView = go.GetComponent<MoveableView>();

            moveableView.Bind(slot.Moveable);
            _registerView?.Invoke(moveableView);

            var presenter = new CardPresenter(slot.Model, view);

            return new CardViewBinding {
                Slot = slot,
                Presenter = presenter,
                View = view,
                MoveableView = moveableView,
                GameObject = go
            };
        }

        private async UniTaskVoid LoadCardSprite(CardViewBinding binding, string spritePath) {
            try {
                var handle = Addressables.LoadAssetAsync<Sprite>(spritePath);
                var sprite = await handle.ToUniTask();
                if (binding.View != null) {
                    binding.SpriteHandle = handle;
                    binding.View.SetSprite(sprite);
                } else {
                    Addressables.Release(handle);
                }
            } catch (Exception e) {
                _logger.Warning($"Failed to load card sprite '{spritePath}': {e.Message}", "CardHandPresenter");
            }
        }

        private void DestroyAllViewBindings() {
            foreach (var binding in _viewBindings) {
                DestroyViewBinding(binding);
            }

            _viewBindings.Clear();
        }

        private void DestroyViewBinding(CardViewBinding binding) {
            binding.Presenter.Dispose();
            _unregisterView?.Invoke(binding.MoveableView);

            if (binding.SpriteHandle.HasValue && binding.SpriteHandle.Value.IsValid()) {
                Addressables.Release(binding.SpriteHandle.Value);
            }

            if (binding.GameObject != null) {
                UnityEngine.Object.Destroy(binding.GameObject);
            }
        }

        public void Dispose() {
            DestroyAllViewBindings();
            _cardHand.Clear();

            if (_prefabLoaded && _prefabHandle.IsValid()) {
                Addressables.Release(_prefabHandle);
            }
        }
    }
}
