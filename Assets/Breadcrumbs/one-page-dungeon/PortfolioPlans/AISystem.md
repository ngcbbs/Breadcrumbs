# AI 시스템 구현 계획

## 개요

포트폴리오 프로젝트의 AI 시스템은 던전 크롤러 게임의 몬스터와 환경 요소에 지능적인 행동을 부여하는 핵심 시스템입니다. 1개월의 개발 기간을 고려하여, 확장 가능한 아키텍처를 기반으로 우선순위가 높은 AI 기능을 구현할 계획입니다.

## 핵심 구현 요소

### 1. 기본 몬스터 AI 프레임워크

#### 상태 기반 AI 시스템
- **유한 상태 기계(FSM) 구현**
  - 기본 상태: 대기(Idle), 순찰(Patrol), 추적(Chase), 공격(Attack), 후퇴(Retreat)
  - 상태 전환 조건 및 지속 시간 관리
  - 상태별 애니메이션 통합
- **코드 구조**
```csharp
public abstract class BaseEnemyAI : MonoBehaviour
{
    protected IEnemyState currentState;
    protected Dictionary<EnemyStateType, IEnemyState> availableStates;
    
    protected virtual void SetupStates() 
    {
        // 기본 상태 초기화 및 등록
    }
    
    public void ChangeState(EnemyStateType newStateType)
    {
        // 상태 전환 로직
    }
    
    // 감지, 이동, 전투 관련 공통 기능
}
```

#### 기본 행동 패턴
- **순찰 시스템**
  - 웨이포인트 기반 기본 이동
  - 방 내 랜덤 이동 패턴
- **추적 알고리즘**
  - Unity NavMesh 기반 경로 탐색
  - 장애물 회피 및 최적 경로 계산
- **공격 패턴**
  - 근접 공격 메커니즘
  - 원거리 공격 (투사체 시스템 연동)

### 2. 감지 시스템

#### 시각적 감지
- **시야각 및 거리 기반 감지**
  - 콘(Cone) 형태의 시야 구현
  - 장애물에 의한 시야 차단
  - 시야 범위 시각화 (디버깅용)
- **구현 접근법**
```csharp
public bool CheckLineOfSight(Transform target)
{
    // 시야각 계산
    Vector3 directionToTarget = (target.position - transform.position).normalized;
    float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
    
    if (angleToTarget <= fieldOfViewAngle * 0.5f)
    {
        // 장애물 확인을 위한 레이캐스트
        if (Physics.Raycast(eyePosition.position, directionToTarget, out RaycastHit hit, 
                           sightRange, obstacleLayer))
        {
            return hit.transform == target;
        }
    }
    return false;
}
```

#### 청각적 감지
- **소리 이벤트 시스템**
  - 플레이어 행동에 의한 소리 발생 및 전파
  - 소리 크기에 따른 감지 범위 조정
- **구현 방식**
  - 이벤트 기반 소리 전파 시스템
  - 콜라이더 기반 소리 감지 범위

### 3. 몬스터 유형별 AI

#### 근접 전투형 몬스터
- **기본 슬라임 AI**
  - 단순한 접근 및 공격 패턴
  - 체력에 따른 행동 변화
- **고블린 전사 AI**
  - 기본 전투 패턴 + 간단한 회피 동작
  - 집단 행동 고려 (다른 고블린 근처에서 공격력 증가)

#### 원거리 공격형 몬스터
- **고블린 궁수 AI**
  - 거리 유지 및 투사체 발사
  - 근접 시 후퇴 행동
- **구현 예시**
```csharp
public class ArcherEnemyAI : BaseEnemyAI
{
    [SerializeField] private float preferredCombatDistance = 5f;
    [SerializeField] private float retreatDistance = 3f;
    
    protected override void SetupStates()
    {
        base.SetupStates();
        // 원거리 공격자용 특수 상태 추가
        availableStates.Add(EnemyStateType.MaintainDistance, new MaintainDistanceState(this));
    }
    
    // 원거리 특화 로직 구현
}
```

### 4. 환경 상호작용 AI

#### 기본 함정 시스템
- **감지 기반 함정**
  - 플레이어 접근 시 활성화
  - 단순한 피해 또는 상태 효과 적용
- **구현 방식**
```csharp
public class TriggerTrap : MonoBehaviour
{
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private float damage = 10f;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated && !inCooldown)
        {
            StartCoroutine(ActivateTrap());
        }
    }
    
    // 함정 활성화 및 효과 적용 로직
}
```

#### 상호작용 가능한 오브젝트
- **보물 상자 및 컨테이너**
  - 단순 열기/잠금 메커니즘
  - 함정이 설치된 미믹 상자 (적 AI와 연동)
- **문 및 장애물**
  - 열쇠 기반 잠금 해제
  - 레버나 압력판으로 작동하는 문

### 5. AI 관리 및 최적화

#### AI 매니저 시스템
- **전역 AI 관리자**
  - 활성 AI 엔티티 관리
  - 성능 기반 AI 활성화/비활성화
- **구현 구조**
```csharp
public class AIManager : MonoBehaviour
{
    private List<BaseEnemyAI> activeEnemies = new List<BaseEnemyAI>();
    [SerializeField] private float aiUpdateInterval = 0.2f;
    
    public void RegisterEnemy(BaseEnemyAI enemy)
    {
        // AI 등록 및 관리 로직
    }
    
    private void UpdateAIPriorities()
    {
        // 거리 및 중요도 기반 AI 우선순위 조정
    }
}
```

#### 성능 최적화 전략
- **LOD (Level of Detail) AI**
  - 플레이어와의 거리에 따른 AI 복잡성 조절
  - 원거리 몬스터는 간소화된 업데이트 사용
- **시간차 업데이트**
  - 모든 AI를 동시에 업데이트하지 않고 시간차 적용
  - 프레임당 처리할 AI 수 제한

## 구현 우선순위 및 일정

### 1주차: 기본 프레임워크 구현
- 유한 상태 기계(FSM) 기본 구조 구현
- 기본 감지 시스템 구현
- NavMesh 기반 이동 시스템 구현

### 2주차: 기본 몬스터 AI 구현
- 근접 전투형 몬스터(슬라임) AI 구현
- 원거리 공격형 몬스터(고블린 궁수) 기본 구현
- 기본 공격 및 회피 패턴 구현

### 3주차: 환경 상호작용 및 확장
- 기본 함정 시스템 구현
- 상호작용 오브젝트 AI 연동
- AI 매니저 및 최적화 시스템 구현

### 4주차: 통합 및 테스트
- 네트워크 동기화 연동
- 성능 최적화 및 버그 수정
- 데모 시나리오용 AI 설정 및 테스트

## 기술 부채 및 미래 확장

현재 1개월 포트폴리오 프로젝트에서는 시간 제약으로 인해 다음 기능들은 구현하지 않지만, 향후 확장 가능성을 고려한 구조로 설계합니다:

1. **행동 트리(Behavior Tree)**: 더 복잡한 의사 결정을 위한 구조
2. **유틸리티 AI**: 다중 요인을 고려한 점수 기반 결정 시스템
3. **고급 군집 행동**: 지능적인 협동 및 전술적 그룹 행동
4. **학습형 AI**: 플레이어 행동에 적응하는 학습 메커니즘
5. **고급 환경 인식**: 더 정교한 환경 분석 및 활용 AI

## 테스트 및 디버깅

### 시각화 도구
- **AI 상태 시각화**
  - 게임 내 디버그 오버레이로 현재 상태 표시
  - 시야 및 감지 범위 시각화
- **로깅 시스템**
  - 중요 AI 결정 및 전환 로깅
  - 성능 모니터링 및 병목 현상 추적

### 유닛 테스트
- **상태 전환 테스트**
  - 유효한 상태 전환만 발생하는지 확인
  - 조건부 전환 테스트
- **감지 시스템 테스트**
  - 다양한 환경 조건에서의 감지 정확도 테스트
  - 가장자리 케이스 및 예외 상황 처리 확인

## 통합 계획

### 네트워크 연동
- **AI 상태 동기화**
  - 서버 기반 AI 결정 및 클라이언트 시각화
  - 중요 이벤트만 네트워크 전송 (최적화)
- **분산 처리**
  - 클라이언트-서버 간 AI 작업 분배
  - 권한 모델에 따른 책임 분리

### 게임플레이 시스템 연동
- **전투 시스템 통합**
  - 공격 판정 및 피해 처리
  - 상태 효과 적용 및 반응
- **아이템 및 인벤토리 연동**
  - 몬스터 전리품 드롭 시스템
  - 특수 아이템에 대한 AI 반응

## 결론

본 AI 시스템 구현 계획은 1개월이라는 제한된 시간 내에 작동하는 기본 AI 시스템을 구축하는 데 초점을 맞추고 있습니다. 확장 가능한 아키텍처를 기반으로 주요 기능을 우선 구현하고, 향후 개발을 위한 기반을 마련하는 것을 목표로 합니다. FSM 기반 상태 관리, NavMesh 기반 이동, 기본 감지 시스템, 그리고 몬스터 유형별 특화된 AI 구현을 통해 게임에 생동감을 불어넣을 계획입니다.

[메인 계획으로 돌아가기](./MasterPlan.md)
