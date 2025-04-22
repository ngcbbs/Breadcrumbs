using System;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class ArmorItem : EquipmentItem {
        public ArmorType armorType;
        public float baseDefense;
        public float magicDefense;

        // 방어구 특성
        public float movementPenalty = 0f;
        public bool hasSetBonus = false;

        // 방어 효과
        public GameObject defendEffect;
        public AudioClip defendSound;

        // 방어구별 특수 능력
        public override int CalculateItemScore() {
            int baseScore = base.CalculateItemScore();

            // 방어구 특수 점수 계산
            int armorScore = Mathf.RoundToInt(baseDefense * 8 + magicDefense * 8);

            // 이동 페널티 감점
            armorScore -= Mathf.RoundToInt(movementPenalty * 10);

            // 세트 보너스
            if (hasSetBonus) {
                armorScore += 50;
            }

            return baseScore + armorScore;
        }
    }
}