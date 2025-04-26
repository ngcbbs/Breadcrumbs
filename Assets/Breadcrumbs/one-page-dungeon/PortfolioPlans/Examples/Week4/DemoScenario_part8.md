# 데모 시나리오 구현 예제 - Part 8

## DemoManager 클래스 (계속)

```csharp
            case 4: // 전리품 획득
                ShowGuidanceMessage("5단계: 전리품 획득. 전투 후 획득한 전리품을 확인하고 수집해보세요.");
                _gameManager.ChangeState(GameState.Loot);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "전리품 획득");
                break;
                
            case 5: // 보스 전투
                ShowGuidanceMessage("6단계: 보스 전투. 던전의 최종 보스에 맞서 모든 기술을 활용해 싸워보세요.");
                await _dungeonManager.TriggerBossCombatEvent();
                _gameManager.ChangeState(GameState.Combat);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "보스 전투");
                break;
                
            case 6: // 던전 탈출
                ShowGuidanceMessage("7단계: 던전 탈출. 모든 목표를 달성하고 던전을 탈출해보세요.");
                _gameManager.ChangeState(GameState.Exit);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "던전 탈출");
                break;
                
            case 7: // 전체 경험 결과
                ShowGuidanceMessage("전체 게임 루프 데모가 완료되었습니다. 게임의 최종 결과를 확인해보세요.");
                _uiManager.ShowGameSummaryUI();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "전체 경험 결과");
                
                // 데모 종료
                CompleteDemo();
                break;
        }
    }

    /// <summary>
    /// 전체 게임 루프 데모의 현재 단계 가이드 메시지
    /// </summary>
    private void ShowFullGameLoopGuidance()
    {
        switch (_currentStepIndex)
        {
            case 0:
                ShowGuidanceMessage("1단계: 게임 시작 및 매치메이킹. 멀티플레이어 게임을 시작하고, 매치메이킹 과정을 확인해보세요.");
                break;
                
            case 1:
                ShowGuidanceMessage("2단계: 던전 생성 및 입장. 프로시저럴 생성된 던전으로 입장하고 초기 목표를 확인해보세요.");
                break;
                
            case 2:
                ShowGuidanceMessage("3단계: 던전 탐험. 던전을 탐험하며 문을 열고, 트랩을 피하며, 숨겨진 보물을 찾아보세요.");
                break;
                
            case 3:
                ShowGuidanceMessage("4단계: 전투. 적과 전투하며 협력과 전략을 활용해보세요.");
                break;
                
            case 4:
                ShowGuidanceMessage("5단계: 전리품 획득. 전투 후 획득한 전리품을 확인하고 수집해보세요.");
                break;
                
            case 5:
                ShowGuidanceMessage("6단계: 보스 전투. 던전의 최종 보스에 맞서 모든 기술을 활용해 싸워보세요.");
                break;
                
            case 6:
                ShowGuidanceMessage("7단계: 던전 탈출. 모든 목표를 달성하고 던전을 탈출해보세요.");
                break;
                
            case 7:
                ShowGuidanceMessage("전체 게임 루프 데모 완료. '완료' 버튼을 클릭하여 데모를 종료하세요.");
                break;
        }
    }

    #endregion
```
