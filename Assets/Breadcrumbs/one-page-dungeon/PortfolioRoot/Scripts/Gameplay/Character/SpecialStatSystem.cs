using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Character {
    public class SpecialStatSystem : MonoBehaviour {
        [Header("Special Stat Settings")]
        [SerializeField]
        private float criticalHitChanceBase = 0.05f;
        [SerializeField]
        private float criticalHitDamageBase = 1.5f;
        [SerializeField]
        private float dodgeChanceBase = 0.03f;
        [SerializeField]
        private float blockChanceBase = 0.1f;
        [SerializeField]
        private float lifeStealBase = 0.0f;
        [SerializeField]
        private float cooldownReductionBase = 0.0f;

        [Header("Stat Caps")]
        [SerializeField]
        private float criticalHitChanceMax = 0.7f;
        [SerializeField]
        private float criticalHitDamageMax = 3.0f;
        [SerializeField]
        private float dodgeChanceMax = 0.5f;
        [SerializeField]
        private float blockChanceMax = 0.75f;
        [SerializeField]
        private float lifeStealMax = 0.3f;
        [SerializeField]
        private float cooldownReductionMax = 0.5f;

        [Header("Stat Scaling")]
        [SerializeField]
        private float strengthToCritDamage = 0.01f;
        [SerializeField]
        private float dexterityToCritChance = 0.005f;
        [SerializeField]
        private float dexterityToDodge = 0.003f;
        [SerializeField]
        private float intelligenceToCooldown = 0.005f;

        private Dictionary<SpecialStat, float> baseStats = new Dictionary<SpecialStat, float>();
        private Dictionary<SpecialStat, float> currentStats = new Dictionary<SpecialStat, float>();
        private Dictionary<SpecialStat, List<StatModifier>> statModifiers = new Dictionary<SpecialStat, List<StatModifier>>();

        private PlayerStats playerStats;
        private StatAllocationSystem statAllocationSystem;

        public event Action<SpecialStat, float> OnSpecialStatChanged;

        private void Awake() {
            playerStats = GetComponent<PlayerStats>();
            statAllocationSystem = GetComponent<StatAllocationSystem>();

            InitializeSpecialStats();
        }

        private void Start() {
            CalculateStatsFromAttributes();

            if (statAllocationSystem != null) {
                statAllocationSystem.OnStatChanged += OnAttributeChanged;
            }
        }

        private void OnDestroy() {
            if (statAllocationSystem != null) {
                statAllocationSystem.OnStatChanged -= OnAttributeChanged;
            }
        }

        private void InitializeSpecialStats() {
            foreach (SpecialStat stat in Enum.GetValues(typeof(SpecialStat))) {
                baseStats[stat] = GetBaseStatValue(stat);
                currentStats[stat] = baseStats[stat];
                statModifiers[stat] = new List<StatModifier>();
            }
        }

        private float GetBaseStatValue(SpecialStat stat) {
            switch (stat) {
                case SpecialStat.CriticalHitChance:
                    return criticalHitChanceBase;
                case SpecialStat.CriticalHitDamage:
                    return criticalHitDamageBase;
                case SpecialStat.DodgeChance:
                    return dodgeChanceBase;
                case SpecialStat.BlockChance:
                    return blockChanceBase;
                case SpecialStat.LifeSteal:
                    return lifeStealBase;
                case SpecialStat.CooldownReduction:
                    return cooldownReductionBase;
                default:
                    return 0;
            }
        }

        private float GetStatCap(SpecialStat stat) {
            switch (stat) {
                case SpecialStat.CriticalHitChance:
                    return criticalHitChanceMax;
                case SpecialStat.CriticalHitDamage:
                    return criticalHitDamageMax;
                case SpecialStat.DodgeChance:
                    return dodgeChanceMax;
                case SpecialStat.BlockChance:
                    return blockChanceMax;
                case SpecialStat.LifeSteal:
                    return lifeStealMax;
                case SpecialStat.CooldownReduction:
                    return cooldownReductionMax;
                default:
                    return 1.0f;
            }
        }

        public void AddStatModifier(SpecialStat stat, StatModifier modifier) {
            if (!statModifiers.ContainsKey(stat)) {
                statModifiers[stat] = new List<StatModifier>();
            }

            statModifiers[stat].Add(modifier);
            RecalculateStat(stat);
        }

        public bool RemoveStatModifier(SpecialStat stat, int modifierId) {
            if (!statModifiers.ContainsKey(stat))
                return false;

            for (int i = 0; i < statModifiers[stat].Count; i++) {
                if (statModifiers[stat][i].id == modifierId) {
                    statModifiers[stat].RemoveAt(i);
                    RecalculateStat(stat);
                    return true;
                }
            }

            return false;
        }

        public void ClearStatModifiers(SpecialStat stat) {
            if (statModifiers.ContainsKey(stat)) {
                statModifiers[stat].Clear();
                RecalculateStat(stat);
            }
        }

        public void ClearAllStatModifiers() {
            foreach (SpecialStat stat in Enum.GetValues(typeof(SpecialStat))) {
                ClearStatModifiers(stat);
            }
        }

        private void RecalculateStat(SpecialStat stat) {
            float baseValue = baseStats[stat];
            float finalValue = baseValue;

            // First apply additive modifiers
            foreach (var mod in statModifiers[stat]) {
                if (mod.type == StatModifierType.Additive) {
                    finalValue += mod.value;
                }
            }

            // Then apply multiplicative modifiers
            float multiplier = 1.0f;
            foreach (var mod in statModifiers[stat]) {
                if (mod.type == StatModifierType.Multiplicative) {
                    multiplier *= (1.0f + mod.value);
                }
            }

            finalValue *= multiplier;

            // Apply attribute-based bonuses
            finalValue += GetAttributeBonus(stat);

            // Apply caps
            finalValue = Mathf.Clamp(finalValue, 0, GetStatCap(stat));

            // Update stat and notify
            if (Mathf.Abs(currentStats[stat] - finalValue) > 0.001f) {
                currentStats[stat] = finalValue;
                OnSpecialStatChanged?.Invoke(stat, finalValue);
            }
        }

        private void CalculateStatsFromAttributes() {
            if (statAllocationSystem == null)
                return;

            foreach (SpecialStat stat in Enum.GetValues(typeof(SpecialStat))) {
                RecalculateStat(stat);
            }
        }

        private float GetAttributeBonus(SpecialStat stat) {
            if (statAllocationSystem == null)
                return 0f;

            switch (stat) {
                case SpecialStat.CriticalHitChance:
                    return statAllocationSystem.GetTotalStatValue(CharacterStat.Dexterity) * dexterityToCritChance;

                case SpecialStat.CriticalHitDamage:
                    return statAllocationSystem.GetTotalStatValue(CharacterStat.Strength) * strengthToCritDamage;

                case SpecialStat.DodgeChance:
                    return statAllocationSystem.GetTotalStatValue(CharacterStat.Dexterity) * dexterityToDodge;

                case SpecialStat.CooldownReduction:
                    return statAllocationSystem.GetTotalStatValue(CharacterStat.Intelligence) * intelligenceToCooldown;

                default:
                    return 0f;
            }
        }

        private void OnAttributeChanged(CharacterStat attribute, int value) {
            CalculateStatsFromAttributes();
        }

        public float GetSpecialStat(SpecialStat stat) {
            return currentStats.ContainsKey(stat) ? currentStats[stat] : 0f;
        }

        public bool RollForStat(SpecialStat stat) {
            float value = GetSpecialStat(stat);
            return UnityEngine.Random.value <= value;
        }

        public float GetDamageMultiplier(bool isCritical) {
            if (isCritical) {
                return GetSpecialStat(SpecialStat.CriticalHitDamage);
            }

            return 1.0f;
        }

        public float CalculateLifeSteal(float damage) {
            float lifeSteal = GetSpecialStat(SpecialStat.LifeSteal);
            return damage * lifeSteal;
        }

        public float ApplyCooldownReduction(float baseCooldown) {
            float reduction = GetSpecialStat(SpecialStat.CooldownReduction);
            return baseCooldown * (1.0f - reduction);
        }
    }

    public enum SpecialStat {
        CriticalHitChance,
        CriticalHitDamage,
        DodgeChance,
        BlockChance,
        LifeSteal,
        CooldownReduction
    }

    public enum StatModifierType {
        Additive,
        Multiplicative
    }

    public class StatModifier {
        public int id;
        public float value;
        public StatModifierType type;
        public object source;

        private static int nextId = 1;

        public StatModifier(float value, StatModifierType type, object source = null) {
            this.id = nextId++;
            this.value = value;
            this.type = type;
            this.source = source;
        }
    }
}