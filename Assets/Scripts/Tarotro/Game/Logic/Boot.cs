using System.Threading;
using Cysharp.Threading.Tasks;
using Tarotro.Game.Core;
using Tarotro.Game.Utils;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Tarotro.Game.Logic {
    public class Boot : IAsyncStartable {
        private readonly SceneLoader _sceneLoader;
        private readonly Battle _battle;
        private readonly ConfigLoader _configLoader;
        private readonly IGameLogger _logger;

        [Inject]
        public Boot(SceneLoader sceneLoader, Battle battle, ConfigLoader configLoader, IGameLogger logger) {
            _sceneLoader = sceneLoader;
            _battle = battle;
            _configLoader = configLoader;
            _logger = logger;
        }

        public async UniTask StartAsync(CancellationToken ct = default) {
            _logger.Info("Boot started", "Boot");
            await _configLoader.Load();
            _logger.Info("Configs loaded", "Boot");
            await _sceneLoader.LoadSceneAsync("Scenes/debug.unity", LoadSceneMode.Additive, ct);
            _logger.Info("Debug scene loaded, starting load level scene", "Boot");
            var levelScene = await _sceneLoader.LoadSceneAsync("Scenes/level_0.unity", LoadSceneMode.Additive, ct);
            SceneManager.SetActiveScene(levelScene.Scene);
            _logger.Info("Level scene loaded, starting battle", "Boot");
            await _battle.StartBattle(ct);
        }
    }
}
