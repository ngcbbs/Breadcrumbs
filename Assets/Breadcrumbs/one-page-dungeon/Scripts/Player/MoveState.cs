using UnityEngine;

namespace Breadcrumbs.Player {
    public class MoveState : PlayerStateBase {
        public override void OnEnterState() {
            Debug.Log("Move State Entered");
        }

        public override void OnExitState() {
            Debug.Log("Move State Exited");
        }

        public override void UpdateState() {
            // 이동 상태에서의 로직
        }

        public override void HandleInput(InputData input) {
            if (!input.IsMoving) {
                Debug.Log("move -> idle 상태 변경");
                CurrentController.ChangeState<IdleState>();
            }

            if (input.attackPressed) {
                Debug.Log("Move State: Attack Input Received");
                // 이동 중 공격 로직
            }

            if (input.dashPressed) {
                Debug.Log("대쉬 상태 변경 처리 yo");
                CurrentController.ChangeState<DashState>();
            }
        }
    }
}