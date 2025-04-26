# 시스템 통합 예제 - Part 3

## GameInitializer 클래스 (계속)

```csharp
    /// <summary>
    /// 모든 시스템 초기화
    /// </summary>
    private async UniTask InitializeAllSystems()
    {
        if (_initializationInProgress)
            return;
            
        _initializationInProgress = true;
        
        try
        {
            // UI 시스템 먼저 초기화하여 로딩 화면 표시
            await InitializeUISystem();
            
            // 로딩 화면 표시
            _uiManager.ShowLoadingUI("시스템 초기화 중...");
            
            // 오디오 시스템 초기화
            await InitializeAudioSystem();
            _uiManager.UpdateLoadingProgress(0.1f);
            
            // 게임 매니저 초기화
            await InitializeGameManager();
            _uiManager.UpdateLoadingProgress(0.2f);
            
            // 플레이어 시스템 초기화
            await InitializePlayerSystem();
            _uiManager.UpdateLoadingProgress(0.4f);
            
            // 던전 시스템 초기화
            await InitializeDungeonSystem();
            _uiManager.UpdateLoadingProgress(0.6f);
            
            // 네트워크 시스템 초기화 (옵션에 따라)
            if (_initializeNetworkOnStart)
            {
                await InitializeNetworkSystem();
            }
            else
            {
                _systemStatus["NetworkManager"] = true;
            }
            _uiManager.UpdateLoadingProgress(0.8f);
            
            // 시스템 간 이벤트 연결
            ConnectSystemEvents();
            
            // 이전 세션 로드 (옵션에 따라)
            if (_loadLastSessionOnStart)
            {
                await LoadLastSession();
            }
            _uiManager.UpdateLoadingProgress(1.0f);
            
            // 초기화 완료 상태 업데이트
            _isInitialized = true;
            _initializationInProgress = false;
            
            // 로딩 화면 숨김
            await UniTask.Delay(500); // 잠시 대기 후 로딩 화면 숨김
            _uiManager.HideLoadingUI();
            
            // 게임 준비 완료 이벤트 발생
            _gameManager.OnGameReady();
            
            Debug.Log("All systems initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"System initialization failed: {ex.Message}");
            _uiManager.ShowErrorMessage("시스템 초기화 실패");
            _initializationInProgress = false;
        }
    }
```
