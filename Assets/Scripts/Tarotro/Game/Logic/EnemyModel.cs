using System;
using Tarotro.Game.Data;
using UnityEngine;

namespace Tarotro.Game.Logic {
    public class EnemyModel {
        public int CurrentHealth { get; private set; }

        public event Action<int> HealthChanged;
        public event Action<int> OnAttack;
        public event Action Died;

        private readonly int _damage;

        public EnemyModel(EnemyData data, int? healthOverride = null) {
            CurrentHealth = healthOverride ?? data.maxHealth;
            _damage = data.damage;
        }

        public void TakeDamage(int amount) {
            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);
            
            HealthChanged?.Invoke(CurrentHealth);
            if (CurrentHealth <= 0) {
                Died?.Invoke();
            }
        }

        public void Attack() {
            OnAttack?.Invoke(_damage);
        }
    }
}
