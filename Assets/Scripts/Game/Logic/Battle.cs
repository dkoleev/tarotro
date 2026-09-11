using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Messages;
using Game.Presenters;
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

            public EnemyWrapper(EnemyModel model, EnemyPresenter presenter, EnemyView view) {
                Model = model;
                Presenter = presenter;
                View = view;
            }
        }
        
        private readonly IPublisher<EnemyDiedMessage> _enemyDiedPub;
        private readonly GameData _gameData;
        private AsyncOperationHandle<GameObject> _currentEnemyHandle;
        private EnemyWrapper _currentEnemy;
        private CancellationTokenSource _cts;

        public Battle(IPublisher<EnemyDiedMessage> enemyDiedPub, GameData gameData) {
            _enemyDiedPub = enemyDiedPub;
            _gameData = gameData;
        }

        public async UniTask StartBattle(CancellationToken ct = default) {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            
            await CreateDesk();
            await SpawnRandomEnemy(ct);
        }

        private async UniTask CreateDesk() {
        }

        private async UniTask SpawnRandomEnemy(CancellationToken ct = default) {
            
            var enemyData = _gameData.Enemies.Values.ToList()[Random.Range(0, _gameData.Enemies.Count)];
            await SpawnEnemy(enemyData, ct);
        }

        private async UniTask SpawnEnemy(EnemyData enemyData, CancellationToken ct) {
            Debug.Log(enemyData.prefabPath + "; " + enemyData.id);
            var handle = Addressables.InstantiateAsync(enemyData.prefabPath);
            var enemyGo = await handle.ToUniTask(cancellationToken: ct);

            var view = enemyGo.GetComponent<EnemyView>();
            if (view == null) {
                Debug.LogError($"EnemyView component not found on prefab: {enemyData.prefabPath}");
                Addressables.ReleaseInstance(handle);
                return;
            }

            _currentEnemyHandle = handle;
            var model = new EnemyModel(enemyData);
            model.Died += OnEnemyDied;
            var presenter = new EnemyPresenter(model, view);

            _currentEnemy = new EnemyWrapper(model, presenter, view);

            await UniTask.Delay(1000);
            model.TakeDamage(100);
        }
        
        private void OnEnemyDied() {
            HandleEnemyDeath(_cts.Token).Forget();
        }

        private async UniTask HandleEnemyDeath(CancellationToken ct) {
            await _currentEnemy.View.PlayDeathAnimation(ct);

            _enemyDiedPub.Publish(new EnemyDiedMessage { Enemy = _currentEnemy.Model });

            _currentEnemy.Model.Died -= OnEnemyDied;
            _currentEnemy.Presenter.Dispose();

            if (_currentEnemyHandle.IsValid()) {
                Addressables.ReleaseInstance(_currentEnemyHandle);
            }
                
            _currentEnemy = null;
        }

        public void Dispose() {
            _cts.Cancel();
            _cts.Dispose();
            
            _currentEnemy?.Presenter.Dispose();

            if (_currentEnemyHandle.IsValid()) {
                Addressables.ReleaseInstance(_currentEnemyHandle);
            }
        }
    }
}
