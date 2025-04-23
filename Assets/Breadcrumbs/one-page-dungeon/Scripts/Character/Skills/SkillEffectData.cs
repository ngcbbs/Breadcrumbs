using System;
using UnityEngine;

namespace Breadcrumbs.Character.Skills
{
    /// <summary>
    /// 스킬 효과의 데이터를 저장하는 클래스
    /// </summary>
    [Serializable]
    public class SkillEffectData
    {
        [SerializeField] private string effectName;
        [SerializeField] private EffectType effectType;
        [SerializeField] private float baseValue;
        [SerializeField] private float valuePerLevel;
        [SerializeField] private float duration;
        [SerializeField] private float durationPerLevel;
        [SerializeField] private StatType targetStat;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        
        // 프로퍼티
        public string EffectName => effectName;
        public EffectType EffectType => effectType;
        public float BaseValue => baseValue;
        public float ValuePerLevel => valuePerLevel;
        public float Duration => duration;
        public float DurationPerLevel => durationPerLevel;
        public StatType TargetStat => targetStat;
        public DamageType DamageType => damageType;
        
        /// <summary>
        /// 특정 레벨에서의 효과 값을 계산합니다.
        /// </summary>
        public float GetValue(int skillLevel)
        {
            if (skillLevel <= 0) skillLevel = 1;
            return baseValue + (skillLevel - 1) * valuePerLevel;
        }
        
        /// <summary>
        /// 특정 레벨에서의 지속시간을 계산합니다.
        /// </summary>
        public float GetDuration(int skillLevel)
        {
            if (skillLevel <= 0) skillLevel = 1;
            return duration + (skillLevel - 1) * durationPerLevel;
        }
    }
    
    /// <summary>
    /// 스킬 효과의 유형
    /// </summary>
    public enum EffectType
    {
        Damage,             // 데미지
        Heal,               // 회복
        StatBoost,          // 능력치 증가
        StatReduction,      // 능력치 감소
        StatusEffect,       // 상태 효과 (스턴, 빙결 등)
        Movement,           // 이동 (점프, 대쉬 등)
        Shield,             // 보호막
        Summon,             // 소환
        Taunt,              // 도발
        CrowdControl        // 군중 제어
    }
}
