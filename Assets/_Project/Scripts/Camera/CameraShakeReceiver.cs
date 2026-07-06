using UnityEngine;

namespace TapKnockout.Camera
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class CameraShakeReceiver : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool useOwnTransformWhenMissing = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField, Range(0f, 0.3f)] private float maxAmplitude = 0.12f;
        [SerializeField, Range(0f, 0.4f)] private float maxDuration = 0.18f;

        private Vector3 appliedLocalOffset;
        private Vector3 expectedLocalPositionWithOffset;
        private float remainingDuration;
        private float totalDuration;
        private float amplitude;
        private bool hasAppliedOffset;

        public bool IsShaking => remainingDuration > 0f;
        public float RemainingDuration => remainingDuration;
        public float MaxAmplitude => maxAmplitude;
        public float MaxDuration => maxDuration;

        private void Reset()
        {
            targetTransform = transform;
        }

        private void LateUpdate()
        {
            Tick(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        private void OnDisable()
        {
            RemoveAppliedOffsetIfStillApplied();
            remainingDuration = 0f;
            totalDuration = 0f;
            amplitude = 0f;
        }

        public void Shake(float requestedAmplitude, float requestedDuration)
        {
            var clampedAmplitude = Mathf.Clamp(requestedAmplitude, 0f, maxAmplitude);
            var clampedDuration = Mathf.Clamp(requestedDuration, 0f, maxDuration);
            if (clampedAmplitude <= 0f || clampedDuration <= 0f)
            {
                return;
            }

            amplitude = Mathf.Max(amplitude, clampedAmplitude);
            remainingDuration = Mathf.Max(remainingDuration, clampedDuration);
            totalDuration = Mathf.Max(totalDuration, remainingDuration);
        }

        public void Tick(float deltaTime)
        {
            var target = ResolveTarget();
            if (target == null)
            {
                return;
            }

            RemoveAppliedOffsetIfStillApplied();

            if (remainingDuration <= 0f)
            {
                return;
            }

            remainingDuration = Mathf.Max(0f, remainingDuration - Mathf.Max(0f, deltaTime));
            var normalizedRemaining = totalDuration > 0f ? Mathf.Clamp01(remainingDuration / totalDuration) : 0f;
            var decay = normalizedRemaining * normalizedRemaining;
            var randomOffset = Random.insideUnitCircle * (amplitude * decay);
            appliedLocalOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
            expectedLocalPositionWithOffset = target.localPosition + appliedLocalOffset;
            target.localPosition = expectedLocalPositionWithOffset;
            hasAppliedOffset = true;

            if (remainingDuration <= 0f)
            {
                amplitude = 0f;
                totalDuration = 0f;
            }
        }

        private Transform ResolveTarget()
        {
            if (targetTransform != null)
            {
                return targetTransform;
            }

            return useOwnTransformWhenMissing ? transform : null;
        }

        private void RemoveAppliedOffsetIfStillApplied()
        {
            if (!hasAppliedOffset)
            {
                return;
            }

            var target = ResolveTarget();
            if (target != null)
            {
                var delta = target.localPosition - expectedLocalPositionWithOffset;
                if (delta.sqrMagnitude <= 0.0001f)
                {
                    target.localPosition -= appliedLocalOffset;
                }
            }

            appliedLocalOffset = Vector3.zero;
            expectedLocalPositionWithOffset = Vector3.zero;
            hasAppliedOffset = false;
        }
    }
}
