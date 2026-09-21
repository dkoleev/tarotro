using System.Collections.Generic;
using Tarotro.Game.Logic.Motion;
using Tarotro.Game.Logic.Sequencing;
using VContainer.Unity;

namespace Tarotro.Game.Logic
{
    public sealed class GameLoopRunner : ITickable, IPostLateTickable
    {
        private readonly EventQueue _queue;
        private readonly MotionSystem _motion;
        private readonly CardHand _cardHand;
        private readonly List<MoveableView> _views = new List<MoveableView>(256);

        public GameLoopRunner(EventQueue queue, MotionSystem motion, CardHand cardHand)
        {
            _queue = queue;
            _motion = motion;
            _cardHand = cardHand;
        }

        public void RegisterView(MoveableView view) => _views.Add(view);
        public void UnregisterView(MoveableView view) => _views.Remove(view);

        void ITickable.Tick()
        {
            float dt = UnityEngine.Time.unscaledDeltaTime;
            _queue.Tick(dt);
            _motion.AdvanceTime(dt);
            _cardHand.Tick();
            _motion.Tick(dt);
        }

        void IPostLateTickable.PostLateTick()
        {
            for (int i = 0; i < _views.Count; i++)
            {
                _views[i].Apply();
            }
        }
    }
}
