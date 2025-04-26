# 네트워크 구조 및 MagicOnion 구현

## 시스템 개요

네트워크 시스템은 MagicOnion 프레임워크를 활용하여 멀티플레이어 던전 크롤러 게임의 실시간 통신을 구현합니다. 이 시스템은 클라이언트-서버 아키텍처를 기반으로 하며, 게임 상태 동기화, 플레이어 액션 전파, 그리고 게임 세션 관리를 담당합니다.

## 기술 선택: MagicOnion

### MagicOnion 소개
MagicOnion은 .NET 환경을 위한 고성능 RPC(Remote Procedure Call) 프레임워크로, Unity 클라이언트와 .NET 서버 간의 원활한 통신을 가능하게 합니다. 다음과 같은 이점이 있습니다:

- **gRPC 기반**: 고성능 바이너리 통신 프로토콜 사용
- **MessagePack 직렬화**: 효율적인 데이터 직렬화/역직렬화
- **강력한 타입 시스템**: 컴파일 타임 타입 검사
- **양방향 스트리밍**: 실시간 양방향 데이터 전송
- **코드 생성**: 공통 인터페이스에서 클라이언트 및 서버 코드 자동 생성

### 주요 통신 패턴
1. **Service 패턴**: 단방향 요청-응답 방식 RPC (로그인, 데이터 요청 등)
2. **Hub 패턴**: 양방향 실시간 통신 (게임 상태 동기화, 이벤트 등)
3. **스트리밍**: 대량 데이터 전송 (초기 맵 데이터, 에셋 등)

## 네트워크 아키텍처

### 전체 아키텍처 개요

```
+----------------+        +-----------------+        +----------------+
|                |        |                 |        |                |
|  Unity Client  | <----> | MagicOnion      | <----> |  Game Server   |
|  (Player 1)    |        | Communication   |        |  (.NET Core)   |
|                |        | Layer           |        |                |
+----------------+        +-----------------+        +----------------+
                                 ^  ^
+----------------+               |  |               +----------------+
|                |               |  |               |                |
|  Unity Client  | <-------------+  +-------------> |   Database     |
|  (Player 2)    |                                  |   (Optional)   |
|                |                                  |                |
+----------------+                                  +----------------+
```

### 클라이언트 측 구조

```csharp
// 네트워크 관리자 싱글톤
public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance => instance;
    
    // MagicOnion 클라이언트
    private GrpcChannelx channel;
    
    // 서비스 클라이언트
    public IGameService GameServiceClient { get; private set; }
    
    // 허브 클라이언트
    public GameHubClient GameHub { get; private set; }
    
    // 네트워크 초기화 및 연결 메서드
    public async Task InitializeNetworkAsync(string serverAddress)
    {
        // 채널 초기화
        channel = GrpcChannelx.ForAddress(serverAddress);
        
        // 서비스 클라이언트 생성
        GameServiceClient = MagicOnionClient.Create<IGameService>(channel);
        
        // 허브 클라이언트 초기화
        GameHub = new GameHubClient();
        await GameHub.ConnectAsync(channel);
    }
    
    // 추가 메서드...
}
```

### 서버 측 구조

```csharp
// Game Service 구현
public class GameService : ServiceBase<IGameService>, IGameService
{
    // 플레이어 인증
    public async UnaryResult<AuthResult> AuthenticateAsync(string userId, string token)
    {
        // 인증 로직...
        return new AuthResult { Success = true };
    }
    
    // 게임 세션 생성
    public async UnaryResult<GameSessionInfo> CreateGameSessionAsync(GameSessionOptions options)
    {
        // 게임 세션 생성 로직...
        return new GameSessionInfo { SessionId = Guid.NewGuid().ToString() };
    }
    
    // 게임 세션 참가
    public async UnaryResult<JoinSessionResult> JoinGameSessionAsync(string sessionId)
    {
        // 세션 참가 로직...
        return new JoinSessionResult { Success = true };
    }
    
    // 추가 메서드...
}

// Game Hub 구현
public class GameHub : StreamingHubBase<IGameHub, IGameHubReceiver>, IGameHub
{
    private IGroup room;
    private Player player;
    private DungeonSessionState sessionState;
    
    // 룸 참가
    public async Task<JoinRoomResult> JoinRoomAsync(string roomId, PlayerInfo playerInfo)
    {
        // 그룹에 추가
        room = await Group.AddAsync(roomId);
        player = new Player { Id = playerInfo.Id, Name = playerInfo.Name };
        
        // 다른 플레이어에게 입장 알림
        await room.BroadcastExceptAsync(Context, (hub) => 
            hub.OnPlayerJoined(player));
        
        return new JoinRoomResult { Success = true };
    }
    
    // 플레이어 위치 업데이트
    public async Task UpdatePositionAsync(Vector3 position, Vector3 rotation)
    {
        player.Position = position;
        player.Rotation = rotation;
        
        // 다른 플레이어에게 위치 업데이트 브로드캐스트
        await room.BroadcastExceptAsync(Context, (hub) => 
            hub.OnPlayerMoved(player.Id, position, rotation));
    }
    
    // 추가 메서드...
}
```

## 핵심 네트워크 시스템

### 세션 관리 시스템

#### 매치메이킹
- **방 생성 및 참가**: 게임 세션 생성 및 참가 기능
- **플레이어 할당**: 적절한 방에 플레이어 배정
- **세션 상태 관리**: 대기, 게임 중, 종료 등 상태 추적

```csharp
// 매치메이킹 서비스 인터페이스
public interface IMatchmakingService
{
    UnaryResult<CreateRoomResponse> CreateRoom(CreateRoomRequest request);
    UnaryResult<JoinRoomResponse> JoinRoom(JoinRoomRequest request);
    UnaryResult<MatchmakingResponse> FindMatch(MatchmakingRequest request);
    UnaryResult<RoomListResponse> GetAvailableRooms();
}
```

#### 플레이어 연결 관리
- **연결 상태 모니터링**: 플레이어 연결 및 연결 해제 감지
- **재연결 처리**: 일시적 연결 끊김 후 재연결 지원
- **세션 종료 처리**: 세션 종료 시 리소스 정리

### 상태 동기화 시스템

#### Transform 동기화
- **위치 및 회전**: 캐릭터 위치 및 회전 동기화
- **보간**: 네트워크 지연을 고려한 부드러운 보간
- **예측 및 재조정**: 클라이언트 측 예측 및 서버 권한 모델

```csharp
// 위치 동기화 메시지
public class TransformSyncMessage
{
    public string EntityId { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public long Timestamp { get; set; }
}
```

#### 상태 동기화
- **캐릭터 상태**: 체력, 마나, 상태 효과 등 동기화
- **게임 오브젝트**: 문, 상자, 함정 등 환경 오브젝트 상태
- **이벤트 기반 동기화**: 상태 변경 이벤트 전파

### 액션 동기화 시스템

#### 입력 처리
- **입력 이벤트 전송**: 플레이어 액션 서버 전송
- **입력 검증**: 서버 측 입력 유효성 검증
- **결과 브로드캐스트**: 검증된 액션 결과 브로드캐스트

```csharp
// 플레이어 액션 이벤트
public enum ActionType
{
    Attack,
    UseSkill,
    UseItem,
    Interact,
    Jump,
    Dodge
}

public class PlayerActionEvent
{
    public string PlayerId { get; set; }
    public ActionType ActionType { get; set; }
    public int TargetId { get; set; } // 필요한 경우
    public int SkillId { get; set; } // 스킬 액션인 경우
    public Vector3 Direction { get; set; } // 방향이 필요한 경우
    public long Timestamp { get; set; }
}
```

#### 전투 동기화
- **공격 액션**: 공격 시작, 히트 판정, 데미지 계산
- **스킬 사용**: 스킬 캐스팅, 효과 적용, 쿨다운
- **피격 반응**: 피격 애니메이션, 넉백, 상태 효과

### 던전 생성 및 관리

#### 던전 데이터 전송
- **초기 로딩**: 던전 구조 데이터 전송
- **스트리밍 방식**: 필요에 따른 청크 기반 전송
- **동적 업데이트**: 던전 변경사항 실시간 동기화

```csharp
// 던전 초기화 메시지
public class DungeonInitData
{
    public string DungeonId { get; set; }
    public DungeonType Type { get; set; }
    public int Seed { get; set; }
    public List<RoomData> Rooms { get; set; }
    public List<CorridorData> Corridors { get; set; }
    public Dictionary<string, EntitySpawnData> Entities { get; set; }
}
```

#### 오브젝트 스폰 및 관리
- **몬스터 스폰**: 서버 제어 몬스터 생성 및 할당
- **아이템 스폰**: 드롭 아이템 생성 및 관리
- **환경 오브젝트**: 함정, 문, 상자 등 관리

## 포트폴리오 구현 계획

1개월의 제한된 시간 내에서 다음과 같은 단계로 구현할 계획입니다:

### 1주차: 기본 MagicOnion 설정
- MagicOnion 프로젝트 구조 설정
- 기본 서비스 및 허브 인터페이스 정의
- 간단한 연결 테스트 환경 구축

### 2주차: 핵심 통신 패턴 구현
- 플레이어 위치 동기화 구현
- 기본 액션 전송 및 처리 구현
- 세션 관리 기초 기능 구현

### 3주차: 게임 상태 동기화
- 던전 초기 데이터 전송 시스템
- 몬스터 AI 및 상태 동기화
- 전투 시스템 통합 및 동기화

### 4주차: 최적화 및 안정화
- 예측 및 재조정 알고리즘 개선
- 네트워크 성능 최적화
- 오류 처리 및 복구 메커니즘 구현

## 기술적 도전과 해결책

### 도전 1: 네트워크 지연 처리
- **문제**: 플레이어 간 지연 차이로 인한 불공정성
- **해결책**: 서버 권한 모델과 클라이언트 측 예측 혼합 사용

### 도전 2: 확장성
- **문제**: 여러 던전 인스턴스 및 플레이어 지원
- **해결책**: 마이크로서비스 설계 및 수평적 확장 구조

### 도전 3: 데이터 사용량 최적화
- **문제**: 제한된 대역폭에서 원활한 게임 경험 제공
- **해결책**: 델타 압축, 관심 영역 필터링, 우선순위 기반 동기화

## 평가 기준

포트폴리오 프로젝트로서 네트워크 시스템의 성공 여부는 다음 기준으로 평가합니다:

1. **안정성**: 네트워크 오류 및 지연에 대한 견고성
2. **응답성**: 지연 시간을 고려한 반응 속도
3. **확장성**: 다수의 클라이언트 지원 가능성
4. **코드 품질**: 명확한 구조와 유지보수 용이성
5. **리소스 효율성**: 대역폭 및 서버 자원 효율적 사용

[메인 계획으로 돌아가기](./MasterPlan.md)
