using UnityEngine;

namespace Tarotro.Motion
{
    /// <summary>
    /// The only place a Unity Transform is written. Reads VT, writes the scene.
    /// Nothing else in the game should touch transform.position.
    ///
    /// Coordinate note: the source uses a top-left origin with Y pointing down and
    /// treats X/Y as the object's corner. Unity is Y-up and pivots at the sprite's
    /// centre, so this view converts once, here. Keep the conversion in this file —
    /// if it leaks into gameplay code you will be debugging sign errors for a week.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MoveableView : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private SpriteRenderer _shadow;
        [SerializeField] private float _shadowDistance = 0.08f;

        /// <summary>Set true if your art is authored with Y pointing down.</summary>
        [SerializeField] private bool _flipY = true;

        [Tooltip("On: localScale is stretched to VT.W/VT.H — use for a 1x1 unit quad or " +
                 "anything that pinches (panels, collapsing rows). " +
                 "Off: localScale is VT.Scale only — use for a SpriteRenderer already " +
                 "sized correctly by its pixelsPerUnit. Cards usually want it off.")]
        [SerializeField] private bool _stretchToSize;

        public Moveable Moveable { get; private set; }

        private void Awake()
        {
            if (_target == null) _target = transform;
        }

        public void Bind(Moveable moveable)
        {
            Moveable = moveable;
            Apply();
        }

        /// <summary>
        /// Call from LateUpdate, after MotionSystem.Tick has run for this frame.
        /// Drive it from one place (a view loop) rather than giving every card its
        /// own LateUpdate.
        /// </summary>
        public void Apply()
        {
            var m = Moveable;
            if (m == null) return;

            var vt = m.VT;

            // Corner -> centre, so Scale inflates around the middle of the card.
            float cx = vt.X + vt.W * 0.5f;
            float cy = vt.Y + vt.H * 0.5f;
            if (_flipY) cy = -cy;

            _target.localPosition = new Vector3(cx, cy, _target.localPosition.z);
            _target.localRotation = Quaternion.Euler(0f, 0f, (_flipY ? -1f : 1f) * vt.R * Mathf.Rad2Deg);
            _target.localScale = _stretchToSize
                ? new Vector3(vt.W * vt.Scale, vt.H * vt.Scale, 1f)
                : new Vector3(vt.Scale, vt.Scale, 1f);

            if (_shadow != null)
            {
                var p = _shadow.transform.localPosition;
                _shadow.transform.localPosition =
                    new Vector3(m.ShadowParallax * _shadowDistance, p.y, p.z);
            }
        }
    }
}
