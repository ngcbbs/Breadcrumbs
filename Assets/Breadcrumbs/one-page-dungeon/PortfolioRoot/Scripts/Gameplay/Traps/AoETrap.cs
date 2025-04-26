using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Traps
{
    /// <summary>
    /// A trap that deals area of effect damage (poison gas, fire, etc.)
    /// </summary>
    public class AoETrap : TrapSystem
    {
        [Header("AoE Settings")]
        [SerializeField] private float effectRadius = 4f;
        [SerializeField] private float activeDuration = 5f;
        [SerializeField] private float effectInterval = 0.5f;
        [SerializeField] private bool permanentEffect = false;
        [SerializeField] private StatusEffectType effectType = StatusEffectType.None;
        [SerializeField] private float effectDuration = 3f;
        [SerializeField] private float effectStrength = 5f;
        
        [Header("Effect Scaling")]
        [SerializeField] private float intensityRampUp = 1f; // How quickly the effect intensifies
        [SerializeField] private float finalIntensityMultiplier = 2f; // Maximum multiplier for damage/effect
        
        [Header("Visuals")]
        [SerializeField] private GameObject activeEffectVisuals;
        [SerializeField] private bool visualScaling = true;
        [SerializeField] private float maxVisualScale = 1.5f;
        [SerializeField] private Color initialEffectColor = Color.yellow;
        [SerializeField] private Color finalEffectColor = Color.red;
        
        // Tracking
        private Coroutine effectCoroutine;
        private float currentIntensity = 1f;
        private ParticleSystem[] particleSystems;
        private Light[] effectLights;
        
        protected override void Awake()
        {
            base.Awake();
            detectionType = TrapDetectionType.TimeBased;
            
            // Gather components for visual effects
            if (activeEffectVisuals != null)
            {
                particleSystems = activeEffectVisuals.GetComponentsInChildren<ParticleSystem>();
                effectLights = activeEffectVisuals.GetComponentsInChildren<Light>();
                
                // Disable initially
                activeEffectVisuals.SetActive(false);
            }
        }
        
        private void Start()
        {
            // For time-based traps, start a timer
            if (detectionType == TrapDetectionType.TimeBased && !oneTimeUse)
            {
                StartCoroutine(TimeBasedActivation());
            }
        }
        
        private IEnumerator TimeBasedActivation()
        {
            // Wait for initial delay before first activation
            yield return new WaitForSeconds(Random.Range(1f, 5f));
            
            while (!isDisabled)
            {
                // Activate trap
                StartCoroutine(ActivateTrap());
                
                // Wait for cooldown plus active duration
                yield return new WaitForSeconds(cooldownTime + activeDuration);
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
            
            // Show effect visuals
            if (activeEffectVisuals != null)
            {
                activeEffectVisuals.SetActive(true);
                
                if (visualScaling)
                {
                    activeEffectVisuals.transform.localScale = Vector3.one;
                }
            }
            
            // Start dealing damage
            if (effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
            }
            
            effectCoroutine = StartCoroutine(AoEEffectRoutine());
        }
        
        private IEnumerator AoEEffectRoutine()
        {
            currentIntensity = 1f;
            float elapsedTime = 0f;
            
            // Continue until duration expires or trap is permanent
            while (permanentEffect || elapsedTime < activeDuration)
            {
                // Calculate current intensity (ramps up over time)
                if (elapsedTime < intensityRampUp)
                {
                    currentIntensity = Mathf.Lerp(1f, finalIntensityMultiplier, elapsedTime / intensityRampUp);
                }
                else
                {
                    currentIntensity = finalIntensityMultiplier;
                }
                
                // Apply visual effects scaling
                UpdateVisuals(elapsedTime / activeDuration);
                
                // Find targets in radius
                Collider[] victims = Physics.OverlapSphere(transform.position, effectRadius, targetLayers);
                
                foreach (var victim in victims)
                {
                    IDamageable damageable = victim.GetComponent<IDamageable>();
                    
                    if (damageable != null && damageable.IsAlive())
                    {
                        // Apply damage scaled by current intensity
                        float scaledDamage = baseDamage * currentIntensity * effectInterval;
                        DealDamage(damageable, scaledDamage);
                        
                        // Apply status effect if enabled
                        if (effectType != StatusEffectType.None)
                        {
                            StatusEffectManager effectManager = victim.GetComponent<StatusEffectManager>();
                            if (effectManager != null)
                            {
                                Debug.Log("todo: apply effect");
                                //effectManager.ApplyEffect(effectType, effectDuration, effectStrength * currentIntensity);
                            }
                        }
                    }
                }
                
                elapsedTime += effectInterval;
                yield return new WaitForSeconds(effectInterval);
            }
            
            // Hide effect visuals if not permanent
            if (!permanentEffect && activeEffectVisuals != null)
            {
                activeEffectVisuals.SetActive(false);
            }
            
            effectCoroutine = null;
        }
        
        private void UpdateVisuals(float normalizedTime)
        {
            if (activeEffectVisuals != null)
            {
                // Scale visuals
                if (visualScaling)
                {
                    float currentScale = Mathf.Lerp(1f, maxVisualScale, normalizedTime);
                    activeEffectVisuals.transform.localScale = Vector3.one * currentScale;
                }
                
                // Update particle colors
                if (particleSystems != null)
                {
                    Color currentColor = Color.Lerp(initialEffectColor, finalEffectColor, normalizedTime);
                    
                    foreach (var ps in particleSystems)
                    {
                        var main = ps.main;
                        main.startColor = currentColor;
                    }
                }
                
                // Update light colors
                if (effectLights != null)
                {
                    Color currentColor = Color.Lerp(initialEffectColor, finalEffectColor, normalizedTime);
                    
                    foreach (var light in effectLights)
                    {
                        light.color = currentColor;
                        light.intensity = Mathf.Lerp(1f, 2f, normalizedTime);
                    }
                }
            }
        }
        
        /// <summary>
        /// Deactivate the trap effects immediately
        /// </summary>
        public void DeactivateEffect()
        {
            if (effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
                effectCoroutine = null;
            }
            
            if (activeEffectVisuals != null)
            {
                activeEffectVisuals.SetActive(false);
            }
            
            isTriggered = false;
        }
        
        protected override void OnDrawGizmosSelected()
        {
            // Draw effect radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, effectRadius);
        }
        
        void OnDestroy()
        {
            // Clean up coroutines
            if (effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
            }
        }
    }
}