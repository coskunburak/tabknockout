using UnityEngine;

namespace TapKnockout.Camera
{
    [DisallowMultipleComponent]
    public sealed class TapKnockoutCameraTarget : MonoBehaviour
    {
        [SerializeField] private bool isPrimaryGameplayTarget = true;
        [SerializeField] private bool isCinematicOverrideTarget;
        [SerializeField, Range(-10, 10)] private int priority;

        public bool IsPrimaryGameplayTarget => isPrimaryGameplayTarget;
        public bool IsCinematicOverrideTarget => isCinematicOverrideTarget;
        public int Priority => priority;
    }
}
