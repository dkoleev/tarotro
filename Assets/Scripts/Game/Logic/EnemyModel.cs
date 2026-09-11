using System;
using Game.Data;
using UnityEngine;

namespace Game.Logic {
    public class EnemyModel {
        public int CurrentHealth { get; private set; }

        public event Action<int> HealthChanged;
        public event Action Died;

        public EnemyModel(EnemyData data) {
            CurrentHealth = data.maxHealth;
        }

        public void TakeDamage(int amount) {
            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);
            
            HealthChanged?.Invoke(CurrentHealth);
            if (CurrentHealth <= 0) {
                Died?.Invoke();
            }
        }
    }
}
