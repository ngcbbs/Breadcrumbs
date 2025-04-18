using UnityEngine;

namespace Breadcrumbs.Common {
    public class ThirdPersonCamera : MonoBehaviour {
        [Header("타겟 설정")]
        [SerializeField]
        private Transform target;
        [SerializeField]
        private bool findPlayerOnStart = true;
        [SerializeField]
        private string playerTag = "Player";

        [Header("카메라 위치 설정")]
        [SerializeField]
        private float distance = 5.0f;
        [SerializeField]
        private float height = 2.0f;
        [SerializeField]
        private float smoothSpeed = 10.0f;
        [SerializeField]
        private Vector3 targetOffset = Vector3.zero;

        [Header("회전 설정")]
        [SerializeField]
        private bool followTargetRotation = true;
        [SerializeField]
        private bool allowManualRotation = false;
        [SerializeField]
        private float rotationSpeed = 5.0f;
        [SerializeField]
        private float minVerticalAngle = -30.0f;
        [SerializeField]
        private float maxVerticalAngle = 60.0f;

        [Header("충돌 감지")]
        [SerializeField]
        private bool detectCollisions = true;
        [SerializeField]
        private float collisionRadius = 0.2f;
        [SerializeField]
        private LayerMask collisionLayers = -1;
        [SerializeField]
        private float collisionSmoothing = 5.0f;

        [Header("고급 설정")]
        [SerializeField]
        private bool useOcclusion = true;
        [SerializeField]
        private float occlusionSmoothing = 10.0f;
        [SerializeField]
        private float minDistance = 0.5f;
        [SerializeField]
        private bool showDebug = false;

        // 내부 상태 변수
        private Vector3 currentVelocity;
        private float currentDistance;
        private float targetDistance;
        private float currentRotationX;
        private float currentRotationY;
        private float mouseX;
        private float mouseY;
        private bool wasColliding;

        // 위치/회전 초기값을 저장할 변수
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start() {
            // 플레이어 찾기 옵션이 활성화되고 타겟이 없을 경우
            if (findPlayerOnStart && target == null) {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null) {
                    target = player.transform;
                    Debug.Log("플레이어 타겟을 자동으로 찾았습니다.");
                } else {
                    Debug.LogWarning("플레이어 타겟을 찾을 수 없습니다. 수동으로 설정해주세요.");
                }
            }

            // 초기 값 설정
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            currentDistance = distance;
            targetDistance = distance;

            // 현재 회전 값을 초기화
            if (target != null && followTargetRotation) {
                Vector3 eulerAngles = target.eulerAngles;
                currentRotationY = eulerAngles.y;
                currentRotationX = 0;
            } else {
                Vector3 eulerAngles = transform.eulerAngles;
                currentRotationY = eulerAngles.y;
                currentRotationX = eulerAngles.x;
            }

            // 카메라 초기 위치 설정
            if (target != null) {
                UpdateCameraPosition();
            }
        }

        private void LateUpdate() {
            if (target == null) return;

            HandleInput();
            UpdateCameraRotation();
            UpdateCameraPosition();
        }

        private void HandleInput() {
            if (allowManualRotation) {
                // 마우스 입력을 통한 카메라 회전
                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");

                // 회전 값 업데이트
                currentRotationY += mouseX * rotationSpeed;
                currentRotationX -= mouseY * rotationSpeed;

                // 수직 각도 제한
                currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
            } else if (followTargetRotation) {
                // 타겟의 Y축 회전을 따라감
                currentRotationY = target.eulerAngles.y;
            }

            // 마우스 스크롤로 줌인/줌아웃
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            targetDistance = Mathf.Clamp(targetDistance - scroll * 5, minDistance, distance);
        }

        private void UpdateCameraRotation() {
            // 카메라 회전 계산
            Quaternion targetRotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
            transform.rotation = targetRotation;
        }

        private void UpdateCameraPosition() {
            // 부드러운 거리 변경
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

            // 타겟 중심점 계산 (타겟 위치 + 오프셋)
            Vector3 targetPos = target.position + targetOffset;

            // 카메라의 이상적인 위치 계산 (회전 적용)
            Vector3 desiredPosition = targetPos;
            desiredPosition -= transform.forward * currentDistance;
            desiredPosition += Vector3.up * height;

            Vector3 finalPosition = desiredPosition;

            // 충돌 감지
            if (detectCollisions) {
                finalPosition = HandleCollision(targetPos, desiredPosition);
            }

            // 시야 가림 감지
            if (useOcclusion) {
                HandleOcclusion(targetPos, ref finalPosition);
            }

            // 부드러운 카메라 이동
            transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref currentVelocity, 1f / smoothSpeed);

            // 타겟을 항상 바라보도록 (변경 가능)
            transform.LookAt(targetPos);

            // 디버그 시각화
            if (showDebug) {
                Debug.DrawLine(transform.position, targetPos, Color.green);
                Debug.DrawLine(targetPos, desiredPosition, Color.blue);
            }
        }

        private Vector3 HandleCollision(Vector3 targetPos, Vector3 desiredPosition) {
            RaycastHit hitInfo;
            Vector3 direction = desiredPosition - targetPos;
            float targetDistance = direction.magnitude;

            // 타겟 위치에서 원하는 카메라 위치로 레이캐스트
            if (Physics.SphereCast(targetPos, collisionRadius, direction.normalized, out hitInfo, targetDistance,
                    collisionLayers)) {
                // 충돌 거리에 여유를 두기
                float adjustedDistance = hitInfo.distance - 0.1f;
                wasColliding = true;
                return targetPos + direction.normalized * adjustedDistance;
            }

            // 점진적으로 원래 거리로 복귀
            if (wasColliding) {
                wasColliding = false;
                return Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * collisionSmoothing);
            }

            return desiredPosition;
        }

        private void HandleOcclusion(Vector3 targetPos, ref Vector3 cameraPos) {
            RaycastHit hit;
            Vector3 directionToCamera = cameraPos - targetPos;

            // 타겟으로부터 카메라 방향으로 레이를 발사하여 장애물 체크
            if (Physics.Raycast(targetPos, directionToCamera.normalized, out hit, directionToCamera.magnitude, collisionLayers)) {
                // 장애물이 감지되면 카메라 위치 조정
                float occludedDistance = (hit.point - targetPos).magnitude - 0.1f;
                Vector3 occludedPosition = targetPos + directionToCamera.normalized * occludedDistance;

                // 부드러운 전환
                cameraPos = Vector3.Lerp(cameraPos, occludedPosition, Time.deltaTime * occlusionSmoothing);

                if (showDebug) {
                    Debug.DrawLine(targetPos, hit.point, Color.red);
                }
            }
        }

        /// <summary>
        /// 카메라 타겟 설정
        /// </summary>
        public void SetTarget(Transform newTarget) {
            target = newTarget;
        }

        /// <summary>
        /// 카메라 거리 설정
        /// </summary>
        public void SetDistance(float newDistance, bool immediate = false) {
            distance = Mathf.Max(newDistance, minDistance);
            targetDistance = distance;

            if (immediate) {
                currentDistance = distance;
            }
        }

        /// <summary>
        /// 카메라 높이 설정
        /// </summary>
        public void SetHeight(float newHeight, bool immediate = false) {
            height = newHeight;

            if (immediate) {
                Vector3 position = transform.position;
                position.y = target.position.y + height;
                transform.position = position;
            }
        }

        /// <summary>
        /// 타겟에 대한 카메라 오프셋 설정
        /// </summary>
        public void SetTargetOffset(Vector3 newOffset) {
            targetOffset = newOffset;
        }

        /// <summary>
        /// 수동 회전 모드 활성화/비활성화
        /// </summary>
        public void SetManualRotation(bool enabled) {
            allowManualRotation = enabled;
        }

        /// <summary>
        /// 카메라를 기본 위치와 회전으로 초기화
        /// </summary>
        public void ResetCamera() {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            currentDistance = distance;
            targetDistance = distance;

            if (target != null && followTargetRotation) {
                Vector3 eulerAngles = target.eulerAngles;
                currentRotationY = eulerAngles.y;
                currentRotationX = 0;
            }
        }
    }
}