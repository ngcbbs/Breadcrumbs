using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Represents a single character stat with base value and modifiers
    /// </summary>
    [Serializable]
    public class Stat
    {
        [SerializeField] private float baseValue; // Base value
        [SerializeField] private float bonusValue; // Bonus from equipment, levels, etc.
        
        private readonly List<StatModifier> modifiers = new List<StatModifier>();
        
        /// <summary>
        /// Gets or sets the base value of this stat
        /// </summary>
        public float BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = value;
                OnValueChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// Gets or sets the bonus value of this stat
        /// </summary>
        public float BonusValue
        {
            get => bonusValue;
            set
            {
                bonusValue = value;
                OnValueChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// Gets the final calculated value of this stat with all modifiers applied
        /// </summary>
        public float Value
        {
            get
            {
                float finalValue = baseValue + bonusValue;
                float percentAdditive = 0;
                float percentMultiplicative = 1;
                
                // Apply modifiers in order
                foreach (StatModifier mod in modifiers)
                {
                    switch (mod.Type)
                    {
                        case StatModifierType.Flat:
                            finalValue += mod.Value;
                            break;
                        case StatModifierType.PercentAdditive:
                            percentAdditive += mod.Value;
                            break;
                        case StatModifierType.PercentMultiplicative:
                            percentMultiplicative *= (1 + mod.Value);
                            break;
                    }
                }
                
                // Apply percentage modifiers
                finalValue *= (1 + percentAdditive);
                finalValue *= percentMultiplicative;
                
                return finalValue;
            }
        }
        
        /// <summary>
        /// Event triggered when the stat value changes
        /// </summary>
        public event Action OnValueChanged;
        
        /// <summary>
        /// Creates a new stat with the given base value
        /// </summary>
        public Stat(float baseValue = 0)
        {
            this.baseValue = baseValue;
            this.bonusValue = 0;
        }
        
        /// <summary>
        /// Adds a modifier to this stat
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            // Sort modifiers by order to ensure correct application
            modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
            OnValueChanged?.Invoke();
        }
        
        /// <summary>
        /// Removes a modifier from this stat
        /// </summary>
        public bool RemoveModifier(StatModifier modifier)
        {
            bool removed = modifiers.Remove(modifier);
            if (removed)
            {
                OnValueChanged?.Invoke();
            }
            return removed;
        }
        
        /// <summary>
        /// Removes all modifiers from the given source
        /// </summary>
        public bool RemoveAllModifiersFromSource(object source)
        {
            bool removed = false;
            
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (modifiers[i].Source == source)
                {
                    modifiers.RemoveAt(i);
                    removed = true;
                }
            }
            
            if (removed)
            {
                OnValueChanged?.Invoke();
            }
            
            return removed;
        }
        
        /// <summary>
        /// Clears all modifiers
        /// </summary>
        public void ClearModifiers()
        {
            bool hadModifiers = modifiers.Count > 0;
            modifiers.Clear();
            
            if (hadModifiers)
            {
                OnValueChanged?.Invoke();
            }
        }
    }
}
