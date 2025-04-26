# 매치메이킹 시스템 예제 코드 (Part 1)

## 로비 및 세션 관리 시스템

### LobbyManager.cs

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MessagePack;
using UnityEngine;

/// <summary>
/// 로비 및 매치메이킹 시스템을 관리하는 컴포넌트
/// </summary>
public class LobbyManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static LobbyManager Instance { get; private set; }
    
    // 네트워크 서비스 인터페이스
    private ILobbyService _lobbyService;
    
    // 플레이어 정보
    [SerializeField] private string _playerId;
    [SerializeField] private string _playerName;
    
    // 로비 및 세션 상태
    private List<GameSession> _availableSessions = new List<GameSession>();
    private GameSession _currentSession;
    
    // 이벤트 핸들러
    public event Action<List<GameSession>> OnSessionListUpdated;
    public event Action<GameSession> OnJoinedSession;
    public event Action<GameSession> OnLeftSession;
    public event Action<GameSession> OnSessionStarted;
    public event Action<string, string> OnPlayerJoined;
    public event Action<string> OnPlayerLeft;
    
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
        _lobbyService = await LobbyServiceClient.CreateAsync(
            GrpcChannelProvider.GetChannel());
        
        // 로비 연결
        await ConnectToLobbyAsync();
        
        // 세션 목록 업데이트 구독
        ReceiveSessionUpdatesAsync().Forget();
    }
    
    private async UniTask ConnectToLobbyAsync()
    {
        try
        {
            // 로비 연결
            bool success = await _lobbyService.ConnectAsync(_playerId, _playerName);
            
            if (success)
            {
                Debug.Log($"로비 연결 성공: {_playerName} ({_playerId})");
                
                // 초기 세션 목록 요청
                await RefreshSessionListAsync();
            }
            else
            {
                Debug.LogError("로비 연결 실패");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"로비 연결 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 세션 목록 갱신
    /// </summary>
    public async UniTask RefreshSessionListAsync()
    {
        try
        {
            _availableSessions = await _lobbyService.GetSessionListAsync();
            OnSessionListUpdated?.Invoke(_availableSessions);
            
            Debug.Log($"세션 목록 갱신 완료: {_availableSessions.Count}개 세션 발견");
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 목록 갱신 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 새 세션 생성
    /// </summary>
    public async UniTask<bool> CreateSessionAsync(string sessionName, int maxPlayers, 
        Dictionary<string, string> settings)
    {
        try
        {
            GameSession session = new GameSession
            {
                SessionId = Guid.NewGuid().ToString(),
                SessionName = sessionName,
                HostId = _playerId,
                MaxPlayers = maxPlayers,
                Players = new List<SessionPlayer> 
                {
                    new SessionPlayer { PlayerId = _playerId, PlayerName = _playerName, IsReady = true }
                },
                Settings = settings,
                Status = SessionStatus.Waiting
            };
            
            bool success = await _lobbyService.CreateSessionAsync(session);
            
            if (success)
            {
                _currentSession = session;
                OnJoinedSession?.Invoke(_currentSession);
                
                Debug.Log($"세션 생성 성공: {sessionName} ({session.SessionId})");
                return true;
            }
            else
            {
                Debug.LogError("세션 생성 실패");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 생성 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 세션 참가
    /// </summary>
    public async UniTask<bool> JoinSessionAsync(string sessionId)
    {
        try
        {
            bool success = await _lobbyService.JoinSessionAsync(sessionId, _playerId, _playerName);
            
            if (success)
            {
                // 참가한 세션 정보 가져오기
                _currentSession = await _lobbyService.GetSessionAsync(sessionId);
                OnJoinedSession?.Invoke(_currentSession);
                
                Debug.Log($"세션 참가 성공: {_currentSession.SessionName} ({sessionId})");
                return true;
            }
            else
            {
                Debug.LogError($"세션 참가 실패: {sessionId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 참가 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 세션 나가기
    /// </summary>
    public async UniTask<bool> LeaveSessionAsync()
    {
        if (_currentSession == null)
            return false;
            
        try
        {
            bool success = await _lobbyService.LeaveSessionAsync(_currentSession.SessionId, _playerId);
            
            if (success)
            {
                GameSession leftSession = _currentSession;
                _currentSession = null;
                OnLeftSession?.Invoke(leftSession);
                
                Debug.Log($"세션 나가기 성공: {leftSession.SessionName} ({leftSession.SessionId})");
                return true;
            }
            else
            {
                Debug.LogError($"세션 나가기 실패: {_currentSession.SessionId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 나가기 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 준비 상태 토글
    /// </summary>
    public async UniTask<bool> ToggleReadyStateAsync(bool isReady)
    {
        if (_currentSession == null)
            return false;
            
        try
        {
            bool success = await _lobbyService.SetReadyStateAsync(_currentSession.SessionId, _playerId, isReady);
            
            if (success)
            {
                Debug.Log($"준비 상태 변경 성공: {isReady}");
                return true;
            }
            else
            {
                Debug.LogError("준비 상태 변경 실패");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"준비 상태 변경 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 세션 시작
    /// </summary>
    public async UniTask<bool> StartSessionAsync()
    {
        if (_currentSession == null || _currentSession.HostId != _playerId)
            return false;
            
        try
        {
            bool success = await _lobbyService.StartSessionAsync(_currentSession.SessionId);
            
            if (success)
            {
                Debug.Log($"세션 시작 성공: {_currentSession.SessionName} ({_currentSession.SessionId})");
                
                // 업데이트된 세션 정보 가져오기
                _currentSession = await _lobbyService.GetSessionAsync(_currentSession.SessionId);
                OnSessionStarted?.Invoke(_currentSession);
                
                return true;
            }
            else
            {
                Debug.LogError("세션 시작 실패");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 시작 오류: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 세션 업데이트 수신
    /// </summary>
    private async UniTask ReceiveSessionUpdatesAsync()
    {
        try
        {
            // 세션 업데이트 구독
            await foreach (var update in _lobbyService.OnSessionsUpdatedAsync())
            {
                // 세션 목록 업데이트
                _availableSessions = update.Sessions;
                OnSessionListUpdated?.Invoke(_availableSessions);
                
                // 현재 세션 업데이트
                if (_currentSession != null)
                {
                    foreach (var session in _availableSessions)
                    {
                        if (session.SessionId == _currentSession.SessionId)
                        {
                            ProcessSessionChanges(_currentSession, session);
                            _currentSession = session;
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"세션 업데이트 수신 오류: {ex.Message}");
            // 재연결 로직
            await UniTask.Delay(1000);
            ReceiveSessionUpdatesAsync().Forget();
        }
    }
    
    /// <summary>
    /// 세션 변경 처리
    /// </summary>
    private void ProcessSessionChanges(GameSession oldSession, GameSession newSession)
    {
        // 세션 상태 변경 감지
        if (oldSession.Status != newSession.Status && newSession.Status == SessionStatus.InProgress)
        {
            OnSessionStarted?.Invoke(newSession);
        }
        
        // 플레이어 변경 감지
        Dictionary<string, SessionPlayer> oldPlayers = new Dictionary<string, SessionPlayer>();
        foreach (var player in oldSession.Players)
        {
            oldPlayers[player.PlayerId] = player;
        }
        
        // 새로 참가한 플레이어 감지
        foreach (var player in newSession.Players)
        {
            if (!oldPlayers.ContainsKey(player.PlayerId))
            {
                OnPlayerJoined?.Invoke(player.PlayerId, player.PlayerName);
            }
            oldPlayers.Remove(player.PlayerId);
        }
        
        // 나간 플레이어 감지
        foreach (var leftPlayer in oldPlayers.Keys)
        {
            OnPlayerLeft?.Invoke(leftPlayer);
        }
    }
    
    public GameSession GetCurrentSession()
    {
        return _currentSession;
    }
    
    public List<GameSession> GetAvailableSessions()
    {
        return _availableSessions;
    }
    
    public string GetPlayerId()
    {
        return _playerId;
    }
    
    private void OnDestroy()
    {
        // 로비에서 연결 해제
        if (_lobbyService != null && _playerId != null)
        {
            _lobbyService.DisconnectAsync(_playerId).Forget();
        }
    }
}
```

### ILobbyService.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicOnion;
using MessagePack;

/// <summary>
/// 로비 서비스 인터페이스
/// </summary>
public interface ILobbyService : IService<ILobbyService>
{
    // 로비 연결
    Task<bool> ConnectAsync(string playerId, string playerName);
    
    // 로비 연결 해제
    Task<bool> DisconnectAsync(string playerId);
    
    // 세션 목록 가져오기
    Task<List<GameSession>> GetSessionListAsync();
    
    // 세션 생성
    Task<bool> CreateSessionAsync(GameSession session);
    
    // 세션 정보 가져오기
    Task<GameSession> GetSessionAsync(string sessionId);
    
    // 세션 참가
    Task<bool> JoinSessionAsync(string sessionId, string playerId, string playerName);
    
    // 세션 나가기
    Task<bool> LeaveSessionAsync(string sessionId, string playerId);
    
    // 준비 상태 설정
    Task<bool> SetReadyStateAsync(string sessionId, string playerId, bool isReady);
    
    // 세션 시작
    Task<bool> StartSessionAsync(string sessionId);
    
    // 세션 업데이트 구독
    IAsyncEnumerable<SessionUpdateNotification> OnSessionsUpdatedAsync();
}

/// <summary>
/// 게임 세션 정보
/// </summary>
[MessagePackObject]
public class GameSession
{
    [Key(0)]
    public string SessionId { get; set; }
    
    [Key(1)]
    public string SessionName { get; set; }
    
    [Key(2)]
    public string HostId { get; set; }
    
    [Key(3)]
    public int MaxPlayers { get; set; }
    
    [Key(4)]
    public List<SessionPlayer> Players { get; set; }
    
    [Key(5)]
    public Dictionary<string, string> Settings { get; set; }
    
    [Key(6)]
    public SessionStatus Status { get; set; }
    
    [Key(7)]
    public string DungeonId { get; set; }
}

/// <summary>
/// 세션 플레이어 정보
/// </summary>
[MessagePackObject]
public class SessionPlayer
{
    [Key(0)]
    public string PlayerId { get; set; }
    
    [Key(1)]
    public string PlayerName { get; set; }
    
    [Key(2)]
    public bool IsReady { get; set; }
}

/// <summary>
/// 세션 상태
/// </summary>
public enum SessionStatus
{
    Waiting,
    Starting,
    InProgress,
    Completed
}

/// <summary>
/// 세션 업데이트 알림
/// </summary>
[MessagePackObject]
public class SessionUpdateNotification
{
    [Key(0)]
    public List<GameSession> Sessions { get; set; }
}
```

[다음: MatchmakingSystem_part2.md]