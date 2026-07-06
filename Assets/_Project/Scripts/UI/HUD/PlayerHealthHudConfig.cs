using UnityEngine;

namespace TapKnockout.UI.HUD
{
    /// <summary>
    /// ScriptableObject config for the Player Health HUD.
    /// Controls sprites, colors, animation timings, and behavioral thresholds.
    /// Create via: Assets > Create > Tap Knockout > UI > Player Health HUD Config
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerHealthHudConfig",
        menuName = "Tap Knockout/UI/Player Health HUD Config")]
    public sealed class PlayerHealthHudConfig : ScriptableObject
    {
        // ──────────────────────────────────────────────────────────
        // Sprites
        // ──────────────────────────────────────────────────────────

        [Header("Frame Sprites")]
        [Tooltip("Ornamental health bar frame sprite (GabrielaTot gold bar — health meter-28).")]
        [SerializeField] private Sprite frameSprite;

        [Tooltip("HP fill sprite that fits inside the frame (health meter-31 — red parallelogram bar).")]
        [SerializeField] private Sprite fillSprite;

        [Tooltip("Secondary bar frame sprite (health meter-29 — blue-heart gold frame, for mana/stamina).")]
        [SerializeField] private Sprite secondaryFrameSprite;

        [Tooltip("Portrait frame sprite.")]
        [SerializeField] private Sprite portraitFrameSprite;

        [Tooltip("Default portrait sprite shown when no character portrait is set.")]
        [SerializeField] private Sprite defaultPortraitSprite;

        [Tooltip("Sprite for buff slot background frames.")]
        [SerializeField] private Sprite buffSlotFrameSprite;

        // ──────────────────────────────────────────────────────────
        // Colors
        // ──────────────────────────────────────────────────────────

        [Header("Colors")]
        [Tooltip("Main HP fill color (deep red).")]
        [SerializeField] private Color hpFillColor = new Color(0.80f, 0.10f, 0.08f, 1f);

        [Tooltip("Damage delay bar color (muted dark red/orange — trails behind main fill).")]
        [SerializeField] private Color damageDelayColor = new Color(0.55f, 0.18f, 0.05f, 0.90f);

        [Tooltip("Secondary bar fill color (green for stamina/shield).")]
        [SerializeField] private Color secondaryBarColor = new Color(0.18f, 0.72f, 0.22f, 1f);

        [Tooltip("Low health warning tint applied to frame/glow.")]
        [SerializeField] private Color lowHealthWarningColor = new Color(1f, 0.22f, 0.10f, 0.80f);

        [Tooltip("Heal glow/pulse color.")]
        [SerializeField] private Color healGlowColor = new Color(0.40f, 1f, 0.50f, 0.55f);

        // ──────────────────────────────────────────────────────────
        // HP Animation
        // ──────────────────────────────────────────────────────────

        [Header("HP Fill Animation")]
        [Tooltip("Duration (seconds) for the main HP fill to animate to its target.")]
        [SerializeField, Min(0f)] private float hpFillAnimDuration = 0.15f;

        [Tooltip("Duration (seconds) the damage delay bar remains before catching up.")]
        [SerializeField, Min(0f)] private float damageDelayHoldDuration = 0.45f;

        [Tooltip("Duration (seconds) for the damage delay bar to catch down after holding.")]
        [SerializeField, Min(0f)] private float damageDelayCatchDuration = 0.55f;

        // ──────────────────────────────────────────────────────────
        // Damage Feedback
        // ──────────────────────────────────────────────────────────

        [Header("Damage Feedback")]
        [Tooltip("Peak scale multiplier applied to the health bar group on damage (1 = no punch).")]
        [SerializeField, Min(1f)] private float damagePulseScale = 1.045f;

        [Tooltip("Duration (seconds) of the damage scale punch animation.")]
        [SerializeField, Min(0f)] private float damagePulseDuration = 0.18f;

        // ──────────────────────────────────────────────────────────
        // Heal Feedback
        // ──────────────────────────────────────────────────────────

        [Header("Heal Feedback")]
        [Tooltip("Duration (seconds) of the heal glow pulse.")]
        [SerializeField, Min(0f)] private float healPulseDuration = 0.40f;

        // ──────────────────────────────────────────────────────────
        // Low Health State
        // ──────────────────────────────────────────────────────────

        [Header("Low Health")]
        [Tooltip("HP ratio below which the low health warning activates (0.25 = 25%).")]
        [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.25f;

        [Tooltip("Speed of the low health frame pulse (cycles per second).")]
        [SerializeField, Min(0f)] private float lowHealthPulseSpeed = 1.4f;

        [Tooltip("Minimum alpha of the low health pulse (0 = fully transparent at trough).")]
        [SerializeField, Range(0f, 1f)] private float lowHealthPulseMinAlpha = 0.20f;

        // ──────────────────────────────────────────────────────────
        // Text
        // ──────────────────────────────────────────────────────────

        [Header("Text")]
        [Tooltip("HP text format string. {0} = current HP (integer), {1} = max HP (integer).")]
        [SerializeField] private string hpTextFormat = "{0} / {1}";

        [Tooltip("Text to show when health data is unavailable.")]
        [SerializeField] private string hpTextFallback = "-- / --";

        // ──────────────────────────────────────────────────────────
        // Layout
        // ──────────────────────────────────────────────────────────

        [Header("Layout")]
        [Tooltip("Maximum pixel width of the health bar fill area at reference resolution.")]
        [SerializeField, Min(10f)] private float maxBarWidth = 380f;

        // ──────────────────────────────────────────────────────────
        // Public Accessors
        // ──────────────────────────────────────────────────────────

        public Sprite FrameSprite           => frameSprite;
        public Sprite FillSprite            => fillSprite;
        public Sprite SecondaryFrameSprite  => secondaryFrameSprite;
        public Sprite PortraitFrameSprite   => portraitFrameSprite;
        public Sprite DefaultPortraitSprite => defaultPortraitSprite;
        public Sprite BuffSlotFrameSprite   => buffSlotFrameSprite;

        public Color HpFillColor          => hpFillColor;
        public Color DamageDelayColor     => damageDelayColor;
        public Color SecondaryBarColor    => secondaryBarColor;
        public Color LowHealthWarningColor => lowHealthWarningColor;
        public Color HealGlowColor        => healGlowColor;

        public float HpFillAnimDuration       => hpFillAnimDuration;
        public float DamageDelayHoldDuration  => damageDelayHoldDuration;
        public float DamageDelayCatchDuration => damageDelayCatchDuration;

        public float DamagePulseScale    => damagePulseScale;
        public float DamagePulseDuration => damagePulseDuration;

        public float HealPulseDuration => healPulseDuration;

        public float LowHealthThreshold  => lowHealthThreshold;
        public float LowHealthPulseSpeed => lowHealthPulseSpeed;
        public float LowHealthPulseMinAlpha => lowHealthPulseMinAlpha;

        public string HpTextFormat   => hpTextFormat;
        public string HpTextFallback => hpTextFallback;

        public float MaxBarWidth => maxBarWidth;

        // ──────────────────────────────────────────────────────────
        // Validation
        // ──────────────────────────────────────────────────────────

        public bool IsValid(out string error)
        {
            if (hpFillAnimDuration < 0f)       { error = "hpFillAnimDuration must be >= 0"; return false; }
            if (damageDelayHoldDuration < 0f)  { error = "damageDelayHoldDuration must be >= 0"; return false; }
            if (damageDelayCatchDuration < 0f) { error = "damageDelayCatchDuration must be >= 0"; return false; }
            if (lowHealthThreshold < 0f || lowHealthThreshold > 1f) { error = "lowHealthThreshold must be in [0,1]"; return false; }
            if (damagePulseScale < 1f)         { error = "damagePulseScale must be >= 1"; return false; }
            if (maxBarWidth < 10f)             { error = "maxBarWidth must be >= 10"; return false; }
            error = null;
            return true;
        }

        private void OnValidate()
        {
            hpFillAnimDuration       = Mathf.Max(0f, hpFillAnimDuration);
            damageDelayHoldDuration  = Mathf.Max(0f, damageDelayHoldDuration);
            damageDelayCatchDuration = Mathf.Max(0f, damageDelayCatchDuration);
            healPulseDuration        = Mathf.Max(0f, healPulseDuration);
            damagePulseScale         = Mathf.Max(1f, damagePulseScale);
            damagePulseDuration      = Mathf.Max(0f, damagePulseDuration);
            lowHealthThreshold       = Mathf.Clamp01(lowHealthThreshold);
            lowHealthPulseSpeed      = Mathf.Max(0f, lowHealthPulseSpeed);
            lowHealthPulseMinAlpha   = Mathf.Clamp01(lowHealthPulseMinAlpha);
            maxBarWidth              = Mathf.Max(10f, maxBarWidth);

            if (string.IsNullOrEmpty(hpTextFormat))
            {
                hpTextFormat = "{0} / {1}";
            }
        }
    }
}
