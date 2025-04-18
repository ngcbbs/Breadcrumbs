using UnityEngine;

namespace Breadcrumbs.Player {
    public class IdleState : PlayerStateBase {
        public override void OnEnterState() {
            Debug.Log("Idle State Entered");
        }

        public override void OnExitState() {
            Debug.Log("Idle State Exited");
        }

        public override void UpdateState() {
            // Idle 상태에서의 로직 (애니메이션 등)
        }

        public override void HandleInput(InputData input) {
            if (input.IsMoving) {
                Debug.Log("idle -> move 상태 변경");
                CurrentController.ChangeState<MoveState>();
            }

            if (input.attackPressed) {
                Debug.Log("Idle State: Attack Input Received");
                // 공격 시작 로직 (Behavior에 이벤트 발행 등)
            }

            if (input.dashPressed) {
                CurrentController.ChangeState<DashState>();
            }
        }
    }
}