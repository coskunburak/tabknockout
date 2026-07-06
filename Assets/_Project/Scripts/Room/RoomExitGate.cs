using System;
using UnityEngine;

namespace TapKnockout.Room
{
    [DisallowMultipleComponent]
    public sealed class RoomExitGate : MonoBehaviour
    {
        [SerializeField] private Collider blockingCollider;
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject unlockedVisual;
        [SerializeField] private bool startsLocked = true;
        [SerializeField] private bool logDebug;

        public event Action<RoomExitGate> OnGateLocked;
        public event Action<RoomExitGate> OnGateUnlocked;

        public bool IsLocked { get; private set; }

        private void Reset()
        {
            blockingCollider = GetComponent<Collider>();
        }

        private void Awake()
        {
            SetLocked(startsLocked, false);
        }

        public void SetReferences(Collider blocker, GameObject lockedStateVisual, GameObject unlockedStateVisual)
        {
            blockingCollider = blocker;
            lockedVisual = lockedStateVisual;
            unlockedVisual = unlockedStateVisual;
            ApplyState();
        }

        public void Lock()
        {
            SetLocked(true, true);
        }

        public void Unlock()
        {
            SetLocked(false, true);
        }

        private void SetLocked(bool locked, bool notify)
        {
            if (IsLocked == locked && notify)
            {
                return;
            }

            IsLocked = locked;
            ApplyState();

            if (notify)
            {
                if (IsLocked)
                {
                    OnGateLocked?.Invoke(this);
                }
                else
                {
                    OnGateUnlocked?.Invoke(this);
                }
            }

            if (logDebug)
            {
                Debug.Log($"{nameof(RoomExitGate)} {name} {(IsLocked ? "locked" : "unlocked")}.", this);
            }
        }

        private void ApplyState()
        {
            if (blockingCollider != null)
            {
                blockingCollider.enabled = IsLocked;
            }

            if (lockedVisual != null)
            {
                lockedVisual.SetActive(IsLocked);
            }

            if (unlockedVisual != null)
            {
                unlockedVisual.SetActive(!IsLocked);
            }
        }
    }
}
