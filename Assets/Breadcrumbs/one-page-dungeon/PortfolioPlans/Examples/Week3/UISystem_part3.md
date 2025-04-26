# UI 시스템 예제 코드 (Part 3)

## 네트워크 UI 및 피드백 시스템

### RoomLobbyPanel.cs

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방 로비 UI 패널
/// </summary>
public class RoomLobbyPanel : UIPanel
{
    [SerializeField] private Text _roomNameText;
    [SerializeField] private Text _playerCountText;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _leaveButton;
    [SerializeField] private Transform _playerListContent;
    [SerializeField] private PlayerListItemUI _playerItemPrefab;
    
    private List<PlayerListItemUI> _playerItems = new List<PlayerListItemUI>();
    private LobbyManager _lobbyManager;
    private bool _isReady = false;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 버튼 이벤트 등록
        if (_startButton != null)
            _startButton.onClick.AddListener(HandleStartClicked);
            
        if (_readyButton != null)
            _readyButton.onClick.AddListener(HandleReadyClicked);
            
        if (_leaveButton != null)
            _leaveButton.onClick.AddListener(HandleLeaveClicked);
    }
    
    private void Start()
    {
        // 로비 매니저 참조 가져오기
        _lobbyManager = LobbyManager.Instance;
        
        if (_lobbyManager != null)
        {
            // 이벤트 구독
            _lobbyManager.OnPlayerJoined += HandlePlayerJoined;
            _lobbyManager.OnPlayerLeft += HandlePlayerLeft;
            _lobbyManager.OnLeftSession += HandleLeftSession;
            _lobbyManager.OnSessionStarted += HandleSessionStarted;
        }
    }
    
    public override void Show()
    {
        base.Show();
        
        // 방 정보 업데이트
        UpdateRoomInfo();
        
        // 플레이어 목록 갱신
        RefreshPlayerList();
    }
    
    /// <summary>
    /// 방 정보 업데이트
    /// </summary>
    private void UpdateRoomInfo()
    {
        if (_lobbyManager == null)
            return;
            
        GameSession session = _lobbyManager.GetCurrentSession();
        
        if (session == null)
            return;
            
        // 방 이름 업데이트
        if (_roomNameText != null)
            _roomNameText.text = session.SessionName;
            
        // 플레이어 수 업데이트
        if (_playerCountText != null)
            _playerCountText.text = $"플레이어: {session.Players.Count}/{session.MaxPlayers}";
            
        // 호스트 여부에 따라 버튼 설정
        bool isHost = session.HostId == _lobbyManager.GetPlayerId();
        
        if (_startButton != null)
            _startButton.gameObject.SetActive(isHost);
            
        if (_readyButton != null)
            _readyButton.gameObject.SetActive(!isHost);
            
        // 게임 시작 버튼 활성화 조건 검사 (모든 플레이어가 준비 상태)
        if (_startButton != null && isHost)
        {
            bool allPlayersReady = true;
            
            foreach (var player in session.Players)
            {
                if (player.PlayerId != session.HostId && !player.IsReady)
                {
                    allPlayersReady = false;
                    break;
                }
            }
            
            _startButton.interactable = allPlayersReady && session.Players.Count > 1;
        }
    }
    
    /// <summary>
    /// 플레이어 목록 갱신
    /// </summary>
    private void RefreshPlayerList()
    {
        // 기존 아이템 제거
        ClearPlayerItems();
        
        if (_lobbyManager == null || _playerItemPrefab == null || _playerListContent == null)
            return;
            
        GameSession session = _lobbyManager.GetCurrentSession();
        
        if (session == null || session.Players == null)
            return;
            
        // 플레이어 아이템 생성
        foreach (var player in session.Players)
        {
            CreatePlayerItem(player, session.HostId);
        }
    }
    
    /// <summary>
    /// 플레이어 아이템 생성
    /// </summary>
    private void CreatePlayerItem(SessionPlayer player, string hostId)
    {
        if (_playerItemPrefab == null || _playerListContent == null)
            return;
            
        // 아이템 생성
        PlayerListItemUI item = Instantiate(_playerItemPrefab, _playerListContent);
        
        // 플레이어 정보 설정 (호스트 여부, 준비 상태 포함)
        bool isHost = player.PlayerId == hostId;
        item.Initialize(player.PlayerName, isHost, player.IsReady);
        
        // 목록에 추가
        _playerItems.Add(item);
    }
    
    /// <summary>
    /// 플레이어 아이템 정리
    /// </summary>
    private void ClearPlayerItems()
    {
        foreach (var item in _playerItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        
        _playerItems.Clear();
    }
    
    // 이벤트 핸들러
    private void HandlePlayerJoined(string playerId, string playerName)
    {
        RefreshPlayerList();
        UpdateRoomInfo();
        
        // 플레이어 참가 메시지 표시
        UIManager.Instance?.ShowStatusMessage($"{playerName}님이 방에 참가했습니다.", MessageType.Info);
    }
    
    private void HandlePlayerLeft(string playerId)
    {
        RefreshPlayerList();
        UpdateRoomInfo();
        
        // 플레이어 퇴장 메시지 표시 (이름을 알 수 없는 경우)
        UIManager.Instance?.ShowStatusMessage("플레이어가 방을 나갔습니다.", MessageType.Info);
    }
    
    private void HandleLeftSession(GameSession session)
    {
        // 로비로 돌아가기
        UIManager.Instance?.ShowPanel(UIType.Lobby);
    }
    
    private void HandleSessionStarted(GameSession session)
    {
        // 게임 시작 메시지 표시
        UIManager.Instance?.ShowStatusMessage("게임이 시작됩니다...", MessageType.Success);
        
        // 로딩 화면으로 전환
        UIManager.Instance?.ShowPanel(UIType.Loading);
    }
    
    // 버튼 이벤트 처리
    private void HandleStartClicked()
    {
        if (_lobbyManager == null)
            return;
            
        // 로딩 표시
        UIManager.Instance?.SetLoadingOverlay(true);
        
        // 게임 세션 시작
        _lobbyManager.StartSessionAsync().ContinueWith(success => {
            UIManager.Instance?.SetLoadingOverlay(false);
            
            if (!success)
            {
                // 에러 표시
                UIManager.Instance?.ShowStatusMessage("게임 시작에 실패했습니다.", MessageType.Error);
            }
        }).Forget();
    }
    
    private void HandleReadyClicked()
    {
        if (_lobbyManager == null)
            return;
            
        // 준비 상태 토글
        _isReady = !_isReady;
        
        // 준비 버튼 텍스트 변경
        if (_readyButton != null)
        {
            Text buttonText = _readyButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = _isReady ? "준비 취소" : "준비";
            }
        }
        
        // 서버에 준비 상태 전송
        _lobbyManager.ToggleReadyStateAsync(_isReady).Forget();
    }
    
    private void HandleLeaveClicked()
    {
        if (_lobbyManager == null)
            return;
            
        // 로딩 표시
        UIManager.Instance?.SetLoadingOverlay(true);
        
        // 방 나가기
        _lobbyManager.LeaveSessionAsync().ContinueWith(success => {
            UIManager.Instance?.SetLoadingOverlay(false);
            
            if (!success)
            {
                // 에러 표시
                UIManager.Instance?.ShowStatusMessage("방을 나가는데 실패했습니다.", MessageType.Error);
            }
        }).Forget();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // 이벤트 해제
        if (_lobbyManager != null)
        {
            _lobbyManager.OnPlayerJoined -= HandlePlayerJoined;
            _lobbyManager.OnPlayerLeft -= HandlePlayerLeft;
            _lobbyManager.OnLeftSession -= HandleLeftSession;
            _lobbyManager.OnSessionStarted -= HandleSessionStarted;
        }
        
        // 버튼 이벤트 해제
        if (_startButton != null)
            _startButton.onClick.RemoveListener(HandleStartClicked);
            
        if (_readyButton != null)
            _readyButton.onClick.RemoveListener(HandleReadyClicked);
            
        if (_leaveButton != null)
            _leaveButton.onClick.RemoveListener(HandleLeaveClicked);
    }
}
```

### PlayerListItemUI.cs

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 목록 아이템 UI
/// </summary>
public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private GameObject _hostIcon;
    [SerializeField] private GameObject _readyIcon;
    
    /// <summary>
    /// 아이템 초기화
    /// </summary>
    public void Initialize(string playerName, bool isHost, bool isReady)
    {
        // 이름 설정
        if (_nameText != null)
            _nameText.text = playerName;
            
        // 호스트 아이콘 설정
        if (_hostIcon != null)
            _hostIcon.SetActive(isHost);
            
        // 준비 아이콘 설정 (호스트는 항상 준비 상태)
        if (_readyIcon != null)
            _readyIcon.SetActive(isHost || isReady);
    }
}
```

### NetworkStatusUI.cs

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 네트워크 상태 표시 UI
/// </summary>
public class NetworkStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject _statusPanel;
    [SerializeField] private Text _connectionStatusText;
    [SerializeField] private Text _pingText;
    [SerializeField] private Image _signalIcon;
    [SerializeField] private Sprite[] _signalSprites; // 0 = 없음, 1 = 약함, 2 = 중간, 3 = 강함
    
    private NetworkReconnectionHandler _reconnectionHandler;
    private PingMonitor _pingMonitor;
    
    private void Start()
    {
        // 컴포넌트 참조 가져오기
        _reconnectionHandler = NetworkReconnectionHandler.Instance;
        _pingMonitor = FindObjectOfType<PingMonitor>();
        
        // 초기 상태 설정
        if (_statusPanel != null)
            _statusPanel.SetActive(false);
            
        // 이벤트 구독
        if (_reconnectionHandler != null)
        {
            _reconnectionHandler.OnConnectionLost += HandleConnectionLost;
            _reconnectionHandler.OnReconnectStart += HandleReconnectStart;
            _reconnectionHandler.OnReconnectSuccess += HandleReconnectSuccess;
            _reconnectionHandler.OnReconnectFailed += HandleReconnectFailed;
        }
        
        if (_pingMonitor != null)
        {
            _pingMonitor.OnPingUpdated += UpdatePingDisplay;
        }
    }
    
    private void Update()
    {
        // 패널 토글 핫키 (디버그용)
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ToggleStatusPanel();
        }
    }
    
    /// <summary>
    /// 상태 패널 토글
    /// </summary>
    public void ToggleStatusPanel()
    {
        if (_statusPanel != null)
            _statusPanel.SetActive(!_statusPanel.activeSelf);
    }
    
    /// <summary>
    /// 연결 끊김 처리
    /// </summary>
    private void HandleConnectionLost()
    {
        UpdateConnectionStatus("연결 끊김", Color.red);
        UpdateSignalIcon(0);
        
        // 상태 패널 표시
        if (_statusPanel != null)
            _statusPanel.SetActive(true);
            
        // 연결 끊김 알림
        UIManager.Instance?.ShowStatusMessage("서버 연결이 끊겼습니다.", MessageType.Error);
    }
    
    /// <summary>
    /// 재연결 시작 처리
    /// </summary>
    private void HandleReconnectStart()
    {
        UpdateConnectionStatus("재연결 중...", Color.yellow);
        UpdateSignalIcon(1);
    }
    
    /// <summary>
    /// 재연결 성공 처리
    /// </summary>
    private void HandleReconnectSuccess()
    {
        UpdateConnectionStatus("연결됨", Color.green);
        UpdateSignalIcon(3);
        
        // 잠시 후 상태 패널 숨기기
        Invoke(nameof(HideStatusPanel), 3f);
        
        // 재연결 성공 알림
        UIManager.Instance?.ShowStatusMessage("서버에 다시 연결되었습니다.", MessageType.Success);
    }
    
    /// <summary>
    /// 재연결 실패 처리
    /// </summary>
    private void HandleReconnectFailed()
    {
        UpdateConnectionStatus("재연결 실패", Color.red);
        UpdateSignalIcon(0);
        
        // 재연결 실패 알림
        UIManager.Instance?.ShowStatusMessage("서버 재연결에 실패했습니다.", MessageType.Error);
    }
    
    /// <summary>
    /// 핑 업데이트 처리
    /// </summary>
    private void UpdatePingDisplay(float pingMs)
    {
        if (_pingText != null)
        {
            _pingText.text = $"Ping: {Mathf.RoundToInt(pingMs)}ms";
            
            // 핑에 따른 색상 설정
            if (pingMs < 50)
                _pingText.color = Color.green;
            else if (pingMs < 100)
                _pingText.color = Color.yellow;
            else if (pingMs < 200)
                _pingText.color = new Color(1f, 0.5f, 0f); // 주황색
            else
                _pingText.color = Color.red;
        }
        
        // 핑에 따른 신호 아이콘 업데이트
        if (pingMs < 50)
            UpdateSignalIcon(3);
        else if (pingMs < 100)
            UpdateSignalIcon(2);
        else if (pingMs < 200)
            UpdateSignalIcon(1);
        else
            UpdateSignalIcon(0);
    }
    
    /// <summary>
    /// 연결 상태 텍스트 업데이트
    /// </summary>
    private void UpdateConnectionStatus(string status, Color color)
    {
        if (_connectionStatusText != null)
        {
            _connectionStatusText.text = status;
            _connectionStatusText.color = color;
        }
    }
    
    /// <summary>
    /// 신호 아이콘 업데이트
    /// </summary>
    private void UpdateSignalIcon(int strength)
    {
        if (_signalIcon != null && _signalSprites != null && strength >= 0 && strength < _signalSprites.Length)
        {
            _signalIcon.sprite = _signalSprites[strength];
        }
    }
    
    /// <summary>
    /// 상태 패널 숨기기
    /// </summary>
    private void HideStatusPanel()
    {
        if (_statusPanel != null)
            _statusPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // 이벤트 해제
        if (_reconnectionHandler != null)
        {
            _reconnectionHandler.OnConnectionLost -= HandleConnectionLost;
            _reconnectionHandler.OnReconnectStart -= HandleReconnectStart;
            _reconnectionHandler.OnReconnectSuccess -= HandleReconnectSuccess;
            _reconnectionHandler.OnReconnectFailed -= HandleReconnectFailed;
        }
        
        if (_pingMonitor != null)
        {
            _pingMonitor.OnPingUpdated -= UpdatePingDisplay;
        }
    }
}
```

### FeedbackSystem.cs

```csharp
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 게임 내 피드백 시스템
/// </summary>
public class FeedbackSystem : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static FeedbackSystem Instance { get; private set; }
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject _hitEffectPrefab;
    [SerializeField] private GameObject _healEffectPrefab;
    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private GameObject _healTextPrefab;
    [SerializeField] private GameObject _criticalHitPrefab;
    [SerializeField] private CameraShake _cameraShake;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource _effectAudio;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _healSound;
    [SerializeField] private AudioClip _criticalHitSound;
    [SerializeField] private AudioClip _missSound;
    [SerializeField] private AudioClip _levelUpSound;
    [SerializeField] private AudioClip _itemPickupSound;
    
    // 화면 효과 관련
    private Material _screenEffectMaterial;
    private float _damageEffectTime = 0f;
    private float _healEffectTime = 0f;
    
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
        
        // 화면 효과 재질 가져오기
        if (Camera.main != null)
        {
            var effectComponent = Camera.main.GetComponent<ScreenEffects>();
            if (effectComponent != null)
            {
                _screenEffectMaterial = effectComponent.EffectMaterial;
            }
        }
    }
    
    private void Update()
    {
        // 화면 효과 업데이트
        UpdateScreenEffects();
    }
    
    /// <summary>
    /// 데미지 피드백 표시
    /// </summary>
    public void ShowDamageFeedback(GameObject target, int damageAmount, bool isCritical = false)
    {
        if (target == null)
            return;
            
        // 타격 이펙트 생성
        if (_hitEffectPrefab != null)
        {
            Instantiate(_hitEffectPrefab, target.transform.position, Quaternion.identity);
        }
        
        // 데미지 텍스트 생성
        if (_damageTextPrefab != null)
        {
            GameObject textObj = Instantiate(_damageTextPrefab, 
                target.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            
            DamageText damageText = textObj.GetComponent<DamageText>();
            if (damageText != null)
            {
                damageText.SetDamage(damageAmount, isCritical);
            }
        }
        
        // 크리티컬 히트 이펙트
        if (isCritical && _criticalHitPrefab != null)
        {
            Instantiate(_criticalHitPrefab, target.transform.position, Quaternion.identity);
        }
        
        // 카메라 흔들림
        if (_cameraShake != null)
        {
            float shakeAmount = isCritical ? 0.5f : 0.2f;
            _cameraShake.Shake(shakeAmount, 0.2f);
        }
        
        // 화면 효과 활성화
        if (_screenEffectMaterial != null)
        {
            _damageEffectTime = 0.5f;
        }
        
        // 사운드 재생
        if (_effectAudio != null)
        {
            AudioClip clipToPlay = isCritical ? _criticalHitSound : _hitSound;
            if (clipToPlay != null)
            {
                _effectAudio.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                _effectAudio.PlayOneShot(clipToPlay);
            }
        }
    }
    
    /// <summary>
    /// 회복 피드백 표시
    /// </summary>
    public void ShowHealFeedback(GameObject target, int healAmount)
    {
        if (target == null)
            return;
            
        // 회복 이펙트 생성
        if (_healEffectPrefab != null)
        {
            Instantiate(_healEffectPrefab, target.transform.position, Quaternion.identity);
        }
        
        // 회복 텍스트 생성
        if (_healTextPrefab != null)
        {
            GameObject textObj = Instantiate(_healTextPrefab, 
                target.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            
            DamageText healText = textObj.GetComponent<DamageText>();
            if (healText != null)
            {
                healText.SetHeal(healAmount);
            }
        }
        
        // 화면 효과 활성화
        if (_screenEffectMaterial != null)
        {
            _healEffectTime = 0.5f;
        }
        
        // 사운드 재생
        if (_effectAudio != null && _healSound != null)
        {
            _effectAudio.pitch = 1.0f;
            _effectAudio.PlayOneShot(_healSound);
        }
    }
    
    /// <summary>
    /// 미스 피드백 표시
    /// </summary>
    public void ShowMissFeedback(GameObject target)
    {
        if (target == null)
            return;
            
        // 미스 텍스트 생성
        if (_damageTextPrefab != null)
        {
            GameObject textObj = Instantiate(_damageTextPrefab, 
                target.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            
            DamageText missText = textObj.GetComponent<DamageText>();
            if (missText != null)
            {
                missText.SetMiss();
            }
        }
        
        // 사운드 재생
        if (_effectAudio != null && _missSound != null)
        {
            _effectAudio.pitch = 1.0f;
            _effectAudio.PlayOneShot(_missSound);
        }
    }
    
    /// <summary>
    /// 아이템 획득 피드백
    /// </summary>
    public void ShowItemPickupFeedback(ItemData item)
    {
        if (item == null)
            return;
            
        // 아이템 정보 알림
        UIManager.Instance?.ShowStatusMessage($"{item.ItemName} 획득!", MessageType.Success);
        
        // 사운드 재생
        if (_effectAudio != null && _itemPickupSound != null)
        {
            _effectAudio.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            _effectAudio.PlayOneShot(_itemPickupSound);
        }
        
        // 인벤토리 갱신 촉발
        var inventorySystem = InventorySystem.Instance;
        if (inventorySystem != null)
        {
            inventorySystem.TriggerInventoryUpdate();
        }
    }
    
    /// <summary>
    /// 레벨업 피드백
    /// </summary>
    public async UniTask ShowLevelUpFeedback(GameObject target, int newLevel)
    {
        if (target == null)
            return;
            
        // 레벨업 이펙트 (복합적인 파티클 효과)
        var levelUpEffect = Resources.Load<GameObject>("Effects/LevelUpEffect");
        if (levelUpEffect != null)
        {
            GameObject effect = Instantiate(levelUpEffect, 
                target.transform.position, Quaternion.identity);
                
            effect.transform.SetParent(target.transform);
            Destroy(effect, 3f);
        }
        
        // 사운드 재생
        if (_effectAudio != null && _levelUpSound != null)
        {
            _effectAudio.pitch = 1.0f;
            _effectAudio.PlayOneShot(_levelUpSound);
        }
        
        // 레벨업 알림
        UIManager.Instance?.ShowStatusMessage($"레벨 {newLevel} 달성!", MessageType.Success);
        
        // 약간의 시간 간격을 두고 스탯 증가 알림
        await UniTask.Delay(1500);
        
        UIManager.Instance?.ShowStatusMessage("모든 스탯이 증가했습니다!", MessageType.Info);
    }
    
    /// <summary>
    /// 화면 효과 업데이트
    /// </summary>
    private void UpdateScreenEffects()
    {
        if (_screenEffectMaterial == null)
            return;
            
        // 데미지 효과 업데이트
        if (_damageEffectTime > 0)
        {
            _damageEffectTime -= Time.deltaTime;
            _screenEffectMaterial.SetFloat("_DamageAmount", Mathf.Clamp01(_damageEffectTime));
        }
        else
        {
            _screenEffectMaterial.SetFloat("_DamageAmount", 0);
        }
        
        // 회복 효과 업데이트
        if (_healEffectTime > 0)
        {
            _healEffectTime -= Time.deltaTime;
            _screenEffectMaterial.SetFloat("_HealAmount", Mathf.Clamp01(_healEffectTime));
        }
        else
        {
            _screenEffectMaterial.SetFloat("_HealAmount", 0);
        }
    }
}
```

## UI 시스템 핵심 설계 원칙

1. **모듈화**: 각 UI 요소는 독립적으로 설계하여 재사용성과 유지보수성을 높임
2. **단일 책임 원칙**: 각 클래스가 한 가지 책임만 갖도록 설계
3. **이벤트 기반 통신**: 직접적인 의존성을 줄이기 위해 이벤트 시스템 활용
4. **유연한 전환**: 화면 간 부드러운 전환을 위한 애니메이션 및 페이드 효과 구현
5. **반응형 디자인**: 다양한 해상도에 적절히 대응하는 레이아웃 시스템

## UI 피드백 설계 원칙

1. **다중 감각 피드백**: 시각, 청각적 요소를 조합하여 사용자에게 명확한 피드백 제공
2. **일관성**: 유사한 상황에 일관된 피드백을 제공하여 사용자 학습 곡선 완화
3. **적시성**: 적절한 타이밍에 피드백을 제공하여 사용자 액션과 결과의 인과관계 강화
4. **계층화**: 중요도에 따라 다른 강도의 피드백을 제공하여 사용자 주의 집중 유도
5. **비침투성**: 게임플레이를 방해하지 않는 선에서 정보 전달 균형 유지

[이전: UISystem_part2.md]