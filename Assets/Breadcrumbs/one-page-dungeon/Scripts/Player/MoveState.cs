using UnityEngine;

namespace Breadcrumbs.Player {
    public class MoveState : PlayerStateBase {
        public override void OnEnterState() {
            Debug.Log("Move State Entered");
            CurrentController.EnableBehavior(typeof(MovementBehavior));
        }

        public override void OnExitState() {
            Debug.Log("Move State Exited");
            CurrentController.DisableBehavior(typeof(MovementBehavior));
        }

        public override void UpdateState() {
            // 이동 상태에서의 로직
        }

        public override void HandleInput(InputData input) {
            if (!input.IsMoving) {
                CurrentController.ChangeState<IdleState>();
            }

            if (input.attackPressed) {
                Debug.Log("Move State: Attack Input Received");
                // 이동 중 공격 로직
            }

            if (input.dashPressed) {
                CurrentController.ChangeState<DashState>();
            }
        }
    }
}