using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class CombatMovementBehavior : AIMovementBehaviorBase {
        private readonly Vector3[] _directions;
        private readonly float[] _weights;
        private readonly Collider[] _results;
        private bool _isStrafing = false;
        private float _strafingDirection = 1f;

        public CombatMovementBehavior(AIMovementSettings settings) : base(settings) {
            _directions = new Vector3[] {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right,
                (Vector3.forward + Vector3.left).normalized,
                (Vector3.forward + Vector3.right).normalized,
                (Vector3.back + Vector3.left).normalized,
                (Vector3.back + Vector3.right).normalized
            };
            _weights = new float[_directions.Length];
            _results = new Collider[16];
        }

        public override float EvaluateSuitability(AIContextData context) {
            // 타겟이 있고 그 타겟이 적절한 거리에 있으면 전투 이동이 적합
            if (context.target == null) return 0.2f;

            float distanceToTarget = context.distanceToTarget;

            // 타겟이 적절한 범위 내에 있을 때 높은 점수
            if (distanceToTarget < settings.detectionRadius) {
                // 너무 가까우면 회피나 대시가 더 적합할 수 있음
                if (distanceToTarget < settings.strafingRadius * 0.5f) {
                    return 0.5f;
                }

                return 0.8f;
            }

            return 0.3f; // 기본 적합도
        }

        public override Vector3 CalculateDirection(AIContextData context) {
            Transform self = context.self;
            Transform target = context.target;

            if (target == null) return Vector3.zero;

            float distanceToTarget = context.distanceToTarget;

            if (distanceToTarget < settings.strafingRadius) {
                _isStrafing = true;
            } else {
                _isStrafing = false;
            }

            if (_isStrafing) {
                return GetStrafingDirection(context);
            } else {
                Vector3 bestDirection = Vector3.zero;
                float bestWeight = float.MinValue;

                for (int i = 0; i < _directions.Length; i++) {
                    _weights[i] = EvaluateDirection(_directions[i], context);
                    if (_weights[i] > bestWeight) {
                        bestWeight = _weights[i];
                        bestDirection = _directions[i];
                    }
                }

                Vector3 separation = GetSeparationVector(context);
                return (bestDirection + separation * 0.5f).normalized;
            }
        }

        private float EvaluateDirection(Vector3 dir, AIContextData context) {
            Transform self = context.self;
            Transform target = context.target;

            Vector3 toTarget = (target.position - self.position).normalized;
            float targetWeight = Vector3.Dot(dir, toTarget);

            if (Physics.Raycast(self.position, dir, settings.raycastDistance)) {
                return -1f;
            }

            int size = Physics.OverlapSphereNonAlloc(
                self.position + dir,
                settings.separationDistance,
                _results
            );

            for (int i = 0; i < size; i++) {
                var col = _results[i];
                if (col.gameObject != self.gameObject && col.CompareTag("Enemy")) {
                    targetWeight -= 0.5f;
                }
            }

            float distanceToTarget = context.distanceToTarget;
            if (distanceToTarget < settings.detectionRadius) {
                if (dir == Vector3.left || dir == Vector3.right) {
                    targetWeight += 0.3f;
                } else if (dir == Vector3.forward || dir == Vector3.back) {
                    targetWeight -= 0.3f;
                }
            }

            return targetWeight;
        }

        private Vector3 GetSeparationVector(AIContextData context) {
            Transform self = context.self;
            Vector3 separation = Vector3.zero;

            foreach (Transform ally in context.allies) {
                Vector3 away = (self.position - ally.position).normalized;
                away = Quaternion.Euler(0, 30, 0) * away;
                separation += away;
            }

            return separation.normalized;
        }

        private Vector3 GetStrafingDirection(AIContextData context) {
            Transform self = context.self;
            Transform target = context.target;

            Vector3 toTarget = (target.position - self.position).normalized;
            Vector3 strafeDir = Vector3.Cross(Vector3.up, toTarget) * _strafingDirection;

            if (settings.changeStrafingDirection) {
                _strafingDirection = Mathf.Sign(Mathf.Sin(Time.time * settings.strafingSpeed));
            }

            return strafeDir;
        }

        public override void DrawGizmos(AIContextData context) {
            if (_directions == null || _weights == null) return;

            Vector3 pos = context.self.position;

            for (int i = 0; i < _directions.Length; i++) {
                float weight = _weights[i];
                Gizmos.color = weight > 0 ? Color.green : Color.red;
                float length = Mathf.Clamp(Mathf.Abs(weight), 0, 1) * 2f;
                Gizmos.DrawLine(pos, pos + _directions[i] * length);
            }
        }
    }
}