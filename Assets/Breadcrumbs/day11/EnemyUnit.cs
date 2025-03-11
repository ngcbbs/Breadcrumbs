using UnityEngine;
using Unity.Behavior;
using UnityEngine.Serialization;

namespace Breadcrumbs.day11 {
    public class EnemyUnit : MonoBehaviour {
        [Header("유닛 속성")] [SerializeField] private int health = 100;
        [SerializeField] private float moveSpeed = 3f;

        [FormerlySerializedAs("behaviorGraphAsset")] [Header("Behavior Graph 설정")] [SerializeField]
        private BehaviorGraphAgent behaviorGraph; // 행동 그래프 에셋
        
        // 블랙보드 키 (Behavior Graph에서 사용할 변수들)
        private static readonly int TargetKey = Animator.StringToHash("Target");
        private static readonly int HealthKey = Animator.StringToHash("Health");
        private static readonly int SpeedKey = Animator.StringToHash("MoveSpeed");
        private static readonly int IsDeadKey = Animator.StringToHash("IsDead");

        // 유닛이 죽었을 때 발생하는 이벤트
        public delegate void UnitDiedHandler(EnemyUnit unit);

        public event UnitDiedHandler OnUnitDied;
        
        private BlackboardReference _blackboardReference;
        private BlackboardVariable<Transform> _target;
        private BlackboardVariable<int> _health;
        private BlackboardVariable<float> _moveSpeed;
        private BlackboardVariable<bool> _isDead;
        
        public CharacterController controller;
        private bool isGrounded;
        private float verticalVel = -0.5f;

        private void Awake() {
            controller = GetComponent<CharacterController>();
            
            // Behavior Graph 인스턴스 생성
            if (behaviorGraph != null) {
                _blackboardReference = behaviorGraph.BlackboardReference;

                // 초기 블랙보드 값 설정
                if (!_blackboardReference.GetVariable("Target", out _target))
                    Debug.LogWarning("blackboard 에서 Target를 찾을 수 없음.");
                if (!_blackboardReference.GetVariable("Health", out _health))
                    Debug.LogWarning("blackboard 에서 Health를 찾을 수 없음.");
                if (!_blackboardReference.GetVariable("MoveSpeed", out _moveSpeed))
                    Debug.LogWarning("blackboard 에서 MoveSpeed를 찾을 수 없음.");
                if (!_blackboardReference.GetVariable("IsDead", out _isDead))
                    Debug.LogWarning("blackboard 에서 IsDead를 찾을 수 없음.");
                
                _target.SetObjectValueWithoutNotify(null);
                _health.SetValueWithoutNotify(health);
                _moveSpeed.SetValueWithoutNotify(moveSpeed);
                _isDead.SetValueWithoutNotify(false);

                _isDead.OnValueChanged += () => {
                    if (_isDead.Value) {
                        Debug.Log("사망!");
                        Die();
                    }
                };
            }
            else {
                Debug.LogError("Behavior Graph Asset이 설정되지 않았습니다.");
            }
        }

        private void OnEnable() {
            behaviorGraph.Restart();
        }

        private void OnDisable() {
            behaviorGraph.End();
        }

        private void OnDestroy() {
            behaviorGraph.End();
        }

        private void Update() {
            if (behaviorGraph)
                behaviorGraph.Update();
            
            isGrounded = controller.isGrounded;
            if (isGrounded)
            {
                verticalVel -= 0;
            }
            else
            {
                verticalVel -= 1;
                
                var moveVector = new Vector3(0, verticalVel * .2f * Time.deltaTime, 0);
                controller.Move(moveVector);
            }
        }

        public void SetTarget(Transform target) {
            if (_target != null)
                _target.ObjectValue = target;
        }

        public void TakeDamage(int damage) {
            if (behaviorGraph == null) return;

            // 현재 체력 가져오기
            health = _health.Value;
            health -= damage;

            // 블랙보드 값 업데이트
            _health.Value = health;

            // 체력이 0 이하면 사망 처리
            if (health <= 0) {
                _isDead.Value = true;
                Die();
            }
        }

        public void Die() {
            // 죽음 이벤트 호출
            OnUnitDied?.Invoke(this);

            SetTarget(null);

            // 오브젝트 파괴 (딜레이를 주어 사망 애니메이션이 재생될 수 있도록 함)
            Destroy(gameObject, 0.2f);
        }
    }
}
