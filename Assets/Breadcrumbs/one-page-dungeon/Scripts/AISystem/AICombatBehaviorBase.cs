namespace Breadcrumbs.AISystem {
    // AI 전투 행동의 기본 클래스
    public abstract class AICombatBehaviorBase {
        protected AICombatSettings settings;
        
        public AICombatBehaviorBase(AICombatSettings settings) {
            this.settings = settings;
        }
        
        // 행동 적합성 평가
        public abstract float EvaluateSuitability(AIContextData context);
        
        // 행동 실행
        public abstract void Execute(AIContextData context);
        
        // 행동 초기화
        public virtual void Initialize(AIContextData context) { }
        
        // 행동 종료 처리
        public virtual void Cleanup(AIContextData context) { }
        
        // 디버그 기즈모
        public virtual void DrawGizmos(AIContextData context) { }
        
        // 행동이 진행 중인지 확인
        public abstract bool IsActionInProgress();
    }

}