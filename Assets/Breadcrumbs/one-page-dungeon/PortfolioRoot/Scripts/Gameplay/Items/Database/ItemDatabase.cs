using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GamePortfolio.Gameplay.Items.Database {
    /// <summary>
    /// Central database for all items in the game
    /// Manages item templates, prefabs, and provides lookup functionality
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject {
        [Header("Item Collections")]
        [SerializeField]
        private List<Item> allItems = new List<Item>();
        [SerializeField]
        private List<WeaponItem> weapons = new List<WeaponItem>();
        [SerializeField]
        private List<EquippableItem> armor = new List<EquippableItem>();
        [SerializeField]
        private List<ConsumableItem> consumables = new List<ConsumableItem>();
        [SerializeField]
        private List<SetItemDefinition> setItems = new List<SetItemDefinition>();

        // Lookup dictionaries for efficient item retrieval
        private Dictionary<string, Item> itemsById = new Dictionary<string, Item>();
        private Dictionary<ItemType, List<Item>> itemsByType = new Dictionary<ItemType, List<Item>>();
        private Dictionary<ItemRarity, List<Item>> itemsByRarity = new Dictionary<ItemRarity, List<Item>>();
        private Dictionary<string, SetItemDefinition> setDefinitions = new Dictionary<string, SetItemDefinition>();

        /// <summary>
        /// Initialize the database
        /// </summary>
        public void Initialize() {
            // Clear existing dictionaries
            itemsById.Clear();
            itemsByType.Clear();
            itemsByRarity.Clear();
            setDefinitions.Clear();

            // Initialize dictionaries for types and rarities
            foreach (ItemType type in System.Enum.GetValues(typeof(ItemType))) {
                itemsByType[type] = new List<Item>();
            }

            foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity))) {
                itemsByRarity[rarity] = new List<Item>();
            }

            // Populate lookup dictionaries
            foreach (var item in allItems) {
                // Add to ID dictionary, ensuring no duplicates
                if (!itemsById.ContainsKey(item.itemID)) {
                    itemsById.Add(item.itemID, item);
                } else {
                    Debug.LogWarning($"Duplicate item ID found in database: {item.itemID}");
                }

                // Add to type dictionary
                itemsByType[item.itemType].Add(item);

                // Add to rarity dictionary
                itemsByRarity[item.rarity].Add(item);
            }

            // Process set items
            foreach (var setDef in setItems) {
                if (!setDefinitions.ContainsKey(setDef.setId)) {
                    setDefinitions.Add(setDef.setId, setDef);
                } else {
                    Debug.LogWarning($"Duplicate set ID found in database: {setDef.setId}");
                }
            }

            Debug.Log($"Item Database initialized with {allItems.Count} items across {setItems.Count} sets");
        }

        /// <summary>
        /// Get an item by its unique ID
        /// </summary>
        public Item GetItemById(string id) {
            if (itemsById.ContainsKey(id)) {
                return itemsById[id].CreateInstance();
            }

            Debug.LogWarning($"Item with ID {id} not found in database");
            return null;
        }

        /// <summary>
        /// Get all items of a specific type
        /// </summary>
        public List<Item> GetItemsByType(ItemType type) {
            if (itemsByType.ContainsKey(type)) {
                return new List<Item>(itemsByType[type]);
            }

            return new List<Item>();
        }

        /// <summary>
        /// Get all items of a specific rarity
        /// </summary>
        public List<Item> GetItemsByRarity(ItemRarity rarity) {
            if (itemsByRarity.ContainsKey(rarity)) {
                return new List<Item>(itemsByRarity[rarity]);
            }

            return new List<Item>();
        }

        /// <summary>
        /// Get all weapons of a specific type
        /// </summary>
        public List<WeaponItem> GetWeaponsByType(WeaponType weaponType) {
            return weapons.Where(w => w.weaponType == weaponType).ToList();
        }

        /// <summary>
        /// Get all armor for a specific equipment slot
        /// </summary>
        public List<EquippableItem> GetArmorBySlot(EquipmentSlot slot) {
            return armor.Where(a => a.equipSlot == slot).ToList();
        }

        /// <summary>
        /// Get all consumables of a specific type
        /// </summary>
        public List<ConsumableItem> GetConsumablesByType(ConsumableType consumableType) {
            return consumables.Where(c => c.consumableType == consumableType).ToList();
        }

        /// <summary>
        /// Get a random item from the database
        /// </summary>
        public Item GetRandomItem() {
            if (allItems.Count == 0)
                return null;

            int randomIndex = Random.Range(0, allItems.Count);
            return allItems[randomIndex].CreateInstance();
        }

        /// <summary>
        /// Get a random item of a specific type
        /// </summary>
        public Item GetRandomItemByType(ItemType type) {
            if (!itemsByType.ContainsKey(type) || itemsByType[type].Count == 0)
                return null;

            int randomIndex = Random.Range(0, itemsByType[type].Count);
            return itemsByType[type][randomIndex].CreateInstance();
        }

        /// <summary>
        /// Get a random item of a specific rarity
        /// </summary>
        public Item GetRandomItemByRarity(ItemRarity rarity) {
            if (!itemsByRarity.ContainsKey(rarity) || itemsByRarity[rarity].Count == 0)
                return null;

            int randomIndex = Random.Range(0, itemsByRarity[rarity].Count);
            return itemsByRarity[rarity][randomIndex].CreateInstance();
        }

        /// <summary>
        /// Get a set definition by its ID
        /// </summary>
        public SetItemDefinition GetSetDefinition(string setId) {
            if (setDefinitions.ContainsKey(setId)) {
                return setDefinitions[setId];
            }

            return null;
        }

        /// <summary>
        /// Get all set definitions
        /// </summary>
        public List<SetItemDefinition> GetAllSetDefinitions() {
            return new List<SetItemDefinition>(setItems);
        }

        /// <summary>
        /// Check if an item is part of a set
        /// </summary>
        public bool IsSetItem(Item item) {
            if (item == null)
                return false;

            foreach (var setDef in setItems) {
                if (setDef.itemIds.Contains(item.itemID)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the set that an item belongs to
        /// </summary>
        public SetItemDefinition GetSetForItem(Item item) {
            if (item == null)
                return null;

            foreach (var setDef in setItems) {
                if (setDef.itemIds.Contains(item.itemID)) {
                    return setDef;
                }
            }

            return null;
        }

        /// <summary>
        /// Add a new item to the database
        /// </summary>
        public void AddItem(Item item) {
            if (item == null)
                return;

            // Check if item already exists
            if (itemsById.ContainsKey(item.itemID)) {
                Debug.LogWarning($"Item with ID {item.itemID} already exists in database");
                return;
            }

            // Add to relevant collections
            allItems.Add(item);
            itemsById.Add(item.itemID, item);
            itemsByType[item.itemType].Add(item);
            itemsByRarity[item.rarity].Add(item);

            // Add to specialized lists
            if (item is WeaponItem weapon) {
                weapons.Add(weapon);
            } else if (item is EquippableItem equippable) {
                armor.Add(equippable);
            } else if (item is ConsumableItem consumable) {
                consumables.Add(consumable);
            }
        }
    }
}