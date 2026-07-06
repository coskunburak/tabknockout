using TapKnockout.Ability;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAbilityEffectApplier : MonoBehaviour, IAbilityEffectApplier
    {
        [Header("References")]
        [SerializeField] private PlayerRuntimeStats runtimeStats;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Debug")]
        [SerializeField] private bool logAppliedEffects;
        [SerializeField] private bool logUnsupportedEffects;

        public PlayerRuntimeStats RuntimeStats => runtimeStats;
        public PlayerHealth PlayerHealth => playerHealth;

        private void Reset()
        {
            runtimeStats = GetComponent<PlayerRuntimeStats>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void SetRuntimeStats(PlayerRuntimeStats stats)
        {
            runtimeStats = stats;
        }

        public void SetPlayerHealth(PlayerHealth health)
        {
            playerHealth = health;
        }

        public void ApplyAbility(AbilityEffectContext context)
        {
            if (!context.IsValid)
            {
                return;
            }

            ResolveReferences();
            if (runtimeStats == null)
            {
                Debug.LogWarning($"{nameof(PlayerAbilityEffectApplier)} on {name} cannot apply {context.Ability.AbilityId} because no {nameof(PlayerRuntimeStats)} is assigned.", this);
                return;
            }

            var ability = context.Ability;
            switch (ability.EffectType)
            {
                case AbilityEffectType.AttackDamageUp:
                    runtimeStats.AddAttackDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.AttackSpeedUp:
                    runtimeStats.AddAttackCooldownReduction(ability.Value);
                    break;
                case AbilityEffectType.CriticalChanceUp:
                    runtimeStats.AddCritChance(ability.Value);
                    break;
                case AbilityEffectType.CritDamageUp:
                    runtimeStats.AddCritDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.DamageReductionUp:
                    runtimeStats.AddDamageReduction(ability.Value);
                    break;
                case AbilityEffectType.DodgeChanceUp:
                    runtimeStats.AddDodgeChance(ability.Value);
                    break;
                case AbilityEffectType.DashCooldownDown:
                    runtimeStats.AddDashCooldownReduction(ability.Value);
                    break;
                case AbilityEffectType.DashDamageUp:
                    runtimeStats.AddDashDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.DashKnockbackUp:
                    runtimeStats.AddDashKnockbackMultiplier(ability.Value);
                    break;
                case AbilityEffectType.DashIFrameDurationUp:
                    runtimeStats.AddDashIFrameBonus(ability.Value);
                    break;
                case AbilityEffectType.DashStun:
                    runtimeStats.AddDashStunDuration(ResolveDurationOrValue(ability));
                    break;
                case AbilityEffectType.DashShockwave:
                    runtimeStats.AddDashShockwaveRadius(ability.Value);
                    break;
                case AbilityEffectType.ChargedShot:
                case AbilityEffectType.EnergyBeam:
                case AbilityEffectType.EnergyRing:
                case AbilityEffectType.DashBeam:
                    // ActiveSkillController equips and resolves these as hotkey-cast skills.
                    break;
                case AbilityEffectType.DashShieldAfterHit:
                    runtimeStats.EnableDashShieldAfterHit();
                    break;
                case AbilityEffectType.DashCooldownRefundOnKill:
                    runtimeStats.AddDashCooldownRefundOnKill(ability.Value);
                    break;
                case AbilityEffectType.DashDamageLowHealth:
                    runtimeStats.AddDashLowHealthDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.MoveSpeedUp:
                    runtimeStats.AddMoveSpeedMultiplier(ability.Value);
                    break;
                case AbilityEffectType.ProjectileSpeedUp:
                    runtimeStats.AddProjectileSpeedMultiplier(ability.Value);
                    break;
                case AbilityEffectType.MaxHealthUp:
                    ApplyMaxHealthUp(ability.Value);
                    break;
                case AbilityEffectType.ExtraProjectile:
                    runtimeStats.AddExtraProjectileCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.FrontProjectile:
                    runtimeStats.AddFrontProjectileCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.DiagonalProjectiles:
                    runtimeStats.AddDiagonalProjectileCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.SideShot:
                case AbilityEffectType.SideProjectiles:
                    runtimeStats.AddSideProjectileCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.RearProjectile:
                    runtimeStats.AddRearProjectileCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.Pierce:
                case AbilityEffectType.ProjectilePierce:
                    runtimeStats.AddProjectilePierceCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.Ricochet:
                case AbilityEffectType.ProjectileRicochet:
                    runtimeStats.AddProjectileRicochetCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.ProjectileWallBounce:
                    runtimeStats.AddProjectileWallBounceCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.ProjectileHoming:
                    runtimeStats.AddProjectileHomingStrength(ability.Value);
                    break;
                case AbilityEffectType.ProjectileSizeUp:
                    runtimeStats.AddProjectileSizeMultiplier(ability.Value);
                    break;
                case AbilityEffectType.LongRangeDamageUp:
                    runtimeStats.AddLongRangeDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.BurningHits:
                case AbilityEffectType.BurnOnHit:
                case AbilityEffectType.SuperBurn:
                    runtimeStats.AddBurnOnHit(ResolveProcChanceOrValue(ability));
                    break;
                case AbilityEffectType.PoisonOnHit:
                case AbilityEffectType.SuperPoison:
                    runtimeStats.AddPoisonOnHit(ResolveProcChanceOrValue(ability));
                    break;
                case AbilityEffectType.FreezeOnHit:
                case AbilityEffectType.SuperFreeze:
                    runtimeStats.AddFreezeOnHit(ResolveProcChanceOrValue(ability));
                    break;
                case AbilityEffectType.ChainLightning:
                case AbilityEffectType.LightningOnHit:
                case AbilityEffectType.SuperLightning:
                    runtimeStats.AddLightningOnHit(ResolveProcChanceOrValue(ability));
                    break;
                case AbilityEffectType.OrbitingBlade:
                case AbilityEffectType.OrbitalNeutral:
                case AbilityEffectType.OrbitalFire:
                case AbilityEffectType.OrbitalPoison:
                case AbilityEffectType.OrbitalLightning:
                case AbilityEffectType.OrbitalIce:
                case AbilityEffectType.OrbitalWeb:
                    runtimeStats.AddOrbitalCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.DroneBasic:
                case AbilityEffectType.DroneBomb:
                case AbilityEffectType.DroneBeam:
                case AbilityEffectType.DronePoison:
                case AbilityEffectType.DroneExtra:
                    runtimeStats.AddDroneCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.DroneBoost:
                    runtimeStats.AddStrikeProcChance(ability.Value);
                    break;
                case AbilityEffectType.BladeStrikePeriodic:
                case AbilityEffectType.BladeStrikeOnKill:
                case AbilityEffectType.BladeStrikeOnAttack:
                case AbilityEffectType.BladeStormWaveStart:
                    runtimeStats.AddStrikeProcChance(ResolveProcChanceOrValue(ability));
                    break;
                case AbilityEffectType.BladeStrikeCountUp:
                    runtimeStats.AddBladeStrikeCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
                    break;
                case AbilityEffectType.MeteorOnAttack:
                case AbilityEffectType.MeteorOnKill:
                case AbilityEffectType.MeteorFire:
                case AbilityEffectType.MeteorIce:
                case AbilityEffectType.MeteorPoison:
                case AbilityEffectType.MeteorLightning:
                case AbilityEffectType.MeteorChanceUp:
                    runtimeStats.AddMeteorProcChance(ResolveProcChanceOrValue(ability));
                    break;
                case AbilityEffectType.ShieldOnRoomStart:
                case AbilityEffectType.ShieldPerRoom:
                case AbilityEffectType.BossRoomShield:
                    runtimeStats.EnableShieldPerRoom();
                    break;
                case AbilityEffectType.ReviveToken:
                case AbilityEffectType.ReviveOnce:
                    runtimeStats.EnableReviveOnce();
                    break;
                case AbilityEffectType.InvulnerabilityAfterHit:
                    runtimeStats.EnableInvulnerabilityAfterHit(ResolveDurationOrValue(ability));
                    break;
                case AbilityEffectType.HealOnKill:
                    runtimeStats.AddHealOnKill(ability.Value);
                    break;
                case AbilityEffectType.HeartMaxHealthChance:
                    ApplyMaxHealthUp(ability.Value);
                    break;
                case AbilityEffectType.PreBossHeal:
                    playerHealth?.Heal(ability.Value);
                    break;
                case AbilityEffectType.BossDamageUp:
                    runtimeStats.AddBossDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.LowHealthDamageUp:
                    runtimeStats.AddLowHealthDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.LowHealthAttackSpeedUp:
                    runtimeStats.AddLowHealthAttackSpeed(ability.Value);
                    break;
                case AbilityEffectType.LowHealthMoveSpeedUp:
                    runtimeStats.AddLowHealthMoveSpeedMultiplier(ability.Value);
                    break;
                case AbilityEffectType.RewardLuckUp:
                    runtimeStats.AddRewardLuckMultiplier(ability.Value);
                    break;
                case AbilityEffectType.CoinBonus:
                case AbilityEffectType.CoinDropUp:
                    runtimeStats.AddCoinDropMultiplier(ability.Value);
                    break;
                case AbilityEffectType.PotionDropUp:
                case AbilityEffectType.MorePickups:
                    runtimeStats.AddPotionDropMultiplier(ability.Value);
                    break;
                default:
                    LogUnsupported(ability);
                    return;
            }

            if (logAppliedEffects)
            {
                Debug.Log($"{nameof(PlayerAbilityEffectApplier)} applied {ability.AbilityId} ({ability.EffectType}).", this);
            }
        }

        private void ApplyMaxHealthUp(float value)
        {
            var gainedHealth = Mathf.Max(0f, value);
            runtimeStats.AddMaxHealthBonus(gainedHealth);

            if (playerHealth != null)
            {
                playerHealth.RefreshFromRuntimeStats(gainedHealth);
            }
        }

        private void ResolveReferences()
        {
            if (runtimeStats == null)
            {
                runtimeStats = GetComponent<PlayerRuntimeStats>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }
        }

        private void LogUnsupported(AbilityDefinition ability)
        {
            if (logUnsupportedEffects)
            {
                Debug.Log($"{nameof(PlayerAbilityEffectApplier)} does not yet support {ability.EffectType} from {ability.AbilityId}.", this);
            }
        }

        private static float ResolveProcChanceOrValue(AbilityDefinition ability)
        {
            return ability.ProcChance > 0f ? ability.ProcChance : ability.Value;
        }

        private static float ResolveDurationOrValue(AbilityDefinition ability)
        {
            return ability.Duration > 0f ? ability.Duration : ability.Value;
        }
    }
}
