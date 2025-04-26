using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Network.GameHub;

namespace GamePortfolio.Network.Synchronization {
    /// <summary>
    /// Component that handles network synchronization for an entity
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkEntityController : MonoBehaviour {
        // Network identity
        [Header("Network Identity")]
        [SerializeField]
        private string networkId;
        [SerializeField]
        private bool isLocalPlayer;

        // Sync data
        [Header("Synchronization")]
        [SerializeField]
        private NetworkSyncMode syncMode = NetworkSyncMode.Transform;
        [SerializeField]
        private float syncPositionThreshold = 0.05f;
        [SerializeField]
        private float syncRotationThreshold = 1.0f;
        [SerializeField]
        private float syncVelocityThreshold = 0.1f;

        // Smoothing settings
        [Header("Movement Smoothing")]
        [SerializeField]
        private float positionLerpSpeed = 15f;
        [SerializeField]
        private float rotationLerpSpeed = 15f;
        [SerializeField]
        private float extrapolationMultiplier = 1.2f;

        // References
        private Rigidbody rb;
        private Animator animator;

        // Synchronization state
        [HideInInspector]
        public Vector3 StartPosition;
        [HideInInspector]
        public Quaternion StartRotation;
        [HideInInspector]
        public Vector3 TargetPosition;
        [HideInInspector]
        public Quaternion TargetRotation;
        [HideInInspector]
        public float InterpolationTime;
        [HideInInspector]
        public float InterpolationDuration = 0.1f;
        [HideInInspector]
        public bool HasSyncData;

        // Last synced values
        [HideInInspector]
        public Vector3 LastSyncedPosition;
        [HideInInspector]
        public Quaternion LastSyncedRotation;
        [HideInInspector]
        public Vector3 LastSyncedVelocity;

        // Network action handlers
        private Dictionary<ActionType, Action<PlayerAction, ActionResult>> actionHandlers =
            new Dictionary<ActionType, Action<PlayerAction, ActionResult>>();

        // Events
        public event Action<PlayerAction, ActionResult> OnActionPerformed;

        // Properties
        public string NetworkId => networkId;
        public bool IsLocalPlayer => isLocalPlayer;

        private void Awake() {
            // Get components
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();

            // Initialize sync values
            LastSyncedPosition = transform.position;
            LastSyncedRotation = transform.rotation;
            if (rb != null) {
                LastSyncedVelocity = rb.velocity;
            }

            // Register default action handlers
            RegisterDefaultActionHandlers();
        }

        private void Start() {
            // Generate network ID if not set
            if (string.IsNullOrEmpty(networkId)) {
                networkId = System.Guid.NewGuid().ToString();
            }

            // Register with network synchronizer
            NetworkSynchronizer.Instance?.RegisterEntity(this);
        }

        private void OnDestroy() {
            // Unregister from network synchronizer
            if (NetworkSynchronizer.Instance != null) {
                NetworkSynchronizer.Instance.UnregisterEntity(networkId);
            }
        }

        private void FixedUpdate() {
            // Only apply rigidbody synchronization for remote entities in Rigidbody mode
            if (!isLocalPlayer && syncMode == NetworkSyncMode.Rigidbody && HasSyncData) {
                ApplyRigidbodySynchronization();
            }
        }

        /// <summary>
        /// Apply rigidbody-based synchronization
        /// </summary>
        private void ApplyRigidbodySynchronization() {
            if (rb != null) {
                // Calculate position error
                float positionError = Vector3.Distance(rb.position, TargetPosition);

                // If error is large, teleport
                if (positionError > syncPositionThreshold * 10) {
                    rb.position = TargetPosition;
                    rb.rotation = TargetRotation;
                } else {
                    // Otherwise use velocity-based correction
                    Vector3 direction = (TargetPosition - rb.position);
                    float distanceToTarget = direction.magnitude;

                    if (distanceToTarget > syncPositionThreshold) {
                        // Calculate velocity to reach target
                        Vector3 targetVelocity = direction.normalized * ((distanceToTarget / InterpolationDuration) * extrapolationMultiplier);

                        // Limit maximum correction velocity
                        float maxVelocityMagnitude = rb.velocity.magnitude + 5f;
                        if (targetVelocity.magnitude > maxVelocityMagnitude) {
                            targetVelocity = targetVelocity.normalized * maxVelocityMagnitude;
                        }

                        // Apply velocity
                        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * positionLerpSpeed);
                    }

                    // Apply rotation
                    rb.rotation = Quaternion.Slerp(rb.rotation, TargetRotation,
                        Time.fixedDeltaTime * rotationLerpSpeed);
                }
            }
        }

        /// <summary>
        /// Register default action handlers for common action types
        /// </summary>
        private void RegisterDefaultActionHandlers() {
            // Attack action handler
            actionHandlers[ActionType.Attack] = (action, result) => {
                if (animator != null) {
                    animator.SetTrigger("Attack");
                }

                // Play attack effects/sounds
                PlayActionEffects(action);
            };

            // UseItem action handler
            actionHandlers[ActionType.UseItem] = (action, result) => {
                if (animator != null) {
                    animator.SetTrigger("UseItem");
                }

                // Play item use effects/sounds
                PlayActionEffects(action);
            };

            // CastSkill action handler
            actionHandlers[ActionType.CastSkill] = (action, result) => {
                if (animator != null && action.Parameters != null &&
                    action.Parameters.TryGetValue("skillId", out object skillIdObj)) {
                    string skillId = skillIdObj.ToString();
                    animator.SetTrigger("CastSkill");
                    animator.SetInteger("SkillID", int.Parse(skillId));
                }

                // Play skill effects/sounds
                PlayActionEffects(action);
            };

            // Interact action handler
            actionHandlers[ActionType.Interact] = (action, result) => {
                if (animator != null) {
                    animator.SetTrigger("Interact");
                }

                // Play interaction effects/sounds
                PlayActionEffects(action);
            };
        }

        /// <summary>
        /// Register a custom action handler
        /// </summary>
        /// <param name="actionType">Type of action to handle</param>
        /// <param name="handler">Handler function</param>
        public void RegisterActionHandler(ActionType actionType, Action<PlayerAction, ActionResult> handler) {
            actionHandlers[actionType] = handler;
        }

        /// <summary>
        /// Called when a network action is received for this entity
        /// </summary>
        /// <param name="action">Action data</param>
        /// <param name="result">Action result</param>
        public void OnNetworkActionReceived(PlayerAction action, ActionResult result) {
            // Try to find a handler for this action type
            if (actionHandlers.TryGetValue(action.Type, out var handler)) {
                handler.Invoke(action, result);
            }

            // Notify listeners
            OnActionPerformed?.Invoke(action, result);
        }

        /// <summary>
        /// Perform an action that will be synchronized over the network
        /// </summary>
        /// <param name="actionType">Type of action</param>
        /// <param name="targetId">Target entity ID (optional)</param>
        /// <param name="position">Target position (optional)</param>
        /// <param name="parameters">Additional parameters (optional)</param>
        public async void PerformNetworkAction(ActionType actionType, string targetId = null,
            Vector3? position = null, Dictionary<string, object> parameters = null) {
            // Only local player can initiate network actions
            if (!isLocalPlayer)
                return;

            // Create action
            PlayerAction action = new PlayerAction {
                Type = actionType,
                TargetId = targetId,
                Position = position.GetValueOrDefault(transform.position),
                Parameters = parameters ?? new Dictionary<string, object>()
            };

            // Send to server
            try {
                if (NetworkSynchronizer.Instance != null) {
                    ActionResult result = await NetworkSynchronizer.Instance.SendActionAsync(action);

                    if (result.Success) {
                        // Apply locally as well
                        OnNetworkActionReceived(action, result);
                    } else {
                        Debug.LogWarning($"Network action failed: {result.Message}");
                    }
                }
            } catch (Exception ex) {
                Debug.LogError($"Error performing network action: {ex.Message}");
            }
        }

        /// <summary>
        /// Play effects for an action based on parameters
        /// </summary>
        /// <param name="action">Action data</param>
        private void PlayActionEffects(PlayerAction action) {
            if (action.Parameters == null)
                return;

            // Check for VFX prefab
            if (action.Parameters.TryGetValue("vfxPrefab", out object vfxPrefabObj)) {
                string vfxPrefabPath = vfxPrefabObj.ToString();
                GameObject vfxPrefab = Resources.Load<GameObject>(vfxPrefabPath);

                if (vfxPrefab != null) {
                    // Determine spawn position
                    Vector3 spawnPos = transform.position;
                    if (action.Parameters.TryGetValue("vfxOffset", out object vfxOffsetObj) &&
                        vfxOffsetObj is Vector3 vfxOffset) {
                        spawnPos += transform.TransformDirection(vfxOffset);
                    }

                    // Instantiate VFX
                    GameObject vfxInstance = Instantiate(vfxPrefab, spawnPos, transform.rotation);

                    // Auto-destroy after timeout
                    if (action.Parameters.TryGetValue("vfxDuration", out object vfxDurationObj) &&
                        float.TryParse(vfxDurationObj.ToString(), out float vfxDuration)) {
                        Destroy(vfxInstance, vfxDuration);
                    } else {
                        Destroy(vfxInstance, 5f); // Default timeout
                    }
                }
            }

            // Check for SFX
            if (action.Parameters.TryGetValue("sfx", out object sfxObj)) {
                string sfxName = sfxObj.ToString();
                AudioClip clip = Resources.Load<AudioClip>(sfxName);

                if (clip != null) {
                    // Play sound using audio manager
                    // AudioManager.Instance?.PlaySound(clip, transform.position);

                    // Or play directly on AudioSource if available
                    AudioSource audioSource = GetComponent<AudioSource>();
                    if (audioSource != null) {
                        audioSource.PlayOneShot(clip);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sync mode for network entities
    /// </summary>
    public enum NetworkSyncMode {
        Transform, // Direct transform updates (for non-physics objects)
        Rigidbody  // Physics-based synchronization (for dynamic objects)
    }
}