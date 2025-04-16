using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public class AIFleeMovement : MonoBehaviour {
        public Transform target; // 도망칠 대상 (일반적으로 플레이어)
        public AIFleeMovementSetting setting;

        private Vector3 _currentDirection;
        private Vector3 _fleePosition;
        private float _directionChangeTimer;
        private bool _isFleeingDirectly = true;

        private Collider[] _nearbyColliders;
        private Vector3[] _directions;
        private float[] _weights;

        private void Start() {
            // 방향 배열 초기화
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
            _nearbyColliders = new Collider[16];

            // 초기 도망 방향 설정
            _currentDirection = (transform.position - target.position).normalized;

            // 패닉 타이머 초기화
            _directionChangeTimer = setting.panicDirectionChangeTime;
        }

        private void Update() {
            if (target == null) return;

            // 대상으로부터 도망치는 기본 방향
            Vector3 fleeDirection = (transform.position - target.position).normalized;

            // 패닉 상태 처리
            if (setting.panicMovement) {
                _directionChangeTimer -= Time.deltaTime;
                if (_directionChangeTimer <= 0f) {
                    // 직접 도망과 랜덤한 패닉 방향 사이를 교대로 변경
                    _isFleeingDirectly = !_isFleeingDirectly;

                    if (!_isFleeingDirectly) {
                        // 랜덤한 패닉 방향 (기본 도망 방향에서 -45도 ~ 45도 회전)
                        float randomAngle = Random.Range(-45f, 45f);
                        _currentDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;
                    }

                    _directionChangeTimer = setting.panicDirectionChangeTime;
                }
            }

            // 현재 도망 방향 설정
            Vector3 targetDirection;
            if (_isFleeingDirectly || !setting.panicMovement) {
                // 대상으로부터 직접 도망치는 방향
                targetDirection = fleeDirection;
            }
            else {
                // 이미 설정된 패닉 방향 사용
                targetDirection = _currentDirection;
            }

            // 최적의 도망 방향 계산 (장애물 및 다른 AI 고려)
            Vector3 bestDirection = CalculateBestFleeDirection(targetDirection);

            // 거리 제한 확인
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // 최대 도망 거리 확인
            if (distanceToTarget > setting.fleeRadius) {
                // 도망 거리 제한에 도달하면 멈추거나 맴돌기
                bestDirection = Vector3.zero;
            }

            // 부드러운 방향 전환
            _currentDirection = Vector3.Slerp(_currentDirection, bestDirection, Time.deltaTime * setting.turnSpeed);

            // 이동 실행
            MoveInDirection(_currentDirection);
        }

        private Vector3 CalculateBestFleeDirection(Vector3 baseDirection) {
            // 각 방향에 가중치 계산
            float bestWeight = float.MinValue;
            Vector3 bestDir = baseDirection; // 기본적으로 도망 방향 설정

            for (int i = 0; i < _directions.Length; i++) {
                // 월드 좌표계 기준으로 방향 변환
                Vector3 worldDir = transform.TransformDirection(_directions[i]);
                _weights[i] = EvaluateFleeDirection(worldDir);

                if (_weights[i] > bestWeight) {
                    bestWeight = _weights[i];
                    bestDir = worldDir;
                }
            }

            // 다른 AI와의 분리 벡터 계산 및 결합
            Vector3 separation = GetSeparationVector();
            return (bestDir + separation * 0.6f).normalized;
        }

        private float EvaluateFleeDirection(Vector3 dir) {
            // 대상에서 멀어지는 방향과의 일치도 계산
            Vector3 awayFromTarget = (transform.position - target.position).normalized;
            float fleeWeight = Vector3.Dot(dir, awayFromTarget);

            // 장애물 검사
            if (Physics.Raycast(transform.position, dir, setting.raycastDistance, setting.obstacleLayer)) {
                return -2f; // 장애물이 있으면 심한 페널티
            }

            // 최소 도망 거리에 도달했는지 확인
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget < setting.minFleeDistance) {
                // 최소 거리 이내면 더 멀리 도망치는 방향 선호
                fleeWeight *= 1.5f;
            }

            return fleeWeight;
        }

        private void MoveInDirection(Vector3 dir) {
            if (dir.magnitude < 0.1f) return; // 방향이 거의 없으면 이동하지 않음

            // 정규화된 방향으로 이동
            Vector3 movement = dir.normalized * (setting.fleeSpeed * Time.deltaTime);
            transform.position += movement;

            // 이동 방향으로 회전
            transform.rotation = Quaternion.LookRotation(dir);
        }

        private Vector3 GetSeparationVector() {
            Vector3 separation = Vector3.zero;
            int count = Physics.OverlapSphereNonAlloc(transform.position, setting.separationDistance, _nearbyColliders);

            for (int i = 0; i < count; i++) {
                Collider col = _nearbyColliders[i];

                // 자기 자신 제외, Enemy 태그 있는 객체만 고려
                if (col.gameObject != gameObject && col.CompareTag("Enemy")) {
                    Vector3 awayDir = (transform.position - col.transform.position).normalized;
                    float distance = Vector3.Distance(transform.position, col.transform.position);

                    // 거리에 반비례하여 분리 강도 증가
                    float strength = 1.0f - Mathf.Clamp01(distance / setting.separationDistance);
                    separation += awayDir * strength;
                }
            }

            return separation.normalized;
        }

        private void OnDrawGizmos() {
            if (!setting || !setting.showDebugVisuals) return;

            // 도망 거리 반경 표시
            if (target != null) {
                // 최대 도망 거리
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawWireSphere(target.position, setting.fleeRadius);

                // 최소 도망 거리
                Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
                Gizmos.DrawWireSphere(target.position, setting.minFleeDistance);
            }

            if (Application.isPlaying && _weights != null) {
                // 방향 가중치 시각화
                for (int i = 0; i < _directions.Length; i++) {
                    Vector3 worldDir = transform.TransformDirection(_directions[i]);
                    float weight = _weights[i];

                    // 가중치에 따라 색상 결정
                    Gizmos.color = weight > 0 ? Color.green : Color.red;

                    // 가중치 크기에 따른 선 길이
                    float length = Mathf.Clamp(Mathf.Abs(weight), 0, 1) * 2f;
                    Gizmos.DrawLine(transform.position, transform.position + worldDir * length);
                }

                // 현재 도망 방향 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, _currentDirection * 3);
            }
        }
    }
}