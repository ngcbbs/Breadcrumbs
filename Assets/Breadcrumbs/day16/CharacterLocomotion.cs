using System.Collections;
using UnityEngine;

namespace Breadcrumbs.day16 {
    public class CharacterLocomotion : MonoBehaviour {
        [Header("이동 설정")] [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float runSpeed = 10.0f;
        [SerializeField] private float rotationSpeed = 120.0f;

        [Header("점프 및 중력 설정")] [SerializeField]
        private float jumpForce = 7.0f;

        [SerializeField] private float initialGravity = 15.0f; // 상승 시 중력
        [SerializeField] private float fallGravity = 30.0f; // 하강 시 중력
        [SerializeField] private float maxFallSpeed = 20.0f; // 최대 낙하 속도
        [SerializeField] private float fallMultiplier = 1.5f; // 낙하 중력 배율(높을수록 빠르게 떨어짐)
        [SerializeField] private float landingImpactThreshold = 10.0f; // 착지 임팩트 효과를 위한 최소 낙하 속도

        [Header("지면 감지")] [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float coyoteTime = 0.15f; // 절벽에서 떨어진 후에도 잠시 점프 가능하게 하는 시간

        [Header("애니메이션")] [SerializeField] private Animator animator;

        // 컴포넌트 참조
        private CharacterController characterController;

        // 이동 관련 변수
        private Vector3 moveDirection = Vector3.zero;
        private float verticalVelocity = 0f;
        private float currentSpeed;
        private bool isGrounded;
        private bool wasGrounded;
        private bool isRunning;
        private float lastGroundedTime;
        private float fallStartY; // 낙하 시작 높이
        private bool isFalling; // 낙하 중인지 여부

        // 애니메이션 해시 ID
        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;
        private int animIDHardLanding;

        // 캐릭터 상태
        public enum MovementState {
            Idle,
            Walking,
            Running,
            Jumping,
            Falling,
            HardLanding
        }

        private MovementState currentState = MovementState.Idle;
        private MovementState previousState = MovementState.Idle;

        private void Awake() {
            // 컴포넌트 캐싱
            characterController = GetComponent<CharacterController>();

            // 애니메이터가 할당되지 않았다면 현재 게임 오브젝트에서 찾기
            if (animator == null)
                animator = GetComponent<Animator>();

            // 애니메이션 파라미터 해시 ID 설정
            AssignAnimationIDs();

            // 초기 속도 설정
            currentSpeed = walkSpeed;
        }

        private void AssignAnimationIDs() {
            // 애니메이션 파라미터 해시 ID 설정 (문자열 비교보다 빠름)
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            animIDHardLanding = Animator.StringToHash("HardLanding");
        }

        private void Update() {
            // 현재 상태 저장
            previousState = currentState;

            // 지면 체크
            CheckGroundStatus();

            // 입력 처리 및 이동
            HandleMovement();

            // 중력 및 점프 처리
            HandleGravityAndJump();

            // 최종 이동 벡터 적용
            ApplyMovement();

            // 애니메이션 상태 업데이트
            UpdateAnimator();
        }

        private void CheckGroundStatus() {
            // 이전 지면 상태 저장
            wasGrounded = isGrounded;

            // 스피어 캐스트로 지면 체크
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

            // 지면에 닿았을 때
            if (isGrounded) {
                // 지면에 닿은 시간 기록
                lastGroundedTime = Time.time;

                // 높은 높이에서 떨어졌을 경우 Hard Landing 상태로 전환
                if (isFalling && Mathf.Abs(verticalVelocity) > landingImpactThreshold) {
                    currentState = MovementState.HardLanding;
                    animator.SetTrigger(animIDHardLanding);

                    // 착지 효과 (카메라 흔들림, 소리 등을 여기서 추가할 수 있음)
                    float impactForce = Mathf.Abs(verticalVelocity) / maxFallSpeed;
                    StartCoroutine(HardLandingRoutine(impactForce));
                }

                // 낙하 상태 리셋
                isFalling = false;
            }

            // 지면에서 벗어났을 때
            if (!isGrounded && wasGrounded) {
                // 낙하 시작 높이 기록
                fallStartY = transform.position.y;
            }

            // 일정 시간 이상 공중에 있을 경우 낙하 상태로 간주
            if (!isGrounded && verticalVelocity < 0 && !isFalling) {
                isFalling = true;
            }
        }

        private void HandleMovement() {
            // 하드 랜딩 애니메이션 중에는 이동 제한
            if (currentState == MovementState.HardLanding) return;

            // 입력 값 가져오기
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // 달리기 전환
            if (Input.GetKey(KeyCode.LeftShift) && isGrounded) {
                currentSpeed = runSpeed;
                isRunning = true;
            }
            else {
                currentSpeed = walkSpeed;
                isRunning = false;
            }

            // 이동 방향 계산 (카메라 기준)
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            // 이동 처리
            if (direction.magnitude >= 0.1f) {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, 0.1f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                moveDirection = moveDirection.normalized * currentSpeed;

                // 공중에 있을 때 이동 제한
                if (!isGrounded) {
                    // 공중에서 이동 제어를 약화 (선택 사항)
                    moveDirection *= 0.8f;
                }

                // 상태 업데이트
                if (isGrounded && currentState != MovementState.HardLanding) {
                    currentState = isRunning ? MovementState.Running : MovementState.Walking;
                }
            }
            else {
                moveDirection = Vector3.zero;

                // 상태 업데이트
                if (isGrounded && currentState != MovementState.HardLanding) {
                    currentState = MovementState.Idle;
                }
            }
        }

        private float rotationVelocity; // 회전 속도 참조용 변수

        private void HandleGravityAndJump() {
            // 코요테 타임(Coyote Time) 체크 - 절벽에서 떨어진 직후에도 짧은 시간 점프 가능
            bool canJump = isGrounded || (Time.time - lastGroundedTime <= coyoteTime);

            // 점프 입력
            if (Input.GetButtonDown("Jump") && canJump && currentState != MovementState.HardLanding) {
                verticalVelocity = Mathf.Sqrt(jumpForce * 2f * initialGravity);
                currentState = MovementState.Jumping;

                // 점프 애니메이션 트리거
                animator.SetBool(animIDJump, true);
            }

            // 중력 적용 (상승 중과 하강 중 다른 중력 적용)
            float gravity = initialGravity;

            if (verticalVelocity < 0) {
                // 하강 중 더 빠른 중력 적용
                gravity = fallGravity * fallMultiplier;

                // 상태 업데이트
                if (!isGrounded) {
                    currentState = MovementState.Falling;
                }
            }

            // 중력 가속도 적용
            verticalVelocity -= gravity * Time.deltaTime;

            // 최대 낙하 속도 제한
            verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);

            // 지면에 닿았을 때 수직 속도 리셋
            if (isGrounded && verticalVelocity < 0) {
                verticalVelocity = -2f; // 완전한 0보다 작은 값으로 지면에 붙어있게 함
            }
        }

        private void ApplyMovement() {
            // 수평 이동과 수직 이동 벡터 결합
            Vector3 finalMove = moveDirection;
            finalMove.y = verticalVelocity;

            // 캐릭터 컨트롤러 이동
            characterController.Move(finalMove * Time.deltaTime);
        }

        private void UpdateAnimator() {
            // 애니메이터가 없으면 리턴
            if (animator == null) return;

            // 속도 값 계산 (0에서 1 사이로 정규화)
            float speedPercent = 0f;
            if (currentState == MovementState.Walking)
                speedPercent = 0.5f;
            else if (currentState == MovementState.Running)
                speedPercent = 1f;

            // 애니메이터 파라미터 업데이트
            animator.SetFloat(animIDSpeed, speedPercent, 0.1f, Time.deltaTime);
            animator.SetBool(animIDGrounded, isGrounded);

            // 점프 애니메이션 리셋
            if (isGrounded && currentState != MovementState.Jumping) {
                animator.SetBool(animIDJump, false);
            }

            // 공중에 있을 때 상태
            animator.SetBool(animIDFreeFall, !isGrounded && verticalVelocity < 0);
        }

        // 착지 임팩트 처리 코루틴
        private IEnumerator HardLandingRoutine(float impactForce) {
            // 착지 임팩트 효과 (필요시 추가)
            Debug.Log($"Hard landing with impact force: {impactForce}");

            // 하드 랜딩 시간 (임팩트에 비례해서 증가 가능)
            float landingTime = 0.5f + (impactForce * 0.5f);

            // 하드 랜딩 동안 대기
            yield return new WaitForSeconds(landingTime);

            // 하드 랜딩 상태 종료
            if (currentState == MovementState.HardLanding) {
                currentState = MovementState.Idle;
            }
        }

        // 현재 상태 반환 메서드 (외부 접근용)
        public MovementState GetCurrentState() {
            return currentState;
        }

        // 낙하 속도 반환 (외부 접근용)
        public float GetFallVelocity() {
            return verticalVelocity;
        }

        // 디버그용 시각화
        private void OnDrawGizmosSelected() {
            if (groundCheck == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}