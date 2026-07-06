using TapKnockout.Room;
using TapKnockout.UI;
using TapKnockout.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TapKnockout.Editor.Tools
{
    public static class Phase4SceneRuntimeWiringTool
    {
        private const string MenuPath = "Tools/Tap Knockout/Scene/Wire Phase 4 Runtime References";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string GameplayRootName = "GameplayRoot";
        private const string GameSystemsName = "GameSystems";
        private const string RoomInstanceRootName = "RoomInstanceRoot";
        private const string GameplayCanvasName = "GameplayCanvas";
        private const string BossHealthBarName = "BossHealthBar_Playtest";
        private const string BossHealthBarPrefabPath = "Assets/_Project/Prefabs/UI/PF_BossHealthBar_Playtest.prefab";

        [MenuItem(MenuPath)]
        public static void WirePhase4RuntimeReferencesFromMenu()
        {
            if (Application.isPlaying)
            {
                ShowDialog("Phase 4 Runtime Wiring", "Exit Play Mode before wiring scene references.", "OK");
                return;
            }

            WireSampleSceneRuntimeReferences();
            ShowDialog("Phase 4 Runtime Wiring", "SampleScene Phase 4 runtime references are wired. Check Console for details.", "OK");
        }

        public static void WireSampleSceneRuntimeReferences()
        {
            var scene = OpenSampleScene();
            var roomInstanceRoot = EnsureRoomInstanceRoot();
            var runtimeRoomLoader = EnsureRuntimeRoomLoader();
            var vfxService = EnsureVFXService();
            var bossHealthBar = EnsureBossHealthBarUnderCanvas();

            SetObjectReference(runtimeRoomLoader, "roomInstanceRoot", roomInstanceRoot);
            WireVFXCatalog(vfxService);

            EditorUtility.SetDirty(runtimeRoomLoader);
            EditorUtility.SetDirty(vfxService);
            EditorUtility.SetDirty(bossHealthBar);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"{nameof(Phase4SceneRuntimeWiringTool)} Done: RuntimeRoomLoader.roomInstanceRoot -> {GetHierarchyPath(roomInstanceRoot)}");
            Debug.Log($"{nameof(Phase4SceneRuntimeWiringTool)} Done: VFXService.catalog -> {VFXAssetPackCatalogMapper.CatalogPath}");
            Debug.Log($"{nameof(Phase4SceneRuntimeWiringTool)} Done: BossHealthBarController -> {GetHierarchyPath(bossHealthBar.transform)}");
        }

        public static void ValidateSampleSceneRuntimeReferences()
        {
            OpenSampleScene();

            var runtimeRoomLoader = FindAny<RuntimeRoomLoader>();
            var vfxService = FindAny<VFXService>();
            var bossHealthBar = FindAny<BossHealthBarController>();
            var canvas = bossHealthBar != null ? bossHealthBar.GetComponentInParent<Canvas>(true) : null;

            var loaderObject = runtimeRoomLoader != null ? new SerializedObject(runtimeRoomLoader) : null;
            var roomInstanceRoot = loaderObject?.FindProperty("roomInstanceRoot")?.objectReferenceValue as Transform;
            var vfxObject = vfxService != null ? new SerializedObject(vfxService) : null;
            var catalog = vfxObject?.FindProperty("catalog")?.objectReferenceValue as VFXCatalog;

            LogCheck("RuntimeRoomLoader", runtimeRoomLoader != null, "Missing RuntimeRoomLoader in SampleScene.");
            LogCheck("RuntimeRoomLoader.roomInstanceRoot", roomInstanceRoot != null, "RuntimeRoomLoader.roomInstanceRoot is not assigned.");
            LogCheck("VFXService", vfxService != null, "Missing VFXService in SampleScene.");
            LogCheck("VFXService.catalog", catalog != null, "VFXService.catalog is not assigned.");
            LogCheck("BossHealthBarController", bossHealthBar != null, "Missing BossHealthBarController in SampleScene.");
            LogCheck("BossHealthBarController canvas parent", canvas != null, "BossHealthBarController is not under a Canvas.");
        }

        private static Scene OpenSampleScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.path == ScenePath)
            {
                return activeScene;
            }

            return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static Transform EnsureRoomInstanceRoot()
        {
            var gameplayRoot = EnsureRoot(GameplayRootName);
            return EnsureChild(gameplayRoot.transform, RoomInstanceRootName);
        }

        private static RuntimeRoomLoader EnsureRuntimeRoomLoader()
        {
            var existing = FindAny<RuntimeRoomLoader>();
            if (existing != null)
            {
                return existing;
            }

            var gameplayRoot = EnsureRoot(GameplayRootName);
            var systemsRoot = EnsureChild(gameplayRoot.transform, GameSystemsName);
            return systemsRoot.gameObject.AddComponent<RuntimeRoomLoader>();
        }

        private static VFXService EnsureVFXService()
        {
            var existing = FindAny<VFXService>();
            if (existing != null)
            {
                return existing;
            }

            var root = EnsureRoot("VFXFeedbackRoot");
            return root.AddComponent<VFXService>();
        }

        private static void WireVFXCatalog(VFXService vfxService)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<VFXCatalog>(VFXAssetPackCatalogMapper.CatalogPath);
            if (catalog == null)
            {
                VFXAssetPackCatalogMapper.CreateVerticalSliceVfxCatalog();
                catalog = AssetDatabase.LoadAssetAtPath<VFXCatalog>(VFXAssetPackCatalogMapper.CatalogPath);
            }

            SetObjectReference(vfxService, "catalog", catalog);
        }

        private static BossHealthBarController EnsureBossHealthBarUnderCanvas()
        {
            var canvas = EnsureGameplayCanvas();
            var existing = FindAny<BossHealthBarController>();
            if (existing != null)
            {
                if (existing.GetComponentInParent<Canvas>(true) == null)
                {
                    existing.transform.SetParent(canvas.transform, false);
                }

                existing.gameObject.name = BossHealthBarName;
                return existing;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossHealthBarPrefabPath);
            if (prefab == null)
            {
                prefab = BossHealthBarSetupBuilder.CreateOrUpdateBossHealthBarPlaceholder();
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, canvas.transform) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, canvas.transform);
            }

            instance.name = BossHealthBarName;
            ResetRectTransform(instance.GetComponent<RectTransform>());
            var controller = instance.GetComponentInChildren<BossHealthBarController>(true);
            if (controller == null)
            {
                controller = instance.AddComponent<BossHealthBarController>();
            }

            return controller;
        }

        private static Canvas EnsureGameplayCanvas()
        {
            var namedCanvas = FindRoot(GameplayCanvasName)?.GetComponent<Canvas>();
            if (namedCanvas != null)
            {
                return namedCanvas;
            }

            var existing = FindAny<Canvas>();
            if (existing != null)
            {
                return existing;
            }

            var canvasObject = new GameObject(GameplayCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(828f, 1792f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static GameObject EnsureRoot(string rootName)
        {
            var existing = FindRoot(rootName);
            return existing != null ? existing : new GameObject(rootName);
        }

        private static GameObject FindRoot(string rootName)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == rootName)
                {
                    return roots[i];
                }
            }

            return null;
        }

        private static T FindAny<T>() where T : Object
        {
            return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            var existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(childName);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ResetRectTransform(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void LogCheck(string label, bool isValid, string failureMessage)
        {
            if (isValid)
            {
                Debug.Log($"{nameof(Phase4SceneRuntimeWiringTool)} Validate Done: {label}");
                return;
            }

            Debug.LogError($"{nameof(Phase4SceneRuntimeWiringTool)} Validate Error: {failureMessage}");
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

        private static void ShowDialog(string title, string message, string ok)
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(title, message, ok);
            }
        }
    }
}
