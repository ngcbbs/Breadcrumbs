# 데모 시나리오 구현 예제 - Part 5

## DemoManager 클래스 (계속)

```csharp
    #region 전투 시스템 데모

    /// <summary>
    /// 전투 시스템 데모 준비
    /// </summary>
    private async UniTask PrepareCombatDemo()
    {
        // 전투 아레나 생성
        await _dungeonManager.GenerateCombatArena();
        
        // 플레이어 스폰
        await _playerManager.SpawnPlayerForCombatDemo();
        
        // 스킬 및 능력치 설정
        _playerManager.SetupDemoCombatSkills();
        
        // 적 AI 스폰 준비
        await _dungeonManager.PrepareEnemySpawner();
        
        // 가이드 메시지
        ShowGuidanceMessage("전투 시스템 데모를 시작합니다. 다양한 전투 요소와 스킬을 시험해볼 수 있습니다.");
    }

    /// <summary>
    /// 전투 시스템 데모 단계 실행
    /// </summary>
    private async UniTask RunCombatSystemStep()
    {
        switch (_currentStepIndex)
        {
            case 0: // 기본 공격
                ShowGuidanceMessage("1단계: 기본 공격. 마우스 왼쪽 버튼으로 기본 공격을 수행해보세요.");
                await _dungeonManager.SpawnBasicEnemiesForDemo(2);
                _playerManager.HighlightBasicAttackControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "기본 공격");
                break;
                
            case 1: // 방어 및 회피
                ShowGuidanceMessage("2단계: 방어 및 회피. 마우스 오른쪽 버튼으로 방어하고, Space + 방향키로 회피해보세요.");
                await _dungeonManager.SpawnRangedEnemiesForDemo(2);
                _playerManager.HighlightDefenseControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "방어 및 회피");
                break;
                
            case 2: // 스킬 사용
                ShowGuidanceMessage("3단계: 스킬 사용. 1, 2, 3 키를 눌러 다양한 스킬을 사용해보세요.");
                _playerManager.HighlightSkillsUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "스킬 사용");
                break;
                
            case 3: // 연계 공격
                ShowGuidanceMessage("4단계: 연계 공격. 기본 공격과 스킬을 연계하여 콤보를 수행해보세요.");
                _playerManager.ShowComboCombatGuide();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "연계 공격");
                break;
                
            case 4: // 상태 효과
                ShowGuidanceMessage("5단계: 상태 효과. 다양한 버프와 디버프를 적용하고 그 효과를 확인해보세요.");
                await _dungeonManager.SpawnSpecialEnemiesForDemo(1);
                _playerManager.HighlightStatusEffectsUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "상태 효과");
                break;
                
            case 5: // 보스전
                ShowGuidanceMessage("6단계: 보스전. 모든 전투 요소를 활용하여 보스를 처치해보세요.");
                await _dungeonManager.SpawnBossEnemyForDemo();
                _playerManager.HighlightAllCombatControls();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "보스전");
                break;
                
            case 6: // 전투 결과
                ShowGuidanceMessage("전투 시스템 데모가 완료되었습니다. 전투 결과를 확인해보세요.");
                _playerManager.ShowCombatResultsUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "전투 결과");
                
                // 데모 종료
                CompleteDemo();
                break;
        }
    }

    /// <summary>
    /// 전투 시스템 데모의 현재 단계 가이드 메시지
    /// </summary>
    private void ShowCombatGuidance()
    {
        switch (_currentStepIndex)
        {
            case 0:
                ShowGuidanceMessage("1단계: 기본 공격. 마우스 왼쪽 버튼으로 기본 공격을 수행해보세요.");
                break;
                
            case 1:
                ShowGuidanceMessage("2단계: 방어 및 회피. 마우스 오른쪽 버튼으로 방어하고, Space + 방향키로 회피해보세요.");
                break;
                
            case 2:
                ShowGuidanceMessage("3단계: 스킬 사용. 1, 2, 3 키를 눌러 다양한 스킬을 사용해보세요.");
                break;
                
            case 3:
                ShowGuidanceMessage("4단계: 연계 공격. 기본 공격과 스킬을 연계하여 콤보를 수행해보세요.");
                break;
                
            case 4:
                ShowGuidanceMessage("5단계: 상태 효과. 다양한 버프와 디버프를 적용하고 그 효과를 확인해보세요.");
                break;
                
            case 5:
                ShowGuidanceMessage("6단계: 보스전. 모든 전투 요소를 활용하여 보스를 처치해보세요.");
                break;
                
            case 6:
                ShowGuidanceMessage("전투 시스템 데모 완료. '완료' 버튼을 클릭하여 데모를 종료하세요.");
                break;
        }
    }

    #endregion
```
