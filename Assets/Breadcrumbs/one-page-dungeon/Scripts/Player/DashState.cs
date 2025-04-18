using UnityEngine;

namespace Breadcrumbs.Player {
    public class DashState : PlayerStateBase
    {
        private float _dashTimer = 0f;
        private float _dashDuration = 0.5f; // 설정 파일에서 관리할 수 있습니다.

        public override void OnEnterState()
        {
            Debug.Log("Dash State Entered");
            CurrentController.EnableBehavior(typeof(DashBehavior));
            _dashTimer = 0f;
        }

        public override void OnExitState()
        {
            Debug.Log("Dash State Exited");
            CurrentController.DisableBehavior(typeof(DashBehavior));
        }

        public override void UpdateState()
        {
            _dashTimer += Time.deltaTime;
            if (_dashTimer >= _dashDuration)
            {
                CurrentController.ChangeState<IdleState>();
            }
            // 대쉬 상태에서의 추가 로직 (이동 불가 등)
        }

        public override void HandleInput(InputData input)
        {
            // 대쉬 중에는 추가 입력 처리 제한 (선택 사항)
        }
    }
}