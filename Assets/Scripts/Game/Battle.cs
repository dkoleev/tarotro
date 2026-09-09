using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class Battle {
        private const string DefaultEnemy = "Bundles/Enemies/enemy_demon_eye.prefab";
        
        public async UniTask StartBattle() {
            await SpawnEnemy(DefaultEnemy);
        }

        private async UniTask SpawnEnemy(string enemyPath) {
            var handle = Addressables.InstantiateAsync(enemyPath, null, true, true);
            var enemyGo = await handle.Task;
            var enemy = enemyGo.GetComponent<Enemy>();
            Debug.Log(enemy.Health);
        }
    }
}