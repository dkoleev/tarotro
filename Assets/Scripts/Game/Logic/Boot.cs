using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Logic {
    public class Boot : IAsyncStartable {
        private readonly SceneLoader _sceneLoader;
        private readonly Battle _battle;

        public Boot(SceneLoader sceneLoader, Battle battle) {
            _sceneLoader = sceneLoader;
            _battle = battle;
        }

        public async UniTask StartAsync(CancellationToken ct = default) {
            await _sceneLoader.LoadSceneAsync("Scenes/level_0.unity", LoadSceneMode.Single, ct);
            await _battle.StartBattle(ct);
        }
    }
}
