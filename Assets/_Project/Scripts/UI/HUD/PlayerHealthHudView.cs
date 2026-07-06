using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI.HUD
{
    /// <summary>
    /// Pure rendering layer for the Player Health HUD.
    /// Owns no gameplay logic — only displays what the Presenter tells it.
    ///
    /// Structure expected (created by PlayerHealthHudBuilder):
    ///   PF_PlayerHealthHUD_TopLeft
    ///    └─ SafeAreaRoot
    ///         └─ TopLeftAnchor
    ///              ├─ PortraitGroup
    ///              │    ├─ PortraitFrame
    ///              │    ├─ PortraitImage
    ///              │    └─ LevelBadge
    ///              │         ├─ BadgeFrame
    ///              │         └─ LevelText
    ///              ├─ HealthBarGroup
    ///              │    ├─ HealthBarFrame       ← GabrielaTot ornamental frame
    ///              │    ├─ HealthBarBackground  ← empty-bar tint behind fills
    ///              │    ├─ HealthDamageDelayFill
    ///              │    ├─ HealthFill
    ///              │    ├─ HealthGlow
    ///              │    └─ HealthText
    ///              ├─ SecondaryBarGroup
    ///              │    ├─ SecondaryBarBackground
    ///              │    ├─ SecondaryBarFill
    ///              │    └─ SecondaryBarText (optional)
    ///              └─ BuffIconRow
    ///                   ├─ BuffSlot_01 … BuffSlot_04
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHudView : MonoBehaviour
    {
        // ──────────────────────────────────────────────────────────
        // Inspector References — wired by PlayerHealthHudBuilder
        // ──────────────────────────────────────────────────────────

        [Header("Config")]
        [SerializeField] private PlayerHealthHudConfig config;

        [Header("Portrait")]
        [SerializeField] private Image portraitFrameImage;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text  levelText;

        [Header("Health Bar")]
        [SerializeField] private Image healthBarFrame;
        [SerializeField] private Image healthBarBackground;
        [SerializeField] private Image healthDamageDelayFill;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image healthGlow;
        [SerializeField] private Text  healthText;
        [SerializeField] private RectTransform healthBarGroup;

        [Header("Secondary Bar")]
        [SerializeField] private Image secondaryBarBackground;
        [SerializeField] private Image secondaryBarFill;
        [SerializeField] private Text  secondaryBarText;
        [SerializeField] private GameObject secondaryBarGroup;

        [Header("Buff Slots")]
        [SerializeField] private PlayerHealthHudBuffSlot[] buffSlots;

        // ──────────────────────────────────────────────────────────
        // Runtime State
        // ──────────────────────────────────────────────────────────

        private float   currentHpRatio;
        private float   currentSecondaryRatio;
        private int     cachedCurrentHp;
        private int     cachedMaxHp;
        private int     cachedLevel = -1;
        private string  cachedHpText;
        private bool    isLowHealth;
        private bool    lowHealthPulseRunning;
        private Vector3 originalBarGroupScale;

        // Coroutine handles — stored so we can cancel conflicting ones
        private Coroutine fillAnimCoroutine;
        private Coroutine delayFillCoroutine;
        private Coroutine damagePulseCoroutine;
        private Coroutine healGlowCoroutine;
        private Coroutine lowHealthPulseCoroutine;

        // ──────────────────────────────────────────────────────────
        // Unity Lifecycle
        // ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (healthBarGroup != null)
            {
                originalBarGroupScale = healthBarGroup.localScale;
            }

            // Ensure glow starts invisible
            if (healthGlow != null)
            {
                var c = healthGlow.color;
                c.a = 0f;
                healthGlow.color = c;
            }

            ApplyConfigColors();
            HideSecondaryBar();
        }

        private void ApplyConfigColors()
        {
            if (config == null) return;

            if (healthFill != null)          healthFill.color          = config.HpFillColor;
            if (healthDamageDelayFill != null) healthDamageDelayFill.color = config.DamageDelayColor;
            if (secondaryBarFill != null)    secondaryBarFill.color    = config.SecondaryBarColor;
        }

        // ──────────────────────────────────────────────────────────
        // Public API — called by PlayerHealthHudPresenter
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Updates the displayed health values.
        /// If animate is true, runs smooth fill + damage delay animations.
        /// Otherwise snaps immediately (useful for initialisation or respawn).
        /// </summary>
        public void SetHealth(float current, float max, bool isDamage, bool animate = true)
        {
            var safeCurrent = max > 0f ? Mathf.Clamp(current, 0f, max) : 0f;
            var safeMax     = Mathf.Max(0f, max);
            var ratio       = safeMax > 0f ? safeCurrent / safeMax : 0f;

            var currentInt = Mathf.CeilToInt(safeCurrent);
            var maxInt     = Mathf.CeilToInt(safeMax);

            // Avoid redundant text rebuilds
            if (currentInt != cachedCurrentHp || maxInt != cachedMaxHp)
            {
                cachedCurrentHp = currentInt;
                cachedMaxHp     = maxInt;
                RefreshHpText(currentInt, maxInt);
            }

            // Low health state check
            var lowHealthThreshold = config != null ? config.LowHealthThreshold : 0.25f;
            var wasLowHealth       = isLowHealth;
            isLowHealth = ratio > 0f && ratio <= lowHealthThreshold;

            if (isLowHealth && !wasLowHealth)   StartLowHealthPulse();
            if (!isLowHealth && wasLowHealth)   StopLowHealthPulse();

            if (animate)
            {
                if (isDamage)
                {
                    // Main fill drops immediately; delay fill trails
                    SetFillImmediate(ratio);
                    StartDamageDelayAnim(ratio);
                    PlayDamageAnimation();
                }
                else
                {
                    // Heal: fill rises smoothly
                    StartFillAnim(ratio);
                    // Snap delay bar to match immediately (no trailing on heal)
                    SetDelayFillImmediate(ratio);
                    PlayHealAnimation();
                }
            }
            else
            {
                SetFillImmediate(ratio);
                SetDelayFillImmediate(ratio);
            }

            currentHpRatio = ratio;
        }

        /// <summary>
        /// Updates the secondary bar (stamina / shield / energy).
        /// Pass max <= 0 or current < 0 to hide the secondary bar.
        /// </summary>
        public void SetSecondaryValue(float current, float max)
        {
            if (max <= 0f)
            {
                HideSecondaryBar();
                return;
            }

            var ratio = Mathf.Clamp01(current / max);
            currentSecondaryRatio = ratio;

            if (secondaryBarGroup != null) secondaryBarGroup.SetActive(true);
            if (secondaryBarFill  != null) secondaryBarFill.fillAmount = ratio;

            if (secondaryBarText != null)
            {
                secondaryBarText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }

        /// <summary>Updates the level badge number. Skips if unchanged.</summary>
        public void SetLevel(int level)
        {
            if (level == cachedLevel) return;
            cachedLevel = level;

            if (levelText != null)
            {
                levelText.text = level.ToString();
            }
        }

        /// <summary>Sets the character portrait sprite.</summary>
        public void SetPortrait(Sprite portrait)
        {
            if (portraitImage == null) return;

            var sprite = portrait != null
                ? portrait
                : (config != null ? config.DefaultPortraitSprite : null);

            portraitImage.sprite  = sprite;
            portraitImage.enabled = sprite != null;
        }

        // ── Buff Slot API ──────────────────────────────────────────

        /// <summary>Sets a buff/status icon in the given slot (0-based).</summary>
        public void SetBuffIcon(int index, Sprite icon, int stackCount = 0, bool visible = true)
        {
            if (buffSlots == null || index < 0 || index >= buffSlots.Length) return;
            if (buffSlots[index] == null) return;
            buffSlots[index].Apply(icon, stackCount, visible);
        }

        /// <summary>Clears a single buff slot.</summary>
        public void ClearBuffIcon(int index)
        {
            if (buffSlots == null || index < 0 || index >= buffSlots.Length) return;
            if (buffSlots[index] == null) return;
            buffSlots[index].Clear();
        }

        /// <summary>Clears all buff slots.</summary>
        public void ClearAllBuffIcons()
        {
            if (buffSlots == null) return;
            foreach (var slot in buffSlots)
            {
                slot?.Clear();
            }
        }

        // ── Feedback Animations ────────────────────────────────────

        /// <summary>Plays the damage scale punch on the health bar group.</summary>
        public void PlayDamageAnimation()
        {
            if (healthBarGroup == null) return;
            if (config == null) return;

            if (damagePulseCoroutine != null) StopCoroutine(damagePulseCoroutine);
            damagePulseCoroutine = StartCoroutine(DamagePulseCoroutine());
        }

        /// <summary>Plays the heal glow pulse on the health fill/glow image.</summary>
        public void PlayHealAnimation()
        {
            if (healthGlow == null) return;
            if (config == null) return;

            if (healGlowCoroutine != null) StopCoroutine(healGlowCoroutine);
            healGlowCoroutine = StartCoroutine(HealGlowCoroutine());
        }

        // ── Snapshot (Editor Preview) ──────────────────────────────

        /// <summary>Snaps the HUD to the given ratio immediately (no animation). Safe to call in editor.</summary>
        public void SnapshotHealth(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            SetFillImmediate(ratio);
            SetDelayFillImmediate(ratio);
        }

        // ──────────────────────────────────────────────────────────
        // Context Menu Preview (editor use)
        // ──────────────────────────────────────────────────────────

        [ContextMenu("Preview: Full Health")]
        private void PreviewFullHealth()
        {
            if (!Application.isPlaying) { SnapshotHealth(1f); return; }
            SetHealth(100f, 100f, false, false);
        }

        [ContextMenu("Preview: Damage (50%)")]
        private void PreviewDamage()
        {
            if (!Application.isPlaying) { SnapshotHealth(0.5f); return; }
            SetHealth(50f, 100f, true, true);
        }

        [ContextMenu("Preview: Heal (75%)")]
        private void PreviewHeal()
        {
            if (!Application.isPlaying) { SnapshotHealth(0.75f); return; }
            SetHealth(75f, 100f, false, true);
        }

        [ContextMenu("Preview: Low Health (15%)")]
        private void PreviewLowHealth()
        {
            if (!Application.isPlaying) { SnapshotHealth(0.15f); return; }
            SetHealth(15f, 100f, true, true);
        }

        // ──────────────────────────────────────────────────────────
        // Private Helpers
        // ──────────────────────────────────────────────────────────

        private void RefreshHpText(int current, int max)
        {
            if (healthText == null) return;

            string text;
            if (config != null && max > 0)
            {
                text = string.Format(config.HpTextFormat, current, max);
            }
            else if (config != null)
            {
                text = config.HpTextFallback;
            }
            else
            {
                text = max > 0 ? $"{current} / {max}" : "-- / --";
            }

            if (cachedHpText == text) return;
            cachedHpText    = text;
            healthText.text = text;
        }

        private void SetFillImmediate(float ratio)
        {
            if (fillAnimCoroutine != null)
            {
                StopCoroutine(fillAnimCoroutine);
                fillAnimCoroutine = null;
            }

            if (healthFill != null) healthFill.fillAmount = ratio;
        }

        private void SetDelayFillImmediate(float ratio)
        {
            if (delayFillCoroutine != null)
            {
                StopCoroutine(delayFillCoroutine);
                delayFillCoroutine = null;
            }

            if (healthDamageDelayFill != null) healthDamageDelayFill.fillAmount = ratio;
        }

        private void StartFillAnim(float targetRatio)
        {
            if (fillAnimCoroutine != null) StopCoroutine(fillAnimCoroutine);
            fillAnimCoroutine = StartCoroutine(FillAnimCoroutine(targetRatio));
        }

        private void StartDamageDelayAnim(float targetRatio)
        {
            if (delayFillCoroutine != null) StopCoroutine(delayFillCoroutine);
            delayFillCoroutine = StartCoroutine(DamageDelayCoroutine(targetRatio));
        }

        private void HideSecondaryBar()
        {
            if (secondaryBarGroup != null) secondaryBarGroup.SetActive(false);
        }

        private void StartLowHealthPulse()
        {
            if (lowHealthPulseRunning) return;
            if (lowHealthPulseCoroutine != null) StopCoroutine(lowHealthPulseCoroutine);
            lowHealthPulseCoroutine = StartCoroutine(LowHealthPulseCoroutine());
        }

        private void StopLowHealthPulse()
        {
            if (lowHealthPulseCoroutine != null)
            {
                StopCoroutine(lowHealthPulseCoroutine);
                lowHealthPulseCoroutine = null;
            }

            lowHealthPulseRunning = false;

            // Reset glow to invisible
            if (healthGlow != null)
            {
                var c = healthGlow.color;
                c.a = 0f;
                healthGlow.color = c;
            }
        }

        // ──────────────────────────────────────────────────────────
        // Coroutines
        // ──────────────────────────────────────────────────────────

        private IEnumerator FillAnimCoroutine(float targetRatio)
        {
            if (healthFill == null) yield break;

            var startRatio = healthFill.fillAmount;
            var duration   = config != null ? config.HpFillAnimDuration : 0.15f;

            if (duration <= 0f)
            {
                healthFill.fillAmount = targetRatio;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                healthFill.fillAmount = Mathf.Lerp(startRatio, targetRatio, elapsed / duration);
                yield return null;
            }

            healthFill.fillAmount = targetRatio;
            fillAnimCoroutine = null;
        }

        private IEnumerator DamageDelayCoroutine(float targetRatio)
        {
            if (healthDamageDelayFill == null) yield break;

            // Hold phase
            var holdDuration = config != null ? config.DamageDelayHoldDuration : 0.45f;
            var elapsed = 0f;
            while (elapsed < holdDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Catch-down phase
            var startRatio   = healthDamageDelayFill.fillAmount;
            var catchDuration = config != null ? config.DamageDelayCatchDuration : 0.55f;

            if (catchDuration <= 0f)
            {
                healthDamageDelayFill.fillAmount = targetRatio;
                delayFillCoroutine = null;
                yield break;
            }

            elapsed = 0f;
            while (elapsed < catchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                healthDamageDelayFill.fillAmount = Mathf.Lerp(startRatio, targetRatio, elapsed / catchDuration);
                yield return null;
            }

            healthDamageDelayFill.fillAmount = targetRatio;
            delayFillCoroutine = null;
        }

        private IEnumerator DamagePulseCoroutine()
        {
            if (healthBarGroup == null) yield break;

            var peakScale    = config != null ? config.DamagePulseScale    : 1.045f;
            var duration     = config != null ? config.DamagePulseDuration : 0.18f;
            var halfDuration = duration * 0.5f;

            var baseScale  = originalBarGroupScale;
            var targetScale = baseScale * peakScale;

            if (duration <= 0f)
            {
                healthBarGroup.localScale = baseScale;
                yield break;
            }

            // Punch out
            var elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                healthBarGroup.localScale = Vector3.Lerp(baseScale, targetScale, elapsed / halfDuration);
                yield return null;
            }

            // Return
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                healthBarGroup.localScale = Vector3.Lerp(targetScale, baseScale, elapsed / halfDuration);
                yield return null;
            }

            healthBarGroup.localScale = baseScale;
            damagePulseCoroutine = null;
        }

        private IEnumerator HealGlowCoroutine()
        {
            if (healthGlow == null) yield break;

            var duration = config != null ? config.HealPulseDuration : 0.40f;
            var halfDur  = duration * 0.5f;
            var glowColor = config != null ? config.HealGlowColor : new Color(0.4f, 1f, 0.5f, 0.55f);

            if (duration <= 0f)
            {
                var zeroColor = glowColor; zeroColor.a = 0f;
                healthGlow.color = zeroColor;
                yield break;
            }

            // Fade in
            var elapsed = 0f;
            while (elapsed < halfDur)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = elapsed / halfDur;
                var c = glowColor;
                c.a = Mathf.Lerp(0f, glowColor.a, t);
                healthGlow.color = c;
                yield return null;
            }

            // Fade out
            elapsed = 0f;
            while (elapsed < halfDur)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = elapsed / halfDur;
                var c = glowColor;
                c.a = Mathf.Lerp(glowColor.a, 0f, t);
                healthGlow.color = c;
                yield return null;
            }

            var finalColor = glowColor;
            finalColor.a = 0f;
            healthGlow.color = finalColor;
            healGlowCoroutine = null;
        }

        private IEnumerator LowHealthPulseCoroutine()
        {
            lowHealthPulseRunning = true;

            var pulseSpeed    = config != null ? config.LowHealthPulseSpeed    : 1.4f;
            var minAlpha      = config != null ? config.LowHealthPulseMinAlpha : 0.20f;
            var warningColor  = config != null ? config.LowHealthWarningColor  : new Color(1f, 0.22f, 0.10f, 0.80f);

            while (isLowHealth)
            {
                // Sinusoidal alpha pulse on the glow layer
                if (healthGlow != null)
                {
                    var sin   = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                    var alpha = Mathf.Lerp(minAlpha, warningColor.a, sin);
                    var c     = warningColor;
                    c.a = alpha;
                    healthGlow.color = c;
                }

                yield return null;
            }

            lowHealthPulseRunning   = false;
            lowHealthPulseCoroutine = null;
        }

        // ──────────────────────────────────────────────────────────
        // Cleanup
        // ──────────────────────────────────────────────────────────

        private void OnDisable()
        {
            // Stop all coroutines cleanly — avoids stale coroutines on re-enable
            StopAllCoroutines();
            fillAnimCoroutine       = null;
            delayFillCoroutine      = null;
            damagePulseCoroutine    = null;
            healGlowCoroutine       = null;
            lowHealthPulseCoroutine = null;
            lowHealthPulseRunning   = false;

            // Restore scale if disabled mid-animation
            if (healthBarGroup != null && originalBarGroupScale != Vector3.zero)
            {
                healthBarGroup.localScale = originalBarGroupScale;
            }
        }
    }
}
