using UnityEngine;

namespace Breadcrumbs.Player {
    public class DashBehavior : PlayerBehaviorBase {
        private bool _isDashing;
        private float _dashTimer;
        private float _dashDuration = 0.2f;
        private Vector3 _dashDirection = Vector3.zero; // 대쉬 방향 저장
        private float _currentDashForce;

        public DashBehavior(PlayerController controller) {
            SetController(controller);
            InputManager.OnInput += OnInputReceived;
        }

        ~DashBehavior() {
            InputManager.OnInput -= OnInputReceived;
        }

        private void OnInputReceived(object sender, InputData input) {
            if (input.dashPressed && !_isDashing && (input.forwardInput != 0f || input.rotationInput != 0f)) {
                _isDashing = true;
                _dashTimer = 0f;
                _currentDashForce = Settings.dashForce;

                // 최근 이동 입력을 기반으로 대쉬 방향 결정
                if (input.forwardInput > 0f) {
                    _dashDirection = controller.transform.forward;
                } else if (input.forwardInput < 0f) {
                    _dashDirection = -controller.transform.forward;
                } else if (input.rotationInput > 0f) {
                    _dashDirection = controller.transform.right;
                } else if (input.rotationInput < 0f) {
                    _dashDirection = -controller.transform.right;
                } else if (input.strafeInput > 0f) {
                    _dashDirection = controller.transform.right;
                } else if (input.strafeInput < 0f) {
                    _dashDirection = -controller.transform.right;
                } else {
                    _dashDirection = controller.transform.forward; // 이동 입력 없을 시 현재 바라보는 방향으로 대쉬 (기본값)
                }

                Debug.Log($"[DashBehavior] 대쉬 시작! 방향: {_dashDirection}");
                controller.ChangeState<DashState>();
            } else if (input.dashPressed && !_isDashing && input.strafeInput != 0f) {
                _isDashing = true;
                _dashTimer = 0f;
                _currentDashForce = Settings.dashForce;
                _dashDirection = controller.transform.right * input.strafeInput;
                Debug.Log($"[DashBehavior] 평행 이동 대쉬 시작! 방향: {_dashDirection}");
                controller.ChangeState<DashState>();
            } else if (input.dashPressed && !_isDashing) {
                _isDashing = true;
                _dashTimer = 0f;
                _currentDashForce = Settings.dashForce;
                _dashDirection = controller.transform.forward; // 이동 입력 없을 시 현재 바라보는 방향으로 대쉬 (기본값)
                Debug.Log($"[DashBehavior] 제자리 대쉬 시작! 방향: {_dashDirection}");
                controller.ChangeState<DashState>();
            }
        }

        public override void Execute() {
            if (_isDashing) {
                controller.MovePlayer(_dashDirection * (_currentDashForce * Time.deltaTime));
                _dashTimer += Time.deltaTime;
                _currentDashForce = Mathf.Lerp(Settings.dashForce, 0f, _dashTimer / _dashDuration);

                if (_dashTimer >= _dashDuration) {
                    _isDashing = false;
                    Debug.Log("[DashBehavior] 대쉬 종료!");
                    // 대쉬 종료 이벤트 발행
                    if (controller.CurrentState != null) {
                        controller.ChangeState(typeof(IdleState));
                    }
                }
            }
        }

        public override void HandleInput(InputData input) {
            // InputManager의 OnInput 이벤트에서 처리하므로 여기서는 필요 없음
        }
    }
}