using UnityEngine;

namespace Breadcrumbs.Player {
    public class MovementBehavior : PlayerBehaviorBase {
        private float forwardInput = 0f;
        private float rotationInput = 0f;
        private float strafeInput = 0f;

        public MovementBehavior(PlayerController controller) {
            SetController(controller);
            InputManager.OnInput += OnInputReceived;
        }

        ~MovementBehavior() {
            InputManager.OnInput -= OnInputReceived;
        }

        private void OnInputReceived(object sender, InputData input) {
            forwardInput = input.forwardInput;
            rotationInput = input.rotationInput;
            strafeInput = input.strafeInput;
        }

        public override void Execute() {
            // 전진 / 후진 이동
            if (forwardInput != 0f) {
                float moveSpeed = Settings.moveSpeed;
                if (forwardInput < 0f) {
                    moveSpeed *= Settings.backwardSpeedMultiplier;
                }

                Vector3 moveDirection = controller.transform.forward * (forwardInput * moveSpeed * Time.deltaTime);
                controller.MovePlayer(moveDirection);
            }

            // 좌우 회전
            if (rotationInput != 0f) {
                float rotationAmount = rotationInput * Settings.rotationSpeed * Time.deltaTime;
                controller.RotatePlayer(rotationAmount);
            }

            // 좌우 평행 이동 (Strafe)
            if (strafeInput != 0f) {
                Vector3 strafeDirection = controller.transform.right * (strafeInput * Settings.strafeSpeed * Time.deltaTime);
                controller.MovePlayer(strafeDirection);
            }
        }

        public override void HandleInput(InputData input) {
            // InputManager의 OnInput 이벤트에서 처리하므로 여기서는 필요 없음
        }
    }
}