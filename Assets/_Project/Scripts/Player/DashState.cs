using UnityEngine;

namespace TapKnockout.Player
{
    public sealed class DashState
    {
        public bool IsDashing { get; private set; }
        public bool IsIFrameActive { get; private set; }
        public float CooldownRemaining { get; private set; }
        public float DashElapsed { get; private set; }
        public float DashDuration { get; private set; }
        public float DashCooldown { get; private set; }
        public float IFrameDuration { get; private set; }
        public float IFrameRemaining { get; private set; }

        public bool CanDash => !IsDashing && CooldownRemaining <= 0f;
        public float DashRemaining => IsDashing ? Mathf.Max(0f, DashDuration - DashElapsed) : 0f;
        public float NormalizedCooldown => DashCooldown <= 0f
            ? 0f
            : Mathf.Clamp01(CooldownRemaining / DashCooldown);

        public bool TryBegin(float duration, float cooldown, bool hasIFrames, float iFrameDuration)
        {
            if (!CanDash)
            {
                return false;
            }

            DashDuration = Mathf.Max(0.01f, duration);
            DashCooldown = Mathf.Max(0f, cooldown);
            IFrameDuration = hasIFrames ? Mathf.Clamp(iFrameDuration, 0f, DashDuration) : 0f;
            IFrameRemaining = IFrameDuration;
            DashElapsed = 0f;
            CooldownRemaining = DashCooldown;
            IsDashing = true;
            IsIFrameActive = IFrameRemaining > 0f;
            return true;
        }

        public void Tick(float deltaTime, out bool dashEnded, out bool iFrameEnded)
        {
            dashEnded = false;
            iFrameEnded = false;
            deltaTime = Mathf.Max(0f, deltaTime);

            if (CooldownRemaining > 0f)
            {
                CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaTime);
            }

            if (IsIFrameActive)
            {
                IFrameRemaining = Mathf.Max(0f, IFrameRemaining - deltaTime);
                if (IFrameRemaining <= 0f)
                {
                    IsIFrameActive = false;
                    iFrameEnded = true;
                }
            }

            if (!IsDashing)
            {
                return;
            }

            DashElapsed += deltaTime;
            if (DashElapsed < DashDuration)
            {
                return;
            }

            IsDashing = false;
            dashEnded = true;

            if (IsIFrameActive)
            {
                IsIFrameActive = false;
                IFrameRemaining = 0f;
                iFrameEnded = true;
            }
        }

        public void ForceEnd(out bool iFrameEnded)
        {
            iFrameEnded = IsIFrameActive;
            IsDashing = false;
            IsIFrameActive = false;
            IFrameRemaining = 0f;
        }
    }
}
