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
        [SerializeField] private bool logUnsupportedEffects = true;

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
                case AbilityEffectType.DashCooldownDown:
                    runtimeStats.AddDashCooldownReduction(ability.Value);
                    break;
                case AbilityEffectType.DashDamageUp:
                    runtimeStats.AddDashDamageMultiplier(ability.Value);
                    break;
                case AbilityEffectType.MaxHealthUp:
                    ApplyMaxHealthUp(ability.Value);
                    break;
                case AbilityEffectType.ExtraProjectile:
                    runtimeStats.AddExtraProjectileCount(Mathf.Max(1, Mathf.RoundToInt(ability.Value)));
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
    }
}
