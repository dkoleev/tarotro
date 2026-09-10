using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Logic {
    public class Boot : IAsyncStartable {
        private readonly SceneLoader _sceneLoader;
        private readonly Battle _battle;
        private readonly ConfigLoader _configLoader;

        public Boot(SceneLoader sceneLoader, Battle battle, ConfigLoader configLoader) {
            _sceneLoader = sceneLoader;
            _battle = battle;
            _configLoader = configLoader;
        }

        public async UniTask StartAsync(CancellationToken ct = default) {
            await _configLoader.Load();
            await _sceneLoader.LoadSceneAsync("Scenes/level_0.unity", LoadSceneMode.Single, ct);
            await _battle.StartBattle(ct);
        }
    }
}
