using UnityEngine;

namespace TapKnockout.Combat
{
    [CreateAssetMenu(fileName = "ImpactFeedbackConfig", menuName = "Tap Knockout/Combat/Impact Feedback Config")]
    public sealed class ImpactFeedbackConfig : ScriptableObject
    {
        [Header("Hit Pause")]
        [SerializeField, Range(0f, 0.12f)] private float dashHitPauseDuration = 0.05f;

        [Header("Hit Flash")]
        [SerializeField, Range(0f, 0.25f)] private float hitFlashDuration = 0.1f;
        [SerializeField] private Color hitFlashColor = Color.white;

        [Header("Camera Shake")]
        [SerializeField, Range(0f, 0.5f)] private float dashCameraShakeDuration = 0.08f;
        [SerializeField, Range(0f, 0.35f)] private float dashCameraShakeMagnitude = 0.06f;

        [Header("Optional Hooks")]
        [SerializeField] private ParticleSystem dashImpactVfxPrefab;
        [SerializeField] private AudioClip dashImpactSfx;
        [SerializeField, Range(0f, 1f)] private float dashImpactSfxVolume = 0.75f;

        public float DashHitPauseDuration => dashHitPauseDuration;
        public float HitFlashDuration => hitFlashDuration;
        public Color HitFlashColor => hitFlashColor;
        public float DashCameraShakeDuration => dashCameraShakeDuration;
        public float DashCameraShakeMagnitude => dashCameraShakeMagnitude;
        public ParticleSystem DashImpactVfxPrefab => dashImpactVfxPrefab;
        public AudioClip DashImpactSfx => dashImpactSfx;
        public float DashImpactSfxVolume => dashImpactSfxVolume;

        private void OnValidate()
        {
            dashHitPauseDuration = Mathf.Clamp(dashHitPauseDuration, 0f, 0.12f);
            hitFlashDuration = Mathf.Clamp(hitFlashDuration, 0f, 0.25f);
            dashCameraShakeDuration = Mathf.Clamp(dashCameraShakeDuration, 0f, 0.5f);
            dashCameraShakeMagnitude = Mathf.Clamp(dashCameraShakeMagnitude, 0f, 0.35f);
            dashImpactSfxVolume = Mathf.Clamp01(dashImpactSfxVolume);
        }
    }
}
