using System.Collections.Generic;
using TapKnockout.Ability;
using TapKnockout.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TapKnockout.Editor.Tools
{
    public static class AbilitySelectionPanelBuilder
    {
        private const string MenuPath = "Tools/Tap Knockout/UI/Create Ability Selection Panel";
        private const string FixEventSystemMenuPath = "Tools/Tap Knockout/UI/Fix Event System Input Module";
        private const string WireEffectApplierMenuPath = "Tools/Tap Knockout/Ability/Wire Player Ability Effect Applier";

        [MenuItem(MenuPath)]
        public static void CreateAbilitySelectionPanel()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Ability Selection Panel",
                    "Stop Play Mode before creating the panel so the hierarchy is saved in the scene.",
                    "OK");
                return;
            }

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                canvas = CreateCanvas();
            }

            EnsureEventSystem();

            var panelRoot = CreatePanelRoot(canvas.transform);
            var cardViews = CreateCardRow(panelRoot.transform);
            var panelController = panelRoot.AddComponent<AbilitySelectionPanelController>();

            var selectionController = Object.FindFirstObjectByType<AbilitySelectionController>();
            WirePanel(panelController, selectionController, panelRoot.GetComponent<CanvasGroup>(), cardViews);

            Selection.activeGameObject = panelRoot;
            EditorGUIUtility.PingObject(panelRoot);
            EditorUtility.SetDirty(panelRoot);
        }

        [MenuItem(FixEventSystemMenuPath)]
        public static void FixEventSystemInputModule()
        {
            EnsureEventSystem();
        }

        [MenuItem(WireEffectApplierMenuPath)]
        public static void WirePlayerAbilityEffectApplier()
        {
            var selectionController = Object.FindFirstObjectByType<AbilitySelectionController>(FindObjectsInactive.Include);
            if (selectionController == null)
            {
                EditorUtility.DisplayDialog(
                    "Ability Effect Applier",
                    "No AbilitySelectionController was found in the open scene.",
                    "OK");
                return;
            }

            var effectApplier = FindAbilityEffectApplier();
            if (effectApplier == null)
            {
                EditorUtility.DisplayDialog(
                    "Ability Effect Applier",
                    "No scene component implementing IAbilityEffectApplier was found. Add PlayerAbilityEffectApplier to the Player first.",
                    "OK");
                return;
            }

            var serializedObject = new SerializedObject(selectionController);
            serializedObject.FindProperty("abilityEffectApplier").objectReferenceValue = effectApplier;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(selectionController);
            Selection.activeGameObject = selectionController.gameObject;
            EditorGUIUtility.PingObject(selectionController);
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

        private static MonoBehaviour FindAbilityEffectApplier()
        {
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IAbilityEffectApplier)
                {
                    return behaviours[i];
                }
            }

            return null;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
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
            var panelRoot = new GameObject("AbilitySelectionPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            Undo.RegisterCreatedObjectUndo(panelRoot, "Create Ability Selection Panel");
            panelRoot.transform.SetParent(parent, false);

            var rectTransform = panelRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var image = panelRoot.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.62f);

            CreateText(panelRoot.transform, "Title", "Choose an Ability", 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.83f), new Vector2(820f, 96f));
            return panelRoot;
        }

        private static List<AbilityChoiceCardView> CreateCardRow(Transform parent)
        {
            var row = new GameObject("CardRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = Vector2.zero;
            rowRect.sizeDelta = new Vector2(960f, 620f);

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 28f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var cardViews = new List<AbilityChoiceCardView>(3);
            for (var i = 0; i < 3; i++)
            {
                cardViews.Add(CreateCard(row.transform, i));
            }

            return cardViews;
        }

        private static AbilityChoiceCardView CreateCard(Transform parent, int index)
        {
            var card = new GameObject($"AbilityCard_{index + 1}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(VerticalLayoutGroup), typeof(AbilityChoiceCardView));
            card.transform.SetParent(parent, false);

            var rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(290f, 560f);

            var layoutElement = card.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = 290f;
            layoutElement.preferredHeight = 560f;

            var cardLayout = card.GetComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(24, 24, 24, 24);
            cardLayout.spacing = 16f;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            var background = card.GetComponent<Image>();
            background.color = new Color(0.84f, 0.88f, 0.92f, 1f);

            var icon = CreateIcon(card.transform);
            var title = CreateText(card.transform, "Title", "Ability", 32, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(240f, 74f));
            var rarity = CreateText(card.transform, "Rarity", "Common", 22, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(240f, 40f));
            var description = CreateText(card.transform, "Description", "Description", 22, TextAnchor.UpperCenter, Vector2.zero, new Vector2(240f, 180f));
            var stack = CreateText(card.transform, "Stack", "Stack 0/1", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(240f, 42f));
            var cardTextColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            title.color = cardTextColor;
            rarity.color = cardTextColor;
            description.color = cardTextColor;
            stack.color = cardTextColor;

            var button = card.GetComponent<Button>();
            button.targetGraphic = background;

            WireCard(card.GetComponent<AbilityChoiceCardView>(), button, background, icon, title, description, rarity, stack);
            return card.GetComponent<AbilityChoiceCardView>();
        }

        private static Image CreateIcon(Transform parent)
        {
            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(parent, false);

            var layoutElement = iconObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = 120f;
            layoutElement.preferredHeight = 120f;

            var icon = iconObject.GetComponent<Image>();
            icon.color = new Color(1f, 1f, 1f, 0.18f);
            icon.preserveAspect = true;
            icon.enabled = false;
            return icon;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 size)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            if (anchor != Vector2.zero)
            {
                rectTransform.anchorMin = anchor;
                rectTransform.anchorMax = anchor;
                rectTransform.anchoredPosition = Vector2.zero;
            }

            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void WirePanel(
            AbilitySelectionPanelController panel,
            AbilitySelectionController selectionController,
            CanvasGroup canvasGroup,
            IReadOnlyList<AbilityChoiceCardView> cardViews)
        {
            var serializedObject = new SerializedObject(panel);
            serializedObject.FindProperty("abilitySelectionController").objectReferenceValue = selectionController;
            serializedObject.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;

            var cardViewsProperty = serializedObject.FindProperty("cardViews");
            cardViewsProperty.arraySize = cardViews.Count;
            for (var i = 0; i < cardViews.Count; i++)
            {
                cardViewsProperty.GetArrayElementAtIndex(i).objectReferenceValue = cardViews[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireCard(
            AbilityChoiceCardView cardView,
            Button button,
            Image background,
            Image icon,
            Text title,
            Text description,
            Text rarity,
            Text stack)
        {
            var serializedObject = new SerializedObject(cardView);
            serializedObject.FindProperty("selectButton").objectReferenceValue = button;
            serializedObject.FindProperty("background").objectReferenceValue = background;
            serializedObject.FindProperty("icon").objectReferenceValue = icon;
            serializedObject.FindProperty("titleText").objectReferenceValue = title;
            serializedObject.FindProperty("descriptionText").objectReferenceValue = description;
            serializedObject.FindProperty("rarityText").objectReferenceValue = rarity;
            serializedObject.FindProperty("stackText").objectReferenceValue = stack;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
