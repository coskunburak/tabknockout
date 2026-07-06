using TapKnockout.Level;
using TapKnockout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TapKnockout.Editor.Tools
{
    public static class RoomContinuePanelBuilder
    {
        private const string CreateMenuPath = "Tools/Tap Knockout/UI/Create Room Continue Panel";
        private const string AutoWireMenuPath = "Tools/Tap Knockout/UI/Auto Wire Room Continue Panel";

        [MenuItem(CreateMenuPath)]
        public static void CreateRoomContinuePanel()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Room Continue Panel",
                    "Stop Play Mode before creating the continue panel.",
                    "OK");
                return;
            }

            var existingPanel = Object.FindFirstObjectByType<RoomContinuePanelController>(FindObjectsInactive.Include);
            if (existingPanel != null)
            {
                WirePanel(existingPanel);
                SelectAndPing(existingPanel.gameObject);
                return;
            }

            var canvas = ResolveCanvas();
            if (canvas == null)
            {
                canvas = CreateCanvas();
            }

            EnsureEventSystem();

            var panelRoot = CreatePanelRoot(canvas.transform);
            var button = CreateContinueButton(panelRoot.transform, out var label);
            var controller = panelRoot.AddComponent<RoomContinuePanelController>();
            WirePanel(controller, panelRoot, panelRoot.GetComponent<CanvasGroup>(), button, label);
            SetPanelVisible(panelRoot.GetComponent<CanvasGroup>(), button, false);

            EditorUtility.SetDirty(panelRoot);
            EditorSceneManager.MarkSceneDirty(panelRoot.scene);
            SelectAndPing(panelRoot);
        }

        [MenuItem(AutoWireMenuPath)]
        public static void AutoWireRoomContinuePanel()
        {
            var controller = Object.FindFirstObjectByType<RoomContinuePanelController>(FindObjectsInactive.Include);
            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "Room Continue Panel",
                    "No RoomContinuePanelController was found in the open scene. Run Create Room Continue Panel first.",
                    "OK");
                return;
            }

            WirePanel(controller);
            SelectAndPing(controller.gameObject);
        }

        public static void WirePanel(RoomContinuePanelController controller)
        {
            if (controller == null)
            {
                return;
            }

            var flowController = Object.FindFirstObjectByType<ChapterRoomRewardFlowController>(FindObjectsInactive.Include);
            var button = controller.GetComponentInChildren<Button>(true);
            var label = controller.GetComponentInChildren<Text>(true);
            var canvasGroup = controller.GetComponent<CanvasGroup>();
            WirePanel(controller, controller.gameObject, canvasGroup, button, label, flowController);

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }

        private static Canvas ResolveCanvas()
        {
            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                var selectedCanvas = selected.GetComponentInParent<Canvas>();
                if (selectedCanvas != null)
                {
                    return selectedCanvas;
                }
            }

            return Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("GameplayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Gameplay Canvas");

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
                Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create Event System");
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                Undo.DestroyObjectImmediate(legacyModule);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
            }

            EditorUtility.SetDirty(eventSystem.gameObject);
        }

        private static GameObject CreatePanelRoot(Transform parent)
        {
            var panelRoot = new GameObject("RoomContinuePanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            Undo.RegisterCreatedObjectUndo(panelRoot, "Create Room Continue Panel");
            panelRoot.transform.SetParent(parent, false);

            var rectTransform = panelRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.18f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.18f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(680f, 180f);

            var image = panelRoot.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.42f);

            return panelRoot;
        }

        private static Button CreateContinueButton(Transform parent, out Text label)
        {
            var buttonObject = new GameObject("ContinueButton", typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create Continue Button");
            buttonObject.transform.SetParent(parent, false);

            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(520f, 110f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.92f, 0.95f, 0.98f, 1f);

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            label = CreateText(buttonObject.transform, "Label", "Continue", 38, TextAnchor.MiddleCenter, new Vector2(500f, 92f));
            label.color = new Color(0.08f, 0.09f, 0.11f, 1f);
            return button;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment, Vector2 size)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            Undo.RegisterCreatedObjectUndo(textObject, $"Create {name}");
            textObject.transform.SetParent(parent, false);

            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;

            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void WirePanel(
            RoomContinuePanelController controller,
            GameObject root,
            CanvasGroup canvasGroup,
            Button button,
            Text label,
            ChapterRoomRewardFlowController flowController = null)
        {
            flowController ??= Object.FindFirstObjectByType<ChapterRoomRewardFlowController>(FindObjectsInactive.Include);

            controller.enabled = false;
            if (root != null && !root.activeSelf)
            {
                Undo.RecordObject(root, "Activate Room Continue Panel");
                root.SetActive(true);
                EditorUtility.SetDirty(root);
            }

            var serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("root").objectReferenceValue = root;
            serializedObject.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedObject.FindProperty("continueButton").objectReferenceValue = button;
            serializedObject.FindProperty("continueLabel").objectReferenceValue = label;
            serializedObject.FindProperty("flowController").objectReferenceValue = flowController;
            serializedObject.FindProperty("hideOnStart").boolValue = true;
            serializedObject.FindProperty("pollFlowControllerState").boolValue = true;
            serializedObject.FindProperty("logDebug").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            controller.enabled = true;
        }

        private static void SetPanelVisible(CanvasGroup canvasGroup, Button button, bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }

            if (button != null)
            {
                button.interactable = visible;
            }
        }

        private static void SelectAndPing(GameObject gameObject)
        {
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}
