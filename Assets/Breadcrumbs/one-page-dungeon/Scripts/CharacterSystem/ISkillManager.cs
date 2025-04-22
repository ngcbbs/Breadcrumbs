using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    /// <summary>
    /// 스킬 매니저 인터페이스
    /// </summary>
    public interface ISkillManager {
        /// <summary>
        /// 초기화
        /// </summary>
        void Initialize(PlayerCharacter owner);

        /// <summary>
        /// 스킬 추가
        /// </summary>
        bool AddSkill(SkillData skillData);

        /// <summary>
        /// 스킬 사용
        /// </summary>
        bool UseSkill(string skillId, Transform target = null);

        /// <summary>
        /// 스킬 레벨업
        /// </summary>
        bool LevelUpSkill(string skillId);

        /// <summary>
        /// 모든 스킬 업데이트
        /// </summary>
        void UpdateSkills(float deltaTime);

        /// <summary>
        /// 모든 스킬 가져오기
        /// </summary>
        Dictionary<string, Skill> GetAllSkills();

        /// <summary>
        /// 특정 스킬 가져오기
        /// </summary>
        Skill GetSkill(string skillId);
    }
}