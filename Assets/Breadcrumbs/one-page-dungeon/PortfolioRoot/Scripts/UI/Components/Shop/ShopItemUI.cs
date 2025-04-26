#if INCOMPLETE
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.UI.Components
{
    /// <summary>
    /// UI component for a shop item
    /// </summary>
    public class ShopItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Item Components")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private Text itemNameText;
        [SerializeField] private Text itemPriceText;
        [SerializeField] private Text itemQuantityText;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private Image selectedBorder;
        [SerializeField] private GameObject soldOutOverlay;
        
        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = Color.magenta;
        [SerializeField] private Color legendaryColor = Color.yellow;
        
        private ShopItemData itemData;
        private int currentPrice;
        private int currentQuantity;
        private bool isSelected = false;
        
        // Event fired when this item is selected
        public event Action OnItemSelected;
        
        /// <summary>
        /// Initialize the shop item UI
        /// </summary>
        public void Initialize(ShopItemData data, int price, int quantity)
        {
            itemData = data;
            currentPrice = price;
            currentQuantity = quantity;
            
            // Set item icon
            if (itemIcon != null && data.Icon != null)
            {
                itemIcon.sprite = data.Icon;
                itemIcon.enabled = true;
            }
            else if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }
            
            // Set item name with rarity prefix
            if (itemNameText != null)
            {
                itemNameText.text = data.ItemName;
            }
            
            // Set item price
            if (itemPriceText != null)
            {
                itemPriceText.text = $"{price} Gold";
            }
            
            // Set item quantity
            if (itemQuantityText != null)
            {
                itemQuantityText.text = quantity > 0 ? $"x{quantity}" : "Sold Out";
            }
            
            // Set rarity border color
            if (rarityBorder != null)
            {
                Color rarityColor = GetRarityColor(data.Rarity);
                rarityBorder.color = rarityColor;
            }
            
            // Show sold out overlay if quantity is 0
            if (soldOutOverlay != null)
            {
                soldOutOverlay.SetActive(quantity <= 0);
            }
            
            // Hide selection initially
            SetSelected(false);
        }
        
        /// <summary>
        /// Get color based on item rarity
        /// </summary>
        private Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
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
        
        /// <summary>
        /// Set selected state visual
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (selectedBorder != null)
            {
                selectedBorder.enabled = selected;
            }
        }
        
        /// <summary>
        /// Update item price
        /// </summary>
        public void UpdatePrice(int newPrice)
        {
            currentPrice = newPrice;
            
            if (itemPriceText != null)
            {
                itemPriceText.text = $"{newPrice} Gold";
            }
        }
        
        /// <summary>
        /// Update item quantity
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            currentQuantity = newQuantity;
            
            if (itemQuantityText != null)
            {
                itemQuantityText.text = newQuantity > 0 ? $"x{newQuantity}" : "Sold Out";
            }
            
            // Show/hide sold out overlay
            if (soldOutOverlay != null)
            {
                soldOutOverlay.SetActive(newQuantity <= 0);
            }
        }
        
        /// <summary>
        /// Handle item click
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // Invoke selection event
            OnItemSelected?.Invoke();
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }
        
        /// <summary>
        /// Handle pointer enter
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Highlight on hover
            transform.localScale = new Vector3(1.05f, 1.05f, 1f);
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Hover");
            }
        }
        
        /// <summary>
        /// Handle pointer exit
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // Reset scale
            transform.localScale = Vector3.one;
        }
    }
}
#endif