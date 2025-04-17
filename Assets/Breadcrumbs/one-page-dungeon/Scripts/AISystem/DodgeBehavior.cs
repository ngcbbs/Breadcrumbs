using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class DodgeBehavior : AIMovementBehaviorBase {
        private Vector3 _dodgeDirection;
        private float _dodgeTimer;

        public DodgeBehavior(AIMovementSettings settings) : base(settings) { }

        public override float EvaluateSuitability(AIContextData context) {
            // 이미 회피 중이면 높은 점수
            if (_dodgeTimer > 0) return 0.95f;

            // 위협이 있으면 회피 고려
            if (context.threats.Count > 0) {
                // 가장 위험한 위협 찾기
                float highestDanger = 0f;
                foreach (var threat in context.threats) {
                    if (threat.dangerLevel > highestDanger) {
                        highestDanger = threat.dangerLevel;
                    }
                }

                // 위험도에 비례한 점수
                return highestDanger * 0.9f;
            }

            return 0.1f;
        }

        public override void Initialize(AIContextData context) {
            // 가장 위험한 위협 방향 찾기
            AIThreat mostDangerousThreat = null;
            float highestDanger = 0f;

            foreach (var threat in context.threats) {
                if (threat.dangerLevel > highestDanger) {
                    highestDanger = threat.dangerLevel;
                    mostDangerousThreat = threat;
                }
            }

            if (mostDangerousThreat != null) {
                // 위협 방향의 수직 방향으로 회피
                float randomSide = (UnityEngine.Random.value > 0.5f) ? 1f : -1f;
                _dodgeDirection = Vector3.Cross(mostDangerousThreat.direction, Vector3.up).normalized * randomSide;

                // 약간 뒤로도 피하게 함
                _dodgeDirection = (_dodgeDirection + Vector3.back * 0.5f).normalized;

                _dodgeTimer = settings.dodgeDuration;

                // 회피 쿨다운 설정
                AIMovementController controller = context.self.GetComponent<AIMovementController>();
                controller?.SetBehaviorCooldown<DodgeBehavior>(settings.dodgeCooldown);
            }
        }

        public override Vector3 CalculateDirection(AIContextData context) {
            if (_dodgeTimer > 0) {
                _dodgeTimer -= Time.deltaTime;
                return _dodgeDirection;
            }

            // 회피 종료 후 타겟 방향
            return context.target != null
                ? (context.target.position - context.self.position).normalized
                : context.currentDirection;
        }

        public override void DrawGizmos(AIContextData context) {
            if (_dodgeTimer > 0) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(context.self.position, _dodgeDirection * 2f);
            }
        }
    }
}