using UnityEngine;

namespace TapKnockout.Feedback
{
    [DisallowMultipleComponent]
    public sealed class HitPauseService : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.12f)] private float maxPauseDuration = 0.08f;
        [SerializeField, Range(0f, 0.25f)] private float defaultRequestCooldown = 0.04f;

        private bool ownsPause;
        private float remainingDuration;
        private float restoreTimeScale = 1f;
        private float ownedPauseTimeScale;
        private float lastPauseRequestRealtime = -999f;

        public bool IsPauseActive => ownsPause;
        public float RemainingDuration => remainingDuration;

        private void OnValidate()
        {
            defaultRequestCooldown = Mathf.Max(0f, defaultRequestCooldown);
        }

        private void Update()
        {
            Tick(Time.unscaledDeltaTime);
        }

        private void OnDisable()
        {
            RestoreIfOwned();
        }

        public bool RequestHitPause(float duration)
        {
            return RequestHitPause(duration, defaultRequestCooldown, 0f);
        }

        public bool RequestHitPause(float duration, float cooldown)
        {
            return RequestHitPause(duration, cooldown, 0f);
        }

        public bool RequestHitPause(float duration, float cooldown, float pauseTimeScale)
        {
            var requestedDuration = Mathf.Clamp(duration, 0f, maxPauseDuration);
            if (requestedDuration <= 0f)
            {
                return false;
            }

            var now = Time.unscaledTime;
            var resolvedCooldown = cooldown >= 0f ? cooldown : defaultRequestCooldown;
            if (resolvedCooldown > 0f && now - lastPauseRequestRealtime < resolvedCooldown)
            {
                return false;
            }

            if (!ownsPause && Time.timeScale <= 0f)
            {
                return false;
            }

            if (!ownsPause)
            {
                restoreTimeScale = Time.timeScale;
                ownedPauseTimeScale = Mathf.Clamp(pauseTimeScale, 0f, 0.15f);
                Time.timeScale = ownedPauseTimeScale;
                ownsPause = true;
            }

            lastPauseRequestRealtime = now;
            remainingDuration = Mathf.Max(remainingDuration, requestedDuration);
            return true;
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (!ownsPause)
            {
                return;
            }

            remainingDuration -= Mathf.Max(0f, unscaledDeltaTime);
            if (remainingDuration > 0f)
            {
                return;
            }

            RestoreIfOwned();
        }

        public void RestoreIfOwned()
        {
            if (!ownsPause)
            {
                return;
            }

            if (Mathf.Approximately(Time.timeScale, ownedPauseTimeScale))
            {
                Time.timeScale = Mathf.Max(0.0001f, restoreTimeScale);
            }

            ownsPause = false;
            remainingDuration = 0f;
            ownedPauseTimeScale = 0f;
        }
    }
}
