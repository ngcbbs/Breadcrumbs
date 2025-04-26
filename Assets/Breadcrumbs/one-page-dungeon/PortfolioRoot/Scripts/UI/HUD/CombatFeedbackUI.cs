#if INCOMPLETE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Character;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.UI.HUD {
    /// <summary>
    /// Handles visual feedback for combat events including floating damage numbers, 
    /// hit effects, and status indicators
    /// </summary>
    public class CombatFeedbackUI : MonoBehaviour {
        [Header("Floating Text Settings")]
        [SerializeField]
        private GameObject floatingTextPrefab;
        [SerializeField]
        private Transform floatingTextParent;
        [SerializeField]
        private float floatingTextDuration = 1.5f;
        [SerializeField]
        private float floatingTextRiseSpeed = 1.0f;
        [SerializeField]
        private float floatingTextFadeSpeed = 1.0f;
        [SerializeField]
        private Vector2 floatingTextOffsetRange = new Vector2(-0.5f, 0.5f);

        [Header("Damage Color Settings")]
        [SerializeField]
        private Color normalDamageColor = Color.white;
        [SerializeField]
        private Color criticalDamageColor = Color.red;
        [SerializeField]
        private Color healingColor = Color.green;
        [SerializeField]
        private Color missedColor = Color.gray;
        [SerializeField]
        private Color blockColor = Color.blue;

        [Header("Hit Effect Settings")]
        [SerializeField]
        private GameObject hitEffectPrefab;
        [SerializeField]
        private GameObject criticalHitEffectPrefab;
        [SerializeField]
        private GameObject healEffectPrefab;
        [SerializeField]
        private GameObject blockEffectPrefab;
        [SerializeField]
        private float hitEffectDuration = 0.5f;

        [Header("Screen Effects")]
        [SerializeField]
        private Image damageScreenVignette;
        [SerializeField]
        private Image lowHealthPulseVignette;
        [SerializeField]
        private float screenEffectDuration = 0.2f;
        [SerializeField]
        private float lowHealthThreshold = 0.3f;
        [SerializeField]
        private float lowHealthPulseSpeed = 1.0f;

        [Header("Status Effects")]
        [SerializeField]
        private Transform statusEffectContainer;
        [SerializeField]
        private GameObject statusEffectIconPrefab;
        [SerializeField]
        private Sprite poisonedSprite;
        [SerializeField]
        private Sprite bleedingSprite;
        [SerializeField]
        private Sprite stunnedSprite;
        [SerializeField]
        private Sprite buffedSprite;
        [SerializeField]
        private Sprite debuffedSprite;

        [Header("Hit Direction Indicator")]
        [SerializeField]
        private Image hitDirectionIndicator;
        [SerializeField]
        private float hitDirectionDuration = 0.5f;

        // Cached references
        private Camera mainCamera;
        private PlayerCombat playerCombat;
        private PlayerStats playerStats;
        private Dictionary<StatusEffectType, GameObject> activeStatusEffects = new Dictionary<StatusEffectType, GameObject>();

        // Internal state
        private Coroutine lowHealthPulseCoroutine;
        private Coroutine damageVignetteCoroutine;
        private Coroutine hitDirectionCoroutine;

        private void Awake() {
            mainCamera = Camera.main;
            playerCombat = FindObjectOfType<PlayerCombat>();
            playerStats = FindObjectOfType<PlayerStats>();

            // Initialize screen effects
            if (damageScreenVignette != null) {
                damageScreenVignette.color = new Color(1, 0, 0, 0);
            }

            if (lowHealthPulseVignette != null) {
                lowHealthPulseVignette.color = new Color(1, 0, 0, 0);
            }

            if (hitDirectionIndicator != null) {
                hitDirectionIndicator.enabled = false;
            }
        }

        private void Start() {
            // Subscribe to combat events
            if (playerCombat != null) {
                playerCombat.OnDamageTaken += HandleDamageTaken;
                playerCombat.OnDamageDealt += HandleDamageDealt;
                playerCombat.OnHealingReceived += HandleHealingReceived;
                playerCombat.OnStatusEffectChanged += HandleStatusEffectChanged;
            }

            // Subscribe to player stats events
            if (playerStats != null) {
                playerStats.OnHealthChanged += HandleHealthChanged;
            }
        }

        private void OnDestroy() {
            // Unsubscribe from events
            if (playerCombat != null) {
                playerCombat.OnDamageTaken -= HandleDamageTaken;
                playerCombat.OnDamageDealt -= HandleDamageDealt;
                playerCombat.OnHealingReceived -= HandleHealingReceived;
                playerCombat.OnStatusEffectChanged -= HandleStatusEffectChanged;
            }

            if (playerStats != null) {
                playerStats.OnHealthChanged -= HandleHealthChanged;
            }
        }

        /// <summary>
        /// Handle damage taken by the player
        /// </summary>
        private void HandleDamageTaken(int amount, bool isCritical, Vector3 sourcePosition) {
            // Screen effects for player taking damage
            ShowDamageVignette();

            // Show hit direction indicator
            if (sourcePosition != Vector3.zero && hitDirectionIndicator != null) {
                ShowHitDirectionIndicator(sourcePosition);
            }

            // Camera shake effect
            if (isCritical) {
                CameraShake.Shake(0.3f, 0.2f);
            } else {
                CameraShake.Shake(0.1f, 0.1f);
            }

            // Play sound via AudioManager
            if (AudioManager.HasInstance) {
                string soundKey = isCritical ? "PlayerDamageCritical" : "PlayerDamage";
                AudioManager.Instance.PlaySfx(soundKey);
            }
        }

        /// <summary>
        /// Handle damage dealt by the player
        /// </summary>
        private void HandleDamageDealt(int amount, bool isCritical, Vector3 targetPosition, bool isBlocked) {
            // Don't show anything for zero damage
            if (amount <= 0 && !isBlocked)
                return;

            // Show appropriate floating text
            if (isBlocked) {
                ShowFloatingText("BLOCKED", targetPosition, blockColor);
                SpawnHitEffect(blockEffectPrefab, targetPosition);
            } else {
                Color textColor = isCritical ? criticalDamageColor : normalDamageColor;
                string damageText = isCritical ? $"{amount} !" : amount.ToString();
                ShowFloatingText(damageText, targetPosition, textColor);

                // Spawn visual hit effect
                GameObject effectPrefab = isCritical ? criticalHitEffectPrefab : hitEffectPrefab;
                SpawnHitEffect(effectPrefab, targetPosition);
            }

            // Play sound via AudioManager
            if (AudioManager.HasInstance) {
                string soundKey = isBlocked ? "HitBlocked" : (isCritical ? "HitCritical" : "HitNormal");
                AudioManager.Instance.PlaySfx(soundKey);
            }
        }

        /// <summary>
        /// Handle healing received by the player or dealt to targets
        /// </summary>
        private void HandleHealingReceived(int amount, Vector3 targetPosition) {
            if (amount <= 0)
                return;

            // Show floating healing text
            ShowFloatingText("+" + amount, targetPosition, healingColor);

            // Spawn healing visual effect
            SpawnHitEffect(healEffectPrefab, targetPosition);

            // Play sound via AudioManager
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("Healing");
            }
        }

        /// <summary>
        /// Handle status effect changes
        /// </summary>
        private void HandleStatusEffectChanged(StatusEffectType effectType, bool isActive, float duration) {
            if (isActive) {
                // Add or update status effect icon
                AddStatusEffectIcon(effectType, duration);
            } else {
                // Remove status effect icon
                RemoveStatusEffectIcon(effectType);
            }

            // Play sound via AudioManager
            if (AudioManager.HasInstance) {
                string soundKey = isActive ? "StatusApplied" : "StatusRemoved";
                AudioManager.Instance.PlaySfx(soundKey);
            }
        }

        /// <summary>
        /// Handle player health changes to update low health effects
        /// </summary>
        private void HandleHealthChanged(int currentHealth, int maxHealth) {
            float healthPercent = (float)currentHealth / maxHealth;

            // Handle low health pulse effect
            if (healthPercent <= lowHealthThreshold) {
                if (lowHealthPulseCoroutine == null) {
                    lowHealthPulseCoroutine = StartCoroutine(LowHealthPulseRoutine(healthPercent));
                }
            } else {
                // Stop low health pulse if health is restored
                if (lowHealthPulseCoroutine != null) {
                    StopCoroutine(lowHealthPulseCoroutine);
                    lowHealthPulseCoroutine = null;

                    // Ensure vignette is cleared
                    if (lowHealthPulseVignette != null) {
                        lowHealthPulseVignette.color = new Color(1, 0, 0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Show the damage vignette effect when player takes damage
        /// </summary>
        private void ShowDamageVignette() {
            if (damageScreenVignette == null)
                return;

            // Stop existing coroutine if running
            if (damageVignetteCoroutine != null) {
                StopCoroutine(damageVignetteCoroutine);
            }

            // Start new vignette effect
            damageVignetteCoroutine = StartCoroutine(DamageVignetteRoutine());
        }

        /// <summary>
        /// Coroutine for damage vignette effect
        /// </summary>
        private IEnumerator DamageVignetteRoutine() {
            // Fade in quickly
            float elapsedTime = 0f;
            float fadeInDuration = screenEffectDuration * 0.2f;

            // Set initial alpha
            damageScreenVignette.color = new Color(1, 0, 0, 0);

            // Fade in
            while (elapsedTime < fadeInDuration) {
                float alpha = Mathf.Lerp(0, 0.5f, elapsedTime / fadeInDuration);
                damageScreenVignette.color = new Color(1, 0, 0, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure max alpha is reached
            damageScreenVignette.color = new Color(1, 0, 0, 0.5f);

            // Hold briefly
            yield return new WaitForSeconds(screenEffectDuration * 0.1f);

            // Fade out
            elapsedTime = 0f;
            float fadeOutDuration = screenEffectDuration * 0.7f;

            while (elapsedTime < fadeOutDuration) {
                float alpha = Mathf.Lerp(0.5f, 0, elapsedTime / fadeOutDuration);
                damageScreenVignette.color = new Color(1, 0, 0, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure alpha is zero
            damageScreenVignette.color = new Color(1, 0, 0, 0);
            damageVignetteCoroutine = null;
        }

        /// <summary>
        /// Show the low health pulse effect
        /// </summary>
        private IEnumerator LowHealthPulseRoutine(float healthPercent) {
            if (lowHealthPulseVignette == null)
                yield break;

            // Intensity increases as health decreases
            float maxAlpha = Mathf.Lerp(0.2f, 0.4f, 1 - (healthPercent / lowHealthThreshold));

            while (true) {
                // Pulse alpha up and down based on sin wave
                float alpha = (Mathf.Sin(Time.time * lowHealthPulseSpeed) + 1) * 0.5f * maxAlpha;
                lowHealthPulseVignette.color = new Color(1, 0, 0, alpha);
                yield return null;
            }
        }

        /// <summary>
        /// Show hit direction indicator pointing to damage source
        /// </summary>
        private void ShowHitDirectionIndicator(Vector3 sourcePosition) {
            if (hitDirectionIndicator == null || playerCombat == null)
                return;

            if (hitDirectionCoroutine != null) {
                StopCoroutine(hitDirectionCoroutine);
            }

            hitDirectionCoroutine = StartCoroutine(HitDirectionIndicatorRoutine(sourcePosition));
        }

        /// <summary>
        /// Coroutine for showing hit direction indicator
        /// </summary>
        private IEnumerator HitDirectionIndicatorRoutine(Vector3 sourcePosition) {
            // Enable the indicator
            hitDirectionIndicator.enabled = true;

            // Get direction from player to source
            Vector3 playerPos = playerCombat.transform.position;
            Vector3 direction = sourcePosition - playerPos;

            // Calculate angle between forward and direction
            float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);

            // Rotate the indicator
            hitDirectionIndicator.rectTransform.rotation = Quaternion.Euler(0, 0, -angle);

            // Fade in
            float elapsedTime = 0f;
            float fadeInDuration = hitDirectionDuration * 0.2f;

            // Set initial color
            hitDirectionIndicator.color = new Color(1, 0, 0, 0);

            // Fade in
            while (elapsedTime < fadeInDuration) {
                float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
                hitDirectionIndicator.color = new Color(1, 0, 0, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure full opacity
            hitDirectionIndicator.color = new Color(1, 0, 0, 1);

            // Hold briefly
            yield return new WaitForSeconds(hitDirectionDuration * 0.3f);

            // Fade out
            elapsedTime = 0f;
            float fadeOutDuration = hitDirectionDuration * 0.5f;

            while (elapsedTime < fadeOutDuration) {
                float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeOutDuration);
                hitDirectionIndicator.color = new Color(1, 0, 0, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Disable the indicator
            hitDirectionIndicator.enabled = false;
            hitDirectionCoroutine = null;
        }

        /// <summary>
        /// Show floating text at the specified world position
        /// </summary>
        private void ShowFloatingText(string text, Vector3 worldPosition, Color textColor) {
            if (floatingTextPrefab == null || floatingTextParent == null || mainCamera == null)
                return;

            // Instantiate the floating text
            GameObject floatingTextObj = Instantiate(floatingTextPrefab, floatingTextParent);

            // Get text component
            TMP_Text textComponent = floatingTextObj.GetComponentInChildren<TMP_Text>();
            if (textComponent == null)
                return;

            // Set text and color
            textComponent.text = text;
            textComponent.color = textColor;

            // Add random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(floatingTextOffsetRange.x, floatingTextOffsetRange.y),
                0,
                Random.Range(floatingTextOffsetRange.x, floatingTextOffsetRange.y)
            );

            // Position in world space
            worldPosition += randomOffset;

            // Convert to screen position
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // Set position
            RectTransform rectTransform = floatingTextObj.GetComponent<RectTransform>();
            rectTransform.position = screenPosition;

            // Start animation coroutine
            StartCoroutine(AnimateFloatingText(floatingTextObj, rectTransform, textComponent));
        }

        /// <summary>
        /// Animate floating text rising and fading
        /// </summary>
        private IEnumerator AnimateFloatingText(GameObject textObj, RectTransform rectTransform, TMP_Text textComponent) {
            float elapsedTime = 0f;
            Vector2 startPosition = rectTransform.position;
            Color startColor = textComponent.color;

            // Scale up slightly at start
            rectTransform.localScale = Vector3.one * 0.5f;
            float scaleDuration = floatingTextDuration * 0.2f;

            while (elapsedTime < scaleDuration) {
                float scale = Mathf.Lerp(0.5f, 1.2f, elapsedTime / scaleDuration);
                rectTransform.localScale = Vector3.one * scale;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Scale back down slightly
            elapsedTime = 0f;
            float scaleDownDuration = floatingTextDuration * 0.1f;

            while (elapsedTime < scaleDownDuration) {
                float scale = Mathf.Lerp(1.2f, 1f, elapsedTime / scaleDownDuration);
                rectTransform.localScale = Vector3.one * scale;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Hold briefly
            rectTransform.localScale = Vector3.one;
            yield return new WaitForSeconds(floatingTextDuration * 0.1f);

            // Rise and fade
            elapsedTime = 0f;
            float riseDuration = floatingTextDuration * 0.6f;

            Vector2 endPosition = startPosition + Vector2.up * 100; // Rise by 100 pixels

            while (elapsedTime < riseDuration) {
                float t = elapsedTime / riseDuration;

                // Move up
                rectTransform.position = Vector2.Lerp(startPosition, endPosition, t);

                // Fade out
                float alpha = Mathf.Lerp(startColor.a, 0, t);
                textComponent.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Destroy the object
            Destroy(textObj);
        }

        /// <summary>
        /// Spawn a hit effect at the specified world position
        /// </summary>
        private void SpawnHitEffect(GameObject effectPrefab, Vector3 worldPosition) {
            if (effectPrefab == null)
                return;

            // Instantiate effect
            GameObject effect = Instantiate(effectPrefab, worldPosition, Quaternion.identity);

            // Destroy after duration
            Destroy(effect, hitEffectDuration);
        }

        /// <summary>
        /// Add or update a status effect icon
        /// </summary>
        private void AddStatusEffectIcon(StatusEffectType effectType, float duration) {
            if (statusEffectContainer == null)
                return;

            // Get sprite for effect type
            Sprite effectSprite = GetSpriteForStatusEffect(effectType);
            if (effectSprite == null)
                return;

            // Check if icon already exists
            if (activeStatusEffects.TryGetValue(effectType, out GameObject existingIcon)) {
                // Update duration
                StatusEffectIcon iconScript = existingIcon.GetComponent<StatusEffectIcon>();
                if (iconScript != null) {
                    iconScript.UpdateDuration(duration);
                }

                return;
            }

            // Create new icon
            GameObject iconObj = Instantiate(statusEffectIconPrefab, statusEffectContainer);

            // Get icon components
            Image iconImage = iconObj.GetComponentInChildren<Image>();
            StatusEffectIcon iconScript = iconObj.GetComponent<StatusEffectIcon>();

            if (iconImage != null) {
                iconImage.sprite = effectSprite;
            }

            if (iconScript != null) {
                iconScript.Initialize(effectType, duration);
            }

            // Add to active effects
            activeStatusEffects[effectType] = iconObj;

            // Play animation for new status icon
            Animator animator = iconObj.GetComponent<Animator>();
            if (animator != null) {
                animator.SetTrigger("Show");
            }
        }

        /// <summary>
        /// Remove a status effect icon
        /// </summary>
        private void RemoveStatusEffectIcon(StatusEffectType effectType) {
            if (!activeStatusEffects.TryGetValue(effectType, out GameObject iconObj))
                return;

            // Play fadeout animation
            Animator animator = iconObj.GetComponent<Animator>();
            if (animator != null) {
                animator.SetTrigger("Hide");

                // Destroy after animation
                Destroy(iconObj, 0.5f);
            } else {
                // Destroy immediately if no animator
                Destroy(iconObj);
            }

            // Remove from active effects
            activeStatusEffects.Remove(effectType);
        }

        /// <summary>
        /// Get appropriate sprite for status effect type
        /// </summary>
        private Sprite GetSpriteForStatusEffect(StatusEffectType effectType) {
            switch (effectType) {
                case StatusEffectType.Poisoned:
                    return poisonedSprite;
                // case StatusEffectType.Bleeding:
                //     return bleedingSprite;
                case StatusEffectType.Stunned:
                    return stunnedSprite;
                // case StatusEffectType.Buffed:
                //     return buffedSprite;
                // case StatusEffectType.Debuffed:
                //     return debuffedSprite;
                default:
                    Debug.Log($"TODO: {effectType} is not a valid status effect type");
                    return null;
            }
        }
    }

    /// <summary>
    /// Component for status effect icons to handle duration and animation
    /// </summary>
    public class StatusEffectIcon : MonoBehaviour {
        [SerializeField]
        private Image durationFillImage;
        [SerializeField]
        private TMP_Text durationText;

        private StatusEffectType effectType;
        private float maxDuration;
        private float remainingDuration;

        /// <summary>
        /// Initialize the status effect icon
        /// </summary>
        public void Initialize(StatusEffectType type, float duration) {
            effectType = type;
            maxDuration = duration;
            remainingDuration = duration;

            UpdateVisuals();
        }

        /// <summary>
        /// Update the duration of the status effect
        /// </summary>
        public void UpdateDuration(float newDuration) {
            maxDuration = newDuration;
            remainingDuration = newDuration;

            UpdateVisuals();
        }

        private void Update() {
            if (remainingDuration > 0) {
                remainingDuration -= Time.deltaTime;
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Update visual elements based on current duration
        /// </summary>
        private void UpdateVisuals() {
            // Update fill image
            if (durationFillImage != null) {
                durationFillImage.fillAmount = Mathf.Clamp01(remainingDuration / maxDuration);
            }

            // Update text if needed
            if (durationText != null) {
                if (remainingDuration > 0) {
                    durationText.text = Mathf.Ceil(remainingDuration).ToString();
                } else {
                    durationText.text = "";
                }
            }
        }
    }
}
#endif