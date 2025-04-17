using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.AISystem {
    // 메인 AI 이동 컨트롤러
    public class AIMovementController : MonoBehaviour {
        private AICombatController _combatController;
        
        public Transform target;
        public AIMovementSettings settings;

        [Header("행동 가중치")]
        [Range(0, 1)]
        public float combatWeight = 1f;
        [Range(0, 1)]
        public float patrolWeight = 0.5f;
        [Range(0, 1)]
        public float dodgeWeight = 0.8f;
        [Range(0, 1)]
        public float dashWeight = 0.6f;
        [Range(0, 1)]
        public float formationWeight = 0.4f;

        private List<AIMovementBehaviorBase> _behaviors = new List<AIMovementBehaviorBase>();
        private AIContextData _contextData;
        private AIMovementBehaviorBase _currentBehavior;
        private float _behaviorTimer;

        // 타이머 및 쿨다운 처리
        private Dictionary<Type, float> _behaviorCooldowns = new Dictionary<Type, float>();

        // 레이어 마스크
        [SerializeField]
        private LayerMask threatLayer;
        [SerializeField]
        private LayerMask obstacleLayer;
        [SerializeField]
        private LayerMask allyLayer;

        private void Start() {
            InitializeComponents();
            InitializeCombatSystem(); // 전투 시스템 초기화 추가
        }

        private void InitializeComponents() {
            // 컨텍스트 초기화
            _contextData = new AIContextData(transform, target);

            // 이동 행동 생성 및 등록
            RegisterBehavior(new CombatMovementBehavior(settings), combatWeight);
            RegisterBehavior(new PatrolBehavior(settings), patrolWeight);
            RegisterBehavior(new DodgeBehavior(settings), dodgeWeight);
            RegisterBehavior(new DashBehavior(settings), dashWeight);
            RegisterBehavior(new FormationBehavior(settings), formationWeight);

            // 기본 행동 설정
            _currentBehavior = GetBehaviorByType<CombatMovementBehavior>();
            _currentBehavior?.Initialize(_contextData);
        }
        
        // 기존 Start 메서드를 오버라이드하지 않도록 부분 클래스 활용
        private void InitializeCombatSystem() {
            _combatController = GetComponent<AICombatController>();
            
            if (_combatController == null) {
                _combatController = gameObject.AddComponent<AICombatController>();
            }
        }
        
        // 기존 Update 메서드 수정
        private bool ShouldSkipMovement() {
            // 전투 행동 중이면 이동 건너뛰기
            return _combatController != null && _combatController.IsPerformingCombatAction();
        }

        private void Update() {
            // 전투 행동 중이면 이동 처리 건너뛰기
            if (ShouldSkipMovement()) {
                return;
            }
            
            UpdateContextData();
            UpdateBehaviorTimers();

            AIMovementBehaviorBase newBehavior = SelectBestBehavior();

            // 행동이 바뀌면 정리 및 초기화
            if (newBehavior != _currentBehavior) {
                _currentBehavior?.Cleanup(_contextData);
                _currentBehavior = newBehavior;
                _currentBehavior.Initialize(_contextData);
            }

            // 선택된 행동 실행
            Vector3 moveDirection = _currentBehavior.CalculateDirection(_contextData);
            MoveInDirection(moveDirection);
        }

        private void UpdateContextData() {
            _contextData.UpdateData();
            _contextData.currentDirection = transform.forward;

            // 위협 감지
            DetectThreats();

            // 아군 감지
            DetectAllies();
        }

        private void DetectThreats() {
            _contextData.threats.Clear();

            Collider[] threatColliders = new Collider[10];
            int threatCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                settings.threatDetectionRadius,
                threatColliders,
                threatLayer
            );

            for (int i = 0; i < threatCount; i++) {
                Transform threatTransform = threatColliders[i].transform;
                Vector3 directionToThreat = threatTransform.position - transform.position;
                float distance = directionToThreat.magnitude;

                // 위협 타입 결정 (여기서는 단순화)
                AIThreatType threatType = AIThreatType.Projectile;
                if (threatColliders[i].CompareTag("MeleeAttack")) threatType = AIThreatType.MeleeAttack;
                else if (threatColliders[i].CompareTag("AreaEffect")) threatType = AIThreatType.AreaEffect;

                // 거리에 따른 위험도
                float dangerLevel = Mathf.Clamp01(1f - (distance / settings.threatDetectionRadius));

                AIThreat threat = new AIThreat(threatTransform, dangerLevel, threatType) {
                    direction = directionToThreat.normalized,
                    distance = distance
                };

                _contextData.threats.Add(threat);
            }
        }

        private void DetectAllies() {
            _contextData.allies.Clear();

            Collider[] allyColliders = new Collider[10];
            int allyCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                settings.detectionRadius,
                allyColliders,
                allyLayer
            );

            for (int i = 0; i < allyCount; i++) {
                if (allyColliders[i].transform != transform) {
                    _contextData.allies.Add(allyColliders[i].transform);
                }
            }
        }

        private void UpdateBehaviorTimers() {
            List<Type> expiredCooldowns = new List<Type>();

            foreach (var cooldown in _behaviorCooldowns) {
                _behaviorCooldowns[cooldown.Key] -= Time.deltaTime;

                if (_behaviorCooldowns[cooldown.Key] <= 0) {
                    expiredCooldowns.Add(cooldown.Key);
                }
            }

            foreach (var type in expiredCooldowns) {
                _behaviorCooldowns.Remove(type);
            }
        }

        private AIMovementBehaviorBase SelectBestBehavior() {
            AIMovementBehaviorBase bestBehavior = null;
            float highestScore = float.MinValue;

            foreach (var behavior in _behaviors) {
                // 쿨다운 중인 행동은 제외
                Type behaviorType = behavior.GetType();
                if (_behaviorCooldowns.ContainsKey(behaviorType)) {
                    continue;
                }

                // 행동 점수 = 적합성 * 사용자 가중치
                float weight = GetBehaviorWeight(behavior);
                float suitability = behavior.EvaluateSuitability(_contextData);
                float score = suitability * weight;

                if (score > highestScore) {
                    highestScore = score;
                    bestBehavior = behavior;
                }
            }

            // 없으면 기본 전투 이동
            return bestBehavior ?? GetBehaviorByType<CombatMovementBehavior>();
        }

        private float GetBehaviorWeight(AIMovementBehaviorBase behavior) {
            if (behavior is CombatMovementBehavior) return combatWeight;
            if (behavior is PatrolBehavior) return patrolWeight;
            if (behavior is DodgeBehavior) return dodgeWeight;
            if (behavior is DashBehavior) return dashWeight;
            if (behavior is FormationBehavior) return formationWeight;
            return 0.5f; // 기본 가중치
        }

        private void MoveInDirection(Vector3 direction) {
            // 실제 이동 및 회전 처리
            Vector3 move = direction.normalized * (settings.moveSpeed * Time.deltaTime);
            transform.position += move;

            if (direction != Vector3.zero) {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * settings.turnSpeed
                );
            }
        }

        public void SetBehaviorCooldown<T>(float cooldownTime) where T : AIMovementBehaviorBase {
            _behaviorCooldowns[typeof(T)] = cooldownTime;
        }

        private void RegisterBehavior(AIMovementBehaviorBase behavior, float weight) {
            _behaviors.Add(behavior);
        }

        private T GetBehaviorByType<T>() where T : AIMovementBehaviorBase {
            foreach (var behavior in _behaviors) {
                if (behavior is T typedBehavior) {
                    return typedBehavior;
                }
            }

            return null;
        }

        private void OnDrawGizmos() {
            if (_currentBehavior != null && _contextData != null) {
                // 현재 행동 기즈모
                _currentBehavior.DrawGizmos(_contextData);

                // 위협 감지 범위
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, settings.threatDetectionRadius);

                // 일반 감지 범위
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.1f);
                Gizmos.DrawWireSphere(transform.position, settings.detectionRadius);
            }
        }
    }
}
