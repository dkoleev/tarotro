using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneLoader
    {
        private SceneInstance? _currentScene;

        public async UniTask<SceneInstance> LoadSceneAsync(
            string sceneKey,
            LoadSceneMode mode = LoadSceneMode.Single,
            CancellationToken ct = default)
        {
            if (_currentScene.HasValue)
                await Addressables.UnloadSceneAsync(_currentScene.Value).ToUniTask(cancellationToken: ct);

            var handle = Addressables.LoadSceneAsync(sceneKey, mode);
            _currentScene = await handle.ToUniTask(cancellationToken: ct);

            return _currentScene.Value;
        }

        public async UniTask UnloadCurrentSceneAsync(CancellationToken ct = default)
        {
            if (!_currentScene.HasValue)
                return;

            await Addressables.UnloadSceneAsync(_currentScene.Value).ToUniTask(cancellationToken: ct);
            _currentScene = null;
        }
    }
}
