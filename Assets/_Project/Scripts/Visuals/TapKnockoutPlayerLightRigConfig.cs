using UnityEngine;

namespace TapKnockout.Visuals
{
    [CreateAssetMenu(fileName = "TapKnockoutPlayerLightRigConfig", menuName = "Tap Knockout/Visuals/Player Light Rig Config")]
    public sealed class TapKnockoutPlayerLightRigConfig : ScriptableObject
    {
        [Header("Follow")]
        [SerializeField, Min(0f)] private float followSharpness = 28f;
        [SerializeField, Min(0f)] private float forwardDirectionSharpness = 18f;
        [SerializeField, Min(0f)] private float movementDirectionDeadZone = 0.05f;

        [Header("Hero Local Light")]
        [SerializeField] private bool localHeroLightEnabled;
        [SerializeField] private Color localHeroLightColor = new Color(0.72f, 1f, 0.84f, 1f);
        [SerializeField, Range(0f, 3f)] private float localHeroLightIntensity = 0.35f;
        [SerializeField, Range(0.5f, 12f)] private float localHeroLightRange = 4.5f;
        [SerializeField] private Vector3 localHeroLightOffset = new Vector3(0f, 2.65f, 0f);

        [Header("Forward Movement Light")]
        [SerializeField] private bool forwardLightEnabled;
        [SerializeField] private Color forwardLightColor = new Color(0.5f, 1f, 0.76f, 1f);
        [SerializeField, Range(0f, 2f)] private float forwardLightIntensity = 0.16f;
        [SerializeField, Range(0f, 1f)] private float forwardLightIdleIntensityMultiplier = 0f;
        [SerializeField, Range(0.5f, 14f)] private float forwardLightRange = 5.5f;
        [SerializeField, Range(20f, 85f)] private float forwardLightSpotAngle = 80f;
        [SerializeField, Range(15f, 80f)] private float forwardLightDownAngle = 48f;
        [SerializeField] private Vector3 forwardLightOffset = new Vector3(0f, 2.15f, 0.9f);

        [Header("Dash Pulse")]
        [SerializeField] private bool dashPulseEnabled = true;
        [SerializeField] private Color dashPulseColor = new Color(0.78f, 0.92f, 0.9f, 1f);
        [SerializeField, Range(0f, 5f)] private float dashPulseIntensity = 2.4f;
        [SerializeField, Range(0.5f, 14f)] private float dashPulseRange = 8.5f;
        [SerializeField, Range(0.03f, 0.4f)] private float dashPulseDuration = 0.18f;
        [SerializeField] private Vector3 dashPulseOffset = new Vector3(0f, 2.35f, 1.25f);

        [Header("Visible Player Glow")]
        [SerializeField] private bool visibleGlowEnabled = true;
        [SerializeField] private Color groundGlowColor = new Color(0.74f, 0.92f, 0.9f, 0.18f);
        [SerializeField, Range(0.2f, 3f)] private float groundGlowRadius = 1.45f;
        [SerializeField] private Color coreGlowColor = new Color(0.86f, 0.96f, 0.92f, 0.48f);
        [SerializeField] private Vector2 coreGlowSize = new Vector2(0.92f, 1.45f);
        [SerializeField] private Vector3 coreGlowOffset = new Vector3(0f, 0.92f, 0f);
        [SerializeField, Range(0f, 0.5f)] private float glowPulseAmplitude = 0.045f;
        [SerializeField, Range(0.05f, 8f)] private float glowPulseFrequency = 0.85f;

        [Header("Lantern Field")]
        [SerializeField] private bool lanternFieldEnabled = true;
        [SerializeField] private Color lanternFieldColor = new Color(0.66f, 0.82f, 0.84f, 0.08f);
        [SerializeField, Range(1f, 10f)] private float lanternFieldRadius = 4.6f;
        [SerializeField] private Vector3 lanternFieldOffset = new Vector3(0f, 0.045f, 0f);
        [SerializeField] private bool forwardLanternEnabled;
        [SerializeField] private Color forwardLanternColor = new Color(0.78f, 1f, 0.66f, 0.12f);
        [SerializeField, Range(1f, 12f)] private float forwardLanternRange = 5.2f;
        [SerializeField, Range(0.4f, 8f)] private float forwardLanternWidth = 3.4f;
        [SerializeField] private Vector3 forwardLanternOffset = new Vector3(0f, 0.055f, 0.25f);
        [SerializeField, Range(0f, 1f)] private float forwardLanternIdleAlphaMultiplier = 0.35f;

        public float FollowSharpness => followSharpness;
        public float ForwardDirectionSharpness => forwardDirectionSharpness;
        public float MovementDirectionDeadZone => movementDirectionDeadZone;
        public bool LocalHeroLightEnabled => localHeroLightEnabled;
        public Color LocalHeroLightColor => localHeroLightColor;
        public float LocalHeroLightIntensity => localHeroLightIntensity;
        public float LocalHeroLightRange => localHeroLightRange;
        public Vector3 LocalHeroLightOffset => localHeroLightOffset;
        public bool ForwardLightEnabled => forwardLightEnabled;
        public Color ForwardLightColor => forwardLightColor;
        public float ForwardLightIntensity => forwardLightIntensity;
        public float ForwardLightIdleIntensityMultiplier => forwardLightIdleIntensityMultiplier;
        public float ForwardLightRange => forwardLightRange;
        public float ForwardLightSpotAngle => forwardLightSpotAngle;
        public float ForwardLightDownAngle => forwardLightDownAngle;
        public Vector3 ForwardLightOffset => forwardLightOffset;
        public bool DashPulseEnabled => dashPulseEnabled;
        public Color DashPulseColor => dashPulseColor;
        public float DashPulseIntensity => dashPulseIntensity;
        public float DashPulseRange => dashPulseRange;
        public float DashPulseDuration => dashPulseDuration;
        public Vector3 DashPulseOffset => dashPulseOffset;
        public bool VisibleGlowEnabled => visibleGlowEnabled;
        public Color GroundGlowColor => groundGlowColor;
        public float GroundGlowRadius => groundGlowRadius;
        public Color CoreGlowColor => coreGlowColor;
        public Vector2 CoreGlowSize => coreGlowSize;
        public Vector3 CoreGlowOffset => coreGlowOffset;
        public float GlowPulseAmplitude => glowPulseAmplitude;
        public float GlowPulseFrequency => glowPulseFrequency;
        public bool LanternFieldEnabled => lanternFieldEnabled;
        public Color LanternFieldColor => lanternFieldColor;
        public float LanternFieldRadius => lanternFieldRadius;
        public Vector3 LanternFieldOffset => lanternFieldOffset;
        public bool ForwardLanternEnabled => forwardLanternEnabled;
        public Color ForwardLanternColor => forwardLanternColor;
        public float ForwardLanternRange => forwardLanternRange;
        public float ForwardLanternWidth => forwardLanternWidth;
        public Vector3 ForwardLanternOffset => forwardLanternOffset;
        public float ForwardLanternIdleAlphaMultiplier => forwardLanternIdleAlphaMultiplier;

        private void OnValidate()
        {
            followSharpness = Mathf.Max(0f, followSharpness);
            forwardDirectionSharpness = Mathf.Max(0f, forwardDirectionSharpness);
            movementDirectionDeadZone = Mathf.Max(0f, movementDirectionDeadZone);
            localHeroLightIntensity = Mathf.Clamp(localHeroLightIntensity, 0f, 3f);
            localHeroLightRange = Mathf.Clamp(localHeroLightRange, 0.5f, 12f);
            forwardLightIntensity = Mathf.Clamp(forwardLightIntensity, 0f, 2f);
            forwardLightIdleIntensityMultiplier = Mathf.Clamp01(forwardLightIdleIntensityMultiplier);
            forwardLightRange = Mathf.Clamp(forwardLightRange, 0.5f, 14f);
            forwardLightSpotAngle = Mathf.Clamp(forwardLightSpotAngle, 20f, 85f);
            forwardLightDownAngle = Mathf.Clamp(forwardLightDownAngle, 15f, 80f);
            dashPulseIntensity = Mathf.Clamp(dashPulseIntensity, 0f, 5f);
            dashPulseRange = Mathf.Clamp(dashPulseRange, 0.5f, 14f);
            dashPulseDuration = Mathf.Clamp(dashPulseDuration, 0.03f, 0.4f);
            groundGlowRadius = Mathf.Clamp(groundGlowRadius, 0.2f, 3f);
            coreGlowSize = new Vector2(Mathf.Max(0.1f, coreGlowSize.x), Mathf.Max(0.1f, coreGlowSize.y));
            glowPulseAmplitude = Mathf.Clamp(glowPulseAmplitude, 0f, 0.5f);
            glowPulseFrequency = Mathf.Clamp(glowPulseFrequency, 0.05f, 8f);
            lanternFieldRadius = Mathf.Clamp(lanternFieldRadius, 1f, 10f);
            forwardLanternRange = Mathf.Clamp(forwardLanternRange, 1f, 12f);
            forwardLanternWidth = Mathf.Clamp(forwardLanternWidth, 0.4f, 8f);
            forwardLanternIdleAlphaMultiplier = Mathf.Clamp01(forwardLanternIdleAlphaMultiplier);
        }
    }
}
