using System;
using TapKnockout.Boss;
using TapKnockout.Camera;
using TapKnockout.Combat;
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

        [Header("Event Subscriptions")]
        [SerializeField] private bool listenToDashEvents = true;
        [SerializeField] private bool listenToCombatEvents = true;
        [SerializeField] private bool listenToRoomEvents = true;

        public event Action<HitContext> OnImpactFeedbackTriggered;
        public event Action<VFXSpawnRequest> OnVFXRequested;

        private void Reset()
        {
            vfxService = GetComponent<VFXService>();
            hitPauseService = GetComponent<HitPauseService>();
            damageNumberSpawner = GetComponent<DamageNumberSpawner>();
            cameraShakeReceiver = GetComponent<CameraShakeReceiver>();
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
                CombatEvents.OnDamageDealt -= HandleDamageDealt;
                CombatEvents.OnDamageDealt += HandleDamageDealt;
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
            CombatEvents.OnDamageDealt -= HandleDamageDealt;
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
            if (hitContext == null)
            {
                return false;
            }

            if (hitContext.IsDashHit)
            {
                TriggerDashHitFeedback(hitContext);
                return true;
            }

            if (hitContext.IsProjectileHit)
            {
                TriggerProjectileHitFeedback(hitContext);
                return true;
            }

            TriggerGenericHitFeedback(hitContext);
            return true;
        }

        private void HandleDashHit(DashHitEventArgs eventArgs)
        {
            if (eventArgs.HitContext != null)
            {
                TriggerDashHitFeedback(eventArgs.HitContext);
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
            if (hitContext == null || hitContext.IsDashHit)
            {
                return;
            }

            if (hitContext.IsProjectileHit)
            {
                TriggerProjectileHitFeedback(hitContext);
                return;
            }

            TriggerGenericHitFeedback(hitContext);
        }

        private void HandleEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            var position = entityKilledEvent.Entity != null ? entityKilledEvent.Entity.transform.position : ResolveHitPosition(entityKilledEvent.KillingHit);
            SpawnVFX(EnemyDeathVFX, position, Quaternion.identity, entityKilledEvent.Killer, entityKilledEvent.Entity);
            RaiseAudioHook(FeedbackAudioEventType.EnemyDeath, position, entityKilledEvent.Killer, entityKilledEvent.Entity);
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
            RequestHitPause(DashHitPauseDuration);
            RequestCameraShake(DashCameraShakeAmplitude, DashCameraShakeDuration);
            ShowDamageNumber(eventArgs.HitContext, DamageNumberStyle.WallSlam);
            RaiseAudioHook(FeedbackAudioEventType.WallSlam, eventArgs.Position, eventArgs.Source, eventArgs.PrimaryTarget);
        }

        private void HandleChainKnockback(ImpactCollisionEventArgs eventArgs)
        {
            SpawnVFX(ChainKnockbackVFX, eventArgs.Position, Quaternion.identity, eventArgs.Source, eventArgs.SecondaryTarget);
            RequestHitPause(NormalHitPauseDuration);
            ShowDamageNumber(eventArgs.HitContext, DamageNumberStyle.ChainKnockback);
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

        private void TriggerDashHitFeedback(HitContext hitContext)
        {
            var position = ResolveHitPosition(hitContext);
            var rotation = ResolveHitRotation(hitContext);

            SpawnVFX(DashImpactVFX, position, rotation, hitContext.Source, hitContext.Target);
            RequestHitPause(DashHitPauseDuration);
            TriggerHitFlash(hitContext.Target);
            RequestCameraShake(DashCameraShakeAmplitude, DashCameraShakeDuration);
            ShowDamageNumber(hitContext, DamageNumberStyle.DashImpact);
            RaiseAudioHook(FeedbackAudioEventType.DashImpact, position, hitContext.Source, hitContext.Target);
            OnImpactFeedbackTriggered?.Invoke(hitContext);
        }

        private void TriggerProjectileHitFeedback(HitContext hitContext)
        {
            var position = ResolveHitPosition(hitContext);
            var rotation = ResolveHitRotation(hitContext);

            SpawnVFX(ProjectileHitVFX, position, rotation, hitContext.Source, hitContext.Target);
            RequestHitPause(NormalHitPauseDuration);
            TriggerHitFlash(hitContext.Target);
            ShowDamageNumber(hitContext, DamageNumberStyle.Projectile);
            RaiseAudioHook(FeedbackAudioEventType.ProjectileHit, position, hitContext.Source, hitContext.Target);
            OnImpactFeedbackTriggered?.Invoke(hitContext);
        }

        private void TriggerGenericHitFeedback(HitContext hitContext)
        {
            var position = ResolveHitPosition(hitContext);
            var rotation = ResolveHitRotation(hitContext);

            SpawnVFX(EnemyHitVFX, position, rotation, hitContext.Source, hitContext.Target);
            RequestHitPause(NormalHitPauseDuration);
            TriggerHitFlash(hitContext.Target);
            ShowDamageNumber(hitContext, hitContext.IsCritical ? DamageNumberStyle.Critical : DamageNumberStyle.Normal);
            OnImpactFeedbackTriggered?.Invoke(hitContext);
        }

        private void SpawnVFX(VFXEventType eventType, Vector3 position, Quaternion rotation, GameObject source, GameObject target)
        {
            if (!EnableVFX || vfxService == null)
            {
                return;
            }

            var request = new VFXSpawnRequest(eventType, position)
            {
                Rotation = rotation,
                Source = source,
                Target = target
            };

            OnVFXRequested?.Invoke(request);
            vfxService.Spawn(request);
        }

        private void RequestHitPause(float duration)
        {
            if (!EnableHitPause || hitPauseService == null || duration <= 0f)
            {
                return;
            }

            hitPauseService.RequestHitPause(duration);
        }

        private void TriggerHitFlash(GameObject target)
        {
            if (!EnableHitFlash || target == null || HitFlashDuration <= 0f)
            {
                return;
            }

            var hitFlashController = target.GetComponentInChildren<HitFlashController>();
            hitFlashController?.Flash(HitFlashColor, HitFlashDuration);
        }

        private void RequestCameraShake(float amplitude, float duration)
        {
            if (!EnableCameraShake || cameraShakeReceiver == null || amplitude <= 0f || duration <= 0f)
            {
                return;
            }

            cameraShakeReceiver.Shake(amplitude, duration);
        }

        private void ShowDamageNumber(HitContext hitContext)
        {
            ShowDamageNumber(hitContext, DamageNumberStyle.Normal);
        }

        private void ShowDamageNumber(HitContext hitContext, DamageNumberStyle style)
        {
            if (!EnableDamageNumbers || damageNumberSpawner == null || hitContext == null || hitContext.DamageAmount <= 0f)
            {
                return;
            }

            damageNumberSpawner.ShowDamage(hitContext.DamageAmount, ResolveHitPosition(hitContext), hitContext.Target, style);
        }

        private void RaiseAudioHook(FeedbackAudioEventType eventType, Vector3 position, GameObject source, GameObject target)
        {
            if (!EnableSFXHooks)
            {
                return;
            }

            FeedbackAudioEvents.RaiseFeedbackAudioRequested(new FeedbackAudioEventArgs(eventType, position, source, target));
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

        private bool EnableHitPause => config == null || config.EnableHitPause;
        private bool EnableHitFlash => config == null || config.EnableHitFlash;
        private bool EnableCameraShake => config == null || config.EnableCameraShake;
        private bool EnableDamageNumbers => config != null && config.EnableDamageNumbers;
        private bool EnableVFX => config == null || config.EnableVFX;
        private bool EnableSFXHooks => config == null || config.EnableSFXHooks;
        private float DashHitPauseDuration => config != null ? config.DashHitPauseDuration : 0.05f;
        private float NormalHitPauseDuration => config != null ? config.NormalHitPauseDuration : 0.025f;
        private float HitFlashDuration => config != null ? config.HitFlashDuration : 0.1f;
        private Color HitFlashColor => config != null ? config.HitFlashColor : Color.white;
        private float DashCameraShakeAmplitude => config != null ? config.DashCameraShakeAmplitude : 0.06f;
        private float DashCameraShakeDuration => config != null ? config.DashCameraShakeDuration : 0.08f;
        private VFXEventType DashImpactVFX => config != null ? config.DashImpactVFX : VFXEventType.DashImpact;
        private VFXEventType ProjectileHitVFX => config != null ? config.ProjectileHitVFX : VFXEventType.ProjectileHit;
        private VFXEventType EnemyDeathVFX => config != null ? config.EnemyDeathVFX : VFXEventType.EnemyDeath;
        private VFXEventType RoomClearVFX => config != null ? config.RoomClearVFX : VFXEventType.RoomClear;
        private VFXEventType EnemyHitVFX => config != null ? config.EnemyHitVFX : VFXEventType.EnemyHit;
        private VFXEventType WallSlamVFX => config != null ? config.WallSlamVFX : VFXEventType.WallSlam;
        private VFXEventType ChainKnockbackVFX => config != null ? config.ChainKnockbackVFX : VFXEventType.ChainKnockback;
        private VFXEventType PerfectDashVFX => config != null ? config.PerfectDashVFX : VFXEventType.PerfectDash;
        private VFXEventType ProjectileDodgeVFX => config != null ? config.ProjectileDodgeVFX : VFXEventType.ProjectileDodge;
        private VFXEventType EnemyTelegraphVFX => config != null ? config.EnemyTelegraphVFX : VFXEventType.EnemyTelegraph;
        private VFXEventType BossPatternTelegraphVFX => config != null ? config.BossPatternTelegraphVFX : VFXEventType.BossPatternTelegraph;
    }
}
