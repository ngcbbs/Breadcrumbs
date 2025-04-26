using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace GamePortfolio.Gameplay.Items {
    /// <summary>
    /// Manages the player's inventory of items
    /// </summary>
    public class InventoryManager : MonoBehaviour {
        [Header("Inventory Settings")]
        [SerializeField]
        private int maxInventorySlots = 30;
        [SerializeField]
        private float maxCarryWeight = 100f;

        [Header("Quick Slots")]
        [SerializeField]
        private int quickSlotCount = 4;
        [SerializeField]
        private KeyCode[] quickSlotKeys = new KeyCode[] {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4
        };

        [Header("Auto-Pickup")]
        [SerializeField]
        private bool enableAutoPickup = true;
        [SerializeField]
        private float autoPickupRadius = 2f;
        [SerializeField]
        private LayerMask itemLayer;
        [SerializeField]
        private bool autoEquipBetterItems = false;

        // Events
        [Header("Events")]
        public UnityEvent<InventorySlot> OnItemAdded;
        public UnityEvent<InventorySlot> OnItemRemoved;
        public UnityEvent<InventorySlot> OnItemUsed;
        public UnityEvent<float, float> OnWeightChanged;
        public UnityEvent<int, int> OnCurrencyChanged;

        // Inventory data
        private List<InventorySlot> inventorySlots = new List<InventorySlot>();
        private InventorySlot[] quickSlots;
        private Dictionary<Item, float> itemCooldowns = new Dictionary<Item, float>();

        // Currency
        private int currency = 0;

        // Components
        private EquipmentManager equipmentManager;

        // Properties
        public float CurrentWeight { get; private set; }
        public float MaxWeight => maxCarryWeight;
        public int MaxSlots => maxInventorySlots;
        public int UsedSlots => inventorySlots.Count;
        public int Currency => currency;

        private void Awake() {
            equipmentManager = GetComponent<EquipmentManager>();

            // Initialize quick slots
            quickSlots = new InventorySlot[quickSlotCount];
            for (int i = 0; i < quickSlotCount; i++) {
                quickSlots[i] = null;
            }
        }

        private void Update() {
            // Check for quick slot key presses
            for (int i = 0; i < quickSlotCount; i++) {
                if (i < quickSlotKeys.Length && Input.GetKeyDown(quickSlotKeys[i])) {
                    UseQuickSlotItem(i);
                }
            }

            // Update cooldowns
            UpdateCooldowns();

            // Check for auto-pickup
            if (enableAutoPickup) {
                CheckForItemPickup();
            }
        }

        /// <summary>
        /// Add an item to the inventory
        /// </summary>
        public bool AddItem(Item item, int amount = 1) {
            if (item == null || amount <= 0)
                return false;

            // Check if we can add this item based on weight
            float totalAddedWeight = item.weight * amount;
            if (CurrentWeight + totalAddedWeight > maxCarryWeight) {
                Debug.Log("Cannot add item - inventory too heavy");
                return false;
            }

            // Try to stack with existing items
            if (item.maxStackSize > 1) {
                foreach (var slot in inventorySlots) {
                    if (slot.item.CanStackWith(item) && slot.amount < slot.item.maxStackSize) {
                        // Calculate how many can be added to this stack
                        int canAdd = Mathf.Min(amount, slot.item.maxStackSize - slot.amount);
                        slot.amount += canAdd;
                        amount -= canAdd;

                        // Update weight
                        CurrentWeight += item.weight * canAdd;
                        OnWeightChanged?.Invoke(CurrentWeight, maxCarryWeight);

                        // Trigger event
                        OnItemAdded?.Invoke(slot);

                        // If all items were added, return success
                        if (amount == 0)
                            return true;
                    }
                }
            }

            // Add remaining items as new slots
            while (amount > 0) {
                // Check if we have room for a new slot
                if (inventorySlots.Count >= maxInventorySlots) {
                    Debug.Log("Cannot add item - inventory full");
                    return false;
                }

                // Create a new slot
                InventorySlot newSlot = new InventorySlot {
                    item = item.CreateInstance(),
                    amount = Mathf.Min(amount, item.maxStackSize)
                };

                inventorySlots.Add(newSlot);

                // Update weight
                CurrentWeight += item.weight * newSlot.amount;
                OnWeightChanged?.Invoke(CurrentWeight, maxCarryWeight);

                // Trigger event
                OnItemAdded?.Invoke(newSlot);

                // Decrement remaining amount
                amount -= newSlot.amount;
            }

            // Check for auto-equip
            if (autoEquipBetterItems && item is EquippableItem equippable) {
                AttemptAutoEquip(equippable);
            }

            return true;
        }

        /// <summary>
        /// Remove an item from the inventory
        /// </summary>
        public bool RemoveItem(Item item, int amount = 1) {
            if (item == null || amount <= 0)
                return false;

            int remainingToRemove = amount;

            // Find slots with this item and remove from them
            for (int i = inventorySlots.Count - 1; i >= 0; i--) {
                InventorySlot slot = inventorySlots[i];

                if (slot.item.itemID == item.itemID) {
                    // Calculate how many to remove from this slot
                    int toRemove = Mathf.Min(remainingToRemove, slot.amount);
                    slot.amount -= toRemove;
                    remainingToRemove -= toRemove;

                    // Update weight
                    CurrentWeight -= item.weight * toRemove;
                    OnWeightChanged?.Invoke(CurrentWeight, maxCarryWeight);

                    // If slot is now empty, remove it
                    if (slot.amount <= 0) {
                        // Check if this is a quick slot item
                        for (int q = 0; q < quickSlots.Length; q++) {
                            if (quickSlots[q] == slot) {
                                quickSlots[q] = null;
                            }
                        }

                        // Trigger event
                        OnItemRemoved?.Invoke(slot);

                        // Remove the slot
                        inventorySlots.RemoveAt(i);
                    } else {
                        // Slot still has items, trigger update event
                        OnItemRemoved?.Invoke(slot);
                    }

                    // If all items were removed, return success
                    if (remainingToRemove == 0)
                        return true;
                }
            }

            // If we get here, we didn't remove all requested items
            return remainingToRemove < amount;
        }

        /// <summary>
        /// Use an item from the inventory
        /// </summary>
        public bool UseItem(InventorySlot slot) {
            if (slot == null || slot.item == null || slot.amount <= 0)
                return false;

            // Use the item
            bool itemUsed = slot.item.Use(gameObject);

            if (itemUsed) {
                // Trigger event
                OnItemUsed?.Invoke(slot);

                // Reduce amount if consumable
                if (slot.item is ConsumableItem consumable && consumable.isConsumedOnUse) {
                    slot.amount--;

                    // Update weight
                    CurrentWeight -= slot.item.weight;
                    OnWeightChanged?.Invoke(CurrentWeight, maxCarryWeight);

                    // Remove slot if empty
                    if (slot.amount <= 0) {
                        // Check if this is a quick slot item
                        for (int q = 0; q < quickSlots.Length; q++) {
                            if (quickSlots[q] == slot) {
                                quickSlots[q] = null;
                            }
                        }

                        // Remove from inventory
                        inventorySlots.Remove(slot);

                        // Trigger removal event
                        OnItemRemoved?.Invoke(slot);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Set an item to a quick slot
        /// </summary>
        public bool SetQuickSlotItem(InventorySlot slot, int quickSlotIndex) {
            if (quickSlotIndex < 0 || quickSlotIndex >= quickSlotCount)
                return false;

            if (slot != null && !inventorySlots.Contains(slot))
                return false;

            quickSlots[quickSlotIndex] = slot;
            return true;
        }

        /// <summary>
        /// Use an item from a quick slot
        /// </summary>
        public bool UseQuickSlotItem(int quickSlotIndex) {
            if (quickSlotIndex < 0 || quickSlotIndex >= quickSlotCount)
                return false;

            InventorySlot slot = quickSlots[quickSlotIndex];

            if (slot == null || slot.amount <= 0)
                return false;

            return UseItem(slot);
        }

        /// <summary>
        /// Check if an item is on cooldown
        /// </summary>
        public bool IsItemOnCooldown(Item item) {
            if (item == null || !itemCooldowns.ContainsKey(item))
                return false;

            return itemCooldowns[item] > 0f;
        }

        /// <summary>
        /// Start a cooldown for an item
        /// </summary>
        public void StartItemCooldown(Item item, float duration) {
            if (item == null || duration <= 0f)
                return;

            itemCooldowns[item] = duration;
        }

        /// <summary>
        /// Update all item cooldowns
        /// </summary>
        private void UpdateCooldowns() {
            List<Item> completedCooldowns = new List<Item>();

            foreach (var kvp in itemCooldowns) {
                Item item = kvp.Key;
                float cooldown = kvp.Value - Time.deltaTime;

                if (cooldown <= 0f) {
                    completedCooldowns.Add(item);
                } else {
                    itemCooldowns[item] = cooldown;
                }
            }

            // Remove completed cooldowns
            foreach (var item in completedCooldowns) {
                itemCooldowns.Remove(item);
            }
        }

        /// <summary>
        /// Get the remaining cooldown for an item
        /// </summary>
        public float GetItemCooldown(Item item) {
            if (item == null || !itemCooldowns.ContainsKey(item))
                return 0f;

            return itemCooldowns[item];
        }

        /// <summary>
        /// Add currency to the inventory
        /// </summary>
        public void AddCurrency(int amount) {
            if (amount <= 0)
                return;

            int oldCurrency = currency;
            currency += amount;

            OnCurrencyChanged?.Invoke(oldCurrency, currency);
        }

        /// <summary>
        /// Remove currency from the inventory
        /// </summary>
        public bool RemoveCurrency(int amount) {
            if (amount <= 0)
                return true;

            if (currency < amount)
                return false;

            int oldCurrency = currency;
            currency -= amount;

            OnCurrencyChanged?.Invoke(oldCurrency, currency);
            return true;
        }

        /// <summary>
        /// Check for items to auto-pickup
        /// </summary>
        private void CheckForItemPickup() {
            Debug.Log("Checking for items to auto-pickup");
#if false
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, autoPickupRadius, itemLayer);
            
            foreach (var hitCollider in hitColliders)
            {
                WorldItem worldItem = hitCollider.GetComponent<WorldItem>();
                
                if (worldItem != null && !worldItem.isBeingPickedUp)
                {
                    // Mark as being picked up to prevent multiple attempts
                    worldItem.isBeingPickedUp = true;
                    
                    // Try to add to inventory
                    bool success = AddItem(worldItem.item, worldItem.amount);
                    
                    if (success)
                    {
                        // Successfully picked up
                        Destroy(worldItem.gameObject);
                    }
                    else
                    {
                        // Failed to pick up, unmark
                        worldItem.isBeingPickedUp = false;
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Check if an item is better than currently equipped and equip it
        /// </summary>
        private void AttemptAutoEquip(EquippableItem item) {
            if (equipmentManager == null || item == null)
                return;

            // For a real game, implement logic to determine if an item is better
            // based on stats, player level, etc.
            // This is a simplified implementation

            EquippableItem currentItem = equipmentManager.GetEquippedItem(item.equipSlot);

            // Simple check: higher rarity is better
            if (currentItem == null || (int)item.rarity > (int)currentItem.rarity) {
                equipmentManager.EquipItem(item);
            }
        }

        /// <summary>
        /// Get all inventory slots
        /// </summary>
        public List<InventorySlot> GetAllItems() {
            return new List<InventorySlot>(inventorySlots);
        }

        /// <summary>
        /// Get items of a specific type
        /// </summary>
        public List<InventorySlot> GetItemsByType(ItemType type) {
            return inventorySlots.Where(slot => slot.item.itemType == type).ToList();
        }

        /// <summary>
        /// Get count of a specific item
        /// </summary>
        public int GetItemCount(string itemID) {
            int count = 0;

            foreach (var slot in inventorySlots) {
                if (slot.item.itemID == itemID) {
                    count += slot.amount;
                }
            }

            return count;
        }

        /// <summary>
        /// Check if the inventory has a specific item
        /// </summary>
        public bool HasItem(Item item, int count) {
            return inventorySlots.Any(slot => slot.item == item && slot.amount >= count);
        }

        /// <summary>
        /// Sort inventory by a criteria
        /// </summary>
        public void SortInventory(InventorySortType sortType) {
            switch (sortType) {
                case InventorySortType.ByName:
                    inventorySlots.Sort((a, b) => string.Compare(a.item.itemName, b.item.itemName));
                    break;
                case InventorySortType.ByType:
                    inventorySlots.Sort((a, b) => ((int)a.item.itemType).CompareTo((int)b.item.itemType));
                    break;
                case InventorySortType.ByRarity:
                    inventorySlots.Sort((a, b) => ((int)b.item.rarity).CompareTo((int)a.item.rarity));
                    break;
                case InventorySortType.ByWeight:
                    inventorySlots.Sort((a, b) => (a.item.weight).CompareTo(b.item.weight));
                    break;
                case InventorySortType.ByValue:
                    inventorySlots.Sort((a, b) => (b.item.sellPrice).CompareTo(a.item.sellPrice));
                    break;
            }
        }
    }

    /// <summary>
    /// Represents a slot in the inventory
    /// </summary>
    [System.Serializable]
    public class InventorySlot {
        public Item item;
        public int amount;
    }

    /// <summary>
    /// Ways to sort the inventory
    /// </summary>
    public enum InventorySortType {
        ByName,
        ByType,
        ByRarity,
        ByWeight,
        ByValue
    }
}