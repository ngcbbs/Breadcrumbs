using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [System.Serializable]
    public class BuffData {
        public string buffId;   // 버프 고유 ID
        public string buffName; // 버프 이름
        [TextArea]
        public string description; // 설명
        public Sprite iconSprite;  // 아이콘

        public float duration;                // 지속 시간
        public bool isDebuff;                 // 디버프 여부
        public BuffStackingType stackingType; // 중첩 유형
        public int maxStacks = 1;             // 최대 중첩 수

        public float tickInterval; // 효과 발동 간격 (0보다 큰 경우 주기적 효과)

        // 스탯 수정자
        public List<BuffStatModifier> statModifiers = new List<BuffStatModifier>();

        // 효과 처리 대리자
        public delegate void BuffEffectHandler(PlayerCharacter target, ActiveBuff buff);

        // 효과 처리 메서드들
        public BuffEffectHandler onApplyEffect;  // 적용 시 효과
        public BuffEffectHandler onRemoveEffect; // 제거 시 효과
        public BuffEffectHandler onTickEffect;   // 주기적 효과
    }
}