# 데모 시나리오 구현 예제 - Part 4

## DemoManager 클래스 (계속)

```csharp
    #region 멀티플레이어 네트워킹 데모

    /// <summary>
    /// 멀티플레이어 데모 준비
    /// </summary>
    private async UniTask PrepareMultiplayerDemo()
    {
        // 서버 연결
        _uiManager.ShowLoadingUI("서버에 연결 중...");
        bool connected = await _networkManager.ConnectToServer();
        
        if (connected)
        {
            // 로컬 플레이어 스폰
            await _playerManager.SpawnLocalPlayer();
            
            // 가짜 원격 플레이어 스폰 (데모용)
            await _playerManager.SpawnDemoRemotePlayers(2);
            
            // 간단한 던전 생성
            await _dungeonManager.GenerateSimpleDemoMap();
            
            // 네트워크 시각화 활성화
            _networkManager.EnableNetworkVisualization(true);
            
            // 가이드 메시지
            ShowGuidanceMessage("멀티플레이어 네트워킹 데모를 시작합니다. 네트워크 동기화 및 다른 플레이어와의 상호작용을 확인해보세요.");
        }
        else
        {
            ShowGuidanceMessage("서버 연결에 실패했습니다. 데모 서버가 실행 중인지 확인해주세요.");
            StopDemoScenario();
        }
    }

    /// <summary>
    /// 멀티플레이어 네트워킹 데모 단계 실행
    /// </summary>
    private async UniTask RunMultiplayerNetworkingStep()
    {
        switch (_currentStepIndex)
        {
            case 0: // 플레이어 위치 동기화
                ShowGuidanceMessage("1단계: 플레이어 위치 동기화. 이동하며 네트워크 동기화를 확인해보세요.");
                _networkManager.HighlightPositionSynchronization();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "플레이어 위치 동기화");
                break;
                
            case 1: // 애니메이션 동기화
                ShowGuidanceMessage("2단계: 애니메이션 동기화. 달리기, 점프, 공격 등 다양한 액션 동기화를 확인해보세요.");
                _networkManager.HighlightAnimationSynchronization();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "애니메이션 동기화");
                break;
                
            case 2: // 채팅 기능
                ShowGuidanceMessage("3단계: 채팅 기능. Enter 키를 눌러 채팅창을 열고 메시지를 보내보세요.");
                _networkManager.EnableDemoChat();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "채팅 기능");
                break;
                
            case 3: // 상태 효과 동기화
                ShowGuidanceMessage("4단계: 상태 효과 동기화. 플레이어 상태 효과(버프/디버프)가 다른 플레이어에게 어떻게 표시되는지 확인해보세요.");
                await _playerManager.ApplyDemoStatusEffects();
                _networkManager.HighlightStatusEffectSynchronization();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "상태 효과 동기화");
                break;
                
            case 4: // 오브젝트 상호작용 동기화
                ShowGuidanceMessage("5단계: 오브젝트 상호작용 동기화. 주변 오브젝트와 상호작용하며 다른 플레이어에게 어떻게 표시되는지 확인해보세요.");
                await _dungeonManager.SpawnNetworkedInteractableObjectsForDemo();
                _networkManager.HighlightObjectSynchronization();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "오브젝트 상호작용 동기화");
                break;
                
            case 5: // 네트워크 지연 시뮬레이션
                ShowGuidanceMessage("6단계: 네트워크 지연 시뮬레이션. 다양한 네트워크 환경을 시뮬레이션하고, 어떻게 대응하는지 확인해보세요.");
                _networkManager.ShowNetworkSimulationUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "네트워크 지연 시뮬레이션");
                break;
                
            case 6: // 데모 종료
                ShowGuidanceMessage("멀티플레이어 네트워킹 데모가 완료되었습니다.");
                _networkManager.DisableAllHighlights();
                _networkManager.EnableNetworkVisualization(false);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "데모 종료");
                
                // 데모 종료
                CompleteDemo();
                break;
        }
    }

    /// <summary>
    /// 멀티플레이어 데모의 현재 단계 가이드 메시지
    /// </summary>
    private void ShowMultiplayerGuidance()
    {
        switch (_currentStepIndex)
        {
            case 0:
                ShowGuidanceMessage("1단계: 플레이어 위치 동기화. 이동하며 네트워크 동기화를 확인해보세요.");
                break;
                
            case 1:
                ShowGuidanceMessage("2단계: 애니메이션 동기화. 달리기, 점프, 공격 등 다양한 액션 동기화를 확인해보세요.");
                break;
                
            case 2:
                ShowGuidanceMessage("3단계: 채팅 기능. Enter 키를 눌러 채팅창을 열고 메시지를 보내보세요.");
                break;
                
            case 3:
                ShowGuidanceMessage("4단계: 상태 효과 동기화. 플레이어 상태 효과(버프/디버프)가 다른 플레이어에게 어떻게 표시되는지 확인해보세요.");
                break;
                
            case 4:
                ShowGuidanceMessage("5단계: 오브젝트 상호작용 동기화. 주변 오브젝트와 상호작용하며 다른 플레이어에게 어떻게 표시되는지 확인해보세요.");
                break;
                
            case 5:
                ShowGuidanceMessage("6단계: 네트워크 지연 시뮬레이션. 다양한 네트워크 환경을 시뮬레이션하고, 어떻게 대응하는지 확인해보세요.");
                break;
                
            case 6:
                ShowGuidanceMessage("멀티플레이어 네트워킹 데모 완료. '완료' 버튼을 클릭하여 데모를 종료하세요.");
                break;
        }
    }

    #endregion
```
