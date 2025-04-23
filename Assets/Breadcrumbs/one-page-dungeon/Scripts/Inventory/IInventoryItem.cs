using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// Interface for all inventory items
    /// </summary>
    public interface IInventoryItem
    {
        /// <summary>
        /// Gets the unique ID of the item
        /// </summary>
        string ItemId { get; }
        
        /// <summary>
        /// Gets the display name of the item
        /// </summary>
        string ItemName { get; }
        
        /// <summary>
        /// Gets the description of the item
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Gets the icon sprite of the item
        /// </summary>
        Sprite Icon { get; }
        
        /// <summary>
        /// Gets the item type
        /// </summary>
        ItemType ItemType { get; }
        
        /// <summary>
        /// Gets the rarity of the item
        /// </summary>
        ItemRarity Rarity { get; }
        
        /// <summary>
        /// Gets the equipment slot this item can be equipped to (if it's equipment)
        /// </summary>
        EquipmentSlotType? EquipSlot { get; }
        
        /// <summary>
        /// Gets the maximum number of items that can be stacked in one slot
        /// </summary>
        int MaxStackSize { get; }
        
        /// <summary>
        /// Gets the weight of a single item
        /// </summary>
        float Weight { get; }
        
        /// <summary>
        /// Gets the gold value of a single item
        /// </summary>
        int GoldValue { get; }
        
        /// <summary>
        /// Gets whether this item can be used
        /// </summary>
        bool IsUsable { get; }
        
        /// <summary>
        /// Gets whether this item is equipment
        /// </summary>
        bool IsEquipment { get; }
        
        /// <summary>
        /// Gets whether this item is auto-pickup
        /// </summary>
        bool IsAutoPickup { get; }
        
        /// <summary>
        /// Gets the required level to use this item
        /// </summary>
        int RequiredLevel { get; }
        
        /// <summary>
        /// Gets the 3D model prefab for this item
        /// </summary>
        GameObject ModelPrefab { get; }
        
        /// <summary>
        /// Gets the item stats
        /// </summary>
        IReadOnlyList<ItemStat> Stats { get; }
        
        /// <summary>
        /// Gets the color of the item's rarity
        /// </summary>
        /// <returns>The color</returns>
        Color GetRarityColor();
        
        /// <summary>
        /// Uses the item on the specified user
        /// </summary>
        /// <param name="user">The user of the item</param>
        /// <returns>True if the item was used successfully</returns>
        bool Use(GameObject user);
        
        /// <summary>
        /// Equips the item on the specified user
        /// </summary>
        /// <param name="user">The user of the item</param>
        /// <returns>True if the item was equipped successfully</returns>
        bool Equip(GameObject user);
        
        /// <summary>
        /// Unequips the item from the specified user
        /// </summary>
        /// <param name="user">The user of the item</param>
        /// <returns>True if the item was unequipped successfully</returns>
        bool Unequip(GameObject user);
    }
    
    /// <summary>
    /// Defines item types
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Material,
        Quest,
        Miscellaneous
    }
    
    /// <summary>
    /// Defines item rarities
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
    /// Represents a stat modification on an item
    /// </summary>
    public class ItemStat
    {
        /// <summary>
        /// The stat type
        /// </summary>
        public string StatType { get; set; }
        
        /// <summary>
        /// The stat value
        /// </summary>
        public float Value { get; set; }
        
        /// <summary>
        /// The modifier type
        /// </summary>
        public string ModifierType { get; set; }
    }
}
