using System.Threading;
using Cysharp.Threading.Tasks;

namespace Tarotro.Game.View {
    public interface IEnemyView {
        void SetHealth(int amount);
        void UpdateHealth(int amount);
        UniTask PlayDeathAnimation(CancellationToken ct = default);
        UniTask PlayIdleAnimation(CancellationToken ct = default);
        UniTask PlayAttackAnimation(CancellationToken ct = default);
    }
}
