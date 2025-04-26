using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// Base combat system that can be extended for player and enemy implementations
    /// </summary>
    public abstract class CombatSystem : MonoBehaviour, IDamageDealer {
        [Header("Attack Settings")]
        [SerializeField]
        protected float baseDamage = 10f;
        [SerializeField]
        protected float attackRange = 2f;
        [SerializeField]
        protected float attackCooldown = 1f;
        [SerializeField]
        protected float attackAngle = 60f;
        [SerializeField]
        protected DamageType damageType = DamageType.Physical;
        [SerializeField]
        protected LayerMask targetLayers;

        [Header("Effects")]
        [SerializeField]
        protected AudioClip attackSound;
        [SerializeField]
        protected GameObject attackVFX;
        [SerializeField]
        protected bool showAttackGizmos = true;

        // getter
        public float BaseDamage {
            get { return baseDamage; }
            set { baseDamage = value; }
        }
        public float AttackCooldown {
            get { return attackCooldown; }
            set { attackCooldown = value; }
        }
        public float AttackRange {
            get { return attackRange; }
            set { attackRange = value; }
        }
        public DamageType DamageType {
            get { return damageType; }
            set { damageType = value; }
        }
        public GameObject AttackVFX {
            get { return attackVFX; }
            set { attackVFX = value; }
        }
        public AudioClip AttackSound {
            get { return attackSound; }
            set { attackSound = value; }
        }

        // Events
        [Header("Events")]
        public UnityEvent<GameObject> OnAttackPerformed;
        public UnityEvent<GameObject, float> OnDamageDealt;
        public UnityEvent OnAttackStarted;
        public UnityEvent OnAttackEnded;

        // Internal state
        protected float lastAttackTime = -999f;
        protected bool isAttacking = false;

        // Components
        protected Animator animator;
        protected AudioSource audioSource;

        protected virtual void Awake() {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null) {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // 3D sound
            }
        }

        /// <summary>
        /// Check if attack is off cooldown
        /// </summary>
        public bool CanAttack() {
            return Time.time >= lastAttackTime + attackCooldown && !isAttacking;
        }

        /// <summary>
        /// Perform a basic attack
        /// </summary>
        public virtual void PerformAttack() {
            if (!CanAttack())
                return;

            isAttacking = true;
            lastAttackTime = Time.time;

            // Start attack animation
            if (animator != null) {
                animator.SetTrigger("Attack");
            }

            // Attack started event
            OnAttackStarted?.Invoke();

            // Play attack sound
            if (audioSource != null && attackSound != null) {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(attackSound);
            }

            // Spawn attack VFX
            if (attackVFX != null) {
                Instantiate(attackVFX, transform.position, transform.rotation);
            }

            // Handle actual hit detection
            // This is called here for instant attacks, but can be called by animation events for sync with animations
            HandleHitDetection();

            // Schedule end of attack
            StartCoroutine(EndAttackAfterDelay(attackCooldown * 0.7f));
        }

        /// <summary>
        /// End attack state after delay
        /// </summary>
        protected IEnumerator EndAttackAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);

            isAttacking = false;
            OnAttackEnded?.Invoke();
        }

        /// <summary>
        /// Detect hits in the attack area
        /// </summary>
        protected virtual void HandleHitDetection() {
            // Get potential targets in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, targetLayers);
            HashSet<GameObject> hitTargets = new HashSet<GameObject>();

            foreach (var hitCollider in hitColliders) {
                // Skip if we already hit this gameobject
                if (hitTargets.Contains(hitCollider.gameObject))
                    continue;

                // Check if target is in attack angle
                Vector3 directionToTarget = hitCollider.transform.position - transform.position;
                directionToTarget.y = 0; // Ignore height difference

                float angle = Vector3.Angle(transform.forward, directionToTarget);

                if (angle <= attackAngle * 0.5f) {
                    // Check line of sight
                    if (HasLineOfSight(hitCollider.transform)) {
                        // Get IDamageable interface
                        IDamageable target = hitCollider.GetComponent<IDamageable>();

                        if (target != null && target.IsAlive()) {
                            // Deal damage
                            DealDamage(target, CalculateDamage());

                            // Track hit targets
                            hitTargets.Add(hitCollider.gameObject);

                            // Attack performed event
                            OnAttackPerformed?.Invoke(hitCollider.gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if there's a clear line of sight to the target
        /// </summary>
        protected bool HasLineOfSight(Transform target) {
            Vector3 directionToTarget = target.position - transform.position;

            // Raycast to check for obstacles
            if (Physics.Raycast(transform.position, directionToTarget.normalized,
                    out RaycastHit hit, directionToTarget.magnitude)) {
                // Check if we hit the target or something else
                if (hit.transform != target) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calculate damage amount for an attack
        /// </summary>
        protected virtual float CalculateDamage() {
            // Basic implementation - can be overridden for more complex calculations
            return baseDamage * Random.Range(0.9f, 1.1f); // 10% random variance
        }

        /// <summary>
        /// Deal damage to a target
        /// </summary>
        public virtual void DealDamage(IDamageable target, float amount, DamageType type = DamageType.None) {
            // Use default damage type if not specified
            DamageType damageTypeToUse = (type == DamageType.None) ? this.damageType : type;

            // Apply damage to target
            target.TakeDamage(amount, damageTypeToUse, this.gameObject);

            // Damage dealt event
            OnDamageDealt?.Invoke(target is MonoBehaviour behaviour ? behaviour.gameObject : null, amount);
        }

        /// <summary>
        /// Get the current cooldown percentage
        /// </summary>
        public float GetCooldownPercentage() {
            if (Time.time < lastAttackTime + attackCooldown) {
                return (Time.time - lastAttackTime) / attackCooldown;
            }

            return 1f;
        }

        /// <summary>
        /// Draw attack range gizmos
        /// </summary>
        protected virtual void OnDrawGizmosSelected() {
            if (!showAttackGizmos)
                return;

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw attack angle
            Gizmos.color = Color.yellow;
            Vector3 rightDir = Quaternion.Euler(0, attackAngle * 0.5f, 0) * transform.forward;
            Vector3 leftDir = Quaternion.Euler(0, -attackAngle * 0.5f, 0) * transform.forward;

            Gizmos.DrawRay(transform.position, rightDir * attackRange);
            Gizmos.DrawRay(transform.position, leftDir * attackRange);

            // Draw an arc approximation
            int segments = 20;
            Vector3 prevPos = transform.position + rightDir * attackRange;

            for (int i = 1; i <= segments; i++) {
                float angle = attackAngle * (0.5f - i / (float)segments);
                Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 pos = transform.position + dir * attackRange;

                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }
    }
}