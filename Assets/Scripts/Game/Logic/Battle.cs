using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Messages;
using Game.Presenters;
using Game.Utils;
using Game.View;
using MessagePipe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace Game.Logic
{
    public class Battle : IDisposable {
        private class EnemyWrapper {
            public EnemyModel Model { get; }
            public EnemyPresenter Presenter { get; }
            public EnemyView View { get; }
            public AsyncOperationHandle<GameObject> AddressablesHandle { get; }

            public EnemyWrapper(EnemyModel model, EnemyPresenter presenter, EnemyView view, AsyncOperationHandle<GameObject>  addressablesHandle) {
                Model = model;
                Presenter = presenter;
                View = view;
                AddressablesHandle = addressablesHandle;
            }
        }
        
        private readonly IPublisher<EnemyDiedMessage> _enemyDiedPub;
        private readonly GameData _gameData;
        private readonly IGameLogger _logger;
        private EnemyWrapper _currentEnemy;
        private CancellationTokenSource _cts;

        public Battle(IPublisher<EnemyDiedMessage> enemyDiedPub, GameData gameData, IGameLogger logger) {
            _enemyDiedPub = enemyDiedPub;
            _gameData = gameData;
            _logger = logger;
        }

        public async UniTask StartBattle(CancellationToken ct = default) {
            _logger.Info("Starting battle", "Battle");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _logger.Info("Play hand " + _gameData.Battle.playHandSize, "Battle");
            await CreateDesk();
            await SpawnRandomEnemy(ct);
        }

        private async UniTask CreateDesk() {
            
        }

        public async UniTask SpawnRandomEnemy(CancellationToken ct = default) {
            if (_currentEnemy != null) {
                await HandleEnemyDeath(ct);
            }
            
            var enemyData = _gameData.Enemies.Values.ToList()[Random.Range(0, _gameData.Enemies.Count)];
            await SpawnEnemy(enemyData, ct);
        }

        public async UniTask SpawnEnemy(string enemyId, CancellationToken ct = default) {
            if (!_gameData.Enemies.TryGetValue(enemyId, out var enemyData)) {
                _logger.Error($"Enemy with id '{enemyId}' not found", "Battle");
                return;
            }

            if (_currentEnemy != null) {
                await HandleEnemyDeath(ct);
            }

            await SpawnEnemy(enemyData, ct);
        }

        private async UniTask SpawnEnemy(EnemyData enemyData, CancellationToken ct) {
            _logger.Info($"Spawning enemy: {enemyData.id} ({enemyData.prefabPath})", "Battle");
            var handle = Addressables.InstantiateAsync(enemyData.prefabPath);
            var enemyGo = await handle.ToUniTask(cancellationToken: ct);

            var view = enemyGo.GetComponent<EnemyView>();
            if (view == null) {
                _logger.Error($"EnemyView component not found on prefab: {enemyData.prefabPath}", "Battle");
                Addressables.ReleaseInstance(handle);
                return;
            }

            var model = new EnemyModel(enemyData);
            model.Died += OnEnemyDied;
            var presenter = new EnemyPresenter(model, view);

            _currentEnemy = new EnemyWrapper(model, presenter, view, handle);

            await UniTask.Delay(1000, cancellationToken: ct);
            model.TakeDamage(100);
        }
        
        private void OnEnemyDied() {
            HandleEnemyDeath(_cts.Token).Forget();
        }

        private async UniTask HandleEnemyDeath(CancellationToken ct) {
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
        }

        public void Dispose() {
            _cts.Cancel();
            _cts.Dispose();

            if (_currentEnemy != null) {
                _currentEnemy.Presenter.Dispose();

                if (_currentEnemy.AddressablesHandle.IsValid()) {
                    Addressables.ReleaseInstance(_currentEnemy.AddressablesHandle);
                }
            }
        }
    }
}
