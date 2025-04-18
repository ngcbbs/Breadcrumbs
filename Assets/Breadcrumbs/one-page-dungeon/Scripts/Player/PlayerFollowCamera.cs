using UnityEngine;

namespace Breadcrumbs.Common {
    public class PlayerFollowCamera : MonoBehaviour {
        [Header("타겟 설정")]
        [SerializeField]
        private Transform target;
        [SerializeField]
        private bool findPlayerOnStart = true;
        [SerializeField]
        private string playerTag = "Player";

        [Header("위치 설정")]
        [SerializeField]
        private Vector3 offset = new Vector3(0f, 2f, -5f);
        [SerializeField]
        private float smoothSpeed = 5f;
        [SerializeField]
        private bool lookAtTarget = true;

        [Header("고급 설정")]
        [SerializeField]
        private bool useDeadZone = false;
        [SerializeField]
        private float deadZoneRadius = 1f;
        [SerializeField]
        private bool clampPosition = false;
        [SerializeField]
        private Vector2 minPosition;
        [SerializeField]
        private Vector2 maxPosition;

        [Header("화면 흔들림 효과")]
        [SerializeField]
        private float shakeIntensity = 0f;
        [SerializeField]
        private float shakeDuration = 0f;
        [SerializeField]
        private float shakeFadeTime = 0.5f;

        private Vector3 velocity = Vector3.zero;
        private Vector3 currentShakeOffset;
        private Vector3 lastTargetPosition;
        private float shakeTimer = 0f;

        private void Start() {
            // 플레이어 찾기 옵션이 활성화 되었고 타겟이 없을 경우
            if (findPlayerOnStart && target == null) {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null) {
                    target = player.transform;
                    Debug.Log("플레이어 타겟을 자동으로 찾았습니다.");
                } else {
                    Debug.LogWarning("플레이어 타겟을 찾을 수 없습니다. 수동으로 설정해주세요.");
                }
            }

            if (target != null) {
                lastTargetPosition = target.position;
            }
        }

        private void LateUpdate() {
            if (target == null) return;

            Vector3 desiredPosition = CalculateDesiredPosition();
            Vector3 smoothedPosition = SmoothPosition(desiredPosition);
            smoothedPosition = ApplyPositionClamp(smoothedPosition);
            smoothedPosition += CalculateShakeOffset();

            transform.position = smoothedPosition;

            if (lookAtTarget) {
                transform.LookAt(target);
            }

            UpdateShakeEffect();
        }

        private Vector3 CalculateDesiredPosition() {
            Vector3 targetPosition = target.position;

            // 데드존 적용
            if (useDeadZone) {
                float distanceFromLast = Vector3.Distance(
                    new Vector3(targetPosition.x, 0, targetPosition.z),
                    new Vector3(lastTargetPosition.x, 0, lastTargetPosition.z)
                );

                if (distanceFromLast < deadZoneRadius) {
                    targetPosition = new Vector3(
                        lastTargetPosition.x,
                        targetPosition.y,
                        lastTargetPosition.z
                    );
                } else {
                    lastTargetPosition = targetPosition;
                }
            }

            return targetPosition + offset;
        }

        private Vector3 SmoothPosition(Vector3 desiredPosition) {
            // SmoothDamp는 부드러운 카메라 이동에 적합합니다
            return Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                1f / smoothSpeed
            );
        }

        private Vector3 ApplyPositionClamp(Vector3 position) {
            if (clampPosition) {
                position.x = Mathf.Clamp(position.x, minPosition.x, maxPosition.x);
                position.z = Mathf.Clamp(position.z, minPosition.y, maxPosition.y);
            }

            return position;
        }

        private Vector3 CalculateShakeOffset() {
            if (shakeTimer > 0) {
                return currentShakeOffset;
            }

            return Vector3.zero;
        }

        private void UpdateShakeEffect() {
            if (shakeTimer > 0) {
                shakeTimer -= Time.deltaTime;

                // 흔들림 강도 페이드 아웃
                float currentIntensity = Mathf.Lerp(
                    0,
                    shakeIntensity,
                    shakeTimer / shakeDuration
                );

                // 새로운 랜덤 흔들림 위치 계산
                currentShakeOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ) * currentIntensity;
            }
        }

        /// <summary>
        /// 카메라 흔들림 효과 시작
        /// </summary>
        /// <param name="intensity">흔들림 강도</param>
        /// <param name="duration">지속 시간(초)</param>
        public void ShakeCamera(float intensity, float duration) {
            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeTimer = duration;
        }

        /// <summary>
        /// 카메라 타겟 변경
        /// </summary>
        /// <param name="newTarget">새로운 타겟 트랜스폼</param>
        public void SetTarget(Transform newTarget) {
            target = newTarget;
            if (target != null) {
                lastTargetPosition = target.position;
            }
        }

        /// <summary>
        /// 카메라 오프셋 변경
        /// </summary>
        /// <param name="newOffset">새로운 오프셋</param>
        public void SetOffset(Vector3 newOffset) {
            offset = newOffset;
        }

        /// <summary>
        /// 카메라 영역 제한 설정
        /// </summary>
        /// <param name="useClamp">영역 제한 사용 여부</param>
        /// <param name="min">최소 위치 (x, z)</param>
        /// <param name="max">최대 위치 (x, z)</param>
        public void SetPositionClamp(bool useClamp, Vector2 min, Vector2 max) {
            clampPosition = useClamp;
            minPosition = min;
            maxPosition = max;
        }
    }
}