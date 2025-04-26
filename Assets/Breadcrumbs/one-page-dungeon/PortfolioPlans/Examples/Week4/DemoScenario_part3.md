# 데모 시나리오 구현 예제 - Part 3

## DemoManager 클래스 (계속)

```csharp
    #endregion

    #region 싱글플레이어 게임플레이 데모

    /// <summary>
    /// 싱글플레이어 데모 준비
    /// </summary>
    private async UniTask PrepareSinglePlayerDemo()
    {
        // 던전 생성
        await _dungeonManager.GenerateDungeon(1, false);
        
        // 플레이어 스폰
        await _playerManager.SpawnPlayers();
        
        // 가이드 메시지
        ShowGuidanceMessage("싱글플레이어 게임플레이 데모를 시작합니다. WASD로 이동하고, 마우스로 시점을 조절할 수 있습니다.");
    }

    /// <summary>
    /// 싱글플레이어 게임플레이 데모 단계 실행
    /// </summary>
    private async UniTask RunSinglePlayerGameplayStep()
    {
        switch (_currentStepIndex)
        {
            case 0: // 기본 이동 및 카메라 컨트롤
                ShowGuidanceMessage("1단계: 기본 이동 및 카메라 컨트롤. WASD로 이동하고, 마우스로 시점을 조절해보세요.");
                _playerManager.EnablePlayerControls(true);
                _playerManager.HighlightMovementControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "기본 이동 및 카메라 컨트롤");
                break;
                
            case 1: // 달리기 및 점프
                ShowGuidanceMessage("2단계: 달리기 및 점프. Shift 키를 눌러 달리고, Space 키를 눌러 점프해보세요.");
                _playerManager.HighlightSprintJumpControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "달리기 및 점프");
                break;
                
            case 2: // 상호작용
                ShowGuidanceMessage("3단계: 상호작용. E 키를 눌러 주변 오브젝트와 상호작용해보세요.");
                await _dungeonManager.SpawnInteractableObjectsForDemo();
                _playerManager.HighlightInteractionControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "상호작용");
                break;
                
            case 3: // 전투 기본
                ShowGuidanceMessage("4단계: 기본 전투. 마우스 왼쪽 버튼으로 공격해보세요.");
                await _dungeonManager.SpawnEnemiesForDemo(2);
                _playerManager.HighlightCombatControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "전투 기본");
                break;
                
            case 4: // 스킬 사용
                ShowGuidanceMessage("5단계: 스킬 사용. 1, 2, 3 키를 눌러 스킬을 사용해보세요.");
                _playerManager.HighlightSkillControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "스킬 사용");
                break;
                
            case 5: // 인벤토리 관리
                ShowGuidanceMessage("6단계: 인벤토리 관리. I 키를 눌러 인벤토리를 열고, 아이템을 관리해보세요.");
                await _playerManager.AddDemoItems();
                _playerManager.HighlightInventoryControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "인벤토리 관리");
                break;
                
            case 6: // 플레이 종료
                ShowGuidanceMessage("싱글플레이어 게임플레이 데모가 완료되었습니다.");
                _playerManager.DisableHighlightControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "플레이 종료");
                
                // 데모 종료
                CompleteDemo();
                break;
        }
    }

    /// <summary>
    /// 싱글플레이어 데모의 현재 단계 가이드 메시지
    /// </summary>
    private void ShowSinglePlayerGuidance()
    {
        switch (_currentStepIndex)
        {
            case 0:
                ShowGuidanceMessage("1단계: 기본 이동 및 카메라 컨트롤. WASD로 이동하고, 마우스로 시점을 조절해보세요.");
                break;
                
            case 1:
                ShowGuidanceMessage("2단계: 달리기 및 점프. Shift 키를 눌러 달리고, Space 키를 눌러 점프해보세요.");
                break;
                
            case 2:
                ShowGuidanceMessage("3단계: 상호작용. E 키를 눌러 주변 오브젝트와 상호작용해보세요.");
                break;
                
            case 3:
                ShowGuidanceMessage("4단계: 기본 전투. 마우스 왼쪽 버튼으로 공격해보세요.");
                break;
                
            case 4:
                ShowGuidanceMessage("5단계: 스킬 사용. 1, 2, 3 키를 눌러 스킬을 사용해보세요.");
                break;
                
            case 5:
                ShowGuidanceMessage("6단계: 인벤토리 관리. I 키를 눌러 인벤토리를 열고, 아이템을 관리해보세요.");
                break;
                
            case 6:
                ShowGuidanceMessage("싱글플레이어 게임플레이 데모 완료. '완료' 버튼을 클릭하여 데모를 종료하세요.");
                break;
        }
    }

    #endregion
```
