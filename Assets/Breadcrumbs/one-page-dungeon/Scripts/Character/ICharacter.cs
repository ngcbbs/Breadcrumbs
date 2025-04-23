using System;
using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Core interface for all character types in the game
    /// </summary>
    public interface ICharacter
    {
        // Basic properties
        string Name { get; }
        int Level { get; }
        GenderType Gender { get; }
        ClassType ClassType { get; }
        
        // Stats access
        IStatSystem Stats { get; }
        
        // Core functionality
        void Initialize();
        bool GainExperience(int amount);
        void TakeDamage(float amount, DamageType damageType = DamageType.Physical);
        void Heal(float amount);
        
        // Equipment
        bool EquipItem(IEquipmentItem item);
        IEquipmentItem UnequipItem(EquipmentSlot slot);
        IEquipmentItem GetEquippedItem(EquipmentSlot slot);
        
        // Skills
        bool LearnSkill(string skillId);
        bool UseSkill(string skillId, Transform target = null);
        bool UpgradeSkill(string skillId);
        
        // Status
        bool IsAlive { get; }
        
        // Events
        event Action<int> OnLevelUp;
        event Action<float, DamageType> OnDamageTaken;
        event Action<float> OnHealed;
        event Action<IEquipmentItem> OnItemEquipped;
        event Action<IEquipmentItem, EquipmentSlot> OnItemUnequipped;
    }
}
