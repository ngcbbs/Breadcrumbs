# 시스템 통합 예제 - Part 13

## 설정 관리자 클래스 (계속)

```csharp
    /// <summary>
    /// FPS 표시 설정
    /// </summary>
    public void SetShowFps(bool show)
    {
        _settings.ShowFps = show;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFps(_settings.ShowFps);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 데미지 숫자 표시 설정
    /// </summary>
    public void SetShowDamageNumbers(bool show)
    {
        _settings.ShowDamageNumbers = show;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDamageNumbers(_settings.ShowDamageNumbers);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 카메라 흔들림 설정
    /// </summary>
    public void SetCameraShake(bool enable)
    {
        _settings.CameraShake = enable;
        
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetCameraShake(_settings.CameraShake);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// UI 크기 설정
    /// </summary>
    public void SetUIScale(float scale)
    {
        _settings.UiScale = Mathf.Clamp(scale, 0.5f, 2.0f);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetUIScale(_settings.UiScale);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 튜토리얼 표시 설정
    /// </summary>
    public void SetShowTutorials(bool show)
    {
        _settings.ShowTutorials = show;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetShowTutorials(_settings.ShowTutorials);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 최소 UI 설정
    /// </summary>
    public void SetMinimalUI(bool minimal)
    {
        _settings.MinimalUi = minimal;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetMinimalUI(_settings.MinimalUi);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 색맹 모드 설정
    /// </summary>
    public void SetColorBlindMode(bool enable, int type = 0)
    {
        _settings.ColorBlindMode = enable;
        
        if (enable)
        {
            _settings.ColorBlindType = Mathf.Clamp(type, 0, 3);
        }
        
        if (AccessibilityManager.Instance != null)
        {
            AccessibilityManager.Instance.SetColorBlindMode(_settings.ColorBlindMode, _settings.ColorBlindType);
        }
        
        SaveSettings();
    }
```
