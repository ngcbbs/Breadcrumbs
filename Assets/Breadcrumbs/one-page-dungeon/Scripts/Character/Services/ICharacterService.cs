using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character.Services
{
    /// <summary>
    /// Core service for character management functionality
    /// </summary>
    public interface ICharacterService
    {
        /// <summary>
        /// Creates a new character with the specified parameters
        /// </summary>
        ICharacter CreateCharacter(string name, GenderType gender, ClassType classType);
        
        /// <summary>
        /// Gets a character by ID
        /// </summary>
        ICharacter GetCharacter(string characterId);
        
        /// <summary>
        /// Gets all available characters
        /// </summary>
        IEnumerable<ICharacter> GetAllCharacters();
        
        /// <summary>
        /// Gets the active player character
        /// </summary>
        ICharacter GetPlayerCharacter();
        
        /// <summary>
        /// Applies experience to a character
        /// </summary>
        bool GainExperience(ICharacter character, int amount);
        
        /// <summary>
        /// Applies a stat point for the character
        /// </summary>
        bool ApplyStatPoint(ICharacter character, StatType statType);
        
        /// <summary>
        /// Applies a skill point for the character
        /// </summary>
        bool ApplySkillPoint(ICharacter character, string skillId);
        
        /// <summary>
        /// Saves character data to persistent storage
        /// </summary>
        bool SaveCharacterData(ICharacter character);
        
        /// <summary>
        /// Loads character data from persistent storage
        /// </summary>
        ICharacter LoadCharacterData(string characterId);
        
        /// <summary>
        /// Equips an item on a character
        /// </summary>
        bool EquipItem(ICharacter character, IEquipmentItem item);
        
        /// <summary>
        /// Unequips an item from a character
        /// </summary>
        IEquipmentItem UnequipItem(ICharacter character, EquipmentSlot slot);
        
        /// <summary>
        /// Event that fires when a character levels up
        /// </summary>
        event Action<ICharacter, int> OnCharacterLevelUp;
        
        /// <summary>
        /// Event that fires when a character's stats change
        /// </summary>
        event Action<ICharacter, StatType, float> OnCharacterStatChanged;
    }
}
