# 시스템 통합 예제 - Part 11

## 설정 관리자 클래스 (계속)

```csharp
    /// <summary>
    /// UI 설정 적용
    /// </summary>
    private void ApplyUISettings()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetUIScale(_settings.UiScale);
            UIManager.Instance.SetShowTutorials(_settings.ShowTutorials);
            UIManager.Instance.SetMinimalUI(_settings.MinimalUi);
        }
    }
    
    /// <summary>
    /// 접근성 설정 적용
    /// </summary>
    private void ApplyAccessibilitySettings()
    {
        if (AccessibilityManager.Instance != null)
        {
            AccessibilityManager.Instance.SetColorBlindMode(_settings.ColorBlindMode, _settings.ColorBlindType);
            AccessibilityManager.Instance.SetReduceMotion(_settings.ReduceMotion);
            AccessibilityManager.Instance.SetLargeText(_settings.LargeText);
        }
    }
    
    #region 설정 변경 메서드
    
    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _settings.MasterVolume = Mathf.Clamp01(volume);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(_settings.MasterVolume);
        }
        OnMasterVolumeChanged?.Invoke(_settings.MasterVolume);
        SaveSettings();
    }
    
    /// <summary>
    /// 음악 볼륨 설정
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        _settings.MusicVolume = Mathf.Clamp01(volume);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(_settings.MusicVolume);
        }
        OnMusicVolumeChanged?.Invoke(_settings.MusicVolume);
        SaveSettings();
    }
    
    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        _settings.SfxVolume = Mathf.Clamp01(volume);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(_settings.SfxVolume);
        }
        OnSfxVolumeChanged?.Invoke(_settings.SfxVolume);
        SaveSettings();
    }
    
    /// <summary>
    /// 품질 레벨 설정
    /// </summary>
    public void SetQualityLevel(int level)
    {
        if (level >= 0 && level < QualitySettings.names.Length)
        {
            _settings.QualityLevel = level;
            QualitySettings.SetQualityLevel(_settings.QualityLevel, true);
            OnQualityLevelChanged?.Invoke(_settings.QualityLevel);
            SaveSettings();
        }
    }
```
