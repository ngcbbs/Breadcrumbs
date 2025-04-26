using UnityEngine;
using System.Collections.Generic;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Partial class for EquipmentSystemExtension - Dual Wielding and Enchantment systems
    /// </summary>
    public partial class EquipmentSystemExtension : MonoBehaviour
    {
        #region Dual Wielding
        
        /// <summary>
        /// Check if dual wielding is possible with current equipment
        /// </summary>
        public bool CanDualWield()
        {
            if (!enableDualWielding)
                return false;
                
            // Get main and off-hand weapons
            WeaponItem mainHandWeapon = equipmentManager.GetEquippedItem(EquipmentSlot.MainHand) as WeaponItem;
            WeaponItem offHandWeapon = equipmentManager.GetEquippedItem(EquipmentSlot.OffHand) as WeaponItem;
            
            // Check if both slots have weapons
            if (mainHandWeapon == null || offHandWeapon == null)
                return false;
                
            // Check if both weapons are dual-wieldable
            bool mainHandValid = dualWieldableWeapons.Contains(mainHandWeapon.weaponType);
            bool offHandValid = dualWieldableWeapons.Contains(offHandWeapon.weaponType);
            
            return mainHandValid && offHandValid;
        }
        
        /// <summary>
        /// Apply dual wielding bonuses
        /// </summary>
        public void ApplyDualWieldBonuses()
        {
            if (!CanDualWield())
                return;
                
            // Get combat system
            PlayerCombat combat = GetComponent<PlayerCombat>();
            if (combat == null)
                return;
                
            // Apply dual wield modifiers
            /*
            combat.SetDamageMultiplier(dualWieldDamageModifier);
            combat.SetAttackSpeedMultiplier(dualWieldAttackSpeedModifier);
            // */
            
            Debug.Log("Applied dual wielding bonuses");
        }
        
        /// <summary>
        /// Remove dual wielding bonuses
        /// </summary>
        public void RemoveDualWieldBonuses()
        {
            // Get combat system
            PlayerCombat combat = GetComponent<PlayerCombat>();
            if (combat == null)
                return;
                
            // Reset modifiers
            /*
            combat.SetDamageMultiplier(1f);
            combat.SetAttackSpeedMultiplier(1f);
            // */
            
            Debug.Log("Removed dual wielding bonuses");
        }
        
        /// <summary>
        /// Add a weapon type to the dual-wieldable list
        /// </summary>
        public void AddDualWieldableWeaponType(WeaponType weaponType)
        {
            if (!dualWieldableWeapons.Contains(weaponType))
            {
                dualWieldableWeapons.Add(weaponType);
                
                // Check if this enables dual wielding for current equipment
                if (CanDualWield())
                {
                    ApplyDualWieldBonuses();
                }
            }
        }
        
        /// <summary>
        /// Remove a weapon type from the dual-wieldable list
        /// </summary>
        public void RemoveDualWieldableWeaponType(WeaponType weaponType)
        {
            if (dualWieldableWeapons.Contains(weaponType))
            {
                dualWieldableWeapons.Remove(weaponType);
                
                // Check if this disables dual wielding for current equipment
                if (!CanDualWield())
                {
                    RemoveDualWieldBonuses();
                }
            }
        }
        
        /// <summary>
        /// Set the damage modifier for dual wielding
        /// </summary>
        public void SetDualWieldDamageModifier(float modifier)
        {
            if (modifier < 0f)
                modifier = 0f;
                
            dualWieldDamageModifier = modifier;
            
            // Update if currently dual wielding
            if (CanDualWield())
            {
                ApplyDualWieldBonuses();
            }
        }
        
        /// <summary>
        /// Set the attack speed modifier for dual wielding
        /// </summary>
        public void SetDualWieldAttackSpeedModifier(float modifier)
        {
            if (modifier < 0f)
                modifier = 0f;
                
            dualWieldAttackSpeedModifier = modifier;
            
            // Update if currently dual wielding
            if (CanDualWield())
            {
                ApplyDualWieldBonuses();
            }
        }
        
        #endregion
        
        #region Enchantments
        
        /// <summary>
        /// Add an enchantment to an item
        /// </summary>
        public bool EnchantItem(EquippableItem item, ItemEnchantment enchantment)
        {
            if (!enableEnchantments || item == null || enchantment == null)
                return false;
                
            // Initialize enchantment list if needed
            if (!itemEnchantments.ContainsKey(item))
            {
                itemEnchantments[item] = new List<ItemEnchantment>();
            }
            
            // Check if item already has max enchantments
            if (itemEnchantments[item].Count >= maxEnchantmentsPerItem)
            {
                Debug.LogWarning($"Item already has maximum enchantments ({maxEnchantmentsPerItem})");
                return false;
            }
            
            // Check if item already has this enchantment
            if (itemEnchantments[item].Exists(e => e.enchantmentType == enchantment.enchantmentType))
            {
                Debug.LogWarning("Item already has this type of enchantment");
                return false;
            }
            
            // Create a copy of the enchantment
            ItemEnchantment enchantmentCopy = new ItemEnchantment(enchantment);
            
            // Add enchantment
            itemEnchantments[item].Add(enchantmentCopy);
            
            // Apply enchantment effect if item is equipped
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (equipmentManager.GetEquippedItem(slot) == item)
                {
                    ApplyEnchantmentEffect(item, enchantmentCopy);
                    break;
                }
            }
            
            // Notify
            OnItemEnchanted?.Invoke(item, enchantmentCopy);
            
            return true;
        }
        
        /// <summary>
        /// Remove an enchantment from an item
        /// </summary>
        public bool RemoveEnchantment(EquippableItem item, EnchantmentType enchantmentType)
        {
            if (!enableEnchantments || item == null)
                return false;
                
            // Check if item has enchantments
            if (!itemEnchantments.ContainsKey(item) || itemEnchantments[item].Count == 0)
            {
                return false;
            }
            
            // Find the enchantment
            ItemEnchantment enchantment = itemEnchantments[item].Find(e => e.enchantmentType == enchantmentType);
            if (enchantment == null)
            {
                return false;
            }
            
            // Remove enchantment
            itemEnchantments[item].Remove(enchantment);
            
            // Remove enchantment effect if item is equipped
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (equipmentManager.GetEquippedItem(slot) == item)
                {
                    RemoveEnchantmentEffect(item, enchantment);
                    break;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get all enchantments on an item
        /// </summary>
        public List<ItemEnchantment> GetItemEnchantments(EquippableItem item)
        {
            if (!enableEnchantments || item == null || !itemEnchantments.ContainsKey(item))
            {
                return new List<ItemEnchantment>();
            }
            
            return new List<ItemEnchantment>(itemEnchantments[item]);
        }
        
        /// <summary>
        /// Apply enchantment effect to character
        /// </summary>
        private void ApplyEnchantmentEffect(EquippableItem item, ItemEnchantment enchantment)
        {
            CharacterStats stats = GetComponent<CharacterStats>();
            if (stats == null)
                return;
                
            // Apply stat modifier
            if (enchantment.statModifier != null)
            {
                stats.AddStatModifier(
                    enchantment.statModifier.statType,
                    enchantment.statModifier.modifierType,
                    enchantment.statModifier.value
                );
            }
            
            // Apply resistance modifier
            if (enchantment.resistanceModifier != null)
            {
                stats.AddResistanceModifier(
                    enchantment.resistanceModifier.damageType,
                    enchantment.resistanceModifier.value
                );
            }
            
            // Add visual effect
            if (enchantment.visualEffect != null)
            {
                GameObject effect = Instantiate(enchantment.visualEffect, transform.position, transform.rotation, transform);
                enchantment.instancedEffect = effect;
            }
            
            Debug.Log($"Applied enchantment {enchantment.name} to {item.itemName}");
        }
        
        /// <summary>
        /// Remove enchantment effect from character
        /// </summary>
        private void RemoveEnchantmentEffect(EquippableItem item, ItemEnchantment enchantment)
        {
            CharacterStats stats = GetComponent<CharacterStats>();
            if (stats == null)
                return;
                
            // Remove stat modifier
            if (enchantment.statModifier != null)
            {
                stats.RemoveStatModifier(
                    enchantment.statModifier.statType,
                    enchantment.statModifier.modifierType,
                    enchantment.statModifier.value
                );
            }
            
            // Remove resistance modifier
            if (enchantment.resistanceModifier != null)
            {
                stats.RemoveResistanceModifier(
                    enchantment.resistanceModifier.damageType,
                    enchantment.resistanceModifier.value
                );
            }
            
            // Remove visual effect
            if (enchantment.instancedEffect != null)
            {
                Destroy(enchantment.instancedEffect);
                enchantment.instancedEffect = null;
            }
            
            Debug.Log($"Removed enchantment {enchantment.name} from {item.itemName}");
        }
        
        /// <summary>
        /// Get a random available enchantment
        /// </summary>
        public ItemEnchantment GetRandomEnchantment()
        {
            if (!enableEnchantments || availableEnchantments.Count == 0)
                return null;
                
            int randomIndex = Random.Range(0, availableEnchantments.Count);
            return availableEnchantments[randomIndex];
        }
        
        /// <summary>
        /// Get enchantments of a specific type
        /// </summary>
        public List<ItemEnchantment> GetEnchantmentsByType(EnchantmentType type)
        {
            if (!enableEnchantments)
                return new List<ItemEnchantment>();
                
            return availableEnchantments.FindAll(e => e.enchantmentType == type);
        }
        
        /// <summary>
        /// Add a new enchantment to the available enchantments
        /// </summary>
        public void AddAvailableEnchantment(ItemEnchantment enchantment)
        {
            if (enchantment == null)
                return;
                
            // Check for duplicates
            if (!availableEnchantments.Exists(e => e.enchantmentType == enchantment.enchantmentType && e.tier == enchantment.tier))
            {
                availableEnchantments.Add(enchantment);
            }
        }
        
        /// <summary>
        /// Remove an enchantment from the available enchantments
        /// </summary>
        public bool RemoveAvailableEnchantment(EnchantmentType type, int tier)
        {
            ItemEnchantment enchant = availableEnchantments.Find(e => e.enchantmentType == type && e.tier == tier);
            
            if (enchant != null)
            {
                availableEnchantments.Remove(enchant);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Enable or disable the enchantment system
        /// </summary>
        public void SetEnchantmentSystemEnabled(bool enabled)
        {
            // If disabling, remove all active enchantment effects
            if (!enabled && enableEnchantments)
            {
                // Get all equipped items
                foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
                {
                    EquippableItem equippedItem = equipmentManager.GetEquippedItem(slot);
                    
                    if (equippedItem != null && itemEnchantments.ContainsKey(equippedItem))
                    {
                        foreach (var enchantment in itemEnchantments[equippedItem])
                        {
                            RemoveEnchantmentEffect(equippedItem, enchantment);
                        }
                    }
                }
            }
            // If enabling, apply all enchantment effects to equipped items
            else if (enabled && !enableEnchantments)
            {
                // Get all equipped items
                foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
                {
                    EquippableItem equippedItem = equipmentManager.GetEquippedItem(slot);
                    
                    if (equippedItem != null && itemEnchantments.ContainsKey(equippedItem))
                    {
                        foreach (var enchantment in itemEnchantments[equippedItem])
                        {
                            ApplyEnchantmentEffect(equippedItem, enchantment);
                        }
                    }
                }
            }
            
            enableEnchantments = enabled;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Types of enchantments
    /// </summary>
    public enum EnchantmentType
    {
        Damage,
        Defense,
        Speed,
        Life,
        Mana,
        Fire,
        Ice,
        Lightning,
        Poison,
        Holy,
        Void,
        Vampiric,
        Lucky,
        Thorns
    }
    
    /// <summary>
    /// Represents an enchantment that can be applied to an item
    /// </summary>
    [System.Serializable]
    public class ItemEnchantment
    {
        public string name;
        [TextArea(2, 4)]
        public string description;
        public EnchantmentType enchantmentType;
        public int tier = 1; // 1-5 typically
        public StatModifier statModifier;
        public ResistanceModifier resistanceModifier;
        public GameObject visualEffect;
        
        [HideInInspector]
        public GameObject instancedEffect;
        
        // Constructor for copying
        public ItemEnchantment(ItemEnchantment other)
        {
            if (other == null)
                return;
                
            this.name = other.name;
            this.description = other.description;
            this.enchantmentType = other.enchantmentType;
            this.tier = other.tier;
            this.visualEffect = other.visualEffect;
            
            // Deep copy modifiers
            if (other.statModifier != null)
            {
                this.statModifier = new StatModifier
                {
                    statType = other.statModifier.statType,
                    modifierType = other.statModifier.modifierType,
                    value = other.statModifier.value
                };
            }
            
            if (other.resistanceModifier != null)
            {
                this.resistanceModifier = new ResistanceModifier
                {
                    damageType = other.resistanceModifier.damageType,
                    value = other.resistanceModifier.value
                };
            }
        }
        
        // Default constructor
        public ItemEnchantment() { }
    }
}