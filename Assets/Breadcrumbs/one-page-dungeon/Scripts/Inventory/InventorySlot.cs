using System;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// Represents a single inventory slot that can hold an item
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private IInventoryItem item;
        [SerializeField] private int quantity;

        /// <summary>
        /// Gets the item in this slot
        /// </summary>
        public IInventoryItem Item => item;

        /// <summary>
        /// Gets the quantity of items in this slot
        /// </summary>
        public int Quantity => quantity;

        /// <summary>
        /// Creates a new empty inventory slot
        /// </summary>
        public InventorySlot()
        {
            Clear();
        }

        /// <summary>
        /// Creates a new inventory slot with the specified item and quantity
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="quantity">The quantity</param>
        public InventorySlot(IInventoryItem item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }

        /// <summary>
        /// Checks if the slot is empty
        /// </summary>
        /// <returns>True if the slot is empty</returns>
        public bool IsEmpty()
        {
            return item == null || quantity <= 0;
        }

        /// <summary>
        /// Clears the slot
        /// </summary>
        public void Clear()
        {
            item = null;
            quantity = 0;
        }

        /// <summary>
        /// Adds an item to the slot
        /// </summary>
        /// <param name="newItem">The item to add</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>True if the item was added successfully</returns>
        public bool AddItem(IInventoryItem newItem, int amount)
        {
            if (IsEmpty())
            {
                // Empty slot - add the item
                item = newItem;
                quantity = amount;
                return true;
            }
            else if (item.Equals(newItem) && item.MaxStackSize > 1)
            {
                // Same item and stackable - increase quantity
                int newQuantity = quantity + amount;
                if (newQuantity <= item.MaxStackSize)
                {
                    quantity = newQuantity;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes an amount of the item from the slot
        /// </summary>
        /// <param name="amount">The amount to remove</param>
        /// <returns>True if the items were removed successfully</returns>
        public bool RemoveItem(int amount)
        {
            if (IsEmpty() || amount > quantity)
                return false;

            quantity -= amount;
            if (quantity <= 0)
                Clear();

            return true;
        }

        /// <summary>
        /// Copies the contents of another slot to this slot
        /// </summary>
        /// <param name="other">The other slot</param>
        public void CopyFrom(InventorySlot other)
        {
            item = other.item;
            quantity = other.quantity;
        }
        
        /// <summary>
        /// Gets the total weight of the items in this slot
        /// </summary>
        /// <returns>The total weight</returns>
        public float GetTotalWeight()
        {
            if (IsEmpty())
                return 0f;
                
            return item.Weight * quantity;
        }
    }
}
