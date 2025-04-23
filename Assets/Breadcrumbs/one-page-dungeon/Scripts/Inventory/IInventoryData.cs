using System;
using System.Collections.Generic;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// Interface for inventory data container
    /// </summary>
    public interface IInventoryData
    {
        /// <summary>
        /// Gets the maximum capacity of the inventory
        /// </summary>
        int Capacity { get; }
        
        /// <summary>
        /// Gets the current weight of the inventory
        /// </summary>
        float CurrentWeight { get; }
        
        /// <summary>
        /// Gets the maximum weight the inventory can carry
        /// </summary>
        float MaxWeight { get; set; }
        
        /// <summary>
        /// Gets the current gold amount
        /// </summary>
        int Gold { get; }
        
        /// <summary>
        /// Gets all inventory slots
        /// </summary>
        IReadOnlyList<InventorySlot> Slots { get; }
        
        /// <summary>
        /// Gets all equipment slots
        /// </summary>
        IReadOnlyDictionary<EquipmentSlotType, InventorySlot> EquipmentSlots { get; }
        
        /// <summary>
        /// Checks if the inventory can hold the specified item
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <param name="quantity">The quantity to check</param>
        /// <returns>True if the inventory can hold the item</returns>
        bool CanAddItem(IInventoryItem item, int quantity = 1);
        
        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="quantity">The quantity to add</param>
        /// <returns>True if the item was added successfully</returns>
        bool AddItem(IInventoryItem item, int quantity = 1);
        
        /// <summary>
        /// Adds an item to a specific slot
        /// </summary>
        /// <param name="slotIndex">The slot index</param>
        /// <param name="item">The item to add</param>
        /// <param name="quantity">The quantity to add</param>
        /// <returns>True if the item was added successfully</returns>
        bool AddItemToSlot(int slotIndex, IInventoryItem item, int quantity = 1);
        
        /// <summary>
        /// Removes an item from the inventory
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="quantity">The quantity to remove</param>
        /// <returns>True if the item was removed successfully</returns>
        bool RemoveItem(IInventoryItem item, int quantity = 1);
        
        /// <summary>
        /// Removes an item from a specific slot
        /// </summary>
        /// <param name="slotIndex">The slot index</param>
        /// <param name="quantity">The quantity to remove</param>
        /// <returns>True if the item was removed successfully</returns>
        bool RemoveItemFromSlot(int slotIndex, int quantity = 1);
        
        /// <summary>
        /// Moves an item from one slot to another
        /// </summary>
        /// <param name="fromSlotIndex">The source slot index</param>
        /// <param name="toSlotIndex">The destination slot index</param>
        /// <returns>True if the item was moved successfully</returns>
        bool MoveItem(int fromSlotIndex, int toSlotIndex);
        
        /// <summary>
        /// Gets an empty slot index
        /// </summary>
        /// <returns>The empty slot index or -1 if no empty slot</returns>
        int GetEmptySlotIndex();
        
        /// <summary>
        /// Gets a slot where the item can stack
        /// </summary>
        /// <param name="item">The item to stack</param>
        /// <returns>The slot index or -1 if no slot can stack</returns>
        int GetStackableSlotIndex(IInventoryItem item);
        
        /// <summary>
        /// Equips an item from inventory
        /// </summary>
        /// <param name="inventorySlotIndex">The inventory slot index</param>
        /// <returns>True if the item was equipped successfully</returns>
        bool EquipItem(int inventorySlotIndex);
        
        /// <summary>
        /// Unequips an item from an equipment slot
        /// </summary>
        /// <param name="equipSlot">The equipment slot</param>
        /// <returns>True if the item was unequipped successfully</returns>
        bool UnequipItem(EquipmentSlotType equipSlot);
        
        /// <summary>
        /// Uses an item from inventory
        /// </summary>
        /// <param name="slotIndex">The inventory slot index</param>
        /// <returns>True if the item was used successfully</returns>
        bool UseItem(int slotIndex);
        
        /// <summary>
        /// Drops an item from inventory
        /// </summary>
        /// <param name="slotIndex">The inventory slot index</param>
        /// <param name="quantity">The quantity to drop</param>
        /// <returns>True if the item was dropped successfully</returns>
        bool DropItem(int slotIndex, int quantity = 1);
        
        /// <summary>
        /// Splits an item stack
        /// </summary>
        /// <param name="slotIndex">The inventory slot index</param>
        /// <param name="quantity">The quantity to split</param>
        /// <returns>True if the item stack was split successfully</returns>
        bool SplitItem(int slotIndex, int quantity);
        
        /// <summary>
        /// Adds gold to the inventory
        /// </summary>
        /// <param name="amount">The amount to add</param>
        void AddGold(int amount);
        
        /// <summary>
        /// Removes gold from the inventory
        /// </summary>
        /// <param name="amount">The amount to remove</param>
        /// <returns>True if the gold was removed successfully</returns>
        bool RemoveGold(int amount);
        
        /// <summary>
        /// Sorts the inventory
        /// </summary>
        /// <param name="sortByRarity">Whether to sort by rarity</param>
        void SortInventory(bool sortByRarity = true);
        
        /// <summary>
        /// Gets used slot count
        /// </summary>
        /// <returns>The number of used slots</returns>
        int GetUsedSlotCount();
        
        /// <summary>
        /// Event triggered when an inventory slot changes
        /// </summary>
        event Action<int> SlotChanged;
        
        /// <summary>
        /// Event triggered when an equipment slot changes
        /// </summary>
        event Action<EquipmentSlotType> EquipmentSlotChanged;
        
        /// <summary>
        /// Event triggered when gold amount changes
        /// </summary>
        event Action<int> GoldChanged;
        
        /// <summary>
        /// Event triggered when weight changes
        /// </summary>
        event Action<float> WeightChanged;
        
        /// <summary>
        /// Event triggered when an item is picked up
        /// </summary>
        event Action<IInventoryItem> ItemPickedUp;
    }
}
