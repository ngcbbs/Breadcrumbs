using UnityEngine;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Rarity levels for items
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    /// <summary>
    /// Types of items in the game
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Quest,
        Material,
        Treasure
    }
    
    /// <summary>
    /// Base class for all items in the game
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemID;
        public string itemName;
        [TextArea(3, 6)]
        public string description;
        public ItemType itemType;
        public ItemRarity rarity = ItemRarity.Common;
        public Sprite icon;
        public GameObject prefab;
        
        [Header("Properties")]
        public int maxStackSize = 1;
        public int buyPrice;
        public int sellPrice;
        public float weight = 1f;
        public bool isQuestItem = false;
        
        [Header("Effects")]
        public AudioClip useSound;
        public GameObject useEffect;
        
        /// <summary>
        /// Called when the item is used
        /// </summary>
        public virtual bool Use(GameObject user)
        {
            // Base implementation - override in derived classes
            Debug.Log($"{user.name} used {itemName}");
            
            // Play use sound
            if (useSound != null && user.TryGetComponent<AudioSource>(out var audioSource))
            {
                audioSource.PlayOneShot(useSound);
            }
            
            // Spawn use effect
            if (useEffect != null)
            {
                Instantiate(useEffect, user.transform.position, user.transform.rotation);
            }
            
            return true;
        }
        
        /// <summary>
        /// Get a display color based on rarity
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return Color.white;
                case ItemRarity.Uncommon:
                    return new Color(0.3f, 0.8f, 0.3f); // Green
                case ItemRarity.Rare:
                    return new Color(0.3f, 0.5f, 1f); // Blue
                case ItemRarity.Epic:
                    return new Color(0.8f, 0.3f, 0.9f); // Purple
                case ItemRarity.Legendary:
                    return new Color(1f, 0.6f, 0.1f); // Orange
                default:
                    return Color.white;
            }
        }
        
        /// <summary>
        /// Get a formatted name with rarity color
        /// </summary>
        public string GetColoredName()
        {
            Color color = GetRarityColor();
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>{itemName}</color>";
        }
        
        /// <summary>
        /// Check if this item can be stacked with another item
        /// </summary>
        public virtual bool CanStackWith(Item otherItem)
        {
            return otherItem != null && 
                   otherItem.itemID == itemID && 
                   maxStackSize > 1;
        }
        
        /// <summary>
        /// Get the tooltip text for this item
        /// </summary>
        public virtual string GetTooltipText()
        {
            string rarityText = rarity.ToString();
            string typeText = itemType.ToString();
            
            string tooltipText = $"{GetColoredName()}\n" +
                                 $"<size=12>{rarityText} {typeText}</size>\n\n" +
                                 $"{description}\n\n";
            
            if (weight > 0)
            {
                tooltipText += $"Weight: {weight} kg\n";
            }
            
            if (buyPrice > 0)
            {
                tooltipText += $"Value: {buyPrice} gold\n";
            }
            
            return tooltipText;
        }
        
        /// <summary>
        /// Create a copy of this item
        /// </summary>
        public virtual Item CreateInstance()
        {
            return Instantiate(this);
        }
    }
}
