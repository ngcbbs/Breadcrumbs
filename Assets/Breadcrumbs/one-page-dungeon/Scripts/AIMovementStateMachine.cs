using UnityEngine;
using System.Collections;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public enum AIMovementState {
        Idle,
        Combat,
        Flee,
        Disabled
    }

    public class AIMovementStateMachine : MonoBehaviour {
        public Transform player;
        public AIStateTransitionSettings transitionSettings;

        [Header("Movement Components")]
        public AIIdleMovement idleMovement;
        public AICombatMovement combatMovement;
        public AIFleeMovement fleeMovement;

        // 현재 상태
        [SerializeField]
        private AIMovementState _currentState = AIMovementState.Idle;

        // 타이머
        private float _loseInterestTimer = 0f;
        private float _fleeTimer = 0f;
        private float _fleeMinimumTimer = 0f; // 최소 도망 시간을 강제하는 타이머

        // 상태 변화 플래그
        private bool _wasPlayerInRange = false;
        private bool _isFleeingFromSkill = false;

        private void Start() {
            // 시작 상태 설정 (Idle)
            SetState(AIMovementState.Idle);

            // 모든 컴포넌트에 플레이어 참조 설정
            if (combatMovement != null && player != null) {
                combatMovement.target = player;
            }

            if (fleeMovement != null && player != null) {
                fleeMovement.target = player;
            }
        }

        private void Update() {
            if (player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // 현재 상태에 따른 행동
            switch (_currentState) {
                case AIMovementState.Idle:
                    HandleIdleState(distanceToPlayer);
                    break;

                case AIMovementState.Combat:
                    HandleCombatState(distanceToPlayer);
                    break;

                case AIMovementState.Flee:
                    HandleFleeState(distanceToPlayer);
                    break;

                case AIMovementState.Disabled:
                    // 아무 행동도 하지 않음
                    break;
            }
        }

        private void HandleIdleState(float distanceToPlayer) {
            // 플레이어가 감지 범위에 들어왔을 때
            if (distanceToPlayer <= transitionSettings.detectionRadius) {
                SetState(AIMovementState.Combat);
            }
        }

        private void HandleCombatState(float distanceToPlayer) {
            // 플레이어가 관심 범위를 벗어났을 때
            if (distanceToPlayer > transitionSettings.loseInterestRadius) {
                if (!_wasPlayerInRange) {
                    _loseInterestTimer += Time.deltaTime;

                    // 지정된 시간 동안 관심 범위를 벗어나 있었으면 Idle로 전환
                    if (_loseInterestTimer >= transitionSettings.loseInterestDelay) {
                        SetState(AIMovementState.Idle);
                    }
                } else {
                    _wasPlayerInRange = false;
                    _loseInterestTimer = 0f;
                }
            } else {
                _wasPlayerInRange = true;
                _loseInterestTimer = 0f;
            }
        }

        private void HandleFleeState(float distanceToPlayer) {
            // 도망 타이머 증가
            _fleeTimer += Time.deltaTime;
            _fleeMinimumTimer += Time.deltaTime;

            // 최소 도망 시간이 지나지 않았으면 무조건 도망 상태 유지
            if (_fleeMinimumTimer < transitionSettings.fleeForceMinDuration) {
                return; // 최소 시간 동안은 상태 전환 로직을 평가하지 않음
            }

            // 도망 시간이 끝났거나 플레이어와 거리가 가까워지면 전투로 돌아감
            bool timerExpired =
                _fleeTimer >= Random.Range(transitionSettings.fleeMinDuration, transitionSettings.fleeMaxDuration);
            bool playerClose = distanceToPlayer <= transitionSettings.fleeReturnCombatDistance;

            // 스킬에 의한 도망이면 시간이 충분히 지난 후에만 전투로 복귀
            if (_isFleeingFromSkill) {
                if (timerExpired && playerClose) {
                    SetState(AIMovementState.Combat);
                }
            }
            // 일반 도망이면 타이머가 만료되었을 때만 전투로 복귀
            else if (timerExpired) {
                SetState(AIMovementState.Combat);
            }
        }

        public void SetState(AIMovementState newState) {
            // 이미 같은 상태면 무시
            if (_currentState == newState) return;

            // 이전 상태 비활성화
            DisableCurrentState();

            // 새 상태 설정
            _currentState = newState;

            // 타이머 리셋
            if (newState != AIMovementState.Flee)
            {
                _fleeTimer = 0f;
                _fleeMinimumTimer = 0f;
            }
            
            if (newState != AIMovementState.Combat)
            {
                _loseInterestTimer = 0f;
            }

            // 새 상태 활성화
            EnableCurrentState();

            Debug.Log($"{gameObject.name} changed state to: {_currentState}");
        }

        private void DisableCurrentState() {
            switch (_currentState) {
                case AIMovementState.Idle:
                    if (idleMovement != null) idleMovement.enabled = false;
                    break;

                case AIMovementState.Combat:
                    if (combatMovement != null) combatMovement.enabled = false;
                    break;

                case AIMovementState.Flee:
                    if (fleeMovement != null) fleeMovement.enabled = false;
                    break;
            }
        }

        private void EnableCurrentState() {
            switch (_currentState) {
                case AIMovementState.Idle:
                    if (idleMovement != null) idleMovement.enabled = true;
                    break;

                case AIMovementState.Combat:
                    if (combatMovement != null) combatMovement.enabled = true;
                    break;

                case AIMovementState.Flee:
                    if (fleeMovement != null) {
                        fleeMovement.enabled = true;
                        // 새로 도망 상태가 되면 타이머 리셋
                        _fleeTimer = 0f;
                        _fleeMinimumTimer = 0f;
                    }

                    break;
            }
        }

        // 플레이어 스킬에 의한 도망 상태 전환
        public void FleeFromPlayerSkill() {
            SetState(AIMovementState.Flee);
            _isFleeingFromSkill = true;

            // 플레이어 스킬에 의한 도망은 일정 시간 후 자동으로 전투 상태로 복귀
            StartCoroutine(ResetFleeingFromSkill());
        }

        private IEnumerator ResetFleeingFromSkill() {
            yield return new WaitForSeconds(Random.Range(transitionSettings.fleeMinDuration, transitionSettings.fleeMaxDuration));
            _isFleeingFromSkill = false;
        }

        // 디버깅용 기즈모
        private void OnDrawGizmos() {
            if (!transitionSettings.showDebugVisuals || player == null) return;

            // 감지 반경
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, transitionSettings.detectionRadius);

            // 관심 잃는 반경
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, transitionSettings.loseInterestRadius);

            // 현재 상태 표시
            Vector3 labelPos = transform.position + Vector3.up * 2f;

            if (Application.isPlaying) {
                switch (_currentState) {
                    case AIMovementState.Idle:
                        Gizmos.color = Color.green;
                        break;
                    case AIMovementState.Combat:
                        Gizmos.color = Color.red;
                        break;
                    case AIMovementState.Flee:
                        Gizmos.color = Color.blue;
                        break;
                    default:
                        Gizmos.color = Color.gray;
                        break;
                }

                Gizmos.DrawSphere(labelPos, 0.3f);
            }
        }
    }
}