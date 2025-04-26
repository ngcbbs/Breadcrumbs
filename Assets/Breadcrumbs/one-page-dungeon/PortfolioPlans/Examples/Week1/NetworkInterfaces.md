# MagicOnion 네트워크 인터페이스 예제

MagicOnion을 사용한 네트워크 통신을 위한 기본 인터페이스 정의 예시입니다.

## 게임 서비스 인터페이스

```csharp
// 네트워크 서비스 인터페이스 예시
public interface IGameService : IService<IGameService>
{
    UnaryResult<AuthResponse> AuthenticateAsync(string userId, string password);
    UnaryResult<DungeonData> GetDungeonDataAsync(string dungeonId);
    UnaryResult<bool> SavePlayerProgressAsync(PlayerProgress progress);
}
```

## 게임 허브 인터페이스

```csharp
// 네트워크 허브 인터페이스 예시
public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
{
    Task<JoinResult> JoinGameAsync(JoinRequest request);
    Task UpdatePositionAsync(Vector3 position, Vector3 rotation);
    Task PerformActionAsync(PlayerAction action);
    Task LeaveGameAsync();
}

// 클라이언트 수신 인터페이스
public interface IGameHubReceiver
{
    void OnPlayerJoined(PlayerInfo player);
    void OnPlayerLeft(string playerId);
    void OnPlayerMoved(string playerId, Vector3 position, Vector3 rotation);
    void OnActionPerformed(string playerId, PlayerAction action, ActionResult result);
}
```

## 네트워크 매니저 구현 예시

```csharp
public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance => instance;
    
    // 서비스 및 허브 클라이언트
    private IGameService gameService;
    private IGameHub gameHub;
    
    // 연결 상태
    public bool IsConnected => gameHub != null && gameHub.ConnectionState == ConnectionState.Connected;
    
    // 플레이어 정보
    public string PlayerId { get; private set; }
    public Dictionary<string, PlayerInfo> ConnectedPlayers { get; private set; } = new Dictionary<string, PlayerInfo>();
    
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public async Task<bool> ConnectAsync(string serverAddress, int port)
    {
        try
        {
            // 채널 생성
            var channel = GrpcChannelx.ForAddress($"http://{serverAddress}:{port}");
            
            // 서비스 클라이언트 생성
            gameService = MagicOnionClient.Create<IGameService>(channel);
            
            // 허브 클라이언트 생성 및 연결
            var hubClient = StreamingHubClient.Connect<IGameHub, IGameHubReceiver>(channel, new GameHubReceiver(this));
            gameHub = await hubClient;
            
            Debug.Log($"서버에 연결되었습니다: {serverAddress}:{port}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"서버 연결 실패: {ex.Message}");
            return false;
        }
    }
    
    public async Task<AuthResponse> AuthenticateAsync(string userId, string password)
    {
        try
        {
            var response = await gameService.AuthenticateAsync(userId, password);
            if (response.Success)
            {
                PlayerId = response.PlayerId;
                Debug.Log($"인증 성공: {PlayerId}");
            }
            return response;
        }
        catch (Exception ex)
        {
            Debug.LogError($"인증 실패: {ex.Message}");
            return new AuthResponse { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    public async Task<JoinResult> JoinGameAsync(string dungeonId, string characterClass)
    {
        try
        {
            var request = new JoinRequest
            {
                DungeonId = dungeonId,
                CharacterClass = characterClass
            };
            
            var result = await gameHub.JoinGameAsync(request);
            Debug.Log($"게임 참가 {(result.Success ? "성공" : "실패")}: {result.ErrorMessage ?? ""}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 참가 실패: {ex.Message}");
            return new JoinResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    public async Task UpdatePositionAsync(Vector3 position, Vector3 rotation)
    {
        try
        {
            if (IsConnected)
            {
                await gameHub.UpdatePositionAsync(position, rotation);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"위치 업데이트 실패: {ex.Message}");
        }
    }
    
    public async Task PerformActionAsync(PlayerAction action)
    {
        try
        {
            if (IsConnected)
            {
                await gameHub.PerformActionAsync(action);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"액션 수행 실패: {ex.Message}");
        }
    }
    
    public async Task DisconnectAsync()
    {
        try
        {
            if (gameHub != null)
            {
                await gameHub.LeaveGameAsync();
                await gameHub.DisposeAsync();
                gameHub = null;
            }
            
            Debug.Log("서버 연결 종료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"연결 종료 중 오류: {ex.Message}");
        }
    }
    
    // 게임 허브 수신자 클래스
    private class GameHubReceiver : IGameHubReceiver
    {
        private readonly NetworkManager manager;
        
        public GameHubReceiver(NetworkManager manager)
        {
            this.manager = manager;
        }
        
        public void OnPlayerJoined(PlayerInfo player)
        {
            manager.ConnectedPlayers[player.PlayerId] = player;
            Debug.Log($"플레이어 참가: {player.PlayerName} (ID: {player.PlayerId})");
            
            // 이벤트 발생
            manager.OnPlayerJoinedEvent?.Invoke(player);
        }
        
        public void OnPlayerLeft(string playerId)
        {
            if (manager.ConnectedPlayers.TryGetValue(playerId, out var player))
            {
                manager.ConnectedPlayers.Remove(playerId);
                Debug.Log($"플레이어 퇴장: {player.PlayerName} (ID: {playerId})");
                
                // 이벤트 발생
                manager.OnPlayerLeftEvent?.Invoke(playerId);
            }
        }
        
        public void OnPlayerMoved(string playerId, Vector3 position, Vector3 rotation)
        {
            // 다른 플레이어 위치 업데이트 이벤트 발생
            manager.OnPlayerMovedEvent?.Invoke(playerId, position, rotation);
        }
        
        public void OnActionPerformed(string playerId, PlayerAction action, ActionResult result)
        {
            // 플레이어 액션 수행 이벤트 발생
            manager.OnActionPerformedEvent?.Invoke(playerId, action, result);
        }
    }
    
    // 이벤트 정의
    public event Action<PlayerInfo> OnPlayerJoinedEvent;
    public event Action<string> OnPlayerLeftEvent;
    public event Action<string, Vector3, Vector3> OnPlayerMovedEvent;
    public event Action<string, PlayerAction, ActionResult> OnActionPerformedEvent;
}
```

## 데이터 클래스 예시

```csharp
// 인증 응답 클래스
[MessagePackObject]
public class AuthResponse
{
    [Key(0)]
    public bool Success { get; set; }
    
    [Key(1)]
    public string PlayerId { get; set; }
    
    [Key(2)]
    public string ErrorMessage { get; set; }
}

// 게임 참가 요청 클래스
[MessagePackObject]
public class JoinRequest
{
    [Key(0)]
    public string DungeonId { get; set; }
    
    [Key(1)]
    public string CharacterClass { get; set; }
}

// 게임 참가 응답 클래스
[MessagePackObject]
public class JoinResult
{
    [Key(0)]
    public bool Success { get; set; }
    
    [Key(1)]
    public string ErrorMessage { get; set; }
    
    [Key(2)]
    public List<PlayerInfo> Players { get; set; }
    
    [Key(3)]
    public DungeonData DungeonData { get; set; }
}

// 플레이어 정보 클래스
[MessagePackObject]
public class PlayerInfo
{
    [Key(0)]
    public string PlayerId { get; set; }
    
    [Key(1)]
    public string PlayerName { get; set; }
    
    [Key(2)]
    public string CharacterClass { get; set; }
    
    [Key(3)]
    public Vector3 Position { get; set; }
    
    [Key(4)]
    public Vector3 Rotation { get; set; }
}

// 플레이어 액션 클래스
[MessagePackObject]
public class PlayerAction
{
    [Key(0)]
    public ActionType Type { get; set; }
    
    [Key(1)]
    public string TargetId { get; set; }
    
    [Key(2)]
    public Vector3 Position { get; set; }
    
    [Key(3)]
    public Dictionary<string, object> Parameters { get; set; }
    
    [MessagePackObject]
    public enum ActionType
    {
        [Key(0)]
        Attack,
        
        [Key(1)]
        UseItem,
        
        [Key(2)]
        CastSkill,
        
        [Key(3)]
        Interact
    }
}

// 액션 결과 클래스
[MessagePackObject]
public class ActionResult
{
    [Key(0)]
    public bool Success { get; set; }
    
    [Key(1)]
    public string Message { get; set; }
    
    [Key(2)]
    public Dictionary<string, object> Results { get; set; }
}
```