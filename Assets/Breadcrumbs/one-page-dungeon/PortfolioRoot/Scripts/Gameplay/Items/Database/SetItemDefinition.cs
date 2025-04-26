using System.Collections.Generic;
using GamePortfolio.Gameplay.Combat;
using UnityEngine;

namespace GamePortfolio.Gameplay.Items.Database {
    /// <summary>
    /// Defines a set of items that provide bonuses when equipped together
    /// </summary>
    [CreateAssetMenu(fileName = "New Set Definition", menuName = "Inventory/Set Definition")]
    public class SetItemDefinition : ScriptableObject {
        [Header("Set Information")]
        public string setId;
        public string setName;
        [TextArea(3, 6)]
        public string setDescription;
        public ItemRarity setRarity = ItemRarity.Rare;
        public Sprite setIcon;

        [Header("Set Items")]
        public List<string> itemIds = new List<string>();

        [Header("Set Bonuses")]
        public List<SetBonus> setBonuses = new List<SetBonus>();

        /// <summary>
        /// Get the bonus for wearing a specific number of pieces
        /// </summary>
        public SetBonus GetBonusForPieceCount(int pieceCount) {
            foreach (var bonus in setBonuses) {
                if (bonus.requiredPieces == pieceCount) {
                    return bonus;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all applicable bonuses for wearing a specific number of pieces
        /// </summary>
        public List<SetBonus> GetAllBonusesForPieceCount(int pieceCount) {
            List<SetBonus> applicableBonuses = new List<SetBonus>();

            foreach (var bonus in setBonuses) {
                // Include all bonuses for piece counts less than or equal to the current count
                if (bonus.requiredPieces <= pieceCount) {
                    applicableBonuses.Add(bonus);
                }
            }

            return applicableBonuses;
        }

        /// <summary>
        /// Check if the set has a bonus for a specific piece count
        /// </summary>
        public bool HasBonusForPieceCount(int pieceCount) {
            foreach (var bonus in setBonuses) {
                if (bonus.requiredPieces == pieceCount) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the total number of pieces in the set
        /// </summary>
        public int GetTotalPieces() {
            return itemIds.Count;
        }
    }

    /// <summary>
    /// Defines a bonus given for wearing a specific number of set pieces
    /// </summary>
    [System.Serializable]
    public class SetBonus {
        public int requiredPieces;
        public string bonusName;
        [TextArea(2, 4)]
        public string bonusDescription;

        [Header("Stat Bonuses")]
        public List<StatModifier> statModifiers = new List<StatModifier>();
        public List<ResistanceModifier> resistanceModifiers = new List<ResistanceModifier>();

        [Header("Special Effects")]
        public List<SetSpecialEffect> specialEffects = new List<SetSpecialEffect>();
        public GameObject visualEffect;
        public AudioClip activationSound;

        /// <summary>
        /// Apply the set bonus to a character
        /// </summary>
        public void ApplyBonus(GameObject character) {
            if (character == null) return;

            // Apply stat modifiers
            CharacterStats characterStats = character.GetComponent<CharacterStats>();
            if (characterStats != null) {
                foreach (var statMod in statModifiers) {
                    characterStats.AddStatModifier(statMod.statType, statMod.modifierType, statMod.value);
                }

                foreach (var resistMod in resistanceModifiers) {
                    characterStats.AddResistanceModifier(resistMod.damageType, resistMod.value);
                }
            }

            // Visual effects
            if (visualEffect != null) {
                Object.Instantiate(visualEffect, character.transform.position, character.transform.rotation, character.transform);
            }

            // Sound effects
            if (activationSound != null && character.TryGetComponent<AudioSource>(out var audioSource)) {
                audioSource.PlayOneShot(activationSound);
            }

            // Special effects would be handled by the SetEffectManager
        }

        /// <summary>
        /// Remove the set bonus from a character
        /// </summary>
        public void RemoveBonus(GameObject character) {
            if (character == null) return;

            // Remove stat modifiers
            CharacterStats characterStats = character.GetComponent<CharacterStats>();
            if (characterStats != null) {
                foreach (var statMod in statModifiers) {
                    characterStats.RemoveStatModifier(statMod.statType, statMod.modifierType, statMod.value);
                }

                foreach (var resistMod in resistanceModifiers) {
                    characterStats.RemoveResistanceModifier(resistMod.damageType, resistMod.value);
                }
            }

            // Special effects would be handled by the SetEffectManager
        }
    }

    /// <summary>
    /// Types of special effects that set bonuses can have
    /// </summary>
    public enum SetEffectType {
        LifeSteal,
        ManaRegeneration,
        DamageAura,
        ElementalDamage,
        CooldownReduction,
        MovementSpeedBoost,
        DefensiveShield,
        ReflectDamage,
        AreaHeal,
        StatusImmunity,
        ChanceOnHit,
        ResourceGeneration
    }

    /// <summary>
    /// Defines a special effect for a set bonus
    /// </summary>
    [System.Serializable]
    public class SetSpecialEffect {
        public SetEffectType effectType;
        public float value;
        public float chance = 1f;
        public float cooldown = 0f;
        public float duration = 0f;
        public string effectDescription;
        public DamageType damageType = DamageType.Physical;
        public GameObject effectPrefab;
    }
}