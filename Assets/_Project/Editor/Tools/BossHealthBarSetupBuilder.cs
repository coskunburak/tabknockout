using System.IO;
using TapKnockout.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.Editor.Tools
{
    public static class BossHealthBarSetupBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/UI/Create Boss Health Bar Placeholder";
        private const string PrefabFolder = "Assets/_Project/Prefabs/UI";
        private const string PrefabPath = "Assets/_Project/Prefabs/UI/PF_BossHealthBar_Playtest.prefab";

        [MenuItem(MenuPath)]
        public static void CreateBossHealthBarPlaceholder()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Boss Health Bar", "Exit Play Mode before creating UI prefabs.", "OK");
                return;
            }

            CreateOrUpdateBossHealthBarPlaceholder();
            EditorUtility.DisplayDialog("Boss Health Bar", $"Boss health bar placeholder ready:\n\n{PrefabPath}", "OK");
        }

        public static GameObject CreateOrUpdateBossHealthBarPlaceholder()
        {
            EnsureFolder(PrefabFolder);
            var loadedExisting = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
            var root = loadedExisting
                ? PrefabUtility.LoadPrefabContents(PrefabPath)
                : CreateBossHealthBarRoot();

            try
            {
                RepairBossHealthBar(root);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"{nameof(BossHealthBarSetupBuilder)} Done: {PrefabPath}", root);
                return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }
            finally
            {
                if (loadedExisting)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static GameObject CreateBossHealthBarRoot()
        {
            var root = new GameObject("PF_BossHealthBar_Playtest", typeof(RectTransform), typeof(CanvasGroup), typeof(BossHealthBarController));
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.08f, 0.88f);
            rect.anchorMax = new Vector2(0.92f, 0.96f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return root;
        }

        private static void RepairBossHealthBar(GameObject root)
        {
            var canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
            var controller = root.GetComponent<BossHealthBarController>() ?? root.AddComponent<BossHealthBarController>();
            var background = EnsureImage(root.transform, "Background", new Color(0.08f, 0.08f, 0.08f, 0.82f));
            var sliderObject = EnsureChild(root.transform, "HealthSlider");
            var slider = sliderObject.GetComponent<Slider>() ?? sliderObject.gameObject.AddComponent<Slider>();
            var fill = EnsureImage(sliderObject, "Fill", new Color(0.85f, 0.18f, 0.12f, 0.95f));
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0.1f);
            fillRect.anchorMax = new Vector2(1f, 0.55f);
            fillRect.offsetMin = new Vector2(12f, 0f);
            fillRect.offsetMax = new Vector2(-12f, 0f);
            slider.fillRect = fillRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.interactable = false;

            var bossName = EnsureText(root.transform, "BossName", "Dash-Counter Brute", 22, TextAnchor.UpperLeft);
            var phase = EnsureText(root.transform, "Phase", "Phase 1", 16, TextAnchor.UpperRight);
            var serializedObject = new SerializedObject(controller);
            SetObject(serializedObject, "healthSlider", slider);
            SetObject(serializedObject, "bossNameLabel", bossName);
            SetObject(serializedObject, "phaseLabel", phase);
            SetObject(serializedObject, "canvasGroup", canvasGroup);
            SetObject(serializedObject, "root", root);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            var backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
        }

        private static Image EnsureImage(Transform parent, string name, Color color)
        {
            var child = EnsureChild(parent, name);
            var image = child.GetComponent<Image>() ?? child.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text EnsureText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            var child = EnsureChild(parent, name);
            var label = child.GetComponent<Text>() ?? child.gameObject.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rect = child.GetComponent<RectTransform>();
            rect.anchorMin = name == "BossName" ? new Vector2(0.03f, 0.55f) : new Vector2(0.7f, 0.55f);
            rect.anchorMax = name == "BossName" ? new Vector2(0.7f, 1f) : new Vector2(0.97f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return label;
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", Path.GetFileName(folderPath));
        }
    }
}
