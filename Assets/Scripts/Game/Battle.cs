using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game
{
    public class Battle
    {
        private const string DefaultEnemy = "Bundles/Enemies/enemy_demon_eye.prefab";
        private readonly List<AsyncOperationHandle<GameObject>> _spawnedHandles = new();

        public async UniTask StartBattle(CancellationToken ct = default)
        {
            await SpawnEnemy(DefaultEnemy, ct);
        }

        private async UniTask SpawnEnemy(string enemyPath, CancellationToken ct)
        {
            var handle = Addressables.InstantiateAsync(enemyPath, null, true, true);
            var enemyGo = await handle.ToUniTask(cancellationToken: ct);

            var enemy = enemyGo.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError($"Enemy component not found on prefab: {enemyPath}");
                Addressables.ReleaseInstance(handle);
                return;
            }

            _spawnedHandles.Add(handle);
            Debug.Log($"Spawned enemy with health: {enemy.Health}");
        }

        public void ReleaseAll()
        {
            foreach (var handle in _spawnedHandles)
                Addressables.ReleaseInstance(handle);
            _spawnedHandles.Clear();
        }
    }
}
