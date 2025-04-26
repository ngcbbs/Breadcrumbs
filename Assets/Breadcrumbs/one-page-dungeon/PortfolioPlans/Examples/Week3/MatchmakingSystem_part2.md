# 매치메이킹 시스템 예제 코드 (Part 2)

## 세션 관리 시스템

### SessionManager.cs

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 세션 관리 및 던전 생성을 담당하는 컴포넌트
/// </summary>
public class SessionManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SessionManager Instance { get; private set; }
    
    // 로비 매니저
    [SerializeField] private LobbyManager _lobbyManager;
    
    // 네트워크 서비스 인터페이스
    private ISessionService _sessionService;
    
    // 세션 및 던전 상태
    private GameSession _activeSession;
    private string _dungeonId;
    private bool _isHost;
    
    // 이벤트 핸들러
    public event Action<DungeonParams> OnDungeonCreated;
    public event Action OnSessionEnded;
    public event Action<List<string>> OnPlayerConnected;
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private async void Start()
    {
        // MagicOnion 서비스 연결
        _sessionService = await SessionServiceClient.CreateAsync(
            GrpcChannelProvider.GetChannel());
        
        // 로비 이벤트 구독
        _lobbyManager.OnSessionStarted += OnSessionStartedHandler;
    }
    
    /// <summary>
    /// 세션 시작 이벤트 핸들러
    /// </summary>
    private void OnSessionStartedHandler(GameSession session)
    {
        _activeSession = session;
        _isHost = session.HostId == _lobbyManager.GetPlayerId();
        
        // 호스트인 경우 던전 생성
        if (_isHost)
        {
            CreateDungeonAsync().Forget();
        }
        else
        {
            // 던전 정보 요청
            RequestDungeonInfoAsync().Forget();
        }
    }
    
    /// <summary>
    /// 던전 생성
    /// </summary>
    private async UniTask CreateDungeonAsync()
    {
        try
        {
            // 던전 ID 생성
            _dungeonId = $"dungeon_{_activeSession.SessionId}";
            
            // 던전 시드 생성
            int seed = UnityEngine.Random.Range(1, 99999);
            
            // 던전 파라미터 생성
            DungeonParams dungeonParams = new DungeonParams
            {
                DungeonId = _dungeonId,
                Seed = seed,
                Difficulty = 1,
                Size = 1, // 기본 크기
                Theme = "dungeon" // 기본 테마
            };
            
            // 던전 생성 정보를 서버에 등록
            bool success = await _sessionService.RegisterDungeonAsync(_activeSession.SessionId, dungeonParams);
            
            if (success)
            {
                Debug.Log($"던전 생성 성공: {_dungeonId}, 시드: {seed}");
                
                // 던전 씬 로딩
                LoadDungeonScene(dungeonParams);
            }
            else
            {
                Debug.LogError("던전 생성 실패");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"던전 생성 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 던전 정보 요청
    /// </summary>
    private async UniTask RequestDungeonInfoAsync()
    {
        try
        {
            // 던전 파라미터 요청
            DungeonParams dungeonParams = await _sessionService.GetDungeonParamsAsync(_activeSession.SessionId);
            
            if (dungeonParams != null)
            {
                _dungeonId = dungeonParams.DungeonId;
                Debug.Log($"던전 정보 수신 성공: {_dungeonId}, 시드: {dungeonParams.Seed}");
                
                // 던전 씬 로딩
                LoadDungeonScene(dungeonParams);
            }
            else
            {
                Debug.LogError("던전 정보 수신 실패");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"던전 정보 요청 오류: {ex.Message}");
            
            // 잠시 대기 후 재시도
            await UniTask.Delay(1000);
            RequestDungeonInfoAsync().Forget();
        }
    }
    
    /// <summary>
    /// 던전 씬 로딩
    /// </summary>
    private void LoadDungeonScene(DungeonParams dungeonParams)
    {
        // 던전 생성 이벤트 발생
        OnDungeonCreated?.Invoke(dungeonParams);
        
        // 던전 씬 로드
        SceneManager.LoadScene("DungeonScene"); // 실제 씬 이름으로 변경 필요
    }
    
    /// <summary>
    /// 세션에 참가
    /// </summary>
    public async UniTask<bool> JoinSessionAsync(string sessionId, string playerId, string playerName)
    {
        try
        {
            bool success = await _sessionService.JoinSessionGameAsync(sessionId, playerId, playerName);
            
            if (success)
            {
                Debug.Log($"세션 게임 참가 성공: {sessionId}");
                
                // 현재 세션 플레이어 정보 요청
                List<string> connectedPlayers = await _sessionService.GetConnectedPlayersAsync(sessionId);
                OnPlayerConnected?.Invoke(connectedPlayers);
                
                return true;
            }
            else
            {
                Debug.LogError($"세션 게임 참가 실패: {sessionId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 게임 참가 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 세션 종료
    /// </summary>
    public async UniTask<bool> EndSessionAsync()
    {
        if (_activeSession == null)
            return false;
            
        try
        {
            bool success = await _sessionService.EndSessionAsync(_activeSession.SessionId);
            
            if (success)
            {
                Debug.Log($"세션 종료 성공: {_activeSession.SessionId}");
                _activeSession = null;
                _dungeonId = null;
                OnSessionEnded?.Invoke();
                
                // 로비 씬으로 돌아가기
                SceneManager.LoadScene("LobbyScene"); // 실제 씬 이름으로 변경 필요
                
                return true;
            }
            else
            {
                Debug.LogError($"세션 종료 실패: {_activeSession.SessionId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 종료 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 세션 나가기
    /// </summary>
    public async UniTask<bool> LeaveSessionGameAsync()
    {
        if (_activeSession == null)
            return false;
            
        try
        {
            bool success = await _sessionService.LeaveSessionGameAsync(_activeSession.SessionId, _lobbyManager.GetPlayerId());
            
            if (success)
            {
                Debug.Log($"세션 게임 나가기 성공: {_activeSession.SessionId}");
                _activeSession = null;
                _dungeonId = null;
                OnSessionEnded?.Invoke();
                
                // 로비 씬으로 돌아가기
                SceneManager.LoadScene("LobbyScene"); // 실제 씬 이름으로 변경 필요
                
                return true;
            }
            else
            {
                Debug.LogError($"세션 게임 나가기 실패: {_activeSession.SessionId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 게임 나가기 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 던전 ID 반환
    /// </summary>
    public string GetDungeonId()
    {
        return _dungeonId;
    }
    
    /// <summary>
    /// 호스트 여부 반환
    /// </summary>
    public bool IsHost()
    {
        return _isHost;
    }
    
    /// <summary>
    /// 활성 세션 반환
    /// </summary>
    public GameSession GetActiveSession()
    {
        return _activeSession;
    }
}

/// <summary>
/// 던전 파라미터
/// </summary>
[MessagePackObject]
public class DungeonParams
{
    [Key(0)]
    public string DungeonId { get; set; }
    
    [Key(1)]
    public int Seed { get; set; }
    
    [Key(2)]
    public int Difficulty { get; set; }
    
    [Key(3)]
    public int Size { get; set; }
    
    [Key(4)]
    public string Theme { get; set; }
}
```

### ISessionService.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicOnion;

/// <summary>
/// 세션 서비스 인터페이스
/// </summary>
public interface ISessionService : IService<ISessionService>
{
    // 던전 등록
    Task<bool> RegisterDungeonAsync(string sessionId, DungeonParams dungeonParams);
    
    // 던전 파라미터 가져오기
    Task<DungeonParams> GetDungeonParamsAsync(string sessionId);
    
    // 세션 게임 참가
    Task<bool> JoinSessionGameAsync(string sessionId, string playerId, string playerName);
    
    // 세션 게임 나가기
    Task<bool> LeaveSessionGameAsync(string sessionId, string playerId);
    
    // 세션 종료
    Task<bool> EndSessionAsync(string sessionId);
    
    // 연결된 플레이어 목록 가져오기
    Task<List<string>> GetConnectedPlayersAsync(string sessionId);
}
```

## 네트워크 재연결 및 오류 처리 시스템

### NetworkReconnectionHandler.cs

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 네트워크 재연결 및 오류 처리를 담당하는 컴포넌트
/// </summary>
public class NetworkReconnectionHandler : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static NetworkReconnectionHandler Instance { get; private set; }
    
    // 네트워크 상태 및 설정
    private bool _isConnected = false;
    private bool _isReconnecting = false;
    
    [SerializeField] private float _initialReconnectDelay = 1f;
    [SerializeField] private float _maxReconnectDelay = 30f;
    [SerializeField] private int _maxReconnectAttempts = 10;
    
    // 지수 백오프 지연 시간
    private float _currentReconnectDelay;
    private int _reconnectAttempts = 0;
    
    // 이벤트 핸들러
    public event Action OnConnectionLost;
    public event Action OnReconnectStart;
    public event Action OnReconnectSuccess;
    public event Action OnReconnectFailed;
    
    // 재연결 취소 토큰
    private CancellationTokenSource _reconnectCTS;
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _currentReconnectDelay = _initialReconnectDelay;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    /// <summary>
    /// 연결 상태 설정
    /// </summary>
    public void SetConnected(bool connected)
    {
        // 연결 상태 변경 감지
        if (_isConnected && !connected)
        {
            // 연결 끊김 처리
            HandleConnectionLost();
        }
        else if (!_isConnected && connected)
        {
            // 연결 성공 처리
            HandleConnectionRestored();
        }
        
        _isConnected = connected;
    }
    
    /// <summary>
    /// 연결 끊김 처리
    /// </summary>
    private void HandleConnectionLost()
    {
        Debug.LogWarning("네트워크 연결이 끊겼습니다.");
        OnConnectionLost?.Invoke();
        
        // 자동 재연결 시작
        StartReconnection().Forget();
    }
    
    /// <summary>
    /// 연결 복구 처리
    /// </summary>
    private void HandleConnectionRestored()
    {
        if (_isReconnecting)
        {
            Debug.Log("네트워크 연결이 복구되었습니다.");
            
            // 재연결 취소
            CancelReconnection();
            
            // 재연결 성공 이벤트 발생
            OnReconnectSuccess?.Invoke();
            
            // 상태 초기화
            _isReconnecting = false;
            _reconnectAttempts = 0;
            _currentReconnectDelay = _initialReconnectDelay;
        }
        else
        {
            Debug.Log("네트워크 연결이 설정되었습니다.");
        }
    }
    
    /// <summary>
    /// 재연결 시작
    /// </summary>
    private async UniTask StartReconnection()
    {
        if (_isReconnecting)
            return;
            
        _isReconnecting = true;
        _reconnectAttempts = 0;
        _currentReconnectDelay = _initialReconnectDelay;
        
        // 재연결 취소 토큰 생성
        _reconnectCTS = new CancellationTokenSource();
        
        // 재연결 시작 이벤트 발생
        OnReconnectStart?.Invoke();
        
        Debug.Log("자동 재연결을 시도합니다...");
        
        while (_isReconnecting && _reconnectAttempts < _maxReconnectAttempts)
        {
            try
            {
                // 재연결 시도 전 대기
                await UniTask.Delay((int)(_currentReconnectDelay * 1000), cancellationToken: _reconnectCTS.Token);
                
                if (_reconnectCTS.IsCancellationRequested)
                    break;
                
                // 재연결 시도
                bool success = await TryReconnect();
                
                if (success)
                {
                    // 재연결 성공
                    SetConnected(true);
                    return;
                }
                else
                {
                    // 재연결 실패, 다음 시도 준비
                    _reconnectAttempts++;
                    
                    // 지수 백오프 적용 (다음 시도까지 대기 시간 증가)
                    _currentReconnectDelay = Math.Min(_currentReconnectDelay * 1.5f, _maxReconnectDelay);
                    
                    Debug.LogWarning($"재연결 시도 실패: {_reconnectAttempts}/{_maxReconnectAttempts}, " +
                                    $"{_currentReconnectDelay}초 후 재시도합니다.");
                }
            }
            catch (OperationCanceledException)
            {
                // 재연결 취소됨
                Debug.Log("재연결 시도가 취소되었습니다.");
                break;
            }
            catch (Exception ex)
            {
                // 재연결 중 오류 발생
                Debug.LogError($"재연결 시도 중 오류 발생: {ex.Message}");
                _reconnectAttempts++;
            }
        }
        
        // 최대 시도 횟수 초과 또는 재연결 취소됨
        if (_isReconnecting && !_reconnectCTS.IsCancellationRequested)
        {
            _isReconnecting = false;
            Debug.LogError("최대 재연결 시도 횟수를 초과했습니다.");
            OnReconnectFailed?.Invoke();
            
            // 재연결 실패 후 처리 로직 (예: 로비로 돌아가기)
            HandleReconnectFailure();
        }
    }
    
    /// <summary>
    /// 재연결 시도
    /// </summary>
    private async UniTask<bool> TryReconnect()
    {
        // 이 코드는 실제 서버 연결 시도 로직으로 대체해야 함
        try
        {
            // MagicOnion 연결 재설정 로직
            var channel = GrpcChannelProvider.GetChannel(true); // 채널 강제 갱신
            
            // 기본 서비스 연결 테스트
            var pingService = await PingServiceClient.CreateAsync(channel);
            bool pingSuccess = await pingService.PingAsync();
            
            return pingSuccess;
        }
        catch (Exception ex)
        {
            Debug.LogError($"재연결 시도 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 재연결 취소
    /// </summary>
    private void CancelReconnection()
    {
        if (_isReconnecting && _reconnectCTS != null && !_reconnectCTS.IsCancellationRequested)
        {
            _reconnectCTS.Cancel();
            _reconnectCTS.Dispose();
            _reconnectCTS = null;
        }
    }
    
    /// <summary>
    /// 재연결 실패 처리
    /// </summary>
    private void HandleReconnectFailure()
    {
        // 로비 씬으로 돌아가기 또는 다른 조치
        // 예: SceneManager.LoadScene("LobbyScene");
        
        // 또는 사용자에게 재연결 옵션 제공
        // 예: ShowReconnectDialog();
    }
    
    private void OnDestroy()
    {
        CancelReconnection();
    }
}

/// <summary>
/// MagicOnion 채널 제공자
/// </summary>
public static class GrpcChannelProvider
{
    private static Grpc.Core.Channel _channel;
    
    /// <summary>
    /// gRPC 채널 가져오기
    /// </summary>
    public static Grpc.Core.Channel GetChannel(bool forceRenew = false)
    {
        if (_channel == null || forceRenew)
        {
            if (_channel != null)
            {
                _channel.ShutdownAsync().Forget();
            }
            
            // 실제 환경에서는 서버 주소로 변경 필요
            _channel = new Grpc.Core.Channel("localhost", 12345, Grpc.Core.ChannelCredentials.Insecure);
        }
        
        return _channel;
    }
}

/// <summary>
/// 핑 서비스 인터페이스
/// </summary>
public interface IPingService : IService<IPingService>
{
    Task<bool> PingAsync();
}
```

## 메타 정보 및 핵심 구현 사항

### 매치메이킹 시스템 핵심 요소

1. **로비 관리**: 플레이어 연결 및 세션 목록 관리
2. **세션 생성 및 참가**: 게임 세션 생성, 참가 및 설정 관리
3. **던전 생성 및 공유**: 호스트가 던전을 생성하고 클라이언트에게 전달
4. **연결 관리 및 재연결**: 네트워크 오류 처리 및 자동 재연결 시스템

### 설계 철학

1. **확장성**: 모듈식 설계로 다양한 게임 모드 및 환경에 대응
2. **신뢰성**: 네트워크 오류에 대비한 강건한 오류 처리 및 재연결 메커니즘
3. **사용자 경험**: 매치메이킹 과정에서 플레이어에게 명확한 피드백 제공
4. **효율성**: 적절한 동기화 및 데이터 전송 최적화

### 향후 개선 가능 영역

1. **매치메이킹 알고리즘**: 스킬 기반 매칭, 지역 기반 매칭 등 고급 알고리즘 도입
2. **세션 유지 및 복구**: 호스트 마이그레이션, 상태 지속성 향상
3. **보안 강화**: 세션 토큰 및 인증 시스템 도입
4. **성능 최적화**: 대규모 사용자를 위한 분산 매치메이킹 시스템 설계

[이전: MatchmakingSystem_part1.md]