using System.Collections.Generic;
using UnityEngine;

namespace TapKnockout.Visuals
{
    [CreateAssetMenu(fileName = "TapKnockoutLightingConfig", menuName = "Tap Knockout/Visuals/Lighting Config")]
    public sealed class TapKnockoutLightingConfig : ScriptableObject
    {
        [Header("Main Light")]
        [SerializeField] private Color mainLightColor = new Color(0.56f, 0.68f, 0.78f, 1f);
        [SerializeField, Range(0f, 3f)] private float mainLightIntensity = 0.52f;
        [SerializeField] private Vector3 mainLightEuler = new Vector3(54f, -42f, 0f);
        [SerializeField, Range(0f, 1f)] private float mainLightShadowStrength = 0.55f;
        [SerializeField] private bool mainLightSoftShadows = true;

        [Header("Ambient")]
        [SerializeField] private Color ambientSkyColor = new Color(0.052f, 0.066f, 0.072f, 1f);
        [SerializeField] private Color ambientEquatorColor = new Color(0.028f, 0.038f, 0.034f, 1f);
        [SerializeField] private Color ambientGroundColor = new Color(0.014f, 0.016f, 0.014f, 1f);
        [SerializeField, Range(0f, 2f)] private float ambientIntensity = 0.22f;

        [Header("Fog")]
        [SerializeField] private bool fogEnabled = true;
        [SerializeField] private Color fogColor = new Color(0.026f, 0.038f, 0.036f, 1f);
        [SerializeField, Range(0f, 0.03f)] private float fogDensity = 0.011f;

        [Header("Accent Lights")]
        [SerializeField, Range(0, 8)] private int maxRuntimeAccentLights = 4;
        [SerializeField] private List<TapKnockoutLightingAccent> accentLights = new List<TapKnockoutLightingAccent>
        {
            new TapKnockoutLightingAccent("north_west_torch", new Vector3(-16f, 2.2f, 16f), new Color(1f, 0.46f, 0.2f, 1f), 1.25f, 4.8f),
            new TapKnockoutLightingAccent("north_east_torch", new Vector3(16f, 2.2f, 16f), new Color(1f, 0.46f, 0.2f, 1f), 1.25f, 4.8f),
            new TapKnockoutLightingAccent("south_west_torch", new Vector3(-16f, 2.2f, -16f), new Color(1f, 0.46f, 0.2f, 1f), 1.25f, 4.8f),
            new TapKnockoutLightingAccent("south_east_torch", new Vector3(16f, 2.2f, -16f), new Color(1f, 0.46f, 0.2f, 1f), 1.25f, 4.8f)
        };

        public Color MainLightColor => mainLightColor;
        public float MainLightIntensity => mainLightIntensity;
        public Vector3 MainLightEuler => mainLightEuler;
        public float MainLightShadowStrength => mainLightShadowStrength;
        public bool MainLightSoftShadows => mainLightSoftShadows;
        public Color AmbientSkyColor => ambientSkyColor;
        public Color AmbientEquatorColor => ambientEquatorColor;
        public Color AmbientGroundColor => ambientGroundColor;
        public float AmbientIntensity => ambientIntensity;
        public bool FogEnabled => fogEnabled;
        public Color FogColor => fogColor;
        public float FogDensity => fogDensity;
        public int MaxRuntimeAccentLights => maxRuntimeAccentLights;
        public IReadOnlyList<TapKnockoutLightingAccent> AccentLights => accentLights;

        private void OnValidate()
        {
            mainLightIntensity = Mathf.Clamp(mainLightIntensity, 0f, 3f);
            mainLightShadowStrength = Mathf.Clamp01(mainLightShadowStrength);
            ambientIntensity = Mathf.Clamp(ambientIntensity, 0f, 2f);
            fogDensity = Mathf.Clamp(fogDensity, 0f, 0.03f);
            maxRuntimeAccentLights = Mathf.Clamp(maxRuntimeAccentLights, 0, 8);
            accentLights ??= new List<TapKnockoutLightingAccent>();
            for (var i = 0; i < accentLights.Count; i++)
            {
                accentLights[i]?.ClampValues();
            }
        }
    }
}
