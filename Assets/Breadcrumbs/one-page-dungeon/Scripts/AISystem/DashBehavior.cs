using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class DashBehavior : AIMovementBehaviorBase {
        private Vector3 _dashDirection;
        private float _dashTimer;

        public DashBehavior(AIMovementSettings settings) : base(settings) { }

        public override float EvaluateSuitability(AIContextData context) {
            if (context.target == null) return 0f;
            float distanceToTarget = context.distanceToTarget;

            // 이미 대시 중이면 높은 점수
            if (_dashTimer > 0) return 0.9f;

            // 적절한 범위일 때만 대시 고려
            if (distanceToTarget > settings.strafingRadius * 1.5f &&
                distanceToTarget < settings.detectionRadius * 0.8f) {

                Vector3 dirToTarget = (context.target.position - context.self.position).normalized;

                // 타겟까지 직선 경로에 장애물이 없는지 확인
                if (!Physics.Raycast(context.self.position, dirToTarget, distanceToTarget * 0.8f)) {
                    // 랜덤 확률로 대시 결정
                    return UnityEngine.Random.value < settings.dashProbability ? 0.85f : 0.1f;
                }
            }

            return 0.1f;
        }

        public override void Initialize(AIContextData context) {
            // 대시 시작 시 방향 설정 및 타이머 초기화
            _dashDirection = (context.target.position - context.self.position).normalized;
            _dashTimer = settings.dashDuration;

            // 대시 쿨다운 설정 (컨트롤러에게 알림)
            AIMovementController controller = context.self.GetComponent<AIMovementController>();
            controller?.SetBehaviorCooldown<DashBehavior>(settings.dashCooldown);
        }

        public override Vector3 CalculateDirection(AIContextData context) {
            if (_dashTimer > 0) {
                _dashTimer -= Time.deltaTime;
                return _dashDirection;
            }

            // 대시 종료 후 기본 방향
            return (context.target.position - context.self.position).normalized;
        }

        public override void DrawGizmos(AIContextData context) {
            if (_dashTimer > 0) {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(context.self.position, _dashDirection * 3f);
            }
        }
    }
}