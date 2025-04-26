using UnityEngine;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// Damage types supported by the combat system
    /// </summary>
    public enum DamageType {
        None,
        Physical,
        Magical,
        Fire,
        Ice,
        Poison,
        True // Ignores resistances
    }

    /// <summary>
    /// Interface for any entity that can receive damage
    /// </summary>
    public interface IDamageable {
        /// <summary>
        /// Take damage of the specified amount and type
        /// </summary>
        void TakeDamage(float amount, DamageType type = DamageType.Physical, GameObject source = null);

        /// <summary>
        /// Check if the entity is currently alive
        /// </summary>
        bool IsAlive();
    }

    /// <summary>
    /// Interface for entities that can deal damage
    /// </summary>
    public interface IDamageDealer {
        /// <summary>
        /// Deal damage to a target
        /// </summary>
        void DealDamage(IDamageable target, float amount, DamageType type = DamageType.Physical);
    }
}