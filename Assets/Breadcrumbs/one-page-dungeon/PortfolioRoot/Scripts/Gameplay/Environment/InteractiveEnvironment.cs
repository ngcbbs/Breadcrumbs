using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Interaction;
using GamePortfolio.Gameplay.Combat;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.Gameplay.Environment {
    /// <summary>
    /// Base class for all interactive environment elements
    /// </summary>
    public abstract class InteractiveEnvironment : MonoBehaviour, IInteractable {
        [Header("Interactive Environment Settings")]
        [SerializeField]
        protected string interactionPrompt = "Interact";
        [SerializeField]
        protected float interactionTime = 0f; // 0 = instant
        [SerializeField]
        protected bool oneTimeUse = false;
        [SerializeField]
        protected bool requiresKey = false;
        [SerializeField]
        protected string requiredKeyName = "";

        [Header("Audio Visual")]
        [SerializeField]
        protected AudioClip interactSound;
        [SerializeField]
        protected GameObject interactVFX;

        // State tracking
        protected bool isInteracted = false;
        protected bool isDisabled = false;

        // Components
        protected AudioSource audioSource;

        protected virtual void Awake() {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && interactSound != null) {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // 3D sound
            }
        }

        /// <summary>
        /// Execute the interaction
        /// </summary>
        public virtual void Interact() {
            if (isDisabled)
                return;

            // Play sound
            if (audioSource != null && interactSound != null) {
                audioSource.PlayOneShot(interactSound);
            }

            // Spawn VFX
            if (interactVFX != null) {
                Instantiate(interactVFX, transform.position, Quaternion.identity);
            }

            // Execute interaction effect
            ExecuteInteractionEffect();

            // Mark as interacted
            isInteracted = true;

            // Handle one-time use
            if (oneTimeUse) {
                isDisabled = true;
            }
        }

        /// <summary>
        /// Check if interaction is currently possible
        /// </summary>
        public virtual bool CanInteract() {
            if (isDisabled)
                return false;

            // Check if player has required key
            if (requiresKey) {
                // Check if player has the key (simplified implementation)
                // In a real implementation, this would check the player's inventory
                bool hasKey = false;

                // For now, just check if any key item is in the scene
                KeyItem[] keyItems = FindObjectsOfType<KeyItem>();
                foreach (var key in keyItems) {
                    if (key.KeyName == requiredKeyName && key.IsCollected) {
                        hasKey = true;
                        break;
                    }
                }

                if (!hasKey)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get the text prompt for this interaction
        /// </summary>
        public virtual string GetInteractionPrompt() {
            if (requiresKey && !HasRequiredKey()) {
                return $"Requires {requiredKeyName}";
            }

            return interactionPrompt;
        }

        /// <summary>
        /// Get the time required for this interaction (0 for instant)
        /// </summary>
        public virtual float GetInteractionTime() {
            return interactionTime;
        }

        /// <summary>
        /// Execute the specific effect of this interactive element
        /// </summary>
        protected abstract void ExecuteInteractionEffect();

        /// <summary>
        /// Check if player has the required key
        /// </summary>
        protected virtual bool HasRequiredKey() {
            if (!requiresKey)
                return true;

            // Simplified key check logic
            KeyItem[] keyItems = FindObjectsOfType<KeyItem>();
            foreach (var key in keyItems) {
                if (key.KeyName == requiredKeyName && key.IsCollected) {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Interface for objects that receive activation signals from environmental interactions
    /// </summary>
    public interface IActivatable {
        void Activate();
        void Deactivate();
        bool IsActive { get; }
    }

    // todo: Implement IActivatable in other classes
    public class KeyItem : Item {
        [Header("Key Item Settings")]
        public string KeyName;
        public bool IsCollected;
    }
}