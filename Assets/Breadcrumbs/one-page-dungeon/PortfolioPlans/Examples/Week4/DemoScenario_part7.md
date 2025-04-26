# 데모 시나리오 구현 예제 - Part 7

## DemoManager 클래스 (계속)

```csharp
    #region 전체 게임 루프 데모

    /// <summary>
    /// 전체 게임 루프 데모 준비
    /// </summary>
    private async UniTask PrepareFullGameLoopDemo()
    {
        // 게임 매니저를 통한 전체 게임 설정
        _gameManager.SetupFullDemoGameplay();
        
        // 모든 시스템 초기화
        _uiManager.InitializeAllUI();
        
        // 가이드 메시지
        ShowGuidanceMessage("전체 게임 루프 데모를 시작합니다. 입장-탐험-전투-탈출로 이어지는 완전한 게임 경험을 체험해보세요.");
    }

    /// <summary>
    /// 전체 게임 루프 데모 단계 실행
    /// </summary>
    private async UniTask RunFullGameLoopStep()
    {
        switch (_currentStepIndex)
        {
            case 0: // 게임 시작 및 매치메이킹
                ShowGuidanceMessage("1단계: 게임 시작 및 매치메이킹. 멀티플레이어 게임을 시작하고, 매치메이킹 과정을 확인해보세요.");
                await _gameManager.StartDemoMultiplayerGame();
                OnDemoStepChanged?.Invoke(_currentStepIndex, "게임 시작 및 매치메이킹");
                break;
                
            case 1: // 던전 생성 및 입장
                ShowGuidanceMessage("2단계: 던전 생성 및 입장. 프로시저럴 생성된 던전으로 입장하고 초기 목표를 확인해보세요.");
                _gameManager.ChangeState(GameState.DungeonEntry);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "던전 생성 및 입장");
                break;
                
            case 2: // 던전 탐험
                ShowGuidanceMessage("3단계: 던전 탐험. 던전을 탐험하며 문을 열고, 트랩을 피하며, 숨겨진 보물을 찾아보세요.");
                _gameManager.ChangeState(GameState.Exploration);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "던전 탐험");
                break;
                
            case 3: // 전투 시작
                ShowGuidanceMessage("4단계: 전투. 적과 전투하며 협력과 전략을 활용해보세요.");
                await _dungeonManager.TriggerCombatEvent();
                _gameManager.ChangeState(GameState.Combat);
                OnDemoStepChanged?.Invoke(_currentStepIndex, "전투 시작");
                break;
```
