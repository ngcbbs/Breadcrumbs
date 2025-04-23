using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Implementation of the IStatSystem interface for managing character stats
    /// </summary>
    [Serializable]
    public class StatSystem : IStatSystem
    {
        // Stat storage
        private readonly Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();
        
        // Current resources
        [SerializeField] private float currentHealth;
        [SerializeField] private float currentMana;
        
        // Experience and level
        [SerializeField] private int level = 1;
        [SerializeField] private int experience = 0;
        [SerializeField] private int experienceToNextLevel = 100;
        
        // Events
        public event Action<StatType, float> OnStatChanged;
        public event Action<int> OnLevelUp;
        public event Action<float> OnHealthChanged;
        public event Action<float> OnManaChanged;
        
        // Properties
        public float CurrentHealth
        {
            get => currentHealth;
            set
            {
                float oldValue = currentHealth;
                currentHealth = Mathf.Clamp(value, 0, GetStat(StatType.MaxHealth));
                
                if (oldValue != currentHealth)
                {
                    OnHealthChanged?.Invoke(currentHealth);
                }
            }
        }
        
        public float CurrentMana
        {
            get => currentMana;
            set
            {
                float oldValue = currentMana;
                currentMana = Mathf.Clamp(value, 0, GetStat(StatType.MaxMana));
                
                if (oldValue != currentMana)
                {
                    OnManaChanged?.Invoke(currentMana);
                }
            }
        }
        
        public int Level => level;
        public int Experience => experience;
        public int ExperienceToNextLevel => experienceToNextLevel;
        
        /// <summary>
        /// Creates a new StatSystem with default values
        /// </summary>
        public StatSystem()
        {
            // Initialize all stats with default values
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                Stat stat = new Stat();
                stat.OnValueChanged += () => OnStatChanged?.Invoke(type, stat.Value);
                stats[type] = stat;
            }
        }
        
        /// <summary>
        /// Gets the value of the specified stat
        /// </summary>
        public float GetStat(StatType type)
        {
            if (stats.TryGetValue(type, out Stat stat))
            {
                return stat.Value;
            }
            
            Debug.LogWarning($"Stat {type} not found");
            return 0;
        }
        
        /// <summary>
        /// Sets the base value of the specified stat
        /// </summary>
        public void SetBaseStat(StatType type, float value)
        {
            if (stats.TryGetValue(type, out Stat stat))
            {
                stat.BaseValue = value;
            }
            else
            {
                Debug.LogWarning($"Stat {type} not found");
            }
        }
        
        /// <summary>
        /// Adds a bonus value to the specified stat
        /// </summary>
        public void AddBonus(StatType type, float value)
        {
            if (stats.TryGetValue(type, out Stat stat))
            {
                stat.BonusValue += value;
            }
            else
            {
                Debug.LogWarning($"Stat {type} not found");
            }
        }
        
        /// <summary>
        /// Adds a modifier to the specified stat
        /// </summary>
        public void AddModifier(StatType type, StatModifier modifier)
        {
            if (stats.TryGetValue(type, out Stat stat))
            {
                stat.AddModifier(modifier);
            }
            else
            {
                Debug.LogWarning($"Stat {type} not found");
            }
        }
        
        /// <summary>
        /// Removes a modifier from the specified stat
        /// </summary>
        public void RemoveModifier(StatType type, StatModifier modifier)
        {
            if (stats.TryGetValue(type, out Stat stat))
            {
                stat.RemoveModifier(modifier);
            }
            else
            {
                Debug.LogWarning($"Stat {type} not found");
            }
        }
        
        /// <summary>
        /// Removes all modifiers from the specified source
        /// </summary>
        public void RemoveAllModifiersFromSource(object source)
        {
            foreach (Stat stat in stats.Values)
            {
                stat.RemoveAllModifiersFromSource(source);
            }
        }
        
        /// <summary>
        /// Adds experience and handles level up
        /// </summary>
        public bool AddExperience(int amount)
        {
            experience += amount;
            bool leveledUp = false;
            
            // Check for level up
            while (experience >= experienceToNextLevel)
            {
                experience -= experienceToNextLevel;
                leveledUp = true;
                LevelUp();
            }
            
            return leveledUp;
        }
        
        /// <summary>
        /// Handles level up logic
        /// </summary>
        private void LevelUp()
        {
            level++;
            
            // Calculate next level experience requirement (logarithmic scaling)
            experienceToNextLevel = (int)(experienceToNextLevel * 1.5f);
            
            // Notify listeners
            OnLevelUp?.Invoke(level);
        }
        
        /// <summary>
        /// Initializes the character for combat (resets health/mana)
        /// </summary>
        public void InitializeForCombat()
        {
            CurrentHealth = GetStat(StatType.MaxHealth);
            CurrentMana = GetStat(StatType.MaxMana);
        }
        
        /// <summary>
        /// Updates stats over time (regeneration, etc.)
        /// </summary>
        public void UpdateStats(float deltaTime)
        {
            // Health regeneration
            float healthRegen = GetStat(StatType.HealthRegen) * deltaTime;
            if (healthRegen > 0)
            {
                CurrentHealth += healthRegen;
            }
            
            // Mana regeneration
            float manaRegen = GetStat(StatType.ManaRegen) * deltaTime;
            if (manaRegen > 0)
            {
                CurrentMana += manaRegen;
            }
        }
        
        /// <summary>
        /// Calculates derived stats based on base stats and class
        /// </summary>
        public void CalculateDerivedStats(ClassType classType)
        {
            float strength = GetStat(StatType.Strength);
            float dexterity = GetStat(StatType.Dexterity);
            float intelligence = GetStat(StatType.Intelligence);
            float vitality = GetStat(StatType.Vitality);
            float wisdom = GetStat(StatType.Wisdom);
            float luck = GetStat(StatType.Luck);
            
            // Calculate health and mana
            CalculateHealthAndMana(vitality, strength, intelligence, wisdom, classType);
            
            // Calculate attack and defense
            CalculateAttackAndDefense(strength, dexterity, intelligence, vitality, wisdom, classType);
            
            // Calculate secondary stats
            CalculateSecondaryStats(dexterity, luck, classType);
        }
        
        /// <summary>
        /// Calculates health and mana stats
        /// </summary>
        private void CalculateHealthAndMana(float vitality, float strength, float intelligence, float wisdom, ClassType classType)
        {
            // Base values
            float baseHealth = 100;
            float baseMana = 50;
            float healthPerLevel = 10;
            float manaPerLevel = 5;
            
            // Class multipliers
            float healthMultiplier = 1.0f;
            float manaMultiplier = 1.0f;
            
            switch (classType)
            {
                case ClassType.Warrior:
                    healthMultiplier = 1.2f;
                    manaMultiplier = 0.7f;
                    break;
                case ClassType.Mage:
                    healthMultiplier = 0.8f;
                    manaMultiplier = 1.5f;
                    break;
                case ClassType.Rogue:
                    healthMultiplier = 0.9f;
                    manaMultiplier = 0.8f;
                    break;
                case ClassType.Cleric:
                    healthMultiplier = 1.1f;
                    manaMultiplier = 1.3f;
                    break;
            }
            
            // Calculate max health
            float maxHealth = (baseHealth + (vitality * 10) + (strength * 2) + (level * healthPerLevel)) * healthMultiplier;
            SetBaseStat(StatType.MaxHealth, maxHealth);
            
            // Calculate max mana
            float maxMana = (baseMana + (intelligence * 8) + (wisdom * 4) + (level * manaPerLevel)) * manaMultiplier;
            SetBaseStat(StatType.MaxMana, maxMana);
            
            // Calculate regen stats
            SetBaseStat(StatType.HealthRegen, vitality * 0.1f + level * 0.2f);
            SetBaseStat(StatType.ManaRegen, wisdom * 0.2f + intelligence * 0.1f + level * 0.1f);
        }
        
        /// <summary>
        /// Calculates attack and defense stats
        /// </summary>
        private void CalculateAttackAndDefense(float strength, float dexterity, float intelligence, float vitality, float wisdom, ClassType classType)
        {
            switch (classType)
            {
                case ClassType.Warrior:
                    // Physical focus
                    SetBaseStat(StatType.PhysicalAttack, strength * 2 + level * 2);
                    SetBaseStat(StatType.MagicAttack, intelligence * 0.5f + level * 0.5f);
                    SetBaseStat(StatType.PhysicalDefense, vitality * 1f + strength * 0.5f + level * 1.5f);
                    SetBaseStat(StatType.MagicDefense, wisdom * 0.7f + level * 0.5f);
                    break;
                    
                case ClassType.Mage:
                    // Magic focus
                    SetBaseStat(StatType.PhysicalAttack, strength * 0.5f + level * 0.5f);
                    SetBaseStat(StatType.MagicAttack, intelligence * 2.5f + level * 2f);
                    SetBaseStat(StatType.PhysicalDefense, vitality * 0.5f + level * 0.5f);
                    SetBaseStat(StatType.MagicDefense, wisdom * 1f + intelligence * 0.5f + level * 1f);
                    break;
                    
                case ClassType.Rogue:
                    // Dexterity focus
                    SetBaseStat(StatType.PhysicalAttack, dexterity * 1.5f + strength * 1f + level * 1.5f);
                    SetBaseStat(StatType.MagicAttack, intelligence * 1f + level * 0.7f);
                    SetBaseStat(StatType.PhysicalDefense, vitality * 0.7f + level * 0.8f);
                    SetBaseStat(StatType.MagicDefense, wisdom * 0.8f + level * 0.6f);
                    break;
                    
                case ClassType.Cleric:
                    // Balanced
                    SetBaseStat(StatType.PhysicalAttack, strength * 1.2f + level * 1f);
                    SetBaseStat(StatType.MagicAttack, intelligence * 1.8f + level * 1.5f);
                    SetBaseStat(StatType.PhysicalDefense, vitality * 0.8f + strength * 0.2f + level * 1f);
                    SetBaseStat(StatType.MagicDefense, wisdom * 1.2f + level * 1.2f);
                    break;
                    
                default:
                    // Generic calculations
                    SetBaseStat(StatType.PhysicalAttack, strength * 1f + level * 1f);
                    SetBaseStat(StatType.MagicAttack, intelligence * 1f + level * 1f);
                    SetBaseStat(StatType.PhysicalDefense, vitality * 0.8f + level * 0.8f);
                    SetBaseStat(StatType.MagicDefense, wisdom * 0.8f + level * 0.8f);
                    break;
            }
        }
        
        /// <summary>
        /// Calculates secondary stats like crit, speed, etc.
        /// </summary>
        private void CalculateSecondaryStats(float dexterity, float luck, ClassType classType)
        {
            // Base values
            float baseAttackSpeed = 1.0f;
            float baseMovementSpeed = 5.0f;
            float baseCritChance = 0.05f;
            float baseCritDamage = 1.5f;
            float baseAccuracy = 0.9f;
            float baseEvasion = 0.05f;
            
            // Class bonuses
            switch (classType)
            {
                case ClassType.Warrior:
                    baseAttackSpeed = 1.0f;
                    baseMovementSpeed = 5.0f;
                    baseCritChance = 0.05f;
                    baseCritDamage = 1.5f;
                    baseAccuracy = 0.9f;
                    baseEvasion = 0.05f;
                    break;
                    
                case ClassType.Mage:
                    baseAttackSpeed = 0.8f;
                    baseMovementSpeed = 4.5f;
                    baseCritChance = 0.08f;
                    baseCritDamage = 1.7f;
                    baseAccuracy = 0.95f;
                    baseEvasion = 0.03f;
                    break;
                    
                case ClassType.Rogue:
                    baseAttackSpeed = 1.2f;
                    baseMovementSpeed = 5.5f;
                    baseCritChance = 0.1f;
                    baseCritDamage = 1.8f;
                    baseAccuracy = 0.92f;
                    baseEvasion = 0.08f;
                    break;
                    
                case ClassType.Cleric:
                    baseAttackSpeed = 0.9f;
                    baseMovementSpeed = 4.8f;
                    baseCritChance = 0.06f;
                    baseCritDamage = 1.6f;
                    baseAccuracy = 0.93f;
                    baseEvasion = 0.04f;
                    break;
            }
            
            // Apply base stats and bonuses from dexterity and luck
            SetBaseStat(StatType.AttackSpeed, baseAttackSpeed + dexterity * 0.003f);
            SetBaseStat(StatType.MovementSpeed, baseMovementSpeed + dexterity * 0.01f);
            SetBaseStat(StatType.CriticalChance, baseCritChance + luck * 0.001f);
            SetBaseStat(StatType.CriticalDamage, baseCritDamage + luck * 0.003f);
            SetBaseStat(StatType.Accuracy, baseAccuracy + dexterity * 0.002f);
            SetBaseStat(StatType.Evasion, baseEvasion + dexterity * 0.002f);
        }
    }
}
