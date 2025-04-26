#if INCOMPLETE

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.UI.Components.Trade {
    /// <summary>
    /// UI component for a single trade slot
    /// </summary>
    public class TradeSlotUI : MonoBehaviour {
        [Header("UI Elements")]
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private TMP_Text itemNameText;
        [SerializeField]
        private TMP_Text itemCountText;
        [SerializeField]
        private Button removeButton;
        [SerializeField]
        private GameObject infoButton;

        [Header("Visual Settings")]
        [SerializeField]
        private Color commonColor = Color.white;
        [SerializeField]
        private Color uncommonColor = Color.green;
        [SerializeField]
        private Color rareColor = Color.blue;
        [SerializeField]
        private Color epicColor = new Color(0.5f, 0, 0.5f); // Purple
        [SerializeField]
        private Color legendaryColor = new Color(1f, 0.5f, 0); // Orange

        // Item reference
        private Item currentItem;
        private int itemCount;
        private bool isPartnerItem;

        // Callback for remove button
        private Action onRemoveCallback;

        // Item details UI reference
        private ItemDetailsPanel itemDetailsPanel;

        private void Awake() {
            // Find the item details panel
            itemDetailsPanel = FindObjectOfType<ItemDetailsPanel>();

            // Set up info button
            if (infoButton != null) {
                Button infoButtonComponent = infoButton.GetComponent<Button>();
                if (infoButtonComponent != null) {
                    infoButtonComponent.onClick.AddListener(ShowItemDetails);
                }
            }

            // Set up remove button
            if (removeButton != null) {
                removeButton.onClick.AddListener(() => { onRemoveCallback?.Invoke(); });
            }
        }

        /// <summary>
        /// Initialize the trade slot
        /// </summary>
        public void Initialize(Item item, int count, bool isPartnerOffer) {
            currentItem = item;
            itemCount = count;
            isPartnerItem = isPartnerOffer;

            // Set icon
            if (itemIconImage != null && item.icon != null) {
                itemIconImage.sprite = item.icon;
                itemIconImage.enabled = true;
            } else if (itemIconImage != null) {
                itemIconImage.enabled = false;
            }

            // Set name with rarity color
            if (itemNameText != null) {
                Color rarityColor = GetRarityColor(item.rarity);
                string coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{item.itemName}</color>";
                itemNameText.text = coloredName;
            }

            // Set count
            if (itemCountText != null) {
                itemCountText.text = count > 1 ? count.ToString() : string.Empty;
            }

            // Configure remove button
            if (removeButton != null) {
                removeButton.gameObject.SetActive(!isPartnerOffer); // Only player can remove their own items
            }
        }

        /// <summary>
        /// Set the item count
        /// </summary>
        public void SetItemCount(int count) {
            itemCount = count;

            if (itemCountText != null) {
                itemCountText.text = count > 1 ? count.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Set the callback for when the remove button is clicked
        /// </summary>
        public void SetOnRemoveCallback(Action callback) {
            onRemoveCallback = callback;
        }

        /// <summary>
        /// Get the current item ID
        /// </summary>
        public string GetItemID() {
            return currentItem != null ? currentItem.itemID : string.Empty;
        }

        /// <summary>
        /// Get the current item
        /// </summary>
        public Item GetItem() {
            return currentItem;
        }

        /// <summary>
        /// Get the current item count
        /// </summary>
        public int GetItemCount() {
            return itemCount;
        }

        /// <summary>
        /// Disable the remove button
        /// </summary>
        public void DisableRemoveButton() {
            if (removeButton != null) {
                removeButton.interactable = false;
            }
        }

        /// <summary>
        /// Show item details
        /// </summary>
        private void ShowItemDetails() {
            if (itemDetailsPanel != null && currentItem != null) {
                itemDetailsPanel.ShowItemDetails(currentItem, isPartnerItem);
            }
        }

        /// <summary>
        /// Get color for an item rarity
        /// </summary>
        private Color GetRarityColor(ItemRarity rarity) {
            switch (rarity) {
                case ItemRarity.Common:
                    return commonColor;
                case ItemRarity.Uncommon:
                    return uncommonColor;
                case ItemRarity.Rare:
                    return rareColor;
                case ItemRarity.Epic:
                    return epicColor;
                case ItemRarity.Legendary:
                    return legendaryColor;
                default:
                    return commonColor;
            }
        }
    }
}
#endif