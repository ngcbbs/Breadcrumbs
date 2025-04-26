using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Traps
{
    /// <summary>
    /// A dart trap that fires projectiles when triggered
    /// </summary>
    public class DartTrap : TrapSystem
    {
        [Header("Dart Trap Settings")]
        [SerializeField] private float detectionRadius = 3f;
        [SerializeField] private int dartCount = 3;
        [SerializeField] private float dartSpeed = 10f;
        [SerializeField] private float fireRate = 0.2f;
        [SerializeField] private GameObject dartPrefab;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private bool isHidden = false;
        [SerializeField] private GameObject hiddenVisuals;
        [SerializeField] private GameObject exposedVisuals;
        
        protected override void Awake()
        {
            base.Awake();
            detectionType = TrapDetectionType.Proximity;
            
            // Setup initial visual state
            if (hiddenVisuals != null)
                hiddenVisuals.SetActive(isHidden);
            if (exposedVisuals != null)
                exposedVisuals.SetActive(!isHidden);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isActivated && !inCooldown && !isDisabled && ShouldTriggerFor(other.gameObject))
            {
                StartCoroutine(ActivateTrap());
            }
        }
        
        protected override void TriggerTrap()
        {
            isTriggered = true;
            
            // Reveal trap if hidden
            if (isHidden)
            {
                if (hiddenVisuals != null)
                    hiddenVisuals.SetActive(false);
                if (exposedVisuals != null)
                    exposedVisuals.SetActive(true);
                isHidden = false;
            }
            
            // Play trigger sound
            if (audioSource != null && triggerSound != null)
            {
                audioSource.PlayOneShot(triggerSound);
            }
            
            // Start firing darts
            StartCoroutine(FireDarts());
        }
        
        /// <summary>
        /// Fire multiple darts in sequence
        /// </summary>
        private IEnumerator FireDarts()
        {
            for (int i = 0; i < dartCount; i++)
            {
                FireSingleDart();
                yield return new WaitForSeconds(fireRate);
            }
        }
        
        /// <summary>
        /// Fire a single dart
        /// </summary>
        private void FireSingleDart()
        {
            // Choose a random fire point if multiple are available
            Transform firePoint = (firePoints != null && firePoints.Length > 0) ? 
                                 firePoints[Random.Range(0, firePoints.Length)] : 
                                 transform;
                                 
            if (dartPrefab != null)
            {
                GameObject dart = Instantiate(dartPrefab, firePoint.position, firePoint.rotation);
                Rigidbody dartRb = dart.GetComponent<Rigidbody>();
                
                if (dartRb != null)
                {
                    // Apply forward force
                    dartRb.linearVelocity = firePoint.forward * dartSpeed;
                }
                
                // Set up damage component
                DartProjectile dartComponent = dart.GetComponent<DartProjectile>();
                if (dartComponent == null)
                {
                    dartComponent = dart.AddComponent<DartProjectile>();
                }
                
                dartComponent.Initialize(gameObject, baseDamage, damageType);
                
                // Destroy after time
                Destroy(dart, 5f);
            }
        }
        
        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Draw fire directions
            Gizmos.color = Color.yellow;
            if (firePoints != null && firePoints.Length > 0)
            {
                foreach (var point in firePoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawRay(point.position, point.forward * 3f);
                    }
                }
            }
            else
            {
                Gizmos.DrawRay(transform.position, transform.forward * 3f);
            }
        }
    }
    
    /// <summary>
    /// Projectile script for darts fired from traps
    /// </summary>
    public class DartProjectile : MonoBehaviour
    {
        private GameObject owner;
        private float damage;
        private bool hasHit = false;
        private DamageType damageType;
        
        public void Initialize(GameObject owner, float damage, DamageType damageType = DamageType.Physical)
        {
            this.owner = owner;
            this.damage = damage;
            this.damageType = damageType;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasHit || other.gameObject == owner)
                return;
                
            hasHit = true;
            
            // Apply damage
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, damageType, owner);
            }
            
            // Stick to the surface
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // Attach to hit object if possible
            transform.SetParent(other.transform, true);
            
            // Destroy after time
            Destroy(gameObject, 10f);
        }
    }
}