using System;
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

namespace Game.Logic
{
    public class Battle : IDisposable {
        private const string DefaultEnemy = "Bundles/Enemies/enemy_demon_eye.prefab";

        private readonly IPublisher<EnemyDiedMessage> _enemyDiedPub;
        private EnemyPresenter _currentEnemyPresenter;
        private AsyncOperationHandle<GameObject> _currentEnemyHandle;

        public Battle(IPublisher<EnemyDiedMessage> enemyDiedPub) {
            _enemyDiedPub = enemyDiedPub;
        }

        public async UniTask StartBattle(CancellationToken ct = default) {
            await CreateDesk();
            await SpawnEnemy(DefaultEnemy, ct);
        }

        private async UniTask CreateDesk() {
        }

        private async UniTask SpawnEnemy(string enemyPath, CancellationToken ct) {
            var handle = Addressables.InstantiateAsync(enemyPath);
            var enemyGo = await handle.ToUniTask(cancellationToken: ct);

            var view = enemyGo.GetComponent<EnemyView>();
            if (view == null) {
                Debug.LogError($"EnemyView component not found on prefab: {enemyPath}");
                Addressables.ReleaseInstance(handle);
                return;
            }

            _currentEnemyHandle = handle;
            var model = new EnemyModel(new EnemyData("Eye", 100));
            model.Died += OnEnemyDied;
            _currentEnemyPresenter = new EnemyPresenter(model, view);

            // model.TakeDamage(100);
        }

        private void OnEnemyDied(EnemyModel enemyModel) {
            _enemyDiedPub.Publish(new EnemyDiedMessage { Enemy = enemyModel });

            enemyModel.Died -= OnEnemyDied;
            _currentEnemyPresenter.Dispose();
            _currentEnemyPresenter = null;

            if (_currentEnemyHandle.IsValid())
                Addressables.ReleaseInstance(_currentEnemyHandle);
        }

        public void Dispose() {
            _currentEnemyPresenter?.Dispose();

            if (_currentEnemyHandle.IsValid())
                Addressables.ReleaseInstance(_currentEnemyHandle);
        }
    }
}
