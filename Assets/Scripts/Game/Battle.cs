using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class Battle : MonoBehaviour {
        [SerializeField] private string enemyPrefabPath;
        
        private void Start() {
            SpawnEnemy().Forget();
        }

        private async UniTask SpawnEnemy() {
            var handle = Addressables.InstantiateAsync(enemyPrefabPath, null, true, true);
            var enemyGo = await handle.Task;
            var enemy = enemyGo.GetComponent<Enemy>();
            Debug.Log(enemy.Health);
        }
    }
}