using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace day5_scrap {
    public class ActionCharacterController : MonoBehaviour {
        // 스타일 랭크 열거형
        private enum StyleRank {
            C,
            B,
            A,
            S,
            SS,
            SSS
        }

        #region Components
        
        [Header("Components")]
        [SerializeField] 
        private CharacterController characterController;
        [SerializeField] 
        private Animator animator;
        [SerializeField] 
        private Camera mainCamera;
        private PlayerInputActions playerInputs;

        #endregion

        #region Movement Parameters

        [Header("Movement Settings")] public float moveSpeed = 8f;
        public float rotationSpeed = 10f;
        public float acceleration = 50f;
        public float airControl = 0.5f;

        private Vector3 moveDirection;
        private Vector3 currentVelocity;
        private bool isGrounded;

        #endregion

        #region Jump Parameters

        [Header("Jump Settings")] public float jumpForce = 15f;
        public float doubleJumpForce = 12f;
        public float wallJumpForce = 20f;
        public int maxJumps = 2;
        public float wallCheckDistance = 0.5f;

        private int jumpCount;
        private bool canWallJump;

        #endregion

        #region Dash Parameters

        [Header("Dash Settings")] public float dashSpeed = 20f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 0.5f;

        private bool isDashing;
        private float dashTimer;
        private float lastDashTime;

        #endregion

        #region Combat Parameters

        [Header("Combat Settings")] public float comboWindow = 0.5f;
        public Transform lockOnTarget;
        public float lockOnDistance = 20f;
        public LayerMask enemyLayer;

        private bool isAttacking;
        private int comboCount;
        private float lastAttackTime;
        private Dictionary<string, AnimationClip> combos;

        #endregion

        #region Style System

        [Header("Style System")] public float stylePoints;
        public float styleDecayRate = 1f;
        public bool devilTriggerActive;
        private StyleRank currentStyleRank;

        #endregion

        private void Awake() {
            characterController = GetComponent<CharacterController>();
            //animator = GetComponent<Animator>();
            mainCamera = Camera.main;
            InitializeCombos();

            // 입력 시스템 초기화
            playerInputs = new PlayerInputActions();

            // 입력 이벤트 바인딩
            //playerInputs.Movement.performed += ctx => HandleMovementInput();
            playerInputs.Jump.performed += ctx => HandleJump();
            playerInputs.Dash.performed += ctx => HandleDash();
            playerInputs.Attack.performed += ctx => HandleAttack();
            playerInputs.LockOn.performed += ctx => ToggleLockOn();
            
            /*
            playerInputs.Movement.performed += _ => LogPerformed("Movement Performed");
            playerInputs.Jump.performed += _ => LogPerformed("Jump Performed");
            playerInputs.Dash.performed += _ => LogPerformed("Dash Performed");
            playerInputs.Attack.performed += _ => LogPerformed("Attack Performed");
            playerInputs.LockOn.performed += _ => LogPerformed("LockOn Performed");
            playerInputs.CameraLook.performed += _ => LogPerformed("CameraLook Performed");
            // */
        }
        
        /*
        private void LogPerformed(string message) {
            Debug.Log(message);
        }
        // */

        private void OnEnable() {
            playerInputs.asset.Enable();
        }

        private void OnDisable() {
            playerInputs.asset.Disable();
        }

        private void Update() {
            HandleMovementInput();
            UpdateMovement();
            UpdateCombat();
            UpdateStyle();
        }

        private Vector2 movementInput;

        private void HandleMovementInput() {
            Vector2 input = movementInput = playerInputs.Movement.ReadValue<Vector2>();
            Vector3 moveInput = new Vector3(input.x, 0f, input.y).normalized;
            if (moveInput.magnitude >= 0.1f) {
                float targetAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
            else {
                moveDirection = Vector3.zero;
            }
        }

        private void HandleDash() {
            if (Time.time >= lastDashTime + dashCooldown) {
                StartDash();
            }
        }

        // UpdateMovement() 메서드는 동일하게 유지

        private void UpdateMovement() {
            isGrounded = characterController.isGrounded;

            if (isGrounded) {
                currentVelocity.y = -2f;
                jumpCount = 0;
            }
            else {
                currentVelocity.y += Physics.gravity.y * Time.deltaTime;
            }

            if (isDashing) {
                UpdateDash();
            }
            else {
                float targetSpeed = moveDirection.magnitude * moveSpeed;
                float speedDiff = targetSpeed - currentVelocity.magnitude;
                float accelRate = (speedDiff > 0f) ? acceleration : acceleration * 2f;

                if (!isGrounded) {
                    accelRate *= airControl;
                }

                currentVelocity = Vector3.Lerp(currentVelocity, moveDirection * targetSpeed, accelRate * Time.deltaTime);
            }

            if (moveDirection != Vector3.zero && !isDashing) {
                if (lockOnTarget != null) {
                    Vector3 targetDirection = (lockOnTarget.position - transform.position).normalized;
                    targetDirection.y = 0f;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(targetDirection),
                        rotationSpeed * Time.deltaTime);
                }
                else {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }

            characterController.Move(currentVelocity * Time.deltaTime);
            
            animator.SetFloat("Vertical", moveDirection.x);
            animator.SetFloat("Horizontal", moveDirection.y);
            if (moveDirection.sqrMagnitude > 0.1f)
                animator.SetFloat("Speed", isDashing ? 2f : moveDirection.sqrMagnitude);
            else 
                animator.SetFloat("Speed", 0f);
        }

        private void HandleJump() {
            if (isGrounded) {
                Debug.Log("Jump");
                isGrounded = false;
                currentVelocity.y = jumpForce;
                jumpCount = 1;
                animator.SetTrigger("Jump");
            }
            else if (jumpCount < maxJumps) {
                Debug.Log("Jump - Double");
                currentVelocity.y = doubleJumpForce;
                jumpCount++;
                animator.SetTrigger("DoubleJump");
            }
            else if (CheckWallJump()) {
                Debug.Log("Jump - Wall");
                Vector3 wallNormal = GetWallNormal();
                currentVelocity = (wallNormal + Vector3.up).normalized * wallJumpForce;
                animator.SetTrigger("WallJump");
            }
        }

        private bool CheckWallJump() {
            RaycastHit hit;
            return Physics.Raycast(transform.position, transform.forward, out hit, wallCheckDistance);
        }

        private Vector3 GetWallNormal() {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, wallCheckDistance)) {
                return hit.normal;
            }

            return Vector3.zero;
        }

        private void HandleAttack() {
            if (isAttacking && Time.time > lastAttackTime + comboWindow) {
                comboCount = 0;
            }

            if (comboCount < combos.Count) {
                isAttacking = true;
                lastAttackTime = Time.time;
                string comboAnimation = "Combo" + comboCount;
                animator.SetTrigger(comboAnimation);
                comboCount++;

                // 스타일 포인트 추가
                AddStylePoints(10f * comboCount); // 콤보가 늘어날수록 더 많은 포인트
            }
        }

        private void InitializeCombos() {
            combos = new Dictionary<string, AnimationClip>();
            // 애니메이터에서 콤보 애니메이션 클립 가져오기
            var animController = animator.runtimeAnimatorController;
            foreach (var clip in animController.animationClips) {
                if (clip.name.StartsWith("Combo")) {
                    combos.Add(clip.name, clip);
                }
            }
        }

        private void ToggleLockOn() {
            if (lockOnTarget == null) {
                // 가장 가까운 적 찾기
                Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnDistance, enemyLayer);
                float closestDistance = float.MaxValue;

                foreach (Collider collider in colliders) {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        lockOnTarget = collider.transform;
                    }
                }
            }
            else {
                lockOnTarget = null;
            }
        }

        private void UpdateCombat() {
            if (lockOnTarget != null) {
                // 락온 타겟이 범위를 벗어났는지 확인
                float distance = Vector3.Distance(transform.position, lockOnTarget.position);
                if (distance > lockOnDistance) {
                    lockOnTarget = null;
                }

                // 타겟이 유효한지 확인
                if (lockOnTarget != null && !lockOnTarget.gameObject.activeInHierarchy) {
                    lockOnTarget = null;
                }
            }

            // 콤보 시간 초과 체크
            if (isAttacking && Time.time > lastAttackTime + comboWindow) {
                isAttacking = false;
                comboCount = 0;
            }
        }

        private void UpdateStyle() {
            // 스타일 포인트 감소
            if (!isAttacking) {
                stylePoints -= styleDecayRate * Time.deltaTime;
                stylePoints = Mathf.Max(0, stylePoints);
            }

            // 스타일 랭크 업데이트
            UpdateStyleRank();

            // 데빌 트리거 조건 체크 (예: SSS 랭크에서 특정 시간 유지)
            if (currentStyleRank == StyleRank.SSS && stylePoints >= 950f) {
                devilTriggerActive = true;
            }
            else if (stylePoints < 500f) {
                devilTriggerActive = false;
            }
        }

        private void UpdateStyleRank() {
            StyleRank previousRank = currentStyleRank;

            if (stylePoints >= 900) currentStyleRank = StyleRank.SSS;
            else if (stylePoints >= 750) currentStyleRank = StyleRank.SS;
            else if (stylePoints >= 500) currentStyleRank = StyleRank.S;
            else if (stylePoints >= 300) currentStyleRank = StyleRank.A;
            else if (stylePoints >= 100) currentStyleRank = StyleRank.B;
            else currentStyleRank = StyleRank.C;

            // 랭크 변경 시 이벤트 발생 또는 효과 재생
            if (previousRank != currentStyleRank) {
                OnStyleRankChanged(previousRank, currentStyleRank);
            }
        }

        private void OnStyleRankChanged(StyleRank previousRank, StyleRank newRank) {
            // 랭크 상승 시 효과
            if (newRank > previousRank) {
                animator.SetTrigger("StyleUp");
                // 여기에 랭크 상승 효과음, 파티클 등 추가
            }
        }

        public void AddStylePoints(float points) {
            stylePoints += points;
            stylePoints = Mathf.Min(stylePoints, 1000);
        }

        // 애니메이션 이벤트에서 호출되는 메서드
        public void OnAttackEnd() {
            isAttacking = false;
        }

        private void OnDestroy() {
            playerInputs.Dispose();
        }

        private void StartDash() {
            if (Time.time < lastDashTime + dashCooldown) return;

            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;

            // 대시 방향 결정
            Vector3 dashDirection;
            if (lockOnTarget != null) {
                // 락온 상태일 때는 타겟 방향으로 대시
                dashDirection = (lockOnTarget.position - transform.position).normalized;
                dashDirection.y = 0f; // 수직 방향 제거
            }
            else if (moveDirection != Vector3.zero) {
                // 이동 중일 때는 이동 방향으로 대시
                dashDirection = moveDirection;
            }
            else {
                // 정지 상태일 때는 캐릭터가 바라보는 방향으로 대시
                dashDirection = transform.forward;
            }

            // 대시 속도 적용
            currentVelocity = dashDirection * dashSpeed;

            // 대시 시작 효과
            animator.SetTrigger("Dash");

            // 대시 중 무적 프레임 설정 (옵션)
            SetInvincible(true);

            // 대시 이펙트 생성
            CreateDashEffect();

            // 스타일 포인트 추가
            AddStylePoints(5f);
        }

        private void UpdateDash() {
            if (!isDashing) return;

            dashTimer -= Time.deltaTime;

            // 대시 궤적 효과 업데이트
            UpdateDashTrail();

            if (dashTimer <= 0f) {
                EndDash();
            }
            else {
                // 대시 중 방향 전환 가능 여부 (옵션)
                UpdateDashDirection();
            }
        }

        private void EndDash() {
            isDashing = false;

            // 대시 종료 시 속도 감소
            currentVelocity *= 0.5f;

            // 무적 해제
            SetInvincible(false);

            // 대시 이펙트 종료
            StopDashEffect();

            // 대시 종료 애니메이션
            animator.SetTrigger("DashEnd");
        }

        private void SetInvincible(bool invincible) {
            // 무적 상태 처리 로직
            if (TryGetComponent<DamageReceiver>(out var damageReceiver)) {
                damageReceiver.isInvincible = invincible;
            }
        }

        private void CreateDashEffect() {
            // 대시 시작 시 이펙트 생성
            if (TryGetComponent<CharacterEffects>(out var effects)) {
                effects.PlayDashEffect();
            }
        }

        private void UpdateDashTrail() {
            // 대시 중 잔상 이펙트 업데이트
            if (TryGetComponent<CharacterEffects>(out var effects)) {
                effects.UpdateDashTrail(transform.position);
            }
        }

        private void StopDashEffect() {
            // 대시 이펙트 정리
            if (TryGetComponent<CharacterEffects>(out var effects)) {
                effects.StopDashEffect();
            }
        }

        private void UpdateDashDirection() {
            // 대시 중 방향 전환 가능 여부 및 로직
            if (lockOnTarget != null) {
                Vector3 toTarget = (lockOnTarget.position - transform.position).normalized;
                toTarget.y = 0f;
                currentVelocity = Vector3.Lerp(currentVelocity.normalized, toTarget, Time.deltaTime * 2f) * dashSpeed;
            }
            else {
                // 입력에 따른 대시 방향 수정
                Vector2 input = playerInputs.Movement.ReadValue<Vector2>();
                if (input.magnitude > 0.1f) {
                    Vector3 newDirection = new Vector3(input.x, 0f, input.y).normalized;
                    float targetAngle = Mathf.Atan2(newDirection.x, newDirection.z) * Mathf.Rad2Deg +
                                        mainCamera.transform.eulerAngles.y;
                    Vector3 targetDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                    currentVelocity = Vector3.Lerp(currentVelocity.normalized, targetDirection, Time.deltaTime * 2f) * dashSpeed;
                }
            }
        }
    }
}


