using UnityEngine;

// idea:
// > 방향 바꾸기 같은 경우에는 적끼리 회전 중에 충돌 시에도 바꾸게 하면 좋겠네..
// > GetSeparationVector 조정..

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public class AICombatMovement : MonoBehaviour {
        public Transform target;

        public AICombatMovementSetting setting;

        private Vector3[] _directions;
        private float[] _weights;
        private Collider[] _results;

        private bool _isStrafing = false;
        private float _strafingDirection = 1f; // 좌우 방향 (1 또는 -1)

        private Vector3 _currentDirection = Vector3.forward; // 초기 방향

        void Start() {
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

        void Update() {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget < setting.strafingRadius) {
                _isStrafing = true;
            }
            else {
                _isStrafing = false;
            }

            Vector3 targetDirection;

            if (_isStrafing) {
                targetDirection = _currentDirection = GetStrafingDirection();
            }
            else {
                Vector3 bestDirection = Vector3.zero;
                float bestWeight = float.MinValue;

                for (int i = 0; i < _directions.Length; i++) {
                    _weights[i] = EvaluateDirection(_directions[i]);
                    if (_weights[i] > bestWeight) {
                        bestWeight = _weights[i];
                        bestDirection = _directions[i];
                    }
                }

                Vector3 separation = GetSeparationVector();
                targetDirection = (bestDirection + separation * 0.5f).normalized;
            }

            // 부드럽게 방향 전환 (Slerp 사용)
            _currentDirection = Vector3.Slerp(_currentDirection, targetDirection, Time.deltaTime * setting.turnSpeed).normalized;
            MoveInDirection(_currentDirection);
        }

        float EvaluateDirection(Vector3 dir) {
            Vector3 toTarget = (target.position - transform.position).normalized;
            float targetWeight = Vector3.Dot(dir, toTarget);

            if (Physics.Raycast(transform.position, dir, setting.raycastDistance))
                return -1f;

            var size = Physics.OverlapSphereNonAlloc(transform.position + dir, setting.separationDistance, _results);
            for (var i = 0; i < size; i++) {
                var col = _results[i];
                if (col.gameObject != gameObject && col.CompareTag("Enemy"))
                    targetWeight -= 0.5f;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget < setting.detectionRadius) {
                if (dir == Vector3.left || dir == Vector3.right)
                    targetWeight += 0.3f;
                else if (dir == Vector3.forward || dir == Vector3.back)
                    targetWeight -= 0.3f;
            }

            return targetWeight;
        }

        void MoveInDirection(Vector3 dir) {
            Vector3 move = dir.normalized * (setting.moveSpeed * Time.deltaTime);
            transform.position += move;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        Vector3 GetSeparationVector() {
            Vector3 separation = Vector3.zero;
            var size = Physics.OverlapSphereNonAlloc(transform.position, setting.separationDistance, _results);

            for (var i = 0; i < size; i++) {
                var col = _results[i];
                if (col.gameObject != gameObject && col.CompareTag("Enemy")) {
                    Vector3 away = (transform.position - col.transform.position).normalized;

                    // 바로 뒤로 밀지 않고 약간 각도를 줌 (예: 30도 회전)
                    away = Quaternion.Euler(0, 30, 0) * away;

                    separation += away;
                }
            }

            return separation.normalized;
        }

        // 타겟 주변을 좌우로 회전하는 방향 계산
        Vector3 GetStrafingDirection() {
            Vector3 toTarget = (target.position - transform.position).normalized;

            // 타겟 방향에서 좌우 방향 벡터 계산
            Vector3 strafeDir = Vector3.Cross(Vector3.up, toTarget) * _strafingDirection;

            // 회전 방향을 주기적으로 바꾸고 싶으면...
            if (setting.changeStrafingDirection) {
                _strafingDirection = Mathf.Sign(Mathf.Sin(Time.time * setting.strafingSpeed));
            }

            // 타겟과 적 사이 거리에 따라 앞으로 조금씩 접근하거나 멀어짐 조절 가능
            return strafeDir;
        }

        // Gizmo 그리기
        void OnDrawGizmos() {
            if (_directions == null || _weights == null || _directions.Length != _weights.Length)
                return;

            Vector3 pos = transform.position;

            for (int i = 0; i < _directions.Length; i++) {
                float weight = _weights[i];

                // 가중치가 음수면 빨간색, 양수면 초록색
                Gizmos.color = weight > 0 ? Color.green : Color.red;

                // 가중치 크기에 따라 선 길이 조절 (최대 2 단위)
                float length = Mathf.Clamp(Mathf.Abs(weight), 0, 1) * 2f;

                Gizmos.DrawLine(pos, pos + _directions[i] * length);
            }
        }
    }
}
