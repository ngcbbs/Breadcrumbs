using System;
using UnityEngine;

namespace Breadcrumbs.Character.Skills
{
    /// <summary>
    /// 캐릭터가 소유하는 구체적인 스킬 인스턴스
    /// </summary>
    [Serializable]
    public class Skill
    {
        [SerializeField] private string skillId;
        [SerializeField] private int skillLevel;
        [SerializeField] private float remainingCooldown;
        [SerializeField] private bool isLearned;
        [SerializeField] private bool isActive;
        
        // 스킬 정의 참조 (런타임에만 사용)
        private SkillDefinition skillDefinition;
        
        // 프로퍼티
        public string SkillId => skillId;
        public int SkillLevel => skillLevel;
        public float RemainingCooldown => remainingCooldown;
        public bool IsLearned => isLearned;
        public bool IsActive => isActive;
        public bool IsOnCooldown => remainingCooldown > 0;
        public SkillDefinition Definition => skillDefinition;
        
        /// <summary>
        /// 새 스킬을 생성합니다.
        /// </summary>
        public Skill(string skillId, SkillDefinition definition)
        {
            this.skillId = skillId;
            this.skillDefinition = definition;
            skillLevel = 0;
            remainingCooldown = 0;
            isLearned = false;
            isActive = false;
        }
        
        /// <summary>
        /// 스킬 정의를 설정합니다.
        /// </summary>
        public void SetDefinition(SkillDefinition definition)
        {
            if (definition != null && definition.SkillId == skillId)
            {
                skillDefinition = definition;
            }
        }
        
        /// <summary>
        /// 스킬을 배웁니다.
        /// </summary>
        public void Learn()
        {
            if (!isLearned)
            {
                isLearned = true;
                skillLevel = 1;
            }
        }
        
        /// <summary>
        /// 스킬 레벨을 높입니다.
        /// </summary>
        public bool Upgrade()
        {
            if (isLearned && skillDefinition != null && skillLevel < skillDefinition.MaxLevel)
            {
                skillLevel++;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 스킬을 사용합니다.
        /// </summary>
        public bool Use(ICharacter caster, Transform target)
        {
            if (!isLearned || skillLevel <= 0 || remainingCooldown > 0 || skillDefinition == null)
            {
                return false;
            }
            
            // 스킬 사용 조건 확인
            if (!skillDefinition.CanUseSkill(caster, skillLevel))
            {
                return false;
            }
            
            // 마나 비용 차감
            int manaCost = skillDefinition.GetManaCost(skillLevel);
            caster.Stats.CurrentMana -= manaCost;
            
            // 쿨다운 설정
            remainingCooldown = skillDefinition.GetCooldown(skillLevel);
            
            // 토글 스킬인 경우 상태 전환
            if (skillDefinition.SkillType == Services.SkillType.Toggle)
            {
                isActive = !isActive;
            }
            
            return true;
        }
        
        /// <summary>
        /// 스킬 쿨다운을 업데이트합니다.
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (remainingCooldown > 0)
            {
                remainingCooldown = Mathf.Max(0, remainingCooldown - deltaTime);
            }
        }
        
        /// <summary>
        /// 스킬 쿨다운을 리셋합니다.
        /// </summary>
        public void ResetCooldown()
        {
            remainingCooldown = 0;
        }
        
        /// <summary>
        /// 토글 스킬을 비활성화합니다.
        /// </summary>
        public void Deactivate()
        {
            if (isActive)
            {
                isActive = false;
            }
        }
    }
}
