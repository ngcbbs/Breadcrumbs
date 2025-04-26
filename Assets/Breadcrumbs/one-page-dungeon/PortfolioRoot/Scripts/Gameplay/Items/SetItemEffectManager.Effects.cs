using System.Collections.Generic;
using GamePortfolio.Gameplay.Character;
using UnityEngine;
using GamePortfolio.Gameplay.Items.Database;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items {
    /// <summary>
    /// Partial class for SetItemEffectManager - Effect application functionality
    /// </summary>
    public partial class SetItemEffectManager : MonoBehaviour {
        /// <summary>
        /// Apply a specific set bonus
        /// </summary>
        private void ApplySetBonus(string setId, SetBonus bonus) {
            // Apply stat modifiers
            CharacterStats characterStats = GetComponent<CharacterStats>();
            if (characterStats != null) {
                foreach (var statMod in bonus.statModifiers) {
                    characterStats.AddStatModifier(statMod.statType, statMod.modifierType, statMod.value);
                }

                foreach (var resistMod in bonus.resistanceModifiers) {
                    characterStats.AddResistanceModifier(resistMod.damageType, resistMod.value);
                }
            }

            // Create visual effect
            if (bonus.visualEffect != null) {
                GameObject effectObj = Instantiate(bonus.visualEffect, transform.position, transform.rotation, transform);

                // Store effect for later cleanup
                if (!setVisualEffects.ContainsKey(setId)) {
                    setVisualEffects[setId] = new List<GameObject>();
                }

                setVisualEffects[setId].Add(effectObj);
            }

            // Play sound
            if (bonus.activationSound != null && TryGetComponent<AudioSource>(out var audioSource)) {
                audioSource.PlayOneShot(bonus.activationSound);
            }

            // Handle special effects
            foreach (var specialEffect in bonus.specialEffects) {
                ApplySpecialEffect(specialEffect);
            }
        }

        /// <summary>
        /// Remove all active set bonuses
        /// </summary>
        private void RemoveAllSetBonuses() {
            foreach (var kvp in activeSetBonuses) {
                string setId = kvp.Key;
                List<SetBonus> bonuses = kvp.Value;

                foreach (var bonus in bonuses) {
                    RemoveSetBonus(setId, bonus);
                }
            }

            activeSetBonuses.Clear();
        }

        /// <summary>
        /// Remove a specific set bonus
        /// </summary>
        private void RemoveSetBonus(string setId, SetBonus bonus) {
            // Remove stat modifiers
            CharacterStats characterStats = GetComponent<CharacterStats>();
            if (characterStats != null) {
                foreach (var statMod in bonus.statModifiers) {
                    characterStats.RemoveStatModifier(statMod.statType, statMod.modifierType, statMod.value);
                }

                foreach (var resistMod in bonus.resistanceModifiers) {
                    characterStats.RemoveResistanceModifier(resistMod.damageType, resistMod.value);
                }
            }

            // Remove visual effects
            if (setVisualEffects.ContainsKey(setId)) {
                foreach (var effect in setVisualEffects[setId]) {
                    if (effect != null) {
                        Destroy(effect);
                    }
                }

                setVisualEffects[setId].Clear();
            }

            // Special effects are handled separately
        }

        /// <summary>
        /// Clear all set effects and visual objects
        /// </summary>
        private void ClearAllSetEffects() {
            RemoveAllSetBonuses();

            // Destroy all visual effects
            foreach (var kvp in setVisualEffects) {
                foreach (var effect in kvp.Value) {
                    if (effect != null) {
                        Destroy(effect);
                    }
                }
            }

            setVisualEffects.Clear();
        }

        /// <summary>
        /// Apply special set effect
        /// </summary>
        private void ApplySpecialEffect(SetSpecialEffect specialEffect) {
            // Implementation would depend on the effect type
            // This is a simplified version that just logs the effect

            if (showDebugInfo) {
                Debug.Log($"Applied special set effect: {specialEffect.effectType} with value {specialEffect.value}");
            }

            // In a full implementation, we would register different effect types with appropriate systems
            // For example:

            switch (specialEffect.effectType) {
                case SetEffectType.LifeSteal:
                    // Register with combat system for life steal effect
                    CombatSystem combatSystem = GetComponent<CombatSystem>();
                    if (combatSystem != null) {
                        // combatSystem.RegisterLifeStealEffect(specialEffect.value);
                    }

                    break;

                case SetEffectType.DamageAura:
                    // Create a damage aura effect
                    if (specialEffect.effectPrefab != null) {
                        GameObject aura = Instantiate(specialEffect.effectPrefab, transform.position, transform.rotation,
                            transform);

                        // Store for cleanup
                        string dummySetId = "SpecialEffects";
                        if (!setVisualEffects.ContainsKey(dummySetId)) {
                            setVisualEffects[dummySetId] = new List<GameObject>();
                        }

                        setVisualEffects[dummySetId].Add(aura);

                        // Configure the aura's damage
                        // DamageAura auraComponent = aura.GetComponent<DamageAura>();
                        // if (auraComponent != null)
                        // {
                        //     auraComponent.SetDamage(specialEffect.value);
                        //     auraComponent.SetDamageType(specialEffect.damageType);
                        // }
                    }

                    break;

                case SetEffectType.ElementalDamage:
                    // Add elemental damage to player's attacks
                    PlayerCombat playerCombat = GetComponent<PlayerCombat>();
                    if (playerCombat != null) {
                        // playerCombat.AddElementalDamage(specialEffect.damageType, specialEffect.value);
                    }

                    break;

                case SetEffectType.CooldownReduction:
                    // Apply cooldown reduction to abilities
                    // AbilityManager abilityManager = GetComponent<AbilityManager>();
                    // if (abilityManager != null)
                    // {
                    //     abilityManager.ApplyCooldownReduction(specialEffect.value / 100f);
                    // }
                    break;

                case SetEffectType.MovementSpeedBoost:
                    // Apply movement speed boost
                    PlayerController playerController = GetComponent<PlayerController>();
                    if (playerController != null) {
                        // playerController.AddMovementSpeedBonus(specialEffect.value / 100f);
                    }

                    break;

                case SetEffectType.DefensiveShield:
                    // Create a defensive shield
                    if (specialEffect.effectPrefab != null) {
                        GameObject shield = Instantiate(specialEffect.effectPrefab, transform.position, transform.rotation,
                            transform);

                        // Store for cleanup
                        string shieldSetId = "DefensiveShields";
                        if (!setVisualEffects.ContainsKey(shieldSetId)) {
                            setVisualEffects[shieldSetId] = new List<GameObject>();
                        }

                        setVisualEffects[shieldSetId].Add(shield);

                        // Configure shield
                        // DefensiveShield shieldComponent = shield.GetComponent<DefensiveShield>();
                        // if (shieldComponent != null)
                        // {
                        //     shieldComponent.SetShieldAmount(specialEffect.value);
                        //     shieldComponent.SetDuration(specialEffect.duration);
                        // }
                    }

                    break;

                // Add more effect types as needed
            }
        }
    }
}