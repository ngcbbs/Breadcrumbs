using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// Implementation of IInventoryData that stores inventory information
    /// </summary>
    [Serializable]
    public class InventoryData : IInventoryData
    {
        // Serialized data for saving/loading
        [SerializeField] private int gold = 0;
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
        [SerializeField] private Dictionary<EquipmentSlotType, InventorySlot> equipmentSlots = new Dictionary<EquipmentSlotType, InventorySlot>();
        
        // Events
        public event Action<int> SlotChanged;
        public event Action<EquipmentSlotType> EquipmentSlotChanged;
        public event Action<int> GoldChanged;
        public event Action<float> WeightChanged;
        public event Action<IInventoryItem> ItemPickedUp;
        
        // Properties
        public int Capacity => slots.Count;
        public float CurrentWeight => CalculateTotalWeight();
        public float MaxWeight 
        { 
            get => maxWeight;
            set
            {
                if (maxWeight != value)
                {
                    maxWeight = value;
                    WeightChanged?.Invoke(CurrentWeight);
                }
            }
        }
        public int Gold 
        {
            get => gold;
            private set
            {
                if (gold != value)
                {
                    gold = value;
                    GoldChanged?.Invoke(gold);
                }
            }
        }
        public IReadOnlyList<InventorySlot> Slots => slots.AsReadOnly();
        public IReadOnlyDictionary<EquipmentSlotType, InventorySlot> EquipmentSlots => equipmentSlots;
        
        // External dependencies
        private IItemDropService dropService;
        
        /// <summary>
        /// Creates a new inventory with the specified capacity
        /// </summary>
        /// <param name="capacity">The inventory capacity</param>
        public InventoryData(int capacity = 28)
        {
            // Initialize inventory slots
            slots = new List<InventorySlot>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                slots.Add(new InventorySlot());
            }
            
            // Initialize equipment slots
            foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                equipmentSlots[slot] = new InventorySlot();
            }
        }
        
        /// <summary>
        /// Sets the drop service for item dropping functionality
        /// </summary>
        /// <param name="dropService">The drop service</param>
        public void SetDropService(IItemDropService dropService)
        {
            this.dropService = dropService;
        }
        
        /// <summary>
        /// Calculates the total weight of all items
        /// </summary>
        /// <returns>The total weight</returns>
        private float CalculateTotalWeight()
        {
            float totalWeight = 0f;
            
            // Inventory items
            foreach (var slot in slots)
            {
                totalWeight += slot.GetTotalWeight();
            }
            
            // Equipment items
            foreach (var slot in equipmentSlots.Values)
            {
                totalWeight += slot.GetTotalWeight();
            }
            
            return totalWeight;
        }
        
        /// <summary>
        /// Checks if the inventory can hold the specified item
        /// </summary>
        public bool CanAddItem(IInventoryItem item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
                return false;
                
            // Check weight limit
            float newWeight = CurrentWeight + (item.Weight * quantity);
            if (newWeight > MaxWeight)
                return false;
                
            // Check for stackable slot
            int stackableSlot = GetStackableSlotIndex(item);
            if (stackableSlot != -1)
            {
                InventorySlot slot = slots[stackableSlot];
                int availableSpace = item.MaxStackSize - slot.Quantity;
                if (availableSpace >= quantity)
                    return true;
                    
                // We can stack some, but not all
                quantity -= availableSpace;
            }
            
            // Check for empty slots for remaining quantity
            int requiredSlots = Mathf.CeilToInt((float)quantity / item.MaxStackSize);
            int emptySlots = slots.Count(s => s.IsEmpty());
            
            return emptySlots >= requiredSlots;
        }
        
        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        public bool AddItem(IInventoryItem item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
                return false;
                
            // First try to stack with existing items
            int remaining = quantity;
            int stackableSlot;
            
            while (remaining > 0 && (stackableSlot = GetStackableSlotIndex(item)) != -1)
            {
                InventorySlot slot = slots[stackableSlot];
                int availableSpace = item.MaxStackSize - slot.Quantity;
                int amountToAdd = Mathf.Min(availableSpace, remaining);
                
                if (amountToAdd > 0)
                {
                    if (slot.AddItem(item, amountToAdd))
                    {
                        remaining -= amountToAdd;
                        SlotChanged?.Invoke(stackableSlot);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            
            // If we still have items to add, try to find empty slots
            while (remaining > 0)
            {
                int emptySlot = GetEmptySlotIndex();
                if (emptySlot == -1)
                    break;
                    
                int amountToAdd = Mathf.Min(item.MaxStackSize, remaining);
                if (slots[emptySlot].AddItem(item, amountToAdd))
                {
                    remaining -= amountToAdd;
                    SlotChanged?.Invoke(emptySlot);
                }
                else
                {
                    break;
                }
            }
            
            // Calculate how many we successfully added
            int addedAmount = quantity - remaining;
            if (addedAmount > 0)
            {
                // Trigger events
                WeightChanged?.Invoke(CurrentWeight);
                ItemPickedUp?.Invoke(item);
                
                return addedAmount == quantity; // Return true if all items were added
            }
            
            return false;
        }
        
        /// <summary>
        /// Adds an item to a specific slot
        /// </summary>
        public bool AddItemToSlot(int slotIndex, IInventoryItem item, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count || item == null || quantity <= 0)
                return false;
                
            InventorySlot slot = slots[slotIndex];
            if (slot.AddItem(item, quantity))
            {
                SlotChanged?.Invoke(slotIndex);
                WeightChanged?.Invoke(CurrentWeight);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Removes an item from the inventory
        /// </summary>
        public bool RemoveItem(IInventoryItem item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
                return false;
                
            int remainingToRemove = quantity;
            
            // Find all slots with this item
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];
                if (!slot.IsEmpty() && slot.Item.ItemId == item.ItemId)
                {
                    int amountToRemove = Mathf.Min(slot.Quantity, remainingToRemove);
                    if (slot.RemoveItem(amountToRemove))
                    {
                        remainingToRemove -= amountToRemove;
                        SlotChanged?.Invoke(i);
                        
                        if (remainingToRemove <= 0)
                            break;
                    }
                }
            }
            
            if (remainingToRemove < quantity)
            {
                // At least some items were removed
                WeightChanged?.Invoke(CurrentWeight);
                return remainingToRemove == 0; // True if all were removed
            }
            
            return false;
        }
        
        /// <summary>
        /// Removes an item from a specific slot
        /// </summary>
        public bool RemoveItemFromSlot(int slotIndex, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count || quantity <= 0)
                return false;
                
            InventorySlot slot = slots[slotIndex];
            if (slot.RemoveItem(quantity))
            {
                SlotChanged?.Invoke(slotIndex);
                WeightChanged?.Invoke(CurrentWeight);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Moves an item from one slot to another
        /// </summary>
        public bool MoveItem(int fromSlotIndex, int toSlotIndex)
        {
            if (fromSlotIndex < 0 || fromSlotIndex >= slots.Count ||
                toSlotIndex < 0 || toSlotIndex >= slots.Count ||
                fromSlotIndex == toSlotIndex)
            {
                return false;
            }
            
            InventorySlot fromSlot = slots[fromSlotIndex];
            InventorySlot toSlot = slots[toSlotIndex];
            InventorySlot tempSlot = null;
            
            // If source is empty, nothing to move
            if (fromSlot.IsEmpty())
                return false;
                
            // If destination is empty, simple move
            if (toSlot.IsEmpty())
            {
                tempSlot = new InventorySlot();
                tempSlot.CopyFrom(fromSlot);
                fromSlot.Clear();
                toSlot.CopyFrom(tempSlot);
                
                SlotChanged?.Invoke(fromSlotIndex);
                SlotChanged?.Invoke(toSlotIndex);
                return true;
            }
            
            // If same item and can stack
            if (fromSlot.Item.ItemId == toSlot.Item.ItemId && toSlot.Item.MaxStackSize > 1)
            {
                int totalQuantity = fromSlot.Quantity + toSlot.Quantity;
                int maxStack = toSlot.Item.MaxStackSize;
                
                if (totalQuantity <= maxStack)
                {
                    // Can combine completely
                    toSlot.AddItem(toSlot.Item, fromSlot.Quantity);
                    fromSlot.Clear();
                }
                else
                {
                    // Partial stack
                    int amountToMove = maxStack - toSlot.Quantity;
                    toSlot.AddItem(toSlot.Item, amountToMove);
                    fromSlot.RemoveItem(amountToMove);
                }
                
                SlotChanged?.Invoke(fromSlotIndex);
                SlotChanged?.Invoke(toSlotIndex);
                return true;
            }
            
            // Different items, swap them
            tempSlot = new InventorySlot();
            tempSlot.CopyFrom(toSlot);
            toSlot.CopyFrom(fromSlot);
            fromSlot.CopyFrom(tempSlot);
            
            SlotChanged?.Invoke(fromSlotIndex);
            SlotChanged?.Invoke(toSlotIndex);
            return true;
        }
        
        /// <summary>
        /// Gets an empty slot index
        /// </summary>
        public int GetEmptySlotIndex()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty())
                    return i;
            }
            
            return -1;
        }
        
        /// <summary>
        /// Gets a slot where the item can stack
        /// </summary>
        public int GetStackableSlotIndex(IInventoryItem item)
        {
            if (item.MaxStackSize <= 1)
                return -1;
                
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];
                if (!slot.IsEmpty() && 
                    slot.Item.ItemId == item.ItemId && 
                    slot.Quantity < item.MaxStackSize)
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        /// <summary>
        /// Equips an item from inventory
        /// </summary>
        public bool EquipItem(int inventorySlotIndex)
        {
            if (inventorySlotIndex < 0 || inventorySlotIndex >= slots.Count)
                return false;
                
            InventorySlot slot = slots[inventorySlotIndex];
            if (slot.IsEmpty() || !slot.Item.IsEquipment || !slot.Item.EquipSlot.HasValue)
                return false;
                
            EquipmentSlotType equipSlot = slot.Item.EquipSlot.Value;
            
            // Special case for rings
            if (equipSlot == EquipmentSlotType.Ring1 || equipSlot == EquipmentSlotType.Ring2)
            {
                if (equipmentSlots[EquipmentSlotType.Ring1].IsEmpty())
                    equipSlot = EquipmentSlotType.Ring1;
                else if (equipmentSlots[EquipmentSlotType.Ring2].IsEmpty())
                    equipSlot = EquipmentSlotType.Ring2;
                else
                    equipSlot = EquipmentSlotType.Ring1; // Replace first ring by default
            }
            
            // Check if there's already an item in the equipment slot
            if (!equipmentSlots[equipSlot].IsEmpty())
            {
                // Move the currently equipped item to inventory
                IInventoryItem equippedItem = equipmentSlots[equipSlot].Item;
                int equippedQuantity = equipmentSlots[equipSlot].Quantity;
                
                // Clear the equipment slot
                equipmentSlots[equipSlot].Clear();
                
                // Move equipped item to inventory slot
                slots[inventorySlotIndex].Clear();
                AddItem(equippedItem, equippedQuantity);
            }
            else
            {
                // Just clear the inventory slot
                slots[inventorySlotIndex].Clear();
            }
            
            // Move item to equipment slot
            equipmentSlots[equipSlot].AddItem(slot.Item, slot.Quantity);
            
            // Notify changes
            SlotChanged?.Invoke(inventorySlotIndex);
            EquipmentSlotChanged?.Invoke(equipSlot);
            
            return true;
        }
        
        /// <summary>
        /// Unequips an item from an equipment slot
        /// </summary>
        public bool UnequipItem(EquipmentSlotType equipSlot)
        {
            if (!equipmentSlots.ContainsKey(equipSlot) || equipmentSlots[equipSlot].IsEmpty())
                return false;
                
            // Get the item from the equipment slot
            IInventoryItem item = equipmentSlots[equipSlot].Item;
            int quantity = equipmentSlots[equipSlot].Quantity;
            
            // Check if there's room in the inventory
            if (!CanAddItem(item, quantity))
                return false;
                
            // Clear the equipment slot
            equipmentSlots[equipSlot].Clear();
            
            // Add the item to inventory
            AddItem(item, quantity);
            
            // Notify changes
            EquipmentSlotChanged?.Invoke(equipSlot);
            
            return true;
        }
        
        /// <summary>
        /// Uses an item from inventory
        /// </summary>
        public bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return false;
                
            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty() || !slot.Item.IsUsable)
                return false;
                
            // Use the item
            bool success = slot.Item.Use(null);
            
            // If used successfully, reduce quantity
            if (success)
            {
                slot.RemoveItem(1);
                SlotChanged?.Invoke(slotIndex);
                WeightChanged?.Invoke(CurrentWeight);
            }
            
            return success;
        }
        
        /// <summary>
        /// Drops an item from inventory
        /// </summary>
        public bool DropItem(int slotIndex, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count || quantity <= 0)
                return false;
                
            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty() || quantity > slot.Quantity)
                return false;
                
            // Get the item
            IInventoryItem item = slot.Item;
            
            // If drop service is available, use it
            if (dropService != null)
            {
                dropService.DropItem(item, quantity);
            }
            
            // Remove from inventory
            slot.RemoveItem(quantity);
            SlotChanged?.Invoke(slotIndex);
            WeightChanged?.Invoke(CurrentWeight);
            
            return true;
        }
        
        /// <summary>
        /// Splits an item stack
        /// </summary>
        public bool SplitItem(int slotIndex, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count || quantity <= 0)
                return false;
                
            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty() || quantity >= slot.Quantity)
                return false;
                
            // Find an empty slot
            int emptySlot = GetEmptySlotIndex();
            if (emptySlot == -1)
                return false;
                
            // Split the stack
            IInventoryItem item = slot.Item;
            slot.RemoveItem(quantity);
            slots[emptySlot].AddItem(item, quantity);
            
            // Notify changes
            SlotChanged?.Invoke(slotIndex);
            SlotChanged?.Invoke(emptySlot);
            
            return true;
        }
        
        /// <summary>
        /// Adds gold to the inventory
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;
                
            Gold += amount;
        }
        
        /// <summary>
        /// Removes gold from the inventory
        /// </summary>
        public bool RemoveGold(int amount)
        {
            if (amount <= 0)
                return false;
                
            if (Gold >= amount)
            {
                Gold -= amount;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Sorts the inventory
        /// </summary>
        public void SortInventory(bool sortByRarity = true)
        {
            // Collect all items
            List<(IInventoryItem item, int quantity)> allItems = new List<(IInventoryItem item, int quantity)>();
            
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty())
                {
                    allItems.Add((slots[i].Item, slots[i].Quantity));
                    slots[i].Clear();
                }
            }
            
            // Sort items
            if (sortByRarity)
            {
                // Sort by rarity (highest first) then by type
                allItems.Sort((a, b) => {
                    if ((int)a.item.Rarity != (int)b.item.Rarity)
                        return ((int)b.item.Rarity).CompareTo((int)a.item.Rarity);
                    else
                        return ((int)a.item.ItemType).CompareTo((int)b.item.ItemType);
                });
            }
            else
            {
                // Sort by type then by rarity
                allItems.Sort((a, b) => {
                    if ((int)a.item.ItemType != (int)b.item.ItemType)
                        return ((int)a.item.ItemType).CompareTo((int)b.item.ItemType);
                    else
                        return ((int)b.item.Rarity).CompareTo((int)a.item.Rarity);
                });
            }
            
            // Place items back in inventory
            foreach (var itemPair in allItems)
            {
                AddItem(itemPair.item, itemPair.quantity);
            }
        }
        
        /// <summary>
        /// Gets used slot count
        /// </summary>
        public int GetUsedSlotCount()
        {
            int count = 0;
            
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty())
                {
                    count++;
                }
            }
            
            return count;
        }
    }
}
