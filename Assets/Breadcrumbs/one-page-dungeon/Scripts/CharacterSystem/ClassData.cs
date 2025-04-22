using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [CreateAssetMenu(fileName = "ClassData", menuName = "RPG/Class Data")]
    public class ClassData : ScriptableObject {
        [Header("기본 정보")]
        public ClassType classType;
        public string className;
        public string classDescription;

        [Header("시각적 요소")]
        public Sprite classIcon;
        public GameObject classModel; // 기본 모델 프리팹

        [Header("시작 스탯")]
        public int baseStrength;
        public int baseDexterity;
        public int baseIntelligence;
        public int baseVitality;
        public int baseWisdom;
        public int baseLuck;

        [Header("스탯 성장")]
        public float strengthGrowth;     // 레벨당 근력 증가
        public float dexterityGrowth;    // 레벨당 민첩 증가
        public float intelligenceGrowth; // 레벨당 지능 증가
        public float vitalityGrowth;     // 레벨당 체력 증가
        public float wisdomGrowth;       // 레벨당 지혜 증가
        public float luckGrowth;         // 레벨당 행운 증가

        [Header("직업 패시브 스킬")]
        public List<SkillData> passiveSkills = new List<SkillData>();

        [Header("직업 시작 스킬")]
        public List<SkillData> startingSkills = new List<SkillData>();

        [Header("직업별 무기 타입")]
        public List<WeaponType> usableWeaponTypes = new List<WeaponType>();

        [Header("직업별 방어구 타입")]
        public List<ArmorType> usableArmorTypes = new List<ArmorType>();

        // 추가적인 특수 속성이나 기능
        [Header("직업 특수 능력")]
        public Dictionary<string, float> classSpecialTraits = new Dictionary<string, float>();

        // 직업 특수 능력 예시 (에디터 설정용)
        [Serializable]
        public class SpecialTrait {
            public string traitName;
            public float traitValue;
        }

        public List<SpecialTrait> specialTraits = new List<SpecialTrait>();

        // 특수 능력 값 가져오기
        public float GetSpecialTraitValue(string traitName, float defaultValue = 0f) {
            // 에디터에서 설정된 특수 능력을 사전으로 변환
            if (classSpecialTraits.Count == 0 && specialTraits.Count > 0) {
                foreach (var trait in specialTraits) {
                    classSpecialTraits[trait.traitName] = trait.traitValue;
                }
            }

            if (classSpecialTraits.TryGetValue(traitName, out float value)) {
                return value;
            }

            return defaultValue;
        }
    }
}