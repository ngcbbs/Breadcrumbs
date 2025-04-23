using System;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Interface for character stat management
    /// </summary>
    public interface IStatSystem
    {
        // Basic properties
        float CurrentHealth { get; set; }
        float CurrentMana { get; set; }
        int Level { get; }
        int Experience { get; }
        int ExperienceToNextLevel { get; }
        
        // Stat operations
        float GetStat(StatType type);
        void SetBaseStat(StatType type, float value);
        void AddBonus(StatType type, float value);
        void AddModifier(StatType type, StatModifier modifier);
        void RemoveModifier(StatType type, StatModifier modifier);
        void RemoveAllModifiersFromSource(object source);
        
        // Stat management
        bool AddExperience(int amount);
        void InitializeForCombat();
        void UpdateStats(float deltaTime);
        void CalculateDerivedStats(ClassType classType);
        
        // Events
        event Action<StatType, float> OnStatChanged;
        event Action<int> OnLevelUp;
        event Action<float> OnHealthChanged;
        event Action<float> OnManaChanged;
    }
}
