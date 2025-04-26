# 시스템 통합 예제 - Part 10

## 설정 관리자 클래스 (계속)

```csharp
    /// <summary>
    /// 오디오 설정 적용
    /// </summary>
    private void ApplyAudioSettings()
    {
        // AudioManager에 볼륨 설정 적용
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(_settings.MasterVolume);
            AudioManager.Instance.SetMusicVolume(_settings.MusicVolume);
            AudioManager.Instance.SetSfxVolume(_settings.SfxVolume);
            AudioManager.Instance.SetMuteWhenInBackground(_settings.MuteWhenInBackground);
        }
        
        // 이벤트 발생
        OnMasterVolumeChanged?.Invoke(_settings.MasterVolume);
        OnMusicVolumeChanged?.Invoke(_settings.MusicVolume);
        OnSfxVolumeChanged?.Invoke(_settings.SfxVolume);
    }
    
    /// <summary>
    /// 그래픽 설정 적용
    /// </summary>
    private void ApplyGraphicsSettings()
    {
        // 품질 레벨 설정
        if (_settings.QualityLevel >= 0 && _settings.QualityLevel < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(_settings.QualityLevel, true);
            OnQualityLevelChanged?.Invoke(_settings.QualityLevel);
        }
        
        // 해상도 설정
        if (_settings.Resolution >= 0 && _settings.Resolution < _availableResolutions.Length)
        {
            Resolution resolution = _availableResolutions[_settings.Resolution];
            Screen.SetResolution(resolution.width, resolution.height, _settings.FullScreen);
            OnResolutionChanged?.Invoke(_settings.Resolution);
        }
        
        // 전체 화면 설정
        Screen.fullScreen = _settings.FullScreen;
        OnFullScreenChanged?.Invoke(_settings.FullScreen);
        
        // 수직 동기화 설정
        QualitySettings.vSyncCount = _settings.VSync ? 1 : 0;
        
        // 프레임 레이트 제한 설정
        if (!_settings.VSync && _settings.FrameRateLimit > 0)
        {
            Application.targetFrameRate = _settings.FrameRateLimit;
        }
        else if (!_settings.VSync)
        {
            Application.targetFrameRate = -1; // 무제한
        }
    }
    
    /// <summary>
    /// 게임플레이 설정 적용
    /// </summary>
    private void ApplyGameplaySettings()
    {
        // 마우스 감도 설정
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SetMouseSensitivity(_settings.MouseSensitivity);
            PlayerManager.Instance.SetInvertMouseY(_settings.InvertMouseY);
        }
        
        // FPS 표시 설정
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFps(_settings.ShowFps);
            UIManager.Instance.ShowDamageNumbers(_settings.ShowDamageNumbers);
        }
        
        // 카메라 흔들림 설정
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetCameraShake(_settings.CameraShake);
        }
    }
```
