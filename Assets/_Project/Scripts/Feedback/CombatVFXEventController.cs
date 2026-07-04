using TapKnockout.Boss;
using TapKnockout.Combat;
using TapKnockout.Pickups;
using TapKnockout.Survivor;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Feedback
{
    [DisallowMultipleComponent]
    public sealed class CombatVFXEventController : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private VFXService vfxService;
        [SerializeField] private Transform fallbackAnchor;

        [Header("Subscriptions")]
        [SerializeField] private bool listenToProjectileEvents = true;
        [SerializeField] private bool listenToDashEvents = true;
        [SerializeField] private bool listenToActiveSkillEvents = true;
        [SerializeField] private bool listenToEnemyEvents = true;
        [SerializeField] private bool listenToBossEvents = true;
        [SerializeField] private bool listenToPickupEvents = true;

        [Header("Duplication Guards")]
        [SerializeField] private bool skipCatalogSkillVfxWhenDirectPrefabExists = true;

        [Header("Primary Attack")]
        [SerializeField] private VFXEventType primaryProjectileTrailVfx = VFXEventType.PrimaryProjectileTrail;

        [Header("Active Skills")]
        [SerializeField] private VFXEventType forwardCleaveCastVfx = VFXEventType.ForwardCleaveCast;
        [SerializeField] private VFXEventType forwardCleaveHitVfx = VFXEventType.ForwardCleaveHit;
        [SerializeField] private VFXEventType groundImpactCastVfx = VFXEventType.GroundImpactCast;
        [SerializeField] private VFXEventType groundImpactAreaVfx = VFXEventType.GroundImpactArea;
        [SerializeField] private VFXEventType groundImpactHitVfx = VFXEventType.GroundImpactHit;

        [Header("Dash")]
        [SerializeField] private VFXEventType dashStartVfx = VFXEventType.DashStart;
        [SerializeField] private VFXEventType dashTrailVfx = VFXEventType.DashTrail;
        [SerializeField] private VFXEventType dashEndVfx = VFXEventType.DashEnd;

        [Header("Enemy")]
        [SerializeField] private VFXEventType enemySpawnVfx = VFXEventType.EnemySpawn;
        [SerializeField] private VFXEventType eliteSpawnVfx = VFXEventType.EliteSpawn;
        [SerializeField] private VFXEventType eliteDeathVfx = VFXEventType.EliteDeath;
        [SerializeField] private bool spawnRegularEnemySpawnVfx = true;

        [Header("Boss")]
        [SerializeField] private VFXEventType bossSpawnWarningVfx = VFXEventType.BossSpawnWarning;
        [SerializeField] private VFXEventType bossPhaseTransitionVfx = VFXEventType.BossPhaseTransition;
        [SerializeField] private VFXEventType bossHeavyAttackTelegraphVfx = VFXEventType.BossHeavyAttackTelegraph;
        [SerializeField] private VFXEventType bossHeavyAttackImpactVfx = VFXEventType.BossHeavyAttackImpact;
        [SerializeField] private VFXEventType bossDeathVfx = VFXEventType.BossDeath;

        [Header("Reward")]
        [SerializeField] private VFXEventType xpOrbCollectVfx = VFXEventType.XPOrbCollect;
        [SerializeField] private VFXEventType levelUpBurstVfx = VFXEventType.LevelUpBurst;

        [Header("Placement")]
        [SerializeField, Min(0f)] private float groundLift = 0.05f;
        [SerializeField, Min(0f)] private float playerAnchorLift = 0.75f;

        private void Reset()
        {
            vfxService = GetComponent<VFXService>();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (listenToProjectileEvents)
            {
                CombatEvents.OnProjectileSpawned -= HandleProjectileSpawned;
                CombatEvents.OnProjectileSpawned += HandleProjectileSpawned;
            }

            if (listenToDashEvents)
            {
                DashEvents.OnDashStarted -= HandleDashStarted;
                DashEvents.OnDashStarted += HandleDashStarted;
                DashEvents.OnDashEnded -= HandleDashEnded;
                DashEvents.OnDashEnded += HandleDashEnded;
            }

            if (listenToActiveSkillEvents)
            {
                ActiveSkillFeedbackEvents.OnFeedbackRequested -= HandleActiveSkillFeedbackRequested;
                ActiveSkillFeedbackEvents.OnFeedbackRequested += HandleActiveSkillFeedbackRequested;
            }

            if (listenToEnemyEvents)
            {
                SurvivorSpawnDirector.OnAnyEnemySpawned -= HandleEnemySpawned;
                SurvivorSpawnDirector.OnAnyEnemySpawned += HandleEnemySpawned;
                CombatEvents.OnEntityKilled -= HandleEntityKilled;
                CombatEvents.OnEntityKilled += HandleEntityKilled;
            }

            if (listenToBossEvents)
            {
                BossEvents.OnBossWarningStarted -= HandleBossWarningStarted;
                BossEvents.OnBossWarningStarted += HandleBossWarningStarted;
                BossEvents.OnBossIntroStarted -= HandleBossIntroStarted;
                BossEvents.OnBossIntroStarted += HandleBossIntroStarted;
                BossEvents.OnBossPhaseChanged -= HandleBossPhaseChanged;
                BossEvents.OnBossPhaseChanged += HandleBossPhaseChanged;
                BossEvents.OnBossEnraged -= HandleBossEnraged;
                BossEvents.OnBossEnraged += HandleBossEnraged;
                BossEvents.OnBossDefeated -= HandleBossDefeated;
                BossEvents.OnBossDefeated += HandleBossDefeated;
                BossPatternEvents.OnPhaseStarted -= HandleBossPatternPhaseStarted;
                BossPatternEvents.OnPhaseStarted += HandleBossPatternPhaseStarted;
            }

            if (listenToPickupEvents)
            {
                XPOrb.OnAnyCollected -= HandleXPOrbCollected;
                XPOrb.OnAnyCollected += HandleXPOrbCollected;
                PlayerXPController.OnAnyLevelUp -= HandlePlayerLevelUp;
                PlayerXPController.OnAnyLevelUp += HandlePlayerLevelUp;
            }
        }

        private void OnDisable()
        {
            CombatEvents.OnProjectileSpawned -= HandleProjectileSpawned;
            DashEvents.OnDashStarted -= HandleDashStarted;
            DashEvents.OnDashEnded -= HandleDashEnded;
            ActiveSkillFeedbackEvents.OnFeedbackRequested -= HandleActiveSkillFeedbackRequested;
            SurvivorSpawnDirector.OnAnyEnemySpawned -= HandleEnemySpawned;
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            BossEvents.OnBossWarningStarted -= HandleBossWarningStarted;
            BossEvents.OnBossIntroStarted -= HandleBossIntroStarted;
            BossEvents.OnBossPhaseChanged -= HandleBossPhaseChanged;
            BossEvents.OnBossEnraged -= HandleBossEnraged;
            BossEvents.OnBossDefeated -= HandleBossDefeated;
            BossPatternEvents.OnPhaseStarted -= HandleBossPatternPhaseStarted;
            XPOrb.OnAnyCollected -= HandleXPOrbCollected;
            PlayerXPController.OnAnyLevelUp -= HandlePlayerLevelUp;
        }

        public void SetVFXService(VFXService service)
        {
            vfxService = service;
        }

        private void HandleProjectileSpawned(ProjectileSpawnedEvent eventArgs)
        {
            if (eventArgs.Projectile == null)
            {
                return;
            }

            var request = CreateRequest(primaryProjectileTrailVfx, eventArgs.Position, eventArgs.Rotation, 1f, eventArgs.Lifetime);
            request.Parent = eventArgs.Projectile.transform;
            request.Source = eventArgs.Source;
            request.Target = eventArgs.Projectile;
            Spawn(request);
        }

        private void HandleDashStarted(DashStartedEventArgs eventArgs)
        {
            var sourceTransform = eventArgs.Source != null ? eventArgs.Source.transform : fallbackAnchor;
            var position = ResolvePosition(sourceTransform);
            var rotation = ResolveDirectionRotation(eventArgs.Direction);
            Spawn(dashStartVfx, position, rotation, 0.85f, 0.55f, eventArgs.Source, null);

            var trailRequest = CreateRequest(dashTrailVfx, position, rotation, 0.75f, eventArgs.Duration);
            trailRequest.Parent = sourceTransform;
            trailRequest.Source = eventArgs.Source;
            Spawn(trailRequest);
        }

        private void HandleDashEnded(DashEndedEventArgs eventArgs)
        {
            var position = ResolvePosition(eventArgs.Source != null ? eventArgs.Source.transform : fallbackAnchor);
            Spawn(dashEndVfx, position, ResolveDirectionRotation(eventArgs.Direction), 0.8f, 0.55f, eventArgs.Source, null);
        }

        private void HandleActiveSkillFeedbackRequested(ActiveSkillFeedbackEventArgs eventArgs)
        {
            if (skipCatalogSkillVfxWhenDirectPrefabExists && eventArgs.HasDirectPrefab)
            {
                return;
            }

            var eventType = ResolveActiveSkillVFXEvent(eventArgs.EffectType, eventArgs.Phase);
            if (eventType == VFXEventType.GenericBurst)
            {
                return;
            }

            Spawn(
                eventType,
                eventArgs.Position,
                eventArgs.Rotation,
                eventArgs.Scale,
                eventArgs.Lifetime,
                eventArgs.Source,
                null);
        }

        private void HandleEnemySpawned(GameObject enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var traits = enemy.GetComponentInChildren<ICombatTargetTraits>(true);
            if (traits != null && traits.IsBossTarget)
            {
                return;
            }

            if (traits != null && traits.IsEliteTarget)
            {
                Spawn(eliteSpawnVfx, enemy.transform.position, Quaternion.identity, 1.05f, 1.15f, enemy, enemy);
                return;
            }

            if (spawnRegularEnemySpawnVfx)
            {
                Spawn(enemySpawnVfx, enemy.transform.position, Quaternion.identity, 0.55f, 0.8f, enemy, enemy);
            }
        }

        private void HandleEntityKilled(EntityKilledEvent eventArgs)
        {
            if (eventArgs.Entity == null)
            {
                return;
            }

            var traits = eventArgs.Entity.GetComponentInChildren<ICombatTargetTraits>(true);
            if (traits == null || traits.IsBossTarget || !traits.IsEliteTarget)
            {
                return;
            }

            var position = eventArgs.KillingHit != null && eventArgs.KillingHit.HitPoint != Vector3.zero
                ? eventArgs.KillingHit.HitPoint
                : eventArgs.Entity.transform.position;
            Spawn(eliteDeathVfx, position, Quaternion.identity, 1.35f, 1.35f, eventArgs.Killer, eventArgs.Entity);
        }

        private void HandleBossWarningStarted(BossEventArgs eventArgs)
        {
            SpawnBossEvent(bossSpawnWarningVfx, eventArgs.Boss, 1.65f, 2f);
        }

        private void HandleBossIntroStarted(BossEventArgs eventArgs)
        {
            if (eventArgs.Boss != null)
            {
                SpawnBossEvent(bossSpawnWarningVfx, eventArgs.Boss, 1.4f, 1.4f);
            }
        }

        private void HandleBossPhaseChanged(BossPhaseChangedEventArgs eventArgs)
        {
            if (eventArgs.Boss == null ||
                eventArgs.PreviousPhase == BossPhaseState.None ||
                eventArgs.NextPhase == BossPhaseState.Defeated)
            {
                return;
            }

            SpawnBossEvent(bossPhaseTransitionVfx, eventArgs.Boss, 1.35f, 1.25f);
        }

        private void HandleBossEnraged(BossEventArgs eventArgs)
        {
            SpawnBossEvent(bossPhaseTransitionVfx, eventArgs.Boss, 1.5f, 1.35f);
        }

        private void HandleBossDefeated(BossEventArgs eventArgs)
        {
            SpawnBossEvent(bossDeathVfx, eventArgs.Boss, 1.9f, 2.2f);
        }

        private void HandleBossPatternPhaseStarted(BossPatternEventArgs eventArgs)
        {
            if (eventArgs.Phase == BossPatternPhase.Windup)
            {
                Spawn(
                    bossHeavyAttackTelegraphVfx,
                    ResolveBossPatternPosition(eventArgs),
                    ResolveDirectionRotation(ResolveTargetDirection(eventArgs.Source, eventArgs.Target)),
                    1.35f,
                    Mathf.Max(0.25f, eventArgs.PhaseDuration),
                    eventArgs.Source,
                    eventArgs.Target);
                return;
            }

            if (eventArgs.Phase == BossPatternPhase.Active)
            {
                Spawn(
                    bossHeavyAttackImpactVfx,
                    ResolveBossPatternPosition(eventArgs),
                    ResolveDirectionRotation(ResolveTargetDirection(eventArgs.Source, eventArgs.Target)),
                    1.25f,
                    1.05f,
                    eventArgs.Source,
                    eventArgs.Target);
            }
        }

        private void HandleXPOrbCollected(XPOrb orb, PickupCollector collector)
        {
            if (orb == null)
            {
                return;
            }

            var target = collector != null ? collector.gameObject : null;
            Spawn(xpOrbCollectVfx, orb.transform.position, Quaternion.identity, 0.55f, 0.75f, orb.gameObject, target);
        }

        private void HandlePlayerLevelUp(PlayerLevelUpEventArgs eventArgs)
        {
            var source = eventArgs.Source != null ? eventArgs.Source.gameObject : null;
            var position = ResolvePosition(eventArgs.Source != null ? eventArgs.Source.transform : fallbackAnchor);
            position += Vector3.up * playerAnchorLift;
            Spawn(levelUpBurstVfx, position, Quaternion.identity, 1f, 1.2f, source, source);
        }

        private void ResolveReferences()
        {
            if (vfxService == null)
            {
                vfxService = GetComponent<VFXService>();
            }

            if (vfxService == null)
            {
                vfxService = FindFirstObjectByType<VFXService>();
            }

            if (fallbackAnchor == null)
            {
                var collector = FindFirstObjectByType<PickupCollector>();
                fallbackAnchor = collector != null ? collector.transform : transform;
            }
        }

        private VFXEventType ResolveActiveSkillVFXEvent(ActiveSkillEffectType effectType, ActiveSkillFeedbackPhase phase)
        {
            return effectType switch
            {
                ActiveSkillEffectType.ForwardCleave => phase == ActiveSkillFeedbackPhase.Cast
                    ? forwardCleaveCastVfx
                    : forwardCleaveHitVfx,
                ActiveSkillEffectType.GroundImpact => phase switch
                {
                    ActiveSkillFeedbackPhase.Cast => groundImpactCastVfx,
                    ActiveSkillFeedbackPhase.Telegraph => groundImpactAreaVfx,
                    ActiveSkillFeedbackPhase.Impact => groundImpactHitVfx,
                    _ => VFXEventType.GenericBurst
                },
                _ => VFXEventType.GenericBurst
            };
        }

        private void SpawnBossEvent(VFXEventType eventType, GameObject boss, float scale, float lifetime)
        {
            var position = ResolvePosition(boss != null ? boss.transform : fallbackAnchor);
            Spawn(eventType, position, Quaternion.identity, scale, lifetime, boss, boss);
        }

        private void Spawn(
            VFXEventType eventType,
            Vector3 position,
            Quaternion rotation,
            float scale,
            float lifetime,
            GameObject source,
            GameObject target)
        {
            var request = CreateRequest(eventType, position, rotation, scale, lifetime);
            request.Source = source;
            request.Target = target;
            Spawn(request);
        }

        private void Spawn(VFXSpawnRequest request)
        {
            ResolveReferences();
            vfxService?.TrySpawn(request);
        }

        private VFXSpawnRequest CreateRequest(
            VFXEventType eventType,
            Vector3 position,
            Quaternion rotation,
            float scale,
            float lifetime)
        {
            return new VFXSpawnRequest(eventType, position + Vector3.up * groundLift)
            {
                Rotation = rotation,
                Scale = Vector3.one * Mathf.Max(0.05f, scale),
                LifetimeOverride = lifetime
            };
        }

        private Vector3 ResolvePosition(Transform target)
        {
            return target != null ? target.position : transform.position;
        }

        private Vector3 ResolveBossPatternPosition(BossPatternEventArgs eventArgs)
        {
            if (eventArgs.Target != null)
            {
                return eventArgs.Target.transform.position;
            }

            return eventArgs.Source != null ? eventArgs.Source.transform.position : ResolvePosition(fallbackAnchor);
        }

        private static Vector3 ResolveTargetDirection(GameObject source, GameObject target)
        {
            if (source != null && target != null)
            {
                var direction = target.transform.position - source.transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    return direction.normalized;
                }
            }

            return source != null ? source.transform.forward : Vector3.forward;
        }

        private static Quaternion ResolveDirectionRotation(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
        }
    }
}
