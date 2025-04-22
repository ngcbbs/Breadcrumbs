using System;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class MasteryNode : SkillTreeNode {
        public string masteryType;  // 무기, 원소, 특성 등
        public string masteryValue; // 구체적인 값 (검, 도끼, 화염 등)
        public float effectPerLevel;

        protected override void ApplyMasteryEffect(PlayerCharacter character) {
            float effect = effectPerLevel * currentLevel;

            // 마스터리 타입에 따른 처리
            switch (masteryType) {
                case "Weapon":
                    // 무기 마스터리
                    ApplyWeaponMastery(character, masteryValue, effect);
                    break;

                case "Element":
                    // 원소 마스터리
                    ApplyElementMastery(character, masteryValue, effect);
                    break;

                case "Class":
                    // 직업 특성 마스터리
                    ApplyClassMastery(character, masteryValue, effect);
                    break;

                default:
                    Debug.LogWarning($"Unknown mastery type: {masteryType}");
                    break;
            }
        }

        // 무기 마스터리 적용
        private void ApplyWeaponMastery(PlayerCharacter character, string weaponType, float effect) {
            // 예: 특정 무기 타입 사용 시 공격력/명중률 등 증가
            Debug.Log($"Applied {effect}% {weaponType} mastery");

            // 마스터리 효과 구현
            // 실제 구현에서는 무기 타입에 따라 적절한 효과 적용
        }

        // 원소 마스터리 적용
        private void ApplyElementMastery(PlayerCharacter character, string elementType, float effect) {
            // 예: 특정 원소 속성 스킬 강화
            Debug.Log($"Applied {effect}% {elementType} mastery");

            // 원소 마스터리 효과 구현
            if (character.ClassController is MageController mage) {
                // 마법사인 경우 원소 마스터리 추가
                ElementType element = (ElementType)Enum.Parse(typeof(ElementType), elementType);
                mage.AddElementalMastery(element);
            }
        }

        // 직업 특성 마스터리 적용
        private void ApplyClassMastery(PlayerCharacter character, string classFeature, float effect) {
            // 예: 전사의 분노, 도적의 은신 등 특정 직업 기능 강화
            Debug.Log($"Applied {effect}% {classFeature} mastery");

            // 직업별 특성 마스터리 효과 구현
            switch (character.ClassType) {
                case ClassType.Warrior:
                    // 전사 특성 강화
                    if (character.ClassController is WarriorController warrior) {
                        // 특성에 따른 처리
                        // 예: "Rage" - 분노 증가율 향상 등
                    }

                    break;

                case ClassType.Rogue:
                    // 도적 특성 강화
                    if (character.ClassController is RogueController rogue) {
                        // 특성에 따른 처리
                        // 예: "Combo" - 콤보 포인트 최대치 증가 등
                    }

                    break;

                // 기타 직업 처리
            }
        }
    }
}