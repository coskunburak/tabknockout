using System;
using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.Level;
using TapKnockout.Player;
using TapKnockout.Room;
using TapKnockout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TapKnockout.Editor.Tools
{
    public static class ChapterFlowAutoWireTool
    {
        private const string MenuPath = "Tools/Tap Knockout/Chapter/Wire Chapter Room Flow";
        private const string ChapterFolderPath = "Assets/_Project/ScriptableObjects/Chapters";
        private const string RoomFolderPath = "Assets/_Project/ScriptableObjects/Rooms";
        private const string DefaultChapterPath = ChapterFolderPath + "/Chapter_Test_01.asset";

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
            var roomTemplates = ResolveRoomTemplates(roomManager);
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
            WireRoomManager(roomManager);
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
            var chapterGuids = AssetDatabase.FindAssets("t:ChapterConfig", new[] { ChapterFolderPath });
            if (chapterGuids.Length > 0)
            {
                Array.Sort(chapterGuids, StringComparer.Ordinal);
                var existingPath = AssetDatabase.GUIDToAssetPath(chapterGuids[0]);
                return AssetDatabase.LoadAssetAtPath<ChapterConfig>(existingPath);
            }

            EnsureFolder(ChapterFolderPath);
            var chapter = ScriptableObject.CreateInstance<ChapterConfig>();
            AssetDatabase.CreateAsset(chapter, AssetDatabase.GenerateUniqueAssetPath(DefaultChapterPath));

            var serializedObject = new SerializedObject(chapter);
            serializedObject.FindProperty("chapterId").stringValue = "chapter_test_01";
            serializedObject.FindProperty("displayName").stringValue = "Test Chapter 1";
            serializedObject.FindProperty("chapterIndex").intValue = 1;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return chapter;
        }

        private static List<RoomTemplateConfig> ResolveRoomTemplates(RoomManager roomManager)
        {
            var templates = new List<RoomTemplateConfig>();
            var roomGuids = AssetDatabase.FindAssets("t:RoomTemplateConfig", new[] { RoomFolderPath });
            Array.Sort(roomGuids, StringComparer.Ordinal);

            for (var i = 0; i < roomGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(roomGuids[i]);
                var roomTemplate = AssetDatabase.LoadAssetAtPath<RoomTemplateConfig>(path);
                if (roomTemplate != null && !templates.Contains(roomTemplate))
                {
                    templates.Add(roomTemplate);
                }
            }

            if (templates.Count == 0 && roomManager.CurrentRoom != null)
            {
                templates.Add(roomManager.CurrentRoom);
            }

            return templates;
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
            roomsProperty.arraySize = roomTemplates.Count == 1 ? 2 : roomTemplates.Count;

            for (var i = 0; i < roomsProperty.arraySize; i++)
            {
                var roomTemplate = roomTemplates[Mathf.Min(i, roomTemplates.Count - 1)];
                roomsProperty.GetArrayElementAtIndex(i).objectReferenceValue = roomTemplate;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireChapterRunner(ChapterRunner chapterRunner, ChapterConfig chapterConfig, RoomManager roomManager)
        {
            var serializedObject = new SerializedObject(chapterRunner);
            serializedObject.FindProperty("config").objectReferenceValue = chapterConfig;
            serializedObject.FindProperty("roomManager").objectReferenceValue = roomManager;
            serializedObject.FindProperty("startChapterOnStart").boolValue = true;
            serializedObject.FindProperty("autoStartFirstRoom").boolValue = true;
            serializedObject.FindProperty("autoAdvanceRooms").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(chapterRunner);
        }

        private static void WireRoomManager(RoomManager roomManager)
        {
            var serializedObject = new SerializedObject(roomManager);
            serializedObject.FindProperty("startConfiguredRoomOnStart").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(roomManager);
        }

        private static void WireFlowController(
            ChapterRoomRewardFlowController flowController,
            ChapterRunner chapterRunner,
            RoomManager roomManager,
            AbilitySelectionController abilitySelectionController,
            AbilitySelectionPanelController abilitySelectionPanel,
            PlayerHealth playerHealth)
        {
            var serializedObject = new SerializedObject(flowController);
            serializedObject.FindProperty("chapterRunner").objectReferenceValue = chapterRunner;
            serializedObject.FindProperty("roomManager").objectReferenceValue = roomManager;
            serializedObject.FindProperty("abilitySelectionController").objectReferenceValue = abilitySelectionController;
            serializedObject.FindProperty("abilitySelectionPanel").objectReferenceValue = abilitySelectionPanel;
            serializedObject.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            serializedObject.FindProperty("pauseDuringAbilitySelection").boolValue = true;
            serializedObject.FindProperty("autoContinueAfterAbilitySelection").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
    }
}
