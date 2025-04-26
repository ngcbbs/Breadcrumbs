using UnityEngine;
using System.Collections.Generic;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Partial class for EquipmentSystemExtension - Equipment Sets functionality
    /// </summary>
    public partial class EquipmentSystemExtension : MonoBehaviour
    {
        /// <summary>
        /// Check if all equipment sets are complete or broken
        /// </summary>
        private void CheckAllEquipmentSets()
        {
            foreach (var equipmentSet in equipmentSets)
            {
                CheckEquipmentSet(equipmentSet);
            }
        }
        
        /// <summary>
        /// Check if a specific equipment set is complete
        /// </summary>
        private void CheckEquipmentSet(EquipmentSet equipmentSet)
        {
            bool wasActive = activeEquipmentSets.ContainsKey(equipmentSet.setId) && activeEquipmentSets[equipmentSet.setId];
            bool isActive = IsEquipmentSetComplete(equipmentSet);
            
            // Update state
            activeEquipmentSets[equipmentSet.setId] = isActive;
            
            // Trigger events if state changed
            if (isActive && !wasActive)
            {
                // Set is now complete
                ApplyEquipmentSetBonuses(equipmentSet);
                OnEquipmentSetCompleted?.Invoke(equipmentSet);
            }
            else if (!isActive && wasActive)
            {
                // Set is now broken
                RemoveEquipmentSetBonuses(equipmentSet);
                OnEquipmentSetBroken?.Invoke(equipmentSet);
            }
        }
        
        /// <summary>
        /// Check if all items in a set are equipped
        /// </summary>
        private bool IsEquipmentSetComplete(EquipmentSet equipmentSet)
        {
            foreach (var requiredItem in equipmentSet.requiredItems)
            {
                // Check if the item is equipped in the correct slot
                EquippableItem equippedItem = equipmentManager.GetEquippedItem(requiredItem.slot);
                
                if (equippedItem == null || equippedItem.itemID != requiredItem.itemID)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Apply bonuses from an equipment set
        /// </summary>
        private void ApplyEquipmentSetBonuses(EquipmentSet equipmentSet)
        {
            CharacterStats characterStats = GetComponent<CharacterStats>();
            if (characterStats == null)
                return;
                
            // Apply stat bonuses
            foreach (var statBonus in equipmentSet.statBonuses)
            {
                characterStats.AddStatModifier(statBonus.statType, statBonus.modifierType, statBonus.value);
            }
            
            // Apply resistance bonuses
            foreach (var resistanceBonus in equipmentSet.resistanceBonuses)
            {
                characterStats.AddResistanceModifier(resistanceBonus.damageType, resistanceBonus.value);
            }
            
            // Apply set effect
            if (equipmentSet.setEffect != null && equipmentSet.setEffect.prefab != null)
            {
                GameObject effectObj = Instantiate(equipmentSet.setEffect.prefab, transform.position, transform.rotation, transform);
                equipmentSet.setEffect.instancedObject = effectObj;
            }
            
            Debug.Log($"Applied equipment set bonuses for {equipmentSet.setName}");
        }
        
        /// <summary>
        /// Remove bonuses from an equipment set
        /// </summary>
        private void RemoveEquipmentSetBonuses(EquipmentSet equipmentSet)
        {
            CharacterStats characterStats = GetComponent<CharacterStats>();
            if (characterStats == null)
                return;
                
            // Remove stat bonuses
            foreach (var statBonus in equipmentSet.statBonuses)
            {
                characterStats.RemoveStatModifier(statBonus.statType, statBonus.modifierType, statBonus.value);
            }
            
            // Remove resistance bonuses
            foreach (var resistanceBonus in equipmentSet.resistanceBonuses)
            {
                characterStats.RemoveResistanceModifier(resistanceBonus.damageType, resistanceBonus.value);
            }
            
            // Remove set effect
            if (equipmentSet.setEffect != null && equipmentSet.setEffect.instancedObject != null)
            {
                Destroy(equipmentSet.setEffect.instancedObject);
                equipmentSet.setEffect.instancedObject = null;
            }
            
            Debug.Log($"Removed equipment set bonuses for {equipmentSet.setName}");
        }
        
        /// <summary>
        /// Get a list of all equipment sets
        /// </summary>
        public List<EquipmentSet> GetAllEquipmentSets()
        {
            return new List<EquipmentSet>(equipmentSets);
        }
        
        /// <summary>
        /// Get a list of all complete equipment sets
        /// </summary>
        public List<EquipmentSet> GetCompleteEquipmentSets()
        {
            List<EquipmentSet> completeSets = new List<EquipmentSet>();
            
            foreach (var equipmentSet in equipmentSets)
            {
                if (activeEquipmentSets.ContainsKey(equipmentSet.setId) && activeEquipmentSets[equipmentSet.setId])
                {
                    completeSets.Add(equipmentSet);
                }
            }
            
            return completeSets;
        }
        
        /// <summary>
        /// Get completion status of a specific set
        /// </summary>
        public (bool isComplete, int equippedItems, int totalItems) GetSetCompletionStatus(string setId)
        {
            // Find the set
            EquipmentSet targetSet = equipmentSets.Find(set => set.setId == setId);
            
            if (targetSet == null)
                return (false, 0, 0);
                
            int totalItems = targetSet.requiredItems.Count;
            int equippedItems = 0;
            
            // Count equipped items
            foreach (var requiredItem in targetSet.requiredItems)
            {
                EquippableItem equippedItem = equipmentManager.GetEquippedItem(requiredItem.slot);
                
                if (equippedItem != null && equippedItem.itemID == requiredItem.itemID)
                {
                    equippedItems++;
                }
            }
            
            bool isComplete = equippedItems == totalItems;
            return (isComplete, equippedItems, totalItems);
        }
        
        /// <summary>
        /// Add a new equipment set to the system
        /// </summary>
        public void AddEquipmentSet(EquipmentSet newSet)
        {
            // Avoid duplicates
            if (equipmentSets.Exists(set => set.setId == newSet.setId))
            {
                Debug.LogWarning($"Equipment set with ID {newSet.setId} already exists");
                return;
            }
            
            equipmentSets.Add(newSet);
            
            // Check if it's complete
            CheckEquipmentSet(newSet);
        }
        
        /// <summary>
        /// Remove an equipment set from the system
        /// </summary>
        public bool RemoveEquipmentSet(string setId)
        {
            // Find the set
            EquipmentSet targetSet = equipmentSets.Find(set => set.setId == setId);
            
            if (targetSet == null)
                return false;
                
            // Check if it's active and remove bonuses if needed
            if (activeEquipmentSets.ContainsKey(setId) && activeEquipmentSets[setId])
            {
                RemoveEquipmentSetBonuses(targetSet);
            }
            
            // Remove from collections
            equipmentSets.Remove(targetSet);
            activeEquipmentSets.Remove(setId);
            
            return true;
        }
    }
    
    /// <summary>
    /// Represents a set of equipment items with bonuses when all are equipped
    /// </summary>
    [System.Serializable]
    public class EquipmentSet
    {
        public string setId;
        public string setName;
        [TextArea(2, 4)]
        public string setDescription;
        public List<SetRequiredItem> requiredItems = new List<SetRequiredItem>();
        public List<StatModifier> statBonuses = new List<StatModifier>();
        public List<ResistanceModifier> resistanceBonuses = new List<ResistanceModifier>();
        public SetVisualEffect setEffect;
    }
    
    /// <summary>
    /// Represents a required item in an equipment set
    /// </summary>
    [System.Serializable]
    public class SetRequiredItem
    {
        public string itemID;
        public EquipmentSlot slot;
        public string itemName; // For editor reference only
    }
    
    /// <summary>
    /// Visual effect for an equipment set
    /// </summary>
    [System.Serializable]
    public class SetVisualEffect
    {
        public GameObject prefab;
        [HideInInspector]
        public GameObject instancedObject;
    }
}