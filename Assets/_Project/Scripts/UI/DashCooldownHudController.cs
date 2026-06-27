using TapKnockout.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class DashCooldownHudController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerDashController dashController;
        [SerializeField] private Text cooldownLabel;
        [SerializeField] private Image cooldownFill;

        public string CurrentText { get; private set; } = string.Empty;
        public float CurrentFillAmount { get; private set; } = 1f;

        private void Update()
        {
            Refresh();
        }

        public void SetDashController(PlayerDashController controller)
        {
            dashController = controller;
            Refresh();
        }

        public void Refresh()
        {
            if (dashController == null)
            {
                SetState("Dash --", 0f);
                return;
            }

            var cooldownRemaining = dashController.CooldownRemaining;
            if (cooldownRemaining <= 0f)
            {
                SetState("Dash Ready", 1f);
                return;
            }

            SetState($"Dash {cooldownRemaining:0.0}s", 1f - dashController.NormalizedCooldown);
        }

        private void SetState(string text, float fillAmount)
        {
            CurrentText = text;
            CurrentFillAmount = Mathf.Clamp01(fillAmount);

            if (cooldownLabel != null)
            {
                cooldownLabel.text = CurrentText;
            }

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = CurrentFillAmount;
            }
        }
    }
}
