using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Traps
{
    /// <summary>
    /// A pendulum blade trap that swings on a fixed path
    /// </summary>
    public class PendulumTrap : TrapSystem
    {
        [Header("Pendulum Settings")]
        [SerializeField] private float swingAngle = 90f;
        [SerializeField] private float swingPeriod = 3f;
        [SerializeField] private Transform pendulumPivot;
        [SerializeField] private Transform bladeObject;
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;
        [SerializeField] private float bladeLength = 3f;
        [SerializeField] private float hitboxWidth = 0.5f;
        
        [Header("Blade Settings")]
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
        [SerializeField] private float idleSwingSpeedMultiplier = 0.3f;
        [SerializeField] private float activatedSwingSpeedMultiplier = 1.5f;
        
        // State tracking
        private float currentAngle = 0f;
        private float previousAngle = 0f;
        private float timeOffset = 0f;
        private bool swingDirection = true; // true = positive direction, false = negative
        private float currentSpeedMultiplier = 0.3f;
        
        // Cached variables
        private Quaternion initialRotation;
        private HashSet<IDamageable> hitEntitiesThisSwing = new HashSet<IDamageable>();
        
        protected override void Awake()
        {
            base.Awake();
            detectionType = TrapDetectionType.TimeBased;
            
            // Store initial rotation
            if (pendulumPivot != null)
            {
                initialRotation = pendulumPivot.localRotation;
                
                // Randomize starting position
                timeOffset = Random.Range(0f, swingPeriod);
                currentSpeedMultiplier = idleSwingSpeedMultiplier;
            }
        }
        
        private void Update()
        {
            if (pendulumPivot == null || isDisabled)
                return;
                
            // Calculate swing angle
            float time = (Time.time + timeOffset) % swingPeriod;
            float normalizedTime = time / swingPeriod;
            
            // Use cosine function for pendulum motion
            float swingFactor = Mathf.Cos(normalizedTime * Mathf.PI * 2) * currentSpeedMultiplier;
            currentAngle = swingFactor * swingAngle;
            
            // Determine direction change
            bool currentDirection = currentAngle > previousAngle;
            if (currentDirection != swingDirection)
            {
                swingDirection = currentDirection;
                hitEntitiesThisSwing.Clear(); // Reset hit tracking when direction changes
            }
            
            previousAngle = currentAngle;
            
            // Rotate the pendulum
            pendulumPivot.localRotation = initialRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
            
            // Check for hits if activated
            if (isActivated)
            {
                CheckForHits();
            }
        }
        
        private void CheckForHits()
        {
            if (bladeObject == null)
                return;
                
            // Create a capsule collider that follows the blade's area
            Vector3 bladeTip = bladeObject.position + bladeObject.forward * bladeLength;
            Vector3 bladeBase = bladeObject.position;
            
            Collider[] hitColliders = Physics.OverlapCapsule(
                bladeBase, 
                bladeTip, 
                hitboxWidth,
                targetLayers
            );
            
            // Calculate blade speed for damage scaling
            float angularSpeed = Mathf.Abs(currentAngle - previousAngle) / Time.deltaTime;
            float speedFactor = Mathf.Clamp01(angularSpeed / 90f); // Normalize to expected max speed
            
            foreach (var hitCollider in hitColliders)
            {
                IDamageable target = hitCollider.GetComponent<IDamageable>();
                
                // Only damage each target once per swing
                if (target != null && target.IsAlive() && !hitEntitiesThisSwing.Contains(target))
                {
                    // Calculate damage based on speed
                    float speedBasedDamage = baseDamage * (0.5f + 0.5f * speedFactor);
                    
                    // Apply damage
                    DealDamage(target, speedBasedDamage);
                    
                    // Apply knockback
                    Rigidbody targetRb = hitCollider.GetComponent<Rigidbody>();
                    if (targetRb != null)
                    {
                        // Calculate knockback direction based on blade direction
                        Vector3 knockbackDir = bladeObject.right * (swingDirection ? 1 : -1);
                        targetRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                    }
                    
                    // Add to hit tracking
                    hitEntitiesThisSwing.Add(target);
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
            
            // Increase swing speed
            currentSpeedMultiplier = activatedSwingSpeedMultiplier;
            
            // Clear hit entity tracking
            hitEntitiesThisSwing.Clear();
        }
        
        /// <summary>
        /// Handle trap activation when something enters the trigger area
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!isActivated && !inCooldown && !isDisabled && ShouldTriggerFor(other.gameObject))
            {
                StartCoroutine(ActivateTrap());
            }
        }
        
        protected override IEnumerator CooldownRoutine()
        {
            yield return base.CooldownRoutine();
            
            // Return to idle speed
            currentSpeedMultiplier = idleSwingSpeedMultiplier;
        }
        
        protected override void OnDrawGizmosSelected()
        {
            if (pendulumPivot == null || bladeObject == null)
                return;
                
            // Draw pivot point
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pendulumPivot.position, 0.3f);
            
            // Draw swing arc
            Gizmos.color = Color.red;
            
            // Calculate points of the arc
            int segments = 20;
            
            // Draw arc showing range of motion
            Quaternion originalRot = pendulumPivot.rotation;
            Vector3 originalPos = bladeObject.position;
            
            // Draw blade at initial position
            Gizmos.DrawRay(originalPos, bladeObject.forward * bladeLength);
            
            // Draw blade at maximum positive swing
            pendulumPivot.rotation = originalRot * Quaternion.AngleAxis(swingAngle, rotationAxis);
            Gizmos.DrawRay(bladeObject.position, bladeObject.forward * bladeLength);
            
            // Draw blade at maximum negative swing
            pendulumPivot.rotation = originalRot * Quaternion.AngleAxis(-swingAngle, rotationAxis);
            Gizmos.DrawRay(bladeObject.position, bladeObject.forward * bladeLength);
            
            // Reset to original rotation
            pendulumPivot.rotation = originalRot;
            
            // Draw hitbox area
            Gizmos.color = Color.blue;
            Debug.Log("Drawing hitbox type DrawWireCapsule");
            //Gizmos.DrawWireCapsule(originalPos, originalPos + bladeObject.forward * bladeLength, hitboxWidth, 0);
        }
    }
}