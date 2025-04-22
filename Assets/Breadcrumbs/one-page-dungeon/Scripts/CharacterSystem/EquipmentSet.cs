using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [CreateAssetMenu(fileName = "EquipmentSet", menuName = "RPG/Equipment Set")]
    public class EquipmentSet : ScriptableObject {
        public string setId;
        public string setName;
        public string description;

        [Serializable]
        public class SetBonusTier {
            public int requiredPieces;
            public string bonusDescription;

            [Serializable]
            public class BonusStat {
                public StatType statType;
                public float value;
                public StatModifierType modifierType;
            }

            public List<BonusStat> bonusStats = new List<BonusStat>();
        }

        public List<SetBonusTier> setBonusTiers = new List<SetBonusTier>();
        public List<EquipmentItem> setItems = new List<EquipmentItem>();

        // 세트 효과 적용 (특정 개수의 세트 아이템 착용 시)
        public void ApplySetBonuses(PlayerCharacter character, int equippedPieces) {
            foreach (var tier in setBonusTiers) {
                if (equippedPieces >= tier.requiredPieces) {
                    // 해당 티어의 세트 효과 적용
                    foreach (var bonus in tier.bonusStats) {
                        // "Set: SetName" 형태로 소스 이름 생성
                        string source = $"Set: {setName} ({tier.requiredPieces})";

                        // 스탯 수정자 생성 및 적용
                        StatModifier mod = new StatModifier(bonus.value, bonus.modifierType, source);
                        character.Stats.AddModifier(bonus.statType, mod);
                    }

                    Debug.Log($"Applied {setName} set bonus for {tier.requiredPieces} pieces");
                }
            }
        }

        // 세트 효과 제거
        public void RemoveSetBonuses(PlayerCharacter character) {
            string sourcePrefix = $"Set: {setName}";

            // 해당 세트 이름으로 시작하는 모든 수정자 제거
            // (실제 구현에서는 캐릭터 스탯 시스템에 적절한 메서드가 필요)
            character.Stats.RemoveAllModifiersFromSource(sourcePrefix);

            Debug.Log($"Removed all {setName} set bonuses");
        }
    }
}