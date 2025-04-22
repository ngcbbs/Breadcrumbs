using System;
using System.Collections.Generic;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class AccessoryItem : EquipmentItem {
        // 액세서리 특수 속성
        public bool IsUnique { get; set; } = false;
        public bool HasProc { get; set; } = false;

        // 패시브 능력 목록
        private List<PassiveAbility> passiveAbilities = new List<PassiveAbility>();

        /// <summary>
        /// 액세서리 패시브 능력 클래스
        /// </summary>
        public class PassiveAbility {
            public string AbilityName { get; set; }
            public string Description { get; set; }
            public float Value { get; set; }

            public PassiveAbility Clone() {
                return new PassiveAbility {
                    AbilityName = AbilityName,
                    Description = Description,
                    Value = Value
                };
            }
        }

        // 패시브 능력 추가
        public void AddPassiveAbility(string name, string description, float value) {
            passiveAbilities.Add(new PassiveAbility {
                AbilityName = name,
                Description = description,
                Value = value
            });
        }

        // 패시브 능력 목록 가져오기
        public List<PassiveAbility> GetPassiveAbilities() {
            return new List<PassiveAbility>(passiveAbilities);
        }

        public override int CalculateItemScore() {
            int baseScore = base.CalculateItemScore();

            // 액세서리 특수 점수 계산
            int accessoryScore = passiveAbilities.Count * 30;

            // 유니크 아이템 보너스
            if (IsUnique) {
                accessoryScore += 100;
            }

            // 발동 효과 보너스
            if (HasProc) {
                accessoryScore += 75;
            }

            return baseScore + accessoryScore;
        }

        public override void OnEquipped(PlayerCharacter character) {
            base.OnEquipped(character);

            // 액세서리 특수 효과 적용
            if (HasProc) {
                // 발동 효과 등록
                // 예: 데미지를 입을 때 특수 효과 발동 등
            }
        }

        public override EquipmentItem Clone() {
            AccessoryItem clone = new AccessoryItem {
                // 기본 속성 복사
                ItemId = $"{ItemId}_clone",
                ItemName = ItemName,
                Description = Description,
                Icon = Icon,
                EquipSlot = EquipSlot,
                RequiredLevel = RequiredLevel,
                ClassType = ClassType,
                ItemLevel = ItemLevel,
                Rarity = Rarity,
                ItemModel = ItemModel,
                PrimaryColor = PrimaryColor,
                SecondaryColor = SecondaryColor,
                SetName = SetName,

                // 액세서리 속성 복사
                IsUnique = IsUnique,
                HasProc = HasProc
            };

            // 스탯 수정자 복사
            foreach (var modifier in GetStatModifiers()) {
                clone.AddStatModifier((StatType)modifier.Order, modifier.Value, modifier.Type);
            }

            // 효과 복사
            foreach (var effect in GetEffects()) {
                clone.AddEffect(effect.Clone());
            }

            // 패시브 능력 복사
            foreach (var ability in passiveAbilities) {
                clone.passiveAbilities.Add(ability.Clone());
            }

            return clone;
        }
    }
}