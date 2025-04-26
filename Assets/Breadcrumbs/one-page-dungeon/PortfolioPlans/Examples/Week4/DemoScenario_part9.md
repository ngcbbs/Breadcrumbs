# 데모 시나리오 구현 예제 - Part 9

## DemoManager 클래스 (계속)

```csharp
    /// <summary>
    /// 데모 완료 처리
    /// </summary>
    private void CompleteDemo()
    {
        // 데모 완료 이벤트 발생
        OnDemoCompleted?.Invoke();
        
        // 데모 완료 UI 표시
        _uiManager.ShowDemoCompletedUI(_currentScenario.ToString());
        
        // 데모 실행 중 상태 변경
        _isDemoRunning = false;
    }
}

/// <summary>
/// 데모 UI 관리를 위한 확장 클래스
/// </summary>
public static class DemoUIExtensions
{
    /// <summary>
    /// 데모 UI 표시
    /// </summary>
    public static void ShowDemoUI(this UIManager uiManager, string demoName)
    {
        uiManager.ShowDemoPanel();
        uiManager.SetDemoTitle(demoName);
    }
    
    /// <summary>
    /// 데모 가이드 UI 표시
    /// </summary>
    public static void ShowDemoGuidanceUI(this UIManager uiManager, string message)
    {
        uiManager.ShowGuidancePanel();
        uiManager.SetGuidanceText(message);
    }
    
    /// <summary>
    /// 데모 가이드 UI 페이드아웃
    /// </summary>
    public static void FadeOutDemoGuidanceUI(this UIManager uiManager)
    {
        uiManager.FadeOutGuidancePanel();
    }
    
    /// <summary>
    /// 데모 가이드 UI 숨김
    /// </summary>
    public static void HideDemoGuidanceUI(this UIManager uiManager)
    {
        uiManager.HideGuidancePanel();
    }
    
    /// <summary>
    /// 데모 일시정지 UI 표시
    /// </summary>
    public static void ShowDemoPauseUI(this UIManager uiManager)
    {
        uiManager.ShowPausePanel();
    }
    
    /// <summary>
    /// 데모 일시정지 UI 숨김
    /// </summary>
    public static void HideDemoPauseUI(this UIManager uiManager)
    {
        uiManager.HidePausePanel();
    }
    
    /// <summary>
    /// 데모 완료 UI 표시
    /// </summary>
    public static void ShowDemoCompletedUI(this UIManager uiManager, string demoName)
    {
        uiManager.ShowCompletionPanel();
        uiManager.SetCompletionText($"{demoName} 데모가 완료되었습니다.");
    }
}
```
