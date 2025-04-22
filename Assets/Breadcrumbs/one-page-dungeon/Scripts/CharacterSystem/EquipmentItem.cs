using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public abstract class EquipmentItem {
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        public EquipmentSlot equipSlot;
        public int requiredLevel;
        public ClassType classType;
        public int itemLevel;
        public ItemRarity rarity;

        [Serializable]
        public class ItemStat {
            public StatType statType;
            public float value;
            public StatModifierType type;
        }

        public List<ItemStat> stats = new List<ItemStat>();

        // 시각적 외형 정보
        public GameObject itemModel;
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;

        // 세트 정보
        public string setName;

        // 특수 효과
        [Serializable]
        public class SpecialEffect {
            public string effectName;
            public string effectDescription;
            public float activationChance; // 발동 확률
            public GameObject visualEffect;
            public AudioClip soundEffect;
        }

        public List<SpecialEffect> specialEffects = new List<SpecialEffect>();

        // 아이템 성능 점수 계산
        public virtual int CalculateItemScore() {
            int score = itemLevel * 10;

            // 희귀도에 따른 보너스
            switch (rarity) {
                case ItemRarity.Common:
                    score *= 1;
                    break;
                case ItemRarity.Uncommon:
                    score *= 2;
                    break;
                case ItemRarity.Rare:
                    score *= 3;
                    break;
                case ItemRarity.Epic:
                    score *= 4;
                    break;
                case ItemRarity.Legendary:
                    score *= 5;
                    break;
            }

            // 스탯 기여도
            foreach (var stat in stats) {
                score += Mathf.RoundToInt(stat.value * 5);
            }

            // 특수 효과 보너스
            score += specialEffects.Count * 50;

            return score;
        }
    }
}