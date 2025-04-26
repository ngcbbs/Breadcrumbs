using UnityEngine;
using GamePortfolio.Gameplay.Combat;
using System.Collections.Generic;

namespace GamePortfolio.Gameplay.Items {
    /// <summary>
    /// Types of weapons
    /// </summary>
    public enum WeaponType {
        Sword,
        Axe,
        Mace,
        Spear,
        Dagger,
        Bow,
        Staff,
        Wand,
        Shield
    }

    /// <summary>
    /// Class representing weapons that can be equipped and used for combat
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
    public class WeaponItem : EquippableItem {
        [Header("Weapon Properties")]
        public WeaponType weaponType;
        public float baseDamage = 10f;
        public float attackSpeed = 1f;
        public float attackRange = 2f;
        public float criticalChance = 0.05f;
        public float criticalMultiplier = 1.5f;
        public DamageType damageType = DamageType.Physical;

        [Header("Attack Properties")]
        public float knockbackForce = 1f;
        public float staminaCostPerAttack = 5f;
        public bool canBlock = false;
        public float blockDamageReduction = 0.5f;

        [Header("Attack Effects")]
        public GameObject[] attackEffects;
        public AudioClip[] attackSounds;
        public GameObject hitEffect;
        public AudioClip hitSound;

        [Header("Special Properties")]
        public List<WeaponSpecialProperty> specialProperties = new List<WeaponSpecialProperty>();

        /// <summary>
        /// Called when the weapon is equipped
        /// </summary>
        public override void OnEquip(GameObject character) {
            base.OnEquip(character);

            // Update character combat settings
            PlayerCombat combat = character.GetComponent<PlayerCombat>();
            if (combat != null) {
                UpdateCombatSettings(combat);
            }
        }

        /// <summary>
        /// Called when the weapon is unequipped
        /// </summary>
        public override void OnUnequip(GameObject character) {
            base.OnUnequip(character);

            // Reset character combat settings
            PlayerCombat combat = character.GetComponent<PlayerCombat>();
            if (combat != null) {
                // Restore defaults - in a real game, this would need to handle multiple equipment pieces
                // This is a simplified implementation
            }
        }

        /// <summary>
        /// Apply weapon stats to combat system
        /// </summary>
        private void UpdateCombatSettings(PlayerCombat combat) {
            // Adjust combat parameters based on weapon
            // Note: In a real game, you'd want a more sophisticated system that can
            // handle multiple equipment pieces and their stats

            // This is a simplified implementation
            combat.BaseDamage = baseDamage;
            combat.AttackCooldown = 1f / attackSpeed;
            combat.AttackRange = attackRange;
            combat.DamageType = damageType;

            // Set attack effects if available
            if (attackEffects != null && attackEffects.Length > 0) {
                combat.AttackVFX = attackEffects[0];
            }

            if (attackSounds != null && attackSounds.Length > 0) {
                combat.AttackSound = attackSounds[0];
            }

            // Enable or disable blocking based on weapon
            // Note: This would be handled by a more sophisticated system in a real game
        }

        /// <summary>
        /// Apply durability loss when attacking
        /// </summary>
        public void ApplyAttackDurabilityLoss() {
            if (useDurability) {
                ApplyDurabilityLoss(0.5f); // Less wear than when taking damage
            }
        }

        /// <summary>
        /// Apply durability loss when blocking
        /// </summary>
        public void ApplyBlockDurabilityLoss(float damageBlocked) {
            if (useDurability) {
                // More durability loss for blocking bigger hits
                float durabilityLoss = 0.1f + (damageBlocked * 0.01f);
                ApplyDurabilityLoss(durabilityLoss);
            }
        }

        /// <summary>
        /// Get a random attack sound
        /// </summary>
        public AudioClip GetRandomAttackSound() {
            if (attackSounds == null || attackSounds.Length == 0)
                return null;

            return attackSounds[Random.Range(0, attackSounds.Length)];
        }

        /// <summary>
        /// Get a random attack effect
        /// </summary>
        public GameObject GetRandomAttackEffect() {
            if (attackEffects == null || attackEffects.Length == 0)
                return null;

            return attackEffects[Random.Range(0, attackEffects.Length)];
        }

        /// <summary>
        /// Get tooltip text including weapon stats
        /// </summary>
        public override string GetTooltipText() {
            string baseTooltip = base.GetTooltipText();

            string weaponStatsText = $"\n<color=orange>Weapon Stats:</color>\n" +
                                     $"Damage: {baseDamage} ({damageType})\n" +
                                     $"Attack Speed: {attackSpeed}/sec\n" +
                                     $"Range: {attackRange}m\n" +
                                     $"Critical: {criticalChance * 100f}% (+{(criticalMultiplier - 1f) * 100f}%)\n";

            if (canBlock) {
                weaponStatsText += $"Block: {blockDamageReduction * 100f}% reduction\n";
            }

            string specialText = "";
            foreach (var property in specialProperties) {
                specialText += $"<color=#ADF5FF>{property.name}</color>: {property.description}\n";
            }

            if (!string.IsNullOrEmpty(specialText)) {
                weaponStatsText += $"\n<color=yellow>Special Properties:</color>\n{specialText}";
            }

            return baseTooltip + weaponStatsText;
        }
    }

    /// <summary>
    /// Special properties that weapons can have
    /// </summary>
    [System.Serializable]
    public class WeaponSpecialProperty {
        public string name;
        [TextArea(2, 4)]
        public string description;
        public SpecialPropertyType propertyType;
        public float value;

        // Additional data for specific property types
        public DamageType elementalType;
        public GameObject effectPrefab;
    }

    /// <summary>
    /// Types of special properties weapons can have
    /// </summary>
    public enum SpecialPropertyType {
        ElementalDamage,
        LifeSteal,
        BleedChance,
        StunChance,
        ArmorPenetration,
        StatusImmunity,
        AreaDamage
    }
}