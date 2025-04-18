using UnityEngine;

namespace Breadcrumbs.Player {
    public class IdleState : PlayerStateBase {
        public override void OnEnterState() {
            Debug.Log("Idle State Entered");
            CurrentController.EnableBehavior(typeof(MovementBehavior));
            CurrentController.EnableBehavior(typeof(MeleeAttackBehavior));
        }

        public override void OnExitState() {
            Debug.Log("Idle State Exited");
            CurrentController.DisableBehavior(typeof(MovementBehavior));
            CurrentController.DisableBehavior(typeof(MeleeAttackBehavior));
        }

        public override void UpdateState() {
            // Idle 상태에서의 로직 (애니메이션 등)
        }

        public override void HandleInput(InputData input) {
            if (input.IsMoving) {
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