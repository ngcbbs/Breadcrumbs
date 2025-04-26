using UnityEngine;
using System.Collections.Generic;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Equipment slots that can hold equippable items
    /// </summary>
    public enum EquipmentSlot
    {
        Head,
        Body,
        Hands,
        Legs,
        Feet,
        MainHand,
        OffHand,
        Accessory1,
        Accessory2
    }
    
    /// <summary>
    /// Base class for all equippable items like weapons and armor
    /// </summary>
    [CreateAssetMenu(fileName = "New Equippable Item", menuName = "Inventory/Equippable Item")]
    public class EquippableItem : Item
    {
        [Header("Equipment Settings")]
        public EquipmentSlot equipSlot;
        public GameObject equipmentModel;
        
        [Header("Requirements")]
        public int levelRequirement = 1;
        public int strengthRequirement = 0;
        public int dexterityRequirement = 0;
        public int intelligenceRequirement = 0;
        
        [Header("Stats")]
        public List<StatModifier> statModifiers = new List<StatModifier>();
        public List<ResistanceModifier> resistanceModifiers = new List<ResistanceModifier>();
        
        [Header("Durability")]
        public bool useDurability = false;
        public float maxDurability = 100f;
        [HideInInspector]
        public float currentDurability;
        public float durabilityLossRate = 1f;
        
        [Header("Equipment Effects")]
        public GameObject equipEffect;
        public AudioClip equipSound;
        public GameObject unequipEffect;
        public AudioClip unequipSound;
        
        // Initialization
        private void OnEnable()
        {
            // Reset durability when item is created
            currentDurability = maxDurability;
        }
        
        /// <summary>
        /// Called when the item is equipped
        /// </summary>
        public virtual void OnEquip(GameObject character)
        {
            Debug.Log($"{character.name} equipped {itemName}");
            
            // Apply stat modifiers
            CharacterStats characterStats = character.GetComponent<CharacterStats>();
            if (characterStats != null)
            {
                foreach (var statMod in statModifiers)
                {
                    characterStats.AddStatModifier(statMod.statType, statMod.modifierType, statMod.value);
                }
                
                foreach (var resistMod in resistanceModifiers)
                {
                    characterStats.AddResistanceModifier(resistMod.damageType, resistMod.value);
                }
            }
            
            // Spawn equipment model if available
            if (equipmentModel != null)
            {
                // Find appropriate attachment point
                Transform attachPoint = FindAttachPoint(character.transform, equipSlot);
                
                if (attachPoint != null)
                {
                    GameObject spawnedModel = Instantiate(equipmentModel, attachPoint);
                    spawnedModel.transform.localPosition = Vector3.zero;
                    spawnedModel.transform.localRotation = Quaternion.identity;
                    
                    // Save reference to model for unequipping
                    EquipmentManager equipManager = character.GetComponent<EquipmentManager>();
                    if (equipManager != null)
                    {
                        equipManager.SetEquipmentModel(equipSlot, spawnedModel);
                    }
                }
            }
            
            // Play equip effects
            if (equipEffect != null)
            {
                Instantiate(equipEffect, character.transform.position, character.transform.rotation);
            }
            
            if (equipSound != null && character.TryGetComponent<AudioSource>(out var audioSource))
            {
                audioSource.PlayOneShot(equipSound);
            }
        }
        
        /// <summary>
        /// Called when the item is unequipped
        /// </summary>
        public virtual void OnUnequip(GameObject character)
        {
            Debug.Log($"{character.name} unequipped {itemName}");
            
            // Remove stat modifiers
            CharacterStats characterStats = character.GetComponent<CharacterStats>();
            if (characterStats != null)
            {
                foreach (var statMod in statModifiers)
                {
                    characterStats.RemoveStatModifier(statMod.statType, statMod.modifierType, statMod.value);
                }
                
                foreach (var resistMod in resistanceModifiers)
                {
                    characterStats.RemoveResistanceModifier(resistMod.damageType, resistMod.value);
                }
            }
            
            // Destroy equipment model
            EquipmentManager equipManager = character.GetComponent<EquipmentManager>();
            if (equipManager != null)
            {
                equipManager.ClearEquipmentModel(equipSlot);
            }
            
            // Play unequip effects
            if (unequipEffect != null)
            {
                Instantiate(unequipEffect, character.transform.position, character.transform.rotation);
            }
            
            if (unequipSound != null && character.TryGetComponent<AudioSource>(out var audioSource))
            {
                audioSource.PlayOneShot(unequipSound);
            }
        }
        
        /// <summary>
        /// Use the item (equip it)
        /// </summary>
        public override bool Use(GameObject user)
        {
            // Try to equip
            EquipmentManager equipManager = user.GetComponent<EquipmentManager>();
            if (equipManager != null)
            {
                return equipManager.EquipItem(this);
            }
            
            return false;
        }
        
        /// <summary>
        /// Find the appropriate attachment point for equipment
        /// </summary>
        protected virtual Transform FindAttachPoint(Transform characterTransform, EquipmentSlot slot)
        {
            // Try to find by common naming conventions
            string attachPointName = "";
            
            switch (slot)
            {
                case EquipmentSlot.Head:
                    attachPointName = "Head_Attach";
                    break;
                case EquipmentSlot.Body:
                    attachPointName = "Chest_Attach";
                    break;
                case EquipmentSlot.Hands:
                    attachPointName = "Hands_Attach";
                    break;
                case EquipmentSlot.Legs:
                    attachPointName = "Legs_Attach";
                    break;
                case EquipmentSlot.Feet:
                    attachPointName = "Feet_Attach";
                    break;
                case EquipmentSlot.MainHand:
                    attachPointName = "RightHand_Attach";
                    break;
                case EquipmentSlot.OffHand:
                    attachPointName = "LeftHand_Attach";
                    break;
                case EquipmentSlot.Accessory1:
                case EquipmentSlot.Accessory2:
                    attachPointName = "Accessory_Attach";
                    break;
            }
            
            // Try to find the attachment point
            if (!string.IsNullOrEmpty(attachPointName))
            {
                Transform attachPoint = characterTransform.Find(attachPointName);
                if (attachPoint != null)
                {
                    return attachPoint;
                }
            }
            
            // Fallback to character transform
            return characterTransform;
        }
        
        /// <summary>
        /// Apply durability loss
        /// </summary>
        public virtual void ApplyDurabilityLoss(float amount = 1f)
        {
            if (!useDurability)
                return;
                
            currentDurability = Mathf.Max(0, currentDurability - (amount * durabilityLossRate));
            
            // Check if item broke
            if (currentDurability <= 0)
            {
                OnItemBroken();
            }
        }
        
        /// <summary>
        /// Handle item breaking
        /// </summary>
        protected virtual void OnItemBroken()
        {
            Debug.Log($"{itemName} has broken!");
            // Implement item breaking logic
        }
        
        /// <summary>
        /// Repair item durability
        /// </summary>
        public virtual void Repair(float amount)
        {
            if (!useDurability)
                return;
                
            currentDurability = Mathf.Min(maxDurability, currentDurability + amount);
        }
        
        /// <summary>
        /// Get the tooltip text for the item
        /// </summary>
        public override string GetTooltipText()
        {
            string baseTooltip = base.GetTooltipText();
            
            string requirementsText = "";
            if (levelRequirement > 1)
            {
                requirementsText += $"Requires Level: {levelRequirement}\n";
            }
            
            if (strengthRequirement > 0)
            {
                requirementsText += $"Requires Strength: {strengthRequirement}\n";
            }
            
            if (dexterityRequirement > 0)
            {
                requirementsText += $"Requires Dexterity: {dexterityRequirement}\n";
            }
            
            if (intelligenceRequirement > 0)
            {
                requirementsText += $"Requires Intelligence: {intelligenceRequirement}\n";
            }
            
            if (!string.IsNullOrEmpty(requirementsText))
            {
                baseTooltip += $"\n<color=yellow>Requirements:</color>\n{requirementsText}";
            }
            
            string statsText = "";
            foreach (var stat in statModifiers)
            {
                string prefix = stat.value >= 0 ? "+" : "";
                statsText += $"{stat.statType}: {prefix}{stat.value}" + (stat.modifierType == ModifierType.Percentage ? "%" : "") + "\n";
            }
            
            foreach (var resist in resistanceModifiers)
            {
                string prefix = resist.value >= 0 ? "+" : "";
                statsText += $"{resist.damageType} Resistance: {prefix}{resist.value}%\n";
            }
            
            if (!string.IsNullOrEmpty(statsText))
            {
                baseTooltip += $"\n<color=cyan>Stats:</color>\n{statsText}";
            }
            
            if (useDurability)
            {
                float durabilityPercentage = (currentDurability / maxDurability) * 100f;
                baseTooltip += $"\nDurability: {durabilityPercentage:F1}%";
            }
            
            return baseTooltip;
        }
        
        /// <summary>
        /// Create a copy of this item with current durability
        /// </summary>
        public override Item CreateInstance()
        {
            EquippableItem instance = Instantiate(this);
            instance.currentDurability = this.currentDurability;
            return instance;
        }
    }
    
    /// <summary>
    /// Type of stat to modify
    /// </summary>
    public enum StatType
    {
        Health,
        Mana,
        Stamina,
        Strength,
        Dexterity,
        Intelligence,
        PhysicalDamage,
        MagicalDamage,
        CriticalChance,
        CriticalDamage,
        AttackSpeed,
        MovementSpeed,
        Defense
    }
    
    /// <summary>
    /// Type of modifier to apply
    /// </summary>
    public enum ModifierType
    {
        Flat,           // Add/subtract a flat value
        Percentage      // Add/subtract a percentage
    }
    
    /// <summary>
    /// Data container for stat modifiers
    /// </summary>
    [System.Serializable]
    public class StatModifier
    {
        public StatType statType;
        public ModifierType modifierType;
        public float value;
    }
    
    /// <summary>
    /// Data container for resistance modifiers
    /// </summary>
    [System.Serializable]
    public class ResistanceModifier
    {
        public DamageType damageType;
        public float value; // Percentage reduction
    }
}
