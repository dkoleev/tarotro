using System.Threading;
using Cysharp.Threading.Tasks;
using Tarotro.Game.Core;
using Tarotro.Game.Data.Save;
using Tarotro.Game.Logic.Rng;
using Tarotro.Game.Utils;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Tarotro.Game.Logic {
    public class Boot : IAsyncStartable {
        private readonly SceneLoader _sceneLoader;
        private readonly Battle _battle;
        private readonly ConfigLoader _configLoader;
        private readonly SaveManager _saveManager;
        private readonly ScoreManager _scoreManager;
        private readonly GameRng _rng;
        private readonly IGameLogger _logger;

        [Inject]
        public Boot(SceneLoader sceneLoader, Battle battle, ConfigLoader configLoader,
            SaveManager saveManager, ScoreManager scoreManager, GameRng rng, IGameLogger logger) {
            _sceneLoader = sceneLoader;
            _battle = battle;
            _configLoader = configLoader;
            _saveManager = saveManager;
            _scoreManager = scoreManager;
            _rng = rng;
            _logger = logger;
        }

        public async UniTask StartAsync(CancellationToken ct = default) {
            _logger.Info("Boot started", "Boot");
            await _configLoader.Load();
            _logger.Info("Configs loaded", "Boot");
#if UNITY_EDITOR || DEBUG
            await _sceneLoader.LoadSceneAsync("Scenes/debug.unity", LoadSceneMode.Additive, ct);
            _logger.Info("Debug scene loaded, starting load level scene", "Boot");
#endif
            var levelScene = await _sceneLoader.LoadSceneAsync(SceneLoader.LocationHellPath, LoadSceneMode.Additive, ct);
            SceneManager.SetActiveScene(levelScene.Scene);
            _logger.Info("Level scene loaded", "Boot");

            if (_saveManager.HasSave()) {
                _logger.Info("Save file found, restoring game state", "Boot");
                var saveData = await _saveManager.LoadAsync(ct);
                if (saveData != null) {
                    if (saveData.Rng != null) {
                        _rng.RestoreFromSave(saveData.Rng);
                    } else {
                        _rng.SeedFromTime();
                    }
                    _scoreManager.SetScore(saveData.Score);
                    await _battle.RestoreFromSave(saveData.Battle, ct);
                    return;
                }
            }

            _rng.SeedFromTime();
            _logger.Info("Starting new battle", "Boot");
            await _battle.StartBattle(ct);
        }
    }
}
