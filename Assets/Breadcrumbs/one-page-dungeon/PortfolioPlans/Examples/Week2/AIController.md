# AI 컨트롤러 예제 코드 (Part 1)

이 문서는 기본 AI 컨트롤러 시스템의 구현을 보여줍니다. 상태 머신 패턴을 활용하여 몬스터의 다양한 행동 패턴을 관리하고, 시야 및 감지 시스템을 통해 플레이어와 상호작용합니다.

## AIController.cs

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    // AI 설정
    [Header("AI 설정")]
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float fieldOfViewAngle = 120f;
    [SerializeField] private float hearingRange = 15f;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolSpeed = 2f;
    
    // 순찰 지점 설정
    [Header("순찰 설정")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool randomPatrol = true;
    
    // 컴포넌트 참조
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyStats stats;
    private CombatSystem combatSystem;
    
    // 상태 관리
    private AIState currentState;
    private GameObject detectedPlayer;
    private Vector3 lastKnownPlayerPosition;
    private int currentPatrolIndex = 0;
    private float stateTimer = 0f;
    
    // 시각 관련
    [SerializeField] private LayerMask sightMask;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stats = GetComponent<EnemyStats>();
        combatSystem = GetComponent<CombatSystem>();
        
        // 초기 상태 설정
        ChangeState(AIState.Patrol);
    }
    
    private void Update()
    {
        // 죽은 상태면 모든 AI 처리 중지
        if (stats.IsDead)
        {
            agent.isStopped = true;
            return;
        }
        
        // 플레이어 감지 확인
        DetectPlayer();
        
        // 현재 상태 업데이트
        UpdateCurrentState();
        
        // 애니메이션 파라미터 업데이트
        UpdateAnimator();
    }
    
    private void UpdateCurrentState()
    {
        stateTimer -= Time.deltaTime;
        
        switch (currentState)
        {
            case AIState.Idle:
                UpdateIdleState();
                break;
                
            case AIState.Patrol:
                UpdatePatrolState();
                break;
                
            case AIState.Chase:
                UpdateChaseState();
                break;
                
            case AIState.Attack:
                UpdateAttackState();
                break;
                
            case AIState.Investigate:
                UpdateInvestigateState();
                break;
                
            case AIState.Stunned:
                UpdateStunnedState();
                break;
        }
    }
    
    #region 상태 업데이트 메서드
    
    private void UpdateIdleState()
    {
        // 일정 시간 후 순찰 상태로 전환
        if (stateTimer <= 0)
        {
            ChangeState(AIState.Patrol);
        }
    }
    
    private void UpdatePatrolState()
    {
        // 목적지에 도착한 경우
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
        {
            // 일정 시간 대기
            if (stateTimer <= 0)
            {
                // 다음 순찰 지점 설정
                SetNextPatrolPoint();
                stateTimer = patrolWaitTime;
            }
        }
    }
    
    private void UpdateChaseState()
    {
        // 플레이어가 감지 상태인 경우
        if (detectedPlayer != null)
        {
            // 플레이어 위치 추적
            agent.SetDestination(detectedPlayer.transform.position);
            lastKnownPlayerPosition = detectedPlayer.transform.position;
            
            // 공격 범위 내에 있는지 확인
            if (IsInAttackRange(detectedPlayer.transform.position))
            {
                ChangeState(AIState.Attack);
            }
        }
        else
        {
            // 마지막 알려진 위치로 이동
            agent.SetDestination(lastKnownPlayerPosition);
            
            // 목적지에 도착한 경우
            if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
            {
                ChangeState(AIState.Investigate);
            }
        }
    }
    
    private void UpdateAttackState()
    {
        // 플레이어가 감지 상태인 경우
        if (detectedPlayer != null)
        {
            // 플레이어를 바라보도록 회전
            Vector3 direction = (detectedPlayer.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            
            // 공격 쿨다운 확인
            if (stateTimer <= 0)
            {
                // 공격 범위 내에 있는지 다시 확인
                if (IsInAttackRange(detectedPlayer.transform.position))
                {
                    // 공격 실행
                    PerformAttack();
                    stateTimer = combatSystem.GetAttackCooldown();
                }
                else
                {
                    // 범위 밖이면 추적 상태로 전환
                    ChangeState(AIState.Chase);
                }
            }
        }
        else
        {
            // 플레이어 감지가 끊긴 경우 추적 상태로 전환
            ChangeState(AIState.Chase);
        }
    }
    
    private void UpdateInvestigateState()
    {
        // 일정 시간 조사 후 순찰 상태로 전환
        if (stateTimer <= 0)
        {
            ChangeState(AIState.Patrol);
        }
    }
    
    private void UpdateStunnedState()
    {
        // 스턴 시간 종료 후 이전 상태나 순찰 상태로 전환
        if (stateTimer <= 0)
        {
            ChangeState(detectedPlayer != null ? AIState.Chase : AIState.Patrol);
        }
    }
    
    #endregion

    #region 유틸리티 메서드
    
    private void ChangeState(AIState newState)
    {
        // 이전 상태 종료 처리
        switch (currentState)
        {
            case AIState.Patrol:
                break;
                
            case AIState.Chase:
                // 추적 관련 설정 초기화
                break;
                
            case AIState.Attack:
                // 공격 관련 설정 초기화
                break;
        }
        
        // 새 상태 설정
        currentState = newState;
        
        // 새 상태 초기화
        switch (newState)
        {
            case AIState.Idle:
                agent.isStopped = true;
                stateTimer = Random.Range(1f, 3f);
                break;
                
            case AIState.Patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                if (patrolPoints.Length > 0)
                {
                    SetNextPatrolPoint();
                }
                else
                {
                    // 순찰 지점이 없으면 Idle 상태로
                    ChangeState(AIState.Idle);
                }
                break;
                
            case AIState.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                break;
                
            case AIState.Attack:
                agent.isStopped = true;
                stateTimer = 0.5f; // 공격 준비 시간
                break;
                
            case AIState.Investigate:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                stateTimer = Random.Range(3f, 5f);
                break;
                
            case AIState.Stunned:
                agent.isStopped = true;
                // stateTimer는 외부에서 설정
                break;
        }
    }
    
    private void SetNextPatrolPoint()
    {
        // 순찰 지점이 없으면 리턴
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;
            
        // 다음 순찰 지점 설정
        if (randomPatrol)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        }
        else
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }
    
    private void DetectPlayer()
    {
        // 이전 감지 상태 저장
        bool wasPlayerDetected = (detectedPlayer != null);
        detectedPlayer = null;
        
        // 시야 범위 내 플레이어 탐색
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, sightRange, LayerMask.GetMask("Player"));
        
        foreach (var playerCollider in playersInRange)
        {
            PlayerController player = playerCollider.GetComponent<PlayerController>();
            
            if (player != null)
            {
                // 거리 계산
                float distanceToPlayer = Vector3.Distance(transform.position, playerCollider.transform.position);
                
                // 시야각 내에 있는지 확인
                Vector3 directionToPlayer = (playerCollider.transform.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                
                // 시야각 내에 있거나 가까운 거리에 있는 경우
                if (angleToPlayer < fieldOfViewAngle * 0.5f || distanceToPlayer < hearingRange * 0.3f)
                {
                    // 시야 방해물 확인
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToPlayer, out hit, sightRange, sightMask))
                    {
                        // 플레이어가 보이는 경우
                        if (hit.collider.gameObject == playerCollider.gameObject)
                        {
                            detectedPlayer = playerCollider.gameObject;
                            break;
                        }
                    }
                }
                
                // 소리로 감지 (달리기 상태인 경우)
                if (distanceToPlayer < hearingRange && player.IsRunning)
                {
                    // 소리 방해물 확인 (간소화)
                    detectedPlayer = playerCollider.gameObject;
                    break;
                }
            }
        }
        
        // 플레이어 감지 상태 변경 시 상태 전환
        if (detectedPlayer != null && !wasPlayerDetected)
        {
            // 처음 감지된 경우 추적 상태로 전환
            if (currentState != AIState.Stunned)
            {
                ChangeState(AIState.Chase);
            }
        }
        else if (detectedPlayer == null && wasPlayerDetected)
        {
            // 시야에서 사라진 경우, 현재 상태가 추적이나 공격이면 조사 상태로 전환
            if (currentState == AIState.Chase || currentState == AIState.Attack)
            {
                ChangeState(AIState.Investigate);
            }
        }
    }
    
    private bool IsInAttackRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= attackRange;
    }
    
    private void PerformAttack()
    {
        if (combatSystem != null && detectedPlayer != null)
        {
            // 플레이어 방향으로 공격
            Vector3 direction = (detectedPlayer.transform.position - transform.position).normalized;
            combatSystem.TryAttackInDirection(direction);
            
            // 애니메이션 트리거
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }
    
    private void UpdateAnimator()
    {
        if (animator != null)
        {
            // 이동 속도 파라미터 업데이트
            animator.SetFloat("Speed", agent.velocity.magnitude);
            
            // 상태 파라미터 업데이트
            animator.SetBool("IsChasing", currentState == AIState.Chase);
            animator.SetBool("IsAttacking", currentState == AIState.Attack);
            animator.SetBool("IsStunned", currentState == AIState.Stunned);
        }
    }

    // 외부에서 호출할 수 있는 메서드들
    
    // 피해 받을 때 호출되는 메서드
    public void OnTakeDamage(GameObject attacker)
    {
        // 공격자가 플레이어인 경우
        PlayerController player = attacker.GetComponent<PlayerController>();
        if (player != null)
        {
            // 공격자를 타겟으로 설정
            detectedPlayer = attacker;
            lastKnownPlayerPosition = attacker.transform.position;
            
            // 공격 범위 내에 있으면 공격, 아니면 추적
            if (IsInAttackRange(attacker.transform.position))
            {
                ChangeState(AIState.Attack);
            }
            else
            {
                ChangeState(AIState.Chase);
            }
        }
    }
    
    // 스턴 효과 적용
    public void ApplyStun(float duration)
    {
        ChangeState(AIState.Stunned);
        stateTimer = duration;
    }
    
    // 특정 위치 조사
    public void InvestigatePosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
        ChangeState(AIState.Investigate);
    }
    
    #endregion
    
    // 디버깅용 시각화
    private void OnDrawGizmosSelected()
    {
        // 시야 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 청각 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        
        // 시야각
        Gizmos.color = Color.white;
        Vector3 forward = transform.forward * sightRange;
        float halfFOV = fieldOfViewAngle * 0.5f * Mathf.Deg2Rad;
        Vector3 right = new Vector3(Mathf.Sin(halfFOV), 0, Mathf.Cos(halfFOV)) * sightRange;
        Vector3 left = new Vector3(-right.x, 0, right.z);
        
        right = transform.TransformDirection(right);
        left = transform.TransformDirection(left);
        
        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, right);
        Gizmos.DrawRay(transform.position, left);
    }
}

// AI 상태 열거형
public enum AIState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Investigate,
    Stunned
}
```

## EnemySpawner.cs (일부)

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int weight = 1;
        [Range(0, 1)] public float spawnChance = 1f;
    }
    
    [Header("스폰 설정")]
    [SerializeField] private EnemySpawnInfo[] enemyTypes;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool randomizeSpawnPoints = true;
    [SerializeField] private int maxEnemiesAtOnce = 5;
    [SerializeField] private float minSpawnDelay = 5f;
    [SerializeField] private float maxSpawnDelay = 15f;
    
    [Header("트리거 설정")]
    [SerializeField] private bool spawnOnTrigger = true;
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private bool continuousSpawning = false;
    
    // 현재 활성화된 적 목록
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawningActive = false;
    private Coroutine spawningCoroutine;
    
    // 랜덤 적 선택 (가중치 기반)
    private EnemySpawnInfo GetRandomEnemy()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
            return null;
            
        // 가중치 합계 계산
        int totalWeight = 0;
        foreach (var enemy in enemyTypes)
        {
            totalWeight += enemy.weight;
        }
        
        // 랜덤 값 생성
        int randomValue = Random.Range(0, totalWeight);
        
        // 가중치에 따른 적 선택
        int weightSum =
    
```
