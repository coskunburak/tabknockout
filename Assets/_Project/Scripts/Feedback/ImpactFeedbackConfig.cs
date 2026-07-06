using System.Collections.Generic;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Feedback
{
    [CreateAssetMenu(fileName = "ImpactFeedbackConfig", menuName = "Tap Knockout/Feedback/Impact Feedback Config")]
    public sealed class ImpactFeedbackConfig : ScriptableObject
    {
        [Header("Feature Toggles")]
        [SerializeField] private bool enableHitPause = true;
        [SerializeField] private bool enableHitFlash = true;
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private bool enableDamageNumbers = true;
        [SerializeField] private bool enableVFX = true;
        [SerializeField] private bool enableSFXHooks = true;

        [Header("Hit Pause")]
        [SerializeField, Range(0f, 0.12f)] private float dashHitPauseDuration = 0.05f;
        [SerializeField, Range(0f, 0.08f)] private float normalHitPauseDuration = 0.025f;
        [SerializeField, Min(0f)] private float heavyProjectileDamageThreshold = 12f;

        [Header("Hit Flash")]
        [SerializeField, Range(0f, 0.25f)] private float hitFlashDuration = 0.1f;
        [SerializeField] private Color hitFlashColor = Color.white;

        [Header("Camera Shake")]
        [SerializeField, Range(0f, 0.25f)] private float dashCameraShakeAmplitude = 0.06f;
        [SerializeField, Range(0f, 0.25f)] private float dashCameraShakeDuration = 0.08f;
        [SerializeField, Range(0f, 0.15f)] private float normalCameraShakeAmplitude = 0.02f;
        [SerializeField, Range(0f, 0.15f)] private float normalCameraShakeDuration = 0.04f;
        [SerializeField, Min(0f)] private float minimumDamageForNormalCameraShake = 8f;

        [Header("VFX Mapping")]
        [SerializeField] private VFXEventType dashImpactVFX = VFXEventType.DashImpact;
        [SerializeField] private VFXEventType projectileHitVFX = VFXEventType.ProjectileHit;
        [SerializeField] private VFXEventType enemyDeathVFX = VFXEventType.EnemyDeath;
        [SerializeField] private VFXEventType roomClearVFX = VFXEventType.RoomClear;
        [SerializeField] private VFXEventType enemyHitVFX = VFXEventType.EnemyHit;
        [SerializeField] private VFXEventType wallSlamVFX = VFXEventType.WallSlam;
        [SerializeField] private VFXEventType chainKnockbackVFX = VFXEventType.ChainKnockback;
        [SerializeField] private VFXEventType perfectDashVFX = VFXEventType.PerfectDash;
        [SerializeField] private VFXEventType projectileDodgeVFX = VFXEventType.ProjectileDodge;
        [SerializeField] private VFXEventType lowHealthWarningVFX = VFXEventType.LowHealthWarning;
        [SerializeField] private VFXEventType enemyTelegraphVFX = VFXEventType.EnemyTelegraph;
        [SerializeField] private VFXEventType bossPatternTelegraphVFX = VFXEventType.BossPatternTelegraph;
        [SerializeField] private VFXEventType reticleFirePulseVFX = VFXEventType.ReticleFirePulse;

        [Header("Profiles")]
        [SerializeField] private List<ImpactFeedbackProfile> profiles = new List<ImpactFeedbackProfile>();

        public bool EnableHitPause => enableHitPause;
        public bool EnableHitFlash => enableHitFlash;
        public bool EnableCameraShake => enableCameraShake;
        public bool EnableDamageNumbers => enableDamageNumbers;
        public bool EnableVFX => enableVFX;
        public bool EnableSFXHooks => enableSFXHooks;
        public float DashHitPauseDuration => dashHitPauseDuration;
        public float NormalHitPauseDuration => normalHitPauseDuration;
        public float HeavyProjectileDamageThreshold => heavyProjectileDamageThreshold;
        public float HitFlashDuration => hitFlashDuration;
        public Color HitFlashColor => hitFlashColor;
        public float DashCameraShakeAmplitude => dashCameraShakeAmplitude;
        public float DashCameraShakeDuration => dashCameraShakeDuration;
        public float NormalCameraShakeAmplitude => normalCameraShakeAmplitude;
        public float NormalCameraShakeDuration => normalCameraShakeDuration;
        public float MinimumDamageForNormalCameraShake => minimumDamageForNormalCameraShake;
        public VFXEventType DashImpactVFX => dashImpactVFX;
        public VFXEventType ProjectileHitVFX => projectileHitVFX;
        public VFXEventType EnemyDeathVFX => enemyDeathVFX;
        public VFXEventType RoomClearVFX => roomClearVFX;
        public VFXEventType EnemyHitVFX => enemyHitVFX;
        public VFXEventType WallSlamVFX => wallSlamVFX;
        public VFXEventType ChainKnockbackVFX => chainKnockbackVFX;
        public VFXEventType PerfectDashVFX => perfectDashVFX;
        public VFXEventType ProjectileDodgeVFX => projectileDodgeVFX;
        public VFXEventType LowHealthWarningVFX => lowHealthWarningVFX;
        public VFXEventType EnemyTelegraphVFX => enemyTelegraphVFX;
        public VFXEventType BossPatternTelegraphVFX => bossPatternTelegraphVFX;
        public VFXEventType ReticleFirePulseVFX => reticleFirePulseVFX;
        public IReadOnlyList<ImpactFeedbackProfile> Profiles => profiles;

        public ImpactFeedbackProfile GetProfile(ImpactFeedbackProfileId profileId)
        {
            if (profiles != null)
            {
                for (var i = 0; i < profiles.Count; i++)
                {
                    var profile = profiles[i];
                    if (profile != null && profile.ProfileId == profileId)
                    {
                        return profile;
                    }
                }
            }

            return ImpactFeedbackProfile.CreateDefault(profileId);
        }

        public bool HasProfile(ImpactFeedbackProfileId profileId)
        {
            if (profiles == null)
            {
                return false;
            }

            for (var i = 0; i < profiles.Count; i++)
            {
                if (profiles[i] != null && profiles[i].ProfileId == profileId)
                {
                    return true;
                }
            }

            return false;
        }

        public void EnsureProfileDefaults()
        {
            profiles ??= new List<ImpactFeedbackProfile>();

            for (var i = profiles.Count - 1; i >= 0; i--)
            {
                if (profiles[i] == null)
                {
                    profiles.RemoveAt(i);
                }
            }

            foreach (ImpactFeedbackProfileId profileId in System.Enum.GetValues(typeof(ImpactFeedbackProfileId)))
            {
                if (!HasProfile(profileId))
                {
                    profiles.Add(ImpactFeedbackProfile.CreateDefault(profileId));
                }
            }

            for (var i = 0; i < profiles.Count; i++)
            {
                profiles[i]?.Validate();
            }
        }

        private void OnValidate()
        {
            dashHitPauseDuration = Mathf.Clamp(dashHitPauseDuration, 0f, 0.12f);
            normalHitPauseDuration = Mathf.Clamp(normalHitPauseDuration, 0f, 0.08f);
            heavyProjectileDamageThreshold = Mathf.Max(0f, heavyProjectileDamageThreshold);
            hitFlashDuration = Mathf.Clamp(hitFlashDuration, 0f, 0.25f);
            dashCameraShakeAmplitude = Mathf.Clamp(dashCameraShakeAmplitude, 0f, 0.25f);
            dashCameraShakeDuration = Mathf.Clamp(dashCameraShakeDuration, 0f, 0.25f);
            normalCameraShakeAmplitude = Mathf.Clamp(normalCameraShakeAmplitude, 0f, 0.15f);
            normalCameraShakeDuration = Mathf.Clamp(normalCameraShakeDuration, 0f, 0.15f);
            minimumDamageForNormalCameraShake = Mathf.Max(0f, minimumDamageForNormalCameraShake);
            EnsureProfileDefaults();
        }
    }
}
