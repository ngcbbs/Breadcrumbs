using System;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Character.Classes;

namespace GamePortfolio.Gameplay.Character {
    public class CharacterLevelManager : MonoBehaviour {
        [Header("Level Settings")]
        [SerializeField]
        private int maxLevel = 30;
        [SerializeField]
        private int startingLevel = 1;
        [SerializeField]
        private int startingExperience = 0;
        [SerializeField]
        private AnimationCurve experienceCurve;
        [SerializeField]
        private float experienceMultiplier = 1.5f;

        [Header("Level Rewards")]
        [SerializeField]
        private int skillPointsPerLevel = 1;
        [SerializeField]
        private int statsPointsPerLevel = 3;
        [SerializeField]
        private int extraSkillPointsAt = 5; // Extra skill point every X levels

        [Header("UI References")]
        [SerializeField]
        private GameObject levelUpEffect;

        // Components
        private PlayerStats playerStats;
        private BaseCharacterClass characterClass;

        // Level tracking
        private int currentLevel;
        private int currentExperience;
        private int experienceToNextLevel;
        private int unspentSkillPoints;
        private int unspentStatPoints;

        // Events
        public event Action<int> OnLevelUp;
        public event Action<int, int> OnExperienceChanged;
        public event Action<int> OnSkillPointsChanged;
        public event Action<int> OnStatPointsChanged;

        private void Awake() {
            playerStats = GetComponent<PlayerStats>();
            characterClass = GetComponent<BaseCharacterClass>();

            if (playerStats == null) {
                Debug.LogError("CharacterLevelManager: PlayerStats component not found!");
            }

            if (characterClass == null) {
                Debug.LogError("CharacterLevelManager: CharacterClass component not found!");
            }
        }

        private void Start() {
            InitializeLevel();

            // Subscribe to events
            if (playerStats != null) {
                playerStats.OnDeath += OnPlayerDeath;
            }
        }

        private void OnDestroy() {
            // Unsubscribe from events
            if (playerStats != null) {
                playerStats.OnDeath -= OnPlayerDeath;
            }
        }

        public void InitializeLevel() {
            // Set starting level
            currentLevel = startingLevel;
            currentExperience = startingExperience;

            // Calculate XP for next level
            experienceToNextLevel = CalculateExperienceForLevel(currentLevel + 1);

            // Initialize unspent points
            unspentSkillPoints = (currentLevel - 1) * skillPointsPerLevel;
            unspentStatPoints = (currentLevel - 1) * statsPointsPerLevel;

            // Add bonus skill points for milestone levels
            if (currentLevel > 1) {
                int milestoneLevels = (currentLevel - 1) / extraSkillPointsAt;
                unspentSkillPoints += milestoneLevels;
            }

            // Notify listeners
            OnLevelUp?.Invoke(currentLevel);
            OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
            OnSkillPointsChanged?.Invoke(unspentSkillPoints);
            OnStatPointsChanged?.Invoke(unspentStatPoints);
        }

        public void AddExperience(int amount) {
            if (currentLevel >= maxLevel)
                return;

            int previousExperience = currentExperience;
            currentExperience += amount;

            // Check for level up
            while (currentExperience >= experienceToNextLevel && currentLevel < maxLevel) {
                LevelUp();
            }

            // Cap experience at max level
            if (currentLevel >= maxLevel) {
                currentExperience = experienceToNextLevel;
            }

            // Notify if experience changed
            if (previousExperience != currentExperience) {
                OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
            }
        }

        private void LevelUp() {
            // Increase level
            currentLevel++;

            // Award skill and stat points
            unspentSkillPoints += skillPointsPerLevel;
            unspentStatPoints += statsPointsPerLevel;

            // Award bonus skill point at milestone levels
            if (currentLevel % extraSkillPointsAt == 0) {
                unspentSkillPoints += 1;
            }

            // Calculate new experience threshold
            currentExperience -= experienceToNextLevel;
            experienceToNextLevel = CalculateExperienceForLevel(currentLevel + 1);

            // Notify character class about level up
            if (characterClass != null) {
                characterClass.OnLevelUp(currentLevel);
            }

            // Play level up effect
            if (levelUpEffect != null) {
                Instantiate(levelUpEffect, transform.position + Vector3.up, Quaternion.identity, transform);
            }

            // Play level up sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("LevelUp");
            }

            // Notify listeners
            OnLevelUp?.Invoke(currentLevel);
            OnSkillPointsChanged?.Invoke(unspentSkillPoints);
            OnStatPointsChanged?.Invoke(unspentStatPoints);
        }

        private int CalculateExperienceForLevel(int level) {
            if (level <= 1)
                return 100;

            if (experienceCurve != null) {
                // Use curve to determine XP scaling (normalized 0-1 input)
                float normalizedLevel = (float)(level - 1) / (maxLevel - 1);
                float curveValue = experienceCurve.Evaluate(normalizedLevel);

                // Base experience plus curve-based scaling
                return Mathf.RoundToInt(100 * Mathf.Pow(experienceMultiplier, level - 1) * curveValue);
            } else {
                // Fallback to simple exponential scaling
                return Mathf.RoundToInt(100 * Mathf.Pow(experienceMultiplier, level - 1));
            }
        }

        public bool UseSkillPoint() {
            if (unspentSkillPoints > 0) {
                unspentSkillPoints--;
                OnSkillPointsChanged?.Invoke(unspentSkillPoints);
                return true;
            }

            return false;
        }

        public bool UseStatPoints(int amount) {
            if (unspentStatPoints >= amount) {
                unspentStatPoints -= amount;
                OnStatPointsChanged?.Invoke(unspentStatPoints);
                return true;
            }

            return false;
        }

        public void AddSkillPoints(int amount) {
            if (amount <= 0) return;

            unspentSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(unspentSkillPoints);
        }

        public void AddStatPoints(int amount) {
            if (amount <= 0) return;

            unspentStatPoints += amount;
            OnStatPointsChanged?.Invoke(unspentStatPoints);
        }

        private void OnPlayerDeath() {
            // Handle death penalties if any
            // For example, lose some experience but not level

            // In a permadeath game, this might reset the character
            // In other games, it might have no effect on XP/level
        }

        // Getters
        public int GetCurrentLevel() {
            return currentLevel;
        }

        public int GetCurrentExperience() {
            return currentExperience;
        }

        public int GetExperienceToNextLevel() {
            return experienceToNextLevel;
        }

        public int GetUnspentSkillPoints() {
            return unspentSkillPoints;
        }

        public int GetUnspentStatPoints() {
            return unspentStatPoints;
        }

        public float GetLevelProgress() {
            return (float)currentExperience / experienceToNextLevel;
        }
    }
}