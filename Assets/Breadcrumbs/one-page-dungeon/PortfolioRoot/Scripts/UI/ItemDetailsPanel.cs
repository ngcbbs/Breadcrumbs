#if INCOMPLETE
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.UI {
    /// <summary>
    /// Manages the UI panel that displays detailed item information
    /// Handles item stats display, comparison, and action buttons
    /// </summary>
    public class ItemDetailsPanel : MonoBehaviour {
        [Header("Item Details")]
        [SerializeField]
        private TMP_Text itemNameText;
        [SerializeField]
        private TMP_Text itemTypeText;
        [SerializeField]
        private TMP_Text itemDescriptionText;
        [SerializeField]
        private TMP_Text itemStatsText;
        [SerializeField]
        private Image itemIconImage;

        [Header("Action Buttons")]
        [SerializeField]
        private Button useButton;
        [SerializeField]
        private Button equipButton;
        [SerializeField]
        private Button dropButton;

        [Header("Item Comparison")]
        [SerializeField]
        private GameObject comparisonPanel;
        [SerializeField]
        private TMP_Text currentItemStatsText;
        [SerializeField]
        private TMP_Text newItemStatsText;
        [SerializeField]
        private TMP_Text statDifferenceText;

        [Header("Rarity Colors")]
        [SerializeField]
        private Color[] rarityColors;

        // Current item being displayed
        private Item currentItem;

        private void Awake() {
            // Initialize default state
            gameObject.SetActive(false);

            if (comparisonPanel != null) {
                comparisonPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Show item details for the specified item
        /// </summary>
        public void ShowItemDetails(Item item, SlotType slotType) {
            if (item == null) {
                Hide();
                return;
            }

            currentItem = item;

            // Show the panel
            gameObject.SetActive(true);

            // Update item info
            UpdateItemInfo(item, slotType);
        }

        /// <summary>
        /// Update the detailed item information
        /// </summary>
        private void UpdateItemInfo(Item item, SlotType slotType) {
            // Item name with rarity color
            if (itemNameText != null) {
                string rarityColor = GetRarityColorHex(item.Rarity);
                itemNameText.text = $"<color={rarityColor}>{item.Name}</color>";
            }

            // Item type
            if (itemTypeText != null) {
                itemTypeText.text = GetItemTypeText(item);
            }

            // Item description
            if (itemDescriptionText != null) {
                itemDescriptionText.text = item.Description;
            }

            // Item icon
            if (itemIconImage != null && item.Icon != null) {
                itemIconImage.sprite = item.Icon;
                itemIconImage.enabled = true;
            } else if (itemIconImage != null) {
                itemIconImage.enabled = false;
            }

            // Item stats
            if (itemStatsText != null) {
                itemStatsText.text = GetItemStatsText(item);
            }
        }

        /// <summary>
        /// Show item comparison between two items
        /// </summary>
        public void ShowComparison(Item currentItem, Item newItem) {
            if (comparisonPanel == null || currentItem == null || newItem == null)
                return;

            comparisonPanel.SetActive(true);

            // Set comparison texts
            if (currentItemStatsText != null) {
                currentItemStatsText.text = GetItemStatsText(currentItem);
            }

            if (newItemStatsText != null) {
                newItemStatsText.text = GetItemStatsText(newItem);
            }

            // Determine if they're comparable items (both weapons or both armor)
            bool areComparable = false;

            if ((currentItem is WeaponItem && newItem is WeaponItem) ||
                (currentItem is EquippableItem && newItem is EquippableItem &&
                 !(currentItem is WeaponItem) && !(newItem is WeaponItem))) {
                areComparable = true;
            }

            // Show stat differences
            if (statDifferenceText != null && areComparable) {
                if (currentItem is WeaponItem currentWeapon && newItem is WeaponItem newWeapon) {
                    statDifferenceText.text = GetStatDifferenceText(currentWeapon, newWeapon);
                } else if (currentItem is EquippableItem currentEquipment && newItem is EquippableItem newEquipment) {
                    statDifferenceText.text = GetStatDifferenceText(currentEquipment, newEquipment);
                } else {
                    statDifferenceText.text = "Items cannot be directly compared";
                }
            }
        }

        /// <summary>
        /// Hide the comparison panel
        /// </summary>
        public void HideComparison() {
            if (comparisonPanel != null) {
                comparisonPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Hide the item details panel
        /// </summary>
        public void Hide() {
            gameObject.SetActive(false);
            currentItem = null;
        }

        /// <summary>
        /// Setup the use button visibility and callback
        /// </summary>
        public void SetupUseButton(bool show, Action onUseCallback) {
            if (useButton != null) {
                useButton.gameObject.SetActive(show);

                if (show) {
                    useButton.onClick.RemoveAllListeners();
                    useButton.onClick.AddListener(() => onUseCallback?.Invoke());
                }
            }
        }

        /// <summary>
        /// Setup the equip button visibility and callback
        /// </summary>
        public void SetupEquipButton(bool show, Action onEquipCallback) {
            if (equipButton != null) {
                equipButton.gameObject.SetActive(show);

                if (show) {
                    equipButton.onClick.RemoveAllListeners();
                    equipButton.onClick.AddListener(() => onEquipCallback?.Invoke());

                    // Set text to "Equip"
                    TMP_Text buttonText = equipButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null) {
                        buttonText.text = "Equip";
                    }
                }
            }
        }

        /// <summary>
        /// Setup the drop button visibility and callback
        /// </summary>
        public void SetupDropButton(bool show, Action onDropCallback) {
            if (dropButton != null) {
                dropButton.gameObject.SetActive(show);

                if (show) {
                    dropButton.onClick.RemoveAllListeners();
                    dropButton.onClick.AddListener(() => onDropCallback?.Invoke());
                }
            }
        }

        /// <summary>
        /// Hide all action buttons
        /// </summary>
        public void HideAllButtons() {
            if (useButton != null) useButton.gameObject.SetActive(false);
            if (equipButton != null) equipButton.gameObject.SetActive(false);
            if (dropButton != null) dropButton.gameObject.SetActive(false);
        }

        #region Helper Methods

        /// <summary>
        /// Get item type text based on the item
        /// </summary>
        private string GetItemTypeText(Item item) {
            if (item is WeaponItem weapon) {
                return $"Weapon - {weapon.WeaponType}";
            } else if (item is EquippableItem equippable) {
                return $"Armor - {equippable.EquipSlot}";
            } else if (item is ConsumableItem) {
                return "Consumable";
            }

            return "Item";
        }

        /// <summary>
        /// Get detailed stats text for an item
        /// </summary>
        private string GetItemStatsText(Item item) {
            string statsText = "";

            if (item is WeaponItem weapon) {
                statsText += $"Damage: {weapon.MinDamage}-{weapon.MaxDamage}\n";
                statsText += $"Attack Speed: {weapon.AttackSpeed:F1}\n";
                if (weapon.CritChance > 0)
                    statsText += $"Crit Chance: {weapon.CritChance:P0}\n";
                if (weapon.CritDamage > 1)
                    statsText += $"Crit Damage: {weapon.CritDamage:P0}\n";

                // Add weapon-specific stats
                if (weapon.Range > 0)
                    statsText += $"Range: {weapon.Range:F1}\n";

                // Add bonus stats
                if (weapon.StatsBonus.HasBonuses()) {
                    statsText += "\nBonus Stats:\n";
                    statsText += GetBonusStatsText(weapon.StatsBonus);
                }
            } else if (item is EquippableItem equippable) {
                // Base defensive stats
                statsText += $"Armor: {equippable.ArmorValue}\n";

                // Add bonus stats
                if (equippable.StatsBonus.HasBonuses()) {
                    statsText += "\nBonus Stats:\n";
                    statsText += GetBonusStatsText(equippable.StatsBonus);
                }
            } else if (item is ConsumableItem consumable) {
                // Consumable effects
                if (consumable.HealthRestore > 0)
                    statsText += $"Restores {consumable.HealthRestore} Health\n";
                if (consumable.ManaRestore > 0)
                    statsText += $"Restores {consumable.ManaRestore} Mana\n";
                if (consumable.StaminaRestore > 0)
                    statsText += $"Restores {consumable.StaminaRestore} Stamina\n";

                // Duration for buffs
                if (consumable.EffectDuration > 0)
                    statsText += $"Duration: {consumable.EffectDuration:F1}s\n";

                // Add temporary stat bonuses
                if (consumable.TemporaryBonus.HasBonuses()) {
                    statsText += "\nTemporary Bonuses:\n";
                    statsText += GetBonusStatsText(consumable.TemporaryBonus);
                }
            }

            // Common properties
            statsText += $"\nValue: {item.Value} Gold\n";
            statsText += $"Weight: {item.Weight:F1}\n";

            return statsText;
        }

        /// <summary>
        /// Get formatted text for stat bonuses
        /// </summary>
        private string GetBonusStatsText(StatBonus bonus) {
            string text = "";

            if (bonus.StrengthBonus != 0)
                text += $"Strength: {GetSignedValue(bonus.StrengthBonus)}\n";
            if (bonus.DexterityBonus != 0)
                text += $"Dexterity: {GetSignedValue(bonus.DexterityBonus)}\n";
            if (bonus.IntelligenceBonus != 0)
                text += $"Intelligence: {GetSignedValue(bonus.IntelligenceBonus)}\n";
            if (bonus.VitalityBonus != 0)
                text += $"Vitality: {GetSignedValue(bonus.VitalityBonus)}\n";

            if (bonus.MaxHealthBonus != 0)
                text += $"Max Health: {GetSignedValue(bonus.MaxHealthBonus)}\n";
            if (bonus.MaxManaBonus != 0)
                text += $"Max Mana: {GetSignedValue(bonus.MaxManaBonus)}\n";
            if (bonus.MaxStaminaBonus != 0)
                text += $"Max Stamina: {GetSignedValue(bonus.MaxStaminaBonus)}\n";

            if (bonus.PhysicalResistBonus != 0)
                text += $"Physical Resist: {GetSignedValue(bonus.PhysicalResistBonus, true)}\n";
            if (bonus.MagicResistBonus != 0)
                text += $"Magic Resist: {GetSignedValue(bonus.MagicResistBonus, true)}\n";

            return text;
        }

        /// <summary>
        /// Get stat difference text between two weapons
        /// </summary>
        private string GetStatDifferenceText(WeaponItem currentWeapon, WeaponItem newWeapon) {
            string text = "";

            // Calculate average damage
            float currentAvgDamage = (currentWeapon.MinDamage + currentWeapon.MaxDamage) / 2f;
            float newAvgDamage = (newWeapon.MinDamage + newWeapon.MaxDamage) / 2f;
            float damageDiff = newAvgDamage - currentAvgDamage;

            text += $"Damage: <color={(damageDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(damageDiff)}</color>\n";

            // Attack speed
            float speedDiff = newWeapon.AttackSpeed - currentWeapon.AttackSpeed;
            text += $"Attack Speed: <color={(speedDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(speedDiff)}</color>\n";

            // Crit stats
            float critChanceDiff = newWeapon.CritChance - currentWeapon.CritChance;
            if (currentWeapon.CritChance > 0 || newWeapon.CritChance > 0) {
                text +=
                    $"Crit Chance: <color={(critChanceDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(critChanceDiff, true)}</color>\n";
            }

            float critDamageDiff = newWeapon.CritDamage - currentWeapon.CritDamage;
            if (currentWeapon.CritDamage > 1 || newWeapon.CritDamage > 1) {
                text +=
                    $"Crit Damage: <color={(critDamageDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(critDamageDiff, true)}</color>\n";
            }

            // Compare bonus stats
            text += "\nStat Changes:\n";
            text += GetBonusStatDifference(currentWeapon.StatsBonus, newWeapon.StatsBonus);

            return text;
        }

        /// <summary>
        /// Get stat difference text between two armor pieces
        /// </summary>
        private string GetStatDifferenceText(EquippableItem currentItem, EquippableItem newItem) {
            // Skip if one is a weapon (handled by the weapon comparison method)
            if (currentItem is WeaponItem || newItem is WeaponItem)
                return "";

            string text = "";

            // Armor value
            float armorDiff = newItem.ArmorValue - currentItem.ArmorValue;
            text += $"Armor: <color={(armorDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(armorDiff)}</color>\n";

            // Compare bonus stats
            text += "\nStat Changes:\n";
            text += GetBonusStatDifference(currentItem.StatsBonus, newItem.StatsBonus);

            return text;
        }

        /// <summary>
        /// Get formatted text for bonus stat differences
        /// </summary>
        private string GetBonusStatDifference(StatBonus current, StatBonus newBonus) {
            string text = "";

            // Compare primary stats
            float strDiff = newBonus.StrengthBonus - current.StrengthBonus;
            if (current.StrengthBonus != 0 || newBonus.StrengthBonus != 0)
                text += $"Strength: <color={(strDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(strDiff)}</color>\n";

            float dexDiff = newBonus.DexterityBonus - current.DexterityBonus;
            if (current.DexterityBonus != 0 || newBonus.DexterityBonus != 0)
                text += $"Dexterity: <color={(dexDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(dexDiff)}</color>\n";

            float intDiff = newBonus.IntelligenceBonus - current.IntelligenceBonus;
            if (current.IntelligenceBonus != 0 || newBonus.IntelligenceBonus != 0)
                text += $"Intelligence: <color={(intDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(intDiff)}</color>\n";

            float vitDiff = newBonus.VitalityBonus - current.VitalityBonus;
            if (current.VitalityBonus != 0 || newBonus.VitalityBonus != 0)
                text += $"Vitality: <color={(vitDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(vitDiff)}</color>\n";

            // Compare secondary stats
            float healthDiff = newBonus.MaxHealthBonus - current.MaxHealthBonus;
            if (current.MaxHealthBonus != 0 || newBonus.MaxHealthBonus != 0)
                text += $"Max Health: <color={(healthDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(healthDiff)}</color>\n";

            float manaDiff = newBonus.MaxManaBonus - current.MaxManaBonus;
            if (current.MaxManaBonus != 0 || newBonus.MaxManaBonus != 0)
                text += $"Max Mana: <color={(manaDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(manaDiff)}</color>\n";

            float staminaDiff = newBonus.MaxStaminaBonus - current.MaxStaminaBonus;
            if (current.MaxStaminaBonus != 0 || newBonus.MaxStaminaBonus != 0)
                text +=
                    $"Max Stamina: <color={(staminaDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(staminaDiff)}</color>\n";

            // Compare resistances
            float physResDiff = newBonus.PhysicalResistBonus - current.PhysicalResistBonus;
            if (current.PhysicalResistBonus != 0 || newBonus.PhysicalResistBonus != 0)
                text +=
                    $"Physical Resist: <color={(physResDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(physResDiff, true)}</color>\n";

            float magResDiff = newBonus.MagicResistBonus - current.MagicResistBonus;
            if (current.MagicResistBonus != 0 || newBonus.MagicResistBonus != 0)
                text +=
                    $"Magic Resist: <color={(magResDiff >= 0 ? "#00FF00" : "#FF0000")}>{GetSignedValue(magResDiff, true)}</color>\n";

            return text;
        }

        /// <summary>
        /// Get formatted signed value (e.g., +5, -3)
        /// </summary>
        private string GetSignedValue(float value, bool percentage = false) {
            string format = percentage ? "+0%;-0%" : "+0;-0";
            return value.ToString(format);
        }

        /// <summary>
        /// Get the hex color code for item rarity
        /// </summary>
        private string GetRarityColorHex(ItemRarity rarity) {
            if (rarityColors != null && (int)rarity < rarityColors.Length) {
                Color color = rarityColors[(int)rarity];
                return "#" + ColorUtility.ToHtmlStringRGB(color);
            }

            // Default colors if array not set up
            switch (rarity) {
                case ItemRarity.Common:
                    return "#FFFFFF"; // White
                case ItemRarity.Uncommon:
                    return "#00FF00"; // Green
                case ItemRarity.Rare:
                    return "#0080FF"; // Blue
                case ItemRarity.Epic:
                    return "#8000FF"; // Purple
                case ItemRarity.Legendary:
                    return "#FF8000"; // Orange
                default:
                    return "#FFFFFF";
            }
        }

        #endregion
    }
}
#endif