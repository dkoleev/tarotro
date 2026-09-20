using UnityEngine;

namespace Tarotro.Motion
{
    [CreateAssetMenu(menuName = "Tarotro/Motion Tuning", fileName = "MotionTuning")]
    public sealed class MotionTuning : ScriptableObject
    {
        [Header("Smoothing rates (higher = snappier)")]
        [Tooltip("Position. Source: exp(-50 * dt)")]
        public float PositionRate = 50f;

        [Tooltip("Scale. Source: exp(-60 * dt)")]
        public float ScaleRate = 60f;

        [Tooltip("Rotation. Source: exp(-190 * dt). Much stiffer than position.")]
        public float RotationRate = 190f;

        [Header("Position integrator")]
        [Tooltip("Pull toward the target. Source: (T - VT) * 35 * dt")]
        public float Stiffness = 35f;

        [Tooltip("Units per second. Source: 70 * move_dt")]
        public float MaxSpeed = 70f;

        [Tooltip("Movement dt is clamped to this. Source: min(1/20, real_dt)")]
        public float MaxMoveDelta = 1f / 20f;

        [Tooltip("Below this distance AND speed, the visual transform snaps to the target.")]
        public float SnapDistance = 0.01f;

        public float SnapAngle = 0.001f;
        public float SnapScale = 0.001f;

        [Header("Width / height (linear, not smoothed)")]
        [Tooltip("Source: VT.w += 8 * dt * T.w")]
        public float SizeRate = 8f;

        [Header("Lean")]
        [Tooltip("How much horizontal speed tilts the object. Source: 0.015 * vel.x / dt")]
        public float RotationFromSpeed = 0.015f;

        [Header("Interaction zoom")]
        public float HoverScaleBonus = 0.05f;
        public float DragScaleBonus = 0.1f;

        [Header("Juice")]
        public float JuiceDuration = 0.4f;

        [Tooltip("Default amplitude when JuiceUp() is called with no argument.")]
        public float JuiceDefaultAmount = 0.4f;

        [Tooltip("Source: sin(50.8 * t)")]
        public float JuiceScaleFrequency = 50.8f;

        [Tooltip("Source: sin(40.8 * t)")]
        public float JuiceRotationFrequency = 40.8f;

        [Tooltip("Rotation amplitude as a fraction of the scale amplitude. Source: 0.6")]
        public float JuiceRotationRatio = 0.6f;

        [Tooltip("Initial squash applied the instant juice starts. Source: VT.scale = 1 - 0.6 * amount")]
        public float JuicePreSquash = 0.6f;

        [Tooltip("Rotation juice doubled when reaching the rotation target. Source: juice.r * 2")]
        public float JuiceRotationGain = 2f;

        [Header("Shadow parallax")]
        [Tooltip("Source: (centre_x - room_centre_x) / (room_w/2) * 1.5")]
        public float ParallaxStrength = 1.5f;

        [Header("Accessibility")]
        [Tooltip("Suppresses juice and idle bob.")]
        public bool ReducedMotion;

        private static MotionTuning _default;

        public static MotionTuning Default =>
            _default != null ? _default : (_default = CreateInstance<MotionTuning>());
    }
}
