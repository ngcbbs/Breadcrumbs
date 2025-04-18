using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Breadcrumbs.Player {
    public class PlayerController : MonoBehaviour {
        [SerializeField]
        private PlayerSettings settings;
        private ContextData _context;
        private PlayerStateBase _currentState;
        
        public ContextData Context => _context;
        public PlayerSettings Settings => settings;
        
        public PlayerStateBase CurrentState => _currentState;
        private readonly Dictionary<Type, PlayerBehaviorBase> _behaviors = new Dictionary<Type, PlayerBehaviorBase>();
        private readonly InputBuffer _inputBuffer = new InputBuffer();

        // 상태 관리 사전
        private readonly Dictionary<Type, PlayerStateBase> _states = new Dictionary<Type, PlayerStateBase>();

        void Awake() {
            _context = new ContextData();

            // 상태 객체 생성 및 등록
            _states.Add(typeof(IdleState), new IdleState());
            _states.Add(typeof(MoveState), new MoveState());
            _states.Add(typeof(DashState), new DashState());

            // Behavior 객체 생성 및 등록
            _behaviors.Add(typeof(MovementBehavior), new MovementBehavior(this));
            _behaviors.Add(typeof(MeleeAttackBehavior), new MeleeAttackBehavior(this));
            _behaviors.Add(typeof(DashBehavior), new DashBehavior(this));

            // 초기 State 설정
            ChangeState(typeof(IdleState));

            // 모든 상태에 PlayerController 할당
            foreach (var state in _states.Values) {
                state.SetController(this);
            }

            // 모든 Behavior에 PlayerController 할당 (생성 시 전달했으므로 불필요)
            InputManager.Instance.Initialized();
        }

        void OnDestroy() {
            if (_currentState != null) {
                _currentState.OnExitState();
            }

            // Behavior 이벤트 구독 해제 (생성자에서 했으므로 OnDestroy에서 해제)
            foreach (var behavior in _behaviors.Values) {
                // 명시적인 소멸자 대신 인터페이스를 사용하여 해제할 수도 있습니다.
                if (behavior is MovementBehavior movementBehavior) {
                    /* 해제 로직 */
                }

                if (behavior is MeleeAttackBehavior meleeAttackBehavior) {
                    /* 해제 로직 */
                }

                if (behavior is DashBehavior dashBehavior) {
                    /* 해제 로직 */
                }
            }
        }

        void Update() {
            // 입력 버퍼에서 입력 가져와 처리
            InputData? input = _inputBuffer.DequeueInput();
            if (input.HasValue && _currentState != null) {
                _currentState.HandleInput(input.Value);
            }

            // 활성화된 Behavior들의 Execute 호출
            foreach (var behavior in _behaviors.Values
                         .Where(b => (_currentState != null && IsBehaviorEnabledForState(b.GetType(), _currentState.GetType())))
                         .OrderByDescending(b => b.Priority)) {
                behavior.Execute();
            }

            // 현재 상태 업데이트
            if (_currentState != null) {
                _currentState.UpdateState();
            }
        }

        // 특정 상태에서 Behavior가 활성화되어 있는지 확인하는 로직 (예시)
        private bool IsBehaviorEnabledForState(Type behaviorType, Type stateType) {
            // 각 상태별로 활성화되는 Behavior를 정의하는 방식이 필요합니다.
            // 여기서는 간단한 예시로 MovementBehavior는 항상 활성화되어 있다고 가정합니다.
            if (behaviorType == typeof(MovementBehavior)) return true;
            if (stateType == typeof(IdleState) && (behaviorType == typeof(MeleeAttackBehavior))) return true;
            if (stateType == typeof(DashState) && (behaviorType == typeof(DashBehavior))) return true;
            return false;
        }

        // Behavior 활성화 (더 이상 MonoBehaviour의 enabled 사용 안함)
        public void EnableBehavior(Type behaviorType) {
            if (_behaviors.TryGetValue(behaviorType, out var behavior)) {
                // Behavior 활성화 상태를 별도로 관리할 수 있습니다.
                // 현재는 상태 기반으로 활성화를 제어하므로 별도 플래그는 생략합니다.
            }
        }

        // Behavior 비활성화
        public void DisableBehavior(Type behaviorType) {
            if (_behaviors.TryGetValue(behaviorType, out var behavior)) {
                // Behavior 비활성화 상태를 별도로 관리할 수 있습니다.
                // 현재는 상태 기반으로 활성화를 제어하므로 별도 플래그는 생략합니다.
            }
        }

        public void ChangeState<T>() where T : PlayerStateBase {
            ChangeState(typeof(T));
        }

        // 상태 변경 (타입 기반)
        public void ChangeState(Type newState) {
            if (_currentState != null && _currentState.GetType() == newState) return;

            if (_states.TryGetValue(newState, out var nextState)) {
                _currentState?.OnExitState();
                _currentState = nextState;
                _currentState.SetController(this);
                _currentState.OnEnterState();
            } else {
                Debug.LogError($"[PlayerController] 상태 {newState.Name}를 찾을 수 없습니다.");
            }
        }

        // 입력 버퍼에 입력 추가
        public void BufferInput(InputData input) {
            _inputBuffer.EnqueueInput(input);
        }

        // 현재 상태 가져오기 (타입 기반)
        public T GetState<T>() where T : PlayerStateBase {
            if (_states.TryGetValue(typeof(T), out var state)) {
                return (T)state;
            }

            return null;
        }

        // Player 이동 처리
        public void MovePlayer(Vector2 velocity) {
            transform.Translate(new Vector3(velocity.x, 0, velocity.y));
        }
        
        public void MovePlayer(Vector3 velocity) {
            transform.Translate(velocity);
        }
        
        // Player 회전 처리
        public void RotatePlayer(float angle)
        {
            transform.Rotate(Vector3.up, angle);
        }
    }
}