# 시스템 통합 예제 - Part 12

## 설정 관리자 클래스 (계속)

```csharp
    /// <summary>
    /// 해상도 설정
    /// </summary>
    public void SetResolution(int index)
    {
        if (index >= 0 && index < _availableResolutions.Length)
        {
            _settings.Resolution = index;
            Resolution resolution = _availableResolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, _settings.FullScreen);
            OnResolutionChanged?.Invoke(_settings.Resolution);
            SaveSettings();
        }
    }
    
    /// <summary>
    /// 전체 화면 설정
    /// </summary>
    public void SetFullScreen(bool fullScreen)
    {
        _settings.FullScreen = fullScreen;
        Screen.fullScreen = _settings.FullScreen;
        OnFullScreenChanged?.Invoke(_settings.FullScreen);
        SaveSettings();
    }
    
    /// <summary>
    /// 수직 동기화 설정
    /// </summary>
    public void SetVSync(bool vSync)
    {
        _settings.VSync = vSync;
        QualitySettings.vSyncCount = _settings.VSync ? 1 : 0;
        
        // 프레임 레이트 제한 적용
        if (!_settings.VSync && _settings.FrameRateLimit > 0)
        {
            Application.targetFrameRate = _settings.FrameRateLimit;
        }
        else if (!_settings.VSync)
        {
            Application.targetFrameRate = -1; // 무제한
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 프레임 레이트 제한 설정
    /// </summary>
    public void SetFrameRateLimit(int limit)
    {
        _settings.FrameRateLimit = limit;
        
        if (!_settings.VSync && _settings.FrameRateLimit > 0)
        {
            Application.targetFrameRate = _settings.FrameRateLimit;
        }
        else if (!_settings.VSync)
        {
            Application.targetFrameRate = -1; // 무제한
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 마우스 감도 설정
    /// </summary>
    public void SetMouseSensitivity(float sensitivity)
    {
        _settings.MouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 2.0f);
        
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SetMouseSensitivity(_settings.MouseSensitivity);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// Y축 반전 설정
    /// </summary>
    public void SetInvertMouseY(bool invert)
    {
        _settings.InvertMouseY = invert;
        
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SetInvertMouseY(_settings.InvertMouseY);
        }
        
        SaveSettings();
    }
```
