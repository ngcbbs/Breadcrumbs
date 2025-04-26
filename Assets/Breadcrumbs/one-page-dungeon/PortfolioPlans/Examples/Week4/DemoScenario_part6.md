# 데모 시나리오 구현 예제 - Part 6

## DemoManager 클래스 (계속)

```csharp
    #region 아이템 및 인벤토리 데모

    /// <summary>
    /// 아이템 및 인벤토리 데모 준비
    /// </summary>
    private async UniTask PrepareItemsDemo()
    {
        // 간단한 환경 생성
        await _dungeonManager.GenerateItemDemoEnvironment();
        
        // 플레이어 스폰
        await _playerManager.SpawnPlayerForItemDemo();
        
        // 인벤토리 초기화
        _playerManager.InitializeInventoryForDemo();
        
        // 데모용 아이템 스폰
        await _dungeonManager.SpawnDemoItems();
        
        // 가이드 메시지
        ShowGuidanceMessage("아이템 및 인벤토리 데모를 시작합니다. 다양한 아이템 상호작용과 인벤토리 관리를 시험해볼 수 있습니다.");
    }

    /// <summary>
    /// 아이템 및 인벤토리 데모 단계 실행
    /// </summary>
    private async UniTask RunItemsAndInventoryStep()
    {
        switch (_currentStepIndex)
        {
            case 0: // 아이템 획득
                ShowGuidanceMessage("1단계: 아이템 획득. E 키를 눌러 주변 아이템을 획득해보세요.");
                _playerManager.HighlightItemPickupControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "아이템 획득");
                break;
                
            case 1: // 인벤토리 관리
                ShowGuidanceMessage("2단계: 인벤토리 관리. I 키를 눌러 인벤토리를 열고, 아이템을 정렬, 이동, 검색해보세요.");
                _playerManager.HighlightInventoryUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "인벤토리 관리");
                break;
                
            case 2: // 아이템 장착
                ShowGuidanceMessage("3단계: 아이템 장착. 장비 아이템을 장착하고 플레이어 스탯 변화를 확인해보세요.");
                _playerManager.HighlightEquipmentSlots();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "아이템 장착");
                break;
                
            case 3: // 아이템 사용
                ShowGuidanceMessage("4단계: 아이템 사용. 소모품 아이템을 사용하고 효과를 확인해보세요.");
                _playerManager.HighlightConsumableItems();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "아이템 사용");
                break;
                
            case 4: // 아이템 조합
                ShowGuidanceMessage("5단계: 아이템 조합. 재료 아이템을 조합하여 새로운 아이템을 만들어보세요.");
                _playerManager.ShowCraftingUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "아이템 조합");
                break;
                
            case 5: // 아이템 강화
                ShowGuidanceMessage("6단계: 아이템 강화. 장비 아이템을 강화하고 효과를 확인해보세요.");
                _playerManager.ShowEnhancementUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "아이템 강화");
                break;
                
            case 6: // 데모 종료
                ShowGuidanceMessage("아이템 및 인벤토리 데모가 완료되었습니다.");
                _playerManager.DisableAllItemHighlights();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "데모 종료");
                
                // 데모 종료
                CompleteDemo();
                break;
        }
    }

    /// <summary>
    /// 아이템 및 인벤토리 데모의 현재 단계 가이드 메시지
    /// </summary>
    private void ShowItemsGuidance()
    {
        switch (_currentStepIndex)
        {
            case 0:
                ShowGuidanceMessage("1단계: 아이템 획득. E 키를 눌러 주변 아이템을 획득해보세요.");
                break;
                
            case 1:
                ShowGuidanceMessage("2단계: 인벤토리 관리. I 키를 눌러 인벤토리를 열고, 아이템을 정렬, 이동, 검색해보세요.");
                break;
                
            case 2:
                ShowGuidanceMessage("3단계: 아이템 장착. 장비 아이템을 장착하고 플레이어 스탯 변화를 확인해보세요.");
                break;
                
            case 3:
                ShowGuidanceMessage("4단계: 아이템 사용. 소모품 아이템을 사용하고 효과를 확인해보세요.");
                break;
                
            case 4:
                ShowGuidanceMessage("5단계: 아이템 조합. 재료 아이템을 조합하여 새로운 아이템을 만들어보세요.");
                break;
                
            case 5:
                ShowGuidanceMessage("6단계: 아이템 강화. 장비 아이템을 강화하고 효과를 확인해보세요.");
                break;
                
            case 6:
                ShowGuidanceMessage("아이템 및 인벤토리 데모 완료. '완료' 버튼을 클릭하여 데모를 종료하세요.");
                break;
        }
    }

    #endregion
```
