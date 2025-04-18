using UnityEngine;

namespace Breadcrumbs.Player {
    public class MeleeAttackBehavior : PlayerBehaviorBase {
        public MeleeAttackBehavior(PlayerController controller) {
            SetController(controller);
            InputManager.OnInput += OnInputReceived;
        }

        ~MeleeAttackBehavior() {
            InputManager.OnInput -= OnInputReceived;
        }

        private void OnInputReceived(object sender, InputData input) {
            if (input.attackPressed) {
                HandleAttackInput();
            }
        }

        public override void Execute() {
            // 필요에 따라 Update()에서 지속적인 공격 관련 로직 처리
        }

        public override void HandleInput(InputData input) {
            // InputManager의 OnInput 이벤트에서 처리하므로 여기서는 필요 없음
        }

        private void HandleAttackInput() {
            Debug.Log("[MeleeAttackBehavior] 근접 공격!");
            // 실제 근접 공격 로직 (transform 접근 불가)
            // 공격 시작 이벤트 발행
        }
    }
}