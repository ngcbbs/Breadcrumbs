using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Traps
{
    /// <summary>
    /// Base abstract class for all trap types
    /// </summary>
    public abstract class TrapSystem : MonoBehaviour, IDamageDealer
    {
        [Header("Base Trap Settings")]
        [SerializeField] protected float activationDelay = 0.5f;
        [SerializeField] protected float cooldownTime = 3f;
        [SerializeField] protected float baseDamage = 15f;
        [SerializeField] protected bool oneTimeUse = false;
        [SerializeField] protected LayerMask targetLayers;
        [SerializeField] protected DamageType damageType = DamageType.Physical;
        
        [Header("Audio Visual")]
        [SerializeField] protected AudioClip activationSound;
        [SerializeField] protected AudioClip triggerSound;
        [SerializeField] protected GameObject activationVFX;
        
        [Header("Detection")]
        [SerializeField] protected TrapDetectionType detectionType = TrapDetectionType.Proximity;
        
        // State tracking
        protected bool isActivated = false;
        protected bool isTriggered = false;
        protected bool inCooldown = false;
        protected bool isDisabled = false;
        
        // Components
        protected AudioSource audioSource;
        
        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (activationSound != null || triggerSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // 3D sound
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Slight pitch variation
            }
        }
        
        /// <summary>
        /// Activate the trap with delay
        /// </summary>
        protected virtual IEnumerator ActivateTrap()
        {
            if (isActivated || inCooldown || isDisabled)
                yield break;
                
            isActivated = true;
            
            // Play activation sound and VFX
            if (audioSource != null && activationSound != null)
            {
                audioSource.PlayOneShot(activationSound);
            }
            
            if (activationVFX != null)
            {
                Instantiate(activationVFX, transform.position, Quaternion.identity);
            }
            
            // Wait for activation delay
            yield return new WaitForSeconds(activationDelay);
            
            // Trigger the trap effect
            TriggerTrap();
            
            // Handle cooldown or one-time use
            if (oneTimeUse)
            {
                isDisabled = true;
            }
            else
            {
                StartCoroutine(CooldownRoutine());
            }
        }
        
        /// <summary>
        /// Cooldown period after trap activation
        /// </summary>
        protected virtual IEnumerator CooldownRoutine()
        {
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
        /// Check if target should trigger the trap
        /// </summary>
        protected virtual bool ShouldTriggerFor(GameObject target)
        {
            return (targetLayers.value & (1 << target.layer)) != 0;
        }
        
        /// <summary>
        /// Deal damage to a target
        /// </summary>
        public virtual void DealDamage(IDamageable target, float amount, DamageType type = DamageType.None)
        {
            // Use default damage type if not specified
            DamageType damageTypeToUse = (type == DamageType.None) ? this.damageType : type;
            
            // Apply damage to target
            target.TakeDamage(amount, damageTypeToUse, this.gameObject);
        }
        
        /// <summary>
        /// Draw gizmos for debugging
        /// </summary>
        protected virtual void OnDrawGizmosSelected() 
        {
            // Override in subclasses
        }
    }
    
    /// <summary>
    /// Enum for different trap detection types
    /// </summary>
    public enum TrapDetectionType
    {
        Proximity,     // Detect targets in range
        Pressure,      // Activated when stepped on
        TimeBased,     // Activate at intervals
        Remote,        // Activated by another object (lever, etc.)
        LookBased      // Activated when seen/looked at
    }
}