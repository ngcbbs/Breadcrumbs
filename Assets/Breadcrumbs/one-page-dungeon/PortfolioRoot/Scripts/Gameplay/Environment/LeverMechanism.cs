using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Interaction;

namespace GamePortfolio.Gameplay.Environment {
    /// <summary>
    /// A lever that can be pulled to activate or deactivate connected mechanisms
    /// </summary>
    public class LeverMechanism : InteractiveEnvironment {
        [Header("Lever Settings")]
        [SerializeField]
        private Transform leverHandle;
        [SerializeField]
        private Vector3 offRotation = Vector3.zero;
        [SerializeField]
        private Vector3 onRotation = new Vector3(0, 0, -60);
        [SerializeField]
        private float animationSpeed = 5f;
        [SerializeField]
        private bool isLeverOn = false;
        [SerializeField]
        private bool toggleMode = true;
        [SerializeField]
        private float autoResetTime = 0f; // 0 = no auto reset

        [Header("Connected Objects")]
        [SerializeField]
        private GameObject[] activatedObjects;
        [SerializeField]
        private GameObject[] deactivatedObjects;

        // Components
        private Coroutine animationCoroutine;
        private Coroutine resetCoroutine;

        protected override void Awake() {
            base.Awake();

            // Set initial lever position
            if (leverHandle != null) {
                leverHandle.localEulerAngles = isLeverOn ? onRotation : offRotation;
            }

            // Set initial state of connected objects
            UpdateConnectedObjects();
        }

        /// <summary>
        /// Execute the lever pull effect
        /// </summary>
        protected override void ExecuteInteractionEffect() {
            // Toggle or set lever state
            if (toggleMode) {
                isLeverOn = !isLeverOn;
            } else {
                isLeverOn = true;
            }

            // Update visual state
            if (animationCoroutine != null) {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateLever());

            // Update connected objects
            UpdateConnectedObjects();

            // Handle auto reset
            if (autoResetTime > 0f) {
                if (resetCoroutine != null) {
                    StopCoroutine(resetCoroutine);
                }

                resetCoroutine = StartCoroutine(AutoReset());
            }
        }

        /// <summary>
        /// Animate the lever handle movement
        /// </summary>
        private IEnumerator AnimateLever() {
            if (leverHandle == null)
                yield break;

            Vector3 targetRotation = isLeverOn ? onRotation : offRotation;
            Vector3 startRotation = leverHandle.localEulerAngles;

            float animationTime = 0f;
            float animationDuration = 1f / animationSpeed;

            while (animationTime < animationDuration) {
                animationTime += Time.deltaTime;
                float t = Mathf.Clamp01(animationTime / animationDuration);

                // Use smoothstep for more natural motion
                float smoothT = t * t * (3f - 2f * t);

                leverHandle.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, smoothT);

                yield return null;
            }

            // Ensure final position is exact
            leverHandle.localEulerAngles = targetRotation;

            animationCoroutine = null;
        }

        /// <summary>
        /// Auto reset the lever after a delay
        /// </summary>
        private IEnumerator AutoReset() {
            yield return new WaitForSeconds(autoResetTime);

            // Reset lever
            isLeverOn = !isLeverOn;

            // Update visual state
            if (animationCoroutine != null) {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateLever());

            // Update connected objects
            UpdateConnectedObjects();

            resetCoroutine = null;
        }

        /// <summary>
        /// Update all connected objects based on lever state
        /// </summary>
        private void UpdateConnectedObjects() {
            // Handle activation objects
            if (activatedObjects != null) {
                foreach (var obj in activatedObjects) {
                    if (obj != null) {
                        // Enable/disable the object
                        obj.SetActive(isLeverOn);

                        // If it implements IActivatable, call the appropriate method
                        IActivatable activatable = obj.GetComponent<IActivatable>();
                        if (activatable != null) {
                            if (isLeverOn) {
                                activatable.Activate();
                            } else {
                                activatable.Deactivate();
                            }
                        }
                    }
                }
            }

            // Handle deactivation objects (inverse logic)
            if (deactivatedObjects != null) {
                foreach (var obj in deactivatedObjects) {
                    if (obj != null) {
                        // Enable/disable the object
                        obj.SetActive(!isLeverOn);

                        // If it implements IActivatable, call the appropriate method
                        IActivatable activatable = obj.GetComponent<IActivatable>();
                        if (activatable != null) {
                            if (!isLeverOn) {
                                activatable.Activate();
                            } else {
                                activatable.Deactivate();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Force set lever state (for external control)
        /// </summary>
        public void SetState(bool state) {
            if (isLeverOn == state)
                return;

            isLeverOn = state;

            // Update visual state
            if (animationCoroutine != null) {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateLever());

            // Update connected objects
            UpdateConnectedObjects();
        }

        /// <summary>
        /// Get the current lever state
        /// </summary>
        public bool GetState() {
            return isLeverOn;
        }
    }
}