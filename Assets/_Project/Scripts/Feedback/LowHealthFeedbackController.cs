using System;
using TapKnockout.Combat;
using TapKnockout.Player;
using TapKnockout.VFX;
using UnityEngine;

namespace TapKnockout.Feedback
{
    public readonly struct LowHealthStateChangedEventArgs
    {
        public LowHealthStateChangedEventArgs(GameObject source, float currentHealth, float maxHealth, bool isLowHealth)
        {
            Source = source;
            CurrentHealth = Mathf.Max(0f, currentHealth);
            MaxHealth = Mathf.Max(0f, maxHealth);
            NormalizedHealth = MaxHealth > 0f ? Mathf.Clamp01(CurrentHealth / MaxHealth) : 0f;
            IsLowHealth = isLowHealth;
        }

        public GameObject Source { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float NormalizedHealth { get; }
        public bool IsLowHealth { get; }
    }

    [DisallowMultipleComponent]
    public sealed class LowHealthFeedbackController : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private VFXService vfxService;
        [SerializeField, Range(0.01f, 1f)] private float lowHealthThreshold = 0.3f;
        [SerializeField, Range(0f, 0.5f)] private float recoveryHysteresis = 0.08f;
        [SerializeField, Min(0.02f)] private float evaluationInterval = 0.1f;
        [SerializeField] private bool spawnVFXOnEnter = true;
        [SerializeField] private bool raiseAudioHookOnEnter = true;

        private float evaluationRemaining;

        public event Action<LowHealthStateChangedEventArgs> OnLowHealthStateChanged;

        public bool IsLowHealth { get; private set; }

        private void Reset()
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
            vfxService = GetComponentInParent<VFXService>();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (playerHealth != null)
            {
                playerHealth.OnDamaged -= HandlePlayerDamaged;
                playerHealth.OnDamaged += HandlePlayerDamaged;
                playerHealth.OnPlayerDied -= HandlePlayerDied;
                playerHealth.OnPlayerDied += HandlePlayerDied;
            }

            EvaluateNow();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDamaged -= HandlePlayerDamaged;
                playerHealth.OnPlayerDied -= HandlePlayerDied;
            }
        }

        private void OnValidate()
        {
            evaluationInterval = Mathf.Max(0.02f, evaluationInterval);
        }

        private void Update()
        {
            evaluationRemaining -= Time.unscaledDeltaTime;
            if (evaluationRemaining > 0f)
            {
                return;
            }

            evaluationRemaining = evaluationInterval;
            EvaluateNow();
        }

        public void EvaluateNow()
        {
            if (playerHealth == null)
            {
                return;
            }

            var nextLowHealth = ShouldBeLowHealth(
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth,
                lowHealthThreshold,
                recoveryHysteresis,
                IsLowHealth);

            if (nextLowHealth == IsLowHealth)
            {
                return;
            }

            IsLowHealth = nextLowHealth;
            var eventArgs = new LowHealthStateChangedEventArgs(
                playerHealth.gameObject,
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth,
                IsLowHealth);

            OnLowHealthStateChanged?.Invoke(eventArgs);

            if (IsLowHealth)
            {
                TriggerEnterLowHealthFeedback(eventArgs);
            }
        }

        public static bool ShouldBeLowHealth(
            float currentHealth,
            float maxHealth,
            float lowHealthThreshold,
            float recoveryHysteresis,
            bool currentlyLowHealth)
        {
            if (maxHealth <= 0f)
            {
                return false;
            }

            var normalized = Mathf.Clamp01(currentHealth / maxHealth);
            var enterThreshold = Mathf.Clamp01(lowHealthThreshold);
            var exitThreshold = Mathf.Clamp01(enterThreshold + Mathf.Max(0f, recoveryHysteresis));
            return currentlyLowHealth ? normalized <= exitThreshold : normalized <= enterThreshold;
        }

        private void HandlePlayerDamaged(HitContext hitContext)
        {
            EvaluateNow();
        }

        private void HandlePlayerDied(HitContext hitContext)
        {
            EvaluateNow();
        }

        private void TriggerEnterLowHealthFeedback(LowHealthStateChangedEventArgs eventArgs)
        {
            var position = eventArgs.Source != null ? eventArgs.Source.transform.position : transform.position;

            if (spawnVFXOnEnter && vfxService != null)
            {
                vfxService.Spawn(VFXEventType.LowHealthWarning, position);
            }

            if (raiseAudioHookOnEnter)
            {
                FeedbackAudioEvents.RaiseFeedbackAudioRequested(new FeedbackAudioEventArgs(
                    FeedbackAudioEventType.LowHealthWarning,
                    position,
                    eventArgs.Source,
                    null,
                    1f - eventArgs.NormalizedHealth));
            }
        }

        private void ResolveReferences()
        {
            if (playerHealth == null)
            {
                playerHealth = GetComponentInParent<PlayerHealth>();
            }

            if (vfxService == null)
            {
                vfxService = GetComponentInParent<VFXService>();
            }
        }
    }
}
