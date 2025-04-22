using System.Collections;

namespace Breadcrumbs.CharacterSystem {
    public class ActiveBuff {
        public string ID;           // 인스턴스 고유 ID
        public BuffData BuffData;   // 버프 데이터
        public float RemainingTime; // 남은 지속시간
        public bool IsTemporary;    // 임시 여부
        public object Source;       // 버프 출처
        public int StackCount = 1;  // 현재 스택 수

        // 틱 관련
        public bool HasTickEffect;                    // 주기적 효과 여부
        public float TickInterval;                    // 효과 발동 간격
        public float TickTimer;                       // 다음 틱까지 남은 시간
        public BuffData.BuffEffectHandler TickEffect; // 틱 효과 처리 메서드

        // 상태
        public bool InitialEffectApplied; // 초기 효과 적용 여부
    }
}