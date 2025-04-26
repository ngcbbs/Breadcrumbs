# 피드백 시스템 예제 코드 (Part 2)

## VisualFeedbackManager.cs (계속)

```csharp
    /// <summary>
    /// 상태 효과 텍스트 가져오기
    /// </summary>
    private string GetStatusEffectText(StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Poison: return "중독";
            case StatusEffectType.Burn: return "화상";
            case StatusEffectType.Stun: return "기절";
            case StatusEffectType.Slow: return "감속";
            case StatusEffectType.Haste: return "가속";
            case StatusEffectType.Strength: return "힘 증가";
            case StatusEffectType.Defense: return "방어 증가";
            case StatusEffectType.Weakness: return "약화";
            default: return effectType.ToString();
        }
    }
    
    /// <summary>
    /// 상태 효과 색상 가져오기
    /// </summary>
    private Color GetStatusEffectColor(StatusEffectType effectType, bool isPositive)
    {
        if (isPositive)
        {
            switch (effectType)
            {
                case StatusEffectType.Haste: return new Color(0f, 0.7f, 1f);
                case StatusEffectType.Strength: return new Color(1f, 0.5f, 0f);
                case StatusEffectType.Defense: return new Color(0f, 0.6f, 0.2f);
                default: return Color.green;
            }
        }
        else
        {
            switch (effectType)
            {
                case StatusEffectType.Poison: return new Color(0.5f, 0.9f, 0.3f);
                case StatusEffectType.Burn: return new Color(1f, 0.3f, 0f);
                case StatusEffectType.Stun: return new Color(1f, 0.9f, 0f);
                case StatusEffectType.Slow: return new Color(0.4f, 0.4f, 0.8f);
                case StatusEffectType.Weakness: return new Color(0.5f, 0.2f, 0.5f);
                default: return Color.red;
            }
        }
    }
    
    /// <summary>
    /// 파티클 색상 설정
    /// </summary>
    private void SetParticleColor(ParticleSystem particleSystem, Color color)
    {
        if (particleSystem == null)
            return;
            
        var main = particleSystem.main;
        main.startColor = color;
    }
    
    /// <summary>
    /// 체력 낮음 비네트 업데이트
    /// </summary>
    private void UpdateLowHealthVignette()
    {
        if (_lowHealthVignette == null)
            return;
            
        if (_currentHealthPercentage <= _lowHealthThreshold)
        {
            // 낮은 체력 상태일 때 맥박 효과 적용
            _pulseTime += Time.deltaTime * _lowHealthPulseSpeed;
            float pulseValue = Mathf.Abs(Mathf.Sin(_pulseTime));
            
            // 체력에 따른 강도 계산
            float intensity = Mathf.Lerp(0.3f, 0.8f, 1f - (_currentHealthPercentage / _lowHealthThreshold));
            
            // 최종 알파 설정
            _lowHealthVignette.alpha = intensity * pulseValue;
        }
        else
        {
            // 정상 체력 상태
            _lowHealthVignette.alpha = 0f;
        }
    }
}

/// <summary>
/// 상태 효과 타입
/// </summary>
public enum StatusEffectType
{
    None,
    Poison,
    Burn,
    Stun,
    Slow,
    Haste,
    Strength,
    Defense,
    Weakness
}
```

### AudioFeedbackManager.cs

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 오디오 피드백 관리 컴포넌트
/// </summary>
public class AudioFeedbackManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static AudioFeedbackManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _uiSource;
    [SerializeField] private AudioSource _ambientSource;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    [SerializeField] private AudioMixerGroup _uiMixerGroup;
    [SerializeField] private AudioMixerGroup _ambientMixerGroup;
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    
    [Header("Combat Sounds")]
    [SerializeField] private AudioClip[] _meleeSounds;
    [SerializeField] private AudioClip[] _magicSounds;
    [SerializeField] private AudioClip[] _hitSounds;
    [SerializeField] private AudioClip[] _criticalHitSounds;
    [SerializeField] private AudioClip[] _enemyHitSounds;
    [SerializeField] private AudioClip[] _enemyDeathSounds;
    [SerializeField] private AudioClip[] _playerDamageSounds;
    [SerializeField] private AudioClip[] _healSounds;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip _buttonClickSound;
    [SerializeField] private AudioClip _buttonHoverSound;
    [SerializeField] private AudioClip _menuOpenSound;
    [SerializeField] private AudioClip _menuCloseSound;
    [SerializeField] private AudioClip _errorSound;
    [SerializeField] private AudioClip _successSound;
    [SerializeField] private AudioClip _inventoryOpenSound;
    [SerializeField] private AudioClip _itemPickupSound;
    [SerializeField] private AudioClip _itemEquipSound;
    
    [Header("Environment Sounds")]
    [SerializeField] private AudioClip _doorOpenSound;
    [SerializeField] private AudioClip _doorCloseSound;
    [SerializeField] private AudioClip _chestOpenSound;
    [SerializeField] private AudioClip _trapTriggerSound;
    [SerializeField] private AudioClip _teleportSound;
    
    [Header("Ambient Sound")]
    [SerializeField] private AudioClip _dungeonAmbientLoop;
    [SerializeField] private AudioClip _battleAmbientLoop;
    [SerializeField] private AudioClip _menuAmbientLoop;
    
    [Header("Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _explorationMusic;
    [SerializeField] private AudioClip _combatMusic;
    [SerializeField] private AudioClip _bossMusic;
    [SerializeField] private AudioClip _victoryMusic;
    [SerializeField] private AudioClip _defeatMusic;
    
    // 오디오 풀
    private Dictionary<AudioType, List<AudioSource>> _audioSourcePool = new Dictionary<AudioType, List<AudioSource>>();
    private int _poolSize = 10;
    
    // 볼륨 설정
    private float _masterVolume = 1f;
    private float _sfxVolume = 1f;
    private float _musicVolume = 1f;
    private float _uiVolume = 1f;
    private float _ambientVolume = 1f;
    
    // 현재 재생 중인 음악 및 환경음
    private AudioClip _currentMusic;
    private AudioClip _currentAmbient;
    
    // 음악 페이드 상태
    private bool _isMusicFading = false;
    private float _musicFadeTime = 1.5f;
    private float _currentMusicFadeTime = 0f;
    private float _targetMusicVolume = 1f;
    private float _startMusicVolume = 0f;
    
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
        
        // 오디오 소스 초기화
        InitializeAudioSources();
        
        // 오디오 풀 생성
        CreateAudioPool();
    }
    
    private void Update()
    {
        // 음악 페이드 처리
        UpdateMusicFade();
    }
    
    /// <summary>
    /// 오디오 소스 초기화
    /// </summary>
    private void InitializeAudioSources()
    {
        // SFX 오디오 소스 설정
        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
        }
        _sfxSource.outputAudioMixerGroup = _sfxMixerGroup;
        _sfxSource.playOnAwake = false;
        
        // UI 오디오 소스 설정
        if (_uiSource == null)
        {
            _uiSource = gameObject.AddComponent<AudioSource>();
        }
        _uiSource.outputAudioMixerGroup = _uiMixerGroup;
        _uiSource.playOnAwake = false;
        
        // 환경음 오디오 소스 설정
        if (_ambientSource == null)
        {
            _ambientSource = gameObject.AddComponent<AudioSource>();
        }
        _ambientSource.outputAudioMixerGroup = _ambientMixerGroup;
        _ambientSource.loop = true;
        _ambientSource.playOnAwake = false;
        
        // 음악 오디오 소스 설정
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
        }
        _musicSource.outputAudioMixerGroup = _musicMixerGroup;
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
    }
    
    /// <summary>
    /// 오디오 풀 생성
    /// </summary>
    private void CreateAudioPool()
    {
        // 각 타입별 오디오 소스 풀 생성
        _audioSourcePool[AudioType.SFX] = new List<AudioSource>();
        _audioSourcePool[AudioType.UI] = new List<AudioSource>();
        
        // SFX 소스 풀 생성
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = _sfxMixerGroup;
            source.playOnAwake = false;
            source.volume = _sfxVolume * _masterVolume;
            _audioSourcePool[AudioType.SFX].Add(source);
        }
        
        // UI 소스 풀 생성
        for (int i = 0; i < _poolSize / 2; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = _uiMixerGroup;
            source.playOnAwake = false;
            source.volume = _uiVolume * _masterVolume;
            _audioSourcePool[AudioType.UI].Add(source);
        }
    }
    
    /// <summary>
    /// 풀에서 오디오 소스 가져오기
    /// </summary>
    private AudioSource GetAudioSourceFromPool(AudioType type)
    {
        if (!_audioSourcePool.ContainsKey(type))
            return null;
            
        // 사용 가능한 소스 찾기
        foreach (var source in _audioSourcePool[type])
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // 모든 소스가 사용 중이면 새로 생성
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        
        switch (type)
        {
            case AudioType.SFX:
                newSource.outputAudioMixerGroup = _sfxMixerGroup;
                newSource.volume = _sfxVolume * _masterVolume;
                break;
            case AudioType.UI:
                newSource.outputAudioMixerGroup = _uiMixerGroup;
                newSource.volume = _uiVolume * _masterVolume;
                break;
        }
        
        _audioSourcePool[type].Add(newSource);
        return newSource;
    }
```

[이전: FeedbackSystem_part1.md] | [다음: FeedbackSystem_part3.md]