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
        private const string DefaultEnemy = "Bundles/Enemies/enemy_demon_eye.prefab";

        private readonly IPublisher<EnemyDiedMessage> _enemyDiedPub;
        private readonly GameData _gameData;
        private EnemyPresenter _currentEnemyPresenter;
        private AsyncOperationHandle<GameObject> _currentEnemyHandle;

        public Battle(IPublisher<EnemyDiedMessage> enemyDiedPub, GameData gameData) {
            _enemyDiedPub = enemyDiedPub;
            _gameData = gameData;
        }

        public async UniTask StartBattle(CancellationToken ct = default) {
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
