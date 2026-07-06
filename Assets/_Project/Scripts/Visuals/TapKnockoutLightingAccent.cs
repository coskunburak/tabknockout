using System;
using UnityEngine;

namespace TapKnockout.Visuals
{
    [Serializable]
    public sealed class TapKnockoutLightingAccent
    {
        [SerializeField] private string id = "torch_accent";
        [SerializeField] private Vector3 localPosition;
        [SerializeField] private Color color = new Color(1f, 0.58f, 0.28f, 1f);
        [SerializeField, Range(0f, 8f)] private float intensity = 2.2f;
        [SerializeField, Range(0.5f, 16f)] private float range = 6f;
        [SerializeField] private bool castsShadows;

        public string Id => id;
        public Vector3 LocalPosition => localPosition;
        public Color Color => color;
        public float Intensity => intensity;
        public float Range => range;
        public bool CastsShadows => castsShadows;

        public TapKnockoutLightingAccent()
        {
        }

        public TapKnockoutLightingAccent(string id, Vector3 localPosition)
        {
            this.id = string.IsNullOrWhiteSpace(id) ? "torch_accent" : id;
            this.localPosition = localPosition;
        }

        public TapKnockoutLightingAccent(
            string id,
            Vector3 localPosition,
            Color color,
            float intensity,
            float range,
            bool castsShadows = false)
        {
            this.id = string.IsNullOrWhiteSpace(id) ? "torch_accent" : id;
            this.localPosition = localPosition;
            this.color = color;
            this.intensity = intensity;
            this.range = range;
            this.castsShadows = castsShadows;
            ClampValues();
        }

        public void ClampValues()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = "torch_accent";
            }

            intensity = Mathf.Clamp(intensity, 0f, 8f);
            range = Mathf.Clamp(range, 0.5f, 16f);
        }
    }
}
