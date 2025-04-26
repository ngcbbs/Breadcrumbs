using System.Collections;
using UnityEngine;

namespace GamePortfolio.Gameplay.Environment {
    /// <summary>
    /// A platform that can move between waypoints, either on a timer or when activated
    /// </summary>
    public class MovablePlatform : MonoBehaviour, IActivatable {
        [Header("Movement Settings")]
        [SerializeField]
        private Transform[] waypoints;
        [SerializeField]
        private float movementSpeed = 2f;
        [SerializeField]
        private float waitTime = 1f;
        [SerializeField]
        private bool loopMovement = true;
        [SerializeField]
        private bool startActive = true;
        [SerializeField]
        private bool pingPongMovement = false;
        [SerializeField]
        private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Physics")]
        [SerializeField]
        private float gravity = 20f;
        [SerializeField]
        private LayerMask passengerLayers;
        [SerializeField]
        private bool useKinematic = true;

        // Components
        private Rigidbody platformRigidbody;

        // Movement tracking
        private int currentWaypointIndex = 0;
        private int direction = 1;
        private bool isMoving = false;
        private bool isActive = false;
        private Coroutine movementCoroutine;

        // Cached values
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 velocity;
        private Vector3 lastPosition;

        private void Awake() {
            platformRigidbody = GetComponent<Rigidbody>();

            // Configure rigidbody
            if (platformRigidbody != null) {
                platformRigidbody.isKinematic = useKinematic;
                platformRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }

            // Cache initial values
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            lastPosition = transform.position;

            isActive = startActive;
        }

        private void Start() {
            // Start movement if active
            if (isActive && waypoints != null && waypoints.Length > 0) {
                StartMovement();
            }
        }

        private void FixedUpdate() {
            // Calculate platform velocity for passenger movement
            if (isMoving) {
                velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;

                // Apply platform movement to passengers
                MovePassengers();
            } else {
                velocity = Vector3.zero;
            }

            lastPosition = transform.position;
        }

        /// <summary>
        /// Move any entities standing on the platform
        /// </summary>
        private void MovePassengers() {
            // Get all colliders in box above platform
            Bounds platformBounds = GetComponent<Collider>().bounds;
            Vector3 boxCenter = platformBounds.center + Vector3.up * 0.1f;
            Vector3 boxSize = new Vector3(platformBounds.size.x, 0.2f, platformBounds.size.z);

            Collider[] hitColliders = Physics.OverlapBox(boxCenter, boxSize * 0.5f, Quaternion.identity, passengerLayers);

            foreach (var hitCollider in hitColliders) {
                // Skip if this is the platform itself
                if (hitCollider.transform == transform)
                    continue;

                // Apply platform velocity to character controllers
                CharacterController controller = hitCollider.GetComponent<CharacterController>();
                if (controller != null) {
                    controller.Move(velocity * Time.fixedDeltaTime);
                    continue;
                }

                // Apply platform velocity to rigidbodies
                Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic) {
                    // For rigidbodies, add velocity
                    Vector3 rbVelocity = rb.linearVelocity;

                    // Only modify horizontal movement, let gravity handle vertical
                    rbVelocity.x = velocity.x;
                    rbVelocity.z = velocity.z;

                    // If platform is moving up, also move the rigidbody up
                    if (velocity.y > 0) {
                        rbVelocity.y = Mathf.Max(velocity.y, rbVelocity.y);
                    }

                    rb.linearVelocity = rbVelocity;
                }
            }
        }

        /// <summary>
        /// Start platform movement
        /// </summary>
        private void StartMovement() {
            if (waypoints == null || waypoints.Length < 2)
                return;

            // Stop any existing movement
            if (movementCoroutine != null) {
                StopCoroutine(movementCoroutine);
            }

            movementCoroutine = StartCoroutine(MovePlatform());
        }

        /// <summary>
        /// Move the platform between waypoints
        /// </summary>
        private IEnumerator MovePlatform() {
            isMoving = true;

            // If we haven't started yet, move to first waypoint
            if (currentWaypointIndex == 0 && transform.position != waypoints[0].position) {
                yield return StartCoroutine(MoveToWaypoint(0));
            }

            while (isActive) {
                // Calculate next waypoint
                int nextWaypointIndex = currentWaypointIndex + direction;

                // Check for loop conditions
                if (nextWaypointIndex >= waypoints.Length) {
                    if (pingPongMovement) {
                        direction = -1;
                        nextWaypointIndex = waypoints.Length - 2;
                    } else if (loopMovement) {
                        nextWaypointIndex = 0;
                    } else {
                        break; // End movement
                    }
                } else if (nextWaypointIndex < 0) {
                    if (pingPongMovement) {
                        direction = 1;
                        nextWaypointIndex = 1;
                    } else if (loopMovement) {
                        nextWaypointIndex = waypoints.Length - 1;
                    } else {
                        break; // End movement
                    }
                }

                // Move to next waypoint
                yield return StartCoroutine(MoveToWaypoint(nextWaypointIndex));

                // Wait at waypoint
                yield return new WaitForSeconds(waitTime);
            }

            isMoving = false;
            movementCoroutine = null;
        }

        /// <summary>
        /// Move the platform to a specific waypoint
        /// </summary>
        private IEnumerator MoveToWaypoint(int waypointIndex) {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            Vector3 targetPosition = waypoints[waypointIndex].position;
            Quaternion targetRotation = waypoints[waypointIndex].rotation;

            float journeyLength = Vector3.Distance(startPosition, targetPosition);
            float journeyTime = journeyLength / movementSpeed;

            float elapsedTime = 0;

            while (elapsedTime < journeyTime) {
                elapsedTime += Time.deltaTime;
                float percent = Mathf.Clamp01(elapsedTime / journeyTime);

                // Apply easing curve
                float easedPercent = movementCurve.Evaluate(percent);

                // Set position and rotation
                transform.position = Vector3.Lerp(startPosition, targetPosition, easedPercent);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, easedPercent);

                yield return null;
            }

            // Ensure final position is exact
            transform.position = targetPosition;
            transform.rotation = targetRotation;

            // Update current waypoint
            currentWaypointIndex = waypointIndex;
        }

        /// <summary>
        /// Activate the platform movement
        /// </summary>
        public void Activate() {
            isActive = true;

            if (!isMoving && waypoints != null && waypoints.Length > 0) {
                StartMovement();
            }
        }

        /// <summary>
        /// Deactivate the platform movement
        /// </summary>
        public void Deactivate() {
            isActive = false;

            // Platform will stop after completing current movement
            // If immediate stop is desired, uncomment:
            /*
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
                movementCoroutine = null;
                isMoving = false;
            }
            */
        }

        /// <summary>
        /// Reset platform to initial position
        /// </summary>
        public void ResetToStart() {
            // Stop movement
            if (movementCoroutine != null) {
                StopCoroutine(movementCoroutine);
                movementCoroutine = null;
            }

            isMoving = false;

            // Reset position
            transform.position = initialPosition;
            transform.rotation = initialRotation;

            // Reset waypoint index
            currentWaypointIndex = 0;
            direction = 1;

            // Restart if active
            if (isActive) {
                StartMovement();
            }
        }

        /// <summary>
        /// Get platform active state
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// Start movement from a specific waypoint
        /// </summary>
        public void StartFromWaypoint(int waypointIndex) {
            if (waypoints == null || waypointIndex >= waypoints.Length)
                return;

            // Stop existing movement
            if (movementCoroutine != null) {
                StopCoroutine(movementCoroutine);
            }

            // Set position
            transform.position = waypoints[waypointIndex].position;
            transform.rotation = waypoints[waypointIndex].rotation;

            // Update index
            currentWaypointIndex = waypointIndex;

            // Start movement
            if (isActive) {
                StartMovement();
            }
        }

        /// <summary>
        /// Draw waypoint gizmos
        /// </summary>
        private void OnDrawGizmos() {
            if (waypoints == null || waypoints.Length == 0)
                return;

            // Draw waypoints
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++) {
                if (waypoints[i] != null) {
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                    // Draw lines between waypoints
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null) {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    } else if (i == waypoints.Length - 1 && loopMovement && waypoints[0] != null) {
                        // Connect last to first for loops
                        Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                    }
                }
            }

            // Draw platform size at each waypoint
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Collider collider = GetComponent<Collider>();
            if (collider != null) {
                Vector3 size = collider.bounds.size;
                foreach (var waypoint in waypoints) {
                    if (waypoint != null) {
                        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                            waypoint.position,
                            waypoint.rotation,
                            Vector3.one
                        );

                        Gizmos.matrix = rotationMatrix;
                        Gizmos.DrawCube(Vector3.zero, size);
                    }
                }
            }
        }
    }
}