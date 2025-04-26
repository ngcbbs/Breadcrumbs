using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Gameplay;

namespace GamePortfolio.Gameplay
{
    /// <summary>
    /// Handles the trigger behavior for the exit portal
    /// Detects when players enter the portal and manages the visual effects
    /// </summary>
    public class PortalTrigger : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private float rotateSpeed = 30f;
        [SerializeField] private float hoverAmplitude = 0.2f;
        [SerializeField] private float hoverFrequency = 1f;
        [SerializeField] private float pulseFrequency = 2f;
        [SerializeField] private float pulseAmplitude = 0.1f;
        
        [Header("Visual Components")]
        [SerializeField] private Transform portalCore;
        [SerializeField] private Transform portalRing;
        [SerializeField] private ParticleSystem portalParticles;
        [SerializeField] private Light portalLight;
        
        [Header("Audio")]
        [SerializeField] private string portalAmbientSound = "PortalAmbient";
        [SerializeField] private string portalEnterSound = "PortalEnter";
        
        // References
        private PortalSystem portalSystem;
        private AudioSource audioSource;
        
        // Runtime state
        private Vector3 initialPosition;
        private Vector3 initialCoreScale;
        private float timeOffset;
        
        private void Awake()
        {
            // Store initial position and scale
            initialPosition = transform.position;
            
            if (portalCore != null)
            {
                initialCoreScale = portalCore.localScale;
            }
            
            // Get audio source
            audioSource = GetComponent<AudioSource>();
            
            // Random time offset for unique motion
            timeOffset = Random.value * 10f;
        }
        
        private void Start()
        {
            // Play ambient sound if audio source exists
            if (audioSource != null)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
            // Otherwise use audio manager
            else if (AudioManager.HasInstance)
            {
                Debug.Log($"TODO: PlayLoopingSound({portalAmbientSound}, {transform.position})");
                //AudioManager.Instance.PlayLoopingSound(portalAmbientSound, transform.position);
            }
        }
        
        private void Update()
        {
            // Apply visual effects
            AnimatePortal();
        }
        
        /// <summary>
        /// Initialize the portal trigger with the portal system
        /// </summary>
        public void Initialize(PortalSystem system)
        {
            portalSystem = system;
        }
        
        /// <summary>
        /// Animate the portal with rotation, hover, and pulse effects
        /// </summary>
        private void AnimatePortal()
        {
            // Rotation effect for ring
            if (portalRing != null)
            {
                portalRing.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.Self);
            }
            
            // Hover effect for entire portal
            float hoverOffset = Mathf.Sin((Time.time + timeOffset) * hoverFrequency) * hoverAmplitude;
            transform.position = initialPosition + new Vector3(0, hoverOffset, 0);
            
            // Pulse effect for core
            if (portalCore != null)
            {
                float pulseScale = 1f + Mathf.Sin((Time.time + timeOffset) * pulseFrequency) * pulseAmplitude;
                portalCore.localScale = initialCoreScale * pulseScale;
            }
            
            // Adjust light intensity if light exists
            if (portalLight != null)
            {
                float baseLightIntensity = 1.5f;
                float lightPulse = Mathf.Sin((Time.time + timeOffset) * pulseFrequency * 1.5f) * 0.3f;
                portalLight.intensity = baseLightIntensity + lightPulse;
            }
        }
        
        /// <summary>
        /// Handle objects entering the portal trigger
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // Check if it's a player
            if (other.CompareTag("Player"))
            {
                // Ensure portal system is available
                if (portalSystem != null)
                {
                    // Play enter sound
                    if (AudioManager.HasInstance)
                    {
                        AudioManager.Instance.PlaySfx(portalEnterSound);
                    }
                    
                    // Visual effect burst
                    if (portalParticles != null)
                    {
                        portalParticles.Emit(30);
                    }
                    
                    // Notify portal system
                    portalSystem.PlayerEnteredPortal(other.gameObject);
                }
                else
                {
                    Debug.LogWarning("Portal entered but no portal system reference!");
                }
            }
        }
        
        /// <summary>
        /// Display visual range of the portal trigger in editor
        /// </summary>
        private void OnDrawGizmos()
        {
            // Get trigger collider
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                // Draw trigger bounds
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                Gizmos.DrawSphere(transform.position, trigger.bounds.extents.magnitude);
                
                // Draw portal direction
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, transform.up * 2f);
            }
        }
        
        /// <summary>
        /// Clean up when destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Stop ambient sound if using audio manager
            if (AudioManager.HasInstance)
            {
                Debug.Log($"TODO: StopLoopingSound({portalAmbientSound})");
                //AudioManager.Instance.StopLoopingSound(portalAmbientSound);
            }
        }
    }
}