using System;
using Game.Data;

namespace Game.Logic {
    public class EnemyModel {
        public int CurrentHealth { get; private set; }

        public event Action<int> HealthChanged;
        public event Action<EnemyModel> Died;

        public EnemyModel(EnemyData data) {
            CurrentHealth = data.MaxHealth;
        }

        public void TakeDamage(int amount) {
            CurrentHealth -= amount;
            HealthChanged?.Invoke(CurrentHealth);
            if (CurrentHealth <= 0) {
                Died?.Invoke(this);
            }
        }
    }
}
