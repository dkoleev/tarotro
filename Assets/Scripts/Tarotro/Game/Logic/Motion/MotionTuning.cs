using UnityEngine;

namespace Tarotro.Game.Logic.Motion
{
    [CreateAssetMenu(menuName = "Tarotro/Motion Tuning", fileName = "MotionTuning")]
    public sealed class MotionTuning : ScriptableObject
    {
        [Header("Smoothing rates (higher = snappier)")]
        [Tooltip("Position EMA velocity decay rate.")]
        public float PositionRate = 50f;

        [Tooltip("Scale exponential smoothing rate.")]
        public float ScaleRate = 60f;

        [Tooltip("Rotation exponential smoothing rate.")]
        public float RotationRate = 190f;

        [Header("Position")]
        [Tooltip("Velocity gain applied to position error per second.")]
        public float Stiffness = 35f;

        [Tooltip("Maximum velocity magnitude per frame.")]
        public float MaxSpeed = 70f;

        [Tooltip("Movement dt is clamped to this to prevent huge simulation steps.")]
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
