using System;
using UnityEngine;

namespace TapKnockout.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRuntimeStats : MonoBehaviour
    {
        private const float MinCooldownMultiplier = 0.15f;

        private float attackDamageBonus;
        private float attackCooldownReduction;
        private float dashCooldownReduction;
        private float dashDamageBonus;
        private float maxHealthBonus;
        private int extraProjectileCount;

        public event Action<PlayerRuntimeStats> OnStatsChanged;

        public float AttackDamageMultiplier => Mathf.Max(0f, 1f + attackDamageBonus);
        public float AttackCooldownMultiplier => Mathf.Clamp(1f - attackCooldownReduction, MinCooldownMultiplier, 10f);
        public float DashCooldownMultiplier => Mathf.Clamp(1f - dashCooldownReduction, MinCooldownMultiplier, 10f);
        public float DashDamageMultiplier => Mathf.Max(0f, 1f + dashDamageBonus);
        public float MaxHealthBonus => Mathf.Max(0f, maxHealthBonus);
        public int ExtraProjectileCount => Mathf.Max(0, extraProjectileCount);

        public void ResetRunModifiers()
        {
            attackDamageBonus = 0f;
            attackCooldownReduction = 0f;
            dashCooldownReduction = 0f;
            dashDamageBonus = 0f;
            maxHealthBonus = 0f;
            extraProjectileCount = 0;
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

        public void AddMaxHealthBonus(float value)
        {
            maxHealthBonus = Mathf.Max(0f, maxHealthBonus + Mathf.Max(0f, value));
            RaiseStatsChanged();
        }

        public void AddExtraProjectileCount(int value)
        {
            extraProjectileCount = Mathf.Max(0, extraProjectileCount + Mathf.Max(0, value));
            RaiseStatsChanged();
        }

        private void RaiseStatsChanged()
        {
            OnStatsChanged?.Invoke(this);
        }
    }
}
