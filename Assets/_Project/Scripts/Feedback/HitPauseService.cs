using UnityEngine;

namespace TapKnockout.Feedback
{
    [DisallowMultipleComponent]
    public sealed class HitPauseService : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.12f)] private float maxPauseDuration = 0.08f;

        private bool ownsPause;
        private float remainingDuration;
        private float restoreTimeScale = 1f;

        public bool IsPauseActive => ownsPause;
        public float RemainingDuration => remainingDuration;

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
            var requestedDuration = Mathf.Clamp(duration, 0f, maxPauseDuration);
            if (requestedDuration <= 0f)
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
                Time.timeScale = 0f;
                ownsPause = true;
            }

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

            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                Time.timeScale = Mathf.Max(0.0001f, restoreTimeScale);
            }

            ownsPause = false;
            remainingDuration = 0f;
        }
    }
}
