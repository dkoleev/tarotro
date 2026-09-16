using System;
using MessagePipe;
using Tarotro.Game.Messages;
using Tarotro.Game.Utils;
using VContainer;

namespace Tarotro.Game.Logic {
    public class ScoreManager : IDisposable {
        private readonly IDisposable _subscription;
        private readonly IGameLogger _logger;

        private int _currentScore;

        [Inject]
        public ScoreManager(ISubscriber<EnemyDiedMessage> enemyDiedSub, IGameLogger logger) {
            _subscription = enemyDiedSub.Subscribe(msg => AddScore(100));
            _logger = logger;
        }

        private void AddScore(int score) {
            _currentScore += score;
            _logger.Info($"Score: {_currentScore}", "Score");
        }

        public void Dispose() => _subscription.Dispose();
    }
}