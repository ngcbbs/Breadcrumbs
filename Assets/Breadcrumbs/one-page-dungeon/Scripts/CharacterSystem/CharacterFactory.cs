using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    // 캐릭터 팩토리 - 새 캐릭터 생성 도우미
    public static class CharacterFactory {
        // 기본 캐릭터 생성
        public static void CreateCharacter(GameObject characterObject, string name, GenderType gender, ClassType classType,
            ClassData classData) {
            if (characterObject == null)
                return;

            PlayerCharacter character = characterObject.GetComponent<PlayerCharacter>();
            if (character == null) {
                character = characterObject.AddComponent<PlayerCharacter>();
            }

            // 리플렉션 사용하여 private 필드 설정 (실제 구현에서는 생성자나 초기화 메서드 사용 권장)
            System.Type type = typeof(PlayerCharacter);

            System.Reflection.FieldInfo nameField = type.GetField("characterName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField?.SetValue(character, name);

            System.Reflection.FieldInfo genderField = type.GetField("gender",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            genderField?.SetValue(character, gender);

            System.Reflection.FieldInfo classTypeField = type.GetField("classType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            classTypeField?.SetValue(character, classType);

            System.Reflection.FieldInfo classDataField = type.GetField("classData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            classDataField?.SetValue(character, classData);

            // 스킬 매니저 추가
            SkillManager skillManager = characterObject.GetComponent<SkillManager>();
            if (skillManager == null) {
                skillManager = characterObject.AddComponent<SkillManager>();
                skillManager.Initialize(character);
            }

            // 스킬 트리 매니저 추가
            SkillTreeManager skillTreeManager = characterObject.GetComponent<SkillTreeManager>();
            if (skillTreeManager == null) {
                characterObject.AddComponent<SkillTreeManager>();
            }

            // 외형 컨트롤러 추가
            CharacterAppearanceController appearanceController = characterObject.GetComponent<CharacterAppearanceController>();
            if (appearanceController == null) {
                characterObject.AddComponent<CharacterAppearanceController>();
            }

            // 캐릭터 초기화
            character.InitializeCharacter();

            Debug.Log($"Created new character: {name}, Class: {classType}");
        }

        // 레벨 설정된 캐릭터 생성
        public static void CreateLeveledCharacter(GameObject characterObject, string name, GenderType gender, ClassType classType,
            ClassData classData, int targetLevel) {
            // 기본 캐릭터 생성
            CreateCharacter(characterObject, name, gender, classType, classData);

            // 레벨 올리기
            PlayerCharacter character = characterObject.GetComponent<PlayerCharacter>();
            if (character != null && targetLevel > 1) {
                // 타겟 레벨까지 경험치 제공
                for (int i = 1; i < targetLevel; i++) {
                    character.GainExperience(character.Stats.ExperienceToNextLevel);
                }

                Debug.Log($"Character leveled to {targetLevel}");
            }
        }

        // 커스터마이징 옵션 적용
        public static void ApplyCustomization(PlayerCharacter character, CharacterCustomization customization) {
            if (character == null || customization == null)
                return;

            // 캐릭터에 커스터마이징 설정
            System.Type type = typeof(PlayerCharacter);
            System.Reflection.FieldInfo customizationField = type.GetField("customization",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (customizationField != null) {
                CharacterCustomization current = customizationField.GetValue(character) as CharacterCustomization;

                if (current != null) {
                    current.CopyFrom(customization);
                } else {
                    CharacterCustomization copy = customization.Clone();
                    customizationField.SetValue(character, copy);
                }

                // 외형 적용
                character.ApplyCustomization();
            }
        }

        // 시작 장비 설정
        public static void EquipStartingGear(PlayerCharacter character, EquipmentItem[] startingItems) {
            if (character == null || startingItems == null)
                return;

            foreach (var item in startingItems) {
                if (item != null) {
                    character.EquipItem(item);
                }
            }

            Debug.Log("Starting gear equipped");
        }
    }
}