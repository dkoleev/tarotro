using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Utils {
    public static class AnimatorUtils {
        public static async UniTask PlayAnimationAsync(this Animator animator, int animHash, CancellationToken ct = default) {
            animator.Play(animHash, 0, 0f);

            await UniTask.Yield(ct);

            await UniTask.WaitUntil(() =>
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);

                return state.fullPathHash == animHash
                       && state.normalizedTime >= 1f
                       && !animator.IsInTransition(0);
            }, cancellationToken: ct);
        }
    }
}