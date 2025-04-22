using System;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class WeaponItem : EquipmentItem {
        public WeaponType weaponType;
        public float baseDamage;
        public float attackSpeed;
        public float range;

        // 무기 특성
        public bool isTwoHanded;
        public ElementType elementType = ElementType.None;
        public float elementalDamage = 0f;

        // 공격 이펙트
        public GameObject attackEffect;
        public AudioClip attackSound;

        // 무기별 특수 능력
        public override int CalculateItemScore() {
            int baseScore = base.CalculateItemScore();

            // 무기 특수 점수 계산
            int weaponScore = Mathf.RoundToInt(baseDamage * 10 + attackSpeed * 20);

            // 속성 추가 데미지 보너스
            if (elementType != ElementType.None) {
                weaponScore += Mathf.RoundToInt(elementalDamage * 15);
            }

            return baseScore + weaponScore;
        }
    }
}