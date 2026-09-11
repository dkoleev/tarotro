using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Utils;
using TMPro;
using UnityEngine;

namespace Game.View {
    public class EnemyView : MonoBehaviour {
        private static class AnimState
        {
            public static readonly int Idle   = Animator.StringToHash("Base Layer.Idle");
            public static readonly int Death   = Animator.StringToHash("Base Layer.Death");
            public static readonly int Attack = Animator.StringToHash("Base Layer.Attack");
        }
        
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Animator animator;
        
        public void SetHealth(int amount) {
            healthText.text = amount.ToString();
        }

        public void UpdateHealth(int amount) {
            healthText.text = amount.ToString();
        }

        public async UniTask PlayDeathAnimation(CancellationToken ct = default) {
            await animator.PlayAnimationAsync(AnimState.Death, ct);
        }
    }
}
