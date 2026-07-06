using System.Collections.Generic;
using TapKnockout.Camera;
using TapKnockout.Player;
using TapKnockout.Room;
using TapKnockout.Wave;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.Editor.Tools
{
    public static class RoomScenePrefabWireTool
    {
        private const string WireMenuPath = "Tools/Tap Knockout/Room/Wire KayKit Room Into Open Scene";
        private const string ValidateMenuPath = "Tools/Tap Knockout/Room/Validate Open Scene Room Wiring";
        private const string RoomPrefabPath = "Assets/_Project/Prefabs/Rooms/PF_Room_KayKit_Combat_01.prefab";
        private const string GameplayCameraConfigPath = "Assets/_Project/ScriptableObjects/Camera/GameplayCameraConfig_Default.asset";
        private const string EnemyRuntimeRootName = "EnemyRuntimeRoot";
        private const string LegacyGroundName = "Ground_Test";
        private const string LegacySpawnRootName = "SpawnPoints";
        private const string LegacyCameraBoundsName = "CameraBounds";

        [MenuItem(WireMenuPath)]
        public static void WireKayKitRoomIntoOpenScene()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Room Scene Wiring",
                    "Stop Play Mode before wiring scene room references.",
                    "OK");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RoomPrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog(
                    "Room Scene Wiring",
                    $"Room prefab was not found.\n\nRun Tools > Tap Knockout > Room > Create KayKit Combat Room Prefab first.\n\nExpected path:\n{RoomPrefabPath}",
                    "OK");
                return;
            }

            var roomManager = ResolveRoomManager();
            if (roomManager == null)
            {
                EditorUtility.DisplayDialog(
                    "Room Scene Wiring",
                    "No RoomManager was found in the open scene. Select RoomRoot_Test or add RoomManager before running this tool.",
                    "OK");
                return;
            }

            var contract = EnsureRoomPrefabInstance(roomManager.transform, prefab, out var createdPrefabInstance);
            if (contract == null)
            {
                EditorUtility.DisplayDialog(
                    "Room Scene Wiring",
                    "Room prefab could not be instantiated or resolved. Check Console for details.",
                    "OK");
                return;
            }

            if (!contract.TryValidate(out var contractMessage))
            {
                EditorUtility.DisplayDialog(
                    "Room Scene Wiring",
                    $"Room prefab contract is invalid:\n\n{contractMessage}",
                    "OK");
                return;
            }

            var player = ResolvePlayerTransform();
            var enemySpawner = roomManager.GetComponent<EnemySpawner>() ?? Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include);
            var cameraController = Object.FindFirstObjectByType<GameplayCameraController>(FindObjectsInactive.Include);
            var cameraConfig = AssetDatabase.LoadAssetAtPath<GameplayCameraConfig>(GameplayCameraConfigPath);
            var cameraBounds = contract.CameraBounds != null ? contract.CameraBounds.GetComponent<CameraBounds>() : null;
            var enemySpawnPoints = ResolveEnemySpawnPoints(contract);
            var enemyRuntimeRoot = EnsureEnemyRuntimeRoot(roomManager.transform);

            if (player != null)
            {
                MovePlayerToSpawn(player, contract.PlayerSpawn);
            }
            else
            {
                Debug.LogWarning($"{nameof(RoomScenePrefabWireTool)} could not find a player transform. EnemySpawner.playerTarget and camera follow target were left unchanged.");
            }

            if (enemySpawner != null)
            {
                WireEnemySpawner(enemySpawner, enemySpawnPoints, player, enemyRuntimeRoot);
            }
            else
            {
                Debug.LogWarning($"{nameof(RoomScenePrefabWireTool)} could not find EnemySpawner in the open scene.");
            }

            if (cameraController != null && cameraBounds != null)
            {
                WireCamera(cameraController, player, cameraBounds, cameraConfig);
            }
            else
            {
                Debug.LogWarning($"{nameof(RoomScenePrefabWireTool)} could not find GameplayCameraController or prefab CameraBounds component.");
            }

            var legacyObjects = ResolveLegacyObjects(roomManager.transform, contract.transform);
            var deactivatedLegacyCount = MaybeDeactivateLegacyObjects(legacyObjects);

            EditorUtility.SetDirty(roomManager);
            EditorSceneManager.MarkSceneDirty(roomManager.gameObject.scene);

            Selection.activeGameObject = contract.gameObject;
            EditorGUIUtility.PingObject(contract.gameObject);
            Debug.Log(
                $"{nameof(RoomScenePrefabWireTool)} wired {contract.name} into scene {roomManager.gameObject.scene.name}. " +
                $"CreatedPrefabInstance={createdPrefabInstance}, SpawnPoints={enemySpawnPoints.Count}, LegacyObjectsDeactivated={deactivatedLegacyCount}.",
                contract);

            EditorUtility.DisplayDialog(
                "Room Scene Wiring",
                $"Wired room prefab into {roomManager.name}.\n\n" +
                $"Spawn points: {enemySpawnPoints.Count}\n" +
                $"Player: {(player != null ? player.name : "<not found>")}\n" +
                $"Camera bounds: {(cameraBounds != null ? cameraBounds.name : "<not found>")}\n" +
                $"Legacy objects deactivated: {deactivatedLegacyCount}\n\n" +
                "Use File > Save after reviewing the scene.",
                "OK");
        }

        [MenuItem(ValidateMenuPath)]
        public static void ValidateOpenSceneRoomWiring()
        {
            var roomManager = ResolveRoomManager();
            if (roomManager == null)
            {
                EditorUtility.DisplayDialog("Room Scene Wiring", "No RoomManager was found in the open scene.", "OK");
                return;
            }

            var contract = ResolveRoomContract(roomManager.transform);
            if (contract == null)
            {
                EditorUtility.DisplayDialog("Room Scene Wiring", $"No RoomPrefabContract was found under {roomManager.name}.", "OK");
                return;
            }

            var issues = new List<string>();
            if (!contract.TryValidate(out var contractMessage))
            {
                issues.Add(contractMessage);
            }

            ValidateSpawner(roomManager, contract, issues);
            ValidateCamera(contract, issues);

            var activeLegacyObjects = ResolveLegacyObjects(roomManager.transform, contract.transform);
            for (var i = 0; i < activeLegacyObjects.Count; i++)
            {
                if (activeLegacyObjects[i] != null && activeLegacyObjects[i].activeInHierarchy)
                {
                    issues.Add($"Legacy object is still active: {GetHierarchyPath(activeLegacyObjects[i].transform)}");
                }
            }

            var isValid = issues.Count == 0;
            EditorUtility.DisplayDialog(
                "Room Scene Wiring",
                isValid
                    ? $"Open scene room wiring is valid.\n\n{contractMessage}"
                    : "Open scene room wiring needs attention:\n\n" + string.Join("\n", issues),
                isValid ? "OK" : "Fix");
        }

        private static RoomManager ResolveRoomManager()
        {
            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                var selectedRoomManager = selected.GetComponentInParent<RoomManager>();
                if (selectedRoomManager != null)
                {
                    return selectedRoomManager;
                }
            }

            var roomRootTest = GameObject.Find("RoomRoot_Test");
            if (roomRootTest != null && roomRootTest.TryGetComponent<RoomManager>(out var testRoomManager))
            {
                return testRoomManager;
            }

            return Object.FindFirstObjectByType<RoomManager>(FindObjectsInactive.Include);
        }

        private static RoomPrefabContract EnsureRoomPrefabInstance(Transform roomRoot, GameObject prefab, out bool created)
        {
            created = false;
            var existingContract = ResolveRoomContract(roomRoot);
            if (existingContract != null)
            {
                NormalizeRoomInstanceName(existingContract.transform);
                return existingContract;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, roomRoot) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"{nameof(RoomScenePrefabWireTool)} failed to instantiate prefab at {RoomPrefabPath}.");
                return null;
            }

            created = true;
            Undo.RegisterCreatedObjectUndo(instance, "Wire KayKit Room Prefab");
            instance.name = RoomPrefabContract.RoomRootName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance.GetComponentInChildren<RoomPrefabContract>(true);
        }

        private static RoomPrefabContract ResolveRoomContract(Transform roomRoot)
        {
            if (roomRoot == null)
            {
                return null;
            }

            var contracts = Object.FindObjectsByType<RoomPrefabContract>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            RoomPrefabContract fallback = null;
            for (var i = 0; i < contracts.Length; i++)
            {
                var contract = contracts[i];
                if (contract == null || !contract.transform.IsChildOf(roomRoot))
                {
                    continue;
                }

                var source = PrefabUtility.GetCorrespondingObjectFromSource(contract.gameObject);
                var sourcePath = source != null ? AssetDatabase.GetAssetPath(source) : string.Empty;
                if (sourcePath == RoomPrefabPath)
                {
                    return contract;
                }

                fallback ??= contract;
            }

            return fallback;
        }

        private static Transform ResolvePlayerTransform()
        {
            var playerHealth = Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
            if (playerHealth != null)
            {
                return playerHealth.transform;
            }

            var playerMovement = Object.FindFirstObjectByType<PlayerMovementController>(FindObjectsInactive.Include);
            if (playerMovement != null)
            {
                return playerMovement.transform;
            }

            try
            {
                var taggedPlayer = GameObject.FindWithTag("Player");
                if (taggedPlayer != null)
                {
                    return taggedPlayer.transform;
                }
            }
            catch (UnityException)
            {
                // The project may not define a Player tag yet.
            }

            var namedPlayer = GameObject.Find("Player");
            return namedPlayer != null ? namedPlayer.transform : null;
        }

        private static List<Transform> ResolveEnemySpawnPoints(RoomPrefabContract contract)
        {
            var spawnPoints = new List<Transform>();
            if (contract == null || contract.EnemySpawnPoints == null)
            {
                return spawnPoints;
            }

            for (var i = 0; i < contract.EnemySpawnPoints.Count; i++)
            {
                if (contract.EnemySpawnPoints[i] != null)
                {
                    spawnPoints.Add(contract.EnemySpawnPoints[i]);
                }
            }

            return spawnPoints;
        }

        private static Transform EnsureEnemyRuntimeRoot(Transform roomRoot)
        {
            var existing = roomRoot.Find(EnemyRuntimeRootName);
            if (existing != null)
            {
                return existing;
            }

            var runtimeRoot = new GameObject(EnemyRuntimeRootName);
            Undo.RegisterCreatedObjectUndo(runtimeRoot, "Create Enemy Runtime Root");
            runtimeRoot.transform.SetParent(roomRoot, false);
            runtimeRoot.transform.localPosition = Vector3.zero;
            return runtimeRoot.transform;
        }

        private static void MovePlayerToSpawn(Transform player, Transform playerSpawn)
        {
            if (player == null || playerSpawn == null)
            {
                return;
            }

            Undo.RecordObject(player, "Move Player To Room Spawn");
            player.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
            EditorUtility.SetDirty(player);
        }

        private static void WireEnemySpawner(
            EnemySpawner enemySpawner,
            IReadOnlyList<Transform> spawnPoints,
            Transform player,
            Transform spawnParent)
        {
            Undo.RecordObject(enemySpawner, "Wire Room Enemy Spawner");
            var serializedObject = new SerializedObject(enemySpawner);
            var spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
            spawnPointsProperty.arraySize = spawnPoints.Count;
            for (var i = 0; i < spawnPoints.Count; i++)
            {
                spawnPointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
            }

            if (player != null)
            {
                serializedObject.FindProperty("playerTarget").objectReferenceValue = player;
            }

            serializedObject.FindProperty("spawnParent").objectReferenceValue = spawnParent;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(enemySpawner);
        }

        private static void WireCamera(
            GameplayCameraController cameraController,
            Transform player,
            CameraBounds cameraBounds,
            GameplayCameraConfig cameraConfig)
        {
            Undo.RecordObject(cameraController, "Wire Room Camera");
            var serializedObject = new SerializedObject(cameraController);
            if (cameraConfig != null)
            {
                serializedObject.FindProperty("config").objectReferenceValue = cameraConfig;
            }

            if (player != null)
            {
                serializedObject.FindProperty("followTarget").objectReferenceValue = player;
            }

            serializedObject.FindProperty("cameraBounds").objectReferenceValue = cameraBounds;
            serializedObject.ApplyModifiedProperties();
            cameraController.SetBounds(cameraBounds);
            if (player != null)
            {
                Undo.RecordObject(cameraController.transform, "Snap Camera To Room");
                cameraController.SetFollowTarget(player, true);
                EditorUtility.SetDirty(cameraController.transform);
            }

            EditorUtility.SetDirty(cameraController);
        }

        private static List<GameObject> ResolveLegacyObjects(Transform roomRoot, Transform activeRoomPrefabRoot)
        {
            var legacyObjects = new List<GameObject>();
            AddLegacyObjectByName(legacyObjects, LegacyGroundName, activeRoomPrefabRoot);
            AddLegacyObjectByName(legacyObjects, LegacySpawnRootName, activeRoomPrefabRoot);
            AddLegacyObjectByName(legacyObjects, LegacyCameraBoundsName, activeRoomPrefabRoot);

            if (roomRoot == null)
            {
                return legacyObjects;
            }

            for (var i = 0; i < roomRoot.childCount; i++)
            {
                var child = roomRoot.GetChild(i);
                if (child == null || child == activeRoomPrefabRoot || child.IsChildOf(activeRoomPrefabRoot))
                {
                    continue;
                }

                if (child.name == LegacySpawnRootName || child.name == LegacyCameraBoundsName)
                {
                    AddUnique(legacyObjects, child.gameObject);
                }
            }

            return legacyObjects;
        }

        private static void AddLegacyObjectByName(List<GameObject> legacyObjects, string objectName, Transform activeRoomPrefabRoot)
        {
            var candidates = Resources.FindObjectsOfTypeAll<GameObject>();
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || candidate.name != objectName)
                {
                    continue;
                }

                if (EditorUtility.IsPersistent(candidate) || !candidate.scene.IsValid())
                {
                    continue;
                }

                if (activeRoomPrefabRoot != null && candidate.transform.IsChildOf(activeRoomPrefabRoot))
                {
                    continue;
                }

                AddUnique(legacyObjects, candidate);
            }
        }

        private static void AddUnique(List<GameObject> objects, GameObject candidate)
        {
            if (candidate != null && !objects.Contains(candidate))
            {
                objects.Add(candidate);
            }
        }

        private static int MaybeDeactivateLegacyObjects(IReadOnlyList<GameObject> legacyObjects)
        {
            if (legacyObjects == null || legacyObjects.Count == 0)
            {
                return 0;
            }

            var activeLegacyObjects = new List<GameObject>();
            for (var i = 0; i < legacyObjects.Count; i++)
            {
                if (legacyObjects[i] != null && legacyObjects[i].activeSelf)
                {
                    activeLegacyObjects.Add(legacyObjects[i]);
                }
            }

            if (activeLegacyObjects.Count == 0)
            {
                return 0;
            }

            var message = "Deactivate legacy test objects now?\n\n" + BuildObjectList(activeLegacyObjects) +
                "\n\nThey will not be deleted. You can reactivate them from the Hierarchy if needed.";
            var shouldDeactivate = EditorUtility.DisplayDialog(
                "Room Scene Wiring",
                message,
                "Deactivate",
                "Keep Active");

            if (!shouldDeactivate)
            {
                return 0;
            }

            for (var i = 0; i < activeLegacyObjects.Count; i++)
            {
                Undo.RecordObject(activeLegacyObjects[i], "Deactivate Legacy Room Object");
                activeLegacyObjects[i].SetActive(false);
                EditorUtility.SetDirty(activeLegacyObjects[i]);
            }

            return activeLegacyObjects.Count;
        }

        private static void NormalizeRoomInstanceName(Transform roomInstanceRoot)
        {
            if (roomInstanceRoot == null || roomInstanceRoot.name == RoomPrefabContract.RoomRootName)
            {
                return;
            }

            Undo.RecordObject(roomInstanceRoot.gameObject, "Normalize Room Instance Name");
            roomInstanceRoot.name = RoomPrefabContract.RoomRootName;
            EditorUtility.SetDirty(roomInstanceRoot.gameObject);
        }

        private static void ValidateSpawner(RoomManager roomManager, RoomPrefabContract contract, List<string> issues)
        {
            var enemySpawner = roomManager.GetComponent<EnemySpawner>() ?? Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include);
            if (enemySpawner == null)
            {
                issues.Add("EnemySpawner was not found.");
                return;
            }

            var expectedSpawnPoints = ResolveEnemySpawnPoints(contract);
            var serializedObject = new SerializedObject(enemySpawner);
            var spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
            if (spawnPointsProperty.arraySize != expectedSpawnPoints.Count)
            {
                issues.Add($"EnemySpawner has {spawnPointsProperty.arraySize} spawn points, expected {expectedSpawnPoints.Count}.");
                return;
            }

            for (var i = 0; i < expectedSpawnPoints.Count; i++)
            {
                if (spawnPointsProperty.GetArrayElementAtIndex(i).objectReferenceValue != expectedSpawnPoints[i])
                {
                    issues.Add($"EnemySpawner spawn point {i} is not wired to the room prefab.");
                }
            }
        }

        private static void ValidateCamera(RoomPrefabContract contract, List<string> issues)
        {
            var cameraController = Object.FindFirstObjectByType<GameplayCameraController>(FindObjectsInactive.Include);
            if (cameraController == null)
            {
                issues.Add("GameplayCameraController was not found.");
                return;
            }

            var expectedBounds = contract.CameraBounds != null ? contract.CameraBounds.GetComponent<CameraBounds>() : null;
            if (expectedBounds == null)
            {
                issues.Add("Room prefab CameraBounds component was not found.");
                return;
            }

            if (cameraController.Bounds != expectedBounds)
            {
                issues.Add("GameplayCameraController is not wired to the room prefab CameraBounds.");
            }

            var expectedConfig = AssetDatabase.LoadAssetAtPath<GameplayCameraConfig>(GameplayCameraConfigPath);
            if (expectedConfig != null && cameraController.Config != expectedConfig)
            {
                issues.Add("GameplayCameraController is not using GameplayCameraConfig_Default.");
            }
        }

        private static string BuildObjectList(IReadOnlyList<GameObject> objects)
        {
            var names = new string[objects.Count];
            for (var i = 0; i < objects.Count; i++)
            {
                names[i] = "- " + GetHierarchyPath(objects[i].transform);
            }

            return string.Join("\n", names);
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            var path = transform.name;
            var current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
