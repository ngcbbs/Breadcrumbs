using System;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class WeaponItem : EquipmentItem {
        // 무기 특수 속성
        public WeaponType WeaponType { get; set; }
        public float BaseDamage { get; set; }
        public float AttackSpeed { get; set; }
        public float Range { get; set; }
        public bool IsTwoHanded { get; set; }
        public ElementType ElementType { get; set; } = ElementType.None;
        public float ElementalDamage { get; set; } = 0f;

        // 이펙트 및 사운드
        public GameObject AttackEffect { get; set; }
        public AudioClip AttackSound { get; set; }

        public override int CalculateItemScore() {
            int baseScore = base.CalculateItemScore();

            // 무기 데미지 점수 계산
            int damageScore = Mathf.RoundToInt(BaseDamage * 10 + AttackSpeed * 20);

            // 원소 데미지 추가 점수
            if (ElementType != ElementType.None) {
                damageScore += Mathf.RoundToInt(ElementalDamage * 15);
            }

            return baseScore + damageScore;
        }

        public override bool CanEquip(PlayerCharacter character) {
            if (!base.CanEquip(character))
                return false;

            // 무기 타입 확인
            bool canUseWeaponType = character.ClassType switch {
                ClassType.Warrior => WeaponType is WeaponType.Sword or WeaponType.Axe or WeaponType.Mace or WeaponType.Shield,
                ClassType.Mage => WeaponType is WeaponType.Staff or WeaponType.Wand,
                ClassType.Rogue => WeaponType is WeaponType.Dagger or WeaponType.Sword or WeaponType.Bow,
                ClassType.Cleric => WeaponType is WeaponType.Mace or WeaponType.Staff,
                _ => true
            };

            return canUseWeaponType;
        }

        public override EquipmentItem Clone() {
            WeaponItem clone = new WeaponItem {
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

                // 무기 속성 복사
                WeaponType = WeaponType,
                BaseDamage = BaseDamage,
                AttackSpeed = AttackSpeed,
                Range = Range,
                IsTwoHanded = IsTwoHanded,
                ElementType = ElementType,
                ElementalDamage = ElementalDamage,
                AttackEffect = AttackEffect,
                AttackSound = AttackSound
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