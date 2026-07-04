using TapKnockout.Camera;
using TapKnockout.Level;
using TapKnockout.Player;
using TapKnockout.Room;
using TapKnockout.Wave;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TapKnockout.Editor.Tools
{
    public static class GameplayScenePipelineBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/Scene/Create Gameplay Scene Pipeline";

        [MenuItem(MenuPath)]
        public static void CreateGameplayScenePipeline()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Gameplay Scene Pipeline", "Exit Play Mode before creating scene pipeline objects.", "OK");
                return;
            }

            var gameplayRoot = EnsureRoot("GameplayRoot");
            var systemsRoot = EnsureChild(gameplayRoot.transform, "GameSystems");
            var roomInstanceRoot = EnsureChild(gameplayRoot.transform, "RoomInstanceRoot");

            var roomManager = ResolveOrAdd<RoomManager>(systemsRoot);
            var waveManager = ResolveOrAdd<WaveManager>(systemsRoot);
            var enemySpawner = ResolveOrAdd<EnemySpawner>(systemsRoot);
            var chapterRunner = ResolveOrAdd<ChapterRunner>(systemsRoot);
            var rewardFlow = ResolveOrAdd<ChapterRoomRewardFlowController>(systemsRoot);
            var runtimeRoomLoader = ResolveOrAdd<RuntimeRoomLoader>(systemsRoot);
            var cameraController = Object.FindFirstObjectByType<GameplayCameraController>(FindObjectsInactive.Include);
            var player = ResolvePlayer();

            WireRoomManager(roomManager, waveManager);
            WireChapterRunner(chapterRunner, roomManager, runtimeRoomLoader);
            WireRewardFlow(rewardFlow, chapterRunner, roomManager);
            WireRuntimeRoomLoader(runtimeRoomLoader, roomInstanceRoot.transform, enemySpawner, cameraController, player);

            EditorUtility.SetDirty(roomManager);
            EditorUtility.SetDirty(waveManager);
            EditorUtility.SetDirty(enemySpawner);
            EditorUtility.SetDirty(chapterRunner);
            EditorUtility.SetDirty(rewardFlow);
            EditorUtility.SetDirty(runtimeRoomLoader);
            EditorSceneManager.MarkSceneDirty(gameplayRoot.scene);

            Selection.activeGameObject = runtimeRoomLoader.gameObject;
            EditorGUIUtility.PingObject(runtimeRoomLoader.gameObject);
            EditorUtility.DisplayDialog(
                "Gameplay Scene Pipeline",
                "Gameplay scene pipeline objects were created/wired. Review references in Inspector, then use File > Save.",
                "OK");
        }

        private static GameObject EnsureRoot(string rootName)
        {
            var existing = GameObject.Find(rootName);
            if (existing != null)
            {
                return existing;
            }

            var root = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Gameplay Scene Pipeline Root");
            return root;
        }

        private static GameObject EnsureChild(Transform parent, string childName)
        {
            var existing = parent.Find(childName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, "Create Gameplay Scene Pipeline Child");
            child.transform.SetParent(parent, false);
            return child;
        }

        private static T ResolveOrAdd<T>(GameObject host) where T : Component
        {
            var existing = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return existing;
            }

            return Undo.AddComponent<T>(host);
        }

        private static Transform ResolvePlayer()
        {
            var playerHealth = Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
            if (playerHealth != null)
            {
                return playerHealth.transform;
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

            return null;
        }

        private static void WireRoomManager(RoomManager roomManager, WaveManager waveManager)
        {
            var serializedObject = new SerializedObject(roomManager);
            SetObject(serializedObject, "waveManager", waveManager);
            serializedObject.ApplyModifiedProperties();
        }

        private static void WireChapterRunner(ChapterRunner chapterRunner, RoomManager roomManager, RuntimeRoomLoader loader)
        {
            var serializedObject = new SerializedObject(chapterRunner);
            SetObject(serializedObject, "roomManager", roomManager);
            SetObject(serializedObject, "runtimeRoomLoader", loader);
            serializedObject.ApplyModifiedProperties();
        }

        private static void WireRewardFlow(ChapterRoomRewardFlowController rewardFlow, ChapterRunner chapterRunner, RoomManager roomManager)
        {
            var serializedObject = new SerializedObject(rewardFlow);
            SetObject(serializedObject, "chapterRunner", chapterRunner);
            SetObject(serializedObject, "roomManager", roomManager);
            serializedObject.ApplyModifiedProperties();
        }

        private static void WireRuntimeRoomLoader(
            RuntimeRoomLoader loader,
            Transform roomInstanceRoot,
            EnemySpawner enemySpawner,
            GameplayCameraController cameraController,
            Transform player)
        {
            var serializedObject = new SerializedObject(loader);
            SetObject(serializedObject, "roomInstanceRoot", roomInstanceRoot);
            SetObject(serializedObject, "enemySpawner", enemySpawner);
            SetObject(serializedObject, "gameplayCameraController", cameraController);
            SetObject(serializedObject, "playerTransform", player);
            serializedObject.ApplyModifiedProperties();
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
