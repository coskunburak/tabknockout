using System;
using System.Collections.Generic;
using TapKnockout.Boss;
using TapKnockout.Camera;
using TapKnockout.Combat;
using TapKnockout.Player;
using TapKnockout.Room;
using TapKnockout.UI;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Feedback
{
    [DisallowMultipleComponent]
    public sealed class ImpactFeedbackController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private ImpactFeedbackConfig config;

        [Header("Services")]
        [SerializeField] private VFXService vfxService;
        [SerializeField] private HitPauseService hitPauseService;
        [SerializeField] private CameraShakeReceiver cameraShakeReceiver;
        [SerializeField] private DamageNumberSpawner damageNumberSpawner;
        [SerializeField] private AudioSource audioSource;

        [Header("Event Subscriptions")]
        [SerializeField] private bool listenToDashEvents = true;
        [SerializeField] private bool listenToCombatEvents = true;
        [SerializeField] private bool listenToRoomEvents = true;

        public event Action<HitContext> OnImpactFeedbackTriggered;
        public event Action<VFXSpawnRequest> OnVFXRequested;

        private readonly Dictionary<ImpactFeedbackProfileId, float> lastHitStopTimes = new Dictionary<ImpactFeedbackProfileId, float>();
        private readonly Dictionary<ImpactFeedbackProfileId, float> lastCameraShakeTimes = new Dictionary<ImpactFeedbackProfileId, float>();

        private void Reset()
        {
            vfxService = GetComponent<VFXService>();
            hitPauseService = GetComponent<HitPauseService>();
            damageNumberSpawner = GetComponent<DamageNumberSpawner>();
            cameraShakeReceiver = GetComponent<CameraShakeReceiver>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            if (listenToDashEvents)
            {
                DashEvents.OnDashHit -= HandleDashHit;
                DashEvents.OnDashHit += HandleDashHit;
                DashEvents.OnPerfectDash -= HandlePerfectDash;
                DashEvents.OnPerfectDash += HandlePerfectDash;
                DashEvents.OnProjectileDodged -= HandleProjectileDodged;
                DashEvents.OnProjectileDodged += HandleProjectileDodged;
            }

            if (listenToCombatEvents)
            {
                CombatEvents.OnShotFired -= HandleShotFired;
                CombatEvents.OnShotFired += HandleShotFired;
                CombatEvents.OnDamageDealt -= HandleDamageDealt;
                CombatEvents.OnDamageDealt += HandleDamageDealt;
                CombatEvents.OnDamageReceived -= HandleDamageReceived;
                CombatEvents.OnDamageReceived += HandleDamageReceived;
                CombatEvents.OnEntityKilled -= HandleEntityKilled;
                CombatEvents.OnEntityKilled += HandleEntityKilled;
                ImpactCollisionEvents.OnWallSlam -= HandleWallSlam;
                ImpactCollisionEvents.OnWallSlam += HandleWallSlam;
                ImpactCollisionEvents.OnChainKnockback -= HandleChainKnockback;
                ImpactCollisionEvents.OnChainKnockback += HandleChainKnockback;
                EnemyAttackEvents.OnTelegraphStarted -= HandleEnemyTelegraphStarted;
                EnemyAttackEvents.OnTelegraphStarted += HandleEnemyTelegraphStarted;
                EnemyAttackEvents.OnAttackReleased -= HandleEnemyAttackReleased;
                EnemyAttackEvents.OnAttackReleased += HandleEnemyAttackReleased;
                BossPatternEvents.OnPhaseStarted -= HandleBossPatternPhaseStarted;
                BossPatternEvents.OnPhaseStarted += HandleBossPatternPhaseStarted;
            }

            if (listenToRoomEvents)
            {
                RoomEvents.OnRoomCompleted -= HandleRoomCompleted;
                RoomEvents.OnRoomCompleted += HandleRoomCompleted;
            }
        }

        private void OnDisable()
        {
            DashEvents.OnDashHit -= HandleDashHit;
            DashEvents.OnPerfectDash -= HandlePerfectDash;
            DashEvents.OnProjectileDodged -= HandleProjectileDodged;
            CombatEvents.OnShotFired -= HandleShotFired;
            CombatEvents.OnDamageDealt -= HandleDamageDealt;
            CombatEvents.OnDamageReceived -= HandleDamageReceived;
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            ImpactCollisionEvents.OnWallSlam -= HandleWallSlam;
            ImpactCollisionEvents.OnChainKnockback -= HandleChainKnockback;
            EnemyAttackEvents.OnTelegraphStarted -= HandleEnemyTelegraphStarted;
            EnemyAttackEvents.OnAttackReleased -= HandleEnemyAttackReleased;
            BossPatternEvents.OnPhaseStarted -= HandleBossPatternPhaseStarted;
            RoomEvents.OnRoomCompleted -= HandleRoomCompleted;
        }

        public bool TryTriggerFeedback(HitContext hitContext)
        {
            if (hitContext == null || hitContext.WasIgnored)
            {
                return false;
            }

            return TriggerProfileFeedback(ResolveProfileId(hitContext), hitContext);
        }

        private bool HandleShotFired(ShotFiredEvent eventArgs)
        {
            return TriggerShotFiredFeedback(eventArgs);
        }

        private void HandleDashHit(DashHitEventArgs eventArgs)
        {
            if (eventArgs.HitContext != null)
            {
                TriggerProfileFeedback(ImpactFeedbackProfileId.DashImpact, eventArgs.HitContext);
            }
        }

        private void HandlePerfectDash(PerfectDashEventArgs eventArgs)
        {
            SpawnVFX(PerfectDashVFX, eventArgs.Position, Quaternion.identity, eventArgs.Source, eventArgs.IncomingSource);
            RequestCameraShake(DashCameraShakeAmplitude * 0.5f, DashCameraShakeDuration);
            RaiseAudioHook(FeedbackAudioEventType.PerfectDash, eventArgs.Position, eventArgs.Source, eventArgs.IncomingSource);
        }

        private void HandleProjectileDodged(ProjectileDodgeEventArgs eventArgs)
        {
            SpawnVFX(ProjectileDodgeVFX, eventArgs.Position, Quaternion.identity, eventArgs.Source, eventArgs.ProjectileSource);
            RaiseAudioHook(FeedbackAudioEventType.ProjectileDodge, eventArgs.Position, eventArgs.Source, eventArgs.ProjectileSource);
        }

        private void HandleDamageDealt(DamageEvent damageEvent)
        {
            var hitContext = damageEvent.HitContext;
            if (hitContext == null ||
                hitContext.WasIgnored ||
                hitContext.IsDashHit ||
                IsPlayerTarget(hitContext.Target))
            {
                return;
            }

            TriggerProfileFeedback(ResolveProfileId(hitContext), hitContext);
        }

        private void HandleDamageReceived(DamageEvent damageEvent)
        {
            var hitContext = damageEvent.HitContext;
            if (hitContext == null || hitContext.WasIgnored || !IsPlayerTarget(hitContext.Target))
            {
                return;
            }

            TriggerProfileFeedback(ImpactFeedbackProfileId.PlayerDamaged, hitContext);
        }

        private void HandleEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            if (entityKilledEvent.Entity == null || IsPlayerTarget(entityKilledEvent.Entity))
            {
                return;
            }

            var position = entityKilledEvent.Entity.transform.position;
            if (entityKilledEvent.KillingHit != null)
            {
                position = ResolveHitPosition(entityKilledEvent.KillingHit);
            }

            var rotation = entityKilledEvent.KillingHit != null
                ? ResolveHitRotation(entityKilledEvent.KillingHit)
                : Quaternion.identity;

            TriggerWorldProfileFeedback(
                ImpactFeedbackProfileId.EnemyDeath,
                position,
                rotation,
                entityKilledEvent.Killer,
                entityKilledEvent.Entity);
        }

        private void HandleRoomCompleted(RoomCompletedEventArgs eventArgs)
        {
            var sourceObject = eventArgs.Source != null ? eventArgs.Source.gameObject : null;
            var position = sourceObject != null ? sourceObject.transform.position : Vector3.zero;
            SpawnVFX(RoomClearVFX, position, Quaternion.identity, sourceObject, null);
            RaiseAudioHook(FeedbackAudioEventType.RoomClear, position, sourceObject, null);
        }

        private void HandleWallSlam(ImpactCollisionEventArgs eventArgs)
        {
            SpawnVFX(WallSlamVFX, eventArgs.Position, Quaternion.identity, eventArgs.Source, eventArgs.PrimaryTarget);
            RequestHitPause(DashHitPauseDuration, ResolveProfile(ImpactFeedbackProfileId.DashImpact).HitStopCooldown);
            RequestCameraShake(DashCameraShakeAmplitude, DashCameraShakeDuration);
            ShowDamageNumber(eventArgs.HitContext, DamageNumberStyle.WallSlam, 1.15f);
            RaiseAudioHook(FeedbackAudioEventType.WallSlam, eventArgs.Position, eventArgs.Source, eventArgs.PrimaryTarget);
        }

        private void HandleChainKnockback(ImpactCollisionEventArgs eventArgs)
        {
            SpawnVFX(ChainKnockbackVFX, eventArgs.Position, Quaternion.identity, eventArgs.Source, eventArgs.SecondaryTarget);
            RequestHitPause(NormalHitPauseDuration, ResolveProfile(ImpactFeedbackProfileId.SkillHit).HitStopCooldown);
            ShowDamageNumber(eventArgs.HitContext, DamageNumberStyle.ChainKnockback, 1.05f);
            RaiseAudioHook(FeedbackAudioEventType.ChainKnockback, eventArgs.Position, eventArgs.Source, eventArgs.SecondaryTarget);
        }

        private void HandleEnemyTelegraphStarted(EnemyAttackEventArgs eventArgs)
        {
            SpawnVFX(EnemyTelegraphVFX, eventArgs.Position, Quaternion.identity, eventArgs.Source, eventArgs.Target);
            RaiseAudioHook(FeedbackAudioEventType.EnemyTelegraph, eventArgs.Position, eventArgs.Source, eventArgs.Target);
        }

        private void HandleEnemyAttackReleased(EnemyAttackEventArgs eventArgs)
        {
            SpawnVFX(VFXEventType.EnemyAttackRelease, eventArgs.Position, Quaternion.identity, eventArgs.Source, eventArgs.Target);
            RaiseAudioHook(FeedbackAudioEventType.EnemyAttackRelease, eventArgs.Position, eventArgs.Source, eventArgs.Target);
        }

        private void HandleBossPatternPhaseStarted(BossPatternEventArgs eventArgs)
        {
            if (eventArgs.Phase != BossPatternPhase.Windup)
            {
                return;
            }

            var position = eventArgs.Source != null ? eventArgs.Source.transform.position : Vector3.zero;
            SpawnVFX(BossPatternTelegraphVFX, position, Quaternion.identity, eventArgs.Source, eventArgs.Target);
            RaiseAudioHook(FeedbackAudioEventType.BossPatternTelegraph, position, eventArgs.Source, eventArgs.Target);
        }

        private bool TriggerProfileFeedback(ImpactFeedbackProfileId profileId, HitContext hitContext)
        {
            if (hitContext == null)
            {
                return false;
            }

            var profile = ResolveProfile(profileId);
            if (!CanRunProfile(profile, hitContext.DamageAmount))
            {
                return false;
            }

            var position = ResolveHitPosition(hitContext);
            var rotation = ResolveHitRotation(hitContext);

            SpawnProfileVFX(profile, position, rotation, hitContext.Source, hitContext.Target);
            RequestProfileHitStop(profile);
            TriggerProfileHitFlash(profile, hitContext.Target);
            RequestProfileCameraShake(profile);
            ShowDamageNumber(hitContext, profile.DamageNumberStyle, profile.DamageNumberScale, profile);
            PlayProfileAudio(profile, position, hitContext.Source, hitContext.Target, null, null, 1f);
            OnImpactFeedbackTriggered?.Invoke(hitContext);
            return true;
        }

        private bool TriggerWorldProfileFeedback(
            ImpactFeedbackProfileId profileId,
            Vector3 position,
            Quaternion rotation,
            GameObject source,
            GameObject target)
        {
            var profile = ResolveProfile(profileId);
            if (!CanRunProfile(profile, 0f, true))
            {
                return false;
            }

            SpawnProfileVFX(profile, position, rotation, source, target);
            RequestProfileHitStop(profile);
            RequestProfileCameraShake(profile);
            PlayProfileAudio(profile, position, source, target, null, null, 1f);
            return true;
        }

        private bool TriggerShotFiredFeedback(ShotFiredEvent eventArgs)
        {
            var profile = ResolveProfile(ImpactFeedbackProfileId.ShotFired);
            if (!CanRunProfile(profile, 0f, true))
            {
                return false;
            }

            var position = eventArgs.Position;
            if (position == Vector3.zero && eventArgs.Source != null)
            {
                position = eventArgs.Source.transform.position;
            }

            var rotation = eventArgs.Rotation;
            if (IsDefaultQuaternion(rotation))
            {
                rotation = eventArgs.Direction.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(eventArgs.Direction.normalized, Vector3.up)
                    : Quaternion.identity;
            }

            if (profile.PulseReticle)
            {
                eventArgs.ReticlePulseTarget?.Pulse(profile.ReticlePulseScaleAdd, profile.ReticlePulseDuration);
            }

            if (profile.PlayMuzzleFlash && eventArgs.MuzzleFlash != null)
            {
                eventArgs.MuzzleFlash.Play(true);
            }

            SpawnProfileVFX(profile, position, rotation, eventArgs.Source, null);
            if (profile.PulseReticle && eventArgs.HasReticlePosition)
            {
                SpawnVFX(
                    ReticleFirePulseVFX,
                    eventArgs.ReticlePosition,
                    Quaternion.identity,
                    eventArgs.Source,
                    null,
                    Mathf.Max(0.1f, profile.ReticlePulseScaleAdd));
            }

            RequestProfileCameraShake(profile);
            PlayProfileAudio(profile, position, eventArgs.Source, null, eventArgs.AudioSource, eventArgs.ShotSfx, eventArgs.ShotSfxVolume);
            return true;
        }

        private ImpactFeedbackProfileId ResolveProfileId(HitContext hitContext)
        {
            if (hitContext.IsDashHit)
            {
                return ImpactFeedbackProfileId.DashImpact;
            }

            if (IsPlayerTarget(hitContext.Target))
            {
                return ImpactFeedbackProfileId.PlayerDamaged;
            }

            if (IsBossTarget(hitContext.Target))
            {
                return ImpactFeedbackProfileId.BossHit;
            }

            if (hitContext.IsAbilityHit || !string.IsNullOrEmpty(hitContext.AbilityId))
            {
                return ImpactFeedbackProfileId.SkillHit;
            }

            if (hitContext.IsProjectileHit &&
                (hitContext.IsCritical || hitContext.DamageAmount >= HeavyProjectileDamageThreshold))
            {
                return ImpactFeedbackProfileId.HeavyProjectileHit;
            }

            return ImpactFeedbackProfileId.NormalProjectileHit;
        }

        private ImpactFeedbackProfile ResolveProfile(ImpactFeedbackProfileId profileId)
        {
            return config != null
                ? config.GetProfile(profileId)
                : ImpactFeedbackProfile.CreateDefault(profileId);
        }

        private static bool CanRunProfile(ImpactFeedbackProfile profile, float damageAmount, bool ignoreMinimumDamage = false)
        {
            return profile != null &&
                profile.Enabled &&
                (ignoreMinimumDamage || damageAmount >= profile.MinimumDamage);
        }

        private void SpawnProfileVFX(
            ImpactFeedbackProfile profile,
            Vector3 position,
            Quaternion rotation,
            GameObject source,
            GameObject target)
        {
            if (profile == null || !profile.SpawnVFX)
            {
                return;
            }

            SpawnVFX(profile.VFXEvent, position, rotation, source, target, profile.VFXScale);
        }

        private void RequestProfileHitStop(ImpactFeedbackProfile profile)
        {
            if (profile == null ||
                !profile.ApplyHitStop ||
                !EnableHitPause ||
                hitPauseService == null ||
                profile.HitStopDuration <= 0f ||
                !IsProfileCooldownReady(lastHitStopTimes, profile.ProfileId, profile.HitStopCooldown))
            {
                return;
            }

            if (hitPauseService.RequestHitPause(profile.HitStopDuration, profile.HitStopCooldown, profile.HitStopTimeScale))
            {
                lastHitStopTimes[profile.ProfileId] = Time.unscaledTime;
            }
        }

        private void TriggerProfileHitFlash(ImpactFeedbackProfile profile, GameObject target)
        {
            if (profile == null || !profile.PlayHitFlash)
            {
                return;
            }

            TriggerHitFlash(target, profile.HitFlashColor, profile.HitFlashDuration);
        }

        private void RequestProfileCameraShake(ImpactFeedbackProfile profile)
        {
            if (profile == null ||
                !profile.ApplyCameraShake ||
                !IsProfileCooldownReady(lastCameraShakeTimes, profile.ProfileId, profile.CameraShakeCooldown))
            {
                return;
            }

            if (RequestCameraShake(profile.CameraShakeAmplitude, profile.CameraShakeDuration))
            {
                lastCameraShakeTimes[profile.ProfileId] = Time.unscaledTime;
            }
        }

        private void PlayProfileAudio(
            ImpactFeedbackProfile profile,
            Vector3 position,
            GameObject source,
            GameObject target,
            AudioSource fallbackAudioSource,
            AudioClip fallbackClip,
            float fallbackVolume)
        {
            if (profile == null || !profile.PlaySFX || !EnableSFXHooks)
            {
                return;
            }

            var clip = profile.SFX != null ? profile.SFX : fallbackClip;
            var targetAudioSource = audioSource != null ? audioSource : fallbackAudioSource;
            if (clip != null && targetAudioSource != null)
            {
                var volume = profile.SFX != null ? profile.SFXVolume : profile.SFXVolume * Mathf.Clamp01(fallbackVolume);
                targetAudioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            }

            FeedbackAudioEvents.RaiseFeedbackAudioRequested(new FeedbackAudioEventArgs(
                profile.AudioEvent,
                position,
                source,
                target,
                profile.AudioIntensity));
        }

        private bool IsProfileCooldownReady(
            Dictionary<ImpactFeedbackProfileId, float> timestamps,
            ImpactFeedbackProfileId profileId,
            float cooldown)
        {
            if (cooldown <= 0f)
            {
                return true;
            }

            return !timestamps.TryGetValue(profileId, out var lastTime) ||
                Time.unscaledTime - lastTime >= cooldown;
        }

        private void SpawnVFX(VFXEventType eventType, Vector3 position, Quaternion rotation, GameObject source, GameObject target)
        {
            SpawnVFX(eventType, position, rotation, source, target, 1f);
        }

        private void SpawnVFX(
            VFXEventType eventType,
            Vector3 position,
            Quaternion rotation,
            GameObject source,
            GameObject target,
            float scale)
        {
            if (!EnableVFX || vfxService == null)
            {
                return;
            }

            var request = new VFXSpawnRequest(eventType, position)
            {
                Rotation = rotation,
                Source = source,
                Target = target,
                Scale = Vector3.one * Mathf.Max(0.1f, scale)
            };

            OnVFXRequested?.Invoke(request);
            vfxService.Spawn(request);
        }

        private void RequestHitPause(float duration)
        {
            RequestHitPause(duration, -1f);
        }

        private void RequestHitPause(float duration, float cooldown)
        {
            if (!EnableHitPause || hitPauseService == null || duration <= 0f)
            {
                return;
            }

            hitPauseService.RequestHitPause(duration, cooldown);
        }

        private void TriggerHitFlash(GameObject target, Color color, float duration)
        {
            if (!EnableHitFlash || target == null || duration <= 0f)
            {
                return;
            }

            var hitFlashController = target.GetComponentInChildren<HitFlashController>();
            hitFlashController?.Flash(color, duration);
        }

        private bool RequestCameraShake(float amplitude, float duration)
        {
            if (!EnableCameraShake || cameraShakeReceiver == null || amplitude <= 0f || duration <= 0f)
            {
                return false;
            }

            cameraShakeReceiver.Shake(amplitude, duration);
            return true;
        }

        private void ShowDamageNumber(HitContext hitContext, DamageNumberStyle style, float scale)
        {
            ShowDamageNumber(hitContext, style, scale, null);
        }

        private void ShowDamageNumber(
            HitContext hitContext,
            DamageNumberStyle style,
            float scale,
            ImpactFeedbackProfile profile)
        {
            if (!EnableDamageNumbers ||
                damageNumberSpawner == null ||
                hitContext == null ||
                hitContext.DamageAmount <= 0f ||
                profile != null && !profile.ShowDamageNumber)
            {
                return;
            }

            damageNumberSpawner.ShowDamage(
                hitContext.DamageAmount,
                ResolveHitPosition(hitContext),
                hitContext.Target,
                style,
                scale);
        }

        private void RaiseAudioHook(FeedbackAudioEventType eventType, Vector3 position, GameObject source, GameObject target)
        {
            if (!EnableSFXHooks)
            {
                return;
            }

            FeedbackAudioEvents.RaiseFeedbackAudioRequested(new FeedbackAudioEventArgs(eventType, position, source, target));
        }

        private static bool IsPlayerTarget(GameObject target)
        {
            return target != null && target.GetComponentInParent<PlayerHealth>() != null;
        }

        private static bool IsBossTarget(GameObject target)
        {
            return target != null &&
                (target.GetComponentInParent<BossPhaseController>() != null ||
                target.GetComponentInParent<BossPatternController>() != null);
        }

        private static Vector3 ResolveHitPosition(HitContext hitContext)
        {
            if (hitContext == null)
            {
                return Vector3.zero;
            }

            if (hitContext.HitPoint != Vector3.zero)
            {
                return hitContext.HitPoint;
            }

            if (hitContext.Target != null)
            {
                return hitContext.Target.transform.position;
            }

            return hitContext.Source != null ? hitContext.Source.transform.position : Vector3.zero;
        }

        private static Quaternion ResolveHitRotation(HitContext hitContext)
        {
            if (hitContext == null || hitContext.HitDirection.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var direction = hitContext.HitDirection;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction.normalized, Vector3.up) : Quaternion.identity;
        }

        private static bool IsDefaultQuaternion(Quaternion value)
        {
            return Mathf.Approximately(value.x, 0f) &&
                Mathf.Approximately(value.y, 0f) &&
                Mathf.Approximately(value.z, 0f) &&
                Mathf.Approximately(value.w, 0f);
        }

        private bool EnableHitPause => config == null || config.EnableHitPause;
        private bool EnableHitFlash => config == null || config.EnableHitFlash;
        private bool EnableCameraShake => config == null || config.EnableCameraShake;
        private bool EnableDamageNumbers => config == null || config.EnableDamageNumbers;
        private bool EnableVFX => config == null || config.EnableVFX;
        private bool EnableSFXHooks => config == null || config.EnableSFXHooks;
        private float DashHitPauseDuration => config != null ? config.DashHitPauseDuration : 0.05f;
        private float NormalHitPauseDuration => config != null ? config.NormalHitPauseDuration : 0.025f;
        private float HeavyProjectileDamageThreshold => config != null ? config.HeavyProjectileDamageThreshold : 12f;
        private float DashCameraShakeAmplitude => config != null ? config.DashCameraShakeAmplitude : 0.06f;
        private float DashCameraShakeDuration => config != null ? config.DashCameraShakeDuration : 0.08f;
        private VFXEventType RoomClearVFX => config != null ? config.RoomClearVFX : VFXEventType.RoomClear;
        private VFXEventType WallSlamVFX => config != null ? config.WallSlamVFX : VFXEventType.WallSlam;
        private VFXEventType ChainKnockbackVFX => config != null ? config.ChainKnockbackVFX : VFXEventType.ChainKnockback;
        private VFXEventType PerfectDashVFX => config != null ? config.PerfectDashVFX : VFXEventType.PerfectDash;
        private VFXEventType ProjectileDodgeVFX => config != null ? config.ProjectileDodgeVFX : VFXEventType.ProjectileDodge;
        private VFXEventType EnemyTelegraphVFX => config != null ? config.EnemyTelegraphVFX : VFXEventType.EnemyTelegraph;
        private VFXEventType BossPatternTelegraphVFX => config != null ? config.BossPatternTelegraphVFX : VFXEventType.BossPatternTelegraph;
        private VFXEventType ReticleFirePulseVFX => config != null ? config.ReticleFirePulseVFX : VFXEventType.ReticleFirePulse;
    }
}
