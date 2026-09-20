using System.Collections.Generic;
using Tarotro.Motion;
using Tarotro.Sequencing;
using UnityEngine;
using VContainer;

namespace Tarotro.Runtime
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameLoopRunner : MonoBehaviour
    {
        [SerializeField] private float _roomWidth = 20f;

        private readonly List<MoveableView> _views = new List<MoveableView>(256);

        [Inject] public EventQueue Queue { get; private set; }
        [Inject] public MotionSystem Motion { get; private set; }

        private void Start()
        {
            Motion.RoomWidth = _roomWidth;
        }

        public void RegisterView(MoveableView view) => _views.Add(view);
        public void UnregisterView(MoveableView view) => _views.Remove(view);

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
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
