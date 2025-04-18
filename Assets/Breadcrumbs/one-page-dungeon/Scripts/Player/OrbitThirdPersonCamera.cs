using UnityEngine;
using UnityEngine.InputSystem;

namespace Breadcrumbs.Common {
    public class OrbitThirdPersonCamera : MonoBehaviour {
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
        private float minDistance = 1.5f;
        [SerializeField]
        private float maxDistance = 10.0f;
        [SerializeField]
        private float heightOffset = 1.0f;
        [SerializeField]
        private float smoothSpeed = 10.0f;

        [Header("회전 설정")]
        [SerializeField]
        private bool allowOrbit = true;
        [SerializeField]
        private float orbitSpeed = 3.0f;
        [SerializeField]
        private float minVerticalAngle = -30.0f;
        [SerializeField]
        private float maxVerticalAngle = 60.0f;
        [SerializeField]
        private bool invertY = false;

        [Header("회전 스무딩")]
        [SerializeField]
        private bool smoothRotation = true;
        [SerializeField]
        private float rotationSmoothTime = 0.12f;

        [Header("충돌 감지")]
        [SerializeField]
        private bool detectCollisions = true;
        [SerializeField]
        private float collisionRadius = 0.2f;
        [SerializeField]
        private LayerMask collisionLayers = -1;
        [SerializeField]
        private float collisionReturnSpeed = 5.0f;

        [Header("입력 설정")]
        [SerializeField]
        private bool useNewInputSystem = true;
        [SerializeField]
        private string orbitMouseXActionName = "Look";
        [SerializeField]
        private string zoomActionName = "Zoom";
        [SerializeField]
        private KeyCode orbitMouseButton = KeyCode.Mouse1; // 우클릭
        [SerializeField]
        private bool requireMouseButtonForOrbit = true;

        // 내부 상태 변수
        private float currentDistance;
        private float currentOrbitX; // 수평 회전 (y축 기준)
        private float currentOrbitY; // 수직 회전 (x축 기준)
        private Vector3 currentVelocity;
        private float currentZoomVelocity;
        private Vector2 rotationVelocity;

        // 새 입력 시스템 관련 변수
        private PlayerInput playerInput;
        private InputAction lookAction;
        private InputAction zoomAction;
        private InputAction orbitActivateAction;

        // 마우스 입력 상태
        private bool isOrbiting = false;
        private Vector2 lookInput;
        private float zoomInput;

        private void Awake() {
            // 입력 시스템 초기화
            if (useNewInputSystem) {
                SetupNewInputSystem();
            }
        }

        private void Start() {
            // 플레이어 찾기
            if (findPlayerOnStart && target == null) {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null) {
                    target = player.transform;
                    Debug.Log("플레이어 타겟을 자동으로 찾았습니다.");
                } else {
                    Debug.LogWarning("플레이어 타겟을 찾을 수 없습니다. 수동으로 설정해주세요.");
                }
            }

            // 초기 설정
            currentDistance = distance;

            if (target != null) {
                // 현재 카메라의 회전각을 초기값으로 설정
                Vector3 direction = transform.position - target.position;
                direction.Normalize();

                currentOrbitY = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
                currentOrbitX = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            }
        }

        private void SetupNewInputSystem() {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null) {
                playerInput = gameObject.AddComponent<PlayerInput>();
                // 여기서 InputActionAsset을 할당해야 합니다
                // playerInput.actions = 입력 액션 에셋;
            }

            if (playerInput.actions != null) {
                lookAction = playerInput.actions.FindAction(orbitMouseXActionName);
                zoomAction = playerInput.actions.FindAction(zoomActionName);

                if (lookAction != null) {
                    lookAction.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
                    lookAction.canceled += ctx => lookInput = Vector2.zero;
                }

                if (zoomAction != null) {
                    zoomAction.performed += ctx => zoomInput = ctx.ReadValue<float>();
                    zoomAction.canceled += ctx => zoomInput = 0f;
                }
            }
        }

        private void Update() {
            HandleInput();
        }

        private void LateUpdate() {
            if (target == null) return;

            UpdateOrbitRotation();
            UpdateCameraPosition();
        }

        private void HandleInput() {
            if (!allowOrbit) return;

            if (useNewInputSystem) {
                // 새 입력 시스템 사용
                if (requireMouseButtonForOrbit) {
                    // 마우스 버튼 확인 (이 부분은 아직 이전 입력 시스템 사용)
                    isOrbiting = Input.GetKey(orbitMouseButton);
                } else {
                    isOrbiting = true;
                }

                // lookInput과 zoomInput은 이미 입력 액션 이벤트에서 설정됨
            } else {
                // 이전 입력 시스템 사용
                if (requireMouseButtonForOrbit) {
                    isOrbiting = Input.GetKey(orbitMouseButton);
                } else {
                    isOrbiting = true;
                }

                if (isOrbiting) {
                    lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                } else {
                    lookInput = Vector2.zero;
                }

                zoomInput = -Input.GetAxis("Mouse ScrollWheel") * 10f; // 스크롤 속도 조정
            }

            // 줌 처리
            if (Mathf.Abs(zoomInput) > 0.1f) {
                float newDistance = Mathf.Clamp(currentDistance + zoomInput * Time.deltaTime * 5f, minDistance, maxDistance);
                currentDistance = Mathf.SmoothDamp(currentDistance, newDistance, ref currentZoomVelocity, 0.1f);
            }
        }

        private void UpdateOrbitRotation() {
            if (isOrbiting && allowOrbit) {
                // Y 반전 여부에 따라 수직 회전 조정
                float inputY = invertY ? lookInput.y : -lookInput.y;

                if (smoothRotation) {
                    // 부드러운 회전 보간
                    Vector2 targetRotation = new Vector2(
                        currentOrbitX + lookInput.x * orbitSpeed,
                        Mathf.Clamp(currentOrbitY + inputY * orbitSpeed, minVerticalAngle, maxVerticalAngle)
                    );

                    Vector2 smoothedRotation = Vector2.SmoothDamp(
                        new Vector2(currentOrbitX, currentOrbitY),
                        targetRotation,
                        ref rotationVelocity,
                        rotationSmoothTime
                    );

                    currentOrbitX = smoothedRotation.x;
                    currentOrbitY = smoothedRotation.y;
                } else {
                    // 직접 회전 적용
                    currentOrbitX += lookInput.x * orbitSpeed;
                    currentOrbitY = Mathf.Clamp(currentOrbitY + inputY * orbitSpeed, minVerticalAngle, maxVerticalAngle);
                }
            }
        }

        private void UpdateCameraPosition() {
            // 타겟 위치에 높이 오프셋 적용
            Vector3 targetPos = target.position + new Vector3(0, heightOffset, 0);

            // 카메라 위치 계산 (구면 좌표계)
            float y = Mathf.Sin(currentOrbitY * Mathf.Deg2Rad) * currentDistance;
            float horizontalDistance = Mathf.Cos(currentOrbitY * Mathf.Deg2Rad) * currentDistance;
            float x = Mathf.Cos(currentOrbitX * Mathf.Deg2Rad) * horizontalDistance;
            float z = Mathf.Sin(currentOrbitX * Mathf.Deg2Rad) * horizontalDistance;

            Vector3 desiredPosition = targetPos + new Vector3(x, y, z);

            // 충돌 감지
            if (detectCollisions) {
                desiredPosition = HandleCollision(targetPos, desiredPosition);
            }

            // 카메라 위치 부드럽게 변경
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

            // 카메라가 타겟을 바라보도록 설정
            transform.LookAt(targetPos);
        }

        private Vector3 HandleCollision(Vector3 targetPos, Vector3 desiredPosition) {
            // 타겟에서 원하는 카메라 위치로의 방향
            Vector3 direction = desiredPosition - targetPos;
            float targetDistance = direction.magnitude;

            // 충돌 감지를 위한 레이캐스트
            RaycastHit hit;
            if (Physics.SphereCast(targetPos, collisionRadius, direction.normalized, out hit, targetDistance, collisionLayers)) {
                // 충돌 지점에 카메라 위치 설정 (약간의 여유 추가)
                return targetPos + direction.normalized * (hit.distance - 0.1f);
            }

            return desiredPosition;
        }

        /// <summary>
        /// 카메라 타겟 변경
        /// </summary>
        public void SetTarget(Transform newTarget) {
            target = newTarget;
        }

        /// <summary>
        /// 카메라 거리 설정
        /// </summary>
        public void SetDistance(float newDistance, bool immediate = false) {
            distance = Mathf.Clamp(newDistance, minDistance, maxDistance);

            if (immediate) {
                currentDistance = distance;
            }
        }

        /// <summary>
        /// 카메라 오비트 활성화/비활성화
        /// </summary>
        public void SetOrbitEnabled(bool enabled) {
            allowOrbit = enabled;
        }

        /// <summary>
        /// 카메라 회전 속도 설정
        /// </summary>
        public void SetOrbitSpeed(float speed) {
            orbitSpeed = speed;
        }

        /// <summary>
        /// 카메라 위치 즉시 리셋
        /// </summary>
        public void ResetCameraPosition() {
            currentDistance = distance;
            currentOrbitX = 0;
            currentOrbitY = 20; // 약간 위에서 바라보도록

            UpdateCameraPosition();
        }
    }
}