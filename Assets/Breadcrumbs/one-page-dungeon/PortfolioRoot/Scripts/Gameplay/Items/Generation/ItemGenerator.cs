using UnityEngine;
using System.Collections.Generic;
using GamePortfolio.Gameplay.Items.Database;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items.Generation {
    /// <summary>
    /// Generates random items with procedural properties based on templates
    /// </summary>
    public partial class ItemGenerator : MonoBehaviour {
        [SerializeField]
        private ItemDatabase itemDatabase;
        [SerializeField]
        private ItemGenerationSettings generationSettings;

        /// <summary>
        /// Generate a random item
        /// </summary>
        public Item GenerateRandomItem(int playerLevel = 1) {
            // Determine item type
            ItemType itemType = GetRandomItemType();

            // Determine item rarity
            ItemRarity rarity = GetRandomRarity(playerLevel);

            // Generate appropriate item
            switch (itemType) {
                case ItemType.Weapon:
                    return GenerateWeapon(rarity, playerLevel);

                case ItemType.Armor:
                    return GenerateArmor(rarity, playerLevel);

                case ItemType.Consumable:
                    return GenerateConsumable(rarity, playerLevel);

                default:
                    // For other types like quest items, materials, etc.
                    return GenerateBasicItem(itemType, rarity, playerLevel);
            }
        }

        /// <summary>
        /// Generate a random item of a specific type
        /// </summary>
        public Item GenerateRandomItemOfType(ItemType type, int playerLevel = 1) {
            // Determine item rarity
            ItemRarity rarity = GetRandomRarity(playerLevel);

            // Generate appropriate item
            switch (type) {
                case ItemType.Weapon:
                    return GenerateWeapon(rarity, playerLevel);

                case ItemType.Armor:
                    return GenerateArmor(rarity, playerLevel);

                case ItemType.Consumable:
                    return GenerateConsumable(rarity, playerLevel);

                default:
                    // For other types like quest items, materials, etc.
                    return GenerateBasicItem(type, rarity, playerLevel);
            }
        }

        /// <summary>
        /// Generate a random item of a specific rarity
        /// </summary>
        public Item GenerateRandomItemOfRarity(ItemRarity rarity, int playerLevel = 1) {
            // Determine item type
            ItemType itemType = GetRandomItemType();

            // Generate appropriate item
            switch (itemType) {
                case ItemType.Weapon:
                    return GenerateWeapon(rarity, playerLevel);

                case ItemType.Armor:
                    return GenerateArmor(rarity, playerLevel);

                case ItemType.Consumable:
                    return GenerateConsumable(rarity, playerLevel);

                default:
                    // For other types like quest items, materials, etc.
                    return GenerateBasicItem(itemType, rarity, playerLevel);
            }
        }

        /// <summary>
        /// Generate a random weapon
        /// </summary>
        public WeaponItem GenerateWeapon(ItemRarity rarity, int playerLevel = 1) {
            // Get a random weapon template from the database
            List<Item> weapons = itemDatabase.GetItemsByType(ItemType.Weapon);

            if (weapons.Count == 0) {
                Debug.LogWarning("No weapon templates found in database");
                return null;
            }

            // Select a random template
            WeaponItem template = (WeaponItem)weapons[Random.Range(0, weapons.Count)];

            // Create a new instance of the weapon
            WeaponItem weapon = (WeaponItem)template.CreateInstance();

            // Adjust stats based on level and rarity
            AdjustWeaponStats(weapon, rarity, playerLevel);

            // Generate a unique name
            weapon.itemName = GenerateWeaponName(weapon);

            // Set rarity
            weapon.rarity = rarity;

            // Add special properties based on rarity
            AddWeaponSpecialProperties(weapon, rarity);

            // Add set association if applicable
            TryMakeSetItem(weapon, rarity);

            // Setup durability based on rarity
            SetupDurability(weapon, rarity);

            return weapon;
        }

        /// <summary>
        /// Generate a random armor piece
        /// </summary>
        public EquippableItem GenerateArmor(ItemRarity rarity, int playerLevel = 1) {
            // Get a random armor template from the database
            List<Item> armors = itemDatabase.GetItemsByType(ItemType.Armor);

            if (armors.Count == 0) {
                Debug.LogWarning("No armor templates found in database");
                return null;
            }

            // Select a random template
            EquippableItem template = (EquippableItem)armors[Random.Range(0, armors.Count)];

            // Create a new instance of the armor
            EquippableItem armor = (EquippableItem)template.CreateInstance();

            // Adjust stats based on level and rarity
            AdjustArmorStats(armor, rarity, playerLevel);

            // Generate a unique name
            armor.itemName = GenerateArmorName(armor);

            // Set rarity
            armor.rarity = rarity;

            // Add stat modifiers based on rarity
            AddArmorStatModifiers(armor, rarity);

            // Add set association if applicable
            TryMakeSetItem(armor, rarity);

            // Setup durability based on rarity
            SetupDurability(armor, rarity);

            return armor;
        }
    }
}