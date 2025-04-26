# 데모 시나리오 구현 예제 - Part 2

## DemoManager 클래스 (계속)

```csharp
    /// <summary>
    /// 데모 일시 정지/재개
    /// </summary>
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        
        // 일시 정지 UI 표시
        if (_isPaused)
        {
            _uiManager.ShowDemoPauseUI();
        }
        else
        {
            _uiManager.HideDemoPauseUI();
            
            // 자동 모드에서는 재개 시 다음 단계 실행
            if (_isAutoDemo)
            {
                RunNextDemoStep().Forget();
            }
        }
    }

    /// <summary>
    /// 가이드 메시지 표시
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    public void ShowGuidanceMessage(string message)
    {
        _guideMessages.Enqueue(message);
    }

    /// <summary>
    /// 가이드 메시지 처리 코루틴
    /// </summary>
    private IEnumerator ProcessGuideMessages()
    {
        while (true)
        {
            if (_guideMessages.Count > 0)
            {
                string message = _guideMessages.Dequeue();
                _uiManager.ShowDemoGuidanceUI(message);
                
                // 메시지 표시 시간
                yield return new WaitForSeconds(5f);
                
                // 다음 메시지가 없으면 UI 숨김
                if (_guideMessages.Count == 0)
                {
                    _uiManager.FadeOutDemoGuidanceUI();
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 특정 데모 시나리오 준비
    /// </summary>
    private async UniTask PrepareDemo(DemoScenarioType scenarioType)
    {
        // 로딩 UI 표시
        _uiManager.ShowLoadingUI("데모 준비 중...");
        
        try
        {
            switch (scenarioType)
            {
                case DemoScenarioType.DungeonGeneration:
                    await PrepareDungeonGenerationDemo();
                    break;
                    
                case DemoScenarioType.SinglePlayerGameplay:
                    await PrepareSinglePlayerDemo();
                    break;
                    
                case DemoScenarioType.MultiplayerNetworking:
                    await PrepareMultiplayerDemo();
                    break;
                    
                case DemoScenarioType.CombatSystem:
                    await PrepareCombatDemo();
                    break;
                    
                case DemoScenarioType.ItemsAndInventory:
                    await PrepareItemsDemo();
                    break;
                    
                case DemoScenarioType.FullGameLoop:
                    await PrepareFullGameLoopDemo();
                    break;
            }
            
            // 데모 UI 표시
            _uiManager.ShowDemoUI(scenarioType.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"데모 준비 오류: {ex.Message}");
            _uiManager.ShowErrorMessage("데모 준비 중 오류가 발생했습니다.");
            StopDemoScenario();
        }
    }

    /// <summary>
    /// 데모 정리
    /// </summary>
    private void CleanupDemo()
    {
        // 던전 정리
        _dungeonManager.CleanupDungeon();
        
        // 네트워크 연결 정리 (필요 시)
        if (_currentScenario == DemoScenarioType.MultiplayerNetworking || 
            _currentScenario == DemoScenarioType.FullGameLoop)
        {
            _networkManager.DisconnectFromServer();
        }
        
        // 가이드 메시지 큐 초기화
        _guideMessages.Clear();
    }

    #region 던전 생성 데모

    /// <summary>
    /// 던전 생성 데모 준비
    /// </summary>
    private async UniTask PrepareDungeonGenerationDemo()
    {
        // 던전 생성 데모에 필요한 설정 준비
        _dungeonManager.SetDemoMode(true);
        
        // 던전 생성 파라미터 초기화
        await _dungeonManager.InitializeGenerationParameters();
        
        // 첫 단계 가이드 메시지
        ShowGuidanceMessage("던전 생성 데모를 시작합니다. '다음' 버튼을 클릭하여 프로시저럴 던전 생성의 각 단계를 확인하세요.");
    }

    /// <summary>
    /// 던전 생성 데모 단계 실행
    /// </summary>
    private async UniTask RunDungeonGenerationStep()
    {
        switch (_currentStepIndex)
        {
            case 0: // 던전 레이아웃 생성
                ShowGuidanceMessage("1단계: 기본 던전 레이아웃을 생성합니다.");
                await _dungeonManager.GenerateBaseDungeonLayout(true); // 시각화 활성화
                OnDemoStepChanged?.Invoke(_currentStepIndex, "던전 레이아웃 생성");
                break;
                
            case 1: // 방 생성
                ShowGuidanceMessage("2단계: 던전 내 방을 생성합니다.");
                await _dungeonManager.GenerateRooms(true);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "방 생성");
                break;
                
            case 2: // 복도 생성
                ShowGuidanceMessage("3단계: 방을 연결하는 복도를 생성합니다.");
                await _dungeonManager.GenerateCorridors(true);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "복도 생성");
                break;
                
            case 3: // 지형 디테일 추가
                ShowGuidanceMessage("4단계: 지형 디테일을 추가합니다.");
                await _dungeonManager.AddTerrainDetails(true);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "지형 디테일 추가");
                break;
                
            case 4: // 적 스폰 포인트 배치
                ShowGuidanceMessage("5단계: 적 스폰 포인트를 배치합니다.");
                await _dungeonManager.PlaceEnemySpawnPoints(true);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "적 스폰 포인트 배치");
                break;
                
            case 5: // 아이템 스폰 포인트 배치
                ShowGuidanceMessage("6단계: 아이템 스폰 포인트를 배치합니다.");
                await _dungeonManager.PlaceItemSpawnPoints(true);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "아이템 스폰 포인트 배치");
                break;
                
            case 6: // 시각화 완료
                ShowGuidanceMessage("던전 생성이 완료되었습니다. 모든 단계가 종합되어 완전한 던전이 생성되었습니다.");
                await _dungeonManager.FinalizeVisualization();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "시각화 완료");
                
                // 데모 종료
                CompleteDemo();
                break;
        }
    }
    
    /// <summary>
    /// 현재 단계에 맞는 가이드 메시지 표시
    /// </summary>
    private void ShowGuidanceForCurrentStep()
    {
        // 시나리오별로 현재 단계에 맞는 가이드 메시지 표시
        switch (_currentScenario)
        {
            case DemoScenarioType.DungeonGeneration:
                ShowDungeonGenerationGuidance();
                break;
                
            case DemoScenarioType.SinglePlayerGameplay:
                ShowSinglePlayerGuidance();
                break;
                
            case DemoScenarioType.MultiplayerNetworking:
                ShowMultiplayerGuidance();
                break;
                
            case DemoScenarioType.CombatSystem:
                ShowCombatGuidance();
                break;
                
            case DemoScenarioType.ItemsAndInventory:
                ShowItemsGuidance();
                break;
                
            case DemoScenarioType.FullGameLoop:
                ShowFullGameLoopGuidance();
                break;
        }
    }
    
    /// <summary>
    /// 던전 생성 데모의 현재 단계 가이드 메시지
    /// </summary>
    private void ShowDungeonGenerationGuidance()
    {
        switch (_currentStepIndex)
        {
            case 0:
                ShowGuidanceMessage("1단계: 기본 던전 레이아웃을 생성합니다. '다음' 버튼을 클릭하세요.");
                break;
                
            case 1:
                ShowGuidanceMessage("2단계: 던전 내 방을 생성합니다. '다음' 버튼을 클릭하세요.");
                break;
                
            case 2:
                ShowGuidanceMessage("3단계: 방을 연결하는 복도를 생성합니다. '다음' 버튼을 클릭하세요.");
                break;
                
            case 3:
                ShowGuidanceMessage("4단계: 지형 디테일을 추가합니다. '다음' 버튼을 클릭하세요.");
                break;
                
            case 4:
                ShowGuidanceMessage("5단계: 적 스폰 포인트를 배치합니다. '다음' 버튼을 클릭하세요.");
                break;
                
            case 5:
                ShowGuidanceMessage("6단계: 아이템 스폰 포인트를 배치합니다. '다음' 버튼을 클릭하세요.");
                break;
                
            case 6:
                ShowGuidanceMessage("던전 생성 완료. '완료' 버튼을 클릭하여 데모를 종료하세요.");
                break;
        }
    }
```
