using UnityEngine;

namespace Breadcrumbs.Character.Presentation
{
    /// <summary>
    /// Interface for presenting character data to the UI
    /// </summary>
    public interface ICharacterPresenter
    {
        /// <summary>
        /// Gets the character this presenter is presenting
        /// </summary>
        ICharacter Character { get; }
        
        /// <summary>
        /// Initializes the presenter with a character
        /// </summary>
        void Initialize(ICharacter character);
        
        /// <summary>
        /// Updates the character display
        /// </summary>
        void UpdateCharacterDisplay();
        
        /// <summary>
        /// Updates the stats display
        /// </summary>
        void UpdateStatsDisplay();
        
        /// <summary>
        /// Updates the equipment display
        /// </summary>
        void UpdateEquipmentDisplay();
        
        /// <summary>
        /// Updates the skills display
        /// </summary>
        void UpdateSkillsDisplay();
        
        /// <summary>
        /// Handles a stat point being applied
        /// </summary>
        void HandleStatPointApplied(StatType statType);
        
        /// <summary>
        /// Handles a skill point being applied
        /// </summary>
        void HandleSkillPointApplied(string skillId);
        
        /// <summary>
        /// Handles a level up
        /// </summary>
        void HandleLevelUp(int newLevel);
    }
}
