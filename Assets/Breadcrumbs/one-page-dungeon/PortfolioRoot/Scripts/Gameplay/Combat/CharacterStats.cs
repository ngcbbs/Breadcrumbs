using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// Manages character statistics and attributes, affected by equipment and status effects
    /// </summary>
    public class CharacterStats : MonoBehaviour {
        [Header("Base Stats")]
        [SerializeField]
        private int level = 1;
        [SerializeField]
        private float baseHealth = 100f;
        [SerializeField]
        private float baseMana = 100f;
        [SerializeField]
        private float baseStamina = 100f;
        [SerializeField]
        private float baseStrength = 10f;
        [SerializeField]
        private float baseDexterity = 10f;
        [SerializeField]
        private float baseIntelligence = 10f;
        [SerializeField]
        private float baseDefense = 5f;
        [SerializeField]
        private float baseMoveSpeed = 5f;

        [Header("Combat Stats")]
        [SerializeField]
        private float basePhysicalDamage = 10f;
        [SerializeField]
        private float baseMagicalDamage = 10f;
        [SerializeField]
        private float baseCriticalChance = 0.05f;
        [SerializeField]
        private float baseCriticalDamage = 1.5f;
        [SerializeField]
        private float baseAttackSpeed = 1f;

        [Header("Resistance Stats")]
        [SerializeField]
        private float physicalResistance = 0f;
        [SerializeField]
        private float magicalResistance = 0f;
        [SerializeField]
        private float fireResistance = 0f;
        [SerializeField]
        private float iceResistance = 0f;
        [SerializeField]
        private float poisonResistance = 0f;

        // Events
        [Header("Events")]
        public UnityEvent<StatType, float, float> OnStatChanged;
        public UnityEvent<DamageType, float, float> OnResistanceChanged;
        public UnityEvent<int, int> OnLevelChanged;

        // Dictionaries to track stat modifiers
        private Dictionary<StatType, List<StatModifier>> flatModifiers = new Dictionary<StatType, List<StatModifier>>();
        private Dictionary<StatType, List<StatModifier>> percentModifiers = new Dictionary<StatType, List<StatModifier>>();

        // Dictionaries to track resistance modifiers
        private Dictionary<DamageType, float> resistanceModifiers = new Dictionary<DamageType, float>();

        // Temporary modifiers
        private List<TemporaryStatModifier> temporaryModifiers = new List<TemporaryStatModifier>();

        // Components
        private HealthSystem healthSystem;
        private StaminaSystem staminaSystem;

        // Properties
        public int Level => level;

        private void Awake() {
            // Initialize modifier dictionaries
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType))) {
                flatModifiers[statType] = new List<StatModifier>();
                percentModifiers[statType] = new List<StatModifier>();
            }

            // Initialize resistance modifiers
            foreach (DamageType damageType in System.Enum.GetValues(typeof(DamageType))) {
                resistanceModifiers[damageType] = 0f;
            }

            // Get components
            healthSystem = GetComponent<HealthSystem>();
            staminaSystem = GetComponent<StaminaSystem>();

            // Apply base stats to health and stamina systems
            if (healthSystem != null) {
                healthSystem.SetMaxHealth(GetStatValue(StatType.Health));
            }

            if (staminaSystem != null) {
                staminaSystem.SetMaxStamina(GetStatValue(StatType.Stamina));
            }
        }

        private void Update() {
            // Update temporary modifiers
            UpdateTemporaryModifiers();
        }

        /// <summary>
        /// Update temporary stat modifiers and remove expired ones
        /// </summary>
        private void UpdateTemporaryModifiers() {
            if (temporaryModifiers.Count == 0)
                return;

            List<TemporaryStatModifier> expiredModifiers = new List<TemporaryStatModifier>();

            // Update all temporary modifiers
            foreach (var tempMod in temporaryModifiers) {
                tempMod.remainingDuration -= Time.deltaTime;

                if (tempMod.remainingDuration <= 0f) {
                    // Remove modifier when expired
                    RemoveStatModifier(tempMod.statType, tempMod.modifierType, tempMod.value);
                    expiredModifiers.Add(tempMod);
                }
            }

            // Remove expired modifiers
            foreach (var expired in expiredModifiers) {
                temporaryModifiers.Remove(expired);
            }
        }

        /// <summary>
        /// Get the base value for a stat
        /// </summary>
        public float GetBaseStatValue(StatType statType) {
            switch (statType) {
                case StatType.Health:
                    return baseHealth;
                case StatType.Mana:
                    return baseMana;
                case StatType.Stamina:
                    return baseStamina;
                case StatType.Strength:
                    return baseStrength;
                case StatType.Dexterity:
                    return baseDexterity;
                case StatType.Intelligence:
                    return baseIntelligence;
                case StatType.PhysicalDamage:
                    return basePhysicalDamage;
                case StatType.MagicalDamage:
                    return baseMagicalDamage;
                case StatType.CriticalChance:
                    return baseCriticalChance;
                case StatType.CriticalDamage:
                    return baseCriticalDamage;
                case StatType.AttackSpeed:
                    return baseAttackSpeed;
                case StatType.MovementSpeed:
                    return baseMoveSpeed;
                case StatType.Defense:
                    return baseDefense;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Get the current value for a stat including all modifiers
        /// </summary>
        public float GetStatValue(StatType statType) {
            float baseValue = GetBaseStatValue(statType);
            float flatAddition = 0f;
            float percentAddition = 0f;

            // Add flat modifiers
            if (flatModifiers.ContainsKey(statType)) {
                foreach (var mod in flatModifiers[statType]) {
                    flatAddition += mod.value;
                }
            }

            // Calculate percent modifiers
            if (percentModifiers.ContainsKey(statType)) {
                foreach (var mod in percentModifiers[statType]) {
                    percentAddition += mod.value;
                }
            }

            // Calculate final value
            float finalValue = (baseValue + flatAddition) * (1f + percentAddition / 100f);

            // Ensure certain stats have minimum values
            if (statType == StatType.Health || statType == StatType.Mana || statType == StatType.Stamina) {
                finalValue = Mathf.Max(1f, finalValue);
            }

            if (statType == StatType.MovementSpeed || statType == StatType.AttackSpeed) {
                finalValue = Mathf.Max(0.1f, finalValue);
            }

            return finalValue;
        }

        /// <summary>
        /// Get the current resistance value for a damage type
        /// </summary>
        public float GetResistanceValue(DamageType damageType) {
            float baseResistance = 0f;

            // Get base resistance
            switch (damageType) {
                case DamageType.Physical:
                    baseResistance = physicalResistance;
                    break;
                case DamageType.Magical:
                    baseResistance = magicalResistance;
                    break;
                case DamageType.Fire:
                    baseResistance = fireResistance;
                    break;
                case DamageType.Ice:
                    baseResistance = iceResistance;
                    break;
                case DamageType.Poison:
                    baseResistance = poisonResistance;
                    break;
                case DamageType.True:
                    return 0f; // True damage ignores all resistances
            }

            // Add modifiers
            if (resistanceModifiers.ContainsKey(damageType)) {
                baseResistance += resistanceModifiers[damageType];
            }

            // Cap resistance between -100% and 75%
            return Mathf.Clamp(baseResistance, -100f, 75f);
        }

        /// <summary>
        /// Add a stat modifier
        /// </summary>
        public void AddStatModifier(StatType statType, ModifierType modifierType, float value) {
            // Create modifier
            StatModifier newMod = new StatModifier {
                statType = statType,
                modifierType = modifierType,
                value = value
            };

            // Add to appropriate list
            if (modifierType == ModifierType.Flat) {
                flatModifiers[statType].Add(newMod);
            } else {
                percentModifiers[statType].Add(newMod);
            }

            // Update derived stats if needed
            UpdateDerivedStats(statType);

            // Trigger event
            OnStatChanged?.Invoke(statType, GetBaseStatValue(statType), GetStatValue(statType));
        }

        /// <summary>
        /// Add a temporary stat modifier
        /// </summary>
        public void AddTemporaryStatModifier(StatType statType, ModifierType modifierType, float value, float duration) {
            // Add the actual modifier
            AddStatModifier(statType, modifierType, value);

            // Track it for removal
            temporaryModifiers.Add(new TemporaryStatModifier {
                statType = statType,
                modifierType = modifierType,
                value = value,
                remainingDuration = duration
            });
        }

        /// <summary>
        /// Remove a stat modifier
        /// </summary>
        public void RemoveStatModifier(StatType statType, ModifierType modifierType, float value) {
            List<StatModifier> modList =
                (modifierType == ModifierType.Flat) ? flatModifiers[statType] : percentModifiers[statType];

            // Find and remove the modifier
            for (int i = modList.Count - 1; i >= 0; i--) {
                StatModifier mod = modList[i];
                if (mod.value == value) {
                    modList.RemoveAt(i);
                    break;
                }
            }

            // Update derived stats if needed
            UpdateDerivedStats(statType);

            // Trigger event
            OnStatChanged?.Invoke(statType, GetBaseStatValue(statType), GetStatValue(statType));
        }

        /// <summary>
        /// Add a resistance modifier
        /// </summary>
        public void AddResistanceModifier(DamageType damageType, float value) {
            if (resistanceModifiers.ContainsKey(damageType)) {
                float oldValue = resistanceModifiers[damageType];
                resistanceModifiers[damageType] += value;

                // Trigger event
                OnResistanceChanged?.Invoke(damageType, oldValue, resistanceModifiers[damageType]);
            }
        }

        /// <summary>
        /// Remove a resistance modifier
        /// </summary>
        public void RemoveResistanceModifier(DamageType damageType, float value) {
            if (resistanceModifiers.ContainsKey(damageType)) {
                float oldValue = resistanceModifiers[damageType];
                resistanceModifiers[damageType] -= value;

                // Trigger event
                OnResistanceChanged?.Invoke(damageType, oldValue, resistanceModifiers[damageType]);
            }
        }

        /// <summary>
        /// Set character level
        /// </summary>
        public void SetLevel(int newLevel) {
            if (newLevel <= 0 || newLevel == level)
                return;

            int oldLevel = level;
            level = newLevel;

            // Update stats based on level
            UpdateStatsForLevel();

            // Trigger event
            OnLevelChanged?.Invoke(oldLevel, level);
        }

        /// <summary>
        /// Update stats when level changes
        /// </summary>
        private void UpdateStatsForLevel() {
            // This would contain level scaling logic
            // For a real game, this would scale stats based on formulas or curves
        }

        /// <summary>
        /// Update dependent stats when a base stat changes
        /// </summary>
        private void UpdateDerivedStats(StatType changedStat) {
            // Update health system
            if (changedStat == StatType.Health && healthSystem != null) {
                healthSystem.SetMaxHealth(GetStatValue(StatType.Health));
            }

            // Update stamina system
            if (changedStat == StatType.Stamina && staminaSystem != null) {
                staminaSystem.SetMaxStamina(GetStatValue(StatType.Stamina));
            }
        }
    }

    /// <summary>
    /// Data container for temporary stat modifiers
    /// </summary>
    public class TemporaryStatModifier {
        public StatType statType;
        public ModifierType modifierType;
        public float value;
        public float remainingDuration;
    }
}