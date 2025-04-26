using UnityEngine;
using System.Collections.Generic;
using GamePortfolio.Gameplay.Items.Database;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items.Generation {
    /// <summary>
    /// Partial class for ItemGenerator - Item Type specific functionality
    /// </summary>
    public partial class ItemGenerator : MonoBehaviour {
        /// <summary>
        /// Generate a random consumable item
        /// </summary>
        public ConsumableItem GenerateConsumable(ItemRarity rarity, int playerLevel = 1) {
            // Get a random consumable template from the database
            List<Item> consumables = itemDatabase.GetItemsByType(ItemType.Consumable);

            if (consumables.Count == 0) {
                Debug.LogWarning("No consumable templates found in database");
                return null;
            }

            // Select a random template
            ConsumableItem template = (ConsumableItem)consumables[Random.Range(0, consumables.Count)];

            // Create a new instance of the consumable
            ConsumableItem consumable = (ConsumableItem)template.CreateInstance();

            // Adjust effects based on level and rarity
            AdjustConsumableEffects(consumable, rarity, playerLevel);

            // Generate a unique name
            consumable.itemName = GenerateConsumableName(consumable);

            // Set rarity
            consumable.rarity = rarity;

            // Adjust properties based on rarity (e.g., duration, cooldown)
            AdjustConsumableProperties(consumable, rarity);

            return consumable;
        }

        /// <summary>
        /// Generate a basic item (non-weapon, non-armor, non-consumable)
        /// </summary>
        public Item GenerateBasicItem(ItemType type, ItemRarity rarity, int playerLevel = 1) {
            // Get a random template from the database of the specified type
            List<Item> items = itemDatabase.GetItemsByType(type);

            if (items.Count == 0) {
                Debug.LogWarning($"No templates found in database for type {type}");
                return null;
            }

            // Select a random template
            Item template = items[Random.Range(0, items.Count)];

            // Create a new instance of the item
            Item item = template.CreateInstance();

            // Set rarity
            item.rarity = rarity;

            // Adjust value based on rarity
            item.buyPrice = Mathf.RoundToInt(item.buyPrice * generationSettings.rarityValueMultipliers[(int)rarity]);
            item.sellPrice = Mathf.RoundToInt(item.sellPrice * generationSettings.rarityValueMultipliers[(int)rarity]);

            return item;
        }

        /// <summary>
        /// Adjust weapon stats based on level and rarity
        /// </summary>
        private void AdjustWeaponStats(WeaponItem weapon, ItemRarity rarity, int playerLevel) {
            float rarityMultiplier = generationSettings.rarityStatMultipliers[(int)rarity];
            float levelMultiplier = 1f + (playerLevel - 1) * generationSettings.statPerLevelIncrease;

            // Scale base damage
            weapon.baseDamage = Mathf.Round(weapon.baseDamage * rarityMultiplier * levelMultiplier);

            // Scale critical stats
            weapon.criticalChance = Mathf.Clamp01(weapon.criticalChance + (0.01f * (int)rarity));
            weapon.criticalMultiplier = weapon.criticalMultiplier + (0.1f * (int)rarity);

            // Scale value
            weapon.buyPrice = Mathf.RoundToInt(weapon.buyPrice * rarityMultiplier * levelMultiplier);
            weapon.sellPrice = Mathf.RoundToInt(weapon.sellPrice * rarityMultiplier * levelMultiplier);

            // Level requirement
            weapon.levelRequirement = Mathf.Max(1, playerLevel - Random.Range(0, 3));
        }

        /// <summary>
        /// Add special properties to a weapon based on rarity
        /// </summary>
        private void AddWeaponSpecialProperties(WeaponItem weapon, ItemRarity rarity) {
            // Clear existing special properties
            weapon.specialProperties.Clear();

            // Add a number of special properties based on rarity
            int propertyCount = Mathf.Max(0, (int)rarity - 1);

            for (int i = 0; i < propertyCount; i++) {
                // Select a random property type
                SpecialPropertyType propertyType =
                    (SpecialPropertyType)Random.Range(0, System.Enum.GetValues(typeof(SpecialPropertyType)).Length);

                // Create and add the property
                WeaponSpecialProperty property = new WeaponSpecialProperty {
                    name = GetSpecialPropertyName(propertyType),
                    propertyType = propertyType,
                    value = GetSpecialPropertyValue(propertyType, rarity)
                };

                // Set elemental type if applicable
                if (propertyType == SpecialPropertyType.ElementalDamage) {
                    property.elementalType = (DamageType)Random.Range(1, System.Enum.GetValues(typeof(DamageType)).Length);
                    property.description = $"Deals {property.value:F1} additional {property.elementalType} damage";
                } else {
                    property.description = GetSpecialPropertyDescription(propertyType, property.value);
                }

                weapon.specialProperties.Add(property);
            }
        }

        /// <summary>
        /// Adjust armor stats based on level and rarity
        /// </summary>
        private void AdjustArmorStats(EquippableItem armor, ItemRarity rarity, int playerLevel) {
            float rarityMultiplier = generationSettings.rarityStatMultipliers[(int)rarity];
            float levelMultiplier = 1f + (playerLevel - 1) * generationSettings.statPerLevelIncrease;

            // Scale value
            armor.buyPrice = Mathf.RoundToInt(armor.buyPrice * rarityMultiplier * levelMultiplier);
            armor.sellPrice = Mathf.RoundToInt(armor.sellPrice * rarityMultiplier * levelMultiplier);

            // Level requirement
            armor.levelRequirement = Mathf.Max(1, playerLevel - Random.Range(0, 3));
        }

        /// <summary>
        /// Add stat modifiers to armor based on rarity
        /// </summary>
        private void AddArmorStatModifiers(EquippableItem armor, ItemRarity rarity) {
            // Clear existing stat modifiers
            armor.statModifiers.Clear();
            armor.resistanceModifiers.Clear();

            // Add a number of stat modifiers based on rarity
            int modifierCount = (int)rarity + 1;

            for (int i = 0; i < modifierCount; i++) {
                // Decide whether to add a stat modifier or resistance modifier
                if (Random.value < 0.7f) // 70% chance for stat modifier
                {
                    // Select a random stat type
                    StatType statType = (StatType)Random.Range(0, System.Enum.GetValues(typeof(StatType)).Length);

                    // Choose modifier type (flat or percentage)
                    ModifierType modifierType = Random.value < 0.7f ? ModifierType.Flat : ModifierType.Percentage;

                    // Calculate value based on rarity
                    float value = GetStatModifierValue(statType, modifierType, rarity);

                    // Create and add the modifier
                    StatModifier modifier = new StatModifier {
                        statType = statType,
                        modifierType = modifierType,
                        value = value
                    };

                    armor.statModifiers.Add(modifier);
                } else // 30% chance for resistance modifier
                {
                    // Select a random damage type (skip Physical, so start at index 1)
                    DamageType damageType = (DamageType)Random.Range(1, System.Enum.GetValues(typeof(DamageType)).Length);

                    // Calculate value based on rarity (resistance is in percentage)
                    float value = 5f + (5f * (int)rarity);

                    // Create and add the modifier
                    ResistanceModifier modifier = new ResistanceModifier {
                        damageType = damageType,
                        value = value
                    };

                    armor.resistanceModifiers.Add(modifier);
                }
            }
        }

        /// <summary>
        /// Adjust consumable effects based on level and rarity
        /// </summary>
        private void AdjustConsumableEffects(ConsumableItem consumable, ItemRarity rarity, int playerLevel) {
            float rarityMultiplier = generationSettings.rarityEffectMultipliers[(int)rarity];
            float levelMultiplier = 1f + (playerLevel - 1) * generationSettings.effectPerLevelIncrease;

            // Scale effect values
            foreach (var effect in consumable.effects) {
                effect.value = Mathf.Round(effect.value * rarityMultiplier * levelMultiplier);

                // Update description to reflect new value
                effect.description = GetConsumableEffectDescription(effect.effectType, effect.value);
            }

            // Scale value
            consumable.buyPrice = Mathf.RoundToInt(consumable.buyPrice * rarityMultiplier * levelMultiplier);
            consumable.sellPrice = Mathf.RoundToInt(consumable.sellPrice * rarityMultiplier * levelMultiplier);
        }

        /// <summary>
        /// Adjust consumable properties based on rarity
        /// </summary>
        private void AdjustConsumableProperties(ConsumableItem consumable, ItemRarity rarity) {
            // Adjust duration based on rarity
            if (consumable.effectDuration > 0) {
                consumable.effectDuration *= (1f + (0.2f * (int)rarity));
            }

            // Adjust cooldown based on rarity (lower cooldown for higher rarity)
            if (consumable.cooldown > 0) {
                consumable.cooldown *= (1f - (0.1f * (int)rarity));
                consumable.cooldown = Mathf.Max(1f, consumable.cooldown); // Minimum cooldown of 1 second
            }
        }
    }
}