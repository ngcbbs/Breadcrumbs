using System.Collections.Generic;
using GamePortfolio.Gameplay.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Manages the equipment slots for a character
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Equipment Settings")]
        [SerializeField] private Transform weaponParent;
        [SerializeField] private Transform shieldParent;
        [SerializeField] private Transform headParent;
        [SerializeField] private Transform bodyParent;
        
        [Header("Visual Settings")]
        [SerializeField] private bool showEquippedWeapons = true;
        [SerializeField] private bool showEquippedArmor = true;
        
        [Header("Equipment Effects")]
        [SerializeField] private GameObject equipVFX;
        [SerializeField] private AudioClip equipSound;
        [SerializeField] private GameObject unequipVFX;
        [SerializeField] private AudioClip unequipSound;
        
        // Events
        [Header("Events")]
        public UnityEvent<EquippableItem, EquipmentSlot> OnItemEquipped;
        public UnityEvent<EquippableItem, EquipmentSlot> OnItemUnequipped;
        
        // Current equipment
        private Dictionary<EquipmentSlot, EquippableItem> equippedItems = new Dictionary<EquipmentSlot, EquippableItem>();
        private Dictionary<EquipmentSlot, GameObject> equipmentModels = new Dictionary<EquipmentSlot, GameObject>();
        
        // Components
        private InventoryManager inventory;
        private AudioSource audioSource;
        
        private void Awake()
        {
            inventory = GetComponent<InventoryManager>();
            audioSource = GetComponent<AudioSource>();
            
            // Initialize all equipment slots
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems[slot] = null;
                equipmentModels[slot] = null;
            }
        }
        
        /// <summary>
        /// Equip an item to the appropriate slot
        /// </summary>
        public bool EquipItem(EquippableItem item)
        {
            if (item == null)
                return false;
                
            // Check if player meets the requirements
            if (!MeetsRequirements(item))
            {
                Debug.Log($"Cannot equip {item.itemName} - requirements not met");
                return false;
            }
            
            // Unequip any item in that slot
            EquippableItem oldItem = equippedItems[item.equipSlot];
            if (oldItem != null)
            {
                UnequipItem(item.equipSlot);
            }
            
            // Equip the new item
            equippedItems[item.equipSlot] = item;
            
            // Apply item effects
            item.OnEquip(gameObject);
            
            // Play effects
            PlayEquipEffects();
            
            // Trigger event
            OnItemEquipped?.Invoke(item, item.equipSlot);
            
            return true;
        }
        
        /// <summary>
        /// Unequip an item from a specific slot
        /// </summary>
        public EquippableItem UnequipItem(EquipmentSlot slot)
        {
            // Get the current item
            EquippableItem item = equippedItems[slot];
            
            if (item != null)
            {
                // Remove effects
                item.OnUnequip(gameObject);
                
                // Clear slot
                equippedItems[slot] = null;
                
                // Destroy visual model if present
                if (equipmentModels.ContainsKey(slot) && equipmentModels[slot] != null)
                {
                    Destroy(equipmentModels[slot]);
                    equipmentModels[slot] = null;
                }
                
                // Play effects
                PlayUnequipEffects();
                
                // Trigger event
                OnItemUnequipped?.Invoke(item, slot);
                
                // Return the unequipped item
                return item;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get the item equipped in a specific slot
        /// </summary>
        public EquippableItem GetEquippedItem(EquipmentSlot slot)
        {
            if (equippedItems.ContainsKey(slot))
            {
                return equippedItems[slot];
            }
            
            return null;
        }
        
        /// <summary>
        /// Get all equipped items
        /// </summary>
        public Dictionary<EquipmentSlot, EquippableItem> GetAllEquippedItems()
        {
            return new Dictionary<EquipmentSlot, EquippableItem>(equippedItems);
        }
        
        /// <summary>
        /// Set the visual model for an equipment slot
        /// </summary>
        public void SetEquipmentModel(EquipmentSlot slot, GameObject model)
        {
            if (model == null)
                return;
                
            // Clear any existing model
            if (equipmentModels.ContainsKey(slot) && equipmentModels[slot] != null)
            {
                Destroy(equipmentModels[slot]);
            }
            
            // Store new model
            equipmentModels[slot] = model;
            
            // Handle visibility based on settings
            bool shouldShow = true;
            
            if (slot == EquipmentSlot.MainHand || slot == EquipmentSlot.OffHand)
            {
                shouldShow = showEquippedWeapons;
            }
            else if (slot == EquipmentSlot.Head || slot == EquipmentSlot.Body || 
                    slot == EquipmentSlot.Hands || slot == EquipmentSlot.Legs || 
                    slot == EquipmentSlot.Feet)
            {
                shouldShow = showEquippedArmor;
            }
            
            if (model != null)
            {
                model.SetActive(shouldShow);
            }
        }
        
        /// <summary>
        /// Clear the visual model for an equipment slot
        /// </summary>
        public void ClearEquipmentModel(EquipmentSlot slot)
        {
            if (equipmentModels.ContainsKey(slot) && equipmentModels[slot] != null)
            {
                Destroy(equipmentModels[slot]);
                equipmentModels[slot] = null;
            }
        }
        
        /// <summary>
        /// Toggle visibility of equipped weapons
        /// </summary>
        public void ToggleWeaponVisibility(bool visible)
        {
            showEquippedWeapons = visible;
            
            // Update visibility of weapon models
            foreach (EquipmentSlot slot in new[] { EquipmentSlot.MainHand, EquipmentSlot.OffHand })
            {
                if (equipmentModels.ContainsKey(slot) && equipmentModels[slot] != null)
                {
                    equipmentModels[slot].SetActive(visible);
                }
            }
        }
        
        /// <summary>
        /// Toggle visibility of equipped armor
        /// </summary>
        public void ToggleArmorVisibility(bool visible)
        {
            showEquippedArmor = visible;
            
            // Update visibility of armor models
            foreach (EquipmentSlot slot in new[] { EquipmentSlot.Head, EquipmentSlot.Body, 
                                                 EquipmentSlot.Hands, EquipmentSlot.Legs, 
                                                 EquipmentSlot.Feet })
            {
                if (equipmentModels.ContainsKey(slot) && equipmentModels[slot] != null)
                {
                    equipmentModels[slot].SetActive(visible);
                }
            }
        }
        
        /// <summary>
        /// Play equip effects
        /// </summary>
        private void PlayEquipEffects()
        {
            // Play VFX
            if (equipVFX != null)
            {
                Instantiate(equipVFX, transform.position, transform.rotation);
            }
            
            // Play sound
            if (audioSource != null && equipSound != null)
            {
                audioSource.PlayOneShot(equipSound);
            }
        }
        
        /// <summary>
        /// Play unequip effects
        /// </summary>
        private void PlayUnequipEffects()
        {
            // Play VFX
            if (unequipVFX != null)
            {
                Instantiate(unequipVFX, transform.position, transform.rotation);
            }
            
            // Play sound
            if (audioSource != null && unequipSound != null)
            {
                audioSource.PlayOneShot(unequipSound);
            }
        }
        
        /// <summary>
        /// Check if the player meets the requirements to equip an item
        /// </summary>
        private bool MeetsRequirements(EquippableItem item)
        {
            if (item == null)
                return false;
                
            // This would be expanded to check player level, stats, etc.
            // For now, we'll assume all requirements are met
            
            // Example check for level requirement
            CharacterStats stats = GetComponent<CharacterStats>();
            if (stats != null)
            {
                if (stats.Level < item.levelRequirement)
                {
                    return false;
                }
                
                // Check stat requirements
                if (stats.GetStatValue(StatType.Strength) < item.strengthRequirement ||
                    stats.GetStatValue(StatType.Dexterity) < item.dexterityRequirement ||
                    stats.GetStatValue(StatType.Intelligence) < item.intelligenceRequirement)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get appropriate transform for equipment based on slot
        /// </summary>
        public Transform GetEquipmentParent(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.MainHand:
                    return weaponParent != null ? weaponParent : transform;
                    
                case EquipmentSlot.OffHand:
                    return shieldParent != null ? shieldParent : transform;
                    
                case EquipmentSlot.Head:
                    return headParent != null ? headParent : transform;
                    
                case EquipmentSlot.Body:
                    return bodyParent != null ? bodyParent : transform;
                    
                default:
                    return transform;
            }
        }
    }
}
