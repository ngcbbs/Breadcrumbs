# 시스템 통합 예제 - Part 4

## GameInitializer 클래스 (계속)

```csharp
    /// <summary>
    /// UI 시스템 초기화
    /// </summary>
    private async UniTask InitializeUISystem()
    {
        try
        {
            await _uiManager.Initialize();
            _systemStatus["UIManager"] = true;
            Debug.Log("UI System initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"UI System initialization failed: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 오디오 시스템 초기화
    /// </summary>
    private async UniTask InitializeAudioSystem()
    {
        try
        {
            await _audioManager.Initialize();
            _systemStatus["AudioManager"] = true;
            Debug.Log("Audio System initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Audio System initialization failed: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 게임 매니저 초기화
    /// </summary>
    private async UniTask InitializeGameManager()
    {
        try
        {
            await _gameManager.Initialize();
            _systemStatus["GameManager"] = true;
            Debug.Log("Game Manager initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Game Manager initialization failed: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// 플레이어 시스템 초기화
    /// </summary>
    private async UniTask InitializePlayerSystem()
    {
        try
        {
            await _playerManager.Initialize();
            _systemStatus["PlayerManager"] = true;
            Debug.Log("Player System initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Player System initialization failed: {ex.Message}");
            throw;
        }
    }
```
