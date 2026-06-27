using System;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRuntimeStats : MonoBehaviour
    {
        private const float MinCooldownMultiplier = 0.15f;
        private const float MaxDamageReduction = 0.8f;
        private const float MaxProcChance = 1f;
        private const int MaxProjectileCountBonus = 6;
        private const int MaxPierceCount = 5;
        private const int MaxRicochetCount = 5;
        private const int MaxWallBounceCount = 3;
        private const int MaxOrbitalCount = 6;
        private const int MaxDroneCount = 4;

        private float attackDamageBonus;
        private float attackCooldownReduction;
        private float dashCooldownReduction;
        private float dashDamageBonus;
        private float dashKnockbackBonus;
        private float maxHealthBonus;
        private float moveSpeedBonus;
        private float projectileSpeedBonus;
        private float damageReductionBonus;
        private float critChanceBonus;
        private float critDamageBonus;
        private float dashIFrameBonus;
        private float dashStunDuration;
        private float dashShockwaveRadius;
        private float dashCooldownRefundOnKill;
        private float dashLowHealthDamageBonus;
        private float projectileHomingStrength;
        private float projectileSizeBonus;
        private float longRangeDamageBonus;
        private float burnOnHitChance;
        private float poisonOnHitChance;
        private float lightningOnHitChance;
        private float freezeOnHitChance;
        private float strikeProcChance;
        private float meteorProcChance;
        private float healOnKillAmount;
        private float bossDamageBonus;
        private float lowHealthDamageBonus;
        private float lowHealthAttackSpeedBonus;
        private float lowHealthMoveSpeedBonus;
        private float rewardLuckBonus;
        private float coinDropBonus;
        private float potionDropBonus;
        private int extraProjectileCount;
        private int frontProjectileCount;
        private int diagonalProjectileCount;
        private int sideProjectileCount;
        private int rearProjectileCount;
        private int projectilePierceCount;
        private int projectileRicochetCount;
        private int projectileWallBounceCount;
        private int orbitalCount;
        private int droneCount;
        private int bladeStrikeCountBonus;
        private bool dashShieldAfterHit;
        private bool shieldPerRoom;
        private bool reviveOnce;
        private bool invulnerabilityAfterHit;

        public event Action<PlayerRuntimeStats> OnStatsChanged;

        public float AttackDamageMultiplier => Mathf.Max(0f, 1f + attackDamageBonus);
        public float AttackCooldownMultiplier => Mathf.Clamp(1f - attackCooldownReduction, MinCooldownMultiplier, 10f);
        public float DashCooldownMultiplier => Mathf.Clamp(1f - dashCooldownReduction, MinCooldownMultiplier, 10f);
        public float DashDamageMultiplier => Mathf.Max(0f, 1f + dashDamageBonus);
        public float DashKnockbackMultiplier => Mathf.Max(0f, 1f + dashKnockbackBonus);
        public float MaxHealthBonus => Mathf.Max(0f, maxHealthBonus);
        public float MoveSpeedMultiplier => Mathf.Max(0f, 1f + moveSpeedBonus);
        public float ProjectileSpeedMultiplier => Mathf.Max(0f, 1f + projectileSpeedBonus);
        public float DamageReductionMultiplier => Mathf.Clamp(1f - damageReductionBonus, 1f - MaxDamageReduction, 1f);
        public float CritChanceBonus => Mathf.Clamp01(critChanceBonus);
        public float CritDamageMultiplier => Mathf.Max(1f, 1f + critDamageBonus);
        public float DashIFrameBonus => Mathf.Max(0f, dashIFrameBonus);
        public float DashStunDuration => Mathf.Max(0f, dashStunDuration);
        public float DashShockwaveRadius => Mathf.Max(0f, dashShockwaveRadius);
        public bool DashShieldAfterHit => dashShieldAfterHit;
        public float DashCooldownRefundOnKill => Mathf.Clamp01(dashCooldownRefundOnKill);
        public float DashLowHealthDamageMultiplier => Mathf.Max(1f, 1f + dashLowHealthDamageBonus);
        public int ExtraProjectileCount => ClampCount(extraProjectileCount, MaxProjectileCountBonus);
        public int FrontProjectileCount => ClampCount(frontProjectileCount, MaxProjectileCountBonus);
        public int DiagonalProjectileCount => ClampCount(diagonalProjectileCount, MaxProjectileCountBonus);
        public int SideProjectileCount => ClampCount(sideProjectileCount, MaxProjectileCountBonus);
        public int RearProjectileCount => ClampCount(rearProjectileCount, MaxProjectileCountBonus);
        public int ProjectilePierceCount => ClampCount(projectilePierceCount, MaxPierceCount);
        public int ProjectileRicochetCount => ClampCount(projectileRicochetCount, MaxRicochetCount);
        public int ProjectileWallBounceCount => ClampCount(projectileWallBounceCount, MaxWallBounceCount);
        public float ProjectileHomingStrength => Mathf.Max(0f, projectileHomingStrength);
        public float ProjectileSizeMultiplier => Mathf.Max(0.1f, 1f + projectileSizeBonus);
        public float LongRangeDamageMultiplier => Mathf.Max(1f, 1f + longRangeDamageBonus);
        public float BurnOnHitChance => ClampProcChance(burnOnHitChance);
        public float PoisonOnHitChance => ClampProcChance(poisonOnHitChance);
        public float LightningOnHitChance => ClampProcChance(lightningOnHitChance);
        public float FreezeOnHitChance => ClampProcChance(freezeOnHitChance);
        public int OrbitalCount => ClampCount(orbitalCount, MaxOrbitalCount);
        public int DroneCount => ClampCount(droneCount, MaxDroneCount);
        public int BladeStrikeCountBonus => Mathf.Max(0, bladeStrikeCountBonus);
        public float StrikeProcChance => ClampProcChance(strikeProcChance);
        public float MeteorProcChance => ClampProcChance(meteorProcChance);
        public bool ShieldPerRoom => shieldPerRoom;
        public bool ReviveOnce => reviveOnce;
        public bool InvulnerabilityAfterHit => invulnerabilityAfterHit;
        public float HealOnKillAmount => Mathf.Max(0f, healOnKillAmount);
        public float BossDamageMultiplier => Mathf.Max(1f, 1f + bossDamageBonus);
        public float LowHealthDamageMultiplier => Mathf.Max(1f, 1f + lowHealthDamageBonus);
        public float LowHealthAttackSpeedBonus => Mathf.Max(0f, lowHealthAttackSpeedBonus);
        public float LowHealthMoveSpeedMultiplier => Mathf.Max(1f, 1f + lowHealthMoveSpeedBonus);
        public float RewardLuckMultiplier => Mathf.Max(1f, 1f + rewardLuckBonus);
        public float CoinDropMultiplier => Mathf.Max(1f, 1f + coinDropBonus);
        public float PotionDropMultiplier => Mathf.Max(1f, 1f + potionDropBonus);

        public void ResetRunModifiers()
        {
            attackDamageBonus = 0f;
            attackCooldownReduction = 0f;
            dashCooldownReduction = 0f;
            dashDamageBonus = 0f;
            dashKnockbackBonus = 0f;
            maxHealthBonus = 0f;
            moveSpeedBonus = 0f;
            projectileSpeedBonus = 0f;
            damageReductionBonus = 0f;
            critChanceBonus = 0f;
            critDamageBonus = 0f;
            dashIFrameBonus = 0f;
            dashStunDuration = 0f;
            dashShockwaveRadius = 0f;
            dashCooldownRefundOnKill = 0f;
            dashLowHealthDamageBonus = 0f;
            projectileHomingStrength = 0f;
            projectileSizeBonus = 0f;
            longRangeDamageBonus = 0f;
            burnOnHitChance = 0f;
            poisonOnHitChance = 0f;
            lightningOnHitChance = 0f;
            freezeOnHitChance = 0f;
            strikeProcChance = 0f;
            meteorProcChance = 0f;
            healOnKillAmount = 0f;
            bossDamageBonus = 0f;
            lowHealthDamageBonus = 0f;
            lowHealthAttackSpeedBonus = 0f;
            lowHealthMoveSpeedBonus = 0f;
            rewardLuckBonus = 0f;
            coinDropBonus = 0f;
            potionDropBonus = 0f;
            extraProjectileCount = 0;
            frontProjectileCount = 0;
            diagonalProjectileCount = 0;
            sideProjectileCount = 0;
            rearProjectileCount = 0;
            projectilePierceCount = 0;
            projectileRicochetCount = 0;
            projectileWallBounceCount = 0;
            orbitalCount = 0;
            droneCount = 0;
            bladeStrikeCountBonus = 0;
            dashShieldAfterHit = false;
            shieldPerRoom = false;
            reviveOnce = false;
            invulnerabilityAfterHit = false;
            RaiseStatsChanged();
        }

        public void AddAttackDamageMultiplier(float value)
        {
            attackDamageBonus = Mathf.Max(0f, attackDamageBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddAttackCooldownReduction(float value)
        {
            attackCooldownReduction = Mathf.Max(0f, attackCooldownReduction + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddDashCooldownReduction(float value)
        {
            dashCooldownReduction = Mathf.Max(0f, dashCooldownReduction + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddDashDamageMultiplier(float value)
        {
            dashDamageBonus = Mathf.Max(0f, dashDamageBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddDashKnockbackMultiplier(float value)
        {
            dashKnockbackBonus = Mathf.Max(0f, dashKnockbackBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddMaxHealthBonus(float value)
        {
            maxHealthBonus = Mathf.Max(0f, maxHealthBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddMoveSpeedMultiplier(float value)
        {
            moveSpeedBonus = Mathf.Max(0f, moveSpeedBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddProjectileSpeedMultiplier(float value)
        {
            projectileSpeedBonus = Mathf.Max(0f, projectileSpeedBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddExtraProjectileCount(int value)
        {
            extraProjectileCount = AddClampedCount(extraProjectileCount, value, MaxProjectileCountBonus);
            RaiseStatsChanged();
        }

        public void AddDamageReduction(float value)
        {
            damageReductionBonus = Mathf.Clamp(damageReductionBonus + Mathf.Max(0f, value), 0f, MaxDamageReduction);
            RaiseStatsChanged();
        }

        public void AddCritChance(float value)
        {
            critChanceBonus = ClampProcChance(critChanceBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddCritDamageMultiplier(float value)
        {
            critDamageBonus = Mathf.Max(0f, critDamageBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddDashIFrameBonus(float value)
        {
            dashIFrameBonus = Mathf.Max(0f, dashIFrameBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddDashStunDuration(float value)
        {
            dashStunDuration = Mathf.Max(dashStunDuration, Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddDashShockwaveRadius(float value)
        {
            dashShockwaveRadius = Mathf.Max(dashShockwaveRadius, Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void EnableDashShieldAfterHit()
        {
            dashShieldAfterHit = true;
            RaiseStatsChanged();
        }

        public void AddDashCooldownRefundOnKill(float value)
        {
            dashCooldownRefundOnKill = ClampProcChance(dashCooldownRefundOnKill + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddDashLowHealthDamageMultiplier(float value)
        {
            dashLowHealthDamageBonus = Mathf.Max(0f, dashLowHealthDamageBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddFrontProjectileCount(int value)
        {
            frontProjectileCount = AddClampedCount(frontProjectileCount, value, MaxProjectileCountBonus);
            RaiseStatsChanged();
        }

        public void AddDiagonalProjectileCount(int value)
        {
            diagonalProjectileCount = AddClampedCount(diagonalProjectileCount, value, MaxProjectileCountBonus);
            RaiseStatsChanged();
        }

        public void AddSideProjectileCount(int value)
        {
            sideProjectileCount = AddClampedCount(sideProjectileCount, value, MaxProjectileCountBonus);
            RaiseStatsChanged();
        }

        public void AddRearProjectileCount(int value)
        {
            rearProjectileCount = AddClampedCount(rearProjectileCount, value, MaxProjectileCountBonus);
            RaiseStatsChanged();
        }

        public void AddProjectilePierceCount(int value)
        {
            projectilePierceCount = AddClampedCount(projectilePierceCount, value, MaxPierceCount);
            RaiseStatsChanged();
        }

        public void AddProjectileRicochetCount(int value)
        {
            projectileRicochetCount = AddClampedCount(projectileRicochetCount, value, MaxRicochetCount);
            RaiseStatsChanged();
        }

        public void AddProjectileWallBounceCount(int value)
        {
            projectileWallBounceCount = AddClampedCount(projectileWallBounceCount, value, MaxWallBounceCount);
            RaiseStatsChanged();
        }

        public void AddProjectileHomingStrength(float value)
        {
            projectileHomingStrength = Mathf.Max(0f, projectileHomingStrength + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddProjectileSizeMultiplier(float value)
        {
            projectileSizeBonus = Mathf.Max(0f, projectileSizeBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddLongRangeDamageMultiplier(float value)
        {
            longRangeDamageBonus = Mathf.Max(0f, longRangeDamageBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddBurnOnHit(float procChanceBonus)
        {
            burnOnHitChance = ClampProcChance(burnOnHitChance + Mathf.Max(0f, procChanceBonus));
            RaiseStatsChanged();
        }

        public void AddPoisonOnHit(float procChanceBonus)
        {
            poisonOnHitChance = ClampProcChance(poisonOnHitChance + Mathf.Max(0f, procChanceBonus));
            RaiseStatsChanged();
        }

        public void AddLightningOnHit(float procChanceBonus)
        {
            lightningOnHitChance = ClampProcChance(lightningOnHitChance + Mathf.Max(0f, procChanceBonus));
            RaiseStatsChanged();
        }

        public void AddFreezeOnHit(float procChanceBonus)
        {
            freezeOnHitChance = ClampProcChance(freezeOnHitChance + Mathf.Max(0f, procChanceBonus));
            RaiseStatsChanged();
        }

        public void AddOrbitalCount(int value)
        {
            orbitalCount = AddClampedCount(orbitalCount, value, MaxOrbitalCount);
            RaiseStatsChanged();
        }

        public void AddDroneCount(int value)
        {
            droneCount = AddClampedCount(droneCount, value, MaxDroneCount);
            RaiseStatsChanged();
        }

        public void AddBladeStrikeCount(int value)
        {
            bladeStrikeCountBonus = Mathf.Max(0, bladeStrikeCountBonus + Mathf.Max(0, value));
            RaiseStatsChanged();
        }

        public void AddStrikeProcChance(float value)
        {
            strikeProcChance = ClampProcChance(strikeProcChance + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddMeteorProcChance(float value)
        {
            meteorProcChance = ClampProcChance(meteorProcChance + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void EnableShieldPerRoom()
        {
            shieldPerRoom = true;
            RaiseStatsChanged();
        }

        public void EnableReviveOnce()
        {
            reviveOnce = true;
            RaiseStatsChanged();
        }

        public void EnableInvulnerabilityAfterHit()
        {
            invulnerabilityAfterHit = true;
            RaiseStatsChanged();
        }

        public void AddHealOnKill(float value)
        {
            healOnKillAmount = Mathf.Max(0f, healOnKillAmount + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddBossDamageMultiplier(float value)
        {
            bossDamageBonus = Mathf.Max(0f, bossDamageBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddLowHealthDamageMultiplier(float value)
        {
            lowHealthDamageBonus = Mathf.Max(0f, lowHealthDamageBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddLowHealthAttackSpeed(float value)
        {
            lowHealthAttackSpeedBonus = Mathf.Max(0f, lowHealthAttackSpeedBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddLowHealthMoveSpeedMultiplier(float value)
        {
            lowHealthMoveSpeedBonus = Mathf.Max(0f, lowHealthMoveSpeedBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddRewardLuckMultiplier(float value)
        {
            rewardLuckBonus = Mathf.Max(0f, rewardLuckBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddCoinDropMultiplier(float value)
        {
            coinDropBonus = Mathf.Max(0f, coinDropBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddPotionDropMultiplier(float value)
        {
            potionDropBonus = Mathf.Max(0f, potionDropBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        private void RaiseStatsChanged()
        {
            OnStatsChanged?.Invoke(this);
        }

        private static float ClampProcChance(float value)
        {
            return Mathf.Clamp(value, 0f, MaxProcChance);
        }

        private static int ClampCount(int value, int maxValue)
        {
            return Mathf.Clamp(value, 0, maxValue);
        }

        private static int AddClampedCount(int currentValue, int addedValue, int maxValue)
        {
            return ClampCount(currentValue + Mathf.Max(0, addedValue), maxValue);
        }
    }
}
