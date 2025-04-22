using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "RPG/Skill Database")]
    public class SkillDatabase : ScriptableObject {
        [SerializeField] private List<SkillData> allSkills = new List<SkillData>();
        
        // 캐시 딕셔너리
        private Dictionary<string, SkillData> skillsById = null;
        private Dictionary<ClassType, List<SkillData>> skillsByClass = null;
        
        // 싱글톤 인스턴스
        private static SkillDatabase instance;
        
        public static SkillDatabase Instance {
            get {
                if (instance == null) {
                    instance = Resources.Load<SkillDatabase>("SkillDatabase");
                    if (instance == null) {
                        Debug.LogError("SkillDatabase not found in Resources folder!");
                    }
                    else {
                        instance.Initialize();
                    }
                }
                return instance;
            }
        }
        
        private void OnEnable() {
            Initialize();
        }
        
        public void Initialize() {
            skillsById = new Dictionary<string, SkillData>();
            skillsByClass = new Dictionary<ClassType, List<SkillData>>();
            
            // 스킬 ID별 캐싱
            foreach (var skill in allSkills) {
                if (!string.IsNullOrEmpty(skill.skillId)) {
                    skillsById[skill.skillId] = skill;
                    
                    // 직업별 스킬 목록 캐싱
                    ClassType classType = skill.requiredClass;
                    if (!skillsByClass.ContainsKey(classType)) {
                        skillsByClass[classType] = new List<SkillData>();
                    }
                    skillsByClass[classType].Add(skill);
                }
            }
            
            Debug.Log($"Skill database initialized with {skillsById.Count} skills");
        }
        
        // ID로 스킬 찾기
        public static SkillData GetSkill(string skillId) {
            if (Instance.skillsById.TryGetValue(skillId, out SkillData skill)) {
                return skill;
            }
            
            Debug.LogWarning($"Skill not found with ID: {skillId}");
            return null;
        }
        
        // 특정 직업의 모든 스킬 가져오기
        public static List<SkillData> GetSkillsForClass(ClassType classType) {
            if (Instance.skillsByClass.TryGetValue(classType, out List<SkillData> skills)) {
                return new List<SkillData>(skills);
            }
            
            return new List<SkillData>();
        }
        
        // 특정 레벨에서 배울 수 있는 스킬 가져오기
        public static List<SkillData> GetSkillsByLevel(ClassType classType, int level) {
            List<SkillData> results = new List<SkillData>();
            
            if (Instance.skillsByClass.TryGetValue(classType, out List<SkillData> classSkills)) {
                foreach (var skill in classSkills) {
                    if (skill.requiredLevel == level) {
                        results.Add(skill);
                    }
                }
            }
            
            // 모든 클래스가 배울 수 있는 스킬도 확인
            if (Instance.skillsByClass.TryGetValue(ClassType.None, out List<SkillData> commonSkills)) {
                foreach (var skill in commonSkills) {
                    if (skill.requiredLevel == level) {
                        results.Add(skill);
                    }
                }
            }
            
            return results;
        }
        
        // 스킬 타입별 검색
        public static List<SkillData> GetSkillsByType(SkillType skillType) {
            List<SkillData> results = new List<SkillData>();
            
            foreach (var skill in Instance.allSkills) {
                if (skill.skillType == skillType) {
                    results.Add(skill);
                }
            }
            
            return results;
        }
        
        // 원소 속성별 검색
        public static List<SkillData> GetSkillsByElement(ElementType elementType) {
            List<SkillData> results = new List<SkillData>();
            
            foreach (var skill in Instance.allSkills) {
                if (skill.elementType == elementType) {
                    results.Add(skill);
                }
            }
            
            return results;
        }
    }
}