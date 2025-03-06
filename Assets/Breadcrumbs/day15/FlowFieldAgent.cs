using UnityEngine;

namespace Breadcrumbs.day15 {
    public class FlowFieldAgent : MonoBehaviour {
        public float moveSpeed = 5f;
        public float rotationSpeed = 10f; // 부드러운 회전을 위한 속도
        public float lookAheadDistance = 0.5f; // 전방 탐색 거리

        private FlowField flowField;
        private Vector2 currentVelocity = Vector2.zero;
        private float velocitySmoothTime = 0.1f; // 이동의 부드러움
        private bool pathBlocked = false;
        private float pathBlockedTime = 0f;
        private Vector2 lastPosition;
        private float stuckCheckTime = 0.5f;
        private float stuckTimer = 0f;

        private Vector2 targetPosition; // 에이전트의 최종 목표 위치
        private bool reachedTarget = false;
        private float targetReachedThreshold = 0.3f; // 목표 도달 간주 거리

        public void SetFlowField(FlowField field) {
            this.flowField = field;
            lastPosition = transform.position;
        }

        public void SetTargetPosition(Vector2 target) {
            targetPosition = target;
            reachedTarget = false;
        }

        private void Update() {
            if (flowField == null) return;

            // 목표에 도달했는지 확인
            if (!reachedTarget && Vector2.Distance(transform.position, targetPosition) < targetReachedThreshold) {
                reachedTarget = true;
                // 목표 도달 시 행동 추가 가능
                return;
            }

            if (reachedTarget) return;

            // 현재 위치
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);

            // stuck 체크
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckCheckTime) {
                stuckTimer = 0f;
                float distanceMoved = Vector2.Distance(currentPosition, lastPosition);
                if (distanceMoved < 0.01f && !reachedTarget) {
                    pathBlocked = true;
                    pathBlockedTime = 2.0f; // 2초간 특별 회피 모드
                }

                lastPosition = currentPosition;
            }

            // 특별 회피 모드 타이머 업데이트
            if (pathBlocked) {
                pathBlockedTime -= Time.deltaTime;
                if (pathBlockedTime <= 0f) {
                    pathBlocked = false;
                }
            }

            // 기본 이동 방향 결정
            Vector2 flowDirection;

            if (pathBlocked) {
                // 특별 회피 모드: 장애물에서 벗어나는 방향 + 약간의 랜덤성
                Vector2 avoidDirection = GetAvoidanceDirection(currentPosition);
                flowDirection = avoidDirection;
            }
            else {
                // 전방 탐색 - 앞에 있는 셀의 flow도 고려
                flowDirection = LookAheadFlowDirection(currentPosition);
            }

            // 목표에 가까워지면 직접 목표를 향해 이동
            float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
            if (distanceToTarget < 1.0f) {
                Vector2 directDirection = (targetPosition - currentPosition).normalized;
                flowDirection = Vector2.Lerp(flowDirection, directDirection, 1.0f - distanceToTarget);
            }

            // 속도 부드럽게 적용
            Vector2 targetVelocity = flowDirection * moveSpeed;
            currentVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, ref currentVelocity, velocitySmoothTime);

            // 이동 적용
            transform.position += new Vector3(currentVelocity.x, currentVelocity.y, 0) * Time.deltaTime;

            // 회전 적용 (현재 속도 방향으로 부드럽게 회전)
            if (currentVelocity.magnitude > 0.1f) {
                float targetAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // 전방 탐색을 통한 Flow 방향 결정
        private Vector2 LookAheadFlowDirection(Vector2 currentPosition) {
            // 현재 셀의 flow 방향
            Vector2 currentFlow = flowField.GetFlowVectorFromWorldPosition(currentPosition);

            // 전방 위치 계산
            Vector2 lookAheadPos = currentPosition + currentFlow * lookAheadDistance;
            Vector2 lookAheadFlow = flowField.GetFlowVectorFromWorldPosition(lookAheadPos);

            // 둘을 혼합 (전방 flow에 더 가중치)
            return Vector2.Lerp(currentFlow, lookAheadFlow, 0.7f).normalized;
        }

        // 장애물 회피 방향 계산
        private Vector2 GetAvoidanceDirection(Vector2 currentPosition) {
            // 8방향 검사
            Vector2[] directions = new Vector2[] {
                Vector2.up,
                new Vector2(1, 1).normalized,
                Vector2.right,
                new Vector2(1, -1).normalized,
                Vector2.down,
                new Vector2(-1, -1).normalized,
                Vector2.left,
                new Vector2(-1, 1).normalized,
            };

            // 각 방향의 가중치 계산
            float[] weights = new float[directions.Length];
            float totalWeight = 0;

            for (int i = 0; i < directions.Length; i++) {
                // 가상의 위치에서 flow 방향 확인
                Vector2 testPos = currentPosition + directions[i] * 0.5f;
                Vector2 flowAtPos = flowField.GetFlowVectorFromWorldPosition(testPos);

                // 가중치 계산 - 목표 방향과 일치할수록 높은 가중치
                float dot = Vector2.Dot(flowAtPos, (targetPosition - currentPosition).normalized);
                float weight = Mathf.Max(0.1f, (dot + 1) / 2); // -1~1 범위를 0.1~1 범위로 변환

                // 약간의 랜덤성 추가
                weight *= UnityEngine.Random.Range(0.8f, 1.2f);

                weights[i] = weight;
                totalWeight += weight;
            }

            // 가중평균 방향 계산
            Vector2 resultDirection = Vector2.zero;
            if (totalWeight > 0) {
                for (int i = 0; i < directions.Length; i++) {
                    resultDirection += directions[i] * (weights[i] / totalWeight);
                }
            }
            else {
                // 가중치 합이 0이면 목표 방향 직접 사용
                resultDirection = (targetPosition - currentPosition).normalized;
            }

            return resultDirection.normalized;
        }
    }
}
