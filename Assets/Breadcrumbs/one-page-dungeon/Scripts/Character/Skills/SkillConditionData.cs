using System;
using UnityEngine;

namespace Breadcrumbs.Character.Skills
{
    /// <summary>
    /// 스킬 사용 조건을 정의하는 클래스
    /// </summary>
    [Serializable]
    public class SkillConditionData
    {
        [SerializeField] private ConditionType conditionType;
        [SerializeField] private StatType targetStat;
        [SerializeField] private float thresholdValue;
        [SerializeField] private string requiredStatusEffect;
        [SerializeField] private bool inverted;
        
        // 프로퍼티
        public ConditionType ConditionType => conditionType;
        
        /// <summary>
        /// 캐릭터가 조건을 만족하는지 확인합니다.
        /// </summary>
        public bool CheckCondition(ICharacter character, int skillLevel)
        {
            if (character == null) return false;
            
            bool result = false;
            
            switch (conditionType)
            {
                case ConditionType.StatAboveThreshold:
                    result = character.Stats.GetStat(targetStat) >= thresholdValue;
                    break;
                    
                case ConditionType.StatBelowThreshold:
                    result = character.Stats.GetStat(targetStat) <= thresholdValue;
                    break;
                    
                case ConditionType.HealthPercentage:
                    float healthPercentage = character.Stats.CurrentHealth / character.Stats.GetStat(StatType.MaxHealth);
                    result = healthPercentage >= thresholdValue;
                    break;
                    
                case ConditionType.ManaPercentage:
                    float manaPercentage = character.Stats.CurrentMana / character.Stats.GetStat(StatType.MaxMana);
                    result = manaPercentage >= thresholdValue;
                    break;
                    
                case ConditionType.HasStatusEffect:
                    // 스킬 레벨이 1 이상이며, 필요한 상태 효과 구현 필요
                    result = !string.IsNullOrEmpty(requiredStatusEffect) && skillLevel > 0;
                    break;
                    
                case ConditionType.InCombat:
                    // 전투 상태 확인 로직 구현 필요
                    result = true;
                    break;
                    
                case ConditionType.None:
                    result = true;
                    break;
            }
            
            // 조건이 반전된 경우 결과를 반전
            return inverted ? !result : result;
        }
    }
    
    /// <summary>
    /// 스킬 사용 조건의 유형
    /// </summary>
    public enum ConditionType
    {
        None,                   // 조건 없음
        StatAboveThreshold,     // 특정 능력치가 임계값 이상
        StatBelowThreshold,     // 특정 능력치가 임계값 이하
        HealthPercentage,       // 체력 비율 조건
        ManaPercentage,         // 마나 비율 조건
        HasStatusEffect,        // 특정 상태 효과 보유
        InCombat                // 전투 중
    }
}
