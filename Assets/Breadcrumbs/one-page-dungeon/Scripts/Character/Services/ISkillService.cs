using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character.Services
{
    /// <summary>
    /// Service for managing character skills
    /// </summary>
    public interface ISkillService
    {
        /// <summary>
        /// Learn a skill for a character
        /// </summary>
        bool LearnSkill(ICharacter character, string skillId);
        
        /// <summary>
        /// Use a skill for a character
        /// </summary>
        bool UseSkill(ICharacter character, string skillId, Transform target = null);
        
        /// <summary>
        /// Upgrade a skill for a character
        /// </summary>
        bool UpgradeSkill(ICharacter character, string skillId);
        
        /// <summary>
        /// Get the level of a skill for a character
        /// </summary>
        int GetSkillLevel(ICharacter character, string skillId);
        
        /// <summary>
        /// Check if a character has a skill
        /// </summary>
        bool HasSkill(ICharacter character, string skillId);
        
        /// <summary>
        /// Get available skill points for a character
        /// </summary>
        int GetAvailableSkillPoints(ICharacter character);
        
        /// <summary>
        /// Set available skill points for a character
        /// </summary>
        void SetAvailableSkillPoints(ICharacter character, int points);
        
        /// <summary>
        /// Get all skills for a character
        /// </summary>
        IEnumerable<SkillInfo> GetAllSkills(ICharacter character);
        
        /// <summary>
        /// Get all skills available for a class type
        /// </summary>
        IEnumerable<SkillInfo> GetAvailableSkillsForClass(ClassType classType);
        
        /// <summary>
        /// Get skills available at a specific level
        /// </summary>
        IEnumerable<SkillInfo> GetAvailableSkillsForLevel(ICharacter character, int level);
        
        /// <summary>
        /// Event that fires when a skill is learned
        /// </summary>
        event Action<ICharacter, string> OnSkillLearned;
        
        /// <summary>
        /// Event that fires when a skill is used
        /// </summary>
        event Action<ICharacter, string, Transform> OnSkillUsed;
        
        /// <summary>
        /// Event that fires when a skill is upgraded
        /// </summary>
        event Action<ICharacter, string, int> OnSkillUpgraded;
    }
    
    /// <summary>
    /// Contains information about a skill
    /// </summary>
    public class SkillInfo
    {
        public string SkillId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillType Type { get; set; }
        public int RequiredLevel { get; set; }
        public ClassType ClassRequirement { get; set; }
        public int ManaCost { get; set; }
        public float Cooldown { get; set; }
        public int MaxLevel { get; set; }
    }
    
    public enum SkillType {
        Active,  // 주동적으로 사용하는 스킬
        Passive, // 자동 적용되는 패시브 스킬
        Toggle,  // 온/오프 가능한 스킬
        Buff,    // 버프 스킬
        Debuff   // 디버프 스킬
    }
}
