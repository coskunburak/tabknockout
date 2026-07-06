using System;
using TapKnockout.Camera;
using TapKnockout.Wave;
using UnityEngine;

namespace TapKnockout.Room
{
    public readonly struct RuntimeRoomLoadedEventArgs
    {
        public RuntimeRoomLoadedEventArgs(RuntimeRoomLoader source, RoomTemplateConfig roomConfig, RoomInstanceController roomInstance)
        {
            Source = source;
            RoomConfig = roomConfig;
            RoomInstance = roomInstance;
            RoomContract = roomInstance != null ? roomInstance.Contract : null;
        }

        public RuntimeRoomLoader Source { get; }
        public RoomTemplateConfig RoomConfig { get; }
        public RoomInstanceController RoomInstance { get; }
        public RoomPrefabContract RoomContract { get; }
    }

    public readonly struct RuntimeRoomUnloadedEventArgs
    {
        public RuntimeRoomUnloadedEventArgs(RuntimeRoomLoader source, RoomInstanceController previousRoomInstance)
        {
            Source = source;
            PreviousRoomInstance = previousRoomInstance;
            PreviousRoomContract = previousRoomInstance != null ? previousRoomInstance.Contract : null;
        }

        public RuntimeRoomLoader Source { get; }
        public RoomInstanceController PreviousRoomInstance { get; }
        public RoomPrefabContract PreviousRoomContract { get; }
    }

    [DisallowMultipleComponent]
    public sealed class RuntimeRoomLoader : MonoBehaviour
    {
        [SerializeField] private Transform roomInstanceRoot;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private GameplayCameraController gameplayCameraController;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private RoomInstanceController sceneRoomInstance;
        [SerializeField] private bool useExistingSceneRoomInstance = true;
        [SerializeField] private bool destroyPreviousRoom = true;
        [SerializeField] private bool movePlayerToEntryPoint = true;
        [SerializeField] private bool snapCameraOnLoad = true;
        [SerializeField, Min(0f)] private float playerSpawnGroundClearance = 0.03f;
        [SerializeField] private bool logDebug;

        private GameObject activeRoomObject;

        public event Action<RuntimeRoomLoadedEventArgs> OnRoomLoaded;
        public event Action<RuntimeRoomUnloadedEventArgs> OnRoomUnloaded;

        public RoomInstanceController ActiveRoomInstance { get; private set; }
        public RoomPrefabContract ActiveRoomContract => ActiveRoomInstance != null ? ActiveRoomInstance.Contract : null;
        public Transform RoomInstanceRoot => roomInstanceRoot != null ? roomInstanceRoot : transform;

        private void Reset()
        {
            roomInstanceRoot = transform;
            enemySpawner = GetComponent<EnemySpawner>();
            sceneRoomInstance = GetComponentInChildren<RoomInstanceController>(true);
        }

        private void Awake()
        {
            if (roomInstanceRoot == null)
            {
                roomInstanceRoot = transform;
            }
        }

        public void SetReferences(
            Transform instanceRoot,
            EnemySpawner spawner,
            GameplayCameraController cameraController,
            Transform player)
        {
            roomInstanceRoot = instanceRoot;
            enemySpawner = spawner;
            gameplayCameraController = cameraController;
            playerTransform = player;
        }

        public void SetSceneRoomInstance(RoomInstanceController instance)
        {
            sceneRoomInstance = instance;
        }

        public void SetUseExistingSceneRoomInstance(bool isEnabled)
        {
            useExistingSceneRoomInstance = isEnabled;
        }

        public RoomInstanceController LoadRoom(RoomTemplateConfig roomConfig)
        {
            UnloadActiveRoom();

            if (roomConfig == null)
            {
                LogWarning("Cannot load a room because the RoomTemplateConfig is missing.");
                return null;
            }

            var existingSceneRoom = ResolveExistingSceneRoomInstance(roomConfig);
            if (existingSceneRoom != null)
            {
                return UseExistingSceneRoom(roomConfig, existingSceneRoom);
            }

            if (roomConfig.RoomPrefab == null)
            {
                LogWarning($"Room {roomConfig.RoomId} has no room prefab. Falling back to existing scene/test room flow.");
                ApplyFallbackBindings(null);
                return null;
            }

            var instance = Instantiate(roomConfig.RoomPrefab, RoomInstanceRoot);
            instance.name = roomConfig.RoomPrefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var contract = instance.GetComponentInChildren<RoomPrefabContract>(true);
            if (contract == null)
            {
                LogWarning($"Room prefab {roomConfig.RoomPrefab.name} has no {nameof(RoomPrefabContract)}. Destroying generated instance and falling back to scene flow.");
                DestroyRoomObject(instance);
                ApplyFallbackBindings(null);
                return null;
            }

            var controller = instance.GetComponent<RoomInstanceController>();
            if (controller == null)
            {
                controller = instance.AddComponent<RoomInstanceController>();
            }

            controller.Initialize(contract);
            ActiveRoomInstance = controller;
            activeRoomObject = instance;
            ApplyLoadedRoomBindings(contract);

            var loadedArgs = new RuntimeRoomLoadedEventArgs(this, roomConfig, controller);
            OnRoomLoaded?.Invoke(loadedArgs);

            if (logDebug)
            {
                Debug.Log($"{nameof(RuntimeRoomLoader)} loaded {roomConfig.RoomId} from prefab {roomConfig.RoomPrefab.name}.", this);
            }

            return controller;
        }

        public void UnloadActiveRoom()
        {
            if (ActiveRoomInstance == null && activeRoomObject == null)
            {
                return;
            }

            var previous = ActiveRoomInstance;
            var objectToRelease = activeRoomObject;

            if (objectToRelease != null)
            {
                if (destroyPreviousRoom)
                {
                    DestroyRoomObject(objectToRelease);
                }
                else
                {
                    objectToRelease.SetActive(false);
                }
            }

            ActiveRoomInstance = null;
            activeRoomObject = null;
            OnRoomUnloaded?.Invoke(new RuntimeRoomUnloadedEventArgs(this, previous));
        }

        private RoomInstanceController UseExistingSceneRoom(RoomTemplateConfig roomConfig, RoomInstanceController roomInstance)
        {
            if (roomInstance == null)
            {
                return null;
            }

            var contract = roomInstance.Contract != null
                ? roomInstance.Contract
                : roomInstance.GetComponentInChildren<RoomPrefabContract>(true);
            if (contract == null)
            {
                return null;
            }

            roomInstance.Initialize(contract);
            ActiveRoomInstance = roomInstance;
            activeRoomObject = null;
            ApplyLoadedRoomBindings(contract);

            var loadedArgs = new RuntimeRoomLoadedEventArgs(this, roomConfig, roomInstance);
            OnRoomLoaded?.Invoke(loadedArgs);

            if (logDebug)
            {
                Debug.Log($"{nameof(RuntimeRoomLoader)} using existing scene room {contract.name} for {roomConfig.RoomId}.", this);
            }

            return roomInstance;
        }

        private RoomInstanceController ResolveExistingSceneRoomInstance(RoomTemplateConfig roomConfig)
        {
            if (!useExistingSceneRoomInstance || roomConfig == null || roomConfig.RoomPrefab == null)
            {
                return null;
            }

            if (IsUsableExistingSceneRoom(sceneRoomInstance, roomConfig))
            {
                return sceneRoomInstance;
            }

            var contracts = FindObjectsByType<RoomPrefabContract>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < contracts.Length; i++)
            {
                var contract = contracts[i];
                if (!IsMatchingSceneContract(contract, roomConfig))
                {
                    continue;
                }

                var controller = contract.GetComponent<RoomInstanceController>();
                if (controller == null)
                {
                    controller = contract.gameObject.AddComponent<RoomInstanceController>();
                }

                controller.Initialize(contract);
                sceneRoomInstance = controller;
                return sceneRoomInstance;
            }

            return null;
        }

        private bool IsUsableExistingSceneRoom(RoomInstanceController instance, RoomTemplateConfig roomConfig)
        {
            if (instance == null)
            {
                return false;
            }

            var contract = instance.Contract != null
                ? instance.Contract
                : instance.GetComponentInChildren<RoomPrefabContract>(true);
            return IsMatchingSceneContract(contract, roomConfig);
        }

        private bool IsMatchingSceneContract(RoomPrefabContract contract, RoomTemplateConfig roomConfig)
        {
            if (contract == null || roomConfig == null || roomConfig.RoomPrefab == null)
            {
                return false;
            }

            var root = RoomInstanceRoot;
            if (root != null && contract.transform.IsChildOf(root))
            {
                return false;
            }

            var expectedPrefabName = roomConfig.RoomPrefab.name;
            return string.Equals(NormalizeInstanceName(contract.gameObject.name), expectedPrefabName, StringComparison.Ordinal)
                || string.Equals(contract.DebugLabel, expectedPrefabName, StringComparison.Ordinal);
        }

        private static string NormalizeInstanceName(string value)
        {
            const string cloneSuffix = "(Clone)";
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim();
            return normalized.EndsWith(cloneSuffix, StringComparison.Ordinal)
                ? normalized.Substring(0, normalized.Length - cloneSuffix.Length).Trim()
                : normalized;
        }

        private void ApplyLoadedRoomBindings(RoomPrefabContract contract)
        {
            if (contract == null)
            {
                ApplyFallbackBindings(null);
                return;
            }

            if (enemySpawner != null)
            {
                enemySpawner.SetSpawnPoints(contract.GetEnemySpawnPointArray());
                enemySpawner.SetSpawnParent(contract.GameplayRoot != null ? contract.GameplayRoot : contract.transform);
            }

            if (movePlayerToEntryPoint && playerTransform != null && contract.GetPlayerEntryPoint() != null)
            {
                MovePlayerToEntryPoint(contract.GetPlayerEntryPoint());
            }

            var cameraBounds = contract.GetCameraBounds();
            if (gameplayCameraController != null && cameraBounds != null)
            {
                gameplayCameraController.SetRoomBounds(cameraBounds);
                if (snapCameraOnLoad)
                {
                    gameplayCameraController.SnapToTarget();
                }
            }
        }

        private void ApplyFallbackBindings(RoomPrefabContract contract)
        {
            if (contract == null && enemySpawner != null)
            {
                enemySpawner.SetSpawnParent(null);
            }
        }

        private void MovePlayerToEntryPoint(Transform entryPoint)
        {
            var targetPosition = CalculatePlayerRootPositionForSpawn(
                entryPoint.position,
                playerTransform,
                playerSpawnGroundClearance);

            if (playerTransform.TryGetComponent<Rigidbody>(out var playerRigidbody))
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }

            playerTransform.SetPositionAndRotation(targetPosition, entryPoint.rotation);
            Physics.SyncTransforms();
        }

        public static Vector3 CalculatePlayerRootPositionForSpawn(
            Vector3 spawnPosition,
            Transform player,
            float groundClearance = 0.03f)
        {
            if (player == null)
            {
                return spawnPosition;
            }

            var playerCollider = ResolvePlayerCollider(player);
            if (playerCollider == null)
            {
                return spawnPosition;
            }

            var colliderBottomOffset = playerCollider.bounds.min.y - player.position.y;
            var minimumBottomOffset = Mathf.Max(0f, groundClearance);
            if (colliderBottomOffset >= minimumBottomOffset)
            {
                return spawnPosition;
            }

            spawnPosition.y += minimumBottomOffset - colliderBottomOffset;
            return spawnPosition;
        }

        private static Collider ResolvePlayerCollider(Transform player)
        {
            if (player.TryGetComponent<Collider>(out var rootCollider) && !rootCollider.isTrigger)
            {
                return rootCollider;
            }

            var colliders = player.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && !colliders[i].isTrigger)
                {
                    return colliders[i];
                }
            }

            return null;
        }

        private void DestroyRoomObject(GameObject roomObject)
        {
            if (roomObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(roomObject);
            }
            else
            {
                DestroyImmediate(roomObject);
            }
        }

        private void LogWarning(string message)
        {
            if (logDebug)
            {
                Debug.LogWarning($"{nameof(RuntimeRoomLoader)} on {name}: {message}", this);
            }
        }
    }
}
