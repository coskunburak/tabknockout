using System;
using System.Collections;
using UnityEngine;

namespace TapKnockout.Room
{
    [DisallowMultipleComponent]
    public sealed class RoomInstanceController : MonoBehaviour
    {
        [SerializeField] private RoomPrefabContract contract;
        [SerializeField] private bool lockExitsOnStart = true;
        [SerializeField] private bool logDebug;

        public event Action<RoomInstanceController> OnRoomStartLocked;
        public event Action<RoomInstanceController> OnRoomExitUnlocked;

        public RoomPrefabContract Contract => contract;
        public bool HasContract => contract != null;

        private void Reset()
        {
            contract = GetComponent<RoomPrefabContract>();
        }

        private void Awake()
        {
            if (contract == null)
            {
                contract = GetComponent<RoomPrefabContract>();
            }
        }

        public void Initialize(RoomPrefabContract roomContract)
        {
            contract = roomContract;
        }

        public void LockExitsAtRoomStart(RoomTemplateConfig roomConfig)
        {
            if (!lockExitsOnStart || roomConfig == null || !roomConfig.LockExitsUntilCleared || contract == null)
            {
                return;
            }

            var gates = contract.GetExitGates();
            for (var i = 0; i < gates.Count; i++)
            {
                gates[i]?.Lock();
            }

            OnRoomStartLocked?.Invoke(this);

            if (logDebug)
            {
                Debug.Log($"{nameof(RoomInstanceController)} locked exits for {roomConfig.RoomId}.", this);
            }
        }

        public void UnlockExitsOnRoomClear(RoomTemplateConfig roomConfig)
        {
            if (contract == null)
            {
                return;
            }

            var gates = contract.GetExitGates();
            for (var i = 0; i < gates.Count; i++)
            {
                gates[i]?.Unlock();
            }

            OnRoomExitUnlocked?.Invoke(this);

            if (logDebug)
            {
                Debug.Log($"{nameof(RoomInstanceController)} unlocked exits for {roomConfig?.RoomId ?? "<null>"}.", this);
            }
        }

        public IEnumerator PlayRoomStartIntro(RoomTemplateConfig roomConfig)
        {
            if (contract == null)
            {
                yield break;
            }

            var behaviours = contract.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IRoomStartIntro intro || !intro.IsIntroEnabled)
                {
                    continue;
                }

                yield return intro.PlayIntro(roomConfig, contract);
            }
        }

        public Transform ResolveRewardSpawnPoint()
        {
            return contract != null ? contract.GetRewardSpawnPoint() : null;
        }

        public Transform ResolveBossSpawnPoint()
        {
            return contract != null ? contract.GetBossSpawnPoint() : null;
        }
    }
}
