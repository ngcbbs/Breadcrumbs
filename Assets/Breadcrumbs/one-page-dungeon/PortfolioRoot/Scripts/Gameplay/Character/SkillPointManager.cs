using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;
using GamePortfolio.Gameplay.Character.Classes;

namespace GamePortfolio.Gameplay.Character {
    public class SkillPointManager : MonoBehaviour {
        [Header("Skill System Settings")]
        [SerializeField]
        private int maxSkillLevel = 5;
        [SerializeField]
        private int skillStartLevel = 1;
        [SerializeField]
        private int skillUnlockCost = 1;
        [SerializeField]
        private int skillUpgradeCost = 1;

        [Header("Progression Curve")]
        [SerializeField]
        private float skillEffectMultiplier = 1.2f;
        [SerializeField]
        private AnimationCurve skillProgressionCurve;

        private CharacterLevelManager levelManager;
        private BaseCharacterClass characterClass;

        private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
        private HashSet<string> unlockedSkills = new HashSet<string>();

        public event Action<string, int> OnSkillLevelChanged;
        public event Action<string> OnSkillUnlocked;

        private void Awake() {
            levelManager = GetComponent<CharacterLevelManager>();
            characterClass = GetComponent<BaseCharacterClass>();

            if (levelManager == null) {
                Debug.LogError("SkillPointManager: CharacterLevelManager component not found!");
            }

            if (characterClass == null) {
                Debug.LogError("SkillPointManager: CharacterClass component not found!");
            }
        }

        private void Start() {
            InitializeSkills();

            if (characterClass != null) {
                characterClass.OnSkillUnlocked += HandleSkillUnlocked;
            }
        }

        private void OnDestroy() {
            if (characterClass != null) {
                characterClass.OnSkillUnlocked -= HandleSkillUnlocked;
            }
        }

        private void InitializeSkills() {
            if (characterClass == null) return;

            var skills = characterClass.GetClassSkills();

            if (skills == null) return;

            if (skills.Count > 0) {
                UnlockSkill(skills[0].skillName);
                SetSkillLevel(skills[0].skillName, skillStartLevel);
            }
        }

        public bool UnlockSkill(string skillName) {
            if (unlockedSkills.Contains(skillName))
                return false;

            if (levelManager != null && !levelManager.UseSkillPoint())
                return false;

            unlockedSkills.Add(skillName);

            skillLevels[skillName] = 0;

            OnSkillUnlocked?.Invoke(skillName);

            return true;
        }

        public bool UpgradeSkill(string skillName) {
            if (!unlockedSkills.Contains(skillName))
                return false;

            int currentLevel = skillLevels.ContainsKey(skillName) ? skillLevels[skillName] : 0;

            if (currentLevel >= maxSkillLevel)
                return false;

            if (levelManager != null && !levelManager.UseSkillPoint())
                return false;

            skillLevels[skillName] = currentLevel + 1;

            OnSkillLevelChanged?.Invoke(skillName, skillLevels[skillName]);

            ApplySkillUpgrade(skillName);

            return true;
        }

        private void ApplySkillUpgrade(string skillName) {
            if (characterClass == null) return;

            var skills = characterClass.GetClassSkills();
            SkillData skillToUpgrade = null;
            int skillIndex = -1;

            for (int i = 0; i < skills.Count; i++) {
                if (skills[i].skillName == skillName) {
                    skillToUpgrade = skills[i];
                    skillIndex = i;
                    break;
                }
            }

            if (skillToUpgrade == null) return;

            int level = skillLevels[skillName];
            float scalingFactor = CalculateScalingFactor(level);

            switch (skillToUpgrade.skillType) {
                case SkillType.Attack:
                    skillToUpgrade.power *= scalingFactor;
                    skillToUpgrade.range *= 1 + (level * 0.05f);
                    break;

                case SkillType.Heal:
                    skillToUpgrade.power *= scalingFactor;
                    break;

                case SkillType.Buff:
                    skillToUpgrade.duration *= 1 + (level * 0.1f);
                    break;

                case SkillType.Utility:
                    skillToUpgrade.cooldown /= 1 + (level * 0.05f);
                    break;
            }

            if (level >= 3) {
                skillToUpgrade.staminaCost *= 0.9f;
            }
        }

        private float CalculateScalingFactor(int level) {
            if (level <= 0) return 1f;

            if (skillProgressionCurve != null) {
                float normalizedLevel = (float)(level) / maxSkillLevel;
                float curveValue = skillProgressionCurve.Evaluate(normalizedLevel);

                return 1f + (curveValue * (skillEffectMultiplier - 1f));
            } else {
                return 1f + ((level - 1) * 0.2f);
            }
        }

        private void HandleSkillUnlocked(SkillData skill) {
            if (skill != null) {
                UnlockSkill(skill.skillName);
                SetSkillLevel(skill.skillName, skillStartLevel);
            }
        }

        private void SetSkillLevel(string skillName, int level) {
            if (!unlockedSkills.Contains(skillName)) {
                unlockedSkills.Add(skillName);
            }

            skillLevels[skillName] = Mathf.Clamp(level, 0, maxSkillLevel);

            OnSkillLevelChanged?.Invoke(skillName, skillLevels[skillName]);
        }

        public bool IsSkillUnlocked(string skillName) {
            return unlockedSkills.Contains(skillName);
        }

        public int GetSkillLevel(string skillName) {
            if (!skillLevels.ContainsKey(skillName))
                return 0;

            return skillLevels[skillName];
        }

        public float GetSkillEffectiveness(string skillName) {
            if (!skillLevels.ContainsKey(skillName))
                return 1f;

            int level = skillLevels[skillName];
            return CalculateScalingFactor(level);
        }

        public List<string> GetUnlockedSkills() {
            return new List<string>(unlockedSkills);
        }

        public Dictionary<string, int> GetAllSkillLevels() {
            return new Dictionary<string, int>(skillLevels);
        }

        public void ResetSkills() {
            int totalPoints = 0;

            foreach (var skill in skillLevels) {
                totalPoints += skill.Value;
            }

            skillLevels.Clear();
            unlockedSkills.Clear();

            if (levelManager != null) {
                levelManager.AddSkillPoints(totalPoints);
            }

            InitializeSkills();
        }
    }
}