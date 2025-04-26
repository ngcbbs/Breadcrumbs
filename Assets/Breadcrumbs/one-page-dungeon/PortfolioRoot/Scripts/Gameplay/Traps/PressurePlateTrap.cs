using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Traps
{
    /// <summary>
    /// A pressure plate trap that activates when stepped on
    /// </summary>
    public class PressurePlateTrap : TrapSystem
    {
        [Header("Pressure Plate Settings")]
        [SerializeField] private Vector3 triggerBoxSize = new Vector3(1f, 0.2f, 1f);
        [SerializeField] private float resetDistance = 1f;
        [SerializeField] private float plateDepression = 0.1f;
        [SerializeField] private float damageRadius = 1.5f;
        
        private Vector3 originalPosition;
        private Transform pressedTarget;
        
        protected override void Awake()
        {
            base.Awake();
            originalPosition = transform.position;
            detectionType = TrapDetectionType.Pressure;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isActivated && !inCooldown && !isDisabled && ShouldTriggerFor(other.gameObject))
            {
                pressedTarget = other.transform;
                VisuallyDepressPlate();
                StartCoroutine(ActivateTrap());
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (pressedTarget == other.transform)
            {
                pressedTarget = null;
                VisuallyRaisePlate();
            }
        }
        
        private void VisuallyDepressPlate()
        {
            // Animate the plate being pressed down
            transform.position = new Vector3(
                transform.position.x,
                originalPosition.y - plateDepression,
                transform.position.z
            );
        }
        
        private void VisuallyRaisePlate()
        {
            // Only raise if we're not in activation/cooldown
            if (!isActivated)
            {
                transform.position = originalPosition;
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
            
            // Apply damage to targets in area
            Collider[] hitColliders = Physics.OverlapSphere(
                transform.position + Vector3.up * triggerBoxSize.y,
                damageRadius,
                targetLayers
            );
            
            foreach (var hitCollider in hitColliders)
            {
                // Apply damage to damageable entities
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    DealDamage(damageable, baseDamage);
                }
            }
        }
        
        protected override IEnumerator CooldownRoutine()
        {
            yield return base.CooldownRoutine();
            
            // Raise the plate after cooldown
            if (pressedTarget == null)
            {
                VisuallyRaisePlate();
            }
        }
        
        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                transform.position + Vector3.up * triggerBoxSize.y * 0.5f,
                triggerBoxSize
            );
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                transform.position + Vector3.up * triggerBoxSize.y,
                damageRadius
            );
        }
    }
}