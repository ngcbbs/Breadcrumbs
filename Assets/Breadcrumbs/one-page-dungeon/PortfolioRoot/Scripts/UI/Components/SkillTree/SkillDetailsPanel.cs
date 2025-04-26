#if INCOMPLETE
using UnityEngine;
using UnityEngine.UI;
using GamePortfolio.Gameplay.Skills;

namespace GamePortfolio.UI.Components {
    /// <summary>
    /// UI component for displaying skill details
    /// </summary>
    public class SkillDetailsPanel : MonoBehaviour {
        [SerializeField]
        private Text skillNameText;
        [SerializeField]
        private Text skillDescriptionText;
        [SerializeField]
        private Text skillLevelText;
        [SerializeField]
        private Text skillCostText;
        [SerializeField]
        private Text skillTypeText;
        [SerializeField]
        private Text skillRequirementsText;
        [SerializeField]
        private Image skillIcon;
        [SerializeField]
        private Button unlockButton;
        [SerializeField]
        private Image skillTypeIcon;

        [Header("Skill Type Icons")]
        [SerializeField]
        private Sprite activeIcon;
        [SerializeField]
        private Sprite passiveIcon;
        [SerializeField]
        private Sprite toggleIcon;
        [SerializeField]
        private Sprite auraIcon;

        private SkillNodeData currentSkill;

        private void Awake() {
            // Set up unlock button if available
            if (unlockButton != null) {
                unlockButton.onClick.AddListener(OnUnlockButtonClicked);
                unlockButton.gameObject.SetActive(false);
            }

            // Hide panel initially
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show skill details
        /// </summary>
        public void ShowSkillDetails(SkillNodeData skill, bool isUnlocked, bool canUnlock) {
            currentSkill = skill;

            // Show panel
            gameObject.SetActive(true);

            // Set skill name
            if (skillNameText != null) {
                skillNameText.text = skill.SkillName;
            }

            // Set skill description
            if (skillDescriptionText != null) {
                string description = skill.Description;

                // Add level-specific info if available
                if (skill.LevelDescriptions != null && skill.LevelDescriptions.Count > 0) {
                    int level = isUnlocked ? skill.CurrentLevel : 1;
                    if (level > 0 && level <= skill.LevelDescriptions.Count) {
                        description += $"\n\n<color=#FFCC00>Level {level}:</color> {skill.LevelDescriptions[level - 1]}";
                    }
                }

                skillDescriptionText.text = description;
            }

            // Set skill level
            if (skillLevelText != null) {
                if (isUnlocked && skill.MaxLevel > 1) {
                    skillLevelText.text = $"Level: {skill.CurrentLevel}/{skill.MaxLevel}";
                    skillLevelText.gameObject.SetActive(true);
                } else if (skill.MaxLevel > 1) {
                    skillLevelText.text = $"Max Level: {skill.MaxLevel}";
                    skillLevelText.gameObject.SetActive(true);
                } else {
                    skillLevelText.gameObject.SetActive(false);
                }
            }

            // Set skill cost
            if (skillCostText != null) {
                skillCostText.text = $"Cost: {skill.PointCost} point{(skill.PointCost > 1 ? "s" : "")}";
            }

            // Set skill type
            if (skillTypeText != null) {
                string typeText = "";

                switch (skill.SkillType) {
                    case SkillType.Active:
                        typeText = "Active Skill";
                        break;
                    case SkillType.Passive:
                        typeText = "Passive Skill";
                        break;
                    case SkillType.Toggle:
                        typeText = "Toggle Skill";
                        break;
                    case SkillType.Aura:
                        typeText = "Aura";
                        break;
                }

                skillTypeText.text = typeText;
            }

            // Set skill type icon
            if (skillTypeIcon != null) {
                Sprite icon = null;

                switch (skill.SkillType) {
                    case SkillType.Active:
                        icon = activeIcon;
                        break;
                    case SkillType.Passive:
                        icon = passiveIcon;
                        break;
                    case SkillType.Toggle:
                        icon = toggleIcon;
                        break;
                    case SkillType.Aura:
                        icon = auraIcon;
                        break;
                }

                skillTypeIcon.sprite = icon;
                skillTypeIcon.enabled = icon != null;
            }

            // Set skill icon
            if (skillIcon != null && skill.Icon != null) {
                skillIcon.sprite = skill.Icon;
                skillIcon.enabled = true;
            } else if (skillIcon != null) {
                skillIcon.enabled = false;
            }

            // Set requirements text
            if (skillRequirementsText != null) {
                string reqText = "";

                if (skill.LevelRequirement > 1) {
                    reqText += $"• Requires level {skill.LevelRequirement}\n";
                }

                if (skill.PrerequisiteSkillIds.Count > 0) {
                    reqText += "• Requires skills:\n";
                    foreach (string prereqId in skill.PrerequisiteSkillIds) {
                        // Would need to look up skill name from ID
                        reqText += $"  • {GetSkillName(prereqId)}\n";
                    }
                }

                skillRequirementsText.text = reqText;
                skillRequirementsText.gameObject.SetActive(!string.IsNullOrEmpty(reqText));
            }

            // Show/hide unlock button
            if (unlockButton != null) {
                unlockButton.gameObject.SetActive(canUnlock);
            }
        }

        /// <summary>
        /// Clear skill details
        /// </summary>
        public void ClearDetails() {
            // Hide panel
            gameObject.SetActive(false);
            currentSkill = null;
        }

        /// <summary>
        /// Handle unlock button click
        /// </summary>
        private void OnUnlockButtonClicked() {
            // Find skill tree UI
            SkillTreeUI skillTreeUI = GetComponentInParent<SkillTreeUI>();

            if (skillTreeUI != null && currentSkill != null) {
                // Trigger node clicked to handle unlock
                skillTreeUI.SendMessage("OnSkillNodeClicked", currentSkill, SendMessageOptions.DontRequireReceiver);
            }

            // Play UI sound
            PlaySound("Click");
        }

        /// <summary>
        /// Get skill name from ID
        /// </summary>
        private string GetSkillName(string skillId) {
            // In a real implementation, this would look up the skill name
            // from the skill manager or other data source
            return $"Skill {skillId}";
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