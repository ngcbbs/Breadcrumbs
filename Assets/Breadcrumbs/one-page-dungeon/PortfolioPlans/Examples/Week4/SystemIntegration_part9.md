# 시스템 통합 예제 - Part 9

## 설정 관리자 클래스

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 설정 관리자 클래스
/// 게임 설정을 관리하고 다른 시스템에 변경 사항을 알려줍니다.
/// </summary>
public class SettingsManager : MonoBehaviour, ISystemInitializer
{
    private static SettingsManager _instance;
    public static SettingsManager Instance => _instance;
    
    // 현재 게임 설정
    private GameSettings _settings;
    public GameSettings Settings => _settings;
    
    // 이벤트
    public event Action<GameSettings> OnSettingsChanged;
    public event Action<float> OnMasterVolumeChanged;
    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnSfxVolumeChanged;
    public event Action<int> OnQualityLevelChanged;
    public event Action<bool> OnFullScreenChanged;
    public event Action<int> OnResolutionChanged;
    
    // 사용 가능한 해상도 목록
    private Resolution[] _availableResolutions;
    public Resolution[] AvailableResolutions => _availableResolutions;
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 사용 가능한 해상도 가져오기
        _availableResolutions = Screen.resolutions;
    }
    
    /// <summary>
    /// 시스템 초기화
    /// </summary>
    public async UniTask Initialize(CancellationToken cancellationToken = default)
    {
        // 설정 로드
        _settings = GameSettings.LoadFromPlayerPrefs();
        
        // 설정 적용
        ApplySettings();
        
        await UniTask.CompletedTask;
    }
    
    /// <summary>
    /// 시스템 종료
    /// </summary>
    public void Shutdown()
    {
        // 설정 저장
        SaveSettings();
    }
    
    /// <summary>
    /// 설정 저장
    /// </summary>
    public void SaveSettings()
    {
        _settings.SaveToPlayerPrefs();
    }
    
    /// <summary>
    /// 설정 적용
    /// </summary>
    public void ApplySettings()
    {
        // 오디오 설정 적용
        ApplyAudioSettings();
        
        // 그래픽 설정 적용
        ApplyGraphicsSettings();
        
        // 게임플레이 설정 적용
        ApplyGameplaySettings();
        
        // UI 설정 적용
        ApplyUISettings();
        
        // 접근성 설정 적용
        ApplyAccessibilitySettings();
        
        // 이벤트 발생
        OnSettingsChanged?.Invoke(_settings);
    }
```
