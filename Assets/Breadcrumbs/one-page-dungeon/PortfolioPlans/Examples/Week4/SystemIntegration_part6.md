# 시스템 통합 예제 - Part 6

## GameInitializer 클래스 (계속)

```csharp    
    /// <summary>
    /// 이전 세션 로드
    /// </summary>
    private async UniTask LoadLastSession()
    {
        try
        {
            // 저장된 세션이 있는지 확인
            if (PlayerPrefs.HasKey("LastSessionSaved") && PlayerPrefs.GetInt("LastSessionSaved") == 1)
            {
                _uiManager.ShowLoadingUI("이전 세션 로드 중...");
                
                // 게임 상태 로드
                await _gameManager.LoadGameState();
                
                // 플레이어 상태 로드
                await _playerManager.LoadPlayerState();
                
                // 던전 상태 로드
                await _dungeonManager.LoadDungeonState();
                
                Debug.Log("Last session loaded");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load last session: {ex.Message}");
            _uiManager.ShowWarningMessage("이전 세션을 로드할 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 게임 상태 저장
    /// </summary>
    private void SaveGameState()
    {
        if (!_isInitialized)
            return;
            
        try
        {
            // 게임 상태 저장
            _gameManager.SaveGameState();
            
            // 플레이어 상태 저장
            _playerManager.SavePlayerState();
            
            // 던전 상태 저장
            _dungeonManager.SaveDungeonState();
            
            // 저장 완료 표시
            PlayerPrefs.SetInt("LastSessionSaved", 1);
            PlayerPrefs.Save();
            
            Debug.Log("Game state saved");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save game state: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 모든 시스템 종료
    /// </summary>
    private void ShutdownAllSystems()
    {
        try
        {
            // 네트워크 시스템 종료
            if (_systemStatus["NetworkManager"])
            {
                _networkManager.Shutdown();
            }
            
            // 던전 시스템 종료
            if (_systemStatus["DungeonManager"])
            {
                _dungeonManager.Shutdown();
            }
            
            // 플레이어 시스템 종료
            if (_systemStatus["PlayerManager"])
            {
                _playerManager.Shutdown();
            }
            
            // 게임 매니저 종료
            if (_systemStatus["GameManager"])
            {
                _gameManager.Shutdown();
            }
            
            // 오디오 시스템 종료
            if (_systemStatus["AudioManager"])
            {
                _audioManager.Shutdown();
            }
            
            // UI 시스템 종료
            if (_systemStatus["UIManager"])
            {
                _uiManager.Shutdown();
            }
            
            Debug.Log("All systems shutdown successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during system shutdown: {ex.Message}");
        }
    }
    
    #endregion
}
```
