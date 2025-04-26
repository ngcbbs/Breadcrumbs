using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Gameplay.Items.Generation {
    /// <summary>
    /// Settings for item generation used by the ItemGenerator
    /// </summary>
    [CreateAssetMenu(fileName = "ItemGenerationSettings", menuName = "Inventory/Item Generation Settings")]
    public class ItemGenerationSettings : ScriptableObject {
        [Header("Item Type Weights")]
        public List<ItemTypeWeight> itemTypeWeights = new List<ItemTypeWeight>();

        [Header("Rarity Settings")]
        public RarityDropRate[] rarityDropRates = new RarityDropRate[5];          // One for each rarity
        public float[] rarityStatMultipliers = { 1f, 1.2f, 1.5f, 2f, 3f };        // Stat multipliers by rarity
        public float[] rarityEffectMultipliers = { 1f, 1.3f, 1.7f, 2.2f, 3.5f };  // Effect multipliers by rarity
        public float[] rarityValueMultipliers = { 1f, 2f, 5f, 10f, 25f };         // Value multipliers by rarity
        public float[] rarityDurabilityMultipliers = { 1f, 1.25f, 1.5f, 2f, 3f }; // Durability multipliers by rarity

        [Header("Level Scaling")]
        public float statPerLevelIncrease = 0.1f;   // Stats increase by 10% per level
        public float effectPerLevelIncrease = 0.1f; // Effects increase by 10% per level

        [Header("Set Items")]
        public float setItemChance = 0.15f; // Base chance for an item to be part of a set

        [Header("Durability")]
        public float baseDurability = 100f; // Base durability value for all items

        [Header("Prefixes and Suffixes")]
        public List<ItemAffix> commonPrefixes = new List<ItemAffix>();
        public List<ItemAffix> uncommonPrefixes = new List<ItemAffix>();
        public List<ItemAffix> rarePrefixes = new List<ItemAffix>();
        public List<ItemAffix> epicPrefixes = new List<ItemAffix>();
        public List<ItemAffix> legendaryPrefixes = new List<ItemAffix>();

        public List<ItemAffix> commonSuffixes = new List<ItemAffix>();
        public List<ItemAffix> uncommonSuffixes = new List<ItemAffix>();
        public List<ItemAffix> rareSuffixes = new List<ItemAffix>();
        public List<ItemAffix> epicSuffixes = new List<ItemAffix>();
        public List<ItemAffix> legendarySuffixes = new List<ItemAffix>();

        /// <summary>
        /// Get prefixes for a specific rarity
        /// </summary>
        public List<ItemAffix> GetPrefixesForRarity(ItemRarity rarity) {
            switch (rarity) {
                case ItemRarity.Common:
                    return commonPrefixes;
                case ItemRarity.Uncommon:
                    return uncommonPrefixes;
                case ItemRarity.Rare:
                    return rarePrefixes;
                case ItemRarity.Epic:
                    return epicPrefixes;
                case ItemRarity.Legendary:
                    return legendaryPrefixes;
                default:
                    return commonPrefixes;
            }
        }

        /// <summary>
        /// Get suffixes for a specific rarity
        /// </summary>
        public List<ItemAffix> GetSuffixesForRarity(ItemRarity rarity) {
            switch (rarity) {
                case ItemRarity.Common:
                    return commonSuffixes;
                case ItemRarity.Uncommon:
                    return uncommonSuffixes;
                case ItemRarity.Rare:
                    return rareSuffixes;
                case ItemRarity.Epic:
                    return epicSuffixes;
                case ItemRarity.Legendary:
                    return legendarySuffixes;
                default:
                    return commonSuffixes;
            }
        }
    }

    /// <summary>
    /// Defines a weight for an item type during random generation
    /// </summary>
    [System.Serializable]
    public class ItemTypeWeight {
        public ItemType itemType;
        public float weight = 1f;
    }

    /// <summary>
    /// Defines drop rates for a specific rarity tier
    /// </summary>
    [System.Serializable]
    public class RarityDropRate {
        public ItemRarity rarity;
        [Range(0f, 1f)]
        public float baseDropChance = 0.1f; // Base chance regardless of level
        [Range(0f, 1f)]
        public float maxDropChance = 0.5f; // Maximum chance at high levels
        public int maxLevelInfluence = 50; // Level at which max chance is reached
    }

    /// <summary>
    /// Defines item name prefix or suffix with associated stats
    /// </summary>
    [System.Serializable]
    public class ItemAffix {
        public string text; // The actual prefix/suffix text
        public List<StatModifier> statModifiers = new List<StatModifier>();
        public List<ResistanceModifier> resistanceModifiers = new List<ResistanceModifier>();
        public float weight = 1f; // Chance weight for random selection

        // Optional special property
        public bool hasSpecialProperty = false;
        public SpecialPropertyType specialPropertyType;
        public float specialPropertyValue = 0f;
    }
}