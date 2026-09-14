using System;
using Tarotro.Game.Logic;
using Tarotro.Game.View;

namespace Tarotro.Game.Presenters {
    public class EnemyPresenter : IDisposable {
        private readonly EnemyModel _model;
        private readonly IEnemyView _view;

        public EnemyPresenter(EnemyModel model, IEnemyView view) {
            _model = model;
            _view = view;
            _model.HealthChanged += OnHealthChanged;
            // _model.Died += OnDied;
            
            _view.SetHealth(_model.CurrentHealth);
        }

        private void OnHealthChanged(int health) => _view.UpdateHealth(health);
        // private void OnDied() => _view.PlayDeathAnimation();

        public void Dispose() {
            _model.HealthChanged -= OnHealthChanged;
            // _model.Died -= OnDied;
        }
    }
}
