using System;
using System.Collections;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Interaction;

namespace GamePortfolio.Gameplay.Interaction
{
    /// <summary>
    /// A lever mechanism that can be pulled to trigger events
    /// </summary>
    public class LeverMechanism : MonoBehaviour, IInteractable
    {
        [Header("Lever Settings")]
        [SerializeField] private string leverName = "Lever";
        [Tooltip("Time needed to pull the lever")]
        [SerializeField] private float interactionTime = 1.0f;
        [Tooltip("Can the lever be pulled multiple times")]
        [SerializeField] private bool isReusable = true;
        [Tooltip("Time before the lever can be pulled again")]
        [SerializeField] private float cooldownTime = 5.0f;
        [Tooltip("Whether the lever starts in the on position")]
        [SerializeField] private bool startInOnPosition = false;
        
        [Header("Animation")]
        [SerializeField] private Transform leverHandle;
        [SerializeField] private float rotationAmount = 60f;
        [SerializeField] private float animationSpeed = 2f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private Light statusLight;
        [SerializeField] private Color offColor = Color.red;
        [SerializeField] private Color onColor = Color.green;
        [SerializeField] private GameObject[] activateObjects;
        [SerializeField] private GameObject[] deactivateObjects;
        
        [Header("Audio")]
        [SerializeField] private string pullSound = "LeverPull";
        [SerializeField] private string activatedSound = "LeverActivated";
        [SerializeField] private string deactivatedSound = "LeverDeactivated";
        
        // Events
        public event Action<bool> OnLeverStateChanged;
        
        // Runtime state
        private bool isOn;
        private bool isInCooldown = false;
        private bool hasBeenUsed = false;
        private float cooldownEndTime = 0f;
        private Coroutine animationCoroutine;
        
        private void Awake()
        {
            // Set initial state
            isOn = startInOnPosition;
            
            // Set initial rotation
            if (leverHandle != null)
            {
                float rotation = isOn ? rotationAmount : 0f;
                leverHandle.localRotation = Quaternion.Euler(rotation, 0, 0);
            }
            
            // Set initial light color
            if (statusLight != null)
            {
                statusLight.color = isOn ? onColor : offColor;
            }
            
            // Set initial object states
            UpdateActivatedObjects();
        }
        
        private void Start()
        {
            // Trigger initial state change event
            OnLeverStateChanged?.Invoke(isOn);
        }
        
        private void Update()
        {
            // Update cooldown
            if (isInCooldown && Time.time > cooldownEndTime)
            {
                isInCooldown = false;
            }
        }
        
        /// <summary>
        /// Interact with the lever
        /// </summary>
        public void Interact()
        {
            // If already in use or in cooldown, don't proceed
            if (animationCoroutine != null || isInCooldown)
                return;
                
            // If not reusable and already used, don't proceed
            if (!isReusable && hasBeenUsed)
                return;
                
            // Toggle state
            isOn = !isOn;
            hasBeenUsed = true;
            
            // Start lever animation
            animationCoroutine = StartCoroutine(AnimateLever());
            
            // Play sound effect
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySfx(pullSound);
            }
            
            // Emit particles
            if (activationParticles != null)
            {
                activationParticles.Play();
            }
            
            // Start cooldown if needed
            if (cooldownTime > 0)
            {
                isInCooldown = true;
                cooldownEndTime = Time.time + cooldownTime;
            }
            
            // Notify listeners of state change
            OnLeverStateChanged?.Invoke(isOn);
        }
        
        /// <summary>
        /// Force the lever to a specific state
        /// </summary>
        public void SetState(bool on)
        {
            if (isOn == on)
                return;
                
            isOn = on;
            
            // Update visuals immediately
            if (leverHandle != null)
            {
                float rotation = isOn ? rotationAmount : 0f;
                leverHandle.localRotation = Quaternion.Euler(rotation, 0, 0);
            }
            
            if (statusLight != null)
            {
                statusLight.color = isOn ? onColor : offColor;
            }
            
            // Update activated objects
            UpdateActivatedObjects();
            
            // Notify listeners of state change
            OnLeverStateChanged?.Invoke(isOn);
        }
        
        /// <summary>
        /// Animate the lever movement
        /// </summary>
        private IEnumerator AnimateLever()
        {
            if (leverHandle == null)
            {
                animationCoroutine = null;
                yield break;
            }
            
            // Calculate start and end rotations
            Quaternion startRotation = leverHandle.localRotation;
            Quaternion endRotation = Quaternion.Euler(isOn ? rotationAmount : 0f, 0, 0);
            
            // Animate over time
            float time = 0;
            float duration = 1f / animationSpeed;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                
                // Apply smooth interpolation
                t = t * t * (3f - 2f * t); // Smoothstep
                
                leverHandle.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }
            
            leverHandle.localRotation = endRotation;
            
            // Update light color
            if (statusLight != null)
            {
                statusLight.color = isOn ? onColor : offColor;
            }
            
            // Play activation/deactivation sound
            if (AudioManager.HasInstance)
            {
                string sound = isOn ? activatedSound : deactivatedSound;
                AudioManager.Instance.PlaySfx(sound);
            }
            
            // Update activated objects
            UpdateActivatedObjects();
            
            animationCoroutine = null;
        }
        
        /// <summary>
        /// Enable or disable linked objects based on lever state
        /// </summary>
        private void UpdateActivatedObjects()
        {
            // Enable objects when on
            if (activateObjects != null)
            {
                foreach (GameObject obj in activateObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(isOn);
                    }
                }
            }
            
            // Disable objects when on
            if (deactivateObjects != null)
            {
                foreach (GameObject obj in deactivateObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(!isOn);
                    }
                }
            }
        }
        
        /// <summary>
        /// Reset the lever to its original state
        /// </summary>
        public void ResetLever()
        {
            // Cancel any ongoing animation
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            
            // Reset state
            isOn = startInOnPosition;
            hasBeenUsed = false;
            isInCooldown = false;
            
            // Reset visuals
            if (leverHandle != null)
            {
                float rotation = isOn ? rotationAmount : 0f;
                leverHandle.localRotation = Quaternion.Euler(rotation, 0, 0);
            }
            
            if (statusLight != null)
            {
                statusLight.color = isOn ? onColor : offColor;
            }
            
            // Update activated objects
            UpdateActivatedObjects();
            
            // Notify listeners of reset
            OnLeverStateChanged?.Invoke(isOn);
        }
        
        /// <summary>
        /// Get the current state of the lever
        /// </summary>
        public bool GetState()
        {
            return isOn;
        }
        
        #region IInteractable Interface
        
        /// <summary>
        /// Execute the interaction for the IInteractable interface
        /// </summary>
        void IInteractable.Interact()
        {
            Interact();
        }
        
        /// <summary>
        /// Check if interaction is possible for the IInteractable interface
        /// </summary>
        bool IInteractable.CanInteract()
        {
            // Can interact if not in cooldown and either reusable or not yet used
            return !isInCooldown && (isReusable || !hasBeenUsed);
        }
        
        /// <summary>
        /// Get the interaction prompt for the IInteractable interface
        /// </summary>
        string IInteractable.GetInteractionPrompt()
        {
            if (isInCooldown)
            {
                float remainingTime = cooldownEndTime - Time.time;
                return $"Lever Recharging ({Mathf.CeilToInt(remainingTime)}s)";
            }
            
            return $"Pull {leverName}";
        }
        
        /// <summary>
        /// Get the interaction time for the IInteractable interface
        /// </summary>
        float IInteractable.GetInteractionTime()
        {
            return interactionTime;
        }
        
        #endregion
    }
}