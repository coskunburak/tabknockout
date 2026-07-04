using TapKnockout.Survivor;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class ActiveSkillSlotHud : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private Text hotkeyLabel;
        [SerializeField] private Text chargesLabel;
        [SerializeField] private Text stateLabel;
        [SerializeField] private Image readyFrameImage;
        [SerializeField] private Image disabledOverlayImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("State Colors")]
        [SerializeField] private Color readyColor = new Color(0.35f, 0.95f, 0.65f, 1f);
        [SerializeField] private Color cooldownColor = new Color(0.8f, 0.88f, 1f, 1f);
        [SerializeField] private Color castingColor = new Color(1f, 0.82f, 0.28f, 1f);
        [SerializeField] private Color emptyColor = new Color(0.7f, 0.72f, 0.76f, 1f);
        [SerializeField, Range(0.3f, 1f)] private float emptyAlpha = 0.68f;

        private Vector3 baseScale = Vector3.one;
        private bool hasBaseScale;

        private void Awake()
        {
            CaptureBaseScale();
        }

        private void OnEnable()
        {
            CaptureBaseScale();
        }

        public void Bind(Sprite icon, string hotkey, int charges = 0)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (hotkeyLabel != null)
            {
                hotkeyLabel.text = hotkey ?? string.Empty;
            }

            SetCharges(charges);
            SetCooldown(0f);
        }

        public void ApplyState(ActiveSkillSlotState state)
        {
            var icon = state.Ability != null ? state.Ability.Icon : null;
            Bind(icon, state.HotkeyLabel);
            SetCooldown(state.NormalizedCooldown);
            SetVisualState(state.IsReady, state.IsCasting, state.Ability == null, state.CooldownRemaining);
        }

        public void SetCooldown(float normalizedCooldown)
        {
            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = Mathf.Clamp01(normalizedCooldown);
            }
        }

        public void SetCharges(int charges)
        {
            if (chargesLabel != null)
            {
                chargesLabel.text = charges > 0 ? charges.ToString() : string.Empty;
            }
        }

        private void SetVisualState(bool ready, bool casting, bool empty, float cooldownRemaining)
        {
            var color = ResolveStateColor(ready, casting, empty);

            if (readyFrameImage != null)
            {
                var tintAlpha = casting ? 0.5f : ready ? 0.32f : 0.22f;
                readyFrameImage.color = new Color(color.r, color.g, color.b, tintAlpha);
                readyFrameImage.enabled = ready || casting || empty;
            }

            if (disabledOverlayImage != null)
            {
                disabledOverlayImage.enabled = empty && !ready && !casting;
            }

            if (stateLabel != null)
            {
                stateLabel.text = ResolveStateText(ready, casting, cooldownRemaining);
                stateLabel.color = color;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = empty && !ready && !casting ? emptyAlpha : 1f;
            }

            CaptureBaseScale();
            transform.localScale = casting
                ? baseScale * 1.06f
                : ready ? baseScale : baseScale * 0.98f;
        }

        private Color ResolveStateColor(bool ready, bool casting, bool empty)
        {
            if (casting)
            {
                return castingColor;
            }

            if (ready)
            {
                return empty ? emptyColor : readyColor;
            }

            return cooldownColor;
        }

        private static string ResolveStateText(bool ready, bool casting, float cooldownRemaining)
        {
            if (casting)
            {
                return "CAST";
            }

            if (ready)
            {
                return "READY";
            }

            return Mathf.CeilToInt(Mathf.Max(0f, cooldownRemaining)).ToString();
        }

        private void CaptureBaseScale()
        {
            if (hasBaseScale)
            {
                return;
            }

            baseScale = transform.localScale;
            hasBaseScale = true;
        }
    }
}
