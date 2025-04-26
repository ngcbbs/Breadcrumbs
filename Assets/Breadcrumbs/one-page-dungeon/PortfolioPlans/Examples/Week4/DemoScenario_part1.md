# 데모 시나리오 구현 예제 - Part 1

## DemoManager 클래스

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;

/// <summary>
/// 포트폴리오 데모 시나리오 관리 클래스
/// 핵심 기능 시연을 위한 시퀀스 및 설정을 관리합니다.
/// </summary>
public class DemoManager : MonoBehaviour
{
    #region Singleton
    private static DemoManager _instance;
    public static DemoManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DemoManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DemoManager");
                    _instance = go.AddComponent<DemoManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    // 시스템 매니저 참조
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private DungeonManager _dungeonManager;
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private UIManager _uiManager;

    // 데모 시나리오 설정
    [SerializeField] private DemoScenarioType _currentScenario = DemoScenarioType.None;
    [SerializeField] private bool _isAutoDemo = false;
    [SerializeField] private float _demoStepDelay = 1.5f;
    
    // 데모 진행 상태
    private int _currentStepIndex = 0;
    private bool _isDemoRunning = false;
    private bool _isPaused = false;
    
    // 이벤트
    public event Action<DemoScenarioType> OnDemoScenarioChanged;
    public event Action<int, string> OnDemoStepChanged;
    public event Action OnDemoCompleted;

    // 가이드 메시지 큐
    private Queue<string> _guideMessages = new Queue<string>();
    private Coroutine _guideMessageCoroutine;
    
    // 데모 시나리오 타입
    public enum DemoScenarioType
    {
        None,
        DungeonGeneration,
        SinglePlayerGameplay,
        MultiplayerNetworking,
        CombatSystem,
        ItemsAndInventory,
        FullGameLoop
    }

    private void Awake()
    {
        // 싱글톤 로직
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 시스템 참조 초기화
        InitializeSystemReferences();
    }

    private void Start()
    {
        // 가이드 메시지 코루틴 시작
        _guideMessageCoroutine = StartCoroutine(ProcessGuideMessages());
    }

    private void OnDestroy()
    {
        // 코루틴 정리
        if (_guideMessageCoroutine != null)
        {
            StopCoroutine(_guideMessageCoroutine);
        }
    }

    private void InitializeSystemReferences()
    {
        if (_gameManager == null)
            _gameManager = GameManager.Instance;
            
        if (_dungeonManager == null)
            _dungeonManager = FindObjectOfType<DungeonManager>();
            
        if (_playerManager == null)
            _playerManager = FindObjectOfType<PlayerManager>();
            
        if (_networkManager == null)
            _networkManager = FindObjectOfType<NetworkManager>();
            
        if (_uiManager == null)
            _uiManager = FindObjectOfType<UIManager>();
    }

    /// <summary>
    /// 특정 데모 시나리오 시작
    /// </summary>
    /// <param name="scenarioType">실행할 데모 시나리오 타입</param>
    /// <param name="autoRun">자동 실행 여부</param>
    public async UniTask StartDemoScenario(DemoScenarioType scenarioType, bool autoRun = false)
    {
        // 이미 실행 중인 데모가 있으면 중지
        if (_isDemoRunning)
        {
            StopDemoScenario();
        }
        
        _currentScenario = scenarioType;
        _isAutoDemo = autoRun;
        _currentStepIndex = 0;
        _isDemoRunning = true;
        _isPaused = false;
        
        // 이벤트 발생
        OnDemoScenarioChanged?.Invoke(scenarioType);
        
        // 데모 준비
        await PrepareDemo(scenarioType);
        
        // 첫 단계 실행
        if (_isAutoDemo)
        {
            await RunNextDemoStep();
        }
        else
        {
            // 수동 모드에서는 가이드 메시지만 표시
            ShowGuidanceForCurrentStep();
        }
    }

    /// <summary>
    /// 현재 데모 시나리오 중지
    /// </summary>
    public void StopDemoScenario()
    {
        if (!_isDemoRunning)
            return;
            
        _isDemoRunning = false;
        _currentScenario = DemoScenarioType.None;
        
        // 데모 관련 리소스 정리
        CleanupDemo();
        
        // UI 초기화
        _uiManager.HideDemoGuidanceUI();
        
        // 게임 상태 초기화
        _gameManager.ReturnToMainMenu();
    }

    /// <summary>
    /// 다음 데모 단계 실행
    /// </summary>
    public async UniTask RunNextDemoStep()
    {
        if (!_isDemoRunning || _isPaused)
            return;
            
        switch (_currentScenario)
        {
            case DemoScenarioType.DungeonGeneration:
                await RunDungeonGenerationStep();
                break;
                
            case DemoScenarioType.SinglePlayerGameplay:
                await RunSinglePlayerGameplayStep();
                break;
                
            case DemoScenarioType.MultiplayerNetworking:
                await RunMultiplayerNetworkingStep();
                break;
                
            case DemoScenarioType.CombatSystem:
                await RunCombatSystemStep();
                break;
                
            case DemoScenarioType.ItemsAndInventory:
                await RunItemsAndInventoryStep();
                break;
                
            case DemoScenarioType.FullGameLoop:
                await RunFullGameLoopStep();
                break;
        }
        
        // 다음 단계로 이동
        _currentStepIndex++;
        
        // 자동 데모가 활성화되어 있으면 다음 단계 자동 실행
        if (_isAutoDemo)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_demoStepDelay));
            await RunNextDemoStep();
        }
        else
        {
            // 수동 모드에서는 가이드 메시지만 표시
            ShowGuidanceForCurrentStep();
        }
    }
```
