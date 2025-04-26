# 게임 루프 구현 예제 - Part 2

## GameManager 클래스 (계속)

```csharp
    // 시스템 참조 초기화
    private void InitializeSystemReferences()
    {
        if (_dungeonManager == null)
            _dungeonManager = FindObjectOfType<DungeonManager>();
        
        if (_networkManager == null)
            _networkManager = FindObjectOfType<NetworkManager>();
            
        if (_uiManager == null)
            _uiManager = FindObjectOfType<UIManager>();
            
        if (_playerManager == null)
            _playerManager = FindObjectOfType<PlayerManager>();

        if (_objectiveManager == null)
            _objectiveManager = FindObjectOfType<ObjectiveManager>();
    }

    // 메인 메뉴 설정
    private void SetupMainMenu()
    {
        _uiManager.ShowMainMenuUI();
    }

    // 매치메이킹 시작
    private async void StartMatchmaking()
    {
        _uiManager.ShowLoadingUI("매치메이킹 중...");
        
        try
        {
            // 네트워크 매니저를 통한 매치메이킹
            bool matchSuccess = await _networkManager.StartMatchmaking();
            if (matchSuccess)
            {
                _isMultiplayerSession = true;
                ChangeState(GameState.DungeonEntry);
            }
            else
            {
                // 매치메이킹 실패 처리
                _uiManager.ShowErrorMessage("매치메이킹에 실패했습니다.");
                ChangeState(GameState.MainMenu);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"매치메이킹 오류: {ex.Message}");
            _uiManager.ShowErrorMessage("네트워크 오류가 발생했습니다.");
            ChangeState(GameState.MainMenu);
        }
    }

    // 새로운 던전 준비
    private async void PrepareNewDungeon()
    {
        _uiManager.ShowLoadingUI("던전 생성 중...");
        
        try
        {
            // 던전 생성
            bool dungeonCreated = await _dungeonManager.GenerateDungeon(_currentLevel, _isMultiplayerSession);
            
            if (dungeonCreated)
            {
                // 플레이어 스폰
                await _playerManager.SpawnPlayers();
                
                // 목표 설정
                _objectiveManager.SetupObjectives(_currentLevel);
                
                // 타이머 초기화
                _remainingTime = _timeLimit;
                
                // 던전 입장 UI 표시
                _uiManager.ShowDungeonEntryUI();
            }
            else
            {
                // 던전 생성 실패 처리
                _uiManager.ShowErrorMessage("던전을 생성할 수 없습니다.");
                ChangeState(GameState.MainMenu);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"던전 생성 오류: {ex.Message}");
            _uiManager.ShowErrorMessage("던전 생성 중 오류가 발생했습니다.");
            ChangeState(GameState.MainMenu);
        }
    }

    // 탐험 시작
    private void StartExploration()
    {
        // 탐험 UI로 전환
        _uiManager.ShowGameplayUI();
        
        // 플레이어 컨트롤 활성화
        _playerManager.EnablePlayerControls(true);
    }

    // 전투 시작
    private void StartCombat()
    {
        // 전투 UI 요소 표시
        _uiManager.ShowCombatUI();
    }

    // 전리품 처리
    private void HandleLoot()
    {
        // 전리품 UI 표시
        _uiManager.ShowLootUI();
    }

    // 던전 탈출
    private void ExitDungeon()
    {
        _uiManager.ShowLoadingUI("던전에서 나가는 중...");
        
        // 리소스 정리
        _dungeonManager.CleanupDungeon();
        
        // 플레이어 데이터 저장 (추후 구현)
        _playerManager.SavePlayerData();
        
        // 멀티플레이어 세션 종료 (필요시)
        if (_isMultiplayerSession)
        {
            _networkManager.LeaveSession();
            _isMultiplayerSession = false;
        }
        
        ChangeState(GameState.MainMenu);
    }

    // 게임 오버 처리
    private void HandleGameOver()
    {
        // 게임 오버 UI 표시
        _uiManager.ShowGameOverUI();
        
        // 플레이어 컨트롤 비활성화
        _playerManager.EnablePlayerControls(false);
        
        // 게임 오버 이벤트 발생
        OnGameOver?.Invoke();
    }

    // 승리 처리
    private void HandleVictory()
    {
        // 승리 UI 표시
        _uiManager.ShowVictoryUI();
        
        // 플레이어 컨트롤 비활성화
        _playerManager.EnablePlayerControls(false);
        
        // 승리 이벤트 발생
        OnVictory?.Invoke();
    }

    // 목표 완료 확인 (ObjectiveManager에서 호출)
    public void CheckObjectiveCompletion()
    {
        if (_objectiveManager.AreAllObjectivesCompleted())
        {
            ChangeState(GameState.Victory);
        }
    }

    // 싱글플레이어 게임 시작
    public void StartSinglePlayerGame()
    {
        _isMultiplayerSession = false;
        ChangeState(GameState.DungeonEntry);
    }

    // 멀티플레이어 게임 시작
    public void StartMultiplayerGame()
    {
        ChangeState(GameState.Matchmaking);
    }

    // 게임 종료 후 메인 메뉴로
    public void ReturnToMainMenu()
    {
        ChangeState(GameState.MainMenu);
    }

    // 게임 종료
    public void QuitGame()
    {
        // 네트워크 연결 해제 등 정리 작업
        if (_isMultiplayerSession)
        {
            _networkManager.DisconnectFromServer();
        }
        
        // 앱 종료
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
```
