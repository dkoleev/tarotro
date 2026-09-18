using System;
using Tarotro.Game.Data.Save;
using Tarotro.Game.Logic.Rng;
using Tarotro.Game.Utils;
using UnityEngine;
using VContainer;

namespace Tarotro.Game.Logic {
    public class AutoSaveHandler : IDisposable {
        private readonly SaveManager _saveManager;
        private readonly Battle _battle;
        private readonly ScoreManager _scoreManager;
        private readonly GameRng _rng;
        private readonly IGameLogger _logger;

        [Inject]
        public AutoSaveHandler(SaveManager saveManager, Battle battle, ScoreManager scoreManager, GameRng rng, IGameLogger logger) {
            _saveManager = saveManager;
            _battle = battle;
            _scoreManager = scoreManager;
            _rng = rng;
            _logger = logger;

            Application.quitting += OnQuitting;
        }

        private void OnQuitting() {
            _logger.Info("Application quitting, saving game", "AutoSave");

            var data = new GameSaveData {
                Score = _scoreManager.CurrentScore,
                Battle = _battle.CreateSaveSnapshot(),
                Rng = _rng.CreateSaveSnapshot()
            };

            _saveManager.Save(data);
        }

        public void Dispose() {
            Application.quitting -= OnQuitting;
        }
    }
}
