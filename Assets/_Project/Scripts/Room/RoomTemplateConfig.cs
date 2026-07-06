using System.Collections.Generic;
using TapKnockout.Wave;
using UnityEngine;

namespace TapKnockout.Room
{
    [CreateAssetMenu(fileName = "RoomTemplateConfig", menuName = "Tap Knockout/Rooms/Room Template Config")]
    public sealed class RoomTemplateConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string roomId = "room_001";
        [SerializeField] private RoomType roomType = RoomType.Combat;

        [Header("Waves")]
        [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>();
        [SerializeField, Min(0f)] private float startDelay = 0.5f;

        [Header("Room Hooks")]
        [SerializeField] private bool lockExitsUntilCleared = true;
        [SerializeField] private GameObject roomPrefab;

        [Header("Rewards")]
        [SerializeField] private RoomRewardType rewardType = RoomRewardType.None;
        [SerializeField] private bool autoAdvanceAfterClear = true;
        [SerializeField] private bool grantsAbilityReward;
        [SerializeField] private bool grantsHealReward;

        [Header("Future Placeholders")]
        [SerializeField] private bool isBossRoom;
        [SerializeField] private string environmentThemeId = "theme_grass_01";

        public string RoomId => roomId;
        public RoomType RoomType => roomType;
        public RoomRewardType RewardType => rewardType;
        public IReadOnlyList<WaveConfig> Waves => waves;
        public float StartDelay => startDelay;
        public bool LockExitsUntilCleared => lockExitsUntilCleared;
        public GameObject RoomPrefab => roomPrefab;
        public bool IsBossRoom => isBossRoom || roomType == RoomType.Boss;
        public bool GrantsAbilityReward => grantsAbilityReward || rewardType == RoomRewardType.Ability || roomType == RoomType.AbilityReward;
        public bool GrantsHealReward => grantsHealReward || rewardType == RoomRewardType.Heal || roomType == RoomType.Heal;
        public bool AutoAdvanceAfterClear => autoAdvanceAfterClear;
        public string EnvironmentThemeId => environmentThemeId;
        public bool HasWaves => waves != null && waves.Count > 0;
        public bool HasRoomPrefab => roomPrefab != null;

        public bool TryGetRoomPrefabContract(out RoomPrefabContract contract)
        {
            contract = roomPrefab != null ? roomPrefab.GetComponentInChildren<RoomPrefabContract>(true) : null;
            return contract != null;
        }

        public bool HasValidRoomPrefabReference()
        {
            return roomPrefab == null || roomPrefab.GetComponentInChildren<RoomPrefabContract>(true) != null;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                roomId = "room_001";
            }

            if (string.IsNullOrWhiteSpace(environmentThemeId))
            {
                environmentThemeId = "theme_grass_01";
            }

            startDelay = Mathf.Max(0f, startDelay);
            waves ??= new List<WaveConfig>();
            isBossRoom = isBossRoom || roomType == RoomType.Boss;

            if (roomType == RoomType.AbilityReward && rewardType == RoomRewardType.None)
            {
                rewardType = RoomRewardType.Ability;
            }

            if (roomType == RoomType.Boss && rewardType == RoomRewardType.None)
            {
                rewardType = RoomRewardType.BossClear;
            }

            if (roomPrefab != null && roomPrefab.GetComponentInChildren<RoomPrefabContract>(true) == null)
            {
                Debug.LogWarning($"{name} references a room prefab without {nameof(RoomPrefabContract)}: {roomPrefab.name}", this);
            }
        }
    }
}
