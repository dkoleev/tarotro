using System;
using Game.Messages;
using MessagePipe;

namespace Game.Logic {
    public class ScoreManager : IDisposable {
        private readonly IDisposable _subscription;

        public ScoreManager(ISubscriber<EnemyDiedMessage> enemyDiedSub)
        {
            _subscription = enemyDiedSub.Subscribe(msg => AddScore(100));
        }

        private void AddScore(int score) {
            
        }

        public void Dispose() => _subscription.Dispose();
    }
}