using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Utils;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Logic {
    public class Boot : IAsyncStartable {
        private readonly SceneLoader _sceneLoader;
        private readonly Battle _battle;
        private readonly ConfigLoader _configLoader;
        private readonly IGameLogger _logger;

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
            await _sceneLoader.LoadSceneAsync("Scenes/level_0.unity", LoadSceneMode.Additive, ct);
            _logger.Info("Level loaded, starting battle", "Boot");
            await _battle.StartBattle(ct);
        }
    }
}
