#if INCOMPLETE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Network.Synchronization;

namespace GamePortfolio.Network {
    /// <summary>
    /// Handles network synchronization and actions for a specific entity
    /// Acts as a bridge between network synchronization and actual game behavior
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkEntityController : MonoBehaviour {
        // TODO: 체크 네트워크 아이디와 동일하게 사용하는지 여부 체크 필요.
        public string EntityId => entityId;

        [Header("Entity Settings")]
        [SerializeField]
        private string entityId;
        [SerializeField]
        private bool isLocalPlayer;
        [SerializeField]
        private bool useRigidbody = true;

        [Header("Synchronization")]
        [SerializeField]
        private float smoothPosLerpRate = 15f;
        [SerializeField]
        private float smoothRotLerpRate = 15f;
        [SerializeField]
        private float positionTolerance = 0.05f;
        [SerializeField]
        private float rotationTolerance = 1f;

        [Header("Animation")]
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private string moveSpeedParameter = "MoveSpeed";
        [SerializeField]
        private string turnSpeedParameter = "TurnSpeed";

        // Components
        private Rigidbody rb;
        private NetworkSynchronizer networkSync;

        // Runtime state
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private Vector3 velocity;
        private float lastMoveTime;
        private Dictionary<string, object> lastActionState = new Dictionary<string, object>();

        // Animation tracking
        private float currentMoveSpeed;
        private float currentTurnSpeed;

        private void Awake() {
            rb = GetComponent<Rigidbody>();

            // Find animator if not set
            if (animator == null) {
                animator = GetComponentInChildren<Animator>();
            }

            // Generate entity ID if not set
            if (string.IsNullOrEmpty(entityId)) {
                entityId = System.Guid.NewGuid().ToString();
            }

            // Initialize target transform
            targetPosition = transform.position;
            targetRotation = transform.rotation;
        }

        private void Start() {
            // Get network synchronizer
            networkSync = NetworkSynchronizer.Instance;

            if (networkSync != null) {
                if (isLocalPlayer) {
                    // Register as local player
                    networkSync.SetLocalPlayer(entityId, transform);
                }

                // Register this entity for synchronization
                networkSync.RegisterPlayerObject(entityId, gameObject);
            }
        }

        private void OnDestroy() {
            // Unregister from network synchronizer
            if (networkSync != null) {
                networkSync.UnregisterPlayerObject(entityId);
            }
        }

        private void Update() {
            // Only update remote players or AI entities
            if (isLocalPlayer) return;

            // Smooth movement
            SmoothMoveToTarget();

            // Update animation
            UpdateAnimation();
        }

        /// <summary>
        /// Smoothly move to target position/rotation
        /// </summary>
        private void SmoothMoveToTarget() {
            // Skip if unchanged
            if (Vector3.Distance(transform.position, targetPosition) < positionTolerance &&
                Quaternion.Angle(transform.rotation, targetRotation) < rotationTolerance) {
                return;
            }

            // Apply movement based on synchronization type
            if (useRigidbody && rb != null) {
                // Use MovePosition/MoveRotation for physics-based entities
                rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, smoothPosLerpRate * Time.deltaTime));
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, smoothRotLerpRate * Time.deltaTime));
            } else {
                // Direct transform update for non-physics entities
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothPosLerpRate * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotLerpRate * Time.deltaTime);
            }

            // Calculate velocity for animation
            velocity = (targetPosition - transform.position) / Time.deltaTime;
            lastMoveTime = Time.time;
        }

        /// <summary>
        /// Update animation parameters
        /// </summary>
        private void UpdateAnimation() {
            if (animator == null) return;

            // Calculate movement parameters
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float moveSpeed = localVelocity.z;
            float turnSpeed = localVelocity.x;

            // Smooth animation parameters
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, Time.deltaTime * 8f);
            currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, turnSpeed, Time.deltaTime * 8f);

            // Apply to animator
            animator.SetFloat(moveSpeedParameter, currentMoveSpeed);
            animator.SetFloat(turnSpeedParameter, currentTurnSpeed);
        }

        /// <summary>
        /// Set the network entity ID
        /// </summary>
        public void SetEntityId(string id) {
            if (string.IsNullOrEmpty(id)) return;

            // Unregister old ID
            if (networkSync != null && !string.IsNullOrEmpty(entityId)) {
                networkSync.UnregisterPlayerObject(entityId);
            }

            // Set new ID
            entityId = id;

            // Register with new ID
            if (networkSync != null) {
                networkSync.RegisterPlayerObject(entityId, gameObject);

                if (isLocalPlayer) {
                    networkSync.SetLocalPlayer(entityId, transform);
                }
            }
        }

        /// <summary>
        /// Set whether this is the local player
        /// </summary>
        public void SetIsLocalPlayer(bool local) {
            isLocalPlayer = local;

            if (networkSync != null && isLocalPlayer) {
                networkSync.SetLocalPlayer(entityId, transform);
            }
        }

        /// <summary>
        /// Set target position and rotation for synchronization
        /// </summary>
        public void SetTargetTransform(Vector3 position, Quaternion rotation) {
            targetPosition = position;
            targetRotation = rotation;
        }

        /// <summary>
        /// Execute a network action
        /// </summary>
        public void ExecuteAction(string actionType, Dictionary<string, object> parameters) {
            if (string.IsNullOrEmpty(actionType))
                return;

            // Store last action state
            lastActionState["type"] = actionType;
            lastActionState["parameters"] = parameters;

            // Handle different action types
            switch (actionType) {
                case "Attack":
                    ExecuteAttackAction(parameters);
                    break;

                case "UseItem":
                    ExecuteUseItemAction(parameters);
                    break;

                case "Emote":
                    ExecuteEmoteAction(parameters);
                    break;

                case "Interact":
                    ExecuteInteractAction(parameters);
                    break;

                default:
                    Debug.LogWarning($"Unknown action type: {actionType}");
                    break;
            }
        }

        /// <summary>
        /// Execute an attack action
        /// </summary>
        private void ExecuteAttackAction(Dictionary<string, object> parameters) {
            if (parameters == null)
                return;

            // Example parameters:
            // - "attackType": "melee", "ranged", "spell"
            // - "targetId": entity ID of target
            // - "direction": attack direction

            if (parameters.TryGetValue("attackType", out object attackTypeObj) &&
                attackTypeObj is string attackType) {
                // Get direction if provided
                Vector3 direction = transform.forward;
                if (parameters.TryGetValue("direction", out object dirObj) &&
                    dirObj is Vector3 dir) {
                    direction = dir;
                }

                // Execute the appropriate attack type
                switch (attackType) {
                    case "melee":
                        // Trigger melee attack animation
                        if (animator != null) {
                            animator.SetTrigger("MeleeAttack");
                        }

                        // Play sound effect
                        if (AudioManager.HasInstance) {
                            AudioManager.Instance.PlaySfx("MeleeAttack");
                        }

                        break;

                    case "ranged":
                        // Trigger ranged attack animation
                        if (animator != null) {
                            animator.SetTrigger("RangedAttack");
                        }

                        // Play sound effect
                        if (AudioManager.HasInstance) {
                            AudioManager.Instance.PlaySfx("RangedAttack");
                        }

                        // Spawn projectile
                        if (parameters.TryGetValue("projectileType", out object projTypeObj) &&
                            projTypeObj is string projectileType) {
                            SpawnProjectile(projectileType, direction);
                        }

                        break;

                    case "spell":
                        // Trigger spell casting animation
                        if (animator != null) {
                            animator.SetTrigger("CastSpell");
                        }

                        // Play sound effect
                        if (AudioManager.HasInstance) {
                            AudioManager.Instance.PlaySfx("SpellCast");
                        }

                        // Spawn spell effect
                        if (parameters.TryGetValue("spellType", out object spellTypeObj) &&
                            spellTypeObj is string spellType) {
                            SpawnSpellEffect(spellType, direction);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Execute a use item action
        /// </summary>
        private void ExecuteUseItemAction(Dictionary<string, object> parameters) {
            if (parameters == null)
                return;

            // Example parameters:
            // - "itemId": ID of item being used
            // - "itemType": type of item (potion, scroll, etc.)

            if (parameters.TryGetValue("itemType", out object itemTypeObj) &&
                itemTypeObj is string itemType) {
                // Play use animation based on item type
                if (animator != null) {
                    switch (itemType) {
                        case "potion":
                            animator.SetTrigger("DrinkPotion");
                            break;

                        case "scroll":
                            animator.SetTrigger("ReadScroll");
                            break;

                        default:
                            animator.SetTrigger("UseItem");
                            break;
                    }
                }

                // Play sound effect
                if (AudioManager.HasInstance) {
                    string soundEffect = itemType == "potion" ? "DrinkPotion" :
                        itemType == "scroll" ? "ReadScroll" : "UseItem";
                    AudioManager.Instance.PlaySfx(soundEffect);
                }

                // Spawn relevant VFX if specified
                if (parameters.TryGetValue("effectType", out object effectTypeObj) &&
                    effectTypeObj is string effectType) {
                    SpawnItemUseEffect(effectType);
                }
            }
        }

        /// <summary>
        /// Execute an emote action
        /// </summary>
        private void ExecuteEmoteAction(Dictionary<string, object> parameters) {
            if (parameters == null || animator == null)
                return;

            // Example parameters:
            // - "emoteType": type of emote (wave, dance, etc.)

            if (parameters.TryGetValue("emoteType", out object emoteTypeObj) &&
                emoteTypeObj is string emoteType) {
                // Play emote animation
                switch (emoteType) {
                    case "wave":
                        animator.SetTrigger("Wave");
                        break;

                    case "dance":
                        animator.SetTrigger("Dance");
                        break;

                    case "bow":
                        animator.SetTrigger("Bow");
                        break;

                    case "cheer":
                        animator.SetTrigger("Cheer");
                        break;
                }

                // Play sound effect if applicable
                if (AudioManager.HasInstance) {
                    AudioManager.Instance.PlaySfx("Emote" + emoteType);
                }
            }
        }

        /// <summary>
        /// Execute an interact action
        /// </summary>
        private void ExecuteInteractAction(Dictionary<string, object> parameters) {
            if (parameters == null)
                return;

            // Example parameters:
            // - "interactableId": ID of object being interacted with
            // - "interactionType": type of interaction (open, pickup, etc.)

            if (animator != null) {
                animator.SetTrigger("Interact");
            }

            // Play sound effect if applicable
            if (AudioManager.HasInstance &&
                parameters.TryGetValue("interactionType", out object interactionTypeObj) &&
                interactionTypeObj is string interactionType) {
                AudioManager.Instance.PlaySfx("Interact" + interactionType);
            }
        }

        /// <summary>
        /// Spawn a projectile
        /// </summary>
        private void SpawnProjectile(string projectileType, Vector3 direction) {
            // This would instantiate a projectile prefab and set its direction
            // For now, we'll just log it
            Debug.Log($"Would spawn projectile of type {projectileType} in direction {direction}");
        }

        /// <summary>
        /// Spawn a spell effect
        /// </summary>
        private void SpawnSpellEffect(string spellType, Vector3 direction) {
            // This would instantiate a spell effect prefab
            // For now, we'll just log it
            Debug.Log($"Would spawn spell effect of type {spellType} in direction {direction}");
        }

        /// <summary>
        /// Spawn an item use effect
        /// </summary>
        private void SpawnItemUseEffect(string effectType) {
            // This would instantiate an item use effect
            // For now, we'll just log it
            Debug.Log($"Would spawn item use effect of type {effectType}");
        }
    }
}
#endif