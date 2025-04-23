using System;
using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Character.Services;

namespace Breadcrumbs.Character.Skills
{
    /// <summary>
    /// 스킬의 핵심 정의를 담당하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Breadcrumbs/Skills/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string skillId;
        [SerializeField] private string skillName;
        [SerializeField] [TextArea(3, 5)] private string description;
        [SerializeField] private SkillType skillType;
        [SerializeField] private Sprite icon;
        
        [Header("요구사항")]
        [SerializeField] private int requiredLevel = 1;
        [SerializeField] private ClassType classRequirement;
        [SerializeField] private string[] prerequisiteSkills;
        
        [Header("비용 및 쿨다운")]
        [SerializeField] private int baseManaСost;
        [SerializeField] private float baseCooldown;
        [SerializeField] private int maxLevel = 1;
        
        [Header("레벨당 증가치")]
        [SerializeField] private int manaСostPerLevel;
        [SerializeField] private float cooldownReductionPerLevel;
        
        [Header("효과 및 대상")]
        [SerializeField] private TargetType targetType;
        [SerializeField] private float baseRange;
        [SerializeField] private float baseRadius;
        [SerializeField] private float baseDuration;
        
        [Header("효과 스케일링")]
        [SerializeField] private StatType primaryScalingStat;
        [SerializeField] private float primaryScalingFactor = 1.0f;
        [SerializeField] private StatType secondaryScalingStat;
        [SerializeField] private float secondaryScalingFactor = 0.5f;
        
        [Header("효과 구성요소")]
        [SerializeField] private List<SkillEffectData> effects = new List<SkillEffectData>();
        [SerializeField] private List<SkillConditionData> conditions = new List<SkillConditionData>();
        
        // 프로퍼티
        public string SkillId => skillId;
        public string SkillName => skillName;
        public string Description => description;
        public SkillType SkillType => skillType;
        public Sprite Icon => icon;
        public int RequiredLevel => requiredLevel;
        public ClassType ClassRequirement => classRequirement;
        public string[] PrerequisiteSkills => prerequisiteSkills;
        public int MaxLevel => maxLevel;
        public TargetType TargetType => targetType;
        
        /// <summary>
        /// 특정 레벨에서의 마나 비용을 계산합니다.
        /// </summary>
        public int GetManaCost(int skillLevel)
        {
            if (skillLevel <= 0) return 0;
            return baseManaСost + (skillLevel - 1) * manaСostPerLevel;
        }
        
        /// <summary>
        /// 특정 레벨에서의 쿨다운을 계산합니다.
        /// </summary>
        public float GetCooldown(int skillLevel)
        {
            if (skillLevel <= 0) return 0;
            float reduction = (skillLevel - 1) * cooldownReductionPerLevel;
            return Mathf.Max(0.5f, baseCooldown - reduction);
        }
        
        /// <summary>
        /// 특정 레벨에서의 범위를 계산합니다.
        /// </summary>
        public float GetRange(int skillLevel)
        {
            return baseRange;
        }
        
        /// <summary>
        /// 특정 레벨에서의 반경을 계산합니다.
        /// </summary>
        public float GetRadius(int skillLevel)
        {
            return baseRadius;
        }
        
        /// <summary>
        /// 특정 레벨에서의 지속시간을 계산합니다.
        /// </summary>
        public float GetDuration(int skillLevel)
        {
            return baseDuration;
        }
        
        /// <summary>
        /// 스킬에 적용할 효과 데이터를 반환합니다.
        /// </summary>
        public IReadOnlyList<SkillEffectData> GetEffects()
        {
            return effects;
        }
        
        /// <summary>
        /// 스킬 사용 조건 데이터를 반환합니다.
        /// </summary>
        public IReadOnlyList<SkillConditionData> GetConditions()
        {
            return conditions;
        }
        
        /// <summary>
        /// 스킬 설명을 생성합니다 (레벨에 따른 값 포함).
        /// </summary>
        public string GetFormattedDescription(int skillLevel)
        {
            if (skillLevel <= 0) skillLevel = 1;
            
            string result = description;
            
            // 설명에 포함된 변수를 교체합니다
            foreach (var effect in effects)
            {
                float value = effect.GetValue(skillLevel);
                result = result.Replace($"{{{effect.EffectName}}}", value.ToString("F1"));
            }
            
            // 추가 변수도 교체합니다
            result = result.Replace("{ManaCost}", GetManaCost(skillLevel).ToString());
            result = result.Replace("{Cooldown}", GetCooldown(skillLevel).ToString("F1"));
            result = result.Replace("{Duration}", GetDuration(skillLevel).ToString("F1"));
            
            return result;
        }
        
        /// <summary>
        /// 스킬 사용 가능 여부를 검사합니다.
        /// </summary>
        public bool CanUseSkill(ICharacter character, int skillLevel)
        {
            if (character == null || skillLevel <= 0)
                return false;
                
            // 조건 검사
            foreach (var condition in conditions)
            {
                if (!condition.CheckCondition(character, skillLevel))
                    return false;
            }
            
            // 마나 비용 검사
            if (character.Stats.CurrentMana < GetManaCost(skillLevel))
                return false;
                
            return true;
        }
        
        /// <summary>
        /// 기본 데이터 유효성 검사
        /// </summary>
        private void OnValidate()
        {
            // skillId가 비어있으면 자동으로 생성
            if (string.IsNullOrEmpty(skillId))
            {
                skillId = Guid.NewGuid().ToString();
            }
            
            // 기본값 설정
            baseCooldown = Mathf.Max(0.5f, baseCooldown);
            baseManaСost = Mathf.Max(0, baseManaСost);
            maxLevel = Mathf.Max(1, maxLevel);
        }
    }
    
    /// <summary>
    /// 스킬의 대상 유형
    /// </summary>
    public enum TargetType
    {
        Self,           // 자기 자신
        SingleAlly,     // 단일 아군
        SingleEnemy,    // 단일 적
        AllAllies,      // 모든 아군
        AllEnemies,     // 모든 적
        Area,           // 영역
        Direction       // 방향성
    }
}
