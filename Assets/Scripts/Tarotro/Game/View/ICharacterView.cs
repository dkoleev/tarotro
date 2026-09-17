using System.Threading;
using Cysharp.Threading.Tasks;

namespace Tarotro.Game.View {
    public interface ICharacterView {
        void SetHealth(int amount);
        void UpdateHealth(int amount);
        UniTask PlayDeathAnimation(CancellationToken ct = default);
        UniTask PlayIdleAnimation(CancellationToken ct = default);
        UniTask PlayAttackAnimation(CancellationToken ct = default);
        public void SetInterfaceActive(bool active);
    }
}
