using TapKnockout.Pickups;
using TapKnockout.Player;
using TapKnockout.Survivor;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class SurvivorHudController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArenaRunDirector runDirector;
        [SerializeField] private SurvivorSpawnDirector spawnDirector;
        [SerializeField] private PlayerXPController xpController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerDashController dashController;
        [SerializeField] private ActiveSkillController activeSkillController;

        [Header("UI")]
        [SerializeField] private Text runTimerText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text xpText;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image dashCooldownFill;
        [SerializeField] private ActiveSkillSlotHud[] activeSkillSlots;
        [SerializeField] private Text liveEnemyText;
        [SerializeField] private Text bossWarningText;

        private float bossWarningVisibleUntil;

        public ActiveSkillController ActiveSkillController => activeSkillController;
        public int ActiveSkillSlotViewCount => activeSkillSlots != null ? activeSkillSlots.Length : 0;

        private void OnEnable()
        {
            if (xpController != null)
            {
                xpController.OnXPChanged += HandleXPChanged;
            }

            if (runDirector != null)
            {
                runDirector.OnBossWarning += HandleBossWarning;
                runDirector.OnRunStateChanged += HandleRunStateChanged;
            }

            if (activeSkillController != null)
            {
                activeSkillController.OnSlotStateChanged += HandleActiveSkillSlotChanged;
            }

            RefreshAll();
        }

        private void OnDisable()
        {
            if (xpController != null)
            {
                xpController.OnXPChanged -= HandleXPChanged;
            }

            if (runDirector != null)
            {
                runDirector.OnBossWarning -= HandleBossWarning;
                runDirector.OnRunStateChanged -= HandleRunStateChanged;
            }

            if (activeSkillController != null)
            {
                activeSkillController.OnSlotStateChanged -= HandleActiveSkillSlotChanged;
            }
        }

        private void Update()
        {
            RefreshTimer();
            RefreshHealth();
            RefreshDash();
            RefreshLiveEnemies();
            RefreshBossWarning();
        }

        public void Bind(
            ArenaRunDirector director,
            SurvivorSpawnDirector survivorSpawnDirector,
            PlayerXPController playerXP,
            PlayerHealth health,
            PlayerDashController dash)
        {
            Bind(director, survivorSpawnDirector, playerXP, health, dash, activeSkillController);
        }

        public void Bind(
            ArenaRunDirector director,
            SurvivorSpawnDirector survivorSpawnDirector,
            PlayerXPController playerXP,
            PlayerHealth health,
            PlayerDashController dash,
            ActiveSkillController skills)
        {
            OnDisable();
            runDirector = director;
            spawnDirector = survivorSpawnDirector;
            xpController = playerXP;
            playerHealth = health;
            dashController = dash;
            activeSkillController = skills;
            OnEnable();
        }

        private void RefreshAll()
        {
            RefreshTimer();
            RefreshXP();
            RefreshHealth();
            RefreshDash();
            RefreshActiveSkills();
            RefreshLiveEnemies();
            RefreshBossWarning();
        }

        private void RefreshTimer()
        {
            if (runTimerText == null || runDirector == null)
            {
                return;
            }

            var elapsed = runDirector.RunTimer.ElapsedSeconds;
            var minutes = Mathf.FloorToInt(elapsed / 60f);
            var seconds = Mathf.FloorToInt(elapsed % 60f);
            runTimerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void RefreshXP()
        {
            if (xpController == null)
            {
                return;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv {xpController.Level}";
            }

            if (xpText != null)
            {
                xpText.text = $"{xpController.CurrentXP}/{xpController.XPForNextLevel}";
            }

            if (xpSlider != null)
            {
                xpSlider.value = xpController.NormalizedXP;
            }
        }

        private void RefreshHealth()
        {
            if (healthSlider == null || playerHealth == null)
            {
                return;
            }

            healthSlider.value = playerHealth.MaxHealth > 0f
                ? Mathf.Clamp01(playerHealth.CurrentHealth / playerHealth.MaxHealth)
                : 0f;
        }

        private void RefreshDash()
        {
            if (dashCooldownFill != null && dashController != null)
            {
                dashCooldownFill.fillAmount = dashController.NormalizedCooldown;
            }
        }

        private void RefreshActiveSkills()
        {
            if (activeSkillController == null || activeSkillSlots == null)
            {
                return;
            }

            for (var i = 0; i < activeSkillSlots.Length; i++)
            {
                if (activeSkillSlots[i] == null)
                {
                    continue;
                }

                ApplyActiveSkillSlotState(activeSkillController.GetSlotState(i));
            }
        }

        private void RefreshLiveEnemies()
        {
            if (liveEnemyText != null && spawnDirector != null)
            {
                liveEnemyText.text = spawnDirector.LiveEnemyCount.ToString();
            }
        }

        private void RefreshBossWarning()
        {
            if (bossWarningText == null)
            {
                return;
            }

            bossWarningText.enabled = Time.unscaledTime < bossWarningVisibleUntil;
        }

        private void HandleXPChanged(PlayerXPChangedEventArgs eventArgs)
        {
            RefreshXP();
        }

        private void HandleBossWarning(float secondsUntilBoss)
        {
            bossWarningVisibleUntil = Time.unscaledTime + 3f;
            if (bossWarningText != null)
            {
                bossWarningText.text = secondsUntilBoss > 0f ? "Boss Incoming" : "Boss Active";
            }
        }

        private void HandleRunStateChanged(SurvivorRunStateChangedEventArgs eventArgs)
        {
            if (bossWarningText == null)
            {
                return;
            }

            if (eventArgs.NewState == SurvivorRunState.BossActive)
            {
                bossWarningVisibleUntil = Time.unscaledTime + 3f;
                bossWarningText.text = "Boss Active";
            }
        }

        private void HandleActiveSkillSlotChanged(ActiveSkillSlotState state)
        {
            ApplyActiveSkillSlotState(state);
        }

        private void ApplyActiveSkillSlotState(ActiveSkillSlotState state)
        {
            if (activeSkillSlots == null ||
                state.SlotIndex < 0 ||
                state.SlotIndex >= activeSkillSlots.Length ||
                activeSkillSlots[state.SlotIndex] == null)
            {
                return;
            }

            var slotHud = activeSkillSlots[state.SlotIndex];
            slotHud.ApplyState(state);
        }
    }
}
