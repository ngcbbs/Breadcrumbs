using GamePortfolio.Gameplay.Combat;
using UnityEngine;

namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// AI controller for ranged enemies
    /// </summary>
    public class RangedEnemyAI : BaseEnemyAI {
        [Header("Ranged-Specific Settings")]
        [SerializeField]
        private float preferredCombatDistance = 6f;
        [SerializeField]
        private float minDistanceFromTarget = 3f;
        [SerializeField]
        private GameObject projectilePrefab;
        [SerializeField]
        private Transform firePoint;
        [SerializeField]
        private float projectileSpeed = 15f;
        [SerializeField]
        private float projectileDamage = 10f;
        [SerializeField]
        private bool canStrafe = true;

        // Strafing variables
        private Vector3 strafeDirection;
        private float strafeTimer;
        private float strafeDuration;

        protected override void Awake() {
            base.Awake();

            // Register with AI Manager
            AIManager.Instance.RegisterEnemy(this);

            // Set default fire point if not assigned
            if (firePoint == null)
                firePoint = transform;
        }

        protected override void SetupStates() {
            base.SetupStates();

            // Override chase state with maintain distance for ranged enemies
            availableStates[EnemyStateType.Chase] = new MaintainDistanceState(this, preferredCombatDistance);

            // Override attack state with ranged attack
            availableStates[EnemyStateType.Attack] = new RangedAttackState(this);
        }

        /// <summary>
        /// Fire a projectile at the target
        /// </summary>
        public void FireProjectile() {
            if (target == null || projectilePrefab == null)
                return;

            // Create projectile and set its properties
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile projectileComponent = projectile.GetComponent<Projectile>();

            if (projectileComponent != null) {
                // Calculate lead position for moving targets
                Vector3 targetPosition = CalculateLeadPosition();

                // Set projectile properties
                projectileComponent.Initialize(gameObject, targetPosition, projectileSpeed, projectileDamage);
            } else {
                // Simple physics-based projectile if no Projectile component
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null) {
                    Vector3 direction = (target.position - firePoint.position).normalized;
                    rb.velocity = direction * projectileSpeed;

                    // Destroy after time if no other logic
                    Destroy(projectile, 5f);
                }
            }
        }

        /// <summary>
        /// Calculate a lead position for moving targets
        /// </summary>
        private Vector3 CalculateLeadPosition() {
            Rigidbody targetRb = target.GetComponent<Rigidbody>();

            if (targetRb != null) {
                // Target has physics - predict movement
                float distanceToTarget = Vector3.Distance(firePoint.position, target.position);
                float timeToReach = distanceToTarget / projectileSpeed;

                // Predict position based on current velocity
                return target.position + targetRb.velocity * timeToReach;
            }

            return target.position;
        }

        /// <summary>
        /// Begin strafing movement
        /// </summary>
        public void StartStrafing() {
            if (!canStrafe)
                return;

            // Choose left or right strafing
            strafeDirection = Quaternion.Euler(0, Random.Range(0, 2) == 0 ? 90 : -90, 0) *
                              (transform.position - target.position).normalized;

            strafeDuration = Random.Range(1.5f, 3f);
            strafeTimer = 0f;
        }

        /// <summary>
        /// Update strafing movement
        /// </summary>
        public void UpdateStrafing() {
            if (!canStrafe || target == null)
                return;

            strafeTimer += Time.deltaTime;

            if (strafeTimer <= strafeDuration) {
                // Calculate strafe position while maintaining distance
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = transform.position + strafeDirection;

                // Ensure we don't get too close to target
                float distanceToTarget = Vector3.Distance(targetPosition, target.position);
                if (distanceToTarget < minDistanceFromTarget) {
                    // Adjust position to maintain minimum distance
                    targetPosition = target.position - directionToTarget * minDistanceFromTarget;
                }

                MoveToPosition(targetPosition);

                // Look at target while strafing
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            }
        }

        /// <summary>
        /// Ranged attack state implementation
        /// </summary>
        private class RangedAttackState : AttackState {
            private RangedEnemyAI rangedOwner;
            private bool hasFired;
            private bool isStrafing;

            public RangedAttackState(BaseEnemyAI owner) : base(owner) {
                this.rangedOwner = owner as RangedEnemyAI;
            }

            public override void EnterState() {
                base.EnterState();
                hasFired = false;
                isStrafing = false;

                // Stop movement to fire
                owner.StopMovement();

                // Face target
                if (owner.Target != null) {
                    Vector3 lookDirection = owner.Target.position - owner.transform.position;
                    lookDirection.y = 0;
                    owner.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }

            public override void UpdateState() {
                if (!hasFired && rangedOwner.CanAttack()) {
                    // Fire projectile
                    rangedOwner.FireProjectile();
                    hasFired = true;

                    // Start strafing after firing
                    if (rangedOwner.canStrafe) {
                        rangedOwner.StartStrafing();
                        isStrafing = true;
                        owner.ResumeMovement();
                    }
                }

                // Update strafing movement
                if (isStrafing) {
                    rangedOwner.UpdateStrafing();
                }
            }

            public override EnemyStateType? CheckTransitions() {
                // Return to maintain distance state after firing and optional strafing
                if (hasFired && (!isStrafing || rangedOwner.strafeTimer > rangedOwner.strafeDuration)) {
                    return EnemyStateType.Chase; // This is MaintainDistance for ranged enemies
                }

                // If target is too close, retreat
                float distanceToTarget = Vector3.Distance(owner.transform.position, owner.Target.position);
                if (distanceToTarget < rangedOwner.minDistanceFromTarget) {
                    return EnemyStateType.Retreat;
                }

                return null;
            }
        }

        private void OnDestroy() {
            // Unregister from AI Manager
            if (AIManager.Instance != null) {
                AIManager.Instance.UnregisterEnemy(this);
            }
        }
    }

    /// <summary>
    /// Simple projectile class for ranged attacks
    /// </summary>
    public class Projectile : MonoBehaviour {
        private GameObject owner;
        private float speed;
        private float damage;
        private Vector3 targetPosition;
        private bool isInitialized;

        // Optional components
        private TrailRenderer trail;
        private Light lightComponent;

        private void Awake() {
            trail = GetComponent<TrailRenderer>();
            lightComponent = GetComponent<Light>();
        }

        /// <summary>
        /// Initialize the projectile
        /// </summary>
        public void Initialize(GameObject owner, Vector3 targetPosition, float speed, float damage) {
            this.owner = owner;
            this.targetPosition = targetPosition;
            this.speed = speed;
            this.damage = damage;

            isInitialized = true;

            // Calculate direction to target
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Set rotation to face direction
            transform.forward = direction;

            // Destroy after time to prevent orphaned objects
            Destroy(gameObject, 5f);
        }

        private void Update() {
            if (!isInitialized)
                return;

            // Move projectile
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other) {
            // Skip collision with owner
            if (other.gameObject == owner)
                return;

            // Deal damage to damageable entities
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(damage);
            }

            // Play hit effect
            PlayHitEffect();

            // Destroy projectile
            Destroy(gameObject);
        }

        private void PlayHitEffect() {
            // Simple effect - can be enhanced with particle systems
            if (trail != null) {
                trail.enabled = false;
            }

            if (lightComponent != null) {
                lightComponent.enabled = false;
            }
        }
    }
}