using UnityEngine;
using System.Collections.Generic;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Partial class for EquipmentSystemExtension - Transmogrification functionality
    /// </summary>
    public partial class EquipmentSystemExtension : MonoBehaviour
    {
        /// <summary>
        /// Transmogrify (change appearance of) an equipped item
        /// </summary>
        public bool TransmogrifyItem(EquipmentSlot slot, EquippableItem appearanceItem)
        {
            if (!enableTransmogSystem)
                return false;
                
            EquippableItem equippedItem = equipmentManager.GetEquippedItem(slot);
            
            // Check if item is equipped in this slot
            if (equippedItem == null)
            {
                Debug.LogWarning("No item equipped in this slot to transmogrify");
                return false;
            }
            
            // Check if appearance item is valid for this slot
            if (appearanceItem != null && appearanceItem.equipSlot != slot)
            {
                Debug.LogWarning("Appearance item isn't compatible with this equipment slot");
                return false;
            }
            
            // Store the appearance item
            transmogrifiedItems[slot] = appearanceItem;
            
            // Update visual
            UpdateEquipmentVisual(slot);
            
            // Notify
            OnItemTransmogrified?.Invoke(equippedItem, appearanceItem);
            
            return true;
        }
        
        /// <summary>
        /// Remove transmogrification from an item
        /// </summary>
        public bool RemoveTransmogrification(EquipmentSlot slot)
        {
            if (!enableTransmogSystem || !transmogrifiedItems.ContainsKey(slot) || transmogrifiedItems[slot] == null)
                return false;
                
            EquippableItem equippedItem = equipmentManager.GetEquippedItem(slot);
            
            // Remember the old appearance
            EquippableItem oldAppearance = transmogrifiedItems[slot];
            
            // Clear the appearance
            transmogrifiedItems[slot] = null;
            
            // Update visual
            UpdateEquipmentVisual(slot);
            
            // Notify
            OnItemTransmogrified?.Invoke(equippedItem, null);
            
            return true;
        }
        
        /// <summary>
        /// Update the visual appearance of an equipment slot
        /// </summary>
        private void UpdateEquipmentVisual(EquipmentSlot slot)
        {
            EquippableItem equippedItem = equipmentManager.GetEquippedItem(slot);
            
            if (equippedItem == null)
                return;
                
            // Get the appearance item (or use the actual item if no transmog)
            EquippableItem appearanceItem = transmogrifiedItems[slot] ?? equippedItem;
            
            // Update equipment model
            if (appearanceItem.equipmentModel != null)
            {
                // Clear existing model
                equipmentManager.ClearEquipmentModel(slot);
                
                // Get appropriate attachment point
                Transform attachPoint = equipmentManager.GetEquipmentParent(slot);
                
                // Create new model
                GameObject model = Instantiate(appearanceItem.equipmentModel, attachPoint);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                
                // Set as current model
                equipmentManager.SetEquipmentModel(slot, model);
            }
        }
        
        /// <summary>
        /// Remove all transmogrifications
        /// </summary>
        public void ClearAllTransmogrifications()
        {
            if (!enableTransmogSystem)
                return;
                
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (transmogrifiedItems.ContainsKey(slot) && transmogrifiedItems[slot] != null)
                {
                    RemoveTransmogrification(slot);
                }
            }
        }
        
        /// <summary>
        /// Get the transmogrified appearance for a slot
        /// </summary>
        public EquippableItem GetTransmogrifiedAppearance(EquipmentSlot slot)
        {
            if (!enableTransmogSystem || !transmogrifiedItems.ContainsKey(slot))
                return null;
                
            return transmogrifiedItems[slot];
        }
        
        /// <summary>
        /// Enable or disable the transmogrification system
        /// </summary>
        public void SetTransmogSystemEnabled(bool enabled)
        {
            enableTransmogSystem = enabled;
            
            if (!enabled)
            {
                // Remove all transmogrifications if system is disabled
                ClearAllTransmogrifications();
            }
            else
            {
                // Refresh appearances if system is enabled
                foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
                {
                    if (equipmentManager.GetEquippedItem(slot) != null)
                    {
                        UpdateEquipmentVisual(slot);
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if a slot has transmogrification applied
        /// </summary>
        public bool HasTransmogrification(EquipmentSlot slot)
        {
            return enableTransmogSystem && 
                   transmogrifiedItems.ContainsKey(slot) && 
                   transmogrifiedItems[slot] != null;
        }
        
        /// <summary>
        /// Copy transmogrification settings to another equipment manager
        /// </summary>
        public void CopyTransmogrificationSettings(EquipmentSystemExtension targetExtension)
        {
            if (targetExtension == null || !enableTransmogSystem || !targetExtension.enableTransmogSystem)
                return;
                
            foreach (var kvp in transmogrifiedItems)
            {
                if (kvp.Value != null)
                {
                    targetExtension.TransmogrifyItem(kvp.Key, kvp.Value);
                }
            }
        }
    }
}