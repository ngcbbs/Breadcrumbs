using System;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// Service interface for inventory operations
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Gets the inventory data
        /// </summary>
        IInventoryData InventoryData { get; }
        
        /// <summary>
        /// Gets whether the inventory is visible
        /// </summary>
        bool IsInventoryVisible { get; }
        
        /// <summary>
        /// Shows or hides the inventory UI
        /// </summary>
        /// <param name="visible">Whether the inventory should be visible</param>
        void SetInventoryVisible(bool visible);
        
        /// <summary>
        /// Toggles the inventory visibility
        /// </summary>
        void ToggleInventory();
        
        /// <summary>
        /// Handles item pickup
        /// </summary>
        /// <param name="item">The item to pick up</param>
        /// <param name="quantity">The quantity to pick up</param>
        /// <returns>True if the item was picked up successfully</returns>
        bool PickupItem(IInventoryItem item, int quantity = 1);
        
        /// <summary>
        /// Checks if an item can be picked up
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <param name="quantity">The quantity to check</param>
        /// <returns>True if the item can be picked up</returns>
        bool CanPickupItem(IInventoryItem item, int quantity = 1);
        
        /// <summary>
        /// Tries to use an item from inventory
        /// </summary>
        /// <param name="item">The item to use</param>
        /// <returns>True if the item was used successfully</returns>
        bool UseItem(IInventoryItem item);
        
        /// <summary>
        /// Tries to use an item from a specific slot
        /// </summary>
        /// <param name="slotIndex">The slot index</param>
        /// <returns>True if the item was used successfully</returns>
        bool UseItemInSlot(int slotIndex);
        
        /// <summary>
        /// Tries to equip an item from inventory
        /// </summary>
        /// <param name="slotIndex">The slot index</param>
        /// <returns>True if the item was equipped successfully</returns>
        bool EquipItemFromSlot(int slotIndex);
        
        /// <summary>
        /// Tries to unequip an item
        /// </summary>
        /// <param name="equipSlot">The equipment slot</param>
        /// <returns>True if the item was unequipped successfully</returns>
        bool UnequipItem(EquipmentSlotType equipSlot);
        
        /// <summary>
        /// Shows the context menu for a slot
        /// </summary>
        /// <param name="slotIndex">The slot index</param>
        /// <param name="position">The screen position</param>
        void ShowContextMenu(int slotIndex, Vector2 position);
        
        /// <summary>
        /// Shows the item drop dialog
        /// </summary>
        /// <param name="slotIndex">The slot index</param>
        void ShowDropItemDialog(int slotIndex);
        
        /// <summary>
        /// Shows the item split dialog
        /// </summary>
        /// <param name="slotIndex">The slot index</param>
        void ShowSplitItemDialog(int slotIndex);
        
        /// <summary>
        /// Updates inventory UI
        /// </summary>
        void UpdateInventoryUI();
        
        /// <summary>
        /// Event triggered when an item is picked up
        /// </summary>
        event Action<IInventoryItem, int> OnItemPickedUp;
        
        /// <summary>
        /// Event triggered when an item is equipped
        /// </summary>
        event Action<IInventoryItem, EquipmentSlotType> OnItemEquipped;
        
        /// <summary>
        /// Event triggered when an item is unequipped
        /// </summary>
        event Action<IInventoryItem, EquipmentSlotType> OnItemUnequipped;
    }
}
