using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Character.Classes {
    public abstract class BaseCharacterClass : MonoBehaviour {
        [Header("Class Identity")]
        [SerializeField]
        protected string className;
        [SerializeField]
        protected string classDescription;
        [SerializeField]
        protected Sprite classIcon;

        [Header("Base Stats")]
        [SerializeField]
        protected int baseHealth = 100;
        [SerializeField]
        protected int baseStamina = 100;
        [SerializeField]
        protected int baseStrength = 10;
        [SerializeField]
        protected int baseDexterity = 10;
        [SerializeField]
        protected int baseIntelligence = 10;
        [SerializeField]
        protected float baseAttackSpeed = 1.0f;
        [SerializeField]
        protected float baseMoveSpeed = 5.0f;

        [Header("Growth Stats")]
        [SerializeField]
        protected float healthPerLevel = 10f;
        [SerializeField]
        protected float staminaPerLevel = 5f;
        [SerializeField]
        protected float strengthPerLevel = 1f;
        [SerializeField]
        protected float dexterityPerLevel = 1f;
        [SerializeField]
        protected float intelligencePerLevel = 1f;

        [Header("Class Skills")]
        [SerializeField]
        protected List<SkillData> classSkills = new List<SkillData>();

        // References
        protected PlayerStats playerStats;
        protected PlayerCombat playerCombat;

        // Events
        public event Action<SkillData> OnSkillUnlocked;

        protected virtual void Awake() {
            playerStats = GetComponent<PlayerStats>();
            playerCombat = GetComponent<PlayerCombat>();

            if (playerStats == null) {
                Debug.LogError($"[{className}] PlayerStats component not found!");
            }

            if (playerCombat == null) {
                Debug.LogError($"[{className}] PlayerCombat component not found!");
            }
        }

        protected virtual void Start() {
            InitializeBaseStats();
            InitializeClassSkills();
        }

        protected virtual void InitializeBaseStats() {
            // Apply base stats to the player stats component
            // This would typically be implemented in a more sophisticated way with a data-driven approach
            // For this example, we're assuming PlayerStats has methods to set these values
        }

        protected virtual void InitializeClassSkills() {
            // Initialize skills based on starting level
            // Add base skills to the player's skill set
        }

        public virtual void OnLevelUp(int newLevel) {
            // Apply stat increases based on new level
            // Unlock new skills if appropriate
        }

        public virtual bool TryUseClassSkill(int skillIndex) {
            if (skillIndex < 0 || skillIndex >= classSkills.Count)
                return false;

            // Class-specific implementation for using skills
            return false;
        }

        public virtual string GetClassName() {
            return className;
        }

        public virtual string GetClassDescription() {
            return classDescription;
        }

        public virtual List<SkillData> GetClassSkills() {
            return classSkills;
        }

        public virtual SkillData GetSkill(int index) {
            if (index < 0 || index >= classSkills.Count)
                return null;

            return classSkills[index];
        }

        // Apply special class effects/bonuses that are unique to this class
        public abstract void ApplyClassEffects();
    }
}