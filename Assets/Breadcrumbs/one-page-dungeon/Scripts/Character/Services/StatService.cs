using System;
using System.Collections.Generic;
using Breadcrumbs.DependencyInjection;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.Character.Services
{
    /// <summary>
    /// Implementation of the stat service
    /// </summary>
    public class StatService : Singleton<StatService>, IStatService, IDependencyProvider
    {
        // Character stat points tracker
        private Dictionary<int, int> characterStatPoints = new Dictionary<int, int>();
        
        // Events
        public event Action<ICharacter, StatType, float> OnStatChanged;
        public event Action<ICharacter, int> OnStatPointsChanged;
        
        /// <summary>
        /// Service provider for stat service
        /// </summary>
        [Provide]
        public IStatService ProvideStatService()
        {
            return this;
        }
        
        protected override void Awake()
        {
            base.Awake();
        }
        
        /// <summary>
        /// Applies a stat point to the specified stat
        /// </summary>
        public bool ApplyStatPoint(ICharacter character, StatType statType)
        {
            int availablePoints = GetAvailableStatPoints(character);
            
            if (character == null || availablePoints <= 0)
            {
                return false;
            }
            
            // Apply stat point
            character.Stats.AddBonus(statType, 1);
            
            // Decrement available points
            SetAvailableStatPoints(character, availablePoints - 1);
            
            // Recalculate derived stats
            CalculateDerivedStats(character);
            
            Debug.Log($"{character.Name} increased {statType} by 1. Remaining points: {GetAvailableStatPoints(character)}");
            
            return true;
        }
        
        /// <summary>
        /// Gets available stat points for a character
        /// </summary>
        public int GetAvailableStatPoints(ICharacter character)
        {
            if (character == null) return 0;

            int characterId = ((Character)character).GetInstanceID();
            
            // Otherwise use our internal tracking
            return characterStatPoints.GetValueOrDefault(characterId, 0);
        }
        
        /// <summary>
        /// Sets available stat points for a character
        /// </summary>
        public void SetAvailableStatPoints(ICharacter character, int points)
        {
            if (character == null) return;
            
            int characterId = ((Character)character).GetInstanceID();
            int oldPoints = characterStatPoints.GetValueOrDefault(characterId, 0);
            
            characterStatPoints[characterId] = points;
            
            // Notify if points changed
            if (oldPoints != points)
            {
                OnStatPointsChanged?.Invoke(character, points);
            }
        }
        
        /// <summary>
        /// Adds a stat modifier to a character
        /// </summary>
        public void AddStatModifier(ICharacter character, StatType statType, StatModifier modifier)
        {
            if (character == null || character.Stats == null) return;
            
            character.Stats.AddModifier(statType, modifier);
            CalculateDerivedStats(character);
        }
        
        /// <summary>
        /// Removes a stat modifier from a character
        /// </summary>
        public void RemoveStatModifier(ICharacter character, StatType statType, StatModifier modifier)
        {
            if (character == null || character.Stats == null) return;
            
            character.Stats.RemoveModifier(statType, modifier);
            CalculateDerivedStats(character);
        }
        
        /// <summary>
        /// Removes all modifiers from a source
        /// </summary>
        public void RemoveAllModifiersFromSource(ICharacter character, object source)
        {
            if (character == null || character.Stats == null) return;
            
            character.Stats.RemoveAllModifiersFromSource(source);
            CalculateDerivedStats(character);
        }
        
        /// <summary>
        /// Calculates all derived stats for a character
        /// </summary>
        public void CalculateDerivedStats(ICharacter character)
        {
            if (character == null || character.Stats == null) return;
            
            character.Stats.CalculateDerivedStats(character.ClassType);
        }
        
        /// <summary>
        /// Receive initial stat points for a character based on level
        /// </summary>
        public int CalculateInitialStatPoints(ICharacter character)
        {
            if (character == null) return 0;
            
            // Base points plus additional for each level above 1
            return 5 + (character.Level - 1) * 5;
        }
        
        /// <summary>
        /// Receive stat points for a character upon leveling up
        /// </summary>
        public void HandleLevelUp(ICharacter character)
        {
            if (character == null) return;
            
            // Add stat points
            int currentPoints = GetAvailableStatPoints(character);
            SetAvailableStatPoints(character, currentPoints + 5);
            
            // Recalculate derived stats
            CalculateDerivedStats(character);
        }
    }
}
