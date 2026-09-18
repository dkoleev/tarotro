using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Tarotro.Game.Data;
using Tarotro.Game.Data.Save;
using Tarotro.Game.Messages;
using Tarotro.Game.Presenters;
using Tarotro.Game.Utils;
using Tarotro.Game.View;
using UnityEngine;
using VContainer;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace Tarotro.Game.Logic {
    public class Battle : IDisposable {
        private class EnemyWrapper {
            public EnemyModel Model { get; }
            public EnemyPresenter Presenter { get; }
            public ICharacterView View { get; }
            public AsyncOperationHandle<GameObject> AddressablesHandle { get; }

            public EnemyWrapper(EnemyModel model, EnemyPresenter presenter, ICharacterView view, AsyncOperationHandle<GameObject> addressablesHandle) {
                Model = model;
                Presenter = presenter;
                View = view;
                AddressablesHandle = addressablesHandle;
            }
        }

        private readonly IPublisher<EnemyDiedMessage> _enemyDiedPub;
        private readonly GameData _gameData;
        private readonly BattleProgressionManager _progressionManager;
        private readonly IGameLogger _logger;
        private EnemyWrapper _currentEnemy;
        private PlayerModel _currentPlayer;
        private CancellationTokenSource _cts;
        private FightRoundData _currentRoundData;
        private int _currentCircleIndex = 0;
        private List<FightRoundData> _currentCircle;

        [Inject]
        public Battle(IPublisher<EnemyDiedMessage> enemyDiedPub, GameData gameData, BattleProgressionManager progressionManager, IGameLogger logger) {
            _enemyDiedPub = enemyDiedPub;
            _gameData = gameData;
            _progressionManager = progressionManager;
            _logger = logger;
        }

        public async UniTask StartBattle(CancellationToken ct = default) {
            _logger.Info("Starting battle", "Battle");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // Initialize player
            CreatePlayer();

            _logger.Info("Play hand size: " + _gameData.Battle.playHandSize, "Battle");
            await CreateDesk();

            // Start first circle
            var circle = _progressionManager.GenerateCircle(CircleType.Limbo);
            _currentCircle = circle;
            _currentCircleIndex = 0;

            if (_currentCircle.Count > 0) {
                _currentRoundData = _currentCircle[0];
                await SpawnEnemyFromRound(_currentRoundData, ct);
            }
        }

        public BattleSaveData CreateSaveSnapshot() {
            var battleSave = new BattleSaveData {
                CurrentCircleIndex = _currentCircleIndex,
                CurrentCircle = _currentCircle?.Select(ToRoundSaveData).ToList(),
                CurrentRound = _currentRoundData != null ? ToRoundSaveData(_currentRoundData) : null,
                Player = CreatePlayerSnapshot(),
                CurrentEnemy = _currentEnemy != null
                    ? new EnemySaveData {
                        EnemyId = _currentEnemy.Model.Id,
                        CurrentHealth = _currentEnemy.Model.CurrentHealth
                    }
                    : null
            };

            _logger.Info("Save snapshot created", "Battle");
            return battleSave;
        }

        public async UniTask RestoreFromSave(BattleSaveData saveData, CancellationToken ct = default) {
            _logger.Info("Restoring battle from save", "Battle");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _currentCircleIndex = saveData.CurrentCircleIndex;
            _currentCircle = saveData.CurrentCircle?.Select(ToFightRoundData).ToList();
            _currentRoundData = saveData.CurrentRound != null ? ToFightRoundData(saveData.CurrentRound) : null;

            if (saveData.Player != null) {
                RestorePlayer(saveData.Player);
            } else {
                CreatePlayer();
            }

            if (saveData.CurrentEnemy != null) {
                await SpawnEnemy(saveData.CurrentEnemy.EnemyId, saveData.CurrentEnemy.CurrentHealth, ct);
            }

            _logger.Info($"Battle restored: circle index {_currentCircleIndex}, enemy {saveData.CurrentEnemy?.EnemyId ?? "none"}", "Battle");
        }

        private void StartNextRound() {
            if (_currentRoundData is null) {
                _currentRoundData = _progressionManager.GenerateRound(CircleType.Fraud, EnemyType.Common);
            }
            else {
                // Advance to next round in current circle
                _currentCircleIndex++;
                if (_currentCircleIndex < _currentCircle.Count) {
                    _currentRoundData = _currentCircle[_currentCircleIndex];
                    // Spawn new enemy for this round
                    SpawnEnemyFromRound(_currentRoundData, _cts.Token).Forget();
                }
                else {
                    // End of circle - could start a new circle or end battle
                    _logger.Info("Circle completed", "Battle");
                }
            }
        }

        private void CreatePlayer() {
            _currentPlayer = new PlayerModel();
        }

        private void RestorePlayer(PlayerSaveData saveData) {
            var hand = saveData.Hand?.Select(ToCardModel).ToList() ?? new List<CardModel>();
            var deck = RestoreDeck(saveData.Deck);
            _currentPlayer = new PlayerModel(hand, deck);
        }

        private Deck RestoreDeck(DeckSaveData saveData) {
            if (saveData == null) return new Deck();

            var cards = saveData.Cards?.Select(ToCardModel).ToList() ?? new List<CardModel>();
            var drawPile = saveData.DrawPile?.Select(ToCardModel).ToList() ?? new List<CardModel>();
            var discardPile = saveData.DiscardPile?.Select(ToCardModel).ToList() ?? new List<CardModel>();
            return new Deck(cards, drawPile, discardPile);
        }

        private async UniTask CreateDesk() {
            // Initialize player's starting hand
            // This would typically involve drawing initial cards from the deck
            _logger.Info("Creating player desk", "Battle");

            // Initialize with a basic hand of cards
            // In a real implementation, this would draw cards from the player's deck
        }

        public async UniTask SpawnRandomEnemy(int health, CancellationToken ct = default) {
            if (_currentEnemy != null) {
                await HandleEnemyDeath(ct);
            }

            var enemyList = _gameData.Enemies.Values.ToList();
            if (enemyList.Count == 0) {
                _logger.Error("No enemies available in game data", "Battle");
                return;
            }

            var enemyData = enemyList[Random.Range(0, enemyList.Count)];
            await SpawnEnemy(enemyData, health, ct);
        }

        public async UniTask SpawnEnemy(string enemyId, int health, CancellationToken ct = default) {
            if (!_gameData.Enemies.TryGetValue(enemyId, out var enemyData)) {
                _logger.Error($"Enemy with id '{enemyId}' not found", "Battle");
                return;
            }

            if (_currentEnemy != null) {
                await HandleEnemyDeath(ct);
            }

            await SpawnEnemy(enemyData, health, ct);
        }

        public async UniTask SpawnEnemyFromRound(FightRoundData roundData, CancellationToken ct = default) {
            if (!_gameData.Enemies.TryGetValue(roundData.EnemyId, out var enemyData)) {
                _logger.Error($"Enemy with id '{roundData.EnemyId}' not found", "Battle");
                return;
            }

            if (_currentEnemy != null) {
                await HandleEnemyDeath(ct);
            }

            _logger.Info($"Spawning enemy for Circle {roundData.Circle}, {roundData.EnemyType}, HP: {roundData.TargetScore}", "Battle");
            await SpawnEnemy(enemyData, roundData.TargetScore, ct);
        }

        private async UniTask SpawnEnemy(EnemyData enemyData, int health, CancellationToken ct) {
            var characterData = _gameData.Characters[enemyData.id];
            _logger.Info($"Spawning enemy: {enemyData.id} ({characterData.prefabPath})", "Battle");
            var handle = Addressables.InstantiateAsync(characterData.prefabPath);
            var enemyGo = await handle.ToUniTask(cancellationToken: ct);

            var view = enemyGo.GetComponent<ICharacterView>();
            if (view == null) {
                _logger.Error($"ICharacterView component not found on prefab: {characterData.prefabPath}", "Battle");
                Addressables.ReleaseInstance(handle);
                return;
            }

            var model = new EnemyModel(enemyData, health);
            model.Died += OnEnemyDied;
            var presenter = new EnemyPresenter(model, view);

            _currentEnemy = new EnemyWrapper(model, presenter, view, handle);

            await UniTask.Delay(1000, cancellationToken: ct);
        }

        private void OnEnemyDied() {
            HandleEnemyDeath(_cts.Token).Forget();
        }

        private async UniTask HandleEnemyDeath(CancellationToken ct) {
            if (_currentEnemy == null) return;

            _logger.Info("Enemy died, playing death animation", "Battle");
            await _currentEnemy.View.PlayDeathAnimation(ct);

            _logger.Info("Death animation complete, publishing EnemyDiedMessage", "Battle");
            _enemyDiedPub.Publish(new EnemyDiedMessage { Enemy = _currentEnemy.Model });

            _currentEnemy.Model.Died -= OnEnemyDied;
            _currentEnemy.Presenter.Dispose();

            if (_currentEnemy.AddressablesHandle.IsValid()) {
                Addressables.ReleaseInstance(_currentEnemy.AddressablesHandle);
            }

            _currentEnemy = null;

            // Move to next round after enemy death
            StartNextRound();
        }

        public void PlayerAttack(int? damageOverride = null) {
            if (_currentPlayer == null || _currentEnemy == null) {
                _logger.Warning("Cannot attack: no player or enemy", "Battle");
                return;
            }

            var damage = damageOverride ?? _currentPlayer.PlayHand();
            _currentEnemy.Model.TakeDamage(damage);
        }

        public void Dispose() {
            _cts?.Cancel();
            _cts?.Dispose();

            if (_currentEnemy != null) {
                _currentEnemy.Presenter.Dispose();

                if (_currentEnemy.AddressablesHandle.IsValid()) {
                    Addressables.ReleaseInstance(_currentEnemy.AddressablesHandle);
                }
            }

            // Clear any remaining references
            _currentEnemy = null;
            _currentPlayer = null;
        }

        private PlayerSaveData CreatePlayerSnapshot() {
            if (_currentPlayer == null) return null;

            return new PlayerSaveData {
                Hand = _currentPlayer.Hand?.Select(ToCardSaveData).ToList(),
                Deck = new DeckSaveData {
                    Cards = _currentPlayer.Deck.Cards.Select(ToCardSaveData).ToList(),
                    DrawPile = _currentPlayer.Deck.DrawPile.Select(ToCardSaveData).ToList(),
                    DiscardPile = _currentPlayer.Deck.DiscardPile.Select(ToCardSaveData).ToList()
                }
            };
        }

        private static CardSaveData ToCardSaveData(CardModel card) {
            return new CardSaveData {
                Id = card.Id,
                Name = card.Name,
                Damage = card.Damage,
                Cost = card.Cost,
                Description = card.Description
            };
        }

        private static CardModel ToCardModel(CardSaveData save) {
            return new CardModel(save.Id, save.Name, save.Damage, save.Cost, save.Description);
        }

        private static FightRoundSaveData ToRoundSaveData(FightRoundData round) {
            return new FightRoundSaveData {
                Circle = round.Circle,
                CircleStep = round.CircleStep,
                EnemyType = round.EnemyType,
                TargetScore = round.TargetScore,
                EnemyId = round.EnemyId
            };
        }

        private static FightRoundData ToFightRoundData(FightRoundSaveData save) {
            return new FightRoundData {
                Circle = save.Circle,
                CircleStep = save.CircleStep,
                EnemyType = save.EnemyType,
                TargetScore = save.TargetScore,
                EnemyId = save.EnemyId
            };
        }
    }
}
