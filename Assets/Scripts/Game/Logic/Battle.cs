using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Messages;
using Game.Presenters;
using Game.View;
using MessagePipe;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Logic
{
    public class Battle {
        private const string DefaultEnemy = "Bundles/Enemies/enemy_demon_eye.prefab";
        
        private readonly IPublisher<EnemyDiedMessage> _enemyDiedPub;

        public Battle(IPublisher<EnemyDiedMessage> enemyDiedPub) {
            _enemyDiedPub = enemyDiedPub;
        }
        
        public async UniTask StartBattle() {
            await SpawnEnemy(DefaultEnemy);
            
            // When an enemy dies:
            // _enemyDiedPub.Publish(new EnemyDiedMessage { Enemy = model });
        }

        private async UniTask SpawnEnemy(string enemyPath) {
            var enemyGo = await Addressables.InstantiateAsync(enemyPath).Task;
            var view = enemyGo.GetComponent<EnemyView>();
            var model = new EnemyModel(new EnemyData("Eye", 100));
            var presenter = new EnemyPresenter(model, view);
        }
    }
}