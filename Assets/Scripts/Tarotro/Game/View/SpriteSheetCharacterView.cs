using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Tarotro.Game.View {
    public class SpriteSheetCharacterView : MonoBehaviour, ICharacterView {
        [SerializeField] private SpriteSheetAnimator animator;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private string idleAnimation = "Idle";
        [SerializeField] private string deathAnimation = "Death";
        [SerializeField] private string attackAnimation = "Attack";

        public void SetHealth(int amount) {
            healthText.text = amount.ToString();
        }

        public void UpdateHealth(int amount) {
            healthText.text = amount.ToString();
        }

        public async UniTask PlayDeathAnimation(CancellationToken ct = default) {
            await animator.PlayAsync(deathAnimation, ct);
        }

        public async UniTask PlayIdleAnimation(CancellationToken ct = default) {
            await animator.PlayAsync(idleAnimation, ct);
        }

        public async UniTask PlayAttackAnimation(CancellationToken ct = default) {
            await animator.PlayAsync(attackAnimation, ct);
        }

        public void SetInterfaceActive(bool active) {
            healthText.gameObject.SetActive(active);
        }
    }
}
