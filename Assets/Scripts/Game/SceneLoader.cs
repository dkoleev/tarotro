using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Game {
    public class SceneLoader {
        private SceneInstance _currentScene;

        public async UniTask<SceneInstance> LoadSceneAsync(string sceneKey, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var handle = Addressables.LoadSceneAsync(sceneKey, mode);
            _currentScene = await handle.ToUniTask();

            return _currentScene;
        }

        public async UniTask UnloadCurrentSceneAsync()
        {
            await Addressables.UnloadSceneAsync(_currentScene).ToUniTask();
        }
    }
}
