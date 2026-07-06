using System;
using System.Collections.Generic;
using TapKnockout.Camera;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.Editor.Tools
{
    public static class RoomPrefabVariantBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/Rooms/Create Placeholder Room Prefabs";
        private const string RoomPrefabFolder = "Assets/_Project/Prefabs/Rooms";
        private const string MaterialFolder = "Assets/_Project/Art/Materials/Generated";
        private const string GeneratedMarker = "generated_phase3_placeholder_room";

        private static readonly RoomVariantSpec[] Specs =
        {
            new RoomVariantSpec("PF_Room_Small_Combat", "room_small_combat", RoomType.Combat, new Vector2(12f, 16f), 5, false, false),
            new RoomVariantSpec("PF_Room_Medium_Combat", "room_medium_combat", RoomType.Combat, new Vector2(15f, 19f), 6, false, false),
            new RoomVariantSpec("PF_Room_Wide_Combat", "room_wide_combat", RoomType.Combat, new Vector2(19f, 16f), 7, false, false),
            new RoomVariantSpec("PF_Room_Hazard_Placeholder", "room_hazard_placeholder", RoomType.Combat, new Vector2(15f, 19f), 6, false, true),
            new RoomVariantSpec("PF_Room_Elite", "room_elite", RoomType.Elite, new Vector2(16f, 20f), 6, false, false),
            new RoomVariantSpec("PF_Room_Reward", "room_reward", RoomType.Reward, new Vector2(12f, 14f), 3, false, false),
            new RoomVariantSpec("PF_Room_Heal", "room_heal", RoomType.Heal, new Vector2(12f, 14f), 3, false, false),
            new RoomVariantSpec("PF_Room_ShopPlaceholder", "room_shop_placeholder", RoomType.Shop, new Vector2(13f, 15f), 3, false, false),
            new RoomVariantSpec("PF_Room_Boss", "room_boss", RoomType.Boss, new Vector2(20f, 22f), 8, true, false)
        };

        [MenuItem(MenuPath)]
        public static void CreatePlaceholderRoomPrefabs()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Placeholder Room Prefabs", "Exit Play Mode before creating room prefabs.", "OK");
                return;
            }

            EnsureFolder(RoomPrefabFolder);
            EnsureFolder(MaterialFolder);
            var warnings = new List<string>();

            for (var i = 0; i < Specs.Length; i++)
            {
                CreateOrUpdatePrefab(Specs[i], warnings);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            for (var i = 0; i < warnings.Count; i++)
            {
                Debug.LogWarning(warnings[i]);
            }

            EditorUtility.DisplayDialog(
                "Placeholder Room Prefabs",
                warnings.Count == 0
                    ? "Phase 3 placeholder room prefabs were created or updated."
                    : $"Phase 3 placeholder room prefabs completed with {warnings.Count} warning(s). Check Window > General > Console.",
                "OK");
        }

        private static void CreateOrUpdatePrefab(RoomVariantSpec spec, List<string> warnings)
        {
            var path = $"{RoomPrefabFolder}/{spec.PrefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && !IsGeneratedPlaceholder(existing))
            {
                warnings.Add($"Skipped {path}; existing prefab does not look like Phase 3 generated placeholder content.");
                return;
            }

            var root = new GameObject(RoomPrefabContract.RoomRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Placeholder Room Prefab");

            try
            {
                BuildRoom(root, spec);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out var success);
                if (!success || prefab == null)
                {
                    warnings.Add($"Failed to save room prefab: {path}");
                    return;
                }

                Debug.Log($"{nameof(RoomPrefabVariantBuilder)} created/updated {path}.", prefab);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildRoom(GameObject root, RoomVariantSpec spec)
        {
            root.name = RoomPrefabContract.RoomRootName;
            var contract = root.AddComponent<RoomPrefabContract>();
            var visualRoot = CreateChild(root.transform, RoomPrefabContract.VisualRootName);
            var gameplayRoot = CreateChild(root.transform, RoomPrefabContract.GameplayRootName);
            var arena = CreateChild(visualRoot, RoomPrefabContract.ArenaRootName);
            var enemySpawnRoot = CreateChild(gameplayRoot, RoomPrefabContract.EnemySpawnPointsRootName);
            var hazardRoot = CreateChild(gameplayRoot, RoomPrefabContract.HazardRootName);

            BuildArenaGeometry(arena, spec);
            if (spec.HasHazardMarker)
            {
                BuildHazardPlaceholder(hazardRoot, spec);
            }

            var playerEntry = CreateMarker(gameplayRoot, RoomPrefabContract.PlayerSpawnName, new Vector3(0f, 0f, -spec.Size.y * 0.5f + 1.6f));
            var playerExit = CreateMarker(gameplayRoot, RoomPrefabContract.PlayerExitPointName, new Vector3(0f, 0f, spec.Size.y * 0.5f - 1.1f));
            var rewardSpawn = CreateMarker(gameplayRoot, RoomPrefabContract.RewardSpawnPointName, new Vector3(0f, 0f, -1.5f));
            var bossSpawn = CreateMarker(gameplayRoot, RoomPrefabContract.BossSpawnPointName, new Vector3(0f, 0f, spec.IsBoss ? 2f : 0.8f));
            var enemySpawns = BuildEnemySpawnPoints(enemySpawnRoot, spec);
            var gate = BuildExitGate(gameplayRoot, spec);
            var bounds = BuildBounds(gameplayRoot, spec);

            contract.SetReferences(arena, playerEntry, enemySpawnRoot, enemySpawns, gate.transform, bounds.transform, hazardRoot, rewardSpawn);
            SetContractMetadata(contract, spec, playerExit, rewardSpawn, bossSpawn, bounds, gate, visualRoot, gameplayRoot);
        }

        private static void BuildArenaGeometry(Transform arena, RoomVariantSpec spec)
        {
            var halfX = spec.Size.x * 0.5f;
            var halfZ = spec.Size.y * 0.5f;

            CreateCube(arena, "Floor", new Vector3(0f, -0.05f, 0f), new Vector3(spec.Size.x, 0.1f, spec.Size.y), new Color(0.35f, 0.44f, 0.4f));
            CreateCube(arena, "Wall_North", new Vector3(0f, 0.75f, halfZ), new Vector3(spec.Size.x, 1.5f, 0.35f), new Color(0.28f, 0.3f, 0.33f));
            CreateCube(arena, "Wall_South", new Vector3(0f, 0.75f, -halfZ), new Vector3(spec.Size.x, 1.5f, 0.35f), new Color(0.28f, 0.3f, 0.33f));
            CreateCube(arena, "Wall_West", new Vector3(-halfX, 0.75f, 0f), new Vector3(0.35f, 1.5f, spec.Size.y), new Color(0.28f, 0.3f, 0.33f));
            CreateCube(arena, "Wall_East", new Vector3(halfX, 0.75f, 0f), new Vector3(0.35f, 1.5f, spec.Size.y), new Color(0.28f, 0.3f, 0.33f));
        }

        private static void BuildHazardPlaceholder(Transform hazardRoot, RoomVariantSpec spec)
        {
            CreateCube(hazardRoot, "HazardPlaceholder_Left", new Vector3(-spec.Size.x * 0.2f, 0.03f, 0.5f), new Vector3(1.4f, 0.06f, 5f), new Color(0.8f, 0.52f, 0.16f));
            CreateCube(hazardRoot, "HazardPlaceholder_Right", new Vector3(spec.Size.x * 0.2f, 0.03f, 0.5f), new Vector3(1.4f, 0.06f, 5f), new Color(0.8f, 0.52f, 0.16f));
        }

        private static List<Transform> BuildEnemySpawnPoints(Transform root, RoomVariantSpec spec)
        {
            var points = new List<Transform>(spec.SpawnCount);
            var radiusX = spec.Size.x * 0.32f;
            var radiusZ = spec.Size.y * 0.28f;
            for (var i = 0; i < spec.SpawnCount; i++)
            {
                var angle = Mathf.PI * 2f * i / Mathf.Max(1, spec.SpawnCount);
                var position = new Vector3(Mathf.Cos(angle) * radiusX, 0f, Mathf.Sin(angle) * radiusZ + 1f);
                points.Add(CreateMarker(root, $"SP_Enemy_{i + 1:00}", position));
            }

            return points;
        }

        private static RoomExitGate BuildExitGate(Transform parent, RoomVariantSpec spec)
        {
            var gateRoot = new GameObject(RoomPrefabContract.ExitGateName);
            gateRoot.transform.SetParent(parent, false);
            gateRoot.transform.localPosition = new Vector3(0f, 0.75f, spec.Size.y * 0.5f - 0.25f);
            var collider = gateRoot.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.8f, 1.8f, 0.5f);

            var lockedVisual = CreateCube(gateRoot.transform, "LockedVisual", Vector3.zero, new Vector3(2.6f, 1.6f, 0.35f), new Color(0.55f, 0.22f, 0.2f));
            var unlockedVisual = CreateCube(gateRoot.transform, "UnlockedVisual", Vector3.zero, new Vector3(1.4f, 0.15f, 0.35f), new Color(0.25f, 0.65f, 0.35f));
            RemoveCollider(lockedVisual);
            RemoveCollider(unlockedVisual);
            var gate = gateRoot.AddComponent<RoomExitGate>();
            gate.SetReferences(collider, lockedVisual, unlockedVisual);
            return gate;
        }

        private static Transform BuildBounds(Transform parent, RoomVariantSpec spec)
        {
            var boundsRoot = CreateChild(parent, RoomPrefabContract.CameraBoundsName);
            var cameraBounds = boundsRoot.gameObject.AddComponent<CameraBounds>();
            cameraBounds.Center = new Vector3(0f, 0f, -spec.Size.y * 0.42f);
            cameraBounds.Size = spec.IsBoss ? new Vector3(2.5f, 0f, 10f) : new Vector3(2f, 0f, 11f);

            var roomBounds = boundsRoot.gameObject.AddComponent<RoomBounds>();
            roomBounds.SetBounds(Vector3.zero, new Vector3(spec.Size.x, 0f, spec.Size.y));
            roomBounds.SetCameraTargetBounds(cameraBounds.Center, cameraBounds.Size);
            if (spec.IsBoss)
            {
                roomBounds.SetBossCameraOverride(new Vector3(0f, 0f, -spec.Size.y * 0.32f), new Vector3(3f, 0f, 8f));
            }

            return boundsRoot;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static Transform CreateMarker(Transform parent, string name, Vector3 localPosition)
        {
            var marker = CreateChild(parent, name);
            marker.localPosition = localPosition;
            return marker;
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetMaterial(color);
            }

            return cube;
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            var collider = gameObject != null ? gameObject.GetComponent<Collider>() : null;
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static Material GetMaterial(Color color)
        {
            var path = $"{MaterialFolder}/MAT_Phase3Placeholder_{ColorUtility.ToHtmlStringRGB(color)}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            material.name = System.IO.Path.GetFileNameWithoutExtension(path);
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetContractMetadata(
            RoomPrefabContract contract,
            RoomVariantSpec spec,
            Transform playerExit,
            Transform rewardSpawn,
            Transform bossSpawn,
            Transform bounds,
            RoomExitGate gate,
            Transform visualRoot,
            Transform gameplayRoot)
        {
            var serializedObject = new SerializedObject(contract);
            SetString(serializedObject, "roomId", spec.RoomId);
            SetEnum(serializedObject, "roomType", (int)spec.RoomType);
            SetObject(serializedObject, "playerExitPoint", playerExit);
            SetObject(serializedObject, "rewardSpawnPoint", rewardSpawn);
            SetObject(serializedObject, "bossSpawnPoint", bossSpawn);
            SetObject(serializedObject, "roomBounds", bounds.GetComponent<RoomBounds>());
            SetObject(serializedObject, "visualRoot", visualRoot);
            SetObject(serializedObject, "gameplayRoot", gameplayRoot);
            SetString(serializedObject, "debugLabel", GeneratedMarker);

            var gates = serializedObject.FindProperty("exitGates");
            if (gates != null)
            {
                gates.arraySize = 1;
                gates.GetArrayElementAtIndex(0).objectReferenceValue = gate;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(contract);
        }

        private static bool IsGeneratedPlaceholder(GameObject prefab)
        {
            var contract = prefab.GetComponentInChildren<RoomPrefabContract>(true);
            return contract != null && string.Equals(contract.DebugLabel, GeneratedMarker, StringComparison.Ordinal);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int enumValueIndex)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = enumValueIndex;
            }
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private readonly struct RoomVariantSpec
        {
            public RoomVariantSpec(string prefabName, string roomId, RoomType roomType, Vector2 size, int spawnCount, bool isBoss, bool hasHazardMarker)
            {
                PrefabName = prefabName;
                RoomId = roomId;
                RoomType = roomType;
                Size = size;
                SpawnCount = Mathf.Max(1, spawnCount);
                IsBoss = isBoss;
                HasHazardMarker = hasHazardMarker;
            }

            public string PrefabName { get; }
            public string RoomId { get; }
            public RoomType RoomType { get; }
            public Vector2 Size { get; }
            public int SpawnCount { get; }
            public bool IsBoss { get; }
            public bool HasHazardMarker { get; }
        }
    }
}
