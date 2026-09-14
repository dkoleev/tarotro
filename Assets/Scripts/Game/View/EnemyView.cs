using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Utils;
using TMPro;
using UnityEngine;

namespace Game.View {
    public class EnemyView : MonoBehaviour {
        [SerializeField] private Dictionary<AnimationType, string> animationStatsOverride;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Animator animator;
        
        private enum AnimationType {
            Idle,
            Attack,
            Death,
            Emote
        }
        
        private readonly Dictionary<AnimationType, int> _animationStates = new() {
            { AnimationType.Idle, Animator.StringToHash("Base Layer.Idle") },
            { AnimationType.Death, Animator.StringToHash("Base Layer.Death") },
            { AnimationType.Attack, Animator.StringToHash("Base Layer.Attack") },
            { AnimationType.Emote, Animator.StringToHash("Base Layer.Emote") },
        };

        private void Awake() {
            foreach (var animationState in _animationStates) {
                if (animationStatsOverride.TryGetValue(animationState.Key, out var value)) {
                    _animationStates[animationState.Key] = Animator.StringToHash(value);
                }
            }
        }

        public void SetHealth(int amount) {
            healthText.text = amount.ToString();
        }

        public void UpdateHealth(int amount) {
            healthText.text = amount.ToString();
        }
        
        public async UniTask PlayDeathAnimation(CancellationToken ct = default) {
            await animator.PlayAnimationAsync(_animationStates[AnimationType.Death], ct);
        }
        
        public async UniTask PlayIdleAnimation(CancellationToken ct = default) {
            await animator.PlayAnimationAsync(_animationStates[AnimationType.Idle], ct);
        }
        
        public async UniTask PlayAttackAnimation(CancellationToken ct = default) {
            await animator.PlayAnimationAsync(_animationStates[AnimationType.Attack], ct);
        }
    }
}
