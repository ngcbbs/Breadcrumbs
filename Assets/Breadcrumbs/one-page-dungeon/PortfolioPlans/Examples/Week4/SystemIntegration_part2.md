# 시스템 통합 예제 - Part 2

## GameInitializer 클래스 (계속)

```csharp
    #region 시스템 참조 초기화
    
    /// <summary>
    /// 시스템 매니저 참조 초기화
    /// </summary>
    private void InitializeSystemReferences()
    {
        if (_gameManager == null)
            _gameManager = FindOrCreateManager<GameManager>("GameManager");
            
        if (_networkManager == null)
            _networkManager = FindOrCreateManager<NetworkManager>("NetworkManager");
            
        if (_dungeonManager == null)
            _dungeonManager = FindOrCreateManager<DungeonManager>("DungeonManager");
            
        if (_playerManager == null)
            _playerManager = FindOrCreateManager<PlayerManager>("PlayerManager");
            
        if (_uiManager == null)
            _uiManager = FindOrCreateManager<UIManager>("UIManager");
            
        if (_audioManager == null)
            _audioManager = FindOrCreateManager<AudioManager>("AudioManager");
    }
    
    /// <summary>
    /// 매니저 찾기 또는 생성
    /// </summary>
    private T FindOrCreateManager<T>(string managerName) where T : Component
    {
        T manager = FindObjectOfType<T>();
        
        if (manager == null)
        {
            GameObject go = new GameObject(managerName);
            manager = go.AddComponent<T>();
            DontDestroyOnLoad(go);
        }
        
        return manager;
    }
    
    #endregion
    
    #region 시스템 초기화 및 통합
    
    /// <summary>
    /// 시스템 상태 초기화
    /// </summary>
    private void InitializeSystemStatus()
    {
        _systemStatus.Clear();
        _systemStatus.Add("GameManager", false);
        _systemStatus.Add("NetworkManager", false);
        _systemStatus.Add("DungeonManager", false);
        _systemStatus.Add("PlayerManager", false);
        _systemStatus.Add("UIManager", false);
        _systemStatus.Add("AudioManager", false);
    }
```
