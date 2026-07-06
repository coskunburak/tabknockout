using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI.HUD
{
    /// <summary>
    /// Controls a single buff/status icon slot in the Player Health HUD.
    /// Set icon, stack count, and visibility via the public API.
    /// The View owns an array of these and calls them through PlayerHealthHudView's
    /// SetBuffIcon / ClearBuffIcon API.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealthHudBuffSlot : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private Text stackCountLabel;

        private int slotIndex = -1;

        public int SlotIndex => slotIndex;
        public bool IsVisible { get; private set; }

        private void Reset()
        {
            iconImage  = GetComponentInChildren<Image>();
            stackCountLabel = GetComponentInChildren<Text>();
        }

        public void Initialize(int index)
        {
            slotIndex = index;
            Clear();
        }

        /// <summary>
        /// Sets this slot's icon, optional stack count, and visibility.
        /// </summary>
        public void Apply(Sprite icon, int stackCount = 0, bool visible = true)
        {
            IsVisible = visible;
            gameObject.SetActive(visible);

            if (iconImage != null)
            {
                iconImage.sprite  = icon;
                iconImage.enabled = icon != null;
                iconImage.color   = Color.white;
            }

            if (stackCountLabel != null)
            {
                var showCount = stackCount > 1;
                stackCountLabel.enabled = showCount;
                if (showCount)
                {
                    stackCountLabel.text = stackCount > 99 ? "99+" : stackCount.ToString();
                }
            }
        }

        /// <summary>Hides this slot and clears its icon.</summary>
        public void Clear()
        {
            IsVisible = false;
            gameObject.SetActive(false);

            if (iconImage != null)
            {
                iconImage.sprite  = null;
                iconImage.enabled = false;
            }

            if (stackCountLabel != null)
            {
                stackCountLabel.enabled = false;
                stackCountLabel.text    = string.Empty;
            }
        }

        /// <summary>Sets the frame sprite (called by builder/config at setup time).</summary>
        public void SetFrameSprite(Sprite frameSprite)
        {
            if (frameImage != null)
            {
                frameImage.sprite = frameSprite;
            }
        }
    }
}
