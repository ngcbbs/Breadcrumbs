using UnityEngine;

namespace Breadcrumbs.AISystem {
    // 이동 행동 기본 클래스
    public abstract class AIMovementBehaviorBase {
        protected AIMovementSettings settings;

        public AIMovementBehaviorBase(AIMovementSettings settings) {
            this.settings = settings;
        }

        // 적합성 점수 - 이 행동이 현재 상황에 얼마나 적합한지 평가 (0-1)
        public abstract float EvaluateSuitability(AIContextData context);

        // 이동 방향 계산
        public abstract Vector3 CalculateDirection(AIContextData context);

        // 행동 실행 전 초기화
        public virtual void Initialize(AIContextData context) { }

        // 행동 종료 시 정리
        public virtual void Cleanup(AIContextData context) { }

        // 디버그 기즈모 그리기
        public virtual void DrawGizmos(AIContextData context) { }
    }
}