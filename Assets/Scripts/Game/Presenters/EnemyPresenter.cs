using System;
using Game.Logic;
using Game.View;

namespace Game.Presenters {
    public class EnemyPresenter : IDisposable {
        private readonly EnemyModel _model;
        private readonly EnemyView _view;

        public EnemyPresenter(EnemyModel model, EnemyView view) {
            _model = model;
            _view = view;
            _model.HealthChanged += OnHealthChanged;
            _model.Died += OnDied;
            
            _view.SetHealth(_model.CurrentHealth);
        }

        private void OnHealthChanged(int health) => _view.UpdateHealth(health);
        private void OnDied(EnemyModel enemyModel) => _view.PlayDeathAnimation();

        public void Dispose() {
            _model.HealthChanged -= OnHealthChanged;
            _model.Died -= OnDied;
        }
    }
}
