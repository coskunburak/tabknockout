using System;
using TapKnockout.Ability;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [Serializable]
    public sealed class ActiveSkillSlot
    {
        [SerializeField] private AbilityDefinition ability;
        [SerializeField] private ActiveSkillEffectType fallbackEffectType = ActiveSkillEffectType.ForwardCleave;
        [SerializeField] private string hotkeyLabel = "Q";
        [SerializeField, Min(0f)] private float fallbackDamage = 25f;
        [SerializeField, Min(0f)] private float fallbackCooldown = 4f;
        [SerializeField, Min(0f)] private float fallbackCastTime;
        [SerializeField, Min(0.1f)] private float fallbackRange = 5f;
        [SerializeField, Min(0.1f)] private float fallbackRadius = 2.5f;
        [SerializeField, Range(1f, 180f)] private float fallbackConeAngle = 75f;
        [SerializeField, Min(0f)] private float fallbackKnockbackForce = 4f;
        [SerializeField, Min(0f)] private float fallbackKnockbackDuration = 0.15f;
        [SerializeField] private bool lockMovementDuringCast;
        [SerializeField, Min(0f)] private float effectDelay;
        [SerializeField] private ActiveSkillAimMode aimMode = ActiveSkillAimMode.MouseAim;
        [SerializeField] private ActiveSkillTargetMode targetMode = ActiveSkillTargetMode.DirectionalArea;
        [SerializeField] private ActiveSkillOriginMode originMode = ActiveSkillOriginMode.Player;
        [SerializeField] private ActiveSkillFeedbackConfig feedback = new ActiveSkillFeedbackConfig();

        private float cooldownRemaining;
        private bool isCasting;

        public ActiveSkillSlot()
        {
        }

        public ActiveSkillSlot(string hotkey, ActiveSkillEffectType effectType)
        {
            hotkeyLabel = hotkey;
            fallbackEffectType = effectType;
            ApplyDefaultPoliciesForEffect(effectType);
        }
        public AbilityDefinition Ability => ability;
        public ActiveSkillEffectType FallbackEffectType => fallbackEffectType;
        public string HotkeyLabel => hotkeyLabel ?? string.Empty;
        public float CooldownRemaining => cooldownRemaining;
        public bool IsCasting => isCasting;
        public bool IsReady => cooldownRemaining <= 0f && !isCasting;
        public float CooldownDuration => ResolveCooldown();
        public float NormalizedCooldown => CooldownDuration > 0f ? Mathf.Clamp01(cooldownRemaining / CooldownDuration) : 0f;
        public bool LockMovementDuringCast => lockMovementDuringCast;
        public float EffectDelay => effectDelay;
        public ActiveSkillAimMode AimMode => aimMode;
        public ActiveSkillTargetMode TargetMode => targetMode;
        public ActiveSkillOriginMode OriginMode => originMode;
        public ActiveSkillFeedbackConfig Feedback => feedback;

        public void SetFallbackDefaults(string hotkey, ActiveSkillEffectType effectType)
        {
            if (string.IsNullOrWhiteSpace(hotkeyLabel))
            {
                hotkeyLabel = hotkey;
            }

            if (fallbackEffectType == ActiveSkillEffectType.None)
            {
                fallbackEffectType = effectType;
            }
        }

        public void SetAbility(AbilityDefinition definition)
        {
            ability = definition;
            cooldownRemaining = 0f;
            isCasting = false;
        }

        public void Tick(float deltaTime)
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Mathf.Max(0f, deltaTime));
            }
        }

        public void BeginCast()
        {
            isCasting = true;
        }

        public void CompleteCast()
        {
            isCasting = false;
            cooldownRemaining = ResolveCooldown();
        }

        public void CancelCast()
        {
            isCasting = false;
        }

        public ActiveSkillEffectType ResolveEffectType()
        {
            if (ability == null)
            {
                return fallbackEffectType;
            }

            switch (ability.EffectType)
            {
                case AbilityEffectType.EnergyRing:
                case AbilityEffectType.DashShockwave:
                    return ActiveSkillEffectType.GroundImpact;
                case AbilityEffectType.EnergyBeam:
                case AbilityEffectType.ChargedShot:
                case AbilityEffectType.DashBeam:
                    return ActiveSkillEffectType.ForwardCleave;
                default:
                    var abilityId = ability.AbilityId ?? string.Empty;
                    if (abilityId.Contains("slam") || abilityId.Contains("impact") || abilityId.Contains("ring"))
                    {
                        return ActiveSkillEffectType.GroundImpact;
                    }

                    if (abilityId.Contains("arc") || abilityId.Contains("cleave") || abilityId.Contains("blast"))
                    {
                        return ActiveSkillEffectType.ForwardCleave;
                    }

                    return fallbackEffectType;
            }
        }

        public float ResolveDamage()
        {
            return ability != null && ability.Value > 0f ? ability.Value : fallbackDamage;
        }

        public float ResolveCooldown()
        {
            return ability != null && ability.Cooldown > 0f ? ability.Cooldown : fallbackCooldown;
        }

        public float ResolveCastTime()
        {
            return ability != null && ability.Duration > 0f ? Mathf.Min(ability.Duration, 1f) : fallbackCastTime;
        }

        public float ResolveRange()
        {
            return ability != null && ability.SecondaryValue > 0f ? ability.SecondaryValue : fallbackRange;
        }

        public float ResolveRadius()
        {
            return ability != null && ability.SecondaryValue > 0f ? ability.SecondaryValue : fallbackRadius;
        }

        public float ResolveConeAngle()
        {
            return fallbackConeAngle;
        }

        public float ResolveKnockbackForce()
        {
            return fallbackKnockbackForce;
        }

        public float ResolveKnockbackDuration()
        {
            return fallbackKnockbackDuration;
        }

        public float ResolveEffectDelay()
        {
            return Mathf.Max(0f, effectDelay);
        }

        public void EnsureFeedbackConfig()
        {
            feedback ??= new ActiveSkillFeedbackConfig();
        }
        private void ApplyDefaultPoliciesForEffect(ActiveSkillEffectType effectType)
        {
            switch (effectType)
            {
                case ActiveSkillEffectType.GroundImpact:
                    aimMode = ActiveSkillAimMode.MouseAim;
                    targetMode = ActiveSkillTargetMode.SelfArea;
                    originMode = ActiveSkillOriginMode.Player;
                    break;

                case ActiveSkillEffectType.ForwardCleave:
                    aimMode = ActiveSkillAimMode.MouseAim;
                    targetMode = ActiveSkillTargetMode.DirectionalArea;
                    originMode = ActiveSkillOriginMode.Player;
                    break;
            }
        }
    }
}
