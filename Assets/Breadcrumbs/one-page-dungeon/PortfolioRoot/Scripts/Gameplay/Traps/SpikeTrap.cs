using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Traps
{
    /// <summary>
    /// A spike trap that springs from the floor
    /// </summary>
    public class SpikeTrap : TrapSystem
    {
        [Header("Spike Settings")]
        [SerializeField] private float detectionRadius = 2.5f;
        [SerializeField] private float spikeHeight = 1.0f;
        [SerializeField] private float riseSpeed = 5.0f;
        [SerializeField] private float retractSpeed = 2.0f;
        [SerializeField] private float damageInterval = 0.5f;
        [SerializeField] private int maxDamageApplications = 3;
        [SerializeField] private bool applyBleedingEffect = true;
        
        [Header("Visuals")]
        [SerializeField] private Transform spikesObject;
        [SerializeField] private Vector3 spikesRetractedPosition = Vector3.zero;
        [SerializeField] private Vector3 spikesExtendedPosition = Vector3.up;
        
        // Tracking
        private int damageApplicationCount = 0;
        private float lastDamageTime = 0f;
        private Coroutine damageCoroutine = null;
        private HashSet<IDamageable> alreadyDamagedTargets = new HashSet<IDamageable>();
        
        protected override void Awake()
        {
            base.Awake();
            detectionType = TrapDetectionType.Proximity;
            
            // Set initial position
            if (spikesObject != null)
            {
                spikesObject.localPosition = spikesRetractedPosition;
            }
            
            damageType = DamageType.Physical;
        }
        
        private void Update()
        {
            if (!isDisabled && !isActivated && !inCooldown)
            {
                // Check for targets in detection radius
                Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, targetLayers);
                if (colliders.Length > 0)
                {
                    StartCoroutine(ActivateTrap());
                }
            }
        }
        
        protected override void TriggerTrap()
        {
            isTriggered = true;
            
            // Play trigger sound
            if (audioSource != null && triggerSound != null)
            {
                audioSource.PlayOneShot(triggerSound);
            }
            
            // Rise spikes
            StartCoroutine(MoveSpikes(spikesRetractedPosition, spikesExtendedPosition, riseSpeed, true));
            
            // Start damage routine
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
            
            damageCoroutine = StartCoroutine(DamageRoutine());
        }
        
        /// <summary>
        /// Animates the spikes between positions
        /// </summary>
        private IEnumerator MoveSpikes(Vector3 from, Vector3 to, float speed, bool extendingSpikes)
        {
            if (spikesObject == null)
                yield break;
                
            float distance = Vector3.Distance(from, to);
            float journeyLength = distance;
            float startTime = Time.time;
            float journeyTime = journeyLength / speed;
            float fractionComplete = 0f;
            
            while (fractionComplete < 1.0f)
            {
                // Calculate completion
                fractionComplete = (Time.time - startTime) / journeyTime;
                
                // Move spikes
                spikesObject.localPosition = Vector3.Lerp(from, to, fractionComplete);
                
                yield return null;
            }
            
            // Ensure precise final position
            spikesObject.localPosition = to;
            
            // If extending completed, schedule retraction
            if (extendingSpikes)
            {
                yield return new WaitForSeconds(1.5f);
                StartCoroutine(MoveSpikes(spikesExtendedPosition, spikesRetractedPosition, retractSpeed, false));
            }
        }
        
        /// <summary>
        /// Apply damage to entities in the spike area
        /// </summary>
        private IEnumerator DamageRoutine()
        {
            alreadyDamagedTargets.Clear();
            damageApplicationCount = 0;
            
            while (isTriggered && damageApplicationCount < maxDamageApplications)
            {
                // Detect victims in the spike area
                Collider[] victims = Physics.OverlapBox(
                    transform.position + Vector3.up * spikeHeight * 0.5f,
                    new Vector3(transform.localScale.x * 0.4f, spikeHeight * 0.5f, transform.localScale.z * 0.4f),
                    Quaternion.identity,
                    targetLayers
                );
                
                foreach (var victim in victims)
                {
                    IDamageable damageable = victim.GetComponent<IDamageable>();
                    if (damageable != null && damageable.IsAlive() && !alreadyDamagedTargets.Contains(damageable))
                    {
                        // Apply damage
                        DealDamage(damageable, baseDamage);
                        
                        // Apply bleeding if enabled
                        if (applyBleedingEffect)
                        {
                            StatusEffectManager effectManager = victim.GetComponent<StatusEffectManager>();
                            if (effectManager != null)
                            {
                                Debug.Log("todo:Applying bleeding effect");
                                //effectManager.ApplyEffect(StatusEffectType.Bleeding, 5f, baseDamage * 0.2f);
                            }
                        }
                        
                        // Track damaged targets
                        alreadyDamagedTargets.Add(damageable);
                    }
                }
                
                damageApplicationCount++;
                
                yield return new WaitForSeconds(damageInterval);
            }
            
            damageCoroutine = null;
        }
        
        protected override IEnumerator CooldownRoutine()
        {
            // Reset tracking
            alreadyDamagedTargets.Clear();
            damageApplicationCount = 0;
            
            return base.CooldownRoutine();
        }
        
        protected override void OnDrawGizmosSelected()
        {
            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Draw spike area
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                transform.position + Vector3.up * spikeHeight * 0.5f,
                new Vector3(transform.localScale.x * 0.8f, spikeHeight, transform.localScale.z * 0.8f)
            );
        }
    }
}