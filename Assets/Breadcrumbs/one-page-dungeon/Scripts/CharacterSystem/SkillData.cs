using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [CreateAssetMenu(fileName = "SkillData", menuName = "RPG/Skill Data")]
    public class SkillData : ScriptableObject {
        [Header("기본 정보")]
        public string skillId;
        public string skillName;
        [TextArea]
        public string description;
        public Sprite icon;

        [Header("스킬 속성")]
        public SkillType skillType;
        public TargetType targetType;
        public ElementType elementType;

        [Header("요구사항")]
        public ClassType requiredClass; // None이면 모든 클래스 사용 가능
        public int requiredLevel;
        public string[] requiredSkills; // 선행 스킬

        [Header("스킬 비용")]
        public float manaCost;
        public float cooldown;
        public int chargeCount; // 0이면 무제한

        [Header("스킬 효과")]
        public float basePower;       // 기본 위력
        public float baseRange;       // 기본 범위
        public float castTime;        // 시전 시간
        public bool isChanneled;      // 채널링 스킬 여부
        public float channelDuration; // 채널링 지속 시간

        [Header("스킬 강화")]
        public float[] powerPerLevel; // 레벨당 위력 증가
        public float[] rangePerLevel; // 레벨당 범위 증가
        public float[] costPerLevel;  // 레벨당 비용 증가

        [Header("시각 효과")]
        public GameObject castEffect;   // 시전 이펙트
        public GameObject impactEffect; // 적중 이펙트
        public AudioClip castSound;     // 시전 사운드
        public AudioClip impactSound;   // 적중 사운드

        // 특수 효과 데이터
        [Serializable]
        public class EffectData {
            public string effectName;
            public float chance;   // 발동 확률 (0-1)
            public float power;    // 효과 위력
            public float duration; // 지속시간 (초)
        }

        [Header("부가 효과")]
        public List<EffectData> additionalEffects = new List<EffectData>();

        // 스킬 파워 계산
        public float CalculatePower(int skillLevel) {
            float power = basePower;

            if (powerPerLevel != null && skillLevel > 0 && skillLevel <= powerPerLevel.Length) {
                power += powerPerLevel[skillLevel - 1];
            }

            return power;
        }

        // 실제 마나 코스트 계산
        public float CalculateManaCost(int skillLevel) {
            float cost = manaCost;

            if (costPerLevel != null && skillLevel > 0 && skillLevel <= costPerLevel.Length) {
                cost += costPerLevel[skillLevel - 1];
            }

            return cost;
        }
    }
}