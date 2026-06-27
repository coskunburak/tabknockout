using TapKnockout.Ability;
using TapKnockout.Combat;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Feedback
{
    public static class AbilityVFXEventResolver
    {
        public static VFXEventType ResolveSelectionEvent(AbilityDefinition ability)
        {
            return ability != null
                ? ResolveSelectionEvent(ability.EffectType)
                : VFXEventType.AbilityGenericUpgrade;
        }

        public static VFXEventType ResolveSelectionEvent(AbilityEffectType effectType)
        {
            switch (effectType)
            {
                case AbilityEffectType.AttackDamageUp:
                case AbilityEffectType.CriticalChanceUp:
                case AbilityEffectType.CritDamageUp:
                case AbilityEffectType.LongRangeDamageUp:
                case AbilityEffectType.ChargedShot:
                    return VFXEventType.AbilityAttackBuff;

                case AbilityEffectType.AttackSpeedUp:
                case AbilityEffectType.LowHealthAttackSpeedUp:
                    return VFXEventType.AbilityAttackSpeedBuff;

                case AbilityEffectType.MaxHealthUp:
                case AbilityEffectType.HeartMaxHealthChance:
                case AbilityEffectType.PreBossHeal:
                    return VFXEventType.AbilityHealthBuff;

                case AbilityEffectType.MoveSpeedUp:
                case AbilityEffectType.LowHealthMoveSpeedUp:
                    return VFXEventType.AbilityMoveSpeedBuff;

                case AbilityEffectType.DamageReductionUp:
                case AbilityEffectType.DodgeChanceUp:
                    return VFXEventType.AbilityDefenseBuff;

                case AbilityEffectType.DashCooldownDown:
                case AbilityEffectType.DashDamageUp:
                case AbilityEffectType.DashKnockbackUp:
                case AbilityEffectType.DashFireTrail:
                case AbilityEffectType.DashChainLightning:
                case AbilityEffectType.DashCooldownRefundOnKill:
                case AbilityEffectType.DashDamageLowHealth:
                    return VFXEventType.AbilityDashBuff;

                case AbilityEffectType.DashShockwave:
                case AbilityEffectType.DashBeam:
                    return VFXEventType.AbilityDashShockwave;

                case AbilityEffectType.DashIFrameDurationUp:
                    return VFXEventType.AbilityDashPhase;

                case AbilityEffectType.DashStun:
                    return VFXEventType.AbilityDashStagger;

                case AbilityEffectType.ProjectileSpeedUp:
                case AbilityEffectType.ProjectileHoming:
                    return VFXEventType.AbilityProjectileBuff;

                case AbilityEffectType.ExtraProjectile:
                case AbilityEffectType.FrontProjectile:
                case AbilityEffectType.DiagonalProjectiles:
                case AbilityEffectType.SideShot:
                case AbilityEffectType.SideProjectiles:
                case AbilityEffectType.RearProjectile:
                    return VFXEventType.AbilityProjectileSplit;

                case AbilityEffectType.Pierce:
                case AbilityEffectType.ProjectilePierce:
                    return VFXEventType.AbilityProjectilePierce;

                case AbilityEffectType.Ricochet:
                case AbilityEffectType.ProjectileRicochet:
                case AbilityEffectType.ProjectileWallBounce:
                    return VFXEventType.AbilityProjectileRicochet;

                case AbilityEffectType.ProjectileSizeUp:
                    return VFXEventType.AbilityProjectileSize;

                case AbilityEffectType.BurningHits:
                case AbilityEffectType.BurnOnHit:
                case AbilityEffectType.SuperBurn:
                case AbilityEffectType.MeteorFire:
                    return VFXEventType.AbilityFireProc;

                case AbilityEffectType.PoisonOnHit:
                case AbilityEffectType.SuperPoison:
                case AbilityEffectType.MeteorPoison:
                    return VFXEventType.AbilityPoisonProc;

                case AbilityEffectType.FreezeOnHit:
                case AbilityEffectType.SuperFreeze:
                case AbilityEffectType.MeteorIce:
                    return VFXEventType.AbilityIceProc;

                case AbilityEffectType.ChainLightning:
                case AbilityEffectType.LightningOnHit:
                case AbilityEffectType.SuperLightning:
                case AbilityEffectType.MeteorLightning:
                    return VFXEventType.AbilityLightningProc;

                case AbilityEffectType.ShieldOnRoomStart:
                case AbilityEffectType.ShieldPerRoom:
                case AbilityEffectType.BossRoomShield:
                case AbilityEffectType.DashShieldAfterHit:
                    return VFXEventType.AbilityShield;

                case AbilityEffectType.HealOnRoomClear:
                case AbilityEffectType.HealOnKill:
                    return VFXEventType.AbilitySoulHeal;

                case AbilityEffectType.BossDamageUp:
                    return VFXEventType.AbilityBossBreaker;

                case AbilityEffectType.LowHealthDamageUp:
                    return VFXEventType.AbilityLowHealthSurge;

                case AbilityEffectType.CoinBonus:
                case AbilityEffectType.RewardLuckUp:
                case AbilityEffectType.CoinDropUp:
                    return VFXEventType.AbilityRewardLuck;

                case AbilityEffectType.PickupMagnet:
                case AbilityEffectType.PickupFrenzyPotion:
                case AbilityEffectType.PickupMeteorPotion:
                case AbilityEffectType.MorePickups:
                case AbilityEffectType.PotionDropUp:
                    return VFXEventType.AbilityPickupFrenzy;

                case AbilityEffectType.OrbitingBlade:
                case AbilityEffectType.OrbitalNeutral:
                case AbilityEffectType.OrbitalFire:
                case AbilityEffectType.OrbitalPoison:
                case AbilityEffectType.OrbitalLightning:
                case AbilityEffectType.OrbitalIce:
                case AbilityEffectType.OrbitalWeb:
                    return VFXEventType.AbilityOrbital;

                case AbilityEffectType.DroneBasic:
                case AbilityEffectType.DroneBomb:
                case AbilityEffectType.DroneBeam:
                case AbilityEffectType.DronePoison:
                case AbilityEffectType.DroneBoost:
                case AbilityEffectType.DroneExtra:
                    return VFXEventType.AbilityDrone;

                case AbilityEffectType.BladeStrikePeriodic:
                case AbilityEffectType.BladeStrikeOnKill:
                case AbilityEffectType.BladeStrikeOnAttack:
                case AbilityEffectType.BladeStormWaveStart:
                case AbilityEffectType.BladeStrikeCountUp:
                case AbilityEffectType.BladeStrikeDamageUp:
                    return VFXEventType.AbilityBladeStrike;

                case AbilityEffectType.MeteorOnAttack:
                case AbilityEffectType.MeteorOnKill:
                case AbilityEffectType.MeteorChanceUp:
                    return VFXEventType.AbilityMeteor;

                case AbilityEffectType.EnergyBeam:
                    return VFXEventType.AbilityEnergyBeam;

                case AbilityEffectType.EnergyRing:
                    return VFXEventType.AbilityEnergyRing;

                case AbilityEffectType.ReviveToken:
                case AbilityEffectType.ReviveOnce:
                    return VFXEventType.AbilityRevive;

                case AbilityEffectType.InvulnerabilityAfterHit:
                case AbilityEffectType.PickupInvincibilityPotion:
                    return VFXEventType.AbilityInvulnerability;

                default:
                    return VFXEventType.AbilityGenericUpgrade;
            }
        }

        public static bool TryResolveDamageTypeEvent(DamageType damageType, out VFXEventType eventType)
        {
            switch (damageType)
            {
                case DamageType.Fire:
                    eventType = VFXEventType.AbilityFireProc;
                    return true;
                case DamageType.Poison:
                    eventType = VFXEventType.AbilityPoisonProc;
                    return true;
                case DamageType.Ice:
                    eventType = VFXEventType.AbilityIceProc;
                    return true;
                case DamageType.Lightning:
                    eventType = VFXEventType.AbilityLightningProc;
                    return true;
                default:
                    eventType = VFXEventType.GenericBurst;
                    return false;
            }
        }

        public static Color ResolveColor(AbilityEffectType effectType)
        {
            return ResolveSelectionEvent(effectType) switch
            {
                VFXEventType.AbilityFireProc => new Color(1f, 0.36f, 0.08f, 1f),
                VFXEventType.AbilityPoisonProc => new Color(0.25f, 1f, 0.32f, 1f),
                VFXEventType.AbilityIceProc => new Color(0.42f, 0.85f, 1f, 1f),
                VFXEventType.AbilityLightningProc => new Color(0.28f, 0.78f, 1f, 1f),
                VFXEventType.AbilityDashShockwave => new Color(0.16f, 0.72f, 1f, 1f),
                VFXEventType.AbilityDashPhase => new Color(0.68f, 0.55f, 1f, 1f),
                VFXEventType.AbilityDashStagger => new Color(1f, 0.93f, 0.2f, 1f),
                VFXEventType.AbilityShield => new Color(0.5f, 0.95f, 0.75f, 1f),
                VFXEventType.AbilitySoulHeal => new Color(0.35f, 1f, 0.55f, 1f),
                VFXEventType.AbilityLowHealthSurge => new Color(1f, 0.2f, 0.36f, 1f),
                VFXEventType.AbilityBossBreaker => new Color(1f, 0.62f, 0.18f, 1f),
                _ => new Color(0.62f, 0.85f, 1f, 1f)
            };
        }
    }
}
