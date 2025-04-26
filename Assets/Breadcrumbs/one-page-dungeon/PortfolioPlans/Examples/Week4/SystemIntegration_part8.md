# 시스템 통합 예제 - Part 8

## 게임 설정 및 데이터 클래스

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 설정 관리 클래스
/// 게임의 다양한 설정을 관리하고 JSON으로 직렬화/역직렬화할 수 있습니다.
/// </summary>
[Serializable]
public class GameSettings
{
    // 오디오 설정
    public float MasterVolume = 1.0f;
    public float MusicVolume = 0.8f;
    public float SfxVolume = 1.0f;
    public bool MuteWhenInBackground = true;
    
    // 그래픽 설정
    public int QualityLevel = 2;
    public bool FullScreen = true;
    public int Resolution = 1; // 인덱스
    public bool VSync = true;
    public int FrameRateLimit = 60;
    
    // 게임플레이 설정
    public float MouseSensitivity = 1.0f;
    public bool InvertMouseY = false;
    public bool ShowFps = false;
    public bool ShowDamageNumbers = true;
    public bool CameraShake = true;
    
    // 네트워크 설정
    public string RegionPreference = "auto";
    public int NetworkQuality = 1; // 0: Low, 1: Medium, 2: High
    public bool EnableVoiceChat = true;
    public float VoiceChatVolume = 1.0f;
    
    // UI 설정
    public float UiScale = 1.0f;
    public bool ShowTutorials = true;
    public bool MinimalUi = false;
    
    // 접근성 설정
    public bool ColorBlindMode = false;
    public int ColorBlindType = 0; // 0: None, 1: Protanopia, 2: Deuteranopia, 3: Tritanopia
    public bool ReduceMotion = false;
    public bool LargeText = false;
    
    /// <summary>
    /// 게임 설정을 JSON 문자열로 직렬화
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
    
    /// <summary>
    /// JSON 문자열에서 게임 설정을 역직렬화
    /// </summary>
    public static GameSettings FromJson(string json)
    {
        try
        {
            return JsonUtility.FromJson<GameSettings>(json) ?? new GameSettings();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse game settings: {e.Message}");
            return new GameSettings();
        }
    }
    
    /// <summary>
    /// 설정을 PlayerPrefs에 저장
    /// </summary>
    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetString("GameSettings", ToJson());
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// PlayerPrefs에서 설정을 로드
    /// </summary>
    public static GameSettings LoadFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string json = PlayerPrefs.GetString("GameSettings");
            return FromJson(json);
        }
        
        return new GameSettings();
    }
    
    /// <summary>
    /// 기본 설정으로 리셋
    /// </summary>
    public void ResetToDefaults()
    {
        MasterVolume = 1.0f;
        MusicVolume = 0.8f;
        SfxVolume = 1.0f;
        MuteWhenInBackground = true;
        
        QualityLevel = 2;
        FullScreen = true;
        Resolution = 1;
        VSync = true;
        FrameRateLimit = 60;
        
        MouseSensitivity = 1.0f;
        InvertMouseY = false;
        ShowFps = false;
        ShowDamageNumbers = true;
        CameraShake = true;
        
        RegionPreference = "auto";
        NetworkQuality = 1;
        EnableVoiceChat = true;
        VoiceChatVolume = 1.0f;
        
        UiScale = 1.0f;
        ShowTutorials = true;
        MinimalUi = false;
        
        ColorBlindMode = false;
        ColorBlindType = 0;
        ReduceMotion = false;
        LargeText = false;
    }
}
```
