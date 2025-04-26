using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// Manages stamina for player actions like attacking, blocking, and dodging
    /// </summary>
    public class StaminaSystem : MonoBehaviour {
        [Header("Stamina Settings")]
        [SerializeField]
        private float maxStamina = 100f;
        [SerializeField]
        private float currentStamina;
        [SerializeField]
        private float staminaRegenRate = 10f;
        [SerializeField]
        private float staminaRegenDelay = 1f;

        [Header("Depletion Effects")]
        [SerializeField]
        private float depletionDebuffDuration = 3f;
        [SerializeField]
        private float depletedRegenMultiplier = 0.5f;
        [SerializeField]
        private AudioClip depletionSound;
        [SerializeField]
        private GameObject depletionVFX;

        // Events
        [Header("Events")]
        public UnityEvent<float, float> OnStaminaChanged;
        public UnityEvent OnStaminaDepleted;
        public UnityEvent OnStaminaReplenished;

        // Internal state
        private bool isStaminaDepleted = false;
        private bool isRegenerating = true;
        private float lastStaminaUseTime = -999f;
        private Coroutine staminaRegenCoroutine;

        // Properties
        public float MaxStamina => maxStamina;
        public float CurrentStamina => currentStamina;
        public float StaminaPercentage => currentStamina / maxStamina;
        public bool IsStaminaDepleted => isStaminaDepleted;

        private void Awake() {
            // Initialize stamina
            currentStamina = maxStamina;
        }

        private void Start() {
            // Start regeneration coroutine
            staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
        }

        /// <summary>
        /// Use stamina for an action if available
        /// </summary>
        /// <returns>True if stamina was successfully used</returns>
        public bool UseStamina(float amount) {
            if (amount <= 0)
                return true;

            if (currentStamina < amount)
                return false;

            // Use stamina
            currentStamina -= amount;
            lastStaminaUseTime = Time.time;

            // Invoke event
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);

            // Check for depletion
            if (currentStamina <= 0 && !isStaminaDepleted) {
                StaminaDepleted();
            }

            // Stop and restart regeneration
            if (staminaRegenCoroutine != null) {
                StopCoroutine(staminaRegenCoroutine);
            }

            staminaRegenCoroutine = StartCoroutine(RegenerateStamina());

            return true;
        }

        /// <summary>
        /// Add stamina directly
        /// </summary>
        public void AddStamina(float amount) {
            if (amount <= 0)
                return;

            float oldStamina = currentStamina;
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);

            // Only trigger events if stamina actually changed
            if (currentStamina > oldStamina) {
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);

                // Check if we recovered from depletion
                if (isStaminaDepleted && currentStamina >= maxStamina * 0.2f) {
                    RecoverFromDepletion();
                }
            }
        }

        /// <summary>
        /// Regenerate stamina over time
        /// </summary>
        private IEnumerator RegenerateStamina() {
            // Wait for delay after last use
            if (lastStaminaUseTime > 0) {
                yield return new WaitForSeconds(staminaRegenDelay);
            }

            // Regenerate until full
            while (currentStamina < maxStamina) {
                // Calculate regen rate with modifiers
                float actualRegenRate = staminaRegenRate;

                if (isStaminaDepleted) {
                    actualRegenRate *= depletedRegenMultiplier;
                }

                // Add stamina
                float oldStamina = currentStamina;
                currentStamina = Mathf.Min(maxStamina, currentStamina + actualRegenRate * Time.deltaTime);

                // Check for recovery from depletion
                if (isStaminaDepleted && currentStamina >= maxStamina * 0.2f) {
                    RecoverFromDepletion();
                }

                // Send event if changed
                if (currentStamina != oldStamina) {
                    OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                }

                yield return null;
            }

            // Full stamina reached
            if (currentStamina >= maxStamina) {
                OnStaminaReplenished?.Invoke();
            }
        }

        /// <summary>
        /// Handle stamina depletion effects
        /// </summary>
        private void StaminaDepleted() {
            isStaminaDepleted = true;

            // Play depletion effects
            if (depletionVFX != null) {
                Instantiate(depletionVFX, transform.position, Quaternion.identity);
            }

            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && depletionSound != null) {
                audioSource.PlayOneShot(depletionSound);
            }

            // Apply debuffs to player
            // Implementation would depend on character controller
            // Could reduce movement speed, attack speed, etc.

            // Trigger event
            OnStaminaDepleted?.Invoke();

            Debug.Log($"{gameObject.name} stamina depleted!");
        }

        /// <summary>
        /// Recover from stamina depletion
        /// </summary>
        private void RecoverFromDepletion() {
            isStaminaDepleted = false;

            // Remove debuffs
            // Implementation would depend on character controller

            Debug.Log($"{gameObject.name} recovered from stamina depletion!");
        }

        /// <summary>
        /// Set maximum stamina and adjust current stamina accordingly
        /// </summary>
        public void SetMaxStamina(float newMaxStamina, bool maintainPercentage = true) {
            if (newMaxStamina <= 0)
                return;

            float oldMaxStamina = maxStamina;
            maxStamina = newMaxStamina;

            if (maintainPercentage) {
                // Maintain the same stamina percentage
                currentStamina = (currentStamina / oldMaxStamina) * maxStamina;
            } else {
                // Maintain the same current stamina, but capped at new max
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }

            // Invoke event
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        /// <summary>
        /// Check if there's enough stamina for an action
        /// </summary>
        public bool HasEnoughStamina(float amount) {
            return currentStamina >= amount;
        }

        /// <summary>
        /// Pause stamina regeneration
        /// </summary>
        public void PauseRegeneration() {
            isRegenerating = false;

            if (staminaRegenCoroutine != null) {
                StopCoroutine(staminaRegenCoroutine);
                staminaRegenCoroutine = null;
            }
        }

        /// <summary>
        /// Resume stamina regeneration
        /// </summary>
        public void ResumeRegeneration() {
            if (!isRegenerating) {
                isRegenerating = true;
                staminaRegenCoroutine = StartCoroutine(RegenerateStamina());
            }
        }
    }
}