using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Network.Synchronization;
using GamePortfolio.Network.GameHub;

namespace GamePortfolio.Network.Authority {
    /// <summary>
    /// Client-side component to handle server authority validation results
    /// </summary>
    [RequireComponent(typeof(NetworkEntityController))]
    public class ClientAuthorityHandler : MonoBehaviour {
        // References
        private NetworkEntityController networkEntity;
        private NetworkManager networkManager;

        // Last known valid state for rollbacks
        private Vector3 lastValidPosition;
        private Quaternion lastValidRotation;
        private Vector3 lastValidVelocity;

        // Movement prediction
        [Header("Authority Settings")]
        [SerializeField]
        private bool enablePrediction = true;
        [SerializeField]
        private bool enableRollback = true;
        [SerializeField]
        private float rollbackBlendTime = 0.5f;

        // Current rollback blend
        private float currentBlendTime = 0f;
        private bool isRollingBack = false;
        private Vector3 rollbackStartPosition;
        private Quaternion rollbackStartRotation;

        // Validation flags
        private bool hasPositionJumpWarning = false;
        private bool hasSpeedHackWarning = false;
        private bool hasCooldownWarning = false;
        private int validationWarningCount = 0;

        private void Awake() {
            networkEntity = GetComponent<NetworkEntityController>();
        }

        private void Start() {
            // Get network manager
            networkManager = NetworkManager.Instance;

            if (networkManager != null && networkManager.HubReceiver != null) {
                // Register for validation events
                networkManager.OnValidationResultReceived += HandleValidationResult;
            }

            // Initialize last valid state
            lastValidPosition = transform.position;
            lastValidRotation = transform.rotation;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) {
                lastValidVelocity = rb.linearVelocity;
            }
        }

        private void Update() {
            // Handle rollback blending
            if (isRollingBack) {
                currentBlendTime += Time.deltaTime;
                float t = Mathf.Clamp01(currentBlendTime / rollbackBlendTime);

                // Blend position and rotation
                transform.position = Vector3.Lerp(rollbackStartPosition, lastValidPosition, t);
                transform.rotation = Quaternion.Slerp(rollbackStartRotation, lastValidRotation, t);

                // If blend complete, reset rollback
                if (t >= 1.0f) {
                    isRollingBack = false;
                }
            }
        }

        private void OnDestroy() {
            if (networkManager != null) {
                networkManager.OnValidationResultReceived -= HandleValidationResult;
            }
        }

        /// <summary>
        /// Handle a validation result from the server
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="result">Validation result</param>
        /// <param name="correctPosition">Correct position (if position-related)</param>
        /// <param name="correctRotation">Correct rotation (if position-related)</param>
        private void HandleValidationResult(string playerId, ValidationResult result,
            Vector3? correctPosition, Vector3? correctRotation) {
            // Only process for this entity
            Debug.Log("todo: fixme!!!");
            Debug.Log("TODO: if (networkEntity.NetworkId != playerId) check");
            #if false
            if (networkEntity.EntityId != playerId)
                return;

            switch (result) {
                case ValidationResult.Valid:
                    // Update last valid state if this was a valid update
                    if (correctPosition.HasValue) {
                        lastValidPosition = correctPosition.Value;
                    }

                    if (correctRotation.HasValue) {
                        lastValidRotation = Quaternion.Euler(correctRotation.Value);
                    }

                    // Reset flags
                    ResetWarningFlags();
                    break;

                case ValidationResult.PositionJump:
                    HandlePositionJumpViolation(correctPosition, correctRotation);
                    break;

                case ValidationResult.SpeedHack:
                    HandleSpeedHackViolation(correctPosition, correctRotation);
                    break;

                case ValidationResult.CooldownViolation:
                    HandleCooldownViolation();
                    break;

                case ValidationResult.RangeViolation:
                case ValidationResult.DamageHack:
                    // These are typically only handled server-side
                    Debug.LogWarning($"Received {result} validation result for {playerId}");
                    break;
            }
            #endif
        }

        /// <summary>
        /// Handle a position jump violation
        /// </summary>
        /// <param name="correctPosition">Correct position from server</param>
        /// <param name="correctRotation">Correct rotation from server</param>
        private void HandlePositionJumpViolation(Vector3? correctPosition, Vector3? correctRotation) {
            hasPositionJumpWarning = true;
            validationWarningCount++;

            Debug.LogWarning("Position jump violation detected");

            // If we have a correct position from server, roll back to it
            if (correctPosition.HasValue && enableRollback) {
                InitiateRollback(correctPosition.Value,
                    correctRotation.HasValue ? Quaternion.Euler(correctRotation.Value) : transform.rotation);
            }

            // If too many violations, show warning to user
            if (validationWarningCount >= 3) {
                ShowValidationWarning("Connection issues detected. Movement may be unstable.");
            }
        }

        /// <summary>
        /// Handle a speed hack violation
        /// </summary>
        /// <param name="correctPosition">Correct position from server</param>
        /// <param name="correctRotation">Correct rotation from server</param>
        private void HandleSpeedHackViolation(Vector3? correctPosition, Vector3? correctRotation) {
            hasSpeedHackWarning = true;
            validationWarningCount++;

            Debug.LogWarning("Speed hack violation detected");

            // If we have a correct position from server, roll back to it
            if (correctPosition.HasValue && enableRollback) {
                InitiateRollback(correctPosition.Value,
                    correctRotation.HasValue ? Quaternion.Euler(correctRotation.Value) : transform.rotation);
            }

            // If too many violations, show warning to user
            if (validationWarningCount >= 3) {
                ShowValidationWarning("Movement validation issues detected. Your actions may be delayed.");
            }
        }

        /// <summary>
        /// Handle a cooldown violation
        /// </summary>
        private void HandleCooldownViolation() {
            hasCooldownWarning = true;
            validationWarningCount++;

            Debug.LogWarning("Cooldown violation detected");

            // If too many violations, show warning to user
            if (validationWarningCount >= 3) {
                ShowValidationWarning("Action validation issues detected. Your actions may be delayed.");
            }
        }

        /// <summary>
        /// Initiate a position rollback
        /// </summary>
        /// <param name="targetPosition">Target position</param>
        /// <param name="targetRotation">Target rotation</param>
        private void InitiateRollback(Vector3 targetPosition, Quaternion targetRotation) {
            // Only rollback if significant position difference
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
                return;

            isRollingBack = true;
            currentBlendTime = 0f;
            rollbackStartPosition = transform.position;
            rollbackStartRotation = transform.rotation;
            lastValidPosition = targetPosition;
            lastValidRotation = targetRotation;

            // Immediately snap rigidbody if present
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) {
                rb.linearVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Reset warning flags
        /// </summary>
        private void ResetWarningFlags() {
            hasPositionJumpWarning = false;
            hasSpeedHackWarning = false;
            hasCooldownWarning = false;

            // Gradually reduce warning count
            if (validationWarningCount > 0 && Time.frameCount % 300 == 0) // Roughly every 5 seconds at 60fps
            {
                validationWarningCount--;
            }
        }

        /// <summary>
        /// Show a validation warning to the user
        /// </summary>
        /// <param name="message">Warning message</param>
        private void ShowValidationWarning(string message) {
            // In a real implementation, this would display a UI warning
            Debug.LogWarning($"VALIDATION WARNING: {message}");

            // Example of showing a notification in the UI
            try {
                Debug.Log("TODO: Show notification in UI");
                //UI.UIManager.Instance?.ShowNotification(message);
            } catch (Exception ex) {
                Debug.LogError($"Failed to show validation warning: {ex.Message}");
            }
        }
    }
}