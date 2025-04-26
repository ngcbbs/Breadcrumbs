using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Character {
    public class StatAllocationSystem : MonoBehaviour {
        [Header("Stat Allocation Settings")]
        [SerializeField]
        private int maxStatLevel = 50;
        [SerializeField]
        private int statPointCost = 1;
        [SerializeField]
        private int statPointCostIncreaseThreshold = 10;
        [SerializeField]
        private int statPointCostIncrease = 1;

        [Header("Stat Effects")]
        [SerializeField]
        private float strengthToDamage = 0.5f;
        [SerializeField]
        private float dexterityToSpeed = 0.01f;
        [SerializeField]
        private float intelligenceToMana = 2.0f;
        [SerializeField]
        private float constitutionToHealth = 5.0f;

        private CharacterLevelManager levelManager;
        private PlayerStats playerStats;

        private Dictionary<CharacterStat, int> baseStats = new Dictionary<CharacterStat, int>();
        private Dictionary<CharacterStat, int> allocatedPoints = new Dictionary<CharacterStat, int>();

        public event Action<CharacterStat, int> OnStatChanged;

        private void Awake() {
            levelManager = GetComponent<CharacterLevelManager>();
            playerStats = GetComponent<PlayerStats>();

            if (levelManager == null) {
                Debug.LogError("StatAllocationSystem: CharacterLevelManager component not found!");
            }

            if (playerStats == null) {
                Debug.LogError("StatAllocationSystem: PlayerStats component not found!");
            }

            InitializeBaseStats();
        }

        private void InitializeBaseStats() {
            foreach (CharacterStat stat in Enum.GetValues(typeof(CharacterStat))) {
                baseStats[stat] = GetDefaultStatValue(stat);
                allocatedPoints[stat] = 0;
            }

            ApplyAllStats();
        }

        private int GetDefaultStatValue(CharacterStat stat) {
            switch (stat) {
                case CharacterStat.Strength:
                    return 10;
                case CharacterStat.Dexterity:
                    return 10;
                case CharacterStat.Intelligence:
                    return 10;
                case CharacterStat.Constitution:
                    return 10;
                default:
                    return 10;
            }
        }

        public bool AllocateStatPoint(CharacterStat stat) {
            if (allocatedPoints[stat] >= maxStatLevel)
                return false;

            int cost = CalculateStatCost(stat);

            if (levelManager != null && !levelManager.UseStatPoints(cost))
                return false;

            allocatedPoints[stat]++;

            ApplyStat(stat);

            OnStatChanged?.Invoke(stat, GetTotalStatValue(stat));

            return true;
        }

        private int CalculateStatCost(CharacterStat stat) {
            int currentPoints = allocatedPoints[stat];

            int thresholdsPassed = currentPoints / statPointCostIncreaseThreshold;
            return statPointCost + (thresholdsPassed * statPointCostIncrease);
        }

        private void ApplyStat(CharacterStat stat) {
            if (playerStats == null)
                return;

            switch (stat) {
                case CharacterStat.Strength:
                    int strengthBonus = Mathf.RoundToInt(allocatedPoints[stat] * strengthToDamage);
                    playerStats.AddStatModifier(StatType.AttackDamage, strengthBonus);
                    break;
                case CharacterStat.Dexterity:
                    float dexterityBonus = allocatedPoints[stat] * dexterityToSpeed;
                    playerStats.AddStatModifier(StatType.AttackSpeed, dexterityBonus);
                    playerStats.AddStatModifier(StatType.MoveSpeed, dexterityBonus);
                    break;
                case CharacterStat.Intelligence:
                    // Assuming we would have a mana system in a full implementation
                    // int intelligenceBonus = Mathf.RoundToInt(allocatedPoints[stat] * intelligenceToMana);
                    // playerStats.AddStatModifier(StatType.Mana, intelligenceBonus);
                    break;
                case CharacterStat.Constitution:
                    int healthBonus = Mathf.RoundToInt(allocatedPoints[stat] * constitutionToHealth);
                    playerStats.AddStatModifier(StatType.Health, healthBonus);
                    break;
            }
        }

        private void ApplyAllStats() {
            foreach (CharacterStat stat in Enum.GetValues(typeof(CharacterStat))) {
                ApplyStat(stat);
                OnStatChanged?.Invoke(stat, GetTotalStatValue(stat));
            }
        }

        public int GetBaseStatValue(CharacterStat stat) {
            return baseStats.ContainsKey(stat) ? baseStats[stat] : 0;
        }

        public int GetAllocatedPoints(CharacterStat stat) {
            return allocatedPoints.ContainsKey(stat) ? allocatedPoints[stat] : 0;
        }

        public int GetTotalStatValue(CharacterStat stat) {
            return GetBaseStatValue(stat) + GetAllocatedPoints(stat);
        }

        public void ResetStats() {
            int totalPoints = 0;

            foreach (var stat in allocatedPoints) {
                totalPoints += stat.Value;
            }

            foreach (CharacterStat stat in Enum.GetValues(typeof(CharacterStat))) {
                allocatedPoints[stat] = 0;
            }

            // Reset stat modifiers
            if (playerStats != null) {
                playerStats.ResetStats();
            }

            // Apply base stats again
            ApplyAllStats();

            // Refund points
            if (levelManager != null) {
                levelManager.AddStatPoints(totalPoints);
            }
        }
    }

    public enum CharacterStat {
        Strength,
        Dexterity,
        Intelligence,
        Constitution
    }
}