using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class FormationBehavior : AIMovementBehaviorBase {
        public FormationBehavior(AIMovementSettings settings) : base(settings) { }

        public override float EvaluateSuitability(AIContextData context) {
            // 아군이 충분히 있을 때만 집단 이동 고려
            if (context.allies.Count >= 2) {
                return 0.6f;
            }

            return 0.2f;
        }

        public override Vector3 CalculateDirection(AIContextData context) {
            if (context.allies.Count == 0) return Vector3.zero;

            Vector3 cohesion = CalculateCohesion(context);
            Vector3 alignment = CalculateAlignment(context);
            Vector3 separation = CalculateSeparation(context);

            // 세 가지 힘을 조합
            Vector3 formationForce = (
                cohesion * settings.cohesionStrength +
                alignment * settings.alignmentStrength +
                separation
            ).normalized;

            // 타겟이 있으면 타겟 방향도 고려
            if (context.target != null) {
                Vector3 targetDirection = (context.target.position - context.self.position).normalized;
                formationForce = (formationForce * 0.7f + targetDirection * 0.3f).normalized;
            }

            return formationForce;
        }

        private Vector3 CalculateCohesion(AIContextData context) {
            // 중심점으로 이동하려는 힘
            Vector3 centerOfMass = Vector3.zero;

            foreach (Transform ally in context.allies) {
                centerOfMass += ally.position;
            }

            if (context.allies.Count > 0) {
                centerOfMass /= context.allies.Count;
                return (centerOfMass - context.self.position).normalized;
            }

            return Vector3.zero;
        }

        private Vector3 CalculateAlignment(AIContextData context) {
            // 동일한 방향으로 이동하려는 힘
            Vector3 averageDirection = Vector3.zero;

            foreach (Transform ally in context.allies) {
                AIMovementController allyController = ally.GetComponent<AIMovementController>();
                if (allyController != null) {
                    averageDirection += ally.forward;
                }
            }

            if (context.allies.Count > 0) {
                averageDirection /= context.allies.Count;
            }

            return averageDirection.normalized;
        }

        private Vector3 CalculateSeparation(AIContextData context) {
            // 너무 가까우면 밀어내는 힘
            Vector3 separation = Vector3.zero;

            foreach (Transform ally in context.allies) {
                float distance = Vector3.Distance(context.self.position, ally.position);

                if (distance < settings.formationSpacing) {
                    Vector3 repulsionDirection = (context.self.position - ally.position).normalized;
                    float repulsionStrength = 1.0f - (distance / settings.formationSpacing);
                    separation += repulsionDirection * repulsionStrength;
                }
            }

            return separation.normalized;
        }

        public override void DrawGizmos(AIContextData context) {
            // 집단 이동 시각화
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);

            if (context.allies.Count == 0) return;

            // 결속력 시각화
            Vector3 centerOfMass = Vector3.zero;
            foreach (Transform ally in context.allies) {
                centerOfMass += ally.position;
            }

            centerOfMass /= context.allies.Count;

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(context.self.position, centerOfMass);
            Gizmos.DrawWireSphere(centerOfMass, 0.5f);

            // 정렬 시각화
            Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
            Vector3 averageDirection = CalculateAlignment(context);
            Gizmos.DrawRay(context.self.position, averageDirection * 2f);
        }
    }
}