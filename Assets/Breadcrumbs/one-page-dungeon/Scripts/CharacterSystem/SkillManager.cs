using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Core;

namespace Breadcrumbs.CharacterSystem {
    public class SkillManager : MonoBehaviour, ISkillManager {
        private PlayerCharacter character;
        private Dictionary<string, Skill> skills = new Dictionary<string, Skill>();

        private void Awake() {
            // 서비스 로케이터에 등록
            ServiceLocator.RegisterService<ISkillManager>(this);
        }

        private void OnDestroy() {
            // 필요한 정리 작업
        }

        private void Update() {
            // 스킬 업데이트 (쿨다운 등)
            UpdateSkills(Time.deltaTime);
        }

        // 초기화
        public void Initialize(PlayerCharacter owner) {
            this.character = owner;
        }

        // 스킬 추가
        public bool AddSkill(SkillData skillData) {
            if (skillData == null || skills.ContainsKey(skillData.skillId))
                return false;

            // 직업 요구사항 확인
            if (skillData.requiredClass != ClassType.None &&
                skillData.requiredClass != character.ClassType) {
                Debug.Log($"Cannot learn skill {skillData.skillName}: wrong class");
                return false;
            }

            // 레벨 요구사항 확인
            if (character.Level < skillData.requiredLevel) {
                Debug.Log($"Cannot learn skill {skillData.skillName}: level too low");
                return false;
            }

            // 선행 스킬 요구사항 확인
            if (skillData.requiredSkills != null && skillData.requiredSkills.Length > 0) {
                foreach (string requiredSkillId in skillData.requiredSkills) {
                    if (!skills.ContainsKey(requiredSkillId)) {
                        Debug.Log($"Cannot learn skill {skillData.skillName}: missing required skill");
                        return false;
                    }
                }
            }

            // 스킬 인스턴스 생성 및 추가
            Skill newSkill = new Skill(skillData, character);
            skills.Add(skillData.skillId, newSkill);

            Debug.Log($"Learned new skill: {skillData.skillName}");

            // 이벤트 발생
            EventManager.Trigger("Skill.Learned", new SkillLearnedEventData(character, skillData));

            return true;
        }

        // 스킬 사용
        public bool UseSkill(string skillId, Transform target = null) {
            if (skills.TryGetValue(skillId, out Skill skill)) {
                bool result = skill.UseSkill(target);

                if (result) {
                    // 이벤트 발생
                    EventManager.Trigger("Skill.Used", new SkillUsedEventData(character, skill.data, target));
                }

                return result;
            }

            Debug.Log($"Skill not found: {skillId}");
            return false;
        }

        // 스킬 레벨업
        public bool LevelUpSkill(string skillId) {
            if (skills.TryGetValue(skillId, out Skill skill)) {
                skill.LevelUp();
                Debug.Log($"Skill leveled up: {skill.data.skillName} to level {skill.skillLevel}");

                // 이벤트 발생
                EventManager.Trigger("Skill.LeveledUp", new SkillLeveledUpEventData(character, skill.data, skill.skillLevel));

                return true;
            }

            return false;
        }

        // 모든 스킬 업데이트
        public void UpdateSkills(float deltaTime) {
            foreach (var skill in skills.Values) {
                skill.Update(deltaTime);
            }
        }

        // 스킬 목록 가져오기
        public Dictionary<string, Skill> GetAllSkills() {
            return skills;
        }

        // 특정 스킬 가져오기
        public Skill GetSkill(string skillId) {
            skills.TryGetValue(skillId, out Skill skill);
            return skill;
        }
    }
}