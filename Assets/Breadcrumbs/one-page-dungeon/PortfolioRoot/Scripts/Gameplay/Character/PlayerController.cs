using System;
using System.Collections;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Network;

namespace GamePortfolio.Gameplay.Character {
    /// <summary>
    /// Controls the player character in the game world
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour {
        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 5f;
        [SerializeField]
        private float rotationSpeed = 10f;
        [SerializeField]
        private float jumpForce = 8f;
        [SerializeField]
        private float gravity = 20f;

        [Header("Ground Check")]
        [SerializeField]
        private float groundCheckDistance = 0.1f;
        [SerializeField]
        private LayerMask groundMask;

        [Header("Animation")]
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private string moveSpeedParameter = "MoveSpeed";
        [SerializeField]
        private string jumpParameter = "Jump";
        [SerializeField]
        private string groundedParameter = "Grounded";

        [Header("Network")]
        [SerializeField]
        private float positionSyncThreshold = 0.1f;
        [SerializeField]
        private float rotationSyncThreshold = 2f;
        [SerializeField]
        private float syncRate = 0.1f;

        // Component references
        private CharacterController characterController;
        private Camera playerCamera;

        // Movement variables
        private Vector3 moveDirection = Vector3.zero;
        private float verticalVelocity = 0f;
        private bool isJumping = false;

        // Network variables
        private Vector3 lastSyncedPosition;
        private Quaternion lastSyncedRotation;
        private float syncTimer = 0f;
        private bool isLocalPlayer = true;

        // Events
        public event Action<Vector3, Quaternion> OnPositionChanged;

        //
        private bool isInteracting = false;

        public void SetInteracting(bool interacting) {
            isInteracting = interacting;
        }

        private void Awake() {
            // Get component references
            characterController = GetComponent<CharacterController>();

            if (animator == null) {
                animator = GetComponentInChildren<Animator>();
            }

            playerCamera = Camera.main;
        }

        private void Start() {
            // Initialize last synced position/rotation
            lastSyncedPosition = transform.position;
            lastSyncedRotation = transform.rotation;

            // Subscribe to game state changes
            if (GameManager.HasInstance) {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy() {
            // Unsubscribe from game state changes
            if (GameManager.HasInstance) {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void Update() {
            if (!isLocalPlayer) return;

            // Only process input if game is in playing state
            if (GameManager.HasInstance && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            // Process player input
            ProcessInput();

            // Apply movement
            ApplyMovement();

            // Update animations
            UpdateAnimations();

            // Sync position with network if needed
            SyncPositionWithNetwork();
        }

        /// <summary>
        /// Process player input for movement, jumping, etc.
        /// </summary>
        private void ProcessInput() {
            // Get movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Calculate movement direction (relative to camera)
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;

            // Project vectors onto XZ plane
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calculate move direction
            moveDirection = (forward * vertical + right * horizontal).normalized;

            // Handle jumping
            if (IsGrounded() && Input.GetButtonDown("Jump")) {
                verticalVelocity = jumpForce;
                isJumping = true;

                // Trigger jump animation
                if (animator != null) {
                    animator.SetTrigger(jumpParameter);
                }
            }

            // Handle rotation - face movement direction
            if (moveDirection.magnitude > 0.1f) {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Apply movement to character controller
        /// </summary>
        private void ApplyMovement() {
            // Apply gravity
            if (!IsGrounded()) {
                verticalVelocity -= gravity * Time.deltaTime;
            } else if (verticalVelocity < 0) {
                // Small negative value when grounded to keep the character on the ground
                verticalVelocity = -2f;

                // Reset jumping flag
                if (isJumping) {
                    isJumping = false;
                }
            }

            // Calculate velocity with gravity
            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = verticalVelocity;

            // Move character
            characterController.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Update animation parameters
        /// </summary>
        private void UpdateAnimations() {
            if (animator != null) {
                // Update movement speed parameter
                animator.SetFloat(moveSpeedParameter, moveDirection.magnitude * moveSpeed);

                // Update grounded parameter
                animator.SetBool(groundedParameter, IsGrounded());
            }
        }

        /// <summary>
        /// Sync position with network if moved significantly
        /// </summary>
        private void SyncPositionWithNetwork() {
            syncTimer += Time.deltaTime;

            // Check if it's time to potentially sync
            if (syncTimer >= syncRate) {
                syncTimer = 0f;

                // Check if position/rotation has changed enough to sync
                float positionDiff = Vector3.Distance(transform.position, lastSyncedPosition);
                float rotationDiff = Quaternion.Angle(transform.rotation, lastSyncedRotation);

                if (positionDiff > positionSyncThreshold || rotationDiff > rotationSyncThreshold) {
                    // Update last synced values
                    lastSyncedPosition = transform.position;
                    lastSyncedRotation = transform.rotation;

                    // Notify about position change
                    OnPositionChanged?.Invoke(transform.position, transform.rotation);

                    // Send position update to network
                    SyncPositionToNetwork();
                }
            }
        }

        /// <summary>
        /// Send position update to network
        /// </summary>
        private void SyncPositionToNetwork() {
            if (NetworkManager.HasInstance && NetworkManager.Instance.IsConnected) {
                // Use Task instead of async void to avoid compiler warning
                _ = NetworkManager.Instance.UpdatePositionAsync(transform.position, transform.rotation);
            }
        }

        /// <summary>
        /// Check if the character is grounded
        /// </summary>
        /// <returns>True if grounded</returns>
        private bool IsGrounded() {
            // Slightly raise the position to avoid issues with uneven ground
            Vector3 position = transform.position + Vector3.up * 0.1f;

            // Use Physics.Raycast for more precise ground check
            return Physics.Raycast(position, Vector3.down, groundCheckDistance + 0.1f, groundMask);
        }

        /// <summary>
        /// Set whether this is the local player
        /// </summary>
        /// <param name="isLocal">Is this the local player</param>
        public void SetIsLocalPlayer(bool isLocal) {
            isLocalPlayer = isLocal;

            // Disable camera for non-local players
            if (!isLocal && playerCamera != null) {
                playerCamera.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set the player's position and rotation (for remote players)
        /// </summary>
        /// <param name="position">New position</param>
        /// <param name="rotation">New rotation</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) {
            if (isLocalPlayer) return;

            // Set position directly if non-local player
            transform.position = position;
            transform.rotation = rotation;

            // Update animation based on movement
            if (animator != null) {
                Vector3 velocity = (position - lastSyncedPosition) / syncRate;
                animator.SetFloat(moveSpeedParameter, velocity.magnitude);
            }

            lastSyncedPosition = position;
            lastSyncedRotation = rotation;
        }

        /// <summary>
        /// Handle game state changes
        /// </summary>
        /// <param name="newState">New game state</param>
        private void OnGameStateChanged(GameState newState) {
            // Handle player behavior changes based on game state
            switch (newState) {
                case GameState.Paused:
                    // Disable input during pause
                    enabled = false;
                    break;

                case GameState.Playing:
                    // Enable input during play
                    enabled = true;
                    break;

                case GameState.GameOver:
                    // Disable input during game over
                    enabled = false;
                    break;
            }
        }
    }
}