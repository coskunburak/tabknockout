using System;
using UnityEngine;

namespace TapKnockout.VFX
{
    [Serializable]
    public sealed class VFXDefinition
    {
        [SerializeField] private VFXEventType eventType = VFXEventType.GenericBurst;
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(0)] private int initialPoolSize = 2;
        [SerializeField, Min(0f)] private float defaultLifetime = 1f;
        [SerializeField] private bool parentToRequestParent;
        [SerializeField] private bool useRequestRotation = true;
        [SerializeField] private bool useRequestScale = true;
        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 rotationOffsetEuler;
        [SerializeField, Min(0f)] private float scaleMultiplier = 1f;
        [SerializeField] private bool allowColorOverride = true;

        public VFXDefinition()
        {
        }

        public VFXDefinition(
            VFXEventType eventType,
            GameObject prefab,
            int initialPoolSize = 2,
            float defaultLifetime = 1f)
        {
            this.eventType = eventType;
            this.prefab = prefab;
            this.initialPoolSize = Mathf.Max(0, initialPoolSize);
            this.defaultLifetime = Mathf.Max(0f, defaultLifetime);
            scaleMultiplier = 1f;
            useRequestRotation = true;
            useRequestScale = true;
            allowColorOverride = true;
        }

        public VFXDefinition(
            VFXEventType eventType,
            GameObject prefab,
            int initialPoolSize,
            float defaultLifetime,
            bool parentToRequestParent,
            bool useRequestRotation,
            bool useRequestScale,
            Vector3 positionOffset,
            Vector3 rotationOffsetEuler,
            float scaleMultiplier,
            bool allowColorOverride)
        {
            this.eventType = eventType;
            this.prefab = prefab;
            this.initialPoolSize = Mathf.Max(0, initialPoolSize);
            this.defaultLifetime = Mathf.Max(0f, defaultLifetime);
            this.parentToRequestParent = parentToRequestParent;
            this.useRequestRotation = useRequestRotation;
            this.useRequestScale = useRequestScale;
            this.positionOffset = positionOffset;
            this.rotationOffsetEuler = rotationOffsetEuler;
            this.scaleMultiplier = Mathf.Max(0f, scaleMultiplier);
            this.allowColorOverride = allowColorOverride;
        }

        public VFXEventType EventType => eventType;
        public GameObject Prefab => prefab;
        public int InitialPoolSize => initialPoolSize;
        public float DefaultLifetime => defaultLifetime;
        public bool ParentToRequestParent => parentToRequestParent;
        public bool UseRequestRotation => useRequestRotation;
        public bool UseRequestScale => useRequestScale;
        public Vector3 PositionOffset => positionOffset;
        public Vector3 RotationOffsetEuler => rotationOffsetEuler;
        public float ScaleMultiplier => scaleMultiplier;
        public bool AllowColorOverride => allowColorOverride;
        public bool HasPrefab => prefab != null;

        public void ClampValues()
        {
            initialPoolSize = Mathf.Max(0, initialPoolSize);
            defaultLifetime = Mathf.Max(0f, defaultLifetime);
            scaleMultiplier = Mathf.Max(0f, scaleMultiplier);
        }
    }
}
