using UnityEngine;

namespace Tarotro.Motion
{
    [DisallowMultipleComponent]
    public sealed class MoveableView : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private SpriteRenderer _shadow;
        [SerializeField] private float _shadowDistance = 0.08f;
        [SerializeField] private bool _flipY = true;

        [Tooltip("On: localScale is stretched to VT.W/VT.H. Off: localScale is VT.Scale only.")]
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

        public void Apply()
        {
            var m = Moveable;
            if (m == null) {
                return;
            }

            var vt = m.VT;

            var cx = vt.X + vt.W * 0.5f;
            var cy = vt.Y + vt.H * 0.5f;
            
            if (_flipY) {
                cy = -cy;
            }

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
