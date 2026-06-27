using System;
using UnityEngine;

namespace TapKnockout.Combat
{
    [Serializable]
    public readonly struct StatusEffectRequest
    {
        public StatusEffectRequest(
            StatusEffectType effectType,
            GameObject source,
            float duration,
            float tickDamage = 0f,
            float tickInterval = 1f,
            float slowMultiplier = 1f)
        {
            EffectType = effectType;
            Source = source;
            Duration = Mathf.Max(0f, duration);
            TickDamage = Mathf.Max(0f, tickDamage);
            TickInterval = Mathf.Max(0.05f, tickInterval);
            SlowMultiplier = Mathf.Clamp(slowMultiplier, 0f, 1f);
        }

        public StatusEffectType EffectType { get; }
        public GameObject Source { get; }
        public float Duration { get; }
        public float TickDamage { get; }
        public float TickInterval { get; }
        public float SlowMultiplier { get; }
        public bool IsValid => EffectType != StatusEffectType.None && Duration > 0f;
    }
}
