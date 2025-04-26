using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Character;

namespace GamePortfolio.Gameplay.Interaction {
    /// <summary>
    /// Manages interactable objects in the game environment
    /// Provides a centralized system for interaction highlighting, prompts, and processing
    /// </summary>
    public class InteractableSystem : Singleton<InteractableSystem> {
        [Header("Interaction Settings")]
        [SerializeField]
        private float interactionRange = 3f;
        [SerializeField]
        private KeyCode interactionKey = KeyCode.E;
        [SerializeField]
        private LayerMask interactableLayers;

        [Header("UI References")]
        [SerializeField]
        private GameObject interactionPrompt;
        [SerializeField]
        private TMPro.TMP_Text promptText;

        [Header("Highlight Effect")]
        [SerializeField]
        private Material highlightMaterial;
        [SerializeField]
        private Color highlightColor = new Color(1f, 0.8f, 0.2f, 0.3f);
        [SerializeField]
        private float highlightPulseSpeed = 2f;
        [SerializeField]
        private float highlightIntensity = 0.5f;

        // Runtime state
        private IInteractable currentTarget;
        private GameObject highlightObject;
        private List<Renderer> highlightedRenderers = new List<Renderer>();
        private List<Material> originalMaterials = new List<Material>();
        private float highlightPulsePhase = 0f;
        private bool isInteracting = false;

        // Events
        public event Action<IInteractable> OnInteractionStarted;
        public event Action<IInteractable> OnInteractionCompleted;
        public event Action<IInteractable> OnInteractionCancelled;

        protected override void Awake() {
            base.Awake();

            // Hide prompt
            if (interactionPrompt != null) {
                interactionPrompt.SetActive(false);
            }

            // Set up highlight material if provided
            if (highlightMaterial != null) {
                highlightMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);
            }
        }

        private void Update() {
            // If player is actively interacting, don't check for new interactions
            if (isInteracting)
                return;

            // Get local player
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null)
                return;

            // Check for interactable objects in range
            CheckForInteractables(player.transform.position, player.transform.forward);

            // Process interaction input
            if (Input.GetKeyDown(interactionKey) && currentTarget != null) {
                StartInteraction(player);
            }

            // Update highlight effect
            UpdateHighlightEffect();
        }

        /// <summary>
        /// Check for interactable objects in range
        /// </summary>
        private void CheckForInteractables(Vector3 playerPosition, Vector3 playerForward) {
            // Cast a sphere to find potential interactables
            Collider[] colliders = Physics.OverlapSphere(playerPosition, interactionRange, interactableLayers);

            float closestDistance = float.MaxValue;
            IInteractable closestInteractable = null;
            GameObject closestObject = null;

            foreach (Collider collider in colliders) {
                // Skip if this is the player
                if (collider.CompareTag("Player"))
                    continue;

                // Check if object has an interactable component
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract()) {
                    // Check if in front of player (dot product > 0)
                    Vector3 directionToObject = (collider.transform.position - playerPosition).normalized;
                    float dotProduct = Vector3.Dot(playerForward, directionToObject);

                    if (dotProduct > 0.3f) // At least somewhat in front of player
                    {
                        // Calculate distance
                        float distance = Vector3.Distance(playerPosition, collider.transform.position);

                        // Keep track of closest
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestInteractable = interactable;
                            closestObject = collider.gameObject;
                        }
                    }
                }
            }

            // Update current target
            if (closestInteractable != currentTarget) {
                RemoveHighlight();
                currentTarget = closestInteractable;

                if (currentTarget != null) {
                    ApplyHighlight(closestObject);
                    ShowPrompt(currentTarget.GetInteractionPrompt());
                } else {
                    HidePrompt();
                }
            }
        }

        /// <summary>
        /// Start interaction with current target
        /// </summary>
        private void StartInteraction(PlayerController player) {
            if (currentTarget == null)
                return;

            isInteracting = true;
            HidePrompt();

            // Notify that interaction started
            OnInteractionStarted?.Invoke(currentTarget);

            // Begin interaction
            float interactionTime = currentTarget.GetInteractionTime();

            if (interactionTime > 0) {
                // Start interaction timer
                StartCoroutine(InteractionTimerRoutine(player, interactionTime));
            } else {
                // Instant interaction
                CompleteInteraction();
            }
        }

        /// <summary>
        /// Coroutine for timed interactions
        /// </summary>
        private IEnumerator InteractionTimerRoutine(PlayerController player, float duration) {
            Vector3 interactionStartPos = player.transform.position;
            float timeElapsed = 0;

            // Play interaction start sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("InteractionStart");
            }

            // Start interaction animation/state in player if available
            player.SetInteracting(true);

            // Show progress bar or other UI indicator
            ShowInteractionProgress(0);

            // Main interaction loop
            while (timeElapsed < duration) {
                // Check if player moved too far from initial position
                if (Vector3.Distance(player.transform.position, interactionStartPos) > 1f) {
                    // Cancel interaction
                    CancelInteraction();
                    player.SetInteracting(false);
                    yield break;
                }

                // Check if interaction was cancelled
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(interactionKey)) {
                    // Cancel interaction
                    CancelInteraction();
                    player.SetInteracting(false);
                    yield break;
                }

                // Update progress
                timeElapsed += Time.deltaTime;
                float progress = timeElapsed / duration;
                ShowInteractionProgress(progress);

                yield return null;
            }

            // Interaction complete
            CompleteInteraction();

            // End interaction animation/state in player
            player.SetInteracting(false);

            // Hide progress
            HideInteractionProgress();
        }

        /// <summary>
        /// Complete the current interaction
        /// </summary>
        private void CompleteInteraction() {
            if (currentTarget == null)
                return;

            // Execute interaction
            currentTarget.Interact();

            // Play sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("InteractionComplete");
            }

            // Notify completion
            OnInteractionCompleted?.Invoke(currentTarget);

            // Reset state
            isInteracting = false;
            currentTarget = null;
            RemoveHighlight();
        }

        /// <summary>
        /// Cancel the current interaction
        /// </summary>
        private void CancelInteraction() {
            if (currentTarget == null)
                return;

            // Play cancel sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("InteractionCancel");
            }

            // Notify cancellation
            OnInteractionCancelled?.Invoke(currentTarget);

            // Reset state
            isInteracting = false;
            currentTarget = null;
            RemoveHighlight();
            HideInteractionProgress();
        }

        /// <summary>
        /// Apply highlight effect to an object
        /// </summary>
        private void ApplyHighlight(GameObject target) {
            if (target == null || highlightMaterial == null)
                return;

            highlightObject = target;
            highlightedRenderers.Clear();
            originalMaterials.Clear();

            // Find all renderers in object and children
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers) {
                // Save original materials
                Material[] originalMats = renderer.materials;
                for (int i = 0; i < originalMats.Length; i++) {
                    originalMaterials.Add(originalMats[i]);
                }

                // Create new materials array with highlight material appended
                Material[] newMaterials = new Material[originalMats.Length + 1];
                Array.Copy(originalMats, newMaterials, originalMats.Length);
                newMaterials[originalMats.Length] = highlightMaterial;

                // Apply new materials
                renderer.materials = newMaterials;

                // Add to list of highlighted renderers
                highlightedRenderers.Add(renderer);
            }
        }

        /// <summary>
        /// Remove highlight effect
        /// </summary>
        private void RemoveHighlight() {
            if (highlightObject == null)
                return;

            // Restore original materials
            int materialIndex = 0;

            foreach (Renderer renderer in highlightedRenderers) {
                if (renderer != null) {
                    Material[] originalMats = renderer.materials;
                    Material[] newMaterials = new Material[originalMats.Length - 1];

                    for (int i = 0; i < originalMats.Length - 1; i++) {
                        newMaterials[i] = originalMaterials[materialIndex++];
                    }

                    renderer.materials = newMaterials;
                }
            }

            // Clear references
            highlightedRenderers.Clear();
            originalMaterials.Clear();
            highlightObject = null;
        }

        /// <summary>
        /// Update highlight pulse effect
        /// </summary>
        private void UpdateHighlightEffect() {
            if (highlightMaterial == null || highlightObject == null)
                return;

            // Update pulse phase
            highlightPulsePhase += Time.deltaTime * highlightPulseSpeed;

            // Calculate pulse intensity
            float pulseIntensity = highlightIntensity * (0.7f + 0.3f * Mathf.Sin(highlightPulsePhase));

            // Update highlight material
            highlightMaterial.SetColor("_EmissionColor", highlightColor * pulseIntensity);
        }

        /// <summary>
        /// Show interaction prompt with specified text
        /// </summary>
        private void ShowPrompt(string text) {
            if (interactionPrompt != null) {
                interactionPrompt.SetActive(true);

                if (promptText != null) {
                    promptText.text = text + $" <b>[{interactionKey}]</b>";
                }
            }
        }

        /// <summary>
        /// Hide interaction prompt
        /// </summary>
        private void HidePrompt() {
            if (interactionPrompt != null) {
                interactionPrompt.SetActive(false);
            }
        }

        /// <summary>
        /// Show interaction progress (for timed interactions)
        /// </summary>
        private void ShowInteractionProgress(float progress) {
            // This would update a progress bar or other UI element
            // For now, we'll just log it
            if (Debug.isDebugBuild) {
                Debug.Log($"Interaction progress: {progress * 100}%");
            }
        }

        /// <summary>
        /// Hide interaction progress UI
        /// </summary>
        private void HideInteractionProgress() {
            // This would hide the progress UI
        }

        /// <summary>
        /// Force interaction with a specific object
        /// </summary>
        public void ForceInteraction(GameObject interactableObject) {
            if (interactableObject == null)
                return;

            IInteractable interactable = interactableObject.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract()) {
                // Set as current target
                currentTarget = interactable;

                // Start interaction
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null) {
                    StartInteraction(player);
                } else {
                    // No player, just complete interaction directly
                    CompleteInteraction();
                }
            }
        }

        /// <summary>
        /// Check if any interactable is in range of the given position
        /// </summary>
        public bool IsInteractableInRange(Vector3 position, out IInteractable interactable) {
            interactable = null;

            Collider[] colliders = Physics.OverlapSphere(position, interactionRange, interactableLayers);

            foreach (Collider collider in colliders) {
                IInteractable potentialInteractable = collider.GetComponent<IInteractable>();
                if (potentialInteractable != null && potentialInteractable.CanInteract()) {
                    interactable = potentialInteractable;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the current interaction target
        /// </summary>
        public IInteractable GetCurrentTarget() {
            return currentTarget;
        }

        /// <summary>
        /// Check if player is currently interacting
        /// </summary>
        public bool IsInteractionInProgress() {
            return isInteracting;
        }

        private void OnDrawGizmos() {
            // Draw interaction range for debugging
            if (Debug.isDebugBuild) {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(player.transform.position, interactionRange);
                }
            }
        }
    }

    /// <summary>
    /// Interface for all interactable objects
    /// </summary>
    public interface IInteractable {
        /// <summary>
        /// Execute the interaction
        /// </summary>
        void Interact();

        /// <summary>
        /// Check if interaction is currently possible
        /// </summary>
        bool CanInteract();

        /// <summary>
        /// Get the text prompt for this interaction
        /// </summary>
        string GetInteractionPrompt();

        /// <summary>
        /// Get the time required for this interaction (0 for instant)
        /// </summary>
        float GetInteractionTime();
    }
}