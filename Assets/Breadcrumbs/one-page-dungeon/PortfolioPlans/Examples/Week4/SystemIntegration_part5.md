# 시스템 통합 예제 - Part 5

## GameInitializer 클래스 (계속)

```csharp
    /// <summary>
    /// 던전 시스템 초기화
    /// </summary>
    private async UniTask InitializeDungeonSystem()
    {
        try
        {
            await _dungeonManager.Initialize();
            _systemStatus["DungeonManager"] = true;
            Debug.Log("Dungeon System initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Dungeon System initialization failed: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 네트워크 시스템 초기화
    /// </summary>
    private async UniTask InitializeNetworkSystem()
    {
        try
        {
            await _networkManager.Initialize();
            _systemStatus["NetworkManager"] = true;
            Debug.Log("Network System initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Network System initialization failed: {ex.Message}");
            // 네트워크 초기화 실패는 게임 진행에 치명적이지 않을 수 있으므로 예외를 다시 throw하지 않음
            // 대신 UI에 경고 메시지 표시
            _uiManager.ShowWarningMessage("네트워크 연결 실패: 오프라인 모드로 진행합니다.");
        }
    }
    
    /// <summary>
    /// 시스템 간 이벤트 연결
    /// </summary>
    private void ConnectSystemEvents()
    {
        // GameManager - PlayerManager 이벤트 연결
        _gameManager.OnGameStateChanged += _playerManager.HandleGameStateChanged;
        _playerManager.OnPlayerDeath += _gameManager.HandlePlayerDeath;
        
        // GameManager - DungeonManager 이벤트 연결
        _gameManager.OnGameStateChanged += _dungeonManager.HandleGameStateChanged;
        _dungeonManager.OnDungeonCompleted += _gameManager.HandleDungeonCompleted;
        
        // GameManager - UIManager 이벤트 연결
        _gameManager.OnGameStateChanged += _uiManager.HandleGameStateChanged;
        
        // PlayerManager - UIManager 이벤트 연결
        _playerManager.OnPlayerHealthChanged += _uiManager.UpdateHealthUI;
        _playerManager.OnExperienceGained += _uiManager.UpdateExperienceUI;
        _playerManager.OnInventoryChanged += _uiManager.UpdateInventoryUI;
        
        // DungeonManager - UIManager 이벤트 연결
        _dungeonManager.OnObjectiveUpdated += _uiManager.UpdateObjectiveUI;
        _dungeonManager.OnDungeonGenerated += _uiManager.ShowDungeonInfoUI;
        
        // NetworkManager 이벤트 연결 (네트워크 모드일 경우)
        if (_systemStatus["NetworkManager"])
        {
            _networkManager.OnNetworkPlayerJoined += _playerManager.HandleNetworkPlayerJoined;
            _networkManager.OnNetworkPlayerLeft += _playerManager.HandleNetworkPlayerLeft;
            _networkManager.OnNetworkDisconnected += _gameManager.HandleNetworkDisconnected;
        }
        
        Debug.Log("System events connected");
    }
```
