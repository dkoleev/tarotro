using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer;

namespace Tarotro.Game.Logic {
    public class SceneLoader {
        private readonly Dictionary<string, SceneInstance> _loadedScenes = new();

        [Inject]
        public SceneLoader() { }

        public async UniTask<SceneInstance> LoadSceneAsync(
            string sceneKey,
            LoadSceneMode mode = LoadSceneMode.Single,
            CancellationToken ct = default) {
            if (mode == LoadSceneMode.Single)
                await UnloadAllScenesAsync(ct);

            var handle = Addressables.LoadSceneAsync(sceneKey, mode);
            var sceneInstance = await handle.ToUniTask(cancellationToken: ct);
            _loadedScenes[sceneKey] = sceneInstance;

            return sceneInstance;
        }

        public async UniTask UnloadSceneAsync(string sceneKey, CancellationToken ct = default) {
            if (!_loadedScenes.TryGetValue(sceneKey, out var sceneInstance))
                return;

            await Addressables.UnloadSceneAsync(sceneInstance).ToUniTask(cancellationToken: ct);
            _loadedScenes.Remove(sceneKey);
        }

        public async UniTask UnloadAllScenesAsync(CancellationToken ct = default) {
            var tasks = new List<UniTask>();
            foreach (var scene in _loadedScenes.Values)
                tasks.Add(Addressables.UnloadSceneAsync(scene).ToUniTask(cancellationToken: ct));

            await UniTask.WhenAll(tasks);
            _loadedScenes.Clear();
        }

        public bool IsSceneLoaded(string sceneKey) => _loadedScenes.ContainsKey(sceneKey);
    }
}
