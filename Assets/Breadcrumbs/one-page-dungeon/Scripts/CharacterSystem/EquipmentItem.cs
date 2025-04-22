using System;
using System.Collections.Generic;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public abstract class EquipmentItem : IEquippable {
        // 기본 속성
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public EquipmentSlot EquipSlot { get; set; }
        public int RequiredLevel { get; set; }
        public ClassType ClassType { get; set; }
        public int ItemLevel { get; set; }
        public ItemRarity Rarity { get; set; }

        // 시각적 외형 정보
        public GameObject ItemModel { get; set; }
        public Color PrimaryColor { get; set; } = Color.white;
        public Color SecondaryColor { get; set; } = Color.white;

        // 세트 정보
        public string SetName { get; set; }

        // 스탯 수정자 리스트
        private List<StatModifier> statModifiers = new List<StatModifier>();

        // 특수 효과 리스트
        private List<IItemEffect> effects = new List<IItemEffect>();

        // 아이템 점수 계산
        public virtual int CalculateItemScore() {
            int score = ItemLevel * 10;

            // 희귀도에 따른 보너스
            score *= (int)Rarity + 1;

            // 스탯 기여도
            foreach (var mod in statModifiers) {
                score += Mathf.RoundToInt(mod.Value * 5);
            }

            // 특수 효과 보너스
            score += effects.Count * 50;

            return score;
        }

        // 스탯 수정자 추가
        public void AddStatModifier(StatType statType, float value, StatModifierType modType) {
            statModifiers.Add(new StatModifier(value, modType, this));
        }

        // 전체 스탯 수정자 가져오기
        public List<StatModifier> GetStatModifiers() {
            return new List<StatModifier>(statModifiers);
        }

        // 효과 추가
        public void AddEffect(IItemEffect effect) {
            effects.Add(effect);
        }

        // 효과 가져오기
        public List<IItemEffect> GetEffects() {
            return new List<IItemEffect>(effects);
        }

        // 모든 효과 적용
        public void ApplyAllEffects(PlayerCharacter character) {
            foreach (var effect in effects) {
                effect.Apply(character);
            }
        }

        // 모든 효과 제거
        public void RemoveAllEffects(PlayerCharacter character) {
            foreach (var effect in effects) {
                effect.Remove(character);
            }
        }

        // 아이템 착용 가능 여부 확인
        public virtual bool CanEquip(PlayerCharacter character) {
            // 레벨 확인
            if (character.Level < RequiredLevel)
                return false;

            // 클래스 확인
            if (ClassType != ClassType.None && ClassType != character.ClassType)
                return false;

            return true;
        }

        // 아이템 장착 시 호출
        public virtual void OnEquipped(PlayerCharacter character) {
            // 스탯 수정자 적용
            foreach (var modifier in statModifiers) {
                character.Stats.AddModifier((StatType)modifier.Order, modifier);
            }

            // 효과 적용
            ApplyAllEffects(character);

            Debug.Log($"{character.CharacterName}이(가) {ItemName}을(를) 장착했습니다.");
        }

        // 아이템 해제 시 호출
        public virtual void OnUnequipped(PlayerCharacter character) {
            // 스탯 수정자 제거
            character.Stats.RemoveAllModifiersFromSource(this);

            // 효과 제거
            RemoveAllEffects(character);

            Debug.Log($"{character.CharacterName}이(가) {ItemName}을(를) 해제했습니다.");
        }

        // 아이템 복제 (프로토타입 패턴)
        public abstract EquipmentItem Clone();
    }
}