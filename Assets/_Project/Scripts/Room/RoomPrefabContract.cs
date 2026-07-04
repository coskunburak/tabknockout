using System.Collections.Generic;
using TapKnockout.Camera;
using UnityEngine;

namespace TapKnockout.Room
{
    [DisallowMultipleComponent]
    public sealed class RoomPrefabContract : MonoBehaviour
    {
        public const string RoomRootName = "RoomRoot";
        public const string ArenaRootName = "Arena";
        public const string PlayerSpawnName = "PlayerSpawn";
        public const string EnemySpawnPointsRootName = "EnemySpawnPoints";
        public const string ExitGateName = "ExitGate";
        public const string CameraBoundsName = "CameraBounds";
        public const string HazardRootName = "HazardRoot";
        public const string RewardSpawnRootName = "RewardSpawnRoot";
        public const string PlayerExitPointName = "PlayerExitPoint";
        public const string RewardSpawnPointName = "RewardSpawnPoint";
        public const string BossSpawnPointName = "BossSpawnPoint";
        public const string VisualRootName = "VisualRoot";
        public const string GameplayRootName = "GameplayRoot";

        [Header("Identity")]
        [SerializeField] private string roomId = "room_001";
        [SerializeField] private RoomType roomType = RoomType.Combat;

        [Header("Required Roots")]
        [SerializeField] private Transform arenaRoot;
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform enemySpawnPointsRoot;
        [SerializeField] private List<Transform> enemySpawnPoints = new List<Transform>();
        [SerializeField] private Transform exitGate;
        [SerializeField] private Transform cameraBounds;

        [Header("Optional")]
        [SerializeField] private Transform playerExitPoint;
        [SerializeField] private Transform rewardSpawnPoint;
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private RoomBounds roomBounds;
        [SerializeField] private List<RoomExitGate> exitGates = new List<RoomExitGate>();
        [SerializeField] private Transform hazardRoot;
        [SerializeField] private Transform rewardSpawnRoot;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform gameplayRoot;
        [SerializeField] private string debugLabel;

        public string RoomId => roomId;
        public RoomType RoomType => roomType;
        public Transform ArenaRoot => arenaRoot;
        public Transform PlayerSpawn => playerSpawn;
        public Transform PlayerEntryPoint => playerSpawn;
        public Transform PlayerExitPoint => playerExitPoint;
        public Transform EnemySpawnPointsRoot => enemySpawnPointsRoot;
        public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;
        public Transform ExitGate => exitGate;
        public Transform CameraBounds => cameraBounds;
        public RoomBounds Bounds => roomBounds;
        public IReadOnlyList<RoomExitGate> ExitGates => exitGates;
        public Transform HazardRoot => hazardRoot;
        public Transform HazardsRoot => hazardRoot;
        public Transform RewardSpawnRoot => rewardSpawnRoot;
        public Transform RewardSpawnPoint => rewardSpawnPoint != null ? rewardSpawnPoint : rewardSpawnRoot;
        public Transform BossSpawnPoint => bossSpawnPoint;
        public Transform VisualRoot => visualRoot;
        public Transform GameplayRoot => gameplayRoot;
        public string DebugLabel => debugLabel;

        public bool HasRequiredReferences =>
            arenaRoot != null
            && playerSpawn != null
            && enemySpawnPointsRoot != null
            && enemySpawnPoints != null
            && enemySpawnPoints.Count > 0
            && exitGate != null
            && HasBoundsReference;

        private bool HasBoundsReference => roomBounds != null || cameraBounds != null || GetComponentInChildren<CameraBounds>(true) != null;

        private void Reset()
        {
            AutoResolveReferences();
        }

        private void OnValidate()
        {
            enemySpawnPoints ??= new List<Transform>();
            exitGates ??= new List<RoomExitGate>();
            if (string.IsNullOrWhiteSpace(roomId))
            {
                roomId = "room_001";
            }

            RefreshEnemySpawnPoints();
            RefreshExitGates();
        }

        public void SetReferences(
            Transform arena,
            Transform player,
            Transform enemySpawnRoot,
            IReadOnlyList<Transform> enemySpawns,
            Transform exit,
            Transform bounds,
            Transform hazards,
            Transform rewardRoot)
        {
            arenaRoot = arena;
            playerSpawn = player;
            enemySpawnPointsRoot = enemySpawnRoot;
            exitGate = exit;
            cameraBounds = bounds;
            roomBounds = bounds != null ? bounds.GetComponent<RoomBounds>() : null;
            hazardRoot = hazards;
            rewardSpawnRoot = rewardRoot;
            rewardSpawnPoint = rewardRoot;
            gameplayRoot = gameplayRoot != null ? gameplayRoot : arena;
            visualRoot = visualRoot != null ? visualRoot : arena;

            enemySpawnPoints ??= new List<Transform>();
            enemySpawnPoints.Clear();
            if (enemySpawns == null)
            {
                return;
            }

            for (var i = 0; i < enemySpawns.Count; i++)
            {
                if (enemySpawns[i] != null && !enemySpawnPoints.Contains(enemySpawns[i]))
                {
                    enemySpawnPoints.Add(enemySpawns[i]);
                }
            }

            RefreshExitGates();
        }

        public IReadOnlyList<Transform> GetEnemySpawnPoints()
        {
            RefreshEnemySpawnPoints();
            return enemySpawnPoints;
        }

        public Transform[] GetEnemySpawnPointArray()
        {
            RefreshEnemySpawnPoints();
            return enemySpawnPoints.ToArray();
        }

        public Transform GetPlayerEntryPoint()
        {
            return playerSpawn;
        }

        public Transform GetRewardSpawnPoint()
        {
            return rewardSpawnPoint != null ? rewardSpawnPoint : rewardSpawnRoot;
        }

        public Transform GetBossSpawnPoint()
        {
            return bossSpawnPoint;
        }

        public RoomBounds GetRoomBounds()
        {
            return roomBounds;
        }

        public CameraBounds GetCameraBounds()
        {
            if (roomBounds != null && roomBounds.CameraBounds != null)
            {
                return roomBounds.CameraBounds;
            }

            if (cameraBounds != null)
            {
                var bounds = cameraBounds.GetComponent<CameraBounds>();
                if (bounds != null)
                {
                    return bounds;
                }
            }

            return GetComponentInChildren<CameraBounds>(true);
        }

        public IReadOnlyList<RoomExitGate> GetExitGates()
        {
            RefreshExitGates();
            return exitGates;
        }

        public IReadOnlyList<string> ValidateContract()
        {
            AutoResolveReferences();
            var warnings = new List<string>();

            if (arenaRoot == null)
            {
                warnings.Add($"Missing required child: {ArenaRootName}.");
            }

            if (playerSpawn == null)
            {
                warnings.Add($"Missing required child: {PlayerSpawnName}.");
            }

            if (enemySpawnPointsRoot == null)
            {
                warnings.Add($"Missing required child: {EnemySpawnPointsRootName}.");
            }

            if (enemySpawnPoints == null || enemySpawnPoints.Count == 0)
            {
                warnings.Add($"{EnemySpawnPointsRootName} must contain at least one spawn point.");
            }

            if (exitGate == null && (exitGates == null || exitGates.Count == 0))
            {
                warnings.Add($"Missing required child: {ExitGateName}.");
            }

            if (!HasBoundsReference)
            {
                warnings.Add($"Missing required child/component: {CameraBoundsName} or {nameof(RoomBounds)}.");
            }

            if (GetRewardSpawnPoint() == null)
            {
                warnings.Add($"Optional reward spawn point is missing: {RewardSpawnPointName} or {RewardSpawnRootName}.");
            }

            if (roomType == RoomType.Boss && bossSpawnPoint == null)
            {
                warnings.Add($"Boss room has no {BossSpawnPointName}; enemy wave spawn points will be used as fallback.");
            }

            if (exitGate != null && (exitGates == null || exitGates.Count == 0))
            {
                warnings.Add($"{ExitGateName} has no {nameof(RoomExitGate)} component. It can still be used as a transform marker, but lock/unlock will not affect colliders.");
            }

            return warnings;
        }

        [ContextMenu("Auto Resolve References")]
        public void AutoResolveReferences()
        {
            arenaRoot = ResolveNamedReference(arenaRoot, ArenaRootName);
            playerSpawn = ResolveNamedReference(playerSpawn, PlayerSpawnName);
            playerExitPoint = ResolveNamedReference(playerExitPoint, PlayerExitPointName);
            enemySpawnPointsRoot = ResolveNamedReference(enemySpawnPointsRoot, EnemySpawnPointsRootName);
            exitGate = ResolveNamedReference(exitGate, ExitGateName);
            cameraBounds = ResolveNamedReference(cameraBounds, CameraBoundsName);
            roomBounds = ResolveRoomBounds(roomBounds, cameraBounds);
            hazardRoot = ResolveNamedReference(hazardRoot, HazardRootName);
            rewardSpawnRoot = ResolveNamedReference(rewardSpawnRoot, RewardSpawnRootName);
            rewardSpawnPoint = ResolveNamedReference(rewardSpawnPoint, RewardSpawnPointName) ?? rewardSpawnRoot;
            bossSpawnPoint = ResolveNamedReference(bossSpawnPoint, BossSpawnPointName);
            visualRoot = ResolveNamedReference(visualRoot, VisualRootName);
            gameplayRoot = ResolveNamedReference(gameplayRoot, GameplayRootName);
            RefreshEnemySpawnPoints();
            RefreshExitGates();
        }

        public bool TryValidate(out string message)
        {
            var warnings = ValidateContract();
            for (var i = 0; i < warnings.Count; i++)
            {
                var warning = warnings[i];
                if (warning.StartsWith("Optional", System.StringComparison.Ordinal)
                    || warning.IndexOf("lock/unlock", System.StringComparison.Ordinal) >= 0
                    || warning.IndexOf("fallback", System.StringComparison.Ordinal) >= 0)
                {
                    continue;
                }

                message = warning;
                return false;
            }

            message = "Room prefab contract is valid.";
            return true;
        }

        private void RefreshEnemySpawnPoints()
        {
            enemySpawnPoints ??= new List<Transform>();
            enemySpawnPoints.RemoveAll(spawnPoint => spawnPoint == null);

            if (enemySpawnPointsRoot == null)
            {
                return;
            }

            enemySpawnPoints.Clear();
            for (var i = 0; i < enemySpawnPointsRoot.childCount; i++)
            {
                var child = enemySpawnPointsRoot.GetChild(i);
                if (child != null)
                {
                    enemySpawnPoints.Add(child);
                }
            }
        }

        private void RefreshExitGates()
        {
            exitGates ??= new List<RoomExitGate>();
            exitGates.RemoveAll(gate => gate == null);

            var gates = GetComponentsInChildren<RoomExitGate>(true);
            for (var i = 0; i < gates.Length; i++)
            {
                if (gates[i] != null && !exitGates.Contains(gates[i]))
                {
                    exitGates.Add(gates[i]);
                }
            }
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindDeepChild(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private Transform ResolveNamedReference(Transform current, string childName)
        {
            if (current != null && current.name == childName)
            {
                return current;
            }

            return FindDeepChild(transform, childName);
        }

        private RoomBounds ResolveRoomBounds(RoomBounds current, Transform boundsTransform)
        {
            if (current != null)
            {
                return current;
            }

            if (boundsTransform != null)
            {
                var bounds = boundsTransform.GetComponent<RoomBounds>();
                if (bounds != null)
                {
                    return bounds;
                }
            }

            return GetComponentInChildren<RoomBounds>(true);
        }
    }
}
