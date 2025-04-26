# 피드백 시스템 예제 코드 (Part 3)

## AudioFeedbackManager.cs (계속)

```csharp
    /// <summary>
    /// 전투 사운드 재생
    /// </summary>
    public void PlayCombatSound(CombatSoundType soundType, float volumeScale = 1f, float pitchVariation = 0.1f)
    {
        AudioClip clip = null;
        
        // 사운드 타입에 따른 클립 선택
        switch (soundType)
        {
            case CombatSoundType.MeleeAttack:
                clip = GetRandomClip(_meleeSounds);
                break;
            case CombatSoundType.MagicAttack:
                clip = GetRandomClip(_magicSounds);
                break;
            case CombatSoundType.Hit:
                clip = GetRandomClip(_hitSounds);
                break;
            case CombatSoundType.CriticalHit:
                clip = GetRandomClip(_criticalHitSounds);
                break;
            case CombatSoundType.EnemyHit:
                clip = GetRandomClip(_enemyHitSounds);
                break;
            case CombatSoundType.EnemyDeath:
                clip = GetRandomClip(_enemyDeathSounds);
                break;
            case CombatSoundType.PlayerDamage:
                clip = GetRandomClip(_playerDamageSounds);
                break;
            case CombatSoundType.Heal:
                clip = GetRandomClip(_healSounds);
                break;
        }
        
        if (clip != null)
        {
            // 풀에서 오디오 소스 가져오기
            AudioSource source = GetAudioSourceFromPool(AudioType.SFX);
            if (source != null)
            {
                // 피치 변화 적용
                source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                source.volume = _sfxVolume * _masterVolume * volumeScale;
                source.PlayOneShot(clip);
            }
        }
    }
    
    /// <summary>
    /// UI 사운드 재생
    /// </summary>
    public void PlayUISound(UISoundType soundType)
    {
        AudioClip clip = null;
        
        // 사운드 타입에 따른 클립 선택
        switch (soundType)
        {
            case UISoundType.ButtonClick:
                clip = _buttonClickSound;
                break;
            case UISoundType.ButtonHover:
                clip = _buttonHoverSound;
                break;
            case UISoundType.MenuOpen:
                clip = _menuOpenSound;
                break;
            case UISoundType.MenuClose:
                clip = _menuCloseSound;
                break;
            case UISoundType.Error:
                clip = _errorSound;
                break;
            case UISoundType.Success:
                clip = _successSound;
                break;
            case UISoundType.InventoryOpen:
                clip = _inventoryOpenSound;
                break;
            case UISoundType.ItemPickup:
                clip = _itemPickupSound;
                break;
            case UISoundType.ItemEquip:
                clip = _itemEquipSound;
                break;
        }
        
        if (clip != null)
        {
            // UI 사운드 재생
            _uiSource.PlayOneShot(clip, _uiVolume * _masterVolume);
        }
    }
    
    /// <summary>
    /// 환경 사운드 재생
    /// </summary>
    public void PlayEnvironmentSound(EnvironmentSoundType soundType, Vector3 position = default)
    {
        AudioClip clip = null;
        
        // 사운드 타입에 따른 클립 선택
        switch (soundType)
        {
            case EnvironmentSoundType.DoorOpen:
                clip = _doorOpenSound;
                break;
            case EnvironmentSoundType.DoorClose:
                clip = _doorCloseSound;
                break;
            case EnvironmentSoundType.ChestOpen:
                clip = _chestOpenSound;
                break;
            case EnvironmentSoundType.TrapTrigger:
                clip = _trapTriggerSound;
                break;
            case EnvironmentSoundType.Teleport:
                clip = _teleportSound;
                break;
        }
        
        if (clip != null)
        {
            // 3D 위치 설정 필요 여부 확인
            if (position != default)
            {
                // 풀에서 오디오 소스 가져오기
                AudioSource source = GetAudioSourceFromPool(AudioType.SFX);
                if (source != null)
                {
                    source.spatialBlend = 1f; // 3D 사운드
                    source.rolloffMode = AudioRolloffMode.Linear;
                    source.minDistance = 1f;
                    source.maxDistance = 20f;
                    source.transform.position = position;
                    source.PlayOneShot(clip, _sfxVolume * _masterVolume);
                }
            }
            else
            {
                // 2D 사운드로 재생
                _sfxSource.PlayOneShot(clip, _sfxVolume * _masterVolume);
            }
        }
    }
    
    /// <summary>
    /// 환경음 변경
    /// </summary>
    public void ChangeAmbientSound(AmbientSoundType ambientType, float fadeTime = 2f)
    {
        AudioClip newAmbient = null;
        
        // 환경음 타입에 따른 클립 선택
        switch (ambientType)
        {
            case AmbientSoundType.Dungeon:
                newAmbient = _dungeonAmbientLoop;
                break;
            case AmbientSoundType.Battle:
                newAmbient = _battleAmbientLoop;
                break;
            case AmbientSoundType.Menu:
                newAmbient = _menuAmbientLoop;
                break;
        }
        
        // 동일한 환경음이면 무시
        if (newAmbient == _currentAmbient)
            return;
            
        _currentAmbient = newAmbient;
        
        // 페이드 처리
        FadeAmbientSound(newAmbient, fadeTime);
    }
    
    /// <summary>
    /// 음악 변경
    /// </summary>
    public void ChangeMusic(MusicType musicType, float fadeTime = 2f)
    {
        AudioClip newMusic = null;
        
        // 음악 타입에 따른 클립 선택
        switch (musicType)
        {
            case MusicType.Menu:
                newMusic = _menuMusic;
                break;
            case MusicType.Exploration:
                newMusic = _explorationMusic;
                break;
            case MusicType.Combat:
                newMusic = _combatMusic;
                break;
            case MusicType.Boss:
                newMusic = _bossMusic;
                break;
            case MusicType.Victory:
                newMusic = _victoryMusic;
                break;
            case MusicType.Defeat:
                newMusic = _defeatMusic;
                break;
        }
        
        // 동일한 음악이면 무시
        if (newMusic == _currentMusic)
            return;
            
        _currentMusic = newMusic;
        
        // 페이드 처리
        FadeMusicTrack(newMusic, fadeTime);
    }
    
    /// <summary>
    /// 볼륨 설정
    /// </summary>
    public void SetVolume(AudioVolumeType volumeType, float volume)
    {
        volume = Mathf.Clamp01(volume);
        
        switch (volumeType)
        {
            case AudioVolumeType.Master:
                _masterVolume = volume;
                UpdateAllVolumes();
                break;
            case AudioVolumeType.SFX:
                _sfxVolume = volume;
                UpdateSFXVolume();
                break;
            case AudioVolumeType.Music:
                _musicVolume = volume;
                UpdateMusicVolume();
                break;
            case AudioVolumeType.UI:
                _uiVolume = volume;
                UpdateUIVolume();
                break;
            case AudioVolumeType.Ambient:
                _ambientVolume = volume;
                UpdateAmbientVolume();
                break;
        }
    }
    
    /// <summary>
    /// 모든 볼륨 업데이트
    /// </summary>
    private void UpdateAllVolumes()
    {
        UpdateSFXVolume();
        UpdateMusicVolume();
        UpdateUIVolume();
        UpdateAmbientVolume();
    }
    
    /// <summary>
    /// SFX 볼륨 업데이트
    /// </summary>
    private void UpdateSFXVolume()
    {
        if (_sfxSource != null)
            _sfxSource.volume = _sfxVolume * _masterVolume;
            
        foreach (var source in _audioSourcePool[AudioType.SFX])
        {
            source.volume = _sfxVolume * _masterVolume;
        }
    }
    
    /// <summary>
    /// 음악 볼륨 업데이트
    /// </summary>
    private void UpdateMusicVolume()
    {
        if (_musicSource != null && !_isMusicFading)
            _musicSource.volume = _musicVolume * _masterVolume;
    }
    
    /// <summary>
    /// UI 볼륨 업데이트
    /// </summary>
    private void UpdateUIVolume()
    {
        if (_uiSource != null)
            _uiSource.volume = _uiVolume * _masterVolume;
            
        foreach (var source in _audioSourcePool[AudioType.UI])
        {
            source.volume = _uiVolume * _masterVolume;
        }
    }
    
    /// <summary>
    /// 환경음 볼륨 업데이트
    /// </summary>
    private void UpdateAmbientVolume()
    {
        if (_ambientSource != null)
            _ambientSource.volume = _ambientVolume * _masterVolume;
    }
    
    /// <summary>
    /// 환경음 페이드 처리
    /// </summary>
    private async void FadeAmbientSound(AudioClip newClip, float fadeTime)
    {
        if (_ambientSource == null)
            return;
            
        // 현재 재생 중인 환경음 페이드 아웃
        float startVolume = _ambientSource.volume;
        float time = 0;
        
        while (time < fadeTime * 0.5f)
        {
            time += Time.deltaTime;
            _ambientSource.volume = Mathf.Lerp(startVolume, 0f, time / (fadeTime * 0.5f));
            await System.Threading.Tasks.Task.Yield();
        }
        
        // 새 환경음 설정 및 재생
        _ambientSource.clip = newClip;
        
        if (newClip != null)
        {
            _ambientSource.Play();
            
            // 새 환경음 페이드 인
            time = 0;
            while (time < fadeTime * 0.5f)
            {
                time += Time.deltaTime;
                _ambientSource.volume = Mathf.Lerp(0f, _ambientVolume * _masterVolume, time / (fadeTime * 0.5f));
                await System.Threading.Tasks.Task.Yield();
            }
            
            _ambientSource.volume = _ambientVolume * _masterVolume;
        }
    }
    
    /// <summary>
    /// 음악 페이드 처리
    /// </summary>
    private void FadeMusicTrack(AudioClip newClip, float fadeTime)
    {
        if (_musicSource == null)
            return;
            
        // 현재 페이드 중이면 초기화
        _isMusicFading = true;
        _musicFadeTime = fadeTime;
        _currentMusicFadeTime = 0f;
        _startMusicVolume = _musicSource.volume;
        _targetMusicVolume = 0f; // 먼저 페이드 아웃
        
        // 새 트랙 설정을 위한 델리게이트 설정
        StartCoroutine(ChangeMusicAfterFadeOut(newClip, fadeTime));
    }
    
    /// <summary>
    /// 페이드 아웃 후 음악 변경
    /// </summary>
    private System.Collections.IEnumerator ChangeMusicAfterFadeOut(AudioClip newClip, float fadeTime)
    {
        // 페이드 아웃 대기
        yield return new WaitForSeconds(fadeTime * 0.5f);
        
        // 새 트랙 설정 및 재생
        _musicSource.clip = newClip;
        
        if (newClip != null)
        {
            _musicSource.Play();
            
            // 페이드 인 설정
            _startMusicVolume = 0f;
            _targetMusicVolume = _musicVolume * _masterVolume;
            _currentMusicFadeTime = 0f;
            
            // 페이드 인 대기
            yield return new WaitForSeconds(fadeTime * 0.5f);
        }
        
        // 페이드 완료
        _isMusicFading = false;
        _musicSource.volume = _musicVolume * _masterVolume;
    }
    
    /// <summary>
    /// 음악 페이드 업데이트
    /// </summary>
    private void UpdateMusicFade()
    {
        if (!_isMusicFading || _musicSource == null)
            return;
            
        _currentMusicFadeTime += Time.deltaTime;
        float t = Mathf.Clamp01(_currentMusicFadeTime / (_musicFadeTime * 0.5f));
        
        _musicSource.volume = Mathf.Lerp(_startMusicVolume, _targetMusicVolume, t);
    }
    
    /// <summary>
    /// 랜덤 클립 가져오기
    /// </summary>
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return null;
            
        return clips[Random.Range(0, clips.Length)];
    }
}

/// <summary>
/// 오디오 타입
/// </summary>
public enum AudioType
{
    SFX,
    UI
}

/// <summary>
/// 전투 사운드 타입
/// </summary>
public enum CombatSoundType
{
    MeleeAttack,
    MagicAttack,
    Hit,
    CriticalHit,
    EnemyHit,
    EnemyDeath,
    PlayerDamage,
    Heal
}

/// <summary>
/// UI 사운드 타입
/// </summary>
public enum UISoundType
{
    ButtonClick,
    ButtonHover,
    MenuOpen,
    MenuClose,
    Error,
    Success,
    InventoryOpen,
    ItemPickup,
    ItemEquip
}

/// <summary>
/// 환경 사운드 타입
/// </summary>
public enum EnvironmentSoundType
{
    DoorOpen,
    DoorClose,
    ChestOpen,
    TrapTrigger,
    Teleport
}

/// <summary>
/// 환경음 타입
/// </summary>
public enum AmbientSoundType
{
    Dungeon,
    Battle,
    Menu
}

/// <summary>
/// 음악 타입
/// </summary>
public enum MusicType
{
    Menu,
    Exploration,
    Combat,
    Boss,
    Victory,
    Defeat
}

/// <summary>
/// 볼륨 타입
/// </summary>
public enum AudioVolumeType
{
    Master,
    SFX,
    Music,
    UI,
    Ambient
}
```

## 입력 및 액션 피드백 시스템

### HapticFeedbackManager.cs

```csharp
using UnityEngine;

/// <summary>
/// 컨트롤러 햅틱 피드백 관리 컴포넌트 (게임패드 지원)
/// </summary>
public class HapticFeedbackManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static HapticFeedbackManager Instance { get; private set; }
    
    [Header("Vibration Settings")]
    [SerializeField] private bool _enableVibration = true;
    [SerializeField, Range(0f, 1f)] private float _vibrationIntensity = 0.7f;
    
    [Header("Haptic Presets")]
    [SerializeField] private HapticPreset _lightFeedback;
    [SerializeField] private HapticPreset _mediumFeedback;
    [SerializeField] private HapticPreset _heavyFeedback;
    [SerializeField] private HapticPreset _damageFeedback;
    
    // 진동 제어 변수
    private bool _isVibrating = false;
    private float _vibrationEndTime = 0f;
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 기본 프리셋 초기화
        InitializeDefaultPresets();
    }
    
    private void Update()
    {
        // 진동 타이머 업데이트
        UpdateVibration();
    }
    
    /// <summary>
    /// 기본 프리셋 초기화
    /// </summary>
    private void InitializeDefaultPresets()
    {
        if (_lightFeedback == null)
        {
            _lightFeedback = new HapticPreset
            {
                lowFrequency = 0.2f,
                highFrequency = 0.3f,
                duration = 0.1f
            };
        }
        
        if (_mediumFeedback == null)
        {
            _mediumFeedback = new HapticPreset
            {
                lowFrequency = 0.4f,
                highFrequency = 0.5f,
                duration = 0.2f
            };
        }
        
        if (_heavyFeedback == null)
        {
            _heavyFeedback = new HapticPreset
            {
                lowFrequency = 0.7f,
                highFrequency = 0.8f,
                duration = 0.3f
            };
        }
        
        if (_damageFeedback == null)
        {
            _damageFeedback = new HapticPreset
            {
                lowFrequency = 0.8f,
                highFrequency = 0.6f,
                duration = 0.4f
            };
        }
    }
    
    /// <summary>
    /// 진동 업데이트
    /// </summary>
    private void UpdateVibration()
    {
        if (_isVibrating && Time.time > _vibrationEndTime)
        {
            StopVibration();
        }
    }
    
    /// <summary>
    /// 진동 중지
    /// </summary>
    public void StopVibration()
    {
        // XInput 진동 중지
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (UnityEngine.XR.XRDevice.isPresent)
        {
            StopXRHaptics();
        }
        else
        {
            StopGamepadVibration();
        }
#elif UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
        
        _isVibrating = false;
    }
    
    /// <summary>
    /// 경량 피드백 실행
    /// </summary>
    public void PlayLightFeedback()
    {
        PlayHapticFeedback(_lightFeedback);
    }
    
    /// <summary>
    /// 중간 피드백 실행
    /// </summary>
    public void PlayMediumFeedback()
    {
        PlayHapticFeedback(_mediumFeedback);
    }
    
    /// <summary>
    /// 강한 피드백 실행
    /// </summary>
    public void PlayHeavyFeedback()
    {
        PlayHapticFeedback(_heavyFeedback);
    }
    
    /// <summary>
    /// 데미지 피드백 실행
    /// </summary>
    public void PlayDamageFeedback()
    {
        PlayHapticFeedback(_damageFeedback);
    }
    
    /// <summary>
    /// 커스텀 피드백 실행
    /// </summary>
    public void PlayCustomFeedback(float lowFrequency, float highFrequency, float duration)
    {
        if (!_enableVibration)
            return;
            
        HapticPreset preset = new HapticPreset
        {
            lowFrequency = Mathf.Clamp01(lowFrequency),
            highFrequency = Mathf.Clamp01(highFrequency),
            duration = Mathf.Max(0.05f, duration)
        };
        
        PlayHapticFeedback(preset);
    }
    
    /// <summary>
    /// 햅틱 피드백 실행
    /// </summary>
    private void PlayHapticFeedback(HapticPreset preset)
    {
        if (!_enableVibration || preset == null)
            return;
            
        // 강도 조정
        float lowFreq = preset.lowFrequency * _vibrationIntensity;
        float highFreq = preset.highFrequency * _vibrationIntensity;
        
        // 플랫폼에 따른 햅틱 처리
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (UnityEngine.XR.XRDevice.isPresent)
        {
            PlayXRHaptics(lowFreq, highFreq, preset.duration);
        }
        else
        {
            PlayGamepadVibration(lowFreq, highFreq, preset.duration);
        }
#elif UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
        
        // 진동 타이머 설정
        _isVibrating = true;
        _vibrationEndTime = Time.time + preset.duration;
    }
    
    // 플랫폼 별 구현 메서드 (실제 사용 시 해당 플랫폼의 API로 대체)
    
    /// <summary>
    /// 게임패드 진동 재생
    /// </summary>
    private void PlayGamepadVibration(float lowFreq, float highFreq, float duration)
    {
        // XInput 또는 Unity New Input System 사용 (실제 구현 시 대체)
        Debug.Log($"Gamepad vibration - Low: {lowFreq}, High: {highFreq}, Duration: {duration}");
    }
    
    /// <summary>
    /// XR 컨트롤러 햅틱 재생
    /// </summary>
    private void PlayXRHaptics(float amplitude, float frequency, float duration)
    {
        // XR Input 시스템 사용 (실제 구현 시 대체)
        Debug.Log($"XR haptics - Amplitude: {amplitude}, Frequency: {frequency}, Duration: {duration}");
    }
    
    /// <summary>
    /// 게임패드 진동 중지
    /// </summary>
    private void StopGamepadVibration()
    {
        // 진동 중지 로직 (실제 구현 시 대체)
        Debug.Log("Stopping gamepad vibration");
    }
    
    /// <summary>
    /// XR 컨트롤러 햅틱 중지
    /// </summary>
    private void StopXRHaptics()
    {
        // XR 햅틱 중지 로직 (실제 구현 시 대체)
        Debug.Log("Stopping XR haptics");
    }
    
    /// <summary>
    /// 진동 활성화 설정
    /// </summary>
    public void SetVibrationEnabled(bool enabled)
    {
        _enableVibration = enabled;
        
        if (!enabled && _isVibrating)
        {
            StopVibration();
        }
    }
    
    /// <summary>
    /// 진동 강도 설정
    /// </summary>
    public void SetVibrationIntensity(float intensity)
    {
        _vibrationIntensity = Mathf.Clamp01(intensity);
    }
}

/// <summary>
/// 햅틱 프리셋 정의
/// </summary>
[System.Serializable]
public class HapticPreset
{
    public float lowFrequency = 0.5f;
    public float highFrequency = 0.5f;
    public float duration = 0.2f;
}
```

## 피드백 시스템 통합 예제

```csharp
// 공격 시 피드백 예제
public void OnPlayerAttack(bool isCritical)
{
    // 시각적 피드백
    VisualFeedbackManager.Instance.ShowAttackEffect(playerWeapon.gameObject);
    
    // 청각적 피드백
    if (isCritical) {
        AudioFeedbackManager.Instance.PlayCombatSound(CombatSoundType.CriticalHit);
    } else {
        AudioFeedbackManager.Instance.PlayCombatSound(CombatSoundType.MeleeAttack);
    }
    
    // 햅틱 피드백
    if (isCritical) {
        HapticFeedbackManager.Instance.PlayHeavyFeedback();
    } else {
        HapticFeedbackManager.Instance.PlayMediumFeedback();
    }
}

// 피해 시 피드백 예제
public void OnPlayerDamaged(int damageAmount, bool isCritical)
{
    // 시각적 피드백
    VisualFeedbackManager.Instance.ShowDamageEffect(gameObject, damageAmount, isCritical);
    
    // 청각적 피드백
    AudioFeedbackManager.Instance.PlayCombatSound(CombatSoundType.PlayerDamage);
    
    // 햅틱 피드백
    HapticFeedbackManager.Instance.PlayDamageFeedback();
    
    // 체력 UI 업데이트 호출
    VisualFeedbackManager.Instance.UpdateHealthStatus(currentHealth, maxHealth);
}

// 회복 시 피드백 예제
public void OnPlayerHealed(int healAmount)
{
    // 시각적 피드백
    VisualFeedbackManager.Instance.ShowHealEffect(gameObject, healAmount);
    
    // 청각적 피드백
    AudioFeedbackManager.Instance.PlayCombatSound(CombatSoundType.Heal);
    
    // 햅틱 피드백 (약한 피드백)
    HapticFeedbackManager.Instance.PlayLightFeedback();
    
    // 체력 UI 업데이트 호출
    VisualFeedbackManager.Instance.UpdateHealthStatus(currentHealth, maxHealth);
}

// 아이템 획득 시 피드백 예제
public void OnItemPickup(ItemData item)
{
    // 시각적 피드백 (UI 알림)
    UIManager.Instance.ShowStatusMessage($"{item.ItemName} 획득!", MessageType.Success);
    
    // 청각적 피드백
    AudioFeedbackManager.Instance.PlayUISound(UISoundType.ItemPickup);
    
    // 햅틱 피드백 (약한 피드백)
    HapticFeedbackManager.Instance.PlayLightFeedback();
}
```

[이전: FeedbackSystem_part2.md]