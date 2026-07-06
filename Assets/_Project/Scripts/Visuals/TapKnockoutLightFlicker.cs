using UnityEngine;

namespace TapKnockout.Visuals
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public sealed class TapKnockoutLightFlicker : MonoBehaviour
    {
        [SerializeField] private Light targetLight;
        [SerializeField, Min(0f)] private float baseIntensity = 2.2f;
        [SerializeField, Range(0f, 1f)] private float amplitude = 0.12f;
        [SerializeField, Range(0.05f, 12f)] private float frequency = 1.8f;
        [SerializeField] private float phase = 0.37f;
        [SerializeField] private bool useUnscaledTime;

        private void Reset()
        {
            targetLight = GetComponent<Light>();
            if (targetLight != null)
            {
                baseIntensity = targetLight.intensity;
            }
        }

        private void Awake()
        {
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
            }

            if (targetLight != null && baseIntensity <= 0f)
            {
                baseIntensity = targetLight.intensity;
            }
        }

        private void OnValidate()
        {
            baseIntensity = Mathf.Max(0f, baseIntensity);
            amplitude = Mathf.Clamp01(amplitude);
            frequency = Mathf.Clamp(frequency, 0.05f, 12f);
        }

        private void Update()
        {
            if (targetLight == null || baseIntensity <= 0f || amplitude <= 0f)
            {
                return;
            }

            var time = useUnscaledTime ? Time.unscaledTime : Time.time;
            var wave = Mathf.Sin((time + phase) * frequency * Mathf.PI * 2f) * 0.5f + 0.5f;
            var secondary = Mathf.Sin((time * 0.37f + phase * 1.7f) * frequency * Mathf.PI * 2f) * 0.5f + 0.5f;
            var multiplier = 1f + ((wave * 0.7f + secondary * 0.3f) * 2f - 1f) * amplitude;
            targetLight.intensity = baseIntensity * multiplier;
        }

        public void Configure(float intensity, float flickerAmplitude, float flickerFrequency)
        {
            baseIntensity = Mathf.Max(0f, intensity);
            amplitude = Mathf.Clamp01(flickerAmplitude);
            frequency = Mathf.Clamp(flickerFrequency, 0.05f, 12f);
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
            }
        }
    }
}
