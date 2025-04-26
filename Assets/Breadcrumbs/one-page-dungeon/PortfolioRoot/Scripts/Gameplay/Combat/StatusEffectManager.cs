using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// Types of status effects
    /// </summary>
    public enum StatusEffectType {
        None,

        // Positive effects
        HealthRegen,
        ManaRegen,
        StaminaRegen,
        DamageBoost,
        DefenseBoost,
        SpeedBoost,

        // Negative effects
        Burning,
        Poisoned,
        Frozen,
        Slowed,
        Weakened,
        Stunned
    }

    /// <summary>
    /// Manages status effects on a character
    /// </summary>
    public class StatusEffectManager : MonoBehaviour {
        [Header("Status Effect Settings")]
        [SerializeField]
        private int maxStatusEffects = 10;
        [SerializeField]
        private bool allowMultipleEffectsOfSameType = false;
        [SerializeField]
        private bool allowNegativeEffectsToStack = false;

        [Header("Visual Effects")]
        [SerializeField]
        private Transform statusEffectAnchor;
        [SerializeField]
        private GameObject poisonVFX;
        [SerializeField]
        private GameObject burnVFX;
        [SerializeField]
        private GameObject freezeVFX;

        // Events
        [Header("Events")]
        public UnityEvent<StatusEffect> OnStatusEffectAdded;
        public UnityEvent<StatusEffect> OnStatusEffectRemoved;
        public UnityEvent<StatusEffect> OnStatusEffectExpired;

        // Active status effects
        private List<StatusEffect> activeEffects = new List<StatusEffect>();
        private Dictionary<StatusEffectType, StatusEffect> activeEffectsByType = new Dictionary<StatusEffectType, StatusEffect>();
        private Dictionary<StatusEffectType, GameObject> activeVFX = new Dictionary<StatusEffectType, GameObject>();

        // Components
        private HealthSystem healthSystem;
        private CharacterStats characterStats;

        private void Awake() {
            healthSystem = GetComponent<HealthSystem>();
            characterStats = GetComponent<CharacterStats>();

            if (statusEffectAnchor == null) {
                statusEffectAnchor = transform;
            }
        }

        private void Update() {
            UpdateStatusEffects();
        }

        /// <summary>
        /// Update all active status effects
        /// </summary>
        private void UpdateStatusEffects() {
            List<StatusEffect> expiredEffects = new List<StatusEffect>();

            // Update each effect
            foreach (var effect in activeEffects) {
                // Update duration
                effect.remainingDuration -= Time.deltaTime;

                // Process tick effects
                effect.timeSinceLastTick += Time.deltaTime;

                if (effect.timeSinceLastTick >= effect.tickRate) {
                    effect.timeSinceLastTick = 0f;
                    ProcessEffectTick(effect);
                }

                // Check for expiration
                if (effect.remainingDuration <= 0f) {
                    expiredEffects.Add(effect);
                }
            }

            // Remove expired effects
            foreach (var expired in expiredEffects) {
                RemoveStatusEffect(expired);
                OnStatusEffectExpired?.Invoke(expired);
            }
        }

        /// <summary>
        /// Apply a status effect to the character
        /// </summary>
        public bool ApplyStatusEffect(StatusEffect effect) {
            if (effect == null || effect.duration <= 0f)
                return false;

            // Check if we already have this effect type
            if (activeEffectsByType.TryGetValue(effect.effectType, out StatusEffect existingEffect)) {
                // Handle stacking based on settings
                bool isNegative = IsNegativeEffect(effect.effectType);

                if (!allowMultipleEffectsOfSameType) {
                    // For negative effects, only replace if the new one is shorter
                    if (isNegative && !allowNegativeEffectsToStack) {
                        if (effect.duration < existingEffect.remainingDuration) {
                            // Keep the shorter duration for negative effects
                            return false;
                        }
                    }

                    // Remove the existing effect
                    RemoveStatusEffect(existingEffect);
                }
            }

            // Check if we have too many effects
            if (activeEffects.Count >= maxStatusEffects) {
                // Try to remove the oldest effect
                RemoveStatusEffect(activeEffects[0]);
            }

            // Initialize remaining duration and tick timer
            effect.remainingDuration = effect.duration;
            effect.timeSinceLastTick = 0f;

            // Add the effect
            activeEffects.Add(effect);
            activeEffectsByType[effect.effectType] = effect;

            // Apply initial effect
            ApplyEffectImpact(effect);

            // Create visual effect
            CreateEffectVFX(effect);

            // Trigger event
            OnStatusEffectAdded?.Invoke(effect);

            return true;
        }

        /// <summary>
        /// Remove a status effect
        /// </summary>
        public void RemoveStatusEffect(StatusEffect effect) {
            if (effect == null)
                return;

            // Remove the effect
            activeEffects.Remove(effect);
            activeEffectsByType.Remove(effect.effectType);

            // Remove visual effect
            RemoveEffectVFX(effect.effectType);

            // Remove effect impact if necessary
            RemoveEffectImpact(effect);

            // Trigger event
            OnStatusEffectRemoved?.Invoke(effect);
        }

        /// <summary>
        /// Process a tick of an effect (for damage over time, healing over time, etc.)
        /// </summary>
        private void ProcessEffectTick(StatusEffect effect) {
            switch (effect.effectType) {
                case StatusEffectType.HealthRegen:
                    if (healthSystem != null) {
                        healthSystem.Heal(effect.value);
                    }

                    break;

                case StatusEffectType.Burning:
                case StatusEffectType.Poisoned:
                    if (healthSystem != null) {
                        // Apply damage of the appropriate type
                        DamageType damageType =
                            effect.effectType == StatusEffectType.Burning ? DamageType.Fire : DamageType.Poison;

                        healthSystem.TakeDamage(effect.value, damageType);
                    }

                    break;

                // Other tick-based effects would be processed here
            }
        }

        /// <summary>
        /// Apply the initial impact of an effect (stats changes, etc.)
        /// </summary>
        private void ApplyEffectImpact(StatusEffect effect) {
            if (characterStats == null)
                return;

            switch (effect.effectType) {
                case StatusEffectType.DamageBoost:
                    characterStats.AddTemporaryStatModifier(Items.StatType.PhysicalDamage,
                        Items.ModifierType.Percentage,
                        effect.value,
                        effect.duration);
                    break;

                case StatusEffectType.DefenseBoost:
                    characterStats.AddTemporaryStatModifier(Items.StatType.Defense,
                        Items.ModifierType.Percentage,
                        effect.value,
                        effect.duration);
                    break;

                case StatusEffectType.SpeedBoost:
                    characterStats.AddTemporaryStatModifier(Items.StatType.MovementSpeed,
                        Items.ModifierType.Percentage,
                        effect.value,
                        effect.duration);
                    break;

                case StatusEffectType.Weakened:
                    characterStats.AddTemporaryStatModifier(Items.StatType.PhysicalDamage,
                        Items.ModifierType.Percentage,
                        -effect.value,
                        effect.duration);
                    break;

                case StatusEffectType.Slowed:
                    characterStats.AddTemporaryStatModifier(Items.StatType.MovementSpeed,
                        Items.ModifierType.Percentage,
                        -effect.value,
                        effect.duration);
                    break;

                case StatusEffectType.Frozen:
                    // Apply stun effect
                    // This would be tied to the character controller in a full implementation
                    Debug.Log($"{gameObject.name} is frozen for {effect.duration} seconds");
                    break;

                case StatusEffectType.Stunned:
                    // Apply stun effect
                    // This would be tied to the character controller in a full implementation
                    Debug.Log($"{gameObject.name} is stunned for {effect.duration} seconds");
                    break;
            }
        }

        /// <summary>
        /// Remove the impact of an effect when it expires or is removed
        /// </summary>
        private void RemoveEffectImpact(StatusEffect effect) {
            // Most effects are handled through the temporary modifier system in CharacterStats
            // Only effects with special handling need to be processed here

            switch (effect.effectType) {
                case StatusEffectType.Frozen:
                case StatusEffectType.Stunned:
                    // Remove stun effect
                    // This would be tied to the character controller in a full implementation
                    Debug.Log($"{gameObject.name} is no longer stunned/frozen");
                    break;
            }
        }

        /// <summary>
        /// Create a visual effect for a status effect
        /// </summary>
        private void CreateEffectVFX(StatusEffect effect) {
            // Remove any existing VFX for this effect type
            RemoveEffectVFX(effect.effectType);

            // Create appropriate VFX
            GameObject vfxPrefab = null;

            switch (effect.effectType) {
                case StatusEffectType.Poisoned:
                    vfxPrefab = poisonVFX;
                    break;
                case StatusEffectType.Burning:
                    vfxPrefab = burnVFX;
                    break;
                case StatusEffectType.Frozen:
                    vfxPrefab = freezeVFX;
                    break;
            }

            if (vfxPrefab != null) {
                GameObject vfx = Instantiate(vfxPrefab, statusEffectAnchor.position, Quaternion.identity);
                vfx.transform.SetParent(statusEffectAnchor);

                // Store reference
                activeVFX[effect.effectType] = vfx;
            }
        }

        /// <summary>
        /// Remove a visual effect for a status effect
        /// </summary>
        private void RemoveEffectVFX(StatusEffectType effectType) {
            if (activeVFX.TryGetValue(effectType, out GameObject vfx) && vfx != null) {
                Destroy(vfx);
                activeVFX.Remove(effectType);
            }
        }

        /// <summary>
        /// Check if the character has a specific status effect
        /// </summary>
        public bool HasStatusEffect(StatusEffectType effectType) {
            return activeEffectsByType.ContainsKey(effectType);
        }

        /// <summary>
        /// Get the remaining duration of a status effect
        /// </summary>
        public float GetStatusEffectDuration(StatusEffectType effectType) {
            if (activeEffectsByType.TryGetValue(effectType, out StatusEffect effect)) {
                return effect.remainingDuration;
            }

            return 0f;
        }

        /// <summary>
        /// Get all active status effects
        /// </summary>
        public List<StatusEffect> GetAllStatusEffects() {
            return new List<StatusEffect>(activeEffects);
        }

        /// <summary>
        /// Check if this is a negative effect
        /// </summary>
        public bool IsNegativeEffect(StatusEffectType effectType) {
            switch (effectType) {
                case StatusEffectType.Burning:
                case StatusEffectType.Poisoned:
                case StatusEffectType.Frozen:
                case StatusEffectType.Slowed:
                case StatusEffectType.Weakened:
                case StatusEffectType.Stunned:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Remove all negative status effects
        /// </summary>
        public void RemoveAllNegativeEffects() {
            List<StatusEffect> effectsToRemove = new List<StatusEffect>();

            foreach (var effect in activeEffects) {
                if (IsNegativeEffect(effect.effectType)) {
                    effectsToRemove.Add(effect);
                }
            }

            foreach (var effect in effectsToRemove) {
                RemoveStatusEffect(effect);
            }
        }

        /// <summary>
        /// Remove all status effects
        /// </summary>
        public void RemoveAllStatusEffects() {
            List<StatusEffect> effectsToRemove = new List<StatusEffect>(activeEffects);

            foreach (var effect in effectsToRemove) {
                RemoveStatusEffect(effect);
            }
        }
    }

    /// <summary>
    /// Data container for status effects
    /// </summary>
    [System.Serializable]
    public class StatusEffect {
        public string name;
        public StatusEffectType effectType;
        public float value;
        public float duration;
        public float tickRate = 1f;
        public Sprite icon;

        // Runtime data - not serialized
        [System.NonSerialized]
        public float remainingDuration;
        [System.NonSerialized]
        public float timeSinceLastTick;
    }
}