using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Utils;
using TMPro;
using UnityEngine;

namespace Game.View {
    public class EnemyView : MonoBehaviour {
        [SerializeField] 
        private Dictionary<AnimationType, string> animationStatsOverride;
        
        private enum AnimationType {
            Idle,
            Attack,
            Death
        }
        
        private readonly Dictionary<AnimationType, int> _animationStates = new() {
            { AnimationType.Idle, Animator.StringToHash("Base Layer.Idle") },
            { AnimationType.Death, Animator.StringToHash("Base Layer.Death") },
            { AnimationType.Attack, Animator.StringToHash("Base Layer.Attack") },
        };
        
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Animator animator;
        
        public void SetHealth(int amount) {
            healthText.text = amount.ToString();
        }

        public void UpdateHealth(int amount) {
            healthText.text = amount.ToString();
        }
        
        public async UniTask PlayDeathAnimation(CancellationToken ct = default) {
            if (animationStatsOverride.TryGetValue(AnimationType.Death, out var value)) {
                await animator.PlayAnimationAsync(Animator.StringToHash(value), ct);
            }
            else {
                await animator.PlayAnimationAsync(_animationStates[AnimationType.Death], ct);
            }
        }
        
        public async UniTask PlayIdleAnimation(CancellationToken ct = default) {
            if (animationStatsOverride.TryGetValue(AnimationType.Idle, out var value)) {
                await animator.PlayAnimationAsync(Animator.StringToHash(value), ct);
            }
            else {
                await animator.PlayAnimationAsync(_animationStates[AnimationType.Idle], ct);
            }
        }
        
        public async UniTask PlayAttackAnimation(CancellationToken ct = default) {
            if (animationStatsOverride.TryGetValue(AnimationType.Attack, out var value)) {
                await animator.PlayAnimationAsync(Animator.StringToHash(value), ct);
            }
            else {
                await animator.PlayAnimationAsync(_animationStates[AnimationType.Attack], ct);
            }
        }
    }
}
