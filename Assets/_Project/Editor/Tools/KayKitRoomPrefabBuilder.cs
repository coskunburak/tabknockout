using System.Collections.Generic;
using TapKnockout.Camera;
using TapKnockout.Room;
using UnityEditor;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class KayKitRoomPrefabBuilder
    {
        private const string CreateMenuPath = "Tools/Tap Knockout/Room/Create KayKit Combat Room Prefab";
        private const string ValidateMenuPath = "Tools/Tap Knockout/Room/Validate Selected Room Prefab Contract";
        private const string RoomPrefabPath = "Assets/_Project/Prefabs/Rooms/PF_Room_KayKit_Combat_01.prefab";
        private const string MaterialFolder = "Assets/_Project/Art/Materials";
        private const string KayKitModelFolder = "Assets/Assets/game asset packs/KayKit_DungeonRemastered_1.1_FREE/Assets/fbx(unity)";

        private static readonly Vector3[] EnemySpawnPositions =
        {
            new Vector3(-3.8f, 0f, 3.5f),
            new Vector3(0f, 0f, 4.4f),
            new Vector3(3.8f, 0f, 3.5f),
            new Vector3(-2.4f, 0f, 0.6f),
            new Vector3(2.4f, 0f, 0.6f)
        };

        [MenuItem(CreateMenuPath)]
        public static void CreateKayKitCombatRoomPrefab()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "KayKit Room Prefab",
                    "Stop Play Mode before creating or updating room prefabs.",
                    "OK");
                return;
            }

            EnsureFolder("Assets/_Project/Prefabs/Rooms");
            EnsureFolder("Assets/_Project/Prefabs/Arena");
            EnsureFolder(MaterialFolder);

            if (!AssetDatabase.IsValidFolder(KayKitModelFolder))
            {
                Debug.LogWarning(
                    $"{nameof(KayKitRoomPrefabBuilder)} could not find KayKit model folder at {KayKitModelFolder}. The generated room will use fallback primitives.",
                    null);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(RoomPrefabPath) != null)
            {
                var shouldOverwrite = EditorUtility.DisplayDialog(
                    "KayKit Room Prefab",
                    $"Update existing generated prefab?\n\n{RoomPrefabPath}",
                    "Update",
                    "Cancel");
                if (!shouldOverwrite)
                {
                    return;
                }
            }

            var root = new GameObject(RoomPrefabContract.RoomRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create KayKit Combat Room Prefab");

            try
            {
                var contract = root.AddComponent<RoomPrefabContract>();
                var arena = CreateChild(root.transform, RoomPrefabContract.ArenaRootName);
                var floorRoot = CreateChild(arena, "FloorRoot");
                var wallRoot = CreateChild(arena, "WallRoot");
                var propRoot = CreateChild(arena, "PropRoot");
                var collisionRoot = CreateChild(arena, "CollisionRoot");

                BuildFloor(floorRoot);
                BuildWalls(wallRoot);
                BuildProps(propRoot);
                BuildGameplayColliders(collisionRoot);

                var playerSpawn = CreateSpawnPoint(root.transform, RoomPrefabContract.PlayerSpawnName, new Vector3(0f, 0.05f, -5.2f));
                var enemySpawnRoot = CreateChild(root.transform, RoomPrefabContract.EnemySpawnPointsRootName);
                var enemySpawns = CreateEnemySpawnPoints(enemySpawnRoot);
                var exitGate = CreateExitGate(root.transform);
                var cameraBounds = CreateCameraBounds(root.transform);
                var hazardRoot = CreateChild(root.transform, RoomPrefabContract.HazardRootName);
                var rewardSpawnRoot = CreateChild(root.transform, RoomPrefabContract.RewardSpawnRootName);
                rewardSpawnRoot.localPosition = new Vector3(0f, 0f, -1.25f);

                contract.SetReferences(
                    arena,
                    playerSpawn,
                    enemySpawnRoot,
                    enemySpawns,
                    exitGate,
                    cameraBounds,
                    hazardRoot,
                    rewardSpawnRoot);

                MarkEnvironmentStatic(arena.gameObject);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, RoomPrefabPath, out var success);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!success || prefab == null)
                {
                    EditorUtility.DisplayDialog(
                        "KayKit Room Prefab",
                        "Prefab could not be saved. Check Console for details.",
                        "OK");
                    return;
                }

                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log(
                    $"{nameof(KayKitRoomPrefabBuilder)} created or updated {RoomPrefabPath} with {enemySpawns.Count} enemy spawn points, camera bounds, floor collision, boundary collision, and exit trigger.",
                    prefab);
                EditorUtility.DisplayDialog(
                    "KayKit Room Prefab",
                    $"Created/updated production room prefab:\n\n{RoomPrefabPath}",
                    "OK");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [MenuItem(ValidateMenuPath)]
        public static void ValidateSelectedRoomPrefabContract()
        {
            var selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog(
                    "Room Prefab Contract",
                    "Select a room prefab asset or scene instance first.",
                    "OK");
                return;
            }

            var contract = selected.GetComponentInChildren<RoomPrefabContract>(true);
            if (contract == null)
            {
                EditorUtility.DisplayDialog(
                    "Room Prefab Contract",
                    "Selected object has no RoomPrefabContract.",
                    "OK");
                return;
            }

            var isValid = contract.TryValidate(out var message);
            EditorUtility.DisplayDialog(
                "Room Prefab Contract",
                message,
                isValid ? "OK" : "Fix");
        }

        private static void BuildFloor(Transform parent)
        {
            var floorModel = LoadModel("floor_tile_large");
            var fallbackMaterial = GetOrCreateMaterial("MAT_KayKitRoom_Floor", new Color(0.45f, 0.43f, 0.39f, 1f));
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -4; z <= 4; z++)
                {
                    var tile = InstantiateModelOrCube(
                        floorModel,
                        "FloorTile",
                        parent,
                        new Vector3(x * 2f, 0f, z * 2f),
                        new Vector3(1f, 1f, 1f),
                        fallbackMaterial);
                    tile.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private static void BuildWalls(Transform parent)
        {
            var wallModel = LoadModel("wall_half") ?? LoadModel("wall");
            var doorwayModel = LoadModel("wall_doorway") ?? wallModel;
            var cornerModel = LoadModel("wall_corner") ?? wallModel;
            var fallbackMaterial = GetOrCreateMaterial("MAT_KayKitRoom_Wall", new Color(0.36f, 0.39f, 0.42f, 1f));

            for (var x = -3; x <= 3; x++)
            {
                InstantiateModelOrCube(
                    x == 0 ? doorwayModel : wallModel,
                    x == 0 ? "NorthDoorway" : "NorthWall",
                    parent,
                    new Vector3(x * 2f, 0f, 9f),
                    new Vector3(2f, 2f, 0.35f),
                    fallbackMaterial);
                InstantiateModelOrCube(
                    wallModel,
                    "SouthWall",
                    parent,
                    new Vector3(x * 2f, 0f, -9f),
                    new Vector3(2f, 2f, 0.35f),
                    fallbackMaterial);
            }

            for (var z = -3; z <= 3; z++)
            {
                var left = InstantiateModelOrCube(
                    wallModel,
                    "WestWall",
                    parent,
                    new Vector3(-7f, 0f, z * 2f),
                    new Vector3(0.35f, 2f, 2f),
                    fallbackMaterial);
                left.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

                var right = InstantiateModelOrCube(
                    wallModel,
                    "EastWall",
                    parent,
                    new Vector3(7f, 0f, z * 2f),
                    new Vector3(0.35f, 2f, 2f),
                    fallbackMaterial);
                right.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }

            InstantiateModelOrCube(cornerModel, "NorthWestCorner", parent, new Vector3(-7f, 0f, 9f), Vector3.one, fallbackMaterial);
            InstantiateModelOrCube(cornerModel, "NorthEastCorner", parent, new Vector3(7f, 0f, 9f), Vector3.one, fallbackMaterial);
            InstantiateModelOrCube(cornerModel, "SouthWestCorner", parent, new Vector3(-7f, 0f, -9f), Vector3.one, fallbackMaterial);
            InstantiateModelOrCube(cornerModel, "SouthEastCorner", parent, new Vector3(7f, 0f, -9f), Vector3.one, fallbackMaterial);
        }

        private static void BuildProps(Transform parent)
        {
            InstantiateModelOrCube(LoadModel("barrel_large"), "Prop_Barrel_NW", parent, new Vector3(-5.4f, 0f, 6.6f), Vector3.one, null);
            InstantiateModelOrCube(LoadModel("barrel_small_stack"), "Prop_Barrel_NE", parent, new Vector3(5.4f, 0f, 6.2f), Vector3.one, null);
            InstantiateModelOrCube(LoadModel("crates_stacked") ?? LoadModel("box_stacked"), "Prop_Crates_SW", parent, new Vector3(-5.6f, 0f, -6.2f), Vector3.one, null);
            InstantiateModelOrCube(LoadModel("pillar"), "Prop_Pillar_Left", parent, new Vector3(-3.2f, 0f, 2.2f), Vector3.one, null);
            InstantiateModelOrCube(LoadModel("pillar"), "Prop_Pillar_Right", parent, new Vector3(3.2f, 0f, 2.2f), Vector3.one, null);
            InstantiateModelOrCube(LoadModel("torch_lit") ?? LoadModel("torch"), "Prop_Torch_West", parent, new Vector3(-6.6f, 1.1f, 0f), Vector3.one, null);
            InstantiateModelOrCube(LoadModel("torch_lit") ?? LoadModel("torch"), "Prop_Torch_East", parent, new Vector3(6.6f, 1.1f, 0f), Vector3.one, null);
        }

        private static void BuildGameplayColliders(Transform parent)
        {
            CreateBoxCollider(parent, "Collision_Floor", new Vector3(0f, -0.08f, 0f), new Vector3(14f, 0.16f, 18f));
            CreateBoxCollider(parent, "Collision_North", new Vector3(0f, 1f, 9.35f), new Vector3(14f, 2f, 0.6f));
            CreateBoxCollider(parent, "Collision_South", new Vector3(0f, 1f, -9.35f), new Vector3(14f, 2f, 0.6f));
            CreateBoxCollider(parent, "Collision_West", new Vector3(-7.35f, 1f, 0f), new Vector3(0.6f, 2f, 18f));
            CreateBoxCollider(parent, "Collision_East", new Vector3(7.35f, 1f, 0f), new Vector3(0.6f, 2f, 18f));
        }

        private static List<Transform> CreateEnemySpawnPoints(Transform root)
        {
            var spawnPoints = new List<Transform>(EnemySpawnPositions.Length);
            for (var i = 0; i < EnemySpawnPositions.Length; i++)
            {
                spawnPoints.Add(CreateSpawnPoint(root, $"SP_Enemy_{i + 1:00}", EnemySpawnPositions[i]));
            }

            return spawnPoints;
        }

        private static Transform CreateExitGate(Transform parent)
        {
            var exitGate = CreateChild(parent, RoomPrefabContract.ExitGateName);
            exitGate.localPosition = new Vector3(0f, 0f, 8.55f);
            var visual = InstantiateModelOrCube(
                LoadModel("wall_doorway_door") ?? LoadModel("wall_doorway"),
                "ExitGateVisual",
                exitGate,
                Vector3.zero,
                new Vector3(2f, 2.4f, 0.35f),
                GetOrCreateMaterial("MAT_KayKitRoom_GateFallback", new Color(0.58f, 0.34f, 0.22f, 1f)));
            visual.transform.localRotation = Quaternion.identity;

            var trigger = exitGate.gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.2f, 0f);
            trigger.size = new Vector3(3f, 2.4f, 1.25f);
            return exitGate;
        }

        private static Transform CreateCameraBounds(Transform parent)
        {
            var boundsRoot = CreateChild(parent, RoomPrefabContract.CameraBoundsName);
            var cameraBounds = boundsRoot.gameObject.AddComponent<CameraBounds>();
            var serializedObject = new SerializedObject(cameraBounds);
            serializedObject.FindProperty("center").vector3Value = new Vector3(0f, 0f, -11.5f);
            serializedObject.FindProperty("size").vector3Value = new Vector3(1.5f, 0f, 10f);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return boundsRoot;
        }

        private static Transform CreateSpawnPoint(Transform parent, string name, Vector3 localPosition)
        {
            var spawn = CreateChild(parent, name);
            spawn.localPosition = localPosition;
            spawn.localRotation = Quaternion.identity;
            return spawn;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static GameObject InstantiateModelOrCube(
            GameObject model,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 fallbackScale,
            Material fallbackMaterial)
        {
            GameObject instance;
            if (model != null)
            {
                instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(model);
                }
            }
            else
            {
                instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                instance.transform.localScale = fallbackScale;
                if (fallbackMaterial != null && instance.TryGetComponent<Renderer>(out var renderer))
                {
                    renderer.sharedMaterial = fallbackMaterial;
                }
            }

            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            return instance;
        }

        private static void CreateBoxCollider(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var colliderObject = new GameObject(name);
            colliderObject.transform.SetParent(parent, false);
            colliderObject.transform.localPosition = center;
            var boxCollider = colliderObject.AddComponent<BoxCollider>();
            boxCollider.size = size;
        }

        private static GameObject LoadModel(string assetName)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{KayKitModelFolder}/{assetName}.fbx");
        }

        private static Material GetOrCreateMaterial(string materialName, Color color)
        {
            var path = $"{MaterialFolder}/{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
            {
                name = materialName,
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            Debug.Log($"{nameof(KayKitRoomPrefabBuilder)} created material asset at {path}.", material);
            return material;
        }

        private static void MarkEnvironmentStatic(GameObject root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(
                    transforms[i].gameObject,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
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
    }
}
