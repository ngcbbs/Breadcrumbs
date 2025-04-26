# UI 시스템 예제 코드 (Part 1)

## UI 관리자 및 화면 전환 시스템

### UIManager.cs

```csharp
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 UI를 전체적으로 관리하는 컴포넌트
/// </summary>
public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UIManager Instance { get; private set; }
    
    // UI 패널 관리
    [SerializeField] private List<UIPanel> _panels = new List<UIPanel>();
    private Dictionary<UIType, UIPanel> _panelMap = new Dictionary<UIType, UIPanel>();
    
    // 현재 활성화된 패널
    private UIPanel _currentPanel;
    private Stack<UIPanel> _panelHistory = new Stack<UIPanel>();
    
    // 오버레이 컨트롤
    [SerializeField] private GameObject _loadingOverlay;
    [SerializeField] private GameObject _notificationOverlay;
    [SerializeField] private Text _notificationText;
    
    // 코루틴 취소 컨트롤
    private System.Threading.CancellationTokenSource _notificationCTS;
    
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
        
        // UI 패널 맵 구성
        foreach (var panel in _panels)
        {
            if (panel != null)
            {
                _panelMap[panel.PanelType] = panel;
                panel.gameObject.SetActive(false);
            }
        }
        
        // 오버레이 초기 비활성화
        if (_loadingOverlay != null)
            _loadingOverlay.SetActive(false);
            
        if (_notificationOverlay != null)
            _notificationOverlay.SetActive(false);
    }
    
    /// <summary>
    /// 패널 표시
    /// </summary>
    public void ShowPanel(UIType panelType, bool remember = true)
    {
        if (!_panelMap.TryGetValue(panelType, out UIPanel panel))
        {
            Debug.LogError($"UI 패널을 찾을 수 없음: {panelType}");
            return;
        }
        
        // 현재 패널 기록하고 숨기기
        if (_currentPanel != null)
        {
            if (remember)
                _panelHistory.Push(_currentPanel);
                
            _currentPanel.Hide();
        }
        
        // 새 패널 표시
        panel.Show();
        _currentPanel = panel;
        
        Debug.Log($"UI 패널 표시: {panelType}");
    }
    
    /// <summary>
    /// 이전 패널로 돌아가기
    /// </summary>
    public void GoBack()
    {
        if (_panelHistory.Count == 0)
        {
            Debug.LogWarning("이전 UI 패널이 없습니다.");
            return;
        }
        
        // 현재 패널 숨기기
        if (_currentPanel != null)
            _currentPanel.Hide();
            
        // 이전 패널 꺼내서 표시
        _currentPanel = _panelHistory.Pop();
        _currentPanel.Show();
        
        Debug.Log($"이전 UI 패널로 돌아감: {_currentPanel.PanelType}");
    }
    
    /// <summary>
    /// 모든 패널 숨기기
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var panel in _panels)
        {
            if (panel != null)
                panel.Hide();
        }
        
        _currentPanel = null;
        _panelHistory.Clear();
        
        Debug.Log("모든 UI 패널 숨김");
    }
    
    /// <summary>
    /// 로딩 오버레이 토글
    /// </summary>
    public void SetLoadingOverlay(bool show)
    {
        if (_loadingOverlay != null)
            _loadingOverlay.SetActive(show);
    }
    
    /// <summary>
    /// 알림 표시
    /// </summary>
    public async UniTask ShowNotification(string message, float duration = 3.0f)
    {
        if (_notificationOverlay == null || _notificationText == null)
            return;
            
        // 이전 알림 취소
        if (_notificationCTS != null)
        {
            _notificationCTS.Cancel();
            _notificationCTS.Dispose();
        }
        
        // 새 취소 토큰 생성
        _notificationCTS = new System.Threading.CancellationTokenSource();
        
        try
        {
            // 알림 텍스트 설정
            _notificationText.text = message;
            _notificationOverlay.SetActive(true);
            
            // 지정된 시간 동안 대기
            await UniTask.Delay((int)(duration * 1000), cancellationToken: _notificationCTS.Token);
            
            // 알림 숨기기
            _notificationOverlay.SetActive(false);
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            // 알림이 취소됨 (새 알림이 표시되거나 수동으로 취소됨)
        }
    }
    
    /// <summary>
    /// 상태 메시지 표시
    /// </summary>
    public void ShowStatusMessage(string message, MessageType type = MessageType.Info)
    {
        string formattedMessage = FormatStatusMessage(message, type);
        ShowNotification(formattedMessage, GetDurationForType(type)).Forget();
    }
    
    /// <summary>
    /// 메시지 타입에 따른 지속 시간 반환
    /// </summary>
    private float GetDurationForType(MessageType type)
    {
        switch (type)
        {
            case MessageType.Error:
                return 5.0f;
            case MessageType.Warning:
                return 4.0f;
            case MessageType.Success:
                return 3.0f;
            case MessageType.Info:
            default:
                return 2.5f;
        }
    }
    
    /// <summary>
    /// 메시지 타입에 따른 형식 적용
    /// </summary>
    private string FormatStatusMessage(string message, MessageType type)
    {
        switch (type)
        {
            case MessageType.Error:
                return $"<color=red>오류: {message}</color>";
            case MessageType.Warning:
                return $"<color=yellow>경고: {message}</color>";
            case MessageType.Success:
                return $"<color=green>성공: {message}</color>";
            case MessageType.Info:
            default:
                return $"<color=white>{message}</color>";
        }
    }
    
    private void OnDestroy()
    {
        if (_notificationCTS != null)
        {
            _notificationCTS.Cancel();
            _notificationCTS.Dispose();
            _notificationCTS = null;
        }
    }
}

/// <summary>
/// UI 패널 타입
/// </summary>
public enum UIType
{
    None,
    MainMenu,
    Lobby,
    RoomCreation,
    RoomLobby,
    Loading,
    InGame,
    Inventory,
    Equipment,
    Settings,
    GameOver,
    Victory,
    Pause
}

/// <summary>
/// 메시지 타입
/// </summary>
public enum MessageType
{
    Info,
    Success,
    Warning,
    Error
}
```

### UIPanel.cs

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 패널의 기본 기능을 제공하는 베이스 클래스
/// </summary>
public abstract class UIPanel : MonoBehaviour
{
    [SerializeField] private UIType _panelType;
    [SerializeField] private Button _backButton;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _useAnimation = true;
    [SerializeField] private float _animationDuration = 0.25f;
    
    // 패널 이벤트
    public event Action OnPanelShown;
    public event Action OnPanelHidden;
    
    // 패널 타입 프로퍼티
    public UIType PanelType => _panelType;
    
    protected virtual void Awake()
    {
        // 캔버스 그룹 가져오기
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
            
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // 뒤로 가기 버튼 설정
        if (_backButton != null)
            _backButton.onClick.AddListener(OnBackButtonClicked);
            
        // 초기 상태 설정
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 패널 표시
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
        
        if (_useAnimation)
        {
            // 애니메이션 적용
            _canvasGroup.alpha = 0f;
            LeanTween.value(gameObject, 0f, 1f, _animationDuration)
                .setOnUpdate((float val) => {
                    _canvasGroup.alpha = val;
                })
                .setOnComplete(() => {
                    OnShowComplete();
                });
        }
        else
        {
            // 즉시 표시
            _canvasGroup.alpha = 1f;
            OnShowComplete();
        }
    }
    
    /// <summary>
    /// 패널 숨기기
    /// </summary>
    public virtual void Hide()
    {
        if (_useAnimation)
        {
            // 애니메이션 적용
            LeanTween.value(gameObject, 1f, 0f, _animationDuration)
                .setOnUpdate((float val) => {
                    _canvasGroup.alpha = val;
                })
                .setOnComplete(() => {
                    OnHideComplete();
                });
        }
        else
        {
            // 즉시 숨기기
            OnHideComplete();
        }
    }
    
    /// <summary>
    /// 표시 애니메이션 완료 처리
    /// </summary>
    protected virtual void OnShowComplete()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        
        OnPanelShown?.Invoke();
    }
    
    /// <summary>
    /// 숨김 애니메이션 완료 처리
    /// </summary>
    protected virtual void OnHideComplete()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        
        OnPanelHidden?.Invoke();
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 뒤로 가기 버튼 클릭 처리
    /// </summary>
    protected virtual void OnBackButtonClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.GoBack();
    }
    
    protected virtual void OnDestroy()
    {
        // 이벤트 리스너 제거
        if (_backButton != null)
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
}
```

[다음: UISystem_part2.md]