using System;
using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.Player;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Feedback
{
    [DisallowMultipleComponent]
    public sealed class AbilityVFXFeedbackController : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private VFXService vfxService;
        [SerializeField] private PlayerRuntimeStats playerRuntimeStats;
        [SerializeField] private Transform playerAnchor;

        [Header("Subscriptions")]
        [SerializeField] private bool listenToAbilityEvents = true;
        [SerializeField] private bool listenToCombatEvents = true;
        [SerializeField] private bool listenToDashEvents = true;

        [Header("Selection Readability")]
        [SerializeField] private bool spawnOfferPulse = true;
        [SerializeField] private bool spawnSelectionBurst = true;
        [SerializeField] private Vector3 playerAnchorOffset = new Vector3(0f, 0.75f, 0f);
        [SerializeField, Min(0.05f)] private float offerLifetime = 0.85f;
        [SerializeField, Min(0.05f)] private float selectionLifetime = 1.15f;
        [SerializeField, Min(0.05f)] private float selectionScale = 1.05f;
        [SerializeField, Min(0f)] private float stackScaleStep = 0.08f;

        [Header("Combat Accents")]
        [SerializeField] private bool spawnElementalHitAccents = true;
        [SerializeField] private bool spawnProjectileBehaviorAccents = true;
        [SerializeField] private bool spawnDashAbilityAccents = true;
        [SerializeField, Range(0f, 1f)] private float procVisualChanceScale = 1f;
        [SerializeField, Range(0f, 1f)] private float projectileBehaviorAccentChance = 0.35f;
        [SerializeField, Min(0.05f)] private float hitAccentLifetime = 0.8f;
        [SerializeField, Min(0.05f)] private float dashAccentLifetime = 0.95f;

        public event Action<VFXSpawnRequest> OnVFXRequested;

        public VFXService VFXService => vfxService;
        public PlayerRuntimeStats PlayerRuntimeStats => playerRuntimeStats;

        private void Reset()
        {
            vfxService = GetComponent<VFXService>();
            ResolvePlayerReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (listenToAbilityEvents)
            {
                AbilityEvents.OnAbilityOfferGenerated -= HandleAbilityOfferGenerated;
                AbilityEvents.OnAbilityOfferGenerated += HandleAbilityOfferGenerated;
                AbilityEvents.OnAbilitySelected -= HandleAbilitySelected;
                AbilityEvents.OnAbilitySelected += HandleAbilitySelected;
            }

            if (listenToCombatEvents)
            {
                CombatEvents.OnDamageDealt -= HandleDamageDealt;
                CombatEvents.OnDamageDealt += HandleDamageDealt;
                CombatEvents.OnEntityKilled -= HandleEntityKilled;
                CombatEvents.OnEntityKilled += HandleEntityKilled;
            }

            if (listenToDashEvents)
            {
                DashEvents.OnDashStarted -= HandleDashStarted;
                DashEvents.OnDashStarted += HandleDashStarted;
                DashEvents.OnDashHit -= HandleDashHit;
                DashEvents.OnDashHit += HandleDashHit;
            }
        }

        private void OnDisable()
        {
            AbilityEvents.OnAbilityOfferGenerated -= HandleAbilityOfferGenerated;
            AbilityEvents.OnAbilitySelected -= HandleAbilitySelected;
            CombatEvents.OnDamageDealt -= HandleDamageDealt;
            CombatEvents.OnEntityKilled -= HandleEntityKilled;
            DashEvents.OnDashStarted -= HandleDashStarted;
            DashEvents.OnDashHit -= HandleDashHit;
        }

        public void SetVFXService(VFXService service)
        {
            vfxService = service;
        }

        public void SetPlayerRuntimeStats(PlayerRuntimeStats stats)
        {
            playerRuntimeStats = stats;
            if (playerAnchor == null && playerRuntimeStats != null)
            {
                playerAnchor = playerRuntimeStats.transform;
            }
        }

        public void SetPlayerAnchor(Transform anchor)
        {
            playerAnchor = anchor;
        }

        public bool TrySpawnSelectionVFX(AbilityDefinition ability, int stackCount)
        {
            if (ability == null || !spawnSelectionBurst)
            {
                return false;
            }

            var eventType = AbilityVFXEventResolver.ResolveSelectionEvent(ability);
            var request = CreatePlayerAnchoredRequest(eventType, selectionLifetime, selectionScale + Mathf.Max(0, stackCount - 1) * stackScaleStep);
            request.ColorOverride = AbilityVFXEventResolver.ResolveColor(ability.EffectType);
            request.Source = ResolvePlayerObject();

            return Dispatch(request);
        }

        private void HandleAbilityOfferGenerated(AbilityOfferEventArgs eventArgs)
        {
            if (!spawnOfferPulse || !eventArgs.HasChoices)
            {
                return;
            }

            Dispatch(CreatePlayerAnchoredRequest(VFXEventType.AbilityOffered, offerLifetime, 0.72f));
        }

        private void HandleAbilitySelected(AbilitySelectedEventArgs eventArgs)
        {
            if (!eventArgs.HasSelectedAbility)
            {
                return;
            }

            TrySpawnSelectionVFX(eventArgs.SelectedAbility, eventArgs.StackCount);
        }

        private void HandleDamageDealt(DamageEvent damageEvent)
        {
            if (damageEvent.HitContext == null || !IsTrackedPlayerSource(damageEvent.HitContext.Source))
            {
                return;
            }

            if (spawnElementalHitAccents && AbilityVFXEventResolver.TryResolveDamageTypeEvent(damageEvent.DamageType, out var damageTypeEvent))
            {
                SpawnHitAccent(damageTypeEvent, damageEvent.HitContext, 0.85f);
                return;
            }

            if (spawnElementalHitAccents && TrySpawnProcAccent(damageEvent.HitContext))
            {
                return;
            }

            if (spawnProjectileBehaviorAccents)
            {
                TrySpawnProjectileBehaviorAccent(damageEvent.HitContext);
            }
        }

        private void HandleEntityKilled(EntityKilledEvent entityKilledEvent)
        {
            if (!IsTrackedPlayerSource(entityKilledEvent.Killer))
            {
                return;
            }

            ResolvePlayerReferences();
            if (playerRuntimeStats == null)
            {
                return;
            }

            var position = entityKilledEvent.Entity != null
                ? entityKilledEvent.Entity.transform.position
                : ResolveHitPosition(entityKilledEvent.KillingHit);

            if (playerRuntimeStats.HealOnKillAmount > 0f)
            {
                Dispatch(new VFXSpawnRequest(VFXEventType.AbilitySoulHeal, position + playerAnchorOffset)
                {
                    LifetimeOverride = hitAccentLifetime,
                    Scale = Vector3.one * 0.9f,
                    ColorOverride = new Color(0.35f, 1f, 0.55f, 1f),
                    Source = ResolvePlayerObject(),
                    Target = entityKilledEvent.Entity
                });
            }

            if (playerRuntimeStats.DashCooldownRefundOnKill > 0f && entityKilledEvent.KillingHit != null && entityKilledEvent.KillingHit.IsDashHit)
            {
                Dispatch(new VFXSpawnRequest(VFXEventType.AbilityDashBuff, position)
                {
                    LifetimeOverride = dashAccentLifetime,
                    Scale = Vector3.one * 0.95f,
                    Source = ResolvePlayerObject(),
                    Target = entityKilledEvent.Entity
                });
            }
        }

        private void HandleDashStarted(DashStartedEventArgs eventArgs)
        {
            if (!spawnDashAbilityAccents || !IsTrackedPlayerSource(eventArgs.Source))
            {
                return;
            }

            ResolvePlayerReferences();
            if (playerRuntimeStats == null)
            {
                return;
            }

            if (playerRuntimeStats.DashIFrameBonus > 0f)
            {
                var request = CreatePlayerAnchoredRequest(VFXEventType.AbilityDashPhase, dashAccentLifetime, 0.85f);
                request.Rotation = ResolveDirectionRotation(eventArgs.Direction);
                Dispatch(request);
                return;
            }

            if (playerRuntimeStats.DashCooldownMultiplier < 0.98f)
            {
                var request = CreatePlayerAnchoredRequest(VFXEventType.AbilityDashBuff, dashAccentLifetime, 0.75f);
                request.Rotation = ResolveDirectionRotation(eventArgs.Direction);
                Dispatch(request);
            }
        }

        private void HandleDashHit(DashHitEventArgs eventArgs)
        {
            if (!spawnDashAbilityAccents || eventArgs.HitContext == null || !IsTrackedPlayerSource(eventArgs.Source))
            {
                return;
            }

            ResolvePlayerReferences();
            if (playerRuntimeStats == null)
            {
                return;
            }

            if (playerRuntimeStats.DashShockwaveRadius > 0f)
            {
                SpawnHitAccent(VFXEventType.AbilityDashShockwave, eventArgs.HitContext, 1.2f + playerRuntimeStats.DashShockwaveRadius * 0.08f);
            }

            if (playerRuntimeStats.DashStunDuration > 0f)
            {
                SpawnHitAccent(VFXEventType.AbilityDashStagger, eventArgs.HitContext, 0.85f);
            }

            if (playerRuntimeStats.DashShieldAfterHit)
            {
                Dispatch(CreatePlayerAnchoredRequest(VFXEventType.AbilityShield, dashAccentLifetime, 0.95f));
            }

            if (playerRuntimeStats.DashDamageMultiplier > 1.001f || playerRuntimeStats.DashKnockbackMultiplier > 1.001f)
            {
                SpawnHitAccent(VFXEventType.AbilityDashBuff, eventArgs.HitContext, 0.75f);
            }
        }

        private bool TrySpawnProcAccent(HitContext hitContext)
        {
            if (hitContext == null || !hitContext.IsProjectileHit)
            {
                return false;
            }

            ResolvePlayerReferences();
            if (playerRuntimeStats == null)
            {
                return false;
            }

            if (Roll(playerRuntimeStats.LightningOnHitChance))
            {
                SpawnHitAccent(VFXEventType.AbilityLightningProc, hitContext, 0.9f);
                return true;
            }

            if (Roll(playerRuntimeStats.BurnOnHitChance))
            {
                SpawnHitAccent(VFXEventType.AbilityFireProc, hitContext, 0.9f);
                return true;
            }

            if (Roll(playerRuntimeStats.PoisonOnHitChance))
            {
                SpawnHitAccent(VFXEventType.AbilityPoisonProc, hitContext, 0.85f);
                return true;
            }

            if (Roll(playerRuntimeStats.FreezeOnHitChance))
            {
                SpawnHitAccent(VFXEventType.AbilityIceProc, hitContext, 0.85f);
                return true;
            }

            return false;
        }

        private bool TrySpawnProjectileBehaviorAccent(HitContext hitContext)
        {
            if (hitContext == null || !hitContext.IsProjectileHit || !RollFlat(projectileBehaviorAccentChance))
            {
                return false;
            }

            ResolvePlayerReferences();
            if (playerRuntimeStats == null)
            {
                return false;
            }

            if (playerRuntimeStats.ProjectileRicochetCount > 0 || playerRuntimeStats.ProjectileWallBounceCount > 0)
            {
                SpawnHitAccent(VFXEventType.AbilityProjectileRicochet, hitContext, 0.85f);
                return true;
            }

            if (playerRuntimeStats.ProjectilePierceCount > 0)
            {
                SpawnHitAccent(VFXEventType.AbilityProjectilePierce, hitContext, 0.75f);
                return true;
            }

            if (playerRuntimeStats.ProjectileSizeMultiplier > 1.001f)
            {
                SpawnHitAccent(VFXEventType.AbilityProjectileSize, hitContext, Mathf.Min(1.35f, playerRuntimeStats.ProjectileSizeMultiplier));
                return true;
            }

            if (playerRuntimeStats.ExtraProjectileCount > 0
                || playerRuntimeStats.FrontProjectileCount > 0
                || playerRuntimeStats.DiagonalProjectileCount > 0
                || playerRuntimeStats.SideProjectileCount > 0
                || playerRuntimeStats.RearProjectileCount > 0)
            {
                SpawnHitAccent(VFXEventType.AbilityProjectileSplit, hitContext, 0.7f);
                return true;
            }

            if (playerRuntimeStats.ProjectileHomingStrength > 0f || playerRuntimeStats.ProjectileSpeedMultiplier > 1.001f)
            {
                SpawnHitAccent(VFXEventType.AbilityProjectileBuff, hitContext, 0.7f);
                return true;
            }

            return false;
        }

        private void SpawnHitAccent(VFXEventType eventType, HitContext hitContext, float scale)
        {
            var request = new VFXSpawnRequest(eventType, ResolveHitPosition(hitContext))
            {
                Rotation = ResolveHitRotation(hitContext),
                LifetimeOverride = eventType == VFXEventType.AbilityDashShockwave ? dashAccentLifetime : hitAccentLifetime,
                Scale = Vector3.one * Mathf.Max(0.05f, scale),
                Source = hitContext.Source,
                Target = hitContext.Target
            };

            Dispatch(request);
        }

        private VFXSpawnRequest CreatePlayerAnchoredRequest(VFXEventType eventType, float lifetime, float scale)
        {
            var playerObject = ResolvePlayerObject();
            var position = playerAnchor != null ? playerAnchor.position : transform.position;
            return new VFXSpawnRequest(eventType, position + playerAnchorOffset)
            {
                LifetimeOverride = lifetime,
                Scale = Vector3.one * Mathf.Max(0.05f, scale),
                Source = playerObject,
                Target = playerObject
            };
        }

        private bool Dispatch(VFXSpawnRequest request)
        {
            OnVFXRequested?.Invoke(request);

            if (vfxService == null)
            {
                return false;
            }

            if (vfxService.TrySpawn(request))
            {
                return true;
            }

            var originalEventType = request.EventType;
            var fallbackCandidates = ResolveFallbackCandidates(originalEventType);
            for (var i = 0; i < fallbackCandidates.Count; i++)
            {
                var fallbackEventType = fallbackCandidates[i];
                if (fallbackEventType == originalEventType)
                {
                    continue;
                }

                request.EventType = fallbackEventType;
                OnVFXRequested?.Invoke(request);
                if (vfxService.TrySpawn(request))
                {
                    return true;
                }
            }

            return false;
        }

        private void ResolveReferences()
        {
            if (vfxService == null)
            {
                vfxService = GetComponent<VFXService>();
            }

            ResolvePlayerReferences();
        }

        private void ResolvePlayerReferences()
        {
            if (playerRuntimeStats == null)
            {
                playerRuntimeStats = FindFirstObjectByType<PlayerRuntimeStats>();
            }

            if (playerAnchor == null && playerRuntimeStats != null)
            {
                playerAnchor = playerRuntimeStats.transform;
            }
        }

        private bool IsTrackedPlayerSource(GameObject source)
        {
            ResolvePlayerReferences();
            if (playerRuntimeStats == null || source == null)
            {
                return false;
            }

            var playerTransform = playerRuntimeStats.transform;
            return source == playerRuntimeStats.gameObject || source.transform.IsChildOf(playerTransform);
        }

        private GameObject ResolvePlayerObject()
        {
            ResolvePlayerReferences();
            return playerRuntimeStats != null ? playerRuntimeStats.gameObject : null;
        }

        private bool Roll(float chance)
        {
            return RollFlat(Mathf.Clamp01(chance * procVisualChanceScale));
        }

        private static bool RollFlat(float chance)
        {
            return chance >= 1f || chance > 0f && UnityEngine.Random.value <= chance;
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

            return ResolveDirectionRotation(hitContext.HitDirection);
        }

        private static Quaternion ResolveDirectionRotation(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
        }

        private static IReadOnlyList<VFXEventType> ResolveFallbackCandidates(VFXEventType eventType)
        {
            return eventType switch
            {
                VFXEventType.AbilityAttackBuff => SelectionFallbacks,
                VFXEventType.AbilityAttackSpeedBuff => SelectionFallbacks,
                VFXEventType.AbilityDefenseBuff => SelectionFallbacks,
                VFXEventType.AbilityMoveSpeedBuff => SelectionFallbacks,
                VFXEventType.AbilityGenericUpgrade => SelectionFallbacks,
                VFXEventType.AbilityHealthBuff => HealFallbacks,
                VFXEventType.AbilitySoulHeal => HealFallbacks,
                VFXEventType.AbilityRevive => HealFallbacks,
                VFXEventType.AbilityDashBuff => DashFallbacks,
                VFXEventType.AbilityDashShockwave => DashFallbacks,
                VFXEventType.AbilityDashPhase => DashFallbacks,
                VFXEventType.AbilityDashStagger => DashFallbacks,
                VFXEventType.AbilityProjectileBuff => ProjectileFallbacks,
                VFXEventType.AbilityProjectileSplit => ProjectileFallbacks,
                VFXEventType.AbilityProjectilePierce => ProjectileFallbacks,
                VFXEventType.AbilityProjectileRicochet => ProjectileFallbacks,
                VFXEventType.AbilityProjectileHoming => ProjectileFallbacks,
                VFXEventType.AbilityProjectileSize => ProjectileFallbacks,
                VFXEventType.AbilityFireProc => ElementalFallbacks,
                VFXEventType.AbilityPoisonProc => ElementalFallbacks,
                VFXEventType.AbilityIceProc => ElementalFallbacks,
                VFXEventType.AbilityLightningProc => ElementalFallbacks,
                VFXEventType.AbilityShield => ShieldFallbacks,
                VFXEventType.AbilityBossBreaker => BossFallbacks,
                VFXEventType.AbilityLowHealthSurge => SelectionFallbacks,
                VFXEventType.AbilityRewardLuck => RewardFallbacks,
                VFXEventType.AbilityPickupFrenzy => RewardFallbacks,
                VFXEventType.AbilityOrbital => ProjectileFallbacks,
                VFXEventType.AbilityDrone => ProjectileFallbacks,
                VFXEventType.AbilityBladeStrike => ProjectileFallbacks,
                VFXEventType.AbilityMeteor => BossFallbacks,
                VFXEventType.AbilityEnergyBeam => ElementalFallbacks,
                VFXEventType.AbilityEnergyRing => BossFallbacks,
                VFXEventType.GenericBurst => GenericFallbacks,
                _ => System.Array.Empty<VFXEventType>()
            };
        }

        private static readonly VFXEventType[] SelectionFallbacks =
        {
            VFXEventType.AbilitySelected,
            VFXEventType.LevelUpBurst,
            VFXEventType.GenericBurst,
            VFXEventType.ProjectileHit
        };

        private static readonly VFXEventType[] DashFallbacks =
        {
            VFXEventType.DashImpact,
            VFXEventType.DashEnd,
            VFXEventType.ProjectileHit,
            VFXEventType.LevelUpBurst
        };

        private static readonly VFXEventType[] ProjectileFallbacks =
        {
            VFXEventType.ProjectileHit,
            VFXEventType.PrimaryProjectileImpact,
            VFXEventType.EnemyHit,
            VFXEventType.LevelUpBurst
        };

        private static readonly VFXEventType[] ElementalFallbacks =
        {
            VFXEventType.ProjectileHit,
            VFXEventType.EnemyHit,
            VFXEventType.DashImpact,
            VFXEventType.LevelUpBurst
        };

        private static readonly VFXEventType[] HealFallbacks =
        {
            VFXEventType.Heal,
            VFXEventType.XPOrbCollect,
            VFXEventType.LevelUpBurst,
            VFXEventType.AbilitySelected
        };

        private static readonly VFXEventType[] ShieldFallbacks =
        {
            VFXEventType.AbilitySelected,
            VFXEventType.DashImpact,
            VFXEventType.LevelUpBurst
        };

        private static readonly VFXEventType[] BossFallbacks =
        {
            VFXEventType.BossHit,
            VFXEventType.BossWarning,
            VFXEventType.ProjectileHit,
            VFXEventType.LevelUpBurst
        };

        private static readonly VFXEventType[] RewardFallbacks =
        {
            VFXEventType.RoomClear,
            VFXEventType.Pickup,
            VFXEventType.XPOrbCollect,
            VFXEventType.LevelUpBurst
        };

        private static readonly VFXEventType[] GenericFallbacks =
        {
            VFXEventType.LevelUpBurst,
            VFXEventType.ProjectileHit,
            VFXEventType.DashImpact
        };
    }
}
