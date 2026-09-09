using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game {
    public class Boot : IAsyncStartable {
        private readonly SceneLoader _sceneLoader;

        public Boot(SceneLoader sceneLoader) {
            _sceneLoader = sceneLoader;
        }
        
        public UniTask StartAsync(CancellationToken cancellation = new()) {
            // try {
            //     Steamworks.SteamClient.Init(252490);
            // }
            // catch (Exception e) {
            //     Debug.LogException(e);
            // }

            _sceneLoader.LoadSceneAsync("Scenes/level_0.unity", LoadSceneMode.Single).Forget();

            return UniTask.CompletedTask;
        }
    }
}
