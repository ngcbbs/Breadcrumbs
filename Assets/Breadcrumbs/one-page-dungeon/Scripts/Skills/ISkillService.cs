using UnityEngine;
using Breadcrumbs.Character;

namespace Breadcrumbs.Skills
{
    /// <summary>
    /// Service interface for skill management
    /// </summary>
    public interface ISkillService
    {
        /// <summary>
        /// Learn a new skill
        /// </summary>
        /// <param name="character">The character learning the skill</param>
        /// <param name="skillId">The ID of the skill to learn</param>
        /// <returns>True if the skill was learned successfully</returns>
        bool LearnSkill(ICharacter character, string skillId);
        
        /// <summary>
        /// Use a skill
        /// </summary>
        /// <param name="character">The character using the skill</param>
        /// <param name="skillId">The ID of the skill to use</param>
        /// <param name="target">The target of the skill (optional)</param>
        /// <returns>True if the skill was used successfully</returns>
        bool UseSkill(ICharacter character, string skillId, Transform target = null);
        
        /// <summary>
        /// Upgrade a skill
        /// </summary>
        /// <param name="character">The character upgrading the skill</param>
        /// <param name="skillId">The ID of the skill to upgrade</param>
        /// <returns>True if the skill was upgraded successfully</returns>
        bool UpgradeSkill(ICharacter character, string skillId);
        
        /// <summary>
        /// Check if a skill can be learned
        /// </summary>
        /// <param name="character">The character</param>
        /// <param name="skillId">The skill ID</param>
        /// <returns>True if the skill can be learned</returns>
        bool CanLearnSkill(ICharacter character, string skillId);
        
        /// <summary>
        /// Check if a skill can be used
        /// </summary>
        /// <param name="character">The character</param>
        /// <param name="skillId">The skill ID</param>
        /// <returns>True if the skill can be used</returns>
        bool CanUseSkill(ICharacter character, string skillId);
        
        /// <summary>
        /// Check if a skill can be upgraded
        /// </summary>
        /// <param name="character">The character</param>
        /// <param name="skillId">The skill ID</param>
        /// <returns>True if the skill can be upgraded</returns>
        bool CanUpgradeSkill(ICharacter character, string skillId);
    }
}
