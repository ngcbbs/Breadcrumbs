using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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
        private InputData _lastInput; // 최근 입력 저장 (버퍼링 로직에 활용 가능)

        // 상태 관리 사전
        private readonly Dictionary<Type, PlayerStateBase> _states = new Dictionary<Type, PlayerStateBase>();
        
        // 상태별 활성화할 Behavior 타입 목록
        private readonly Dictionary<Type, List<Type>> _stateBehaviors = new Dictionary<Type, List<Type>>()
        {
            { typeof(IdleState), new List<Type> { typeof(MovementBehavior), typeof(MeleeAttackBehavior) } },
            { typeof(MoveState), new List<Type> { typeof(MovementBehavior) } },
            { typeof(DashState), new List<Type> { typeof(DashBehavior) } }
            // 다른 상태에 대한 Behavior 목록 추가
        };

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
            
            // 입력 버퍼 처리를 위한 설정. 
            InputManager.Instance.Initialized(this);
            
            // InputManager 이벤트 구독
            InputManager.OnInput += HandleInputEvent;
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
            foreach (var behavior in _behaviors.Values.Where(b => IsBehaviorEnabledForCurrentState(b.GetType())).OrderByDescending(b => b.Priority))
            {
                behavior.Execute();
            }

            // 현재 상태 업데이트
            if (_currentState != null) {
                _currentState.UpdateState();
            }
        }

        // 현재 상태에서 Behavior가 활성화되어 있는지 확인
        private bool IsBehaviorEnabledForCurrentState(Type behaviorType)
        {
            if (_currentState != null && _stateBehaviors.TryGetValue(_currentState.GetType(), out var enabledBehaviors))
            {
                return enabledBehaviors.Contains(behaviorType);
            }
            return false;
        }

        public void ChangeState<T>() where T : PlayerStateBase {
            ChangeState(typeof(T));
        }

        // 상태 변경 (타입 기반)
        public void ChangeState(Type newState) {
            if (_currentState != null && _currentState.GetType() == newState) 
                return;

            if (_states.TryGetValue(newState, out var nextState)) {
                var from = (_currentState != null) ? _currentState?.GetType().Name : "null";
                Debug.Log($"상태 변경 요청 {from} -> {nextState.GetType()}");
                _currentState?.OnExitState();
                _currentState = nextState;
                _currentState.SetController(this);
                _currentState.OnEnterState();
                UpdateActiveBehaviors();
            } else {
                Debug.LogError($"[PlayerController] 상태 {newState.Name}를 찾을 수 없습니다.");
            }
        }
        
        // 현재 상태에 따라 활성화/비활성화할 Behavior 업데이트
        private void UpdateActiveBehaviors()
        {
            // 모든 Behavior의 활성화 상태를 현재 상태에 맞춰 업데이트
            foreach (var behavior in _behaviors.Values)
            {
                bool shouldBeEnabled = IsBehaviorEnabledForCurrentState(behavior.GetType());
                // Behavior에 활성화/비활성화 상태를 관리하는 별도의 플래그가 있다면 여기서 업데이트
                // 현재는 Execute 함수에서 IsBehaviorEnabledForCurrentState 결과를 사용하므로 명시적인 활성화/비활성화는 생략
            }
        }
        
        // InputManager 이벤트 핸들러 - 입력 발생 즉시 상태에 전달
        private void HandleInputEvent(object sender, InputData input)
        {
            _lastInput = input; // 최근 입력 업데이트
            _currentState?.HandleInput(input);
            // 필요하다면 여기서 특정 조건에 따라 inputBuffer.EnqueueInput(input);
        }

        // 입력 버퍼에 입력 추가
        public bool BufferInput(InputData input) {
            _inputBuffer.EnqueueInput(input);
            return true;
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
            transform.Translate(velocity, Space.World);
        }

        // Player 회전 처리
        public void RotatePlayer(float angle) {
            transform.Rotate(Vector3.up, angle);
        }

        [ReadOnly]
        public string CurrentStateName;

        private void OnDrawGizmos() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3f);

            if (CurrentState != null) {
                CurrentStateName = CurrentState.GetType().ToShortString();
                //Handles.Label(transform.position, $"{CurrentState.GetType().ToShortString()}");
            }
        }
    }
}