using System;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Messages;
using Game.Presenters;
using Game.View;
using MessagePipe;
using UnityEngine.AddressableAssets;

namespace Game.Logic
{
    public class Battle : IDisposable {
        private const string DefaultEnemy = "Bundles/Enemies/enemy_demon_eye.prefab";
        
        private readonly IPublisher<EnemyDiedMessage> _enemyDiedPub;
        private EnemyPresenter _currentEnemyPresenter;

        public Battle(IPublisher<EnemyDiedMessage> enemyDiedPub) {
            _enemyDiedPub = enemyDiedPub;
        }
        
        public async UniTask StartBattle() {
            await SpawnEnemy(DefaultEnemy);
        }

        private async UniTask SpawnEnemy(string enemyPath) {
            var enemyGo = await Addressables.InstantiateAsync(enemyPath).Task;
            var view = enemyGo.GetComponent<EnemyView>();
            var model = new EnemyModel(new EnemyData("Eye", 100));
            model.Died += OnEnemyDied;
            _currentEnemyPresenter = new EnemyPresenter(model, view);

            model.TakeDamage(100);
        }
        
        private void OnEnemyDied(EnemyModel enemyModel) {
            _enemyDiedPub.Publish(new EnemyDiedMessage { Enemy = enemyModel });
            
            enemyModel.Died -= OnEnemyDied;
            _currentEnemyPresenter.Dispose();
        }

        public void Dispose() {
            _currentEnemyPresenter?.Dispose();
        }
    }
}
