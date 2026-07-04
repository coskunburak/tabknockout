using System;
using TapKnockout.Ability;
using UnityEngine;

namespace TapKnockout.Survivor
{
    [Serializable]
    public sealed class ActiveSkillFeedbackConfig
    {
        [Header("VFX")]
        [SerializeField] private GameObject castVfxPrefab;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField] private GameObject telegraphVfxPrefab;
        [SerializeField, Min(0f)] private float vfxLifetime = 1.5f;

        [Header("SFX")]
        [SerializeField] private AudioClip castSfx;
        [SerializeField] private AudioClip impactSfx;
        [SerializeField] private AudioClip loopSfx;
        [SerializeField, Min(0f)] private float loopSfxDuration;
        [SerializeField, Range(0f, 1f)] private float volumeScale = 1f;

        [Header("Camera")]
        [SerializeField, Min(0f)] private float cameraShakeIntensity;
        [SerializeField, Min(0f)] private float cameraShakeDuration;

        public GameObject ResolveVFXPrefab(AbilityDefinition ability, ActiveSkillFeedbackPhase phase)
        {
            if (ability != null)
            {
                var abilityPrefab = phase switch
                {
                    ActiveSkillFeedbackPhase.Cast => ability.CastVFXPrefab,
                    ActiveSkillFeedbackPhase.Telegraph => ability.TelegraphVFXPrefab,
                    ActiveSkillFeedbackPhase.Impact => ability.ImpactVFXPrefab,
                    _ => null
                };

                if (abilityPrefab != null)
                {
                    return abilityPrefab;
                }
            }

            return phase switch
            {
                ActiveSkillFeedbackPhase.Cast => castVfxPrefab,
                ActiveSkillFeedbackPhase.Telegraph => telegraphVfxPrefab,
                ActiveSkillFeedbackPhase.Impact => impactVfxPrefab,
                _ => null
            };
        }

        public AudioClip ResolveSFX(AbilityDefinition ability, ActiveSkillFeedbackPhase phase)
        {
            if (ability != null)
            {
                var abilityClip = phase switch
                {
                    ActiveSkillFeedbackPhase.Cast => ability.CastSFX,
                    ActiveSkillFeedbackPhase.Impact => ability.ImpactSFX,
                    _ => null
                };

                if (abilityClip != null)
                {
                    return abilityClip;
                }
            }

            return phase switch
            {
                ActiveSkillFeedbackPhase.Cast => castSfx,
                ActiveSkillFeedbackPhase.Impact => impactSfx,
                _ => null
            };
        }

        public AudioClip ResolveLoopSFX(AbilityDefinition ability)
        {
            return ability != null && ability.LoopSFX != null ? ability.LoopSFX : loopSfx;
        }

        public float ResolveVFXLifetime(AbilityDefinition ability)
        {
            return ability != null && ability.VFXLifetime > 0f ? ability.VFXLifetime : vfxLifetime;
        }

        public float ResolveLoopSFXDuration(AbilityDefinition ability)
        {
            return ability != null && ability.LoopSFXDuration > 0f ? ability.LoopSFXDuration : loopSfxDuration;
        }

        public float ResolveCameraShakeIntensity(AbilityDefinition ability)
        {
            return ability != null && ability.CameraShakeIntensity > 0f ? ability.CameraShakeIntensity : cameraShakeIntensity;
        }

        public float ResolveCameraShakeDuration(AbilityDefinition ability)
        {
            return ability != null && ability.CameraShakeDuration > 0f ? ability.CameraShakeDuration : cameraShakeDuration;
        }

        public float ResolveVolumeScale(AbilityDefinition ability)
        {
            return ability != null && ability.SFXVolumeScale > 0f ? ability.SFXVolumeScale : volumeScale;
        }
    }
}
