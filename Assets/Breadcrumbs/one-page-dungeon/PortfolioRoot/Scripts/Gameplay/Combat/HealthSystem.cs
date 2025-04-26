using System;
using UnityEngine;
using UnityEngine.Events;
using GamePortfolio.Core;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// A generic health system that can be attached to any GameObject
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable {
        [Header("Health Settings")]
        [SerializeField]
        private float maxHealth = 100f;
        [SerializeField]
        private float currentHealth;
        [SerializeField]
        private bool invulnerable = false;

        [Header("Recovery Settings")]
        [SerializeField]
        private bool autoRecover = false;
        [SerializeField]
        private float recoveryRate = 5f;
        [SerializeField]
        private float recoveryDelay = 3f;

        [Header("Resistance Settings")]
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

        [Header("Effects")]
        [SerializeField]
        private AudioClip hitSound;
        [SerializeField]
        private AudioClip deathSound;
        [SerializeField]
        private GameObject hitVFX;
        [SerializeField]
        private GameObject deathVFX;

        // Events
        [Header("Events")]
        public UnityEvent<float, float> OnHealthChanged;
        public UnityEvent<GameObject> OnDamaged;
        public UnityEvent<GameObject> OnHealed;
        public UnityEvent<GameObject> OnDeath;

        // Internal state
        private bool isDead = false;
        private float lastDamageTime = -999f;
        private float healthPercentage => currentHealth / maxHealth;

        // Components
        private AudioSource audioSource;

        // Properties
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercentage => healthPercentage;

        private void Awake() {
            audioSource = GetComponent<AudioSource>();

            // Initialize health
            currentHealth = maxHealth;
        }

        private void Update() {
            // Handle auto recovery
            if (autoRecover && !isDead && currentHealth < maxHealth) {
                if (Time.time > lastDamageTime + recoveryDelay) {
                    Heal(recoveryRate * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Take damage from a source
        /// </summary>
        public void TakeDamage(float amount, DamageType type = DamageType.Physical, GameObject source = null) {
            if (invulnerable || isDead || amount <= 0)
                return;

            // Apply resistance based on damage type
            float actualDamage = CalculateActualDamage(amount, type);

            // Apply damage
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            lastDamageTime = Time.time;

            // Invoke event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnDamaged?.Invoke(source);

            // Play hit effects
            PlayHitEffect();

            // Check for death
            if (currentHealth <= 0 && !isDead) {
                Die(source);
            }

            // Log for debugging
            Debug.Log(
                $"{gameObject.name} took {actualDamage} damage ({type}) from {(source ? source.name : "unknown")}. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Calculate actual damage after applying resistances
        /// </summary>
        private float CalculateActualDamage(float amount, DamageType type) {
            switch (type) {
                case DamageType.Physical:
                    return amount * (1f - Mathf.Clamp01(physicalResistance / 100f));
                case DamageType.Magical:
                    return amount * (1f - Mathf.Clamp01(magicalResistance / 100f));
                case DamageType.Fire:
                    return amount * (1f - Mathf.Clamp01(fireResistance / 100f));
                case DamageType.Ice:
                    return amount * (1f - Mathf.Clamp01(iceResistance / 100f));
                case DamageType.Poison:
                    return amount * (1f - Mathf.Clamp01(poisonResistance / 100f));
                case DamageType.True:
                    return amount; // True damage ignores all resistances
                default:
                    return amount;
            }
        }

        /// <summary>
        /// Heal the entity for the specified amount
        /// </summary>
        public void Heal(float amount, GameObject source = null) {
            if (isDead || amount <= 0)
                return;

            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

            // Only trigger events if health actually changed
            if (currentHealth > oldHealth) {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                OnHealed?.Invoke(source);

                // Log for debugging
                Debug.Log($"{gameObject.name} healed for {currentHealth - oldHealth}. Health: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// Kill the entity immediately
        /// </summary>
        public void Kill(GameObject source = null) {
            if (!isDead) {
                currentHealth = 0;
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                Die(source);
            }
        }

        /// <summary>
        /// Handle death logic
        /// </summary>
        private void Die(GameObject source) {
            if (isDead)
                return;

            isDead = true;

            // Play death effects
            PlayDeathEffect();

            // Invoke death event
            OnDeath?.Invoke(source);

            // Log for debugging
            Debug.Log($"{gameObject.name} died from {(source ? source.name : "unknown")}");
        }

        /// <summary>
        /// Revive the entity with optional health percentage
        /// </summary>
        public void Revive(float healthPercentage = 1f) {
            if (!isDead)
                return;

            isDead = false;
            currentHealth = maxHealth * Mathf.Clamp01(healthPercentage);

            // Invoke event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Log for debugging
            Debug.Log($"{gameObject.name} revived with {currentHealth}/{maxHealth} health");
        }

        /// <summary>
        /// Set the maximum health and adjust current health accordingly
        /// </summary>
        public void SetMaxHealth(float newMaxHealth, bool maintainPercentage = true) {
            if (newMaxHealth <= 0)
                return;

            float oldMaxHealth = maxHealth;
            maxHealth = newMaxHealth;

            if (maintainPercentage) {
                // Maintain the same health percentage
                currentHealth = (currentHealth / oldMaxHealth) * maxHealth;
            } else {
                // Maintain the same current health, but capped at new max
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            // Invoke event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Check if the entity is alive
        /// </summary>
        public bool IsAlive() {
            return !isDead && currentHealth > 0;
        }

        /// <summary>
        /// Play hit effect and sound
        /// </summary>
        private void PlayHitEffect() {
            if (hitVFX != null) {
                Instantiate(hitVFX, transform.position, Quaternion.identity);
            }

            if (audioSource != null && hitSound != null) {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(hitSound);
            }
        }

        /// <summary>
        /// Play death effect and sound
        /// </summary>
        private void PlayDeathEffect() {
            if (deathVFX != null) {
                Instantiate(deathVFX, transform.position, Quaternion.identity);
            }

            if (audioSource != null && deathSound != null) {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(deathSound);
            }
        }

        /// <summary>
        /// Set resistance to a specific damage type
        /// </summary>
        public void SetResistance(DamageType type, float resistanceValue) {
            resistanceValue = Mathf.Clamp(resistanceValue, -100f, 100f);

            switch (type) {
                case DamageType.Physical:
                    physicalResistance = resistanceValue;
                    break;
                case DamageType.Magical:
                    magicalResistance = resistanceValue;
                    break;
                case DamageType.Fire:
                    fireResistance = resistanceValue;
                    break;
                case DamageType.Ice:
                    iceResistance = resistanceValue;
                    break;
                case DamageType.Poison:
                    poisonResistance = resistanceValue;
                    break;
            }
        }

        /// <summary>
        /// Add temporary invulnerability for a duration
        /// </summary>
        public void AddInvulnerability(float duration) {
            if (duration <= 0)
                return;

            invulnerable = true;
            CancelInvoke(nameof(RemoveInvulnerability));
            Invoke(nameof(RemoveInvulnerability), duration);
        }

        /// <summary>
        /// Remove invulnerability
        /// </summary>
        private void RemoveInvulnerability() {
            invulnerable = false;
        }
    }
}