using TapKnockout.Boss;
using TapKnockout.Combat;
using TapKnockout.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class BossHealthBarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossConfig bossConfig;
        [SerializeField] private EnemyHealth bossHealth;
        [SerializeField] private BossPhaseController phaseController;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text bossNameLabel;
        [SerializeField] private Text phaseLabel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject root;

        [Header("Behavior")]
        [SerializeField] private bool hideWhenNoBoss = true;
        [SerializeField] private bool hideOnDeath = true;

        public float CurrentFillAmount { get; private set; }
        public string CurrentBossNameText { get; private set; } = string.Empty;
        public string CurrentPhaseText { get; private set; } = string.Empty;
        public bool IsVisible { get; private set; }

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            root = gameObject;
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetBoss(BossConfig config, EnemyHealth health, BossPhaseController phases)
        {
            Unsubscribe();
            bossConfig = config;
            bossHealth = health;
            phaseController = phases;
            Subscribe();
            Refresh();
        }

        public void SetBossFromGameObject(GameObject boss, BossConfig config)
        {
            if (boss == null)
            {
                if (bossConfig == null && config != null)
                {
                    bossConfig = config;
                }

                Refresh();
                return;
            }

            var resolvedHealth = boss.GetComponentInChildren<EnemyHealth>(true);
            var resolvedPhaseController = boss.GetComponentInChildren<BossPhaseController>(true);
            var resolvedConfig = config != null ? config : resolvedPhaseController != null ? resolvedPhaseController.Config : bossConfig;
            if (resolvedHealth == bossHealth && resolvedPhaseController == phaseController && resolvedConfig == bossConfig)
            {
                Refresh();
                return;
            }

            SetBoss(resolvedConfig, resolvedHealth, resolvedPhaseController);
        }

        public void Show()
        {
            SetVisible(true);
            Refresh();
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void Refresh()
        {
            if (bossHealth == null)
            {
                CurrentFillAmount = 0f;
                CurrentBossNameText = bossConfig != null ? bossConfig.DisplayName : "Boss";
                CurrentPhaseText = string.Empty;
                ApplyView();

                if (hideWhenNoBoss)
                {
                    SetVisible(false);
                }

                return;
            }

            CurrentFillAmount = bossHealth.MaxHealth > 0f
                ? Mathf.Clamp01(bossHealth.CurrentHealth / bossHealth.MaxHealth)
                : 0f;
            CurrentBossNameText = bossConfig != null ? bossConfig.DisplayName : bossHealth.name;
            CurrentPhaseText = ResolvePhaseText(phaseController != null ? phaseController.CurrentPhase : BossPhaseState.None);
            ApplyView();

            if (!bossHealth.IsAlive && hideOnDeath)
            {
                SetVisible(false);
            }
            else
            {
                SetVisible(true);
            }
        }

        private void Subscribe()
        {
            if (bossHealth != null)
            {
                bossHealth.OnDamaged -= HandleBossHealthChanged;
                bossHealth.OnDamaged += HandleBossHealthChanged;
                bossHealth.OnDied -= HandleBossHealthChanged;
                bossHealth.OnDied += HandleBossHealthChanged;
            }

            if (phaseController != null)
            {
                phaseController.OnPhaseChanged -= HandlePhaseChanged;
                phaseController.OnPhaseChanged += HandlePhaseChanged;
            }

            BossEvents.OnBossIntroStarted -= HandleBossIntroStarted;
            BossEvents.OnBossIntroStarted += HandleBossIntroStarted;
            BossEvents.OnBossPhaseChanged -= HandleBossPhaseChangedGlobal;
            BossEvents.OnBossPhaseChanged += HandleBossPhaseChangedGlobal;
            BossEvents.OnBossDefeated -= HandleBossDefeated;
            BossEvents.OnBossDefeated += HandleBossDefeated;
            BossEvents.OnBossOutroCompleted -= HandleBossOutroCompleted;
            BossEvents.OnBossOutroCompleted += HandleBossOutroCompleted;
        }

        private void Unsubscribe()
        {
            if (bossHealth != null)
            {
                bossHealth.OnDamaged -= HandleBossHealthChanged;
                bossHealth.OnDied -= HandleBossHealthChanged;
            }

            if (phaseController != null)
            {
                phaseController.OnPhaseChanged -= HandlePhaseChanged;
            }

            BossEvents.OnBossIntroStarted -= HandleBossIntroStarted;
            BossEvents.OnBossPhaseChanged -= HandleBossPhaseChangedGlobal;
            BossEvents.OnBossDefeated -= HandleBossDefeated;
            BossEvents.OnBossOutroCompleted -= HandleBossOutroCompleted;
        }

        private void HandleBossHealthChanged(HitContext hitContext)
        {
            Refresh();
        }

        private void HandlePhaseChanged(BossPhaseChangedEventArgs eventArgs)
        {
            Refresh();
        }

        private void HandleBossIntroStarted(BossEventArgs eventArgs)
        {
            SetBossFromGameObject(eventArgs.Boss, eventArgs.BossConfig);
            Show();
        }

        private void HandleBossPhaseChangedGlobal(BossPhaseChangedEventArgs eventArgs)
        {
            if (bossHealth == null || eventArgs.Boss == null || eventArgs.Boss.GetComponentInChildren<EnemyHealth>(true) == bossHealth)
            {
                SetBossFromGameObject(eventArgs.Boss, eventArgs.BossConfig);
                Refresh();
            }
        }

        private void HandleBossDefeated(BossEventArgs eventArgs)
        {
            SetBossFromGameObject(eventArgs.Boss, eventArgs.BossConfig);
            Refresh();
            if (hideOnDeath)
            {
                Hide();
            }
        }

        private void HandleBossOutroCompleted(BossEventArgs eventArgs)
        {
            if (hideOnDeath)
            {
                Hide();
            }
        }

        private void ApplyView()
        {
            if (healthSlider != null)
            {
                healthSlider.value = CurrentFillAmount;
            }

            if (bossNameLabel != null)
            {
                bossNameLabel.text = CurrentBossNameText;
            }

            if (phaseLabel != null)
            {
                phaseLabel.text = CurrentPhaseText;
            }
        }

        private void SetVisible(bool visible)
        {
            IsVisible = visible;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            if (root != null && root != gameObject)
            {
                root.SetActive(visible);
            }
        }

        private static string ResolvePhaseText(BossPhaseState phase)
        {
            switch (phase)
            {
                case BossPhaseState.Phase1:
                    return "Phase 1";
                case BossPhaseState.Phase2:
                    return "Phase 2";
                case BossPhaseState.Phase3:
                    return "Phase 3";
                case BossPhaseState.Defeated:
                    return "Defeated";
                default:
                    return string.Empty;
            }
        }
    }
}
