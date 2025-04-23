using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Interface for all equipment items
    /// </summary>
    public interface IEquipmentItem
    {
        // Basic properties
        string ItemId { get; }
        string ItemName { get; }
        string Description { get; }
        Sprite Icon { get; }
        EquipmentSlot EquipSlot { get; }
        int RequiredLevel { get; }
        ClassType ClassRequirement { get; }
        int ItemLevel { get; }
        ItemRarity Rarity { get; }
        
        // Visual properties
        GameObject ItemModel { get; }
        Color PrimaryColor { get; }
        Color SecondaryColor { get; }
        
        // Set information
        string SetName { get; }
        
        // Stats and effects
        IReadOnlyList<ItemStat> Stats { get; }
        IReadOnlyList<SpecialEffect> SpecialEffects { get; }
        
        // Functionality
        void OnEquip(ICharacter character);
        void OnUnequip(ICharacter character);
        int CalculateItemScore();
    }
    
    /// <summary>
    /// Rarity levels for items
    /// </summary>
    public enum ItemRarity
    {
        Common,   // White
        Uncommon, // Green
        Rare,     // Blue
        Epic,     // Purple
        Legendary // Orange/Gold
    }
    
    /// <summary>
    /// Stat modifier applied by an item
    /// </summary>
    public class ItemStat
    {
        public StatType StatType { get; set; }
        public float Value { get; set; }
        public StatModifierType ModifierType { get; set; }
    }
    
    /// <summary>
    /// Special effect that can be triggered by an item
    /// </summary>
    public class SpecialEffect
    {
        public string EffectName { get; set; }
        public string EffectDescription { get; set; }
        public float ActivationChance { get; set; }
        public GameObject VisualEffect { get; set; }
        public AudioClip SoundEffect { get; set; }
    }
}
