using System;
using UnityEngine;

namespace TapKnockout.VFX
{
    [Serializable]
    public struct VFXSpawnRequest
    {
        public VFXEventType EventType;
        public Vector3 Position;
        public Quaternion Rotation;
        public Transform Parent;
        public Vector3 Scale;
        public float LifetimeOverride;
        public Color ColorOverride;
        public float Intensity;
        public GameObject Source;
        public GameObject Target;

        public VFXSpawnRequest(VFXEventType eventType, Vector3 position)
        {
            EventType = eventType;
            Position = position;
            Rotation = Quaternion.identity;
            Parent = null;
            Scale = Vector3.one;
            LifetimeOverride = 0f;
            ColorOverride = Color.clear;
            Intensity = 1f;
            Source = null;
            Target = null;
        }

        public static VFXSpawnRequest Create(VFXEventType eventType, Vector3 position)
        {
            return new VFXSpawnRequest(eventType, position);
        }

        public Quaternion EffectiveRotation => IsDefaultQuaternion(Rotation) ? Quaternion.identity : Rotation;
        public Vector3 EffectiveScale => Scale == Vector3.zero ? Vector3.one : Scale;
        public float EffectiveIntensity => Intensity > 0f ? Intensity : 1f;
        public bool HasLifetimeOverride => LifetimeOverride > 0f;
        public bool HasColorOverride => ColorOverride.a > 0f;

        private static bool IsDefaultQuaternion(Quaternion value)
        {
            return Mathf.Approximately(value.x, 0f)
                && Mathf.Approximately(value.y, 0f)
                && Mathf.Approximately(value.z, 0f)
                && Mathf.Approximately(value.w, 0f);
        }
    }
}
