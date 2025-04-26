# 시스템 통합 예제 - Part 14

## 설정 관리자 클래스 (계속)

```csharp
    /// <summary>
    /// 움직임 감소 설정
    /// </summary>
    public void SetReduceMotion(bool reduce)
    {
        _settings.ReduceMotion = reduce;
        
        if (AccessibilityManager.Instance != null)
        {
            AccessibilityManager.Instance.SetReduceMotion(_settings.ReduceMotion);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 큰 텍스트 설정
    /// </summary>
    public void SetLargeText(bool large)
    {
        _settings.LargeText = large;
        
        if (AccessibilityManager.Instance != null)
        {
            AccessibilityManager.Instance.SetLargeText(_settings.LargeText);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 지역 설정
    /// </summary>
    public void SetRegionPreference(string region)
    {
        _settings.RegionPreference = region;
        
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SetRegionPreference(_settings.RegionPreference);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 네트워크 품질 설정
    /// </summary>
    public void SetNetworkQuality(int quality)
    {
        _settings.NetworkQuality = Mathf.Clamp(quality, 0, 2);
        
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SetNetworkQuality(_settings.NetworkQuality);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 음성 채팅 활성화 설정
    /// </summary>
    public void SetEnableVoiceChat(bool enable)
    {
        _settings.EnableVoiceChat = enable;
        
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SetEnableVoiceChat(_settings.EnableVoiceChat);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 음성 채팅 볼륨 설정
    /// </summary>
    public void SetVoiceChatVolume(float volume)
    {
        _settings.VoiceChatVolume = Mathf.Clamp01(volume);
        
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SetVoiceChatVolume(_settings.VoiceChatVolume);
        }
        
        SaveSettings();
    }
    
    /// <summary>
    /// 기본 설정으로 리셋
    /// </summary>
    public void ResetToDefaults()
    {
        _settings.ResetToDefaults();
        ApplySettings();
        SaveSettings();
    }
    
    #endregion
}
```
