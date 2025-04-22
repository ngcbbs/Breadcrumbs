using System;
using System.Collections.Generic;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class AccessoryItem : EquipmentItem {
        // 액세서리 특성
        public bool isUnique = false;
        public bool hasProc = false; // 특수 효과 발동 여부

        // 액세서리 능력
        [Serializable]
        public class PassiveAbility {
            public string abilityName;
            public string description;
            public float value;
        }

        public List<PassiveAbility> passiveAbilities = new List<PassiveAbility>();

        // 액세서리별 특수 능력
        public override int CalculateItemScore() {
            int baseScore = base.CalculateItemScore();

            // 액세서리 특수 점수 계산
            int accessoryScore = passiveAbilities.Count * 30;

            // 유니크 아이템 보너스
            if (isUnique) {
                accessoryScore += 100;
            }

            // 발동 효과 보너스
            if (hasProc) {
                accessoryScore += 75;
            }

            return baseScore + accessoryScore;
        }
    }
}