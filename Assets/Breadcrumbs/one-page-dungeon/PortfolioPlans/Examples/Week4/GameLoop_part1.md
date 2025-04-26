# 게임 루프 구현 예제 - Part 1

## GameManager 클래스

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;

public enum GameState
{
    MainMenu,
    Matchmaking,
    DungeonEntry,
    Exploration,
    Combat,
    Loot,
    Exit,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    // 현재 게임 상태
    [SerializeField] private GameState _currentState = GameState.MainMenu;
    public GameState CurrentState => _currentState;

    // 시스템 매니저 참조
    [SerializeField] private DungeonManager _dungeonManager;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private ObjectiveManager _objectiveManager;

    // 게임 진행 관련 변수
    [SerializeField] private int _currentLevel = 1;
    [SerializeField] private float _timeLimit = 600f; // 10분
    [SerializeField] private float _remainingTime;
    [SerializeField] private bool _isMultiplayerSession = false;

    // 이벤트
    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnTimerUpdated;
    public event Action OnGameOver;
    public event Action OnVictory;

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
        // 초기 상태 설정
        ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        // 게임 상태가 활성 상태일 때만 타이머 업데이트
        if (IsActiveGameplay())
        {
            UpdateTimer();
        }
    }

    private bool IsActiveGameplay()
    {
        return _currentState == GameState.Exploration || 
               _currentState == GameState.Combat ||
               _currentState == GameState.Loot;
    }

    private void UpdateTimer()
    {
        if (_remainingTime > 0)
        {
            _remainingTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(_remainingTime);
        }
        else
        {
            // 시간이 다 되면 게임 오버
            ChangeState(GameState.GameOver);
        }
    }

    public void ChangeState(GameState newState)
    {
        if (_currentState == newState)
            return;

        // 이전 상태 종료 로직
        ExitState(_currentState);

        // 새로운 상태로 변경
        _currentState = newState;
        
        // 새로운 상태 진입 로직
        EnterState(newState);

        // 이벤트 발생
        OnGameStateChanged?.Invoke(newState);
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Exploration:
                // 탐색 관련 시스템 정리
                break;
            case GameState.Combat:
                // 전투 종료 처리
                break;
            // 다른 상태들에 대한 종료 로직
        }
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                SetupMainMenu();
                break;
            case GameState.Matchmaking:
                StartMatchmaking();
                break;
            case GameState.DungeonEntry:
                PrepareNewDungeon();
                break;
            case GameState.Exploration:
                StartExploration();
                break;
            case GameState.Combat:
                StartCombat();
                break;
            case GameState.Loot:
                HandleLoot();
                break;
            case GameState.Exit:
                ExitDungeon();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
            case GameState.Victory:
                HandleVictory();
                break;
        }
    }
```
