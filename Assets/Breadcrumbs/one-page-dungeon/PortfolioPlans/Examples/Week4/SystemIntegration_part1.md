# 시스템 통합 예제 - Part 1

## GameInitializer 클래스

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;

/// <summary>
/// 게임 시스템 초기화 및 통합 관리 클래스
/// 모든 주요 시스템을 초기화하고 연결하는 역할을 합니다.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Core Systems")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private DungeonManager _dungeonManager;
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private AudioManager _audioManager;
    
    [Header("Initialization Settings")]
    [SerializeField] private bool _initializeNetworkOnStart = false;
    [SerializeField] private bool _loadLastSessionOnStart = false;
    [SerializeField] private bool _enableAutoSave = true;
    [SerializeField] private float _autoSaveInterval = 300f; // 5분
    
    // 초기화 상태
    private bool _isInitialized = false;
    private bool _initializationInProgress = false;
    private Dictionary<string, bool> _systemStatus = new Dictionary<string, bool>();
    
    // 자동 저장 타이머
    private float _autoSaveTimer = 0f;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        // 시스템 참조 초기화
        InitializeSystemReferences();
        
        // 시스템 상태 초기화
        InitializeSystemStatus();
    }
    
    private void Start()
    {
        // 게임 시작 시 전체 시스템 초기화
        InitializeAllSystems().Forget();
    }
    
    private void Update()
    {
        // 자동 저장 처리
        if (_isInitialized && _enableAutoSave)
        {
            _autoSaveTimer += Time.deltaTime;
            
            if (_autoSaveTimer >= _autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                SaveGameState();
            }
        }
    }
    
    private void OnApplicationQuit()
    {
        // 게임 종료 시 정리 작업
        if (_isInitialized)
        {
            SaveGameState();
            ShutdownAllSystems();
        }
    }
```
