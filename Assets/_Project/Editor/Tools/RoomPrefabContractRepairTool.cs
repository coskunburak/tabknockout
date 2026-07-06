using System.Collections.Generic;
using System.IO;
using TapKnockout.Camera;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class RoomPrefabContractRepairTool
    {
        private const string MenuPath = "Tools/Tap Knockout/Rooms/Repair Room Prefab Contracts";
        private const string RoomPrefabFolder = "Assets/_Project/Prefabs/Rooms";
        private const string ExitGatesRootName = "ExitGates";

        [MenuItem(MenuPath)]
        public static void RepairRoomPrefabContracts()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Room Prefab Contract Repair", "Exit Play Mode before repairing room prefabs.", "OK");
                return;
            }

            var report = RepairAllRoomPrefabContracts();
            EditorUtility.DisplayDialog(
                "Room Prefab Contract Repair",
                $"Scanned {report.ScannedCount} room prefab(s).\nRepaired {report.RepairedCount} prefab(s).\nWarnings: {report.WarningCount}\n\nCheck Window > General > Console for details.",
                "OK");
        }

        public static RoomRepairSummary RepairAllRoomPrefabContracts()
        {
            var summary = new RoomRepairSummary();
            if (!AssetDatabase.IsValidFolder(RoomPrefabFolder))
            {
                Debug.LogWarning($"{nameof(RoomPrefabContractRepairTool)} missing folder: {RoomPrefabFolder}");
                summary.WarningCount++;
                return summary;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { RoomPrefabFolder });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                RepairPrefab(path, summary);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{nameof(RoomPrefabContractRepairTool)} scanned {summary.ScannedCount} prefab(s), repaired {summary.RepairedCount}, warnings {summary.WarningCount}.");
            return summary;
        }

        private static void RepairPrefab(string path, RoomRepairSummary summary)
        {
            summary.ScannedCount++;
            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                summary.WarningCount++;
                Debug.LogWarning($"{nameof(RoomPrefabContractRepairTool)} could not load prefab: {path}");
                return;
            }

            try
            {
                var changed = RepairLoadedPrefab(root, path);
                if (!changed)
                {
                    Debug.Log($"{nameof(RoomPrefabContractRepairTool)} Done: {path} already has a complete generated room contract.", root);
                    return;
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                summary.RepairedCount++;
                Debug.Log($"{nameof(RoomPrefabContractRepairTool)} Done: repaired {path}.", root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool RepairLoadedPrefab(GameObject prefabRoot, string path)
        {
            var changed = false;
            var contract = prefabRoot.GetComponentInChildren<RoomPrefabContract>(true);
            if (contract == null)
            {
                contract = prefabRoot.AddComponent<RoomPrefabContract>();
                changed = true;
            }

            var root = contract.transform;
            if (root.name != RoomPrefabContract.RoomRootName)
            {
                root.name = RoomPrefabContract.RoomRootName;
                changed = true;
            }

            var visualRoot = EnsureChild(root, RoomPrefabContract.VisualRootName, ref changed);
            var gameplayRoot = EnsureChild(root, RoomPrefabContract.GameplayRootName, ref changed);
            var arena = EnsureNamedReference(root, RoomPrefabContract.ArenaRootName, visualRoot, ref changed);
            var playerSpawn = EnsureNamedReference(root, RoomPrefabContract.PlayerSpawnName, gameplayRoot, ref changed);
            EnsureNamedReference(root, "PlayerEntryPoint", gameplayRoot, ref changed);
            var playerExit = EnsureNamedReference(root, RoomPrefabContract.PlayerExitPointName, gameplayRoot, ref changed);
            var enemySpawnRoot = EnsureNamedReference(root, RoomPrefabContract.EnemySpawnPointsRootName, gameplayRoot, ref changed);
            EnsureEnemySpawnChildren(enemySpawnRoot, ref changed);
            var rewardSpawnRoot = EnsureNamedReference(root, RoomPrefabContract.RewardSpawnRootName, gameplayRoot, ref changed);
            var rewardSpawn = EnsureNamedReference(root, RoomPrefabContract.RewardSpawnPointName, gameplayRoot, ref changed);
            var bossSpawn = EnsureNamedReference(root, RoomPrefabContract.BossSpawnPointName, gameplayRoot, ref changed);
            var hazardRoot = EnsureNamedReference(root, RoomPrefabContract.HazardRootName, gameplayRoot, ref changed);
            var cameraBounds = EnsureNamedReference(root, RoomPrefabContract.CameraBoundsName, gameplayRoot, ref changed);
            var roomBounds = EnsureRoomBounds(cameraBounds, IsBossPrefabPath(path), ref changed);
            var exitGate = ResolveOrCreateExitGate(root, gameplayRoot, ref changed);
            var exitGates = ResolveExitGateComponents(root, exitGate);

            var serializedObject = new SerializedObject(contract);
            changed |= SetStringIfMissing(serializedObject, "roomId", CreateRoomId(path));
            changed |= SetEnum(serializedObject, "roomType", (int)ResolveRoomType(path));
            changed |= SetObject(serializedObject, "arenaRoot", arena);
            changed |= SetObject(serializedObject, "playerSpawn", playerSpawn);
            changed |= SetObject(serializedObject, "playerExitPoint", playerExit);
            changed |= SetObject(serializedObject, "enemySpawnPointsRoot", enemySpawnRoot);
            changed |= SetObject(serializedObject, "exitGate", exitGate);
            changed |= SetObject(serializedObject, "cameraBounds", cameraBounds);
            changed |= SetObject(serializedObject, "roomBounds", roomBounds);
            changed |= SetObject(serializedObject, "hazardRoot", hazardRoot);
            changed |= SetObject(serializedObject, "rewardSpawnRoot", rewardSpawnRoot);
            changed |= SetObject(serializedObject, "rewardSpawnPoint", rewardSpawn);
            changed |= SetObject(serializedObject, "bossSpawnPoint", bossSpawn);
            changed |= SetObject(serializedObject, "visualRoot", visualRoot);
            changed |= SetObject(serializedObject, "gameplayRoot", gameplayRoot);
            changed |= SetStringIfMissing(serializedObject, "debugLabel", Path.GetFileNameWithoutExtension(path));
            changed |= SetTransformList(serializedObject, "enemySpawnPoints", CollectEnemySpawnPoints(enemySpawnRoot));
            changed |= SetObjectList(serializedObject, "exitGates", exitGates);

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(contract);
            }

            return changed;
        }

        private static Transform EnsureNamedReference(Transform root, string childName, Transform preferredParent, ref bool changed)
        {
            var existing = FindDeepChild(root, childName);
            if (existing != null)
            {
                return existing;
            }

            return EnsureChild(preferredParent != null ? preferredParent : root, childName, ref changed);
        }

        private static Transform EnsureChild(Transform parent, string childName, ref bool changed)
        {
            var direct = parent.Find(childName);
            if (direct != null)
            {
                return direct;
            }

            var child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            changed = true;
            return child.transform;
        }

        private static void EnsureEnemySpawnChildren(Transform enemySpawnRoot, ref bool changed)
        {
            if (enemySpawnRoot == null || enemySpawnRoot.childCount > 0)
            {
                return;
            }

            var positions = new[]
            {
                new Vector3(-3f, 0f, 2.5f),
                new Vector3(0f, 0f, 3.5f),
                new Vector3(3f, 0f, 2.5f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var spawn = new GameObject($"Spawn_{i + 1:00}");
                spawn.transform.SetParent(enemySpawnRoot, false);
                spawn.transform.localPosition = positions[i];
            }

            changed = true;
        }

        private static RoomBounds EnsureRoomBounds(Transform cameraBoundsTransform, bool isBossRoom, ref bool changed)
        {
            if (cameraBoundsTransform == null)
            {
                return null;
            }

            var cameraBounds = cameraBoundsTransform.GetComponent<CameraBounds>();
            if (cameraBounds == null)
            {
                cameraBounds = cameraBoundsTransform.gameObject.AddComponent<CameraBounds>();
                changed = true;
            }

            var roomBounds = cameraBoundsTransform.GetComponent<RoomBounds>();
            if (roomBounds == null)
            {
                roomBounds = cameraBoundsTransform.gameObject.AddComponent<RoomBounds>();
                changed = true;
            }

            roomBounds.SetBounds(Vector3.zero, new Vector3(14f, 0f, 18f));
            roomBounds.SetCameraTargetBounds(Vector3.zero, new Vector3(2f, 0f, 10f));
            if (isBossRoom)
            {
                roomBounds.SetBossCameraOverride(new Vector3(0f, 0f, -3f), new Vector3(3f, 0f, 8f));
            }

            var serializedBounds = new SerializedObject(roomBounds);
            changed |= SetObject(serializedBounds, "cameraBounds", cameraBounds);
            if (changed)
            {
                serializedBounds.ApplyModifiedProperties();
                EditorUtility.SetDirty(roomBounds);
            }

            return roomBounds;
        }

        private static Transform ResolveOrCreateExitGate(Transform root, Transform gameplayRoot, ref bool changed)
        {
            var legacyGate = FindDeepChild(root, RoomPrefabContract.ExitGateName);
            if (legacyGate != null)
            {
                EnsureExitGateComponent(legacyGate, ref changed);
                return legacyGate;
            }

            var gatesRoot = EnsureNamedReference(root, ExitGatesRootName, gameplayRoot, ref changed);
            var gate = EnsureChild(gatesRoot, "Gate_North", ref changed);
            gate.localPosition = new Vector3(0f, 0f, 8f);
            EnsureExitGateComponent(gate, ref changed);
            return gate;
        }

        private static void EnsureExitGateComponent(Transform gate, ref bool changed)
        {
            var collider = gate.GetComponent<Collider>();
            if (collider == null)
            {
                var box = gate.gameObject.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(3f, 2.25f, 1f);
                collider = box;
                changed = true;
            }

            var exitGate = gate.GetComponent<RoomExitGate>();
            if (exitGate == null)
            {
                exitGate = gate.gameObject.AddComponent<RoomExitGate>();
                changed = true;
            }

            exitGate.SetReferences(collider, null, null);
            EditorUtility.SetDirty(exitGate);
        }

        private static List<RoomExitGate> ResolveExitGateComponents(Transform root, Transform primaryGate)
        {
            var gates = new List<RoomExitGate>();
            var primary = primaryGate != null ? primaryGate.GetComponent<RoomExitGate>() : null;
            if (primary != null)
            {
                gates.Add(primary);
            }

            var found = root.GetComponentsInChildren<RoomExitGate>(true);
            for (var i = 0; i < found.Length; i++)
            {
                if (found[i] != null && !gates.Contains(found[i]))
                {
                    gates.Add(found[i]);
                }
            }

            return gates;
        }

        private static List<Transform> CollectEnemySpawnPoints(Transform enemySpawnRoot)
        {
            var points = new List<Transform>();
            if (enemySpawnRoot == null)
            {
                return points;
            }

            for (var i = 0; i < enemySpawnRoot.childCount; i++)
            {
                var child = enemySpawnRoot.GetChild(i);
                if (child != null)
                {
                    points.Add(child);
                }
            }

            return points;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
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

        private static string CreateRoomId(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return name.Replace("PF_", string.Empty).ToLowerInvariant();
        }

        private static RoomType ResolveRoomType(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains("boss"))
            {
                return RoomType.Boss;
            }

            if (name.Contains("elite"))
            {
                return RoomType.Elite;
            }

            if (name.Contains("reward"))
            {
                return RoomType.Reward;
            }

            if (name.Contains("heal"))
            {
                return RoomType.Heal;
            }

            if (name.Contains("shop"))
            {
                return RoomType.Shop;
            }

            return RoomType.Combat;
        }

        private static bool IsBossPrefabPath(string path)
        {
            return Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains("boss");
        }

        private static bool SetStringIfMissing(SerializedObject serializedObject, string propertyName, string value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || !string.IsNullOrWhiteSpace(property.stringValue))
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        private static bool SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.enumValueIndex == value)
            {
                return false;
            }

            property.enumValueIndex = value;
            return true;
        }

        private static bool SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetTransformList(SerializedObject serializedObject, string propertyName, IReadOnlyList<Transform> values)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return false;
            }

            var changed = property.arraySize != values.Count;
            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue == values[i])
                {
                    continue;
                }

                item.objectReferenceValue = values[i];
                changed = true;
            }

            return changed;
        }

        private static bool SetObjectList<T>(SerializedObject serializedObject, string propertyName, IReadOnlyList<T> values)
            where T : Object
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return false;
            }

            var changed = property.arraySize != values.Count;
            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue == values[i])
                {
                    continue;
                }

                item.objectReferenceValue = values[i];
                changed = true;
            }

            return changed;
        }

        public sealed class RoomRepairSummary
        {
            public int ScannedCount;
            public int RepairedCount;
            public int WarningCount;
        }
    }
}
