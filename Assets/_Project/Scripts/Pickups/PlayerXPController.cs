using System;
using UnityEngine;

namespace TapKnockout.Pickups
{
    public readonly struct PlayerXPChangedEventArgs
    {
        public PlayerXPChangedEventArgs(PlayerXPController source, int level, int currentXP, int xpForNextLevel)
        {
            Source = source;
            Level = level;
            CurrentXP = currentXP;
            XPForNextLevel = xpForNextLevel;
        }

        public PlayerXPController Source { get; }
        public int Level { get; }
        public int CurrentXP { get; }
        public int XPForNextLevel { get; }
        public float NormalizedXP => XPForNextLevel > 0 ? Mathf.Clamp01((float)CurrentXP / XPForNextLevel) : 1f;
    }

    public readonly struct PlayerLevelUpEventArgs
    {
        public PlayerLevelUpEventArgs(PlayerXPController source, int previousLevel, int newLevel)
        {
            Source = source;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
        }

        public PlayerXPController Source { get; }
        public int PreviousLevel { get; }
        public int NewLevel { get; }
    }

    [DisallowMultipleComponent]
    public sealed class PlayerXPController : MonoBehaviour
    {
        [Header("Progression")]
        [SerializeField, Min(1)] private int startingLevel = 1;
        [SerializeField] private int[] xpRequirementsPerLevel = { 5, 8, 12, 18, 25, 35, 48, 64, 85, 110 };
        [SerializeField, Min(1)] private int fallbackXPRequirement = 100;

        [Header("Runtime")]
        [SerializeField] private bool resetOnAwake = true;

        public static event Action<PlayerLevelUpEventArgs> OnAnyLevelUp;
        public event Action<PlayerXPChangedEventArgs> OnXPChanged;
        public event Action<PlayerLevelUpEventArgs> OnLevelUp;

        public int Level { get; private set; }
        public int CurrentXP { get; private set; }
        public int XPForNextLevel => GetXPRequiredForLevel(Level);
        public float NormalizedXP => XPForNextLevel > 0 ? Mathf.Clamp01((float)CurrentXP / XPForNextLevel) : 1f;

        private void Awake()
        {
            if (resetOnAwake)
            {
                ResetProgression();
            }
        }

        private void OnValidate()
        {
            startingLevel = Mathf.Max(1, startingLevel);
            fallbackXPRequirement = Mathf.Max(1, fallbackXPRequirement);

            if (xpRequirementsPerLevel == null)
            {
                xpRequirementsPerLevel = Array.Empty<int>();
                return;
            }

            for (var i = 0; i < xpRequirementsPerLevel.Length; i++)
            {
                xpRequirementsPerLevel[i] = Mathf.Max(1, xpRequirementsPerLevel[i]);
            }
        }

        public void SetXPCurve(int[] requirements)
        {
            if (requirements == null || requirements.Length == 0)
            {
                xpRequirementsPerLevel = Array.Empty<int>();
                return;
            }

            xpRequirementsPerLevel = new int[requirements.Length];
            for (var i = 0; i < requirements.Length; i++)
            {
                xpRequirementsPerLevel[i] = Mathf.Max(1, requirements[i]);
            }

            RaiseXPChanged();
        }

        public void ResetProgression()
        {
            Level = startingLevel;
            CurrentXP = 0;
            RaiseXPChanged();
        }

        public void AddXP(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentXP += amount;
            ResolveLevelUps();
            RaiseXPChanged();
        }

        public int GetXPRequiredForLevel(int level)
        {
            var curveIndex = Mathf.Max(0, level - startingLevel);
            if (xpRequirementsPerLevel != null &&
                curveIndex >= 0 &&
                curveIndex < xpRequirementsPerLevel.Length)
            {
                return Mathf.Max(1, xpRequirementsPerLevel[curveIndex]);
            }

            return fallbackXPRequirement;
        }

        private void ResolveLevelUps()
        {
            var guard = 0;
            while (CurrentXP >= XPForNextLevel && guard < 32)
            {
                var previousLevel = Level;
                CurrentXP -= XPForNextLevel;
                Level++;
                var eventArgs = new PlayerLevelUpEventArgs(this, previousLevel, Level);
                OnLevelUp?.Invoke(eventArgs);
                OnAnyLevelUp?.Invoke(eventArgs);
                guard++;
            }
        }

        private void RaiseXPChanged()
        {
            OnXPChanged?.Invoke(new PlayerXPChangedEventArgs(this, Level, CurrentXP, XPForNextLevel));
        }
    }
}
