# 백엔드 시스템 구현 계획

## 개요

던전 크롤러 포트폴리오 프로젝트의 백엔드 시스템은 MagicOnion 기반의 서버 아키텍처를 중심으로 구성됩니다. 1개월이라는 제한된 시간 내에 효율적으로 구현하기 위해 핵심 기능에 초점을 맞추면서도 확장 가능한 구조를 유지하는 것을 목표로 합니다.

## 핵심 구현 요소

### 1. MagicOnion 서버 구조

#### 기본 서버 아키텍처
- **프로젝트 구성**
  - 서버 프로젝트: .NET 6.0 기반 콘솔 애플리케이션
  - 공유 프로젝트: 클라이언트-서버 간 공유 인터페이스 및 DTO
  - 클라이언트 통합: Unity 프로젝트 내 MagicOnion 클라이언트 구현
- **코드 구조**
```csharp
// 서버 진입점
public class Program
{
    public static void Main(string[] args)
    {
        GrpcEnvironment.SetLogger(new ConsoleLogger());
        
        var builder = WebApplication.CreateBuilder(args);
        
        // MagicOnion 및 의존성 등록
        builder.Services.AddGrpc();
        builder.Services.AddMagicOnion();
        
        // 서비스 등록
        builder.Services.AddSingleton<DungeonManager>();
        builder.Services.AddSingleton<PlayerSessionManager>();
        
        var app = builder.Build();
        
        // MagicOnion 미들웨어 구성
        app.UseRouting();
        app.UseGrpcWeb();
        app.MapMagicOnionService();
        
        app.Run("http://localhost:5000");
    }
}
```

#### MagicOnion 서비스 및 허브
- **서비스 인터페이스 (단방향 RPC)**
  - 플레이어 인증 및 세션 관리
  - 던전 생성 및 구성 요청
  - 게임 데이터 조회
- **허브 인터페이스 (양방향 실시간 통신)**
  - 플레이어 액션 및 상태 동기화
  - 몬스터 AI 상태 및 행동 동기화
  - 게임 이벤트 및 알림
- **구현 예시**
```csharp
// 서비스 인터페이스 (공유 프로젝트)
public interface IDungeonService : IService<IDungeonService>
{
    UnaryResult<DungeonDataDto> GenerateDungeonAsync(DungeonRequestDto request);
    UnaryResult<bool> SaveDungeonProgressAsync(DungeonProgressDto progress);
    UnaryResult<PlayerDataDto> GetPlayerDataAsync(PlayerRequestDto request);
}

// 허브 인터페이스 (공유 프로젝트)
public interface IDungeonHub : IStreamingHub<IDungeonHub, IDungeonHubReceiver>
{
    Task<bool> JoinDungeonAsync(DungeonJoinRequestDto request);
    Task LeaveCurrentDungeonAsync();
    Task UpdatePlayerPositionAsync(Vector3Dto position, Vector3Dto rotation);
    Task PerformActionAsync(PlayerActionDto action);
}

public interface IDungeonHubReceiver
{
    void OnPlayerJoined(PlayerInfoDto player);
    void OnPlayerLeft(string playerId);
    void OnPlayerPositionUpdated(string playerId, Vector3Dto position, Vector3Dto rotation);
    void OnPlayerActionPerformed(string playerId, PlayerActionDto action);
    void OnMonsterStateUpdated(string monsterId, MonsterStateDto state);
    void OnGameEvent(GameEventDto gameEvent);
}
```

### 2. MessagePack 직렬화

#### 기본 설정
- **MessagePack 리졸버 구성**
  - 네트워크 통신에 사용되는 모든 타입 등록
  - 최적화된 직렬화/역직렬화 설정
- **구현 방식**
```csharp
// 공유 프로젝트의 MessagePack 설정
public static class MessagePackInitializer
{
    public static void Initialize()
    {
        var resolver = CompositeResolver.Create(
            // 기본 내장 리졸버
            StandardResolver.Instance,
            // Unity 특화 리졸버
            Unity.Formatters.UnityResolver.Instance,
            // 사용자 정의 포매터 등록
            GeneratedResolver.Instance
        );

        var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        MessagePackSerializer.DefaultOptions = options;
    }
}
```

#### DTO (Data Transfer Object) 구조
- **기본 메시지 타입**
  - 플레이어 정보 및 상태
  - 몬스터 정보 및 상태
  - 던전 구성 및 상태
  - 게임 이벤트 및 액션
- **예시 DTO 클래스**
```csharp
[MessagePackObject]
public class PlayerInfoDto
{
    [Key(0)]
    public string PlayerId { get; set; }
    
    [Key(1)]
    public string PlayerName { get; set; }
    
    [Key(2)]
    public int CharacterClass { get; set; }
    
    [Key(3)]
    public PlayerStatusDto Status { get; set; }
}

[MessagePackObject]
public class DungeonDataDto
{
    [Key(0)]
    public string DungeonId { get; set; }
    
    [Key(1)]
    public int Seed { get; set; }
    
    [Key(2)]
    public int DungeonType { get; set; }
    
    [Key(3)]
    public RoomDto[] Rooms { get; set; }
    
    [Key(4)]
    public CorridorDto[] Corridors { get; set; }
    
    [Key(5)]
    public MonsterSpawnDto[] MonsterSpawns { get; set; }
}
```

### 3. 세션 및 상태 관리

#### 플레이어 세션 관리
- **세션 생성 및 관리**
  - 플레이어 연결 및 인증
  - 세션 상태 추적
  - 타임아웃 및 연결 해제 처리
- **구현 예시**
```csharp
public class PlayerSessionManager
{
    private readonly ConcurrentDictionary<string, PlayerSession> _activeSessions = 
        new ConcurrentDictionary<string, PlayerSession>();
    
    public PlayerSession CreateSession(string playerId, IDungeonHubReceiver receiver)
    {
        var session = new PlayerSession(playerId, receiver);
        _activeSessions.TryAdd(playerId, session);
        return session;
    }
    
    public bool TryGetSession(string playerId, out PlayerSession session)
    {
        return _activeSessions.TryGetValue(playerId, out session);
    }
    
    public void RemoveSession(string playerId)
    {
        _activeSessions.TryRemove(playerId, out _);
    }
    
    // 비활성 세션 정리, 상태 업데이트 등의 메서드
}
```

#### 던전 인스턴스 관리
- **던전 생성 및 라이프사이클**
  - 던전 인스턴스 생성 및 구성
  - 플레이어 참여 및 이탈 처리
  - 던전 상태 업데이트 및 종료 조건
- **던전 매니저 예시**
```csharp
public class DungeonManager
{
    private readonly ConcurrentDictionary<string, DungeonInstance> _activeDungeons = 
        new ConcurrentDictionary<string, DungeonInstance>();
    private readonly ILogger<DungeonManager> _logger;
    
    public DungeonManager(ILogger<DungeonManager> logger)
    {
        _logger = logger;
    }
    
    public DungeonInstance CreateDungeon(DungeonRequestDto request)
    {
        var dungeonId = Guid.NewGuid().ToString();
        var dungeon = new DungeonInstance(dungeonId, request, _logger);
        _activeDungeons.TryAdd(dungeonId, dungeon);
        return dungeon;
    }
    
    public bool TryGetDungeon(string dungeonId, out DungeonInstance dungeon)
    {
        return _activeDungeons.TryGetValue(dungeonId, out dungeon);
    }
    
    // 던전 제거, 상태 업데이트, 정리 등의 메서드
}
```

### 4. 게임 로직 처리

#### 서버 측 게임 로직
- **권한 모델**
  - 서버 권한: 던전 생성, 몬스터 AI, 전투 결과, 아이템 드롭
  - 클라이언트 권한: 플레이어 입력, 애니메이션, 효과
- **주요 로직 구현**
  - 전투 계산 및 결과 처리
  - 아이템 및 보상 생성
  - 상태 효과 적용 및 관리

#### 서버-클라이언트 동기화
- **상태 동기화 전략**
  - 초기 상태 전송 (던전 구조, 몬스터 배치 등)
  - 델타 업데이트 (변경된 부분만 전송)
  - 주기적 상태 확인 및 보정
- **구현 접근 방식**
```csharp
public class DungeonHubService : StreamingHubBase<IDungeonHub, IDungeonHubReceiver>, IDungeonHub
{
    private readonly DungeonManager _dungeonManager;
    private readonly PlayerSessionManager _sessionManager;
    private DungeonInstance _currentDungeon;
    private string _playerId;
    
    // 생성자 및 초기화 로직
    
    public async Task<bool> JoinDungeonAsync(DungeonJoinRequestDto request)
    {
        // 플레이어 세션 생성
        _playerId = request.PlayerId;
        var session = _sessionManager.CreateSession(_playerId, Context.GetConnectionContext());
        
        // 던전 참여 처리
        if (!_dungeonManager.TryGetDungeon(request.DungeonId, out _currentDungeon))
        {
            return false;
        }
        
        // 던전에 플레이어 추가
        await _currentDungeon.AddPlayerAsync(session);
        
        // 초기 상태 전송
        var initialState = _currentDungeon.GetFullState();
        await Clients.Caller.OnInitialStateReceived(initialState);
        
        // 다른 플레이어에게 입장 알림
        await BroadcastExceptCaller(client => client.OnPlayerJoined(session.ToPlayerInfoDto()));
        
        return true;
    }
    
    // 기타 허브 메서드 구현
}
```

### 5. 오류 처리 및 로깅

#### 오류 처리 전략
- **예외 관리**
  - 전역 예외 처리기
  - 클라이언트에 안전한 오류 응답
  - 서버 내부 오류 격리
- **구현 예시**
```csharp
public class ErrorHandlingFilter : MagicOnionFilterAttribute
{
    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // 오류 로깅
            context.Logger.LogError(ex, "서비스 처리 중 오류 발생");
            
            // 클라이언트에 안전한 응답
            if (context.MethodType == MethodType.Unary)
            {
                context.SetStatus(StatusCode.Internal, "서버 오류가 발생했습니다.");
            }
            
            throw;
        }
    }
}
```

#### 로깅 시스템
- **구조화된 로깅**
  - Serilog 기반 로깅 구현
  - 로그 레벨 및 필터링
  - JSON 형식 로그 출력
- **모니터링 및 알림**
  - 중요 이벤트 로깅 및 모니터링
  - 오류 및 성능 문제 알림
  - 로그 집계 및 분석

### 6. 성능 최적화

#### 네트워크 최적화
- **메시지 압축 및 최적화**
  - MessagePack 기반 효율적 직렬화
  - 델타 압축 (변경된 부분만 전송)
  - 중요도 기반 업데이트 빈도 조절
- **네트워크 트래픽 관리**
  - 지역 관심(AOI) 기반 동기화
  - 플레이어 근처의 엔티티만 상세 업데이트
  - 우선순위 기반 업데이트 스케줄링

#### 서버 리소스 관리
- **비동기 작업 처리**
  - Task 기반 비동기 패턴 적용
  - 백그라운드 작업 관리
  - 장기 실행 작업 격리
- **메모리 관리**
  - 오브젝트 풀링 및 재사용
  - 가비지 컬렉션 최적화
  - 메모리 사용량 모니터링

## 구현 우선순위 및 일정

### 1주차: 기본 프레임워크 구현
- MagicOnion 서버 프로젝트 설정
- 기본 서비스 및 허브 인터페이스 정의
- MessagePack 설정 및 기본 DTO 구현

### 2주차: 핵심 기능 구현
- 세션 및 던전 매니저 구현
- 기본 게임 로직 서버 측 구현
- 오류 처리 및 로깅 시스템 구현

### 3주차: 클라이언트 통합 및 동기화
- 클라이언트 측 MagicOnion 통합
- 상태 동기화 시스템 구현
- 실시간 통신 테스트 및 최적화

### 4주차: 통합 및 테스트
- 모든 시스템 통합 및 테스트
- 성능 최적화 및 버그 수정
- 최종 포트폴리오 데모 준비

## 기술 부채 및 미래 확장

현재 1개월 포트폴리오 프로젝트에서는 시간 제약으로 인해 다음 기능들은 구현하지 않지만, 향후 확장 가능성을 고려한 구조로 설계합니다:

1. **사용자 인증 및 계정 관리**: OAuth 또는 JWT 기반 인증 시스템
2. **영구 데이터 저장소**: 데이터베이스 연동 및 영구 저장
3. **매치메이킹 및 로비 시스템**: 고급 플레이어 매칭 및 방 관리
4. **리더보드 및 통계**: 플레이어 성과 및 게임 통계 추적
5. **서버 확장성**: 부하 분산 및 서버 클러스터링

## 테스트 및 디버깅

### 서버 테스트 전략
- **유닛 테스트**
  - 핵심 게임 로직 및 알고리즘 테스트
  - 서비스 및 매니저 클래스 테스트
  - 모의 객체(Mock)를 활용한 의존성 격리
- **통합 테스트**
  - 클라이언트-서버 통신 테스트
  - 서비스 간 상호작용 테스트
  - 실제 환경 유사 시나리오 테스트

### 디버깅 도구
- **로그 분석**
  - 구조화된 로그 검색 및 필터링
  - 로그 수준 조정 및 상세 정보 확인
  - 오류 및 예외 상황 트래킹
- **성능 프로파일링**
  - 네트워크 트래픽 분석
  - CPU 및 메모리 사용량 모니터링
  - 병목 현상 식별 및 최적화

## 통합 계획

### 클라이언트와의 통합
- **공유 인터페이스 및 DTO**
  - 클라이언트와 서버 간 코드 공유
  - 일관된 메시지 형식 유지
  - 버전 호환성 관리
- **Unity 클라이언트 연동**
  - MagicOnion 클라이언트 통합
  - 연결 관리 및 재연결 처리
  - 네트워크 상태 및 지연 시간 처리

### 게임 시스템 연동
- **던전 생성 시스템 연동**
  - 서버 측 던전 생성 및 클라이언트 렌더링 조정
  - 던전 상태 동기화 및 변경 관리
- **AI 시스템 연동**
  - 서버 측 AI 결정 및 클라이언트 시각화
  - 효율적인 AI 상태 동기화

## 결론

본 백엔드 시스템 구현 계획은 MagicOnion을 기반으로 확장 가능하고 유지보수 가능한 서버 아키텍처를 1개월 내에 구현하는 것을 목표로 합니다. 핵심 기능에 우선순위를 두고, 명확한 코드 구조와 서비스 인터페이스를 통해 클라이언트와의 원활한 통합을 가능하게 합니다. 초기 구현에서는 기본적인 세션 관리, 던전 생성, 실시간 동기화에 집중하되, 향후 확장 가능성을 고려한 구조로 설계합니다.

[메인 계획으로 돌아가기](./MasterPlan.md)
