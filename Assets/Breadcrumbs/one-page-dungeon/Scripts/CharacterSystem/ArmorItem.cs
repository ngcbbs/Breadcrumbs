using System;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class ArmorItem : EquipmentItem {
        // 방어구 특수 속성
        public ArmorType ArmorType { get; set; }
        public float BaseDefense { get; set; }
        public float MagicDefense { get; set; }
        public float MovementPenalty { get; set; } = 0f;
        public bool HasSetBonus { get; set; }

        // 이펙트 및 사운드
        public GameObject DefendEffect { get; set; }
        public AudioClip DefendSound { get; set; }

        public override int CalculateItemScore() {
            int baseScore = base.CalculateItemScore();

            // 방어구 특수 점수 계산
            int armorScore = Mathf.RoundToInt(BaseDefense * 8 + MagicDefense * 8);

            // 이동 페널티 감점
            armorScore -= Mathf.RoundToInt(MovementPenalty * 10);

            // 세트 보너스
            if (HasSetBonus) {
                armorScore += 50;
            }

            return baseScore + armorScore;
        }

        public override bool CanEquip(PlayerCharacter character) {
            if (!base.CanEquip(character))
                return false;

            // 방어구 타입 확인
            bool canUseArmorType = character.ClassType switch {
                ClassType.Warrior => ArmorType is ArmorType.Plate or ArmorType.Mail,
                ClassType.Mage => ArmorType is ArmorType.Cloth or ArmorType.Robe,
                ClassType.Rogue => ArmorType is ArmorType.Leather or ArmorType.Mail,
                ClassType.Cleric => ArmorType is ArmorType.Mail or ArmorType.Cloth,
                _ => true
            };

            return canUseArmorType;
        }

        public override void OnEquipped(PlayerCharacter character) {
            base.OnEquipped(character);

            // 방어구 특수 효과 - 이동 속도 페널티
            if (MovementPenalty > 0) {
                StatModifier movePenalty = new StatModifier(-MovementPenalty, StatModifierType.PercentAdditive, this);
                character.Stats.AddModifier(StatType.MovementSpeed, movePenalty);
            }

            // 세트 효과 확인 및 적용
            if (!string.IsNullOrEmpty(SetName) && HasSetBonus) {
                // 세트 효과 관리는 별도 시스템으로 처리
            }
        }

        public override EquipmentItem Clone() {
            ArmorItem clone = new ArmorItem {
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

                // 방어구 속성 복사
                ArmorType = ArmorType,
                BaseDefense = BaseDefense,
                MagicDefense = MagicDefense,
                MovementPenalty = MovementPenalty,
                HasSetBonus = HasSetBonus,
                DefendEffect = DefendEffect,
                DefendSound = DefendSound
            };

            // 스탯 수정자 복사
            foreach (var modifier in GetStatModifiers()) {
                clone.AddStatModifier((StatType)modifier.Order, modifier.Value, modifier.Type);
            }

            // 효과 복사
            foreach (var effect in GetEffects()) {
                clone.AddEffect(effect.Clone());
            }

            return clone;
        }
    }
}