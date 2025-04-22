using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class CharacterStats {
        // 스탯 딕셔너리
        private Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

        // 현재 리소스
        [SerializeField]
        private float currentHealth;
        [SerializeField]
        private float currentMana;

        // 경험치
        [SerializeField]
        private int level = 1;
        [SerializeField]
        private int experience = 0;
        [SerializeField]
        private int experienceToNextLevel = 100;

        // 프로퍼티
        public float CurrentHealth {
            get => currentHealth;
            set => currentHealth = Mathf.Clamp(value, 0, GetStat(StatType.MaxHealth));
        }
        public float CurrentMana {
            get => currentMana;
            set => currentMana = Mathf.Clamp(value, 0, GetStat(StatType.MaxMana));
        }
        public int Level => level;
        public int Experience => experience;
        public int ExperienceToNextLevel => experienceToNextLevel;

        // 생성자
        public CharacterStats() {
            // 모든 스탯 초기화
            foreach (StatType type in Enum.GetValues(typeof(StatType))) {
                stats[type] = new Stat();
            }
        }

        // 특정 스탯 값 얻기
        public float GetStat(StatType type) {
            if (stats.TryGetValue(type, out Stat stat)) {
                return stat.Value;
            }

            return 0;
        }

        // 특정 스탯 참조 얻기
        public Stat GetStatReference(StatType type) {
            if (stats.TryGetValue(type, out Stat stat)) {
                return stat;
            }

            return null;
        }

        // 스탯 값 설정
        public void SetBaseStat(StatType type, float value) {
            if (stats.TryGetValue(type, out Stat stat)) {
                stat.BaseValue = value;
            }
        }

        // 스탯 보너스 추가
        public void AddBonus(StatType type, float value) {
            if (stats.TryGetValue(type, out Stat stat)) {
                stat.BonusValue += value;
            }
        }

        // 스탯 수정자 추가
        public void AddModifier(StatType type, StatModifier modifier) {
            if (stats.TryGetValue(type, out Stat stat)) {
                stat.AddModifier(modifier);
            }
        }

        // 특정 출처의 모든 수정자 제거
        public void RemoveAllModifiersFromSource(object source) {
            foreach (var stat in stats.Values) {
                stat.RemoveAllModifiersFromSource(source);
            }
        }

        // 경험치 추가 및 레벨업 처리
        public bool AddExperience(int amount) {
            experience += amount;
            bool leveledUp = false;

            // 레벨업 체크
            while (experience >= experienceToNextLevel) {
                experience -= experienceToNextLevel;
                leveledUp = true;
                LevelUp();
            }

            return leveledUp;
        }

        // 레벨업 처리
        private void LevelUp() {
            level++;
            // 다음 레벨 필요 경험치 계산 (로그 스케일)
            experienceToNextLevel = (int)(experienceToNextLevel * 1.5f);
        }

        // 전투 시작시 초기화
        public void InitializeForCombat() {
            CurrentHealth = GetStat(StatType.MaxHealth);
            CurrentMana = GetStat(StatType.MaxMana);
        }

        // 스탯 업데이트 (초당)
        public void UpdateStats(float deltaTime) {
            // 체력 재생
            float healthRegen = GetStat(StatType.HealthRegen) * deltaTime;
            CurrentHealth += healthRegen;

            // 마나 재생
            float manaRegen = GetStat(StatType.ManaRegen) * deltaTime;
            CurrentMana += manaRegen;
        }
    }
}