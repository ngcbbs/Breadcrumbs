using System;

namespace Breadcrumbs.Character.Services
{
    /// <summary>
    /// Service for managing character statistics
    /// </summary>
    public interface IStatService
    {
        /// <summary>
        /// Applies a stat point to the specified stat
        /// </summary>
        bool ApplyStatPoint(ICharacter character, StatType statType);
        
        /// <summary>
        /// Gets available stat points for a character
        /// </summary>
        int GetAvailableStatPoints(ICharacter character);
        
        /// <summary>
        /// Sets available stat points for a character
        /// </summary>
        void SetAvailableStatPoints(ICharacter character, int points);
        
        /// <summary>
        /// Adds a stat modifier to a character
        /// </summary>
        void AddStatModifier(ICharacter character, StatType statType, StatModifier modifier);
        
        /// <summary>
        /// Removes a stat modifier from a character
        /// </summary>
        void RemoveStatModifier(ICharacter character, StatType statType, StatModifier modifier);
        
        /// <summary>
        /// Removes all modifiers from a source
        /// </summary>
        void RemoveAllModifiersFromSource(ICharacter character, object source);
        
        /// <summary>
        /// Calculates all derived stats for a character
        /// </summary>
        void CalculateDerivedStats(ICharacter character);
        
        /// <summary>
        /// Event that fires when a stat changes
        /// </summary>
        event Action<ICharacter, StatType, float> OnStatChanged;
        
        /// <summary>
        /// Event that fires when available stat points change
        /// </summary>
        event Action<ICharacter, int> OnStatPointsChanged;
    }
}
