using System.Collections.Generic;
using Tarotro.Motion;
using Tarotro.Sequencing;
using UnityEngine;

namespace Tarotro.Runtime
{
    /// <summary>
    /// The single MonoBehaviour that drives both systems. Everything else in Tarotro
    /// should be plain C# registered in VContainer.
    ///
    /// Order is not optional:
    ///   Update      — queue first (it writes targets), then motion (it reads them)
    ///   LateUpdate  — views push VT into Unity transforms
    ///
    /// Register with VContainer:
    ///   builder.RegisterInstance(motionTuning);
    ///   builder.Register&lt;EventQueue&gt;(Lifetime.Singleton);
    ///   builder.Register&lt;MotionSystem&gt;(Lifetime.Singleton);
    ///   builder.RegisterComponentInHierarchy&lt;GameLoopRunner&gt;();
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameLoopRunner : MonoBehaviour
    {
        [SerializeField] private MotionTuning _tuning;
        [SerializeField] private float _roomWidth = 20f;

        private readonly List<MoveableView> _views = new List<MoveableView>(256);

        public EventQueue Queue { get; private set; }
        public MotionSystem Motion { get; private set; }

        private void Awake()
        {
            Queue = new EventQueue();
            Motion = new MotionSystem(_tuning) { RoomWidth = _roomWidth };
        }

        public void RegisterView(MoveableView view) => _views.Add(view);
        public void UnregisterView(MoveableView view) => _views.Remove(view);

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // Unscaled on purpose. Pausing is a flag on the queue, not a timeScale
            // change — that way menus, hover and juice keep animating while the game
            // sits still, which is exactly how Balatro feels when you open the shop.
            Queue.Tick(dt);
            Motion.Tick(dt);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _views.Count; i++)
                _views[i].Apply();
        }
    }
}
