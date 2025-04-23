using Breadcrumbs.EventSystem;
using UnityEngine;

namespace Breadcrumbs.Character
{
    #region Character Events
    
    /// <summary>
    /// Event for character level up
    /// </summary>
    public class PlayerLeveledUpEvent : IEvent
    {
        public ICharacter Character;
        public int NewLevel;
        public int StatPoints;
        public int SkillPoints;
    }
    
    /// <summary>
    /// Event for experience gained
    /// </summary>
    public class ExperienceGainedEvent : IEvent
    {
        public ICharacter Character;
        public int Amount;
        public int CurrentExperience;
        public int ExperienceToNextLevel;
    }
    
    /// <summary>
    /// Event for character taking damage
    /// </summary>
    public class CharacterDamagedEvent : IEvent
    {
        public ICharacter Character;
        public float DamageAmount;
        public DamageType DamageType;
        public float CurrentHealth;
        public float MaxHealth;
    }
    
    /// <summary>
    /// Event for character healing
    /// </summary>
    public class CharacterHealedEvent : IEvent
    {
        public ICharacter Character;
        public float HealAmount;
        public float CurrentHealth;
        public float MaxHealth;
    }
    
    /// <summary>
    /// Event for character death
    /// </summary>
    public class CharacterDeathEvent : IEvent
    {
        public ICharacter Character;
    }
    
    /// <summary>
    /// Event for item equipped
    /// </summary>
    public class ItemEquippedEvent : IEvent
    {
        public ICharacter Character;
        public IEquipmentItem Item;
    }
    
    /// <summary>
    /// Event for item unequipped
    /// </summary>
    public class ItemUnequippedEvent : IEvent
    {
        public ICharacter Character;
        public IEquipmentItem Item;
        public EquipmentSlot Slot;
    }
    
    /// <summary>
    /// Event for stat point used
    /// </summary>
    public class StatPointUsedEvent : IEvent
    {
        public ICharacter Character;
        public StatType StatType;
        public float NewValue;
        public int RemainingPoints;
    }
    
    /// <summary>
    /// Event for skill learned
    /// </summary>
    public class SkillLearnedEvent : IEvent
    {
        public ICharacter Character;
        public string SkillId;
    }
    
    /// <summary>
    /// Event for skill used
    /// </summary>
    public class SkillUsedEvent : IEvent
    {
        public ICharacter Character;
        public string SkillId;
        public Transform Target;
    }
    
    /// <summary>
    /// Event for skill upgraded
    /// </summary>
    public class SkillUpgradedEvent : IEvent
    {
        public ICharacter Character;
        public string SkillId;
        public int NewLevel;
        public int RemainingPoints;
    }
    
    /// <summary>
    /// Event for class special ability used
    /// </summary>
    public class ClassSpecialUsedEvent : IEvent
    {
        public ICharacter Character;
        public ClassType ClassType;
    }
    
    /// <summary>
    /// Event for character loaded
    /// </summary>
    public class CharacterLoadedEvent : IEvent
    {
        public ICharacter Character;
    }
    
    #endregion
}
