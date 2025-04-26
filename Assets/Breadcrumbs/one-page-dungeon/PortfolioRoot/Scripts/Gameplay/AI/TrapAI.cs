using System.Collections;
using GamePortfolio.Gameplay.Combat;
using UnityEngine;

namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// Base class for trap AI behavior
    /// </summary>
    public abstract class TrapAI : MonoBehaviour {
        [Header("Trap Settings")]
        [SerializeField]
        protected float activationDelay = 0.5f;
        [SerializeField]
        protected float cooldownTime = 3f;
        [SerializeField]
        protected float damageAmount = 15f;
        [SerializeField]
        protected bool oneTimeUse = false;
        [SerializeField]
        protected LayerMask targetLayers;

        [Header("Audio Visual")]
        [SerializeField]
        protected AudioClip activationSound;
        [SerializeField]
        protected AudioClip triggerSound;
        [SerializeField]
        protected GameObject activationVFX;

        // State tracking
        protected bool isActivated = false;
        protected bool isTriggered = false;
        protected bool inCooldown = false;
        protected bool isDisabled = false;

        // Components
        protected AudioSource audioSource;

        protected virtual void Awake() {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (activationSound != null || triggerSound != null)) {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f;              // 3D sound
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Slight pitch variation
            }
        }

        /// <summary>
        /// Activate the trap with delay
        /// </summary>
        protected virtual IEnumerator ActivateTrap() {
            if (isActivated || inCooldown || isDisabled)
                yield break;

            isActivated = true;

            // Play activation sound and VFX
            if (audioSource != null && activationSound != null) {
                audioSource.PlayOneShot(activationSound);
            }

            if (activationVFX != null) {
                Instantiate(activationVFX, transform.position, Quaternion.identity);
            }

            // Wait for activation delay
            yield return new WaitForSeconds(activationDelay);

            // Trigger the trap effect
            TriggerTrap();

            // Handle cooldown or one-time use
            if (oneTimeUse) {
                isDisabled = true;
            } else {
                StartCoroutine(CooldownRoutine());
            }
        }

        /// <summary>
        /// Cooldown period after trap activation
        /// </summary>
        protected virtual IEnumerator CooldownRoutine() {
            inCooldown = true;
            yield return new WaitForSeconds(cooldownTime);
            inCooldown = false;
            isActivated = false;
            isTriggered = false;
        }

        /// <summary>
        /// Apply the trap's effect
        /// </summary>
        protected abstract void TriggerTrap();

        /// <summary>
        /// Draw gizmos for debugging
        /// </summary>
        protected virtual void OnDrawGizmosSelected() {
            // Draw activation area - override in subclasses
        }
    }

    /// <summary>
    /// A pressure plate trap that activates when stepped on
    /// </summary>
    public class PressurePlateTrap : TrapAI {
        [Header("Pressure Plate Settings")]
        [SerializeField]
        private Vector3 triggerBoxSize = new Vector3(1f, 0.2f, 1f);
        [SerializeField]
        private float resetDistance = 1f;
        [SerializeField]
        private float plateDepression = 0.1f;

        private Vector3 originalPosition;
        private Transform pressedTarget;

        protected override void Awake() {
            base.Awake();
            originalPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other) {
            if (!isActivated && !inCooldown && !isDisabled &&
                (targetLayers.value & (1 << other.gameObject.layer)) != 0) {
                pressedTarget = other.transform;
                VisuallyDepressPlate();
                StartCoroutine(ActivateTrap());
            }
        }

        private void OnTriggerExit(Collider other) {
            if (pressedTarget == other.transform) {
                pressedTarget = null;
                VisuallyRaisePlate();
            }
        }

        private void VisuallyDepressPlate() {
            // Animate the plate being pressed down
            transform.position = new Vector3(
                transform.position.x,
                originalPosition.y - plateDepression,
                transform.position.z
            );
        }

        private void VisuallyRaisePlate() {
            // Only raise if we're not in activation/cooldown
            if (!isActivated) {
                transform.position = originalPosition;
            }
        }

        protected override void TriggerTrap() {
            isTriggered = true;

            // Play trigger sound
            if (audioSource != null && triggerSound != null) {
                audioSource.PlayOneShot(triggerSound);
            }

            // Apply damage to targets in area
            Collider[] hitColliders = Physics.OverlapBox(
                transform.position + Vector3.up * triggerBoxSize.y * 0.5f,
                triggerBoxSize * 0.5f,
                Quaternion.identity,
                targetLayers
            );

            foreach (var hitCollider in hitColliders) {
                // Apply damage to damageable entities
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null) {
                    damageable.TakeDamage(damageAmount);
                }
            }
        }

        protected override IEnumerator CooldownRoutine() {
            yield return base.CooldownRoutine();

            // Raise the plate after cooldown
            if (pressedTarget == null) {
                VisuallyRaisePlate();
            }
        }

        protected override void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                transform.position + Vector3.up * triggerBoxSize.y * 0.5f,
                triggerBoxSize
            );
        }
    }

    /// <summary>
    /// A dart trap that fires projectiles when triggered
    /// </summary>
    public class DartTrap : TrapAI {
        [Header("Dart Trap Settings")]
        [SerializeField]
        private float detectionRadius = 3f;
        [SerializeField]
        private int dartCount = 3;
        [SerializeField]
        private float dartSpeed = 10f;
        [SerializeField]
        private float fireRate = 0.2f;
        [SerializeField]
        private GameObject dartPrefab;
        [SerializeField]
        private Transform[] firePoints;

        private void OnTriggerEnter(Collider other) {
            if (!isActivated && !inCooldown && !isDisabled &&
                (targetLayers.value & (1 << other.gameObject.layer)) != 0) {
                StartCoroutine(ActivateTrap());
            }
        }

        protected override void TriggerTrap() {
            isTriggered = true;

            // Play trigger sound
            if (audioSource != null && triggerSound != null) {
                audioSource.PlayOneShot(triggerSound);
            }

            // Start firing darts
            StartCoroutine(FireDarts());
        }

        /// <summary>
        /// Fire multiple darts in sequence
        /// </summary>
        private IEnumerator FireDarts() {
            for (int i = 0; i < dartCount; i++) {
                FireSingleDart();
                yield return new WaitForSeconds(fireRate);
            }
        }

        /// <summary>
        /// Fire a single dart
        /// </summary>
        private void FireSingleDart() {
            // Choose a random fire point if multiple are available
            Transform firePoint = (firePoints != null && firePoints.Length > 0)
                ? firePoints[Random.Range(0, firePoints.Length)]
                : transform;

            if (dartPrefab != null) {
                GameObject dart = Instantiate(dartPrefab, firePoint.position, firePoint.rotation);
                Rigidbody dartRb = dart.GetComponent<Rigidbody>();

                if (dartRb != null) {
                    // Apply forward force
                    dartRb.velocity = firePoint.forward * dartSpeed;
                }

                // Set up damage component
                DartProjectile dartComponent = dart.GetComponent<DartProjectile>();
                if (dartComponent == null) {
                    dartComponent = dart.AddComponent<DartProjectile>();
                }

                dartComponent.Initialize(gameObject, damageAmount);

                // Destroy after time
                Destroy(dart, 5f);
            }
        }

        protected override void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Draw fire directions
            Gizmos.color = Color.yellow;
            if (firePoints != null && firePoints.Length > 0) {
                foreach (var point in firePoints) {
                    if (point != null) {
                        Gizmos.DrawRay(point.position, point.forward * 3f);
                    }
                }
            } else {
                Gizmos.DrawRay(transform.position, transform.forward * 3f);
            }
        }
    }

    /// <summary>
    /// Simple dart projectile script
    /// </summary>
    public class DartProjectile : MonoBehaviour {
        private GameObject owner;
        private float damage;
        private bool hasHit = false;

        public void Initialize(GameObject owner, float damage) {
            this.owner = owner;
            this.damage = damage;
        }

        private void OnTriggerEnter(Collider other) {
            if (hasHit || other.gameObject == owner)
                return;

            hasHit = true;

            // Apply damage
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(damage);
            }

            // Stick to the surface
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) {
                rb.isKinematic = true;
            }

            // Attach to hit object if possible
            transform.SetParent(other.transform, true);

            // Destroy after time
            Destroy(gameObject, 10f);
        }
    }
}