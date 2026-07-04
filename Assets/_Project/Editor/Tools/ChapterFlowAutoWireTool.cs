using System;
using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Level;
using TapKnockout.Player;
using TapKnockout.Room;
using TapKnockout.UI;
using TapKnockout.Wave;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class ChapterFlowAutoWireTool
    {
        private const string MenuPath = "Tools/Tap Knockout/Chapter/Wire Chapter Room Flow";
        private const string ValidateMenuPath = "Tools/Tap Knockout/Chapter/Validate Vertical Slice Room Flow";
        private const string DeactivateLegacyMenuPath = "Tools/Tap Knockout/Chapter/Deactivate Legacy Room Test Objects";
        private const string ChapterFolderPath = "Assets/_Project/ScriptableObjects/Chapters";
        private const string RoomFolderPath = "Assets/_Project/ScriptableObjects/Rooms";
        private const string DefaultChapterPath = global::TapKnockout.EditorTools.VerticalSliceChapterContentBuilder.ChapterPath;

        [MenuItem(MenuPath)]
        public static void WireChapterRoomFlow()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Chapter Room Flow",
                    "Stop Play Mode before wiring chapter flow.",
                    "OK");
                return;
            }

            var roomManager = ResolveRoomManager();
            if (roomManager == null)
            {
                EditorUtility.DisplayDialog(
                    "Chapter Room Flow",
                    "No RoomManager was found in the open scene.",
                    "OK");
                return;
            }

            var chapterConfig = GetOrCreateChapterConfig();
            var roomTemplates = ResolveRoomTemplates(roomManager, chapterConfig);
            if (roomTemplates.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Chapter Room Flow",
                    "No RoomTemplateConfig asset was found. Create at least one room template first.",
                    "OK");
                return;
            }

            ApplyRoomDefaults(roomTemplates);
            WireChapterConfig(chapterConfig, roomTemplates);

            var root = roomManager.gameObject;
            var chapterRunner = root.GetComponent<ChapterRunner>() ?? Undo.AddComponent<ChapterRunner>(root);
            var flowController = root.GetComponent<ChapterRoomRewardFlowController>() ?? Undo.AddComponent<ChapterRoomRewardFlowController>(root);
            var abilitySelectionController = UnityEngine.Object.FindFirstObjectByType<AbilitySelectionController>(FindObjectsInactive.Include);
            var abilitySelectionPanel = UnityEngine.Object.FindFirstObjectByType<AbilitySelectionPanelController>(FindObjectsInactive.Include);
            var playerHealth = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);

            WireChapterRunner(chapterRunner, chapterConfig, roomManager);
            WireRoomManager(roomManager, chapterConfig);
            WireFlowController(flowController, chapterRunner, roomManager, abilitySelectionController, abilitySelectionPanel, playerHealth);
            WireContinuePanel();
            DisableLegacyBridge(root);

            EditorUtility.SetDirty(chapterConfig);
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(root.scene);
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            EditorUtility.DisplayDialog(
                "Chapter Room Flow",
                $"Wired chapter flow on {root.name}.\n\nChapterConfig: {AssetDatabase.GetAssetPath(chapterConfig)}",
                "OK");
        }

        [MenuItem(ValidateMenuPath)]
        public static void ValidateVerticalSliceRoomFlow()
        {
            var issues = new List<string>();
            var chapterConfig = AssetDatabase.LoadAssetAtPath<ChapterConfig>(DefaultChapterPath);
            var roomManager = ResolveRoomManager();
            var chapterRunner = roomManager != null ? roomManager.GetComponent<ChapterRunner>() : null;
            var flowController = roomManager != null ? roomManager.GetComponent<ChapterRoomRewardFlowController>() : null;
            var waveManager = roomManager != null ? roomManager.GetComponent<WaveManager>() : null;
            var legacyBridge = roomManager != null ? roomManager.GetComponent<RoomAbilityRewardBridge>() : null;
            var continuePanel = UnityEngine.Object.FindFirstObjectByType<RoomContinuePanelController>(FindObjectsInactive.Include);
            var abilitySelectionController = UnityEngine.Object.FindFirstObjectByType<AbilitySelectionController>(FindObjectsInactive.Include);
            var abilitySelectionPanel = UnityEngine.Object.FindFirstObjectByType<AbilitySelectionPanelController>(FindObjectsInactive.Include);

            if (chapterConfig == null)
            {
                issues.Add($"Missing vertical slice chapter asset: {DefaultChapterPath}");
            }
            else if (chapterConfig.Rooms == null || chapterConfig.Rooms.Count != 10)
            {
                issues.Add("Chapter_VerticalSlice_01 must contain exactly 10 rooms.");
            }

            if (roomManager == null)
            {
                issues.Add("No RoomManager was found in the open scene.");
            }

            if (chapterRunner == null)
            {
                issues.Add("Room root is missing ChapterRunner.");
            }
            else if (chapterRunner.CurrentChapter != chapterConfig)
            {
                issues.Add("ChapterRunner Config is not Chapter_VerticalSlice_01.");
            }

            if (flowController == null)
            {
                issues.Add("Room root is missing ChapterRoomRewardFlowController.");
            }

            if (chapterConfig != null && chapterConfig.Rooms != null && chapterConfig.Rooms.Count > 0 && roomManager != null)
            {
                if (roomManager.CurrentRoom != chapterConfig.Rooms[0])
                {
                    issues.Add("RoomManager visible Config is not RoomTemplate_VS_01_AbilityIntro.");
                }

                if (waveManager == null)
                {
                    issues.Add("Room root is missing WaveManager.");
                }
                else if (chapterConfig.Rooms[0].Waves.Count > 0 && waveManager.CurrentWave != chapterConfig.Rooms[0].Waves[0])
                {
                    issues.Add("WaveManager visible Config is not Wave_VS_01_SmallMelee.");
                }
            }

            if (legacyBridge != null && legacyBridge.enabled)
            {
                issues.Add("Legacy RoomAbilityRewardBridge is still enabled.");
            }

            if (abilitySelectionController == null)
            {
                issues.Add("No AbilitySelectionController was found in the open scene.");
            }

            if (abilitySelectionPanel == null)
            {
                issues.Add("No AbilitySelectionPanelController was found in the open scene.");
            }

            if (continuePanel == null)
            {
                issues.Add("No RoomContinuePanelController was found in the open scene.");
            }
            else if (flowController != null && continuePanel.FlowController != flowController)
            {
                issues.Add("RoomContinuePanelController is not wired to ChapterRoomRewardFlowController.");
            }

            AddActiveLegacySceneObjectIssues(issues);

            var isValid = issues.Count == 0;
            EditorUtility.DisplayDialog(
                "Vertical Slice Room Flow",
                isValid
                    ? "Vertical slice room flow wiring is valid.\n\nChapterRunner: Chapter_VerticalSlice_01\nRoomManager: RoomTemplate_VS_01_AbilityIntro\nWaveManager: Wave_VS_01_SmallMelee\nRooms: 10"
                    : "Vertical slice room flow wiring needs attention:\n\n" + string.Join("\n", issues),
                isValid ? "OK" : "Fix");
        }

        [MenuItem(DeactivateLegacyMenuPath)]
        public static void DeactivateLegacyRoomTestObjects()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Vertical Slice Room Flow",
                    "Stop Play Mode before deactivating legacy scene objects.",
                    "OK");
                return;
            }

            var legacyObjects = CollectActiveLegacySceneObjects();
            if (legacyObjects.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Vertical Slice Room Flow",
                    "No active legacy room test objects were found.",
                    "OK");
                return;
            }

            var message = "Deactivate these legacy scene objects?\n\n" + BuildObjectList(legacyObjects) +
                "\n\nThey will stay in the scene but will no longer affect the vertical slice flow.";
            if (!EditorUtility.DisplayDialog("Vertical Slice Room Flow", message, "Deactivate", "Cancel"))
            {
                return;
            }

            for (var i = 0; i < legacyObjects.Count; i++)
            {
                Undo.RecordObject(legacyObjects[i], "Deactivate Legacy Room Test Object");
                legacyObjects[i].SetActive(false);
                EditorUtility.SetDirty(legacyObjects[i]);
                EditorSceneManager.MarkSceneDirty(legacyObjects[i].scene);
            }

            EditorUtility.DisplayDialog(
                "Vertical Slice Room Flow",
                $"Deactivated {legacyObjects.Count} legacy room test object(s). Use File > Save after reviewing the scene.",
                "OK");
        }

        private static void AddActiveLegacySceneObjectIssues(List<string> issues)
        {
            var legacyObjects = CollectActiveLegacySceneObjects();
            for (var i = 0; i < legacyObjects.Count; i++)
            {
                issues.Add($"Legacy scene object is still active: {GetHierarchyPath(legacyObjects[i].transform)}");
            }
        }

        private static List<GameObject> CollectActiveLegacySceneObjects()
        {
            var legacyObjects = new List<GameObject>();
            var sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (var i = 0; i < sceneObjects.Length; i++)
            {
                var sceneObject = sceneObjects[i];
                if (sceneObject != null
                    && !EditorUtility.IsPersistent(sceneObject)
                    && sceneObject.scene.IsValid()
                    && sceneObject.activeInHierarchy
                    && IsLegacySceneObject(sceneObject))
                {
                    legacyObjects.Add(sceneObject);
                }
            }

            return legacyObjects;
        }

        private static string BuildObjectList(IReadOnlyList<GameObject> objects)
        {
            var lines = new List<string>(objects.Count);
            for (var i = 0; i < objects.Count; i++)
            {
                lines.Add("- " + GetHierarchyPath(objects[i].transform));
            }

            return string.Join("\n", lines);
        }

        private static bool IsLegacySceneObject(GameObject sceneObject)
        {
            if (sceneObject.name == "Ground_Test" || sceneObject.name == "SpawnPoints")
            {
                return true;
            }

            return sceneObject.name == "CameraBounds"
                && sceneObject.GetComponentInParent<RoomPrefabContract>(true) == null;
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            var path = transform.name;
            var parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
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

            return UnityEngine.Object.FindFirstObjectByType<RoomManager>(FindObjectsInactive.Include);
        }

        private static ChapterConfig GetOrCreateChapterConfig()
        {
            var verticalSliceChapter = AssetDatabase.LoadAssetAtPath<ChapterConfig>(DefaultChapterPath);
            if (verticalSliceChapter != null)
            {
                return verticalSliceChapter;
            }

            var chapterGuids = AssetDatabase.FindAssets("t:ChapterConfig", new[] { ChapterFolderPath });
            if (chapterGuids.Length > 0)
            {
                var existingPath = ResolveFirstSortedAssetPath(chapterGuids);
                return AssetDatabase.LoadAssetAtPath<ChapterConfig>(existingPath);
            }

            EnsureFolder(ChapterFolderPath);
            var chapter = ScriptableObject.CreateInstance<ChapterConfig>();
            AssetDatabase.CreateAsset(chapter, AssetDatabase.GenerateUniqueAssetPath(DefaultChapterPath));

            var serializedObject = new SerializedObject(chapter);
            serializedObject.FindProperty("chapterId").stringValue = "chapter_vertical_slice_01";
            serializedObject.FindProperty("displayName").stringValue = "Vertical Slice 01";
            serializedObject.FindProperty("chapterIndex").intValue = 1;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return chapter;
        }

        private static List<RoomTemplateConfig> ResolveRoomTemplates(RoomManager roomManager, ChapterConfig chapterConfig)
        {
            var chapterRooms = ResolveDistinctChapterRooms(chapterConfig);
            if (chapterRooms.Count > 1)
            {
                return chapterRooms;
            }

            var templates = new List<RoomTemplateConfig>();
            var roomGuids = AssetDatabase.FindAssets("t:RoomTemplateConfig", new[] { RoomFolderPath });
            var sortedPaths = ResolveSortedAssetPaths(roomGuids);

            for (var i = 0; i < sortedPaths.Count; i++)
            {
                var roomTemplate = AssetDatabase.LoadAssetAtPath<RoomTemplateConfig>(sortedPaths[i]);
                if (roomTemplate != null && !templates.Contains(roomTemplate))
                {
                    templates.Add(roomTemplate);
                }
            }

            if (templates.Count == 0 && roomManager.CurrentRoom != null)
            {
                templates.Add(roomManager.CurrentRoom);
            }

            if (templates.Count == 1)
            {
                Debug.LogWarning(
                    $"{nameof(ChapterFlowAutoWireTool)} found only one RoomTemplateConfig. It will use a duplicated smoke fallback; run Tools > Tap Knockout > Content > Create Vertical Slice Chapter Content for the 10-room chain.");
            }

            return templates;
        }

        private static List<RoomTemplateConfig> ResolveDistinctChapterRooms(ChapterConfig chapterConfig)
        {
            var rooms = new List<RoomTemplateConfig>();
            if (chapterConfig == null || chapterConfig.Rooms == null)
            {
                return rooms;
            }

            for (var i = 0; i < chapterConfig.Rooms.Count; i++)
            {
                var roomTemplate = chapterConfig.Rooms[i];
                if (roomTemplate != null && !rooms.Contains(roomTemplate))
                {
                    rooms.Add(roomTemplate);
                }
            }

            return rooms;
        }

        private static void ApplyRoomDefaults(IReadOnlyList<RoomTemplateConfig> roomTemplates)
        {
            if (roomTemplates.Count == 0)
            {
                return;
            }

            SetRewardTypeIfNone(roomTemplates[0], RoomRewardType.Ability);

            if (roomTemplates.Count > 1 && roomTemplates[roomTemplates.Count - 1] != roomTemplates[0])
            {
                SetRewardTypeIfNone(roomTemplates[roomTemplates.Count - 1], RoomRewardType.BossClear);
            }
        }

        private static void SetRewardTypeIfNone(RoomTemplateConfig roomTemplate, RoomRewardType rewardType)
        {
            if (roomTemplate == null || roomTemplate.RewardType != RoomRewardType.None)
            {
                return;
            }

            var serializedObject = new SerializedObject(roomTemplate);
            serializedObject.FindProperty("rewardType").enumValueIndex = (int)rewardType;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(roomTemplate);
        }

        private static void WireChapterConfig(ChapterConfig chapterConfig, IReadOnlyList<RoomTemplateConfig> roomTemplates)
        {
            var serializedObject = new SerializedObject(chapterConfig);
            var roomsProperty = serializedObject.FindProperty("rooms");
            var usingSingleRoomFallback = roomTemplates.Count == 1;
            roomsProperty.arraySize = usingSingleRoomFallback ? 2 : roomTemplates.Count;

            for (var i = 0; i < roomsProperty.arraySize; i++)
            {
                var roomTemplate = roomTemplates[Mathf.Min(i, roomTemplates.Count - 1)];
                roomsProperty.GetArrayElementAtIndex(i).objectReferenceValue = roomTemplate;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if (usingSingleRoomFallback)
            {
                Debug.LogWarning(
                    $"{nameof(ChapterFlowAutoWireTool)} wired a duplicated single-room fallback. This is only for smoke testing and does not validate a real chapter sequence.");
            }
        }

        private static void WireChapterRunner(ChapterRunner chapterRunner, ChapterConfig chapterConfig, RoomManager roomManager)
        {
            chapterRunner.enabled = true;
            var serializedObject = new SerializedObject(chapterRunner);
            serializedObject.FindProperty("config").objectReferenceValue = chapterConfig;
            serializedObject.FindProperty("roomManager").objectReferenceValue = roomManager;
            serializedObject.FindProperty("startChapterOnStart").boolValue = true;
            serializedObject.FindProperty("autoStartFirstRoom").boolValue = true;
            serializedObject.FindProperty("autoAdvanceRooms").boolValue = false;
            serializedObject.FindProperty("logLifecycle").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(chapterRunner);
        }

        private static void WireRoomManager(RoomManager roomManager, ChapterConfig chapterConfig)
        {
            roomManager.enabled = true;
            var serializedObject = new SerializedObject(roomManager);
            var firstRoom = chapterConfig != null && chapterConfig.Rooms != null && chapterConfig.Rooms.Count > 0
                ? chapterConfig.Rooms[0]
                : null;

            serializedObject.FindProperty("config").objectReferenceValue = firstRoom;
            serializedObject.FindProperty("startConfiguredRoomOnStart").boolValue = false;
            serializedObject.FindProperty("logLifecycle").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(roomManager);

            WireInitialWaveManagerConfig(roomManager, firstRoom);
        }

        private static void WireInitialWaveManagerConfig(RoomManager roomManager, RoomTemplateConfig firstRoom)
        {
            var waveManager = roomManager != null ? roomManager.GetComponent<WaveManager>() : null;
            if (waveManager == null)
            {
                return;
            }

            waveManager.enabled = true;
            var firstWave = firstRoom != null && firstRoom.Waves != null && firstRoom.Waves.Count > 0
                ? firstRoom.Waves[0]
                : null;

            var serializedObject = new SerializedObject(waveManager);
            serializedObject.FindProperty("config").objectReferenceValue = firstWave;
            serializedObject.FindProperty("runConfiguredWaveOnStart").boolValue = false;
            serializedObject.FindProperty("logLifecycle").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(waveManager);
        }

        private static void WireFlowController(
            ChapterRoomRewardFlowController flowController,
            ChapterRunner chapterRunner,
            RoomManager roomManager,
            AbilitySelectionController abilitySelectionController,
            AbilitySelectionPanelController abilitySelectionPanel,
            PlayerHealth playerHealth)
        {
            flowController.enabled = false;
            var serializedObject = new SerializedObject(flowController);
            serializedObject.FindProperty("chapterRunner").objectReferenceValue = chapterRunner;
            serializedObject.FindProperty("roomManager").objectReferenceValue = roomManager;
            serializedObject.FindProperty("abilitySelectionController").objectReferenceValue = abilitySelectionController;
            serializedObject.FindProperty("abilitySelectionPanel").objectReferenceValue = abilitySelectionPanel;
            serializedObject.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            serializedObject.FindProperty("pauseDuringAbilitySelection").boolValue = true;
            serializedObject.FindProperty("autoContinueAfterAbilitySelection").boolValue = false;
            serializedObject.FindProperty("logDebug").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            flowController.enabled = true;
            EditorUtility.SetDirty(flowController);
        }

        private static void WireContinuePanel()
        {
            var continuePanel = UnityEngine.Object.FindFirstObjectByType<RoomContinuePanelController>(FindObjectsInactive.Include);
            if (continuePanel != null)
            {
                RoomContinuePanelBuilder.WirePanel(continuePanel);
            }
        }

        private static void DisableLegacyBridge(GameObject root)
        {
            var legacyBridge = root.GetComponent<RoomAbilityRewardBridge>();
            if (legacyBridge == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(legacyBridge);
            serializedObject.FindProperty("generateOfferWhenRoomCompletes").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            legacyBridge.enabled = false;
            EditorUtility.SetDirty(legacyBridge);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            const string projectFolder = "Assets/_Project";
            const string scriptableObjectsFolder = projectFolder + "/ScriptableObjects";
            if (!AssetDatabase.IsValidFolder(projectFolder))
            {
                AssetDatabase.CreateFolder("Assets", "_Project");
            }

            if (!AssetDatabase.IsValidFolder(scriptableObjectsFolder))
            {
                AssetDatabase.CreateFolder(projectFolder, "ScriptableObjects");
            }

            AssetDatabase.CreateFolder(scriptableObjectsFolder, "Chapters");
        }

        private static string ResolveFirstSortedAssetPath(IReadOnlyList<string> guids)
        {
            var paths = ResolveSortedAssetPaths(guids);
            return paths.Count > 0 ? paths[0] : string.Empty;
        }

        private static List<string> ResolveSortedAssetPaths(IReadOnlyList<string> guids)
        {
            var paths = new List<string>(guids.Count);
            for (var i = 0; i < guids.Count; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    paths.Add(path);
                }
            }

            paths.Sort(StringComparer.Ordinal);
            return paths;
        }
    }
}
