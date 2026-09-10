using System;
using Game.Messages;
using MessagePipe;
using UnityEngine;

namespace Game.Logic {
    public class ScoreManager : IDisposable {
        private readonly IDisposable _subscription;

        private int _currentScore;

        public ScoreManager(ISubscriber<EnemyDiedMessage> enemyDiedSub)
        {
            _subscription = enemyDiedSub.Subscribe(msg => AddScore(100));
        }

        private void AddScore(int score) {
            _currentScore += score;
            Debug.Log($"Score: {_currentScore}");
        }

        public void Dispose() => _subscription.Dispose();
    }
}