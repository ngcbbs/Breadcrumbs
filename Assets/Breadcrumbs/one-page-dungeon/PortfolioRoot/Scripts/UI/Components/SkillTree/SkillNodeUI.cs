#if INCOMPLETE
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GamePortfolio.Gameplay.Skills;

namespace GamePortfolio.UI.Components {
    /// <summary>
    /// UI component for a skill node in the skill tree
    /// </summary>
    public class SkillNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField]
        private Image skillIcon;
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private Image borderImage;
        [SerializeField]
        private Text skillLevelText;
        [SerializeField]
        private GameObject lockIcon;

        [Header("Node States")]
        [SerializeField]
        private Color unlockedColor = Color.green;
        [SerializeField]
        private Color lockedColor = Color.gray;
        [SerializeField]
        private Color availableColor = Color.yellow;
        [SerializeField]
        private Color hoveredColor = Color.white;
        [SerializeField]
        private float hoverScaleMultiplier = 1.2f;

        private SkillNodeData skillData;
        private bool isUnlocked = false;
        private bool canUnlock = false;
        private bool hasPoints = false;
        private Vector3 defaultScale;

        // Callback for node click
        private Action<SkillNodeData> onNodeClicked;

        // Property to get skill data
        public SkillNodeData SkillData => skillData;

        private void Awake() {
            defaultScale = transform.localScale;
        }

        /// <summary>
        /// Initialize the skill node
        /// </summary>
        public void Initialize(SkillNodeData data, bool unlocked, bool available, bool points,
            Action<SkillNodeData> clickCallback) {
            skillData = data;
            isUnlocked = unlocked;
            canUnlock = available;
            hasPoints = points;
            onNodeClicked = clickCallback;

            // Set skill icon
            if (skillIcon != null && data.Icon != null) {
                skillIcon.sprite = data.Icon;
            }

            // Set node state visuals
            UpdateVisuals();
        }

        /// <summary>
        /// Update node state
        /// </summary>
        public void UpdateState(bool unlocked = false, bool available = false, bool points = false) {
            isUnlocked = unlocked;
            canUnlock = available;
            hasPoints = points;

            UpdateVisuals();
        }

        /// <summary>
        /// Set unlocked state
        /// </summary>
        public void SetUnlocked(bool unlocked) {
            isUnlocked = unlocked;
            UpdateVisuals();
        }

        /// <summary>
        /// Update node visuals based on state
        /// </summary>
        private void UpdateVisuals() {
            // Update background color
            if (backgroundImage != null) {
                backgroundImage.color = GetNodeColor();
            }

            // Update border visibility
            if (borderImage != null) {
                borderImage.enabled = isUnlocked || canUnlock;

                if (borderImage.enabled) {
                    borderImage.color = isUnlocked ? unlockedColor : availableColor;
                }
            }

            // Update skill icon
            if (skillIcon != null) {
                skillIcon.color = isUnlocked ? Color.white : new Color(1, 1, 1, 0.5f);
            }

            // Update lock icon
            if (lockIcon != null) {
                lockIcon.SetActive(!isUnlocked);
            }

            // Update skill level text
            if (skillLevelText != null) {
                if (isUnlocked && skillData.MaxLevel > 1) {
                    skillLevelText.text = $"{skillData.CurrentLevel}/{skillData.MaxLevel}";
                    skillLevelText.gameObject.SetActive(true);
                } else {
                    skillLevelText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Get node color based on state
        /// </summary>
        private Color GetNodeColor() {
            if (isUnlocked)
                return unlockedColor;
            else if (canUnlock && hasPoints)
                return availableColor;
            else
                return lockedColor;
        }

        /// <summary>
        /// Handle pointer click
        /// </summary>
        public void OnPointerClick(PointerEventData eventData) {
            // Invoke callback
            onNodeClicked?.Invoke(skillData);

            // Play sound based on state
            if (isUnlocked) {
                PlaySound("NodeUnlocked");
            } else if (canUnlock && hasPoints) {
                PlaySound("NodeAvailable");
            } else {
                PlaySound("NodeLocked");
            }
        }

        /// <summary>
        /// Handle pointer enter
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData) {
            // Scale up
            transform.localScale = defaultScale * hoverScaleMultiplier;

            // Change border color if not unlocked
            if (borderImage != null && !isUnlocked) {
                borderImage.enabled = true;
                borderImage.color = hoveredColor;
            }

            // Play hover sound
            PlaySound("Hover");
        }

        /// <summary>
        /// Handle pointer exit
        /// </summary>
        public void OnPointerExit(PointerEventData eventData) {
            // Reset scale
            transform.localScale = defaultScale;

            // Reset border color
            if (borderImage != null) {
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Play UI sound if audio manager is available
        /// </summary>
        private void PlaySound(string soundName) {
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound(soundName);
            }
        }
    }
}
#endif