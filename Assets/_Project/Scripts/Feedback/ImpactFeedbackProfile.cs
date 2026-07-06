using System;
using TapKnockout.UI;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Feedback
{
    [Serializable]
    public sealed class ImpactFeedbackProfile
    {
        [SerializeField] private ImpactFeedbackProfileId profileId = ImpactFeedbackProfileId.NormalProjectileHit;
        [SerializeField] private bool enabled = true;
        [SerializeField, Min(0f)] private float minimumDamage;

        [Header("Hit Flash")]
        [SerializeField] private bool playHitFlash = true;
        [SerializeField, Range(0f, 0.25f)] private float hitFlashDuration = 0.08f;
        [SerializeField] private Color hitFlashColor = Color.white;

        [Header("Damage Numbers")]
        [SerializeField] private bool showDamageNumber = true;
        [SerializeField] private DamageNumberStyle damageNumberStyle = DamageNumberStyle.Normal;
        [SerializeField, Range(0.2f, 2.5f)] private float damageNumberScale = 1f;

        [Header("Hit Stop")]
        [SerializeField] private bool applyHitStop;
        [SerializeField, Range(0f, 0.12f)] private float hitStopDuration = 0.025f;
        [SerializeField, Range(0f, 0.15f)] private float hitStopTimeScale;
        [SerializeField, Min(0f)] private float hitStopCooldown = 0.08f;

        [Header("Camera Shake")]
        [SerializeField] private bool applyCameraShake;
        [SerializeField, Range(0f, 0.25f)] private float cameraShakeAmplitude = 0.02f;
        [SerializeField, Range(0f, 0.25f)] private float cameraShakeDuration = 0.04f;
        [SerializeField, Min(0f)] private float cameraShakeCooldown = 0.08f;

        [Header("VFX")]
        [SerializeField] private bool spawnVFX = true;
        [SerializeField] private VFXEventType vfxEvent = VFXEventType.ProjectileHit;
        [SerializeField, Range(0.1f, 3f)] private float vfxScale = 1f;

        [Header("Audio")]
        [SerializeField] private bool playSFX = true;
        [SerializeField] private AudioClip sfx;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] private FeedbackAudioEventType audioEvent = FeedbackAudioEventType.ProjectileHit;
        [SerializeField, Range(0f, 3f)] private float audioIntensity = 1f;

        [Header("Shot Fired")]
        [SerializeField] private bool pulseReticle;
        [SerializeField, Range(0f, 0.75f)] private float reticlePulseScaleAdd = 0.16f;
        [SerializeField, Range(0.01f, 0.35f)] private float reticlePulseDuration = 0.12f;
        [SerializeField] private bool playMuzzleFlash;

        public ImpactFeedbackProfileId ProfileId => profileId;
        public bool Enabled => enabled;
        public float MinimumDamage => minimumDamage;
        public bool PlayHitFlash => playHitFlash;
        public float HitFlashDuration => hitFlashDuration;
        public Color HitFlashColor => hitFlashColor;
        public bool ShowDamageNumber => showDamageNumber;
        public DamageNumberStyle DamageNumberStyle => damageNumberStyle;
        public float DamageNumberScale => damageNumberScale;
        public bool ApplyHitStop => applyHitStop;
        public float HitStopDuration => hitStopDuration;
        public float HitStopTimeScale => hitStopTimeScale;
        public float HitStopCooldown => hitStopCooldown;
        public bool ApplyCameraShake => applyCameraShake;
        public float CameraShakeAmplitude => cameraShakeAmplitude;
        public float CameraShakeDuration => cameraShakeDuration;
        public float CameraShakeCooldown => cameraShakeCooldown;
        public bool SpawnVFX => spawnVFX;
        public VFXEventType VFXEvent => vfxEvent;
        public float VFXScale => vfxScale;
        public bool PlaySFX => playSFX;
        public AudioClip SFX => sfx;
        public float SFXVolume => sfxVolume;
        public FeedbackAudioEventType AudioEvent => audioEvent;
        public float AudioIntensity => audioIntensity;
        public bool PulseReticle => pulseReticle;
        public float ReticlePulseScaleAdd => reticlePulseScaleAdd;
        public float ReticlePulseDuration => reticlePulseDuration;
        public bool PlayMuzzleFlash => playMuzzleFlash;

        public static ImpactFeedbackProfile CreateDefault(ImpactFeedbackProfileId id)
        {
            var profile = new ImpactFeedbackProfile
            {
                profileId = id
            };
            profile.ApplyDefaultsForId();
            profile.Validate();
            return profile;
        }

        public void Validate()
        {
            minimumDamage = Mathf.Max(0f, minimumDamage);
            hitFlashDuration = Mathf.Clamp(hitFlashDuration, 0f, 0.25f);
            damageNumberScale = Mathf.Clamp(damageNumberScale, 0.2f, 2.5f);
            hitStopDuration = Mathf.Clamp(hitStopDuration, 0f, 0.12f);
            hitStopTimeScale = Mathf.Clamp(hitStopTimeScale, 0f, 0.15f);
            hitStopCooldown = Mathf.Max(0f, hitStopCooldown);
            cameraShakeAmplitude = Mathf.Clamp(cameraShakeAmplitude, 0f, 0.25f);
            cameraShakeDuration = Mathf.Clamp(cameraShakeDuration, 0f, 0.25f);
            cameraShakeCooldown = Mathf.Max(0f, cameraShakeCooldown);
            vfxScale = Mathf.Clamp(vfxScale, 0.1f, 3f);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            audioIntensity = Mathf.Clamp(audioIntensity, 0f, 3f);
            reticlePulseScaleAdd = Mathf.Clamp(reticlePulseScaleAdd, 0f, 0.75f);
            reticlePulseDuration = Mathf.Clamp(reticlePulseDuration, 0.01f, 0.35f);
        }

        private void ApplyDefaultsForId()
        {
            switch (profileId)
            {
                case ImpactFeedbackProfileId.HeavyProjectileHit:
                    minimumDamage = 12f;
                    playHitFlash = true;
                    hitFlashDuration = 0.09f;
                    hitFlashColor = new Color(1f, 0.92f, 0.55f, 1f);
                    damageNumberStyle = DamageNumberStyle.HeavyProjectile;
                    damageNumberScale = 1.12f;
                    applyHitStop = true;
                    hitStopDuration = 0.025f;
                    hitStopCooldown = 0.1f;
                    applyCameraShake = true;
                    cameraShakeAmplitude = 0.025f;
                    cameraShakeDuration = 0.045f;
                    cameraShakeCooldown = 0.08f;
                    vfxEvent = VFXEventType.ProjectileHit;
                    audioEvent = FeedbackAudioEventType.ProjectileHit;
                    break;

                case ImpactFeedbackProfileId.SkillHit:
                    playHitFlash = true;
                    hitFlashDuration = 0.11f;
                    hitFlashColor = new Color(1f, 0.72f, 0.28f, 1f);
                    damageNumberStyle = DamageNumberStyle.Skill;
                    damageNumberScale = 1.2f;
                    applyHitStop = true;
                    hitStopDuration = 0.035f;
                    hitStopCooldown = 0.12f;
                    applyCameraShake = true;
                    cameraShakeAmplitude = 0.035f;
                    cameraShakeDuration = 0.06f;
                    cameraShakeCooldown = 0.1f;
                    vfxEvent = VFXEventType.GenericBurst;
                    vfxScale = 1.15f;
                    audioEvent = FeedbackAudioEventType.SkillHit;
                    audioIntensity = 1.2f;
                    break;

                case ImpactFeedbackProfileId.DashImpact:
                    playHitFlash = true;
                    hitFlashDuration = 0.11f;
                    hitFlashColor = new Color(1f, 0.74f, 0.22f, 1f);
                    damageNumberStyle = DamageNumberStyle.DashImpact;
                    damageNumberScale = 1.25f;
                    applyHitStop = true;
                    hitStopDuration = 0.05f;
                    hitStopCooldown = 0.12f;
                    applyCameraShake = true;
                    cameraShakeAmplitude = 0.06f;
                    cameraShakeDuration = 0.08f;
                    cameraShakeCooldown = 0.1f;
                    vfxEvent = VFXEventType.DashImpact;
                    vfxScale = 1.2f;
                    audioEvent = FeedbackAudioEventType.DashImpact;
                    audioIntensity = 1.25f;
                    break;

                case ImpactFeedbackProfileId.EnemyDeath:
                    playHitFlash = false;
                    showDamageNumber = false;
                    applyHitStop = false;
                    applyCameraShake = true;
                    cameraShakeAmplitude = 0.015f;
                    cameraShakeDuration = 0.04f;
                    cameraShakeCooldown = 0.12f;
                    vfxEvent = VFXEventType.EnemyDeath;
                    vfxScale = 1.1f;
                    audioEvent = FeedbackAudioEventType.EnemyDeath;
                    break;

                case ImpactFeedbackProfileId.BossHit:
                    playHitFlash = true;
                    hitFlashDuration = 0.08f;
                    hitFlashColor = new Color(1f, 0.85f, 0.35f, 1f);
                    damageNumberStyle = DamageNumberStyle.Boss;
                    damageNumberScale = 1.18f;
                    applyHitStop = true;
                    hitStopDuration = 0.03f;
                    hitStopCooldown = 0.12f;
                    applyCameraShake = true;
                    cameraShakeAmplitude = 0.03f;
                    cameraShakeDuration = 0.055f;
                    cameraShakeCooldown = 0.1f;
                    vfxEvent = VFXEventType.BossHit;
                    vfxScale = 1.15f;
                    audioEvent = FeedbackAudioEventType.BossHit;
                    audioIntensity = 1.2f;
                    break;

                case ImpactFeedbackProfileId.PlayerDamaged:
                    playHitFlash = true;
                    hitFlashDuration = 0.12f;
                    hitFlashColor = new Color(1f, 0.2f, 0.12f, 1f);
                    damageNumberStyle = DamageNumberStyle.PlayerDamage;
                    damageNumberScale = 1.1f;
                    applyHitStop = false;
                    applyCameraShake = true;
                    cameraShakeAmplitude = 0.045f;
                    cameraShakeDuration = 0.08f;
                    cameraShakeCooldown = 0.15f;
                    spawnVFX = false;
                    vfxEvent = VFXEventType.GenericBurst;
                    audioEvent = FeedbackAudioEventType.PlayerDamaged;
                    audioIntensity = 1.2f;
                    break;

                case ImpactFeedbackProfileId.ShotFired:
                    playHitFlash = false;
                    showDamageNumber = false;
                    applyHitStop = false;
                    applyCameraShake = true;
                    cameraShakeAmplitude = 0.012f;
                    cameraShakeDuration = 0.035f;
                    cameraShakeCooldown = 0.03f;
                    vfxEvent = VFXEventType.PrimaryFireMuzzle;
                    audioEvent = FeedbackAudioEventType.ShotFired;
                    pulseReticle = true;
                    reticlePulseScaleAdd = 0.16f;
                    reticlePulseDuration = 0.12f;
                    playMuzzleFlash = true;
                    break;

                case ImpactFeedbackProfileId.NormalProjectileHit:
                default:
                    playHitFlash = true;
                    hitFlashDuration = 0.07f;
                    hitFlashColor = new Color(0.82f, 0.95f, 1f, 1f);
                    damageNumberStyle = DamageNumberStyle.Projectile;
                    damageNumberScale = 0.95f;
                    applyHitStop = false;
                    applyCameraShake = false;
                    vfxEvent = VFXEventType.ProjectileHit;
                    audioEvent = FeedbackAudioEventType.ProjectileHit;
                    break;
            }
        }
    }
}
