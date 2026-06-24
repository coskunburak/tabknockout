using System;
using TapKnockout.Ability;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI
{
    [DisallowMultipleComponent]
    public sealed class AbilityChoiceCardView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Text stackText;

        [Header("Colors")]
        [SerializeField] private Color commonColor = new Color(0.84f, 0.88f, 0.92f, 1f);
        [SerializeField] private Color uncommonColor = new Color(0.47f, 0.86f, 0.50f, 1f);
        [SerializeField] private Color rareColor = new Color(0.36f, 0.62f, 1f, 1f);
        [SerializeField] private Color epicColor = new Color(0.66f, 0.42f, 1f, 1f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.72f, 0.25f, 1f);

        private AbilityDefinition boundAbility;
        private int boundIndex = -1;
        private Action<int> onSelected;

        private void Awake()
        {
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(HandleSelected);
            }
        }

        private void OnDisable()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(HandleSelected);
            }
        }

        public void Bind(AbilityDefinition ability, int index, int currentStackCount, Action<int> selectedCallback)
        {
            boundAbility = ability;
            boundIndex = index;
            onSelected = selectedCallback;

            if (ability == null)
            {
                Clear();
                return;
            }

            gameObject.SetActive(true);
            SetText(titleText, ability.DisplayName);
            SetText(descriptionText, ability.Description);
            SetText(rarityText, ability.Rarity.ToString());
            var stackAfterSelection = Mathf.Min(currentStackCount + 1, ability.MaxStacks);
            SetText(stackText, $"Stack {stackAfterSelection}/{ability.MaxStacks}");

            if (icon != null)
            {
                icon.sprite = ability.Icon;
                icon.enabled = ability.Icon != null;
            }

            if (background != null)
            {
                background.color = GetRarityColor(ability.Rarity);
            }
        }

        public void Clear()
        {
            boundAbility = null;
            boundIndex = -1;
            onSelected = null;
            SetText(titleText, string.Empty);
            SetText(descriptionText, string.Empty);
            SetText(rarityText, string.Empty);
            SetText(stackText, string.Empty);

            if (icon != null)
            {
                icon.sprite = null;
                icon.enabled = false;
            }

            gameObject.SetActive(false);
        }

        private void HandleSelected()
        {
            if (boundAbility == null || boundIndex < 0)
            {
                return;
            }

            onSelected?.Invoke(boundIndex);
        }

        private Color GetRarityColor(AbilityRarity rarity)
        {
            return rarity switch
            {
                AbilityRarity.Uncommon => uncommonColor,
                AbilityRarity.Rare => rareColor,
                AbilityRarity.Epic => epicColor,
                AbilityRarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
