using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Character.Services;

namespace Breadcrumbs.Character.Skills.Examples
{
    /// <summary>
    /// 전사 클래스의 스킬 정의 예시
    /// </summary>
    public class WarriorSkills : MonoBehaviour
    {
        [Header("기본 공격 스킬")]
        [SerializeField] private SkillDefinition slashSkill;
        [SerializeField] private SkillDefinition whirlwindSkill;
        
        [Header("방어 스킬")]
        [SerializeField] private SkillDefinition blockSkill;
        [SerializeField] private SkillDefinition guardStanceSkill;
        
        [Header("특수 스킬")]
        [SerializeField] private SkillDefinition berserkerRageSkill;
        [SerializeField] private SkillDefinition heroicLeapSkill;
        
        // 의존성 주입
        private ISkillService skillService;
        
        private void Awake()
        {
            // 필요한 서비스 참조 가져오기
            skillService = GetComponent<ISkillService>();
            
            if (skillService == null)
            {
                skillService = FindObjectOfType<SkillService>();
            }
            
            if (skillService == null)
            {
                Debug.LogError("SkillService를 찾을 수 없습니다.");
            }
        }
        
        /// <summary>
        /// 모든 전사 스킬을 생성하고 정의합니다.
        /// </summary>
        public void CreateWarriorSkills()
        {
            // 각 스킬 정의 생성 - 실제로는 ScriptableObject를 통해 에디터에서 정의
            
            // 1. 슬래시 (기본 공격)
            if (slashSkill == null)
            {
                slashSkill = CreateSkillDefinition(
                    "skill_warrior_slash",
                    "강력한 베기",
                    "적에게 {damage} 피해를 입힙니다.",
                    SkillType.Active,
                    ClassType.Warrior,
                    1,  // 요구 레벨
                    10, // 마나 비용
                    1.0f // 쿨다운
                );
                
                // 데미지 효과 추가
                AddDamageEffect(slashSkill, "damage", 40, 10);
            }
            
            // 2. 회오리 베기
            if (whirlwindSkill == null)
            {
                whirlwindSkill = CreateSkillDefinition(
                    "skill_warrior_whirlwind",
                    "회오리 베기",
                    "주변의 모든 적에게 {damage} 피해를 입힙니다.",
                    SkillType.Active,
                    ClassType.Warrior,
                    4,  // 요구 레벨
                    25, // 마나 비용
                    8.0f // 쿨다운
                );
                
                // 데미지 효과 추가
                AddDamageEffect(whirlwindSkill, "damage", 30, 15);
            }
            
            // 3. 방어
            if (blockSkill == null)
            {
                blockSkill = CreateSkillDefinition(
                    "skill_warrior_block",
                    "방패 막기",
                    "{duration}초 동안 물리 데미지를 {reduction}% 감소시킵니다.",
                    SkillType.Active,
                    ClassType.Warrior,
                    2,  // 요구 레벨
                    15, // 마나 비용
                    15.0f // 쿨다운
                );
                
                // 방어력 증가 효과 추가
                AddStatBoostEffect(blockSkill, "reduction", 30, 5, StatType.PhysicalDefense, 5);
            }
            
            // 4. 수비 태세
            if (guardStanceSkill == null)
            {
                guardStanceSkill = CreateSkillDefinition(
                    "skill_warrior_guard_stance",
                    "수비 태세",
                    "전투 태세를 전환하여 물리 방어력을 {defBoost}% 증가시키고 공격력을 {atkReduction}% 감소시킵니다.",
                    SkillType.Toggle,
                    ClassType.Warrior,
                    6,  // 요구 레벨
                    20, // 마나 비용
                    2.0f // 쿨다운
                );
                
                // 방어력 증가 효과 추가
                AddStatBoostEffect(guardStanceSkill, "defBoost", 50, 10, StatType.PhysicalDefense, 0);
                
                // 공격력 감소 효과 추가
                AddStatReductionEffect(guardStanceSkill, "atkReduction", 20, 2, StatType.PhysicalAttack, 0);
            }
            
            // 5. 광전사의 분노
            if (berserkerRageSkill == null)
            {
                berserkerRageSkill = CreateSkillDefinition(
                    "skill_warrior_berserker_rage",
                    "광전사의 분노",
                    "{duration}초 동안 공격력이 {atkBoost}% 증가하지만, 방어력이 {defReduction}% 감소합니다.",
                    SkillType.Active,
                    ClassType.Warrior,
                    8,  // 요구 레벨
                    35, // 마나 비용
                    120.0f // 쿨다운
                );
                
                // 공격력 증가 효과 추가
                AddStatBoostEffect(berserkerRageSkill, "atkBoost", 60, 10, StatType.PhysicalAttack, 15);
                
                // 방어력 감소 효과 추가
                AddStatReductionEffect(berserkerRageSkill, "defReduction", 30, 0, StatType.PhysicalDefense, 15);
            }
            
            // 6. 영웅의 도약
            if (heroicLeapSkill == null)
            {
                heroicLeapSkill = CreateSkillDefinition(
                    "skill_warrior_heroic_leap",
                    "영웅의 도약",
                    "대상 지점으로 도약하여 착지 시 주변 적들에게 {damage} 피해를 입히고 {stunDuration}초 동안 기절시킵니다.",
                    SkillType.Active,
                    ClassType.Warrior,
                    10, // 요구 레벨
                    40, // 마나 비용
                    45.0f // 쿨다운
                );
                
                // 데미지 효과 추가
                AddDamageEffect(heroicLeapSkill, "damage", 80, 20);
                
                // 기절 효과 추가
                AddStatusEffect(heroicLeapSkill, "stunDuration", 2, 0.2f);
            }
        }
        
        /// <summary>
        /// 스킬 정의를 생성합니다. (실제로는 ScriptableObject를 사용)
        /// </summary>
        private SkillDefinition CreateSkillDefinition(
            string id, string name, string description, 
            SkillType type, ClassType classRequirement, 
            int requiredLevel, int manaCost, float cooldown)
        {
            // 실제 구현에서는 ScriptableObject.CreateInstance와 
            // Asset Database를 사용하여 에셋 생성
            // 여기서는 단순화된 모의 구현
            SkillDefinition skillDef = ScriptableObject.CreateInstance<SkillDefinition>();
            
            // 리플렉션을 사용하여 private 필드 설정 (예시)
            var idField = typeof(SkillDefinition).GetField("skillId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (idField != null) idField.SetValue(skillDef, id);
            
            var nameField = typeof(SkillDefinition).GetField("skillName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nameField != null) nameField.SetValue(skillDef, name);
            
            var descField = typeof(SkillDefinition).GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (descField != null) descField.SetValue(skillDef, description);
            
            var typeField = typeof(SkillDefinition).GetField("skillType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null) typeField.SetValue(skillDef, type);
            
            var classField = typeof(SkillDefinition).GetField("classRequirement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (classField != null) classField.SetValue(skillDef, classRequirement);
            
            var levelField = typeof(SkillDefinition).GetField("requiredLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (levelField != null) levelField.SetValue(skillDef, requiredLevel);
            
            var manaField = typeof(SkillDefinition).GetField("baseManaСost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (manaField != null) manaField.SetValue(skillDef, manaCost);
            
            var cooldownField = typeof(SkillDefinition).GetField("baseCooldown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (cooldownField != null) cooldownField.SetValue(skillDef, cooldown);
            
            return skillDef;
        }
        
        /// <summary>
        /// 스킬에 데미지 효과를 추가합니다.
        /// </summary>
        private void AddDamageEffect(
            SkillDefinition skill, string effectName, 
            float baseValue, float valuePerLevel,
            DamageType damageType = DamageType.Physical)
        {
            // 실제 구현에서는 SkillEffectData 객체를 생성하여 스킬에 추가
            SkillEffectData effect = new SkillEffectData();
            
            // 리플렉션을 사용하여 private 필드 설정 (예시)
            var nameField = typeof(SkillEffectData).GetField("effectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nameField != null) nameField.SetValue(effect, effectName);
            
            var typeField = typeof(SkillEffectData).GetField("effectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null) typeField.SetValue(effect, EffectType.Damage);
            
            var valueField = typeof(SkillEffectData).GetField("baseValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (valueField != null) valueField.SetValue(effect, baseValue);
            
            var perLevelField = typeof(SkillEffectData).GetField("valuePerLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (perLevelField != null) perLevelField.SetValue(effect, valuePerLevel);
            
            var damageTypeField = typeof(SkillEffectData).GetField("damageType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (damageTypeField != null) damageTypeField.SetValue(effect, damageType);
            
            // 효과를 스킬에 추가
            var effectsField = typeof(SkillDefinition).GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (effectsField != null)
            {
                var effects = effectsField.GetValue(skill) as List<SkillEffectData>;
                if (effects != null)
                {
                    effects.Add(effect);
                }
            }
        }
        
        /// <summary>
        /// 스킬에 스탯 증가 효과를 추가합니다.
        /// </summary>
        private void AddStatBoostEffect(
            SkillDefinition skill, string effectName,
            float baseValue, float valuePerLevel,
            StatType targetStat, float duration)
        {
            // 실제 구현에서는 SkillEffectData 객체를 생성하여 스킬에 추가
            SkillEffectData effect = new SkillEffectData();
            
            // 리플렉션을 사용하여 private 필드 설정 (예시)
            var nameField = typeof(SkillEffectData).GetField("effectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nameField != null) nameField.SetValue(effect, effectName);
            
            var typeField = typeof(SkillEffectData).GetField("effectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null) typeField.SetValue(effect, EffectType.StatBoost);
            
            var valueField = typeof(SkillEffectData).GetField("baseValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (valueField != null) valueField.SetValue(effect, baseValue);
            
            var perLevelField = typeof(SkillEffectData).GetField("valuePerLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (perLevelField != null) perLevelField.SetValue(effect, valuePerLevel);
            
            var statField = typeof(SkillEffectData).GetField("targetStat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (statField != null) statField.SetValue(effect, targetStat);
            
            var durationField = typeof(SkillEffectData).GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (durationField != null) durationField.SetValue(effect, duration);
            
            // 효과를 스킬에 추가
            var effectsField = typeof(SkillDefinition).GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (effectsField != null)
            {
                var effects = effectsField.GetValue(skill) as List<SkillEffectData>;
                if (effects != null)
                {
                    effects.Add(effect);
                }
            }
        }
        
        /// <summary>
        /// 스킬에 스탯 감소 효과를 추가합니다.
        /// </summary>
        private void AddStatReductionEffect(
            SkillDefinition skill, string effectName,
            float baseValue, float valuePerLevel,
            StatType targetStat, float duration)
        {
            // 실제 구현에서는 SkillEffectData 객체를 생성하여 스킬에 추가
            SkillEffectData effect = new SkillEffectData();
            
            // 리플렉션을 사용하여 private 필드 설정 (예시)
            var nameField = typeof(SkillEffectData).GetField("effectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nameField != null) nameField.SetValue(effect, effectName);
            
            var typeField = typeof(SkillEffectData).GetField("effectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null) typeField.SetValue(effect, EffectType.StatReduction);
            
            var valueField = typeof(SkillEffectData).GetField("baseValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (valueField != null) valueField.SetValue(effect, baseValue);
            
            var perLevelField = typeof(SkillEffectData).GetField("valuePerLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (perLevelField != null) perLevelField.SetValue(effect, valuePerLevel);
            
            var statField = typeof(SkillEffectData).GetField("targetStat", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (statField != null) statField.SetValue(effect, targetStat);
            
            var durationField = typeof(SkillEffectData).GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (durationField != null) durationField.SetValue(effect, duration);
            
            // 효과를 스킬에 추가
            var effectsField = typeof(SkillDefinition).GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (effectsField != null)
            {
                var effects = effectsField.GetValue(skill) as List<SkillEffectData>;
                if (effects != null)
                {
                    effects.Add(effect);
                }
            }
        }
        
        /// <summary>
        /// 스킬에 상태 효과를 추가합니다.
        /// </summary>
        private void AddStatusEffect(
            SkillDefinition skill, string effectName,
            float baseValue, float valuePerLevel)
        {
            // 실제 구현에서는 SkillEffectData 객체를 생성하여 스킬에 추가
            SkillEffectData effect = new SkillEffectData();
            
            // 리플렉션을 사용하여 private 필드 설정 (예시)
            var nameField = typeof(SkillEffectData).GetField("effectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nameField != null) nameField.SetValue(effect, effectName);
            
            var typeField = typeof(SkillEffectData).GetField("effectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (typeField != null) typeField.SetValue(effect, EffectType.StatusEffect);
            
            var valueField = typeof(SkillEffectData).GetField("baseValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (valueField != null) valueField.SetValue(effect, baseValue);
            
            var perLevelField = typeof(SkillEffectData).GetField("valuePerLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (perLevelField != null) perLevelField.SetValue(effect, valuePerLevel);
            
            // 효과를 스킬에 추가
            var effectsField = typeof(SkillDefinition).GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (effectsField != null)
            {
                var effects = effectsField.GetValue(skill) as List<SkillEffectData>;
                if (effects != null)
                {
                    effects.Add(effect);
                }
            }
        }
    }
}
