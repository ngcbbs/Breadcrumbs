using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Items.Database;

namespace GamePortfolio.Gameplay.Items.Generation {
    /// <summary>
    /// Partial class for ItemGenerator - Utility functions
    /// </summary>
    public partial class ItemGenerator : MonoBehaviour {
        /// <summary>
        /// Get a random item type based on configured weights
        /// </summary>
        private ItemType GetRandomItemType() {
            float totalWeight = 0;

            foreach (var typeWeight in generationSettings.itemTypeWeights) {
                totalWeight += typeWeight.weight;
            }

            float randomValue = Random.Range(0, totalWeight);
            float accumulatedWeight = 0;

            foreach (var typeWeight in generationSettings.itemTypeWeights) {
                accumulatedWeight += typeWeight.weight;

                if (randomValue <= accumulatedWeight) {
                    return typeWeight.itemType;
                }
            }

            // Default to material item type
            return ItemType.Material;
        }

        /// <summary>
        /// Get a random rarity based on player level and configured drop rates
        /// </summary>
        private ItemRarity GetRandomRarity(int playerLevel) {
            // Get rarity weights based on player level
            float[] rarityWeights = new float[generationSettings.rarityDropRates.Length];

            for (int i = 0; i < generationSettings.rarityDropRates.Length; i++) {
                RarityDropRate dropRate = generationSettings.rarityDropRates[i];

                // Calculate probability based on player level
                float levelFactor = Mathf.Clamp01((float)playerLevel / dropRate.maxLevelInfluence);
                rarityWeights[i] = Mathf.Lerp(dropRate.baseDropChance, dropRate.maxDropChance, levelFactor);
            }

            // Normalize weights to ensure they sum to 1
            float totalWeight = 0;
            foreach (var weight in rarityWeights) {
                totalWeight += weight;
            }

            for (int i = 0; i < rarityWeights.Length; i++) {
                rarityWeights[i] /= totalWeight;
            }

            // Select rarity based on weights
            float randomValue = Random.value;
            float cumulativeWeight = 0;

            for (int i = 0; i < rarityWeights.Length; i++) {
                cumulativeWeight += rarityWeights[i];

                if (randomValue <= cumulativeWeight) {
                    return (ItemRarity)i;
                }
            }

            // Default to common rarity
            return ItemRarity.Common;
        }

        /// <summary>
        /// Setup item durability based on rarity
        /// </summary>
        private void SetupDurability(EquippableItem item, ItemRarity rarity) {
            // Enable durability system
            item.useDurability = true;

            // Base durability scaled by rarity
            float baseDurability = generationSettings.baseDurability;
            float durabilityMultiplier = generationSettings.rarityDurabilityMultipliers[(int)rarity];

            item.maxDurability = baseDurability * durabilityMultiplier;
            item.currentDurability = item.maxDurability;

            // Higher rarity items lose durability more slowly
            item.durabilityLossRate = 1f - (0.1f * (int)rarity);
            item.durabilityLossRate = Mathf.Max(0.5f, item.durabilityLossRate); // Minimum rate of 0.5
        }

        /// <summary>
        /// Try to make the item part of a set (for rare+ items)
        /// </summary>
        private void TryMakeSetItem(Item item, ItemRarity rarity) {
            // Only eligible for rare or higher
            if (rarity < ItemRarity.Rare)
                return;

            // Chance increases with rarity
            float setChance = generationSettings.setItemChance * (1f + (0.5f * ((int)rarity - 2)));

            if (Random.value <= setChance) {
                // Get available sets of the same rarity
                List<SetItemDefinition> availableSets = new List<SetItemDefinition>();

                foreach (var setDef in itemDatabase.GetAllSetDefinitions()) {
                    if (setDef.setRarity == rarity) {
                        availableSets.Add(setDef);
                    }
                }

                if (availableSets.Count > 0) {
                    // Select a random set
                    SetItemDefinition setDef = availableSets[Random.Range(0, availableSets.Count)];

                    // Add to set (in a real implementation, this would update the item ID to match the set item ID)
                    // and update the name to reflect the set

                    // For now, just update the name as a placeholder for the concept
                    item.itemName = $"{setDef.setName} {item.itemName}";
                }
            }
        }

        /// <summary>
        /// Generate a unique name for a weapon
        /// </summary>
        private string GenerateWeaponName(WeaponItem weapon) {
            // This is a simplified implementation
            // In a real game, you would have more sophisticated name generation

            string[] prefixes = { "Sharp", "Deadly", "Savage", "Brutal", "Fierce", "Honorable", "Glorious", "Ancient" };
            string[] suffixes = { "of Power", "of Strength", "of the Tiger", "of the Eagle", "of the Warrior", "of Victory" };

            if (weapon.rarity <= ItemRarity.Uncommon) {
                // Simple name for common/uncommon
                return weapon.itemName;
            } else if (weapon.rarity == ItemRarity.Rare) {
                // Add a prefix for rare
                string prefix = prefixes[Random.Range(0, prefixes.Length)];
                return $"{prefix} {weapon.itemName}";
            } else if (weapon.rarity == ItemRarity.Epic) {
                // Add a suffix for epic
                string suffix = suffixes[Random.Range(0, suffixes.Length)];
                return $"{weapon.itemName} {suffix}";
            } else // Legendary
            {
                // Add both prefix and suffix for legendary
                string prefix = prefixes[Random.Range(0, prefixes.Length)];
                string suffix = suffixes[Random.Range(0, suffixes.Length)];
                return $"{prefix} {weapon.itemName} {suffix}";
            }
        }

        /// <summary>
        /// Generate a unique name for armor
        /// </summary>
        private string GenerateArmorName(EquippableItem armor) {
            // This is a simplified implementation
            // In a real game, you would have more sophisticated name generation

            string[] prefixes = { "Sturdy", "Reinforced", "Protective", "Guardian", "Defender", "Warden", "Royal", "Sacred" };
            string[] suffixes =
                { "of Defense", "of Warding", "of Protection", "of the Shield", "of the Mountain", "of Resilience" };

            if (armor.rarity <= ItemRarity.Uncommon) {
                // Simple name for common/uncommon
                return armor.itemName;
            } else if (armor.rarity == ItemRarity.Rare) {
                // Add a prefix for rare
                string prefix = prefixes[Random.Range(0, prefixes.Length)];
                return $"{prefix} {armor.itemName}";
            } else if (armor.rarity == ItemRarity.Epic) {
                // Add a suffix for epic
                string suffix = suffixes[Random.Range(0, suffixes.Length)];
                return $"{armor.itemName} {suffix}";
            } else // Legendary
            {
                // Add both prefix and suffix for legendary
                string prefix = prefixes[Random.Range(0, prefixes.Length)];
                string suffix = suffixes[Random.Range(0, suffixes.Length)];
                return $"{prefix} {armor.itemName} {suffix}";
            }
        }

        /// <summary>
        /// Generate a unique name for a consumable
        /// </summary>
        private string GenerateConsumableName(ConsumableItem consumable) {
            // This is a simplified implementation
            // In a real game, you would have more sophisticated name generation

            string[] prefixes = { "Potent", "Superior", "Enhanced", "Refined", "Concentrated", "Pure", "Master", "Divine" };

            if (consumable.rarity <= ItemRarity.Uncommon) {
                // Simple name for common/uncommon
                return consumable.itemName;
            } else {
                // Add appropriate prefix based on rarity
                string prefix = prefixes[Mathf.Min((int)consumable.rarity - 1, prefixes.Length - 1)];
                return $"{prefix} {consumable.itemName}";
            }
        }

        /// <summary>
        /// Get the name for a special property
        /// </summary>
        private string GetSpecialPropertyName(SpecialPropertyType propertyType) {
            switch (propertyType) {
                case SpecialPropertyType.ElementalDamage:
                    return "Elemental Damage";
                case SpecialPropertyType.LifeSteal:
                    return "Life Steal";
                case SpecialPropertyType.BleedChance:
                    return "Bleeding";
                case SpecialPropertyType.StunChance:
                    return "Stunning";
                case SpecialPropertyType.ArmorPenetration:
                    return "Armor Penetration";
                case SpecialPropertyType.StatusImmunity:
                    return "Status Immunity";
                case SpecialPropertyType.AreaDamage:
                    return "Area Damage";
                default:
                    return "Unknown Property";
            }
        }

        /// <summary>
        /// Get the description for a special property
        /// </summary>
        private string GetSpecialPropertyDescription(SpecialPropertyType propertyType, float value) {
            switch (propertyType) {
                case SpecialPropertyType.LifeSteal:
                    return $"{value:F1}% of damage dealt is returned as health";
                case SpecialPropertyType.BleedChance:
                    return $"{value:F1}% chance to cause bleeding, dealing damage over time";
                case SpecialPropertyType.StunChance:
                    return $"{value:F1}% chance to stun the target for 1 second";
                case SpecialPropertyType.ArmorPenetration:
                    return $"Ignores {value:F1}% of target's armor";
                case SpecialPropertyType.StatusImmunity:
                    return $"{value:F1}% chance to resist negative status effects";
                case SpecialPropertyType.AreaDamage:
                    return $"Deals {value:F1} damage to enemies within 3 meters";
                default:
                    return "Unknown effect";
            }
        }

        /// <summary>
        /// Get the value for a special property based on rarity
        /// </summary>
        private float GetSpecialPropertyValue(SpecialPropertyType propertyType, ItemRarity rarity) {
            float baseValue = 0;

            switch (propertyType) {
                case SpecialPropertyType.ElementalDamage:
                    baseValue = 5f;
                    break;
                case SpecialPropertyType.LifeSteal:
                    baseValue = 3f;
                    break;
                case SpecialPropertyType.BleedChance:
                    baseValue = 5f;
                    break;
                case SpecialPropertyType.StunChance:
                    baseValue = 3f;
                    break;
                case SpecialPropertyType.ArmorPenetration:
                    baseValue = 10f;
                    break;
                case SpecialPropertyType.StatusImmunity:
                    baseValue = 10f;
                    break;
                case SpecialPropertyType.AreaDamage:
                    baseValue = 5f;
                    break;
            }

            // Scale by rarity
            return baseValue * (1f + (0.5f * (int)rarity));
        }

        /// <summary>
        /// Get the value for a stat modifier based on stat type, modifier type, and rarity
        /// </summary>
        private float GetStatModifierValue(StatType statType, ModifierType modifierType, ItemRarity rarity) {
            float baseValue = 0;

            // Set base value based on stat type and modifier type
            switch (statType) {
                case StatType.Health:
                case StatType.Mana:
                case StatType.Stamina:
                    baseValue = modifierType == ModifierType.Flat ? 10f : 3f;
                    break;

                case StatType.Strength:
                case StatType.Dexterity:
                case StatType.Intelligence:
                    baseValue = modifierType == ModifierType.Flat ? 1f : 2f;
                    break;

                case StatType.PhysicalDamage:
                case StatType.MagicalDamage:
                    baseValue = modifierType == ModifierType.Flat ? 3f : 2f;
                    break;

                case StatType.CriticalChance:
                    baseValue = 2f; // Always percentage
                    break;

                case StatType.CriticalDamage:
                    baseValue = 5f; // Always percentage
                    break;

                case StatType.AttackSpeed:
                    baseValue = 3f; // Always percentage
                    break;

                case StatType.MovementSpeed:
                    baseValue = 3f; // Always percentage
                    break;

                case StatType.Defense:
                    baseValue = modifierType == ModifierType.Flat ? 2f : 3f;
                    break;
            }

            // Scale by rarity
            return baseValue * (1f + (0.5f * (int)rarity));
        }

        /// <summary>
        /// Get the description for a consumable effect
        /// </summary>
        private string GetConsumableEffectDescription(ConsumableEffectType effectType, float value) {
            switch (effectType) {
                case ConsumableEffectType.RestoreHealth:
                    return $"Restores {value} health";

                case ConsumableEffectType.RestoreMana:
                    return $"Restores {value} mana";

                case ConsumableEffectType.RestoreStamina:
                    return $"Restores {value} stamina";

                case ConsumableEffectType.TemporaryDamageBoost:
                    return $"Increases damage by {value}% for a short time";

                case ConsumableEffectType.TemporaryDefenseBoost:
                    return $"Increases defense by {value}% for a short time";

                case ConsumableEffectType.TemporarySpeedBoost:
                    return $"Increases movement speed by {value}% for a short time";

                case ConsumableEffectType.Cure:
                    return "Removes all negative status effects";

                case ConsumableEffectType.AreaDamage:
                    return $"Deals {value} area damage to enemies in a {value / 10f:F1}m radius";

                case ConsumableEffectType.Teleport:
                    return "Teleports to a safe location";

                case ConsumableEffectType.Invulnerability:
                    return $"Grants invulnerability for {value} seconds";

                default:
                    return "Unknown effect";
            }
        }
    }
}