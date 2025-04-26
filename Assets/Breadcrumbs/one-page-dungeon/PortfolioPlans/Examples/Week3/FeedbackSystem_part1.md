# 피드백 시스템 예제 코드 (Part 1)

## 시각 및 청각적 피드백 요소

### VisualFeedbackManager.cs

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// 시각적 피드백 관리 컴포넌트
/// </summary>
public class VisualFeedbackManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static VisualFeedbackManager Instance { get; private set; }
    
    [Header("Screen Effects")]
    [SerializeField] private Material _postProcessMaterial;
    [SerializeField] private CanvasGroup _damageVignette;
    [SerializeField] private CanvasGroup _healVignette;
    [SerializeField] private CanvasGroup _lowHealthVignette;
    
    [Header("Floating Text")]
    [SerializeField] private FloatingText _damageTextPrefab;
    [SerializeField] private FloatingText _healTextPrefab;
    [SerializeField] private FloatingText _bufTextPrefab;
    [SerializeField] private FloatingText _debuffTextPrefab;
    [SerializeField] private FloatingText _criticalTextPrefab;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem _damageEffectPrefab;
    [SerializeField] private ParticleSystem _healEffectPrefab;
    [SerializeField] private ParticleSystem _criticalEffectPrefab;
    [SerializeField] private ParticleSystem _levelUpEffectPrefab;
    [SerializeField] private ParticleSystem _statusEffectPrefab;
    
    [Header("Camera Effects")]
    [SerializeField] private CameraShaker _cameraShaker;
    
    // 활성화된 이펙트 추적
    private Dictionary<GameObject, List<ParticleSystem>> _activeEffects = new Dictionary<GameObject, List<ParticleSystem>>();
    
    // 체력 비네트 관련
    private float _lowHealthThreshold = 0.3f;
    private float _lowHealthPulseSpeed = 2f;
    private float _currentHealthPercentage = 1f;
    private float _pulseTime = 0f;
    
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
        
        // 초기화
        InitializeEffects();
    }
    
    private void Update()
    {
        // 체력 낮음 비네트 업데이트
        UpdateLowHealthVignette();
    }
    
    /// <summary>
    /// 이펙트 초기화
    /// </summary>
    private void InitializeEffects()
    {
        // 비네트 초기화
        if (_damageVignette != null)
            _damageVignette.alpha = 0f;
            
        if (_healVignette != null)
            _healVignette.alpha = 0f;
            
        if (_lowHealthVignette != null)
            _lowHealthVignette.alpha = 0f;
    }
    
    /// <summary>
    /// 피해 이펙트 표시
    /// </summary>
    public async UniTask ShowDamageEffect(GameObject target, int damageAmount, bool isCritical = false)
    {
        // 대상 없으면 반환
        if (target == null)
            return;
            
        // 플로팅 텍스트 생성
        CreateFloatingText(target, damageAmount, isCritical ? _criticalTextPrefab : _damageTextPrefab);
        
        // 파티클 이펙트 생성
        ParticleSystem effectPrefab = isCritical ? _criticalEffectPrefab : _damageEffectPrefab;
        SpawnParticleEffect(target, effectPrefab);
        
        // 카메라 흔들림
        if (_cameraShaker != null)
        {
            float shakeMagnitude = isCritical ? 0.3f : 0.1f;
            _cameraShaker.Shake(shakeMagnitude, 0.2f);
        }
        
        // 화면 비네트 효과
        if (_damageVignette != null)
        {
            _damageVignette.alpha = 0.8f;
            await FadeOutCanvasGroup(_damageVignette, 0.5f);
        }
    }
    
    /// <summary>
    /// 회복 이펙트 표시
    /// </summary>
    public async UniTask ShowHealEffect(GameObject target, int healAmount)
    {
        if (target == null)
            return;
            
        // 플로팅 텍스트 생성
        CreateFloatingText(target, healAmount, _healTextPrefab);
        
        // 파티클 이펙트 생성
        SpawnParticleEffect(target, _healEffectPrefab);
        
        // 화면 비네트 효과
        if (_healVignette != null)
        {
            _healVignette.alpha = 0.5f;
            await FadeOutCanvasGroup(_healVignette, 0.5f);
        }
    }
    
    /// <summary>
    /// 상태 이펙트 표시
    /// </summary>
    public void ShowStatusEffect(GameObject target, StatusEffectType effectType, bool isPositive)
    {
        if (target == null)
            return;
            
        // 플로팅 텍스트 생성
        string effectText = GetStatusEffectText(effectType);
        FloatingText textPrefab = isPositive ? _bufTextPrefab : _debuffTextPrefab;
        
        var floatingText = Instantiate(textPrefab, GetTextPosition(target), Quaternion.identity);
        if (floatingText != null)
        {
            floatingText.SetText(effectText);
        }
        
        // 상태 이펙트 파티클 생성
        if (_statusEffectPrefab != null)
        {
            var effect = Instantiate(_statusEffectPrefab, target.transform.position, Quaternion.identity);
            SetParticleColor(effect, GetStatusEffectColor(effectType, isPositive));
            
            // 대상에 부착
            effect.transform.SetParent(target.transform);
            
            // 활성 이펙트 추적
            TrackEffect(target, effect);
        }
    }
    
    /// <summary>
    /// 레벨업 이펙트 표시
    /// </summary>
    public void ShowLevelUpEffect(GameObject target, int newLevel)
    {
        if (target == null || _levelUpEffectPrefab == null)
            return;
            
        // 레벨업 파티클 효과 생성
        var levelUpEffect = Instantiate(_levelUpEffectPrefab, target.transform.position, Quaternion.identity);
        levelUpEffect.transform.SetParent(target.transform);
        
        // 플로팅 텍스트 생성
        var levelUpText = Instantiate(_bufTextPrefab, GetTextPosition(target), Quaternion.identity);
        if (levelUpText != null)
        {
            levelUpText.SetText($"레벨 업! ({newLevel})");
            levelUpText.SetScale(1.5f);
        }
    }
    
    /// <summary>
    /// 체력 상태 업데이트
    /// </summary>
    public void UpdateHealthStatus(float currentHealth, float maxHealth)
    {
        _currentHealthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
    }
    
    /// <summary>
    /// 모든 이펙트 제거
    /// </summary>
    public void ClearAllEffects(GameObject target)
    {
        if (target == null || !_activeEffects.ContainsKey(target))
            return;
            
        foreach (var effect in _activeEffects[target])
        {
            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }
        
        _activeEffects.Remove(target);
    }
    
    // 내부 유틸리티 메서드
    
    /// <summary>
    /// 플로팅 텍스트 생성
    /// </summary>
    private void CreateFloatingText(GameObject target, int amount, FloatingText prefab)
    {
        if (prefab == null)
            return;
            
        // 텍스트 위치 계산
        Vector3 position = GetTextPosition(target);
        
        // 랜덤한 오프셋 추가
        position += new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(0f, 0.5f), 0);
        
        // 플로팅 텍스트 생성
        var floatingText = Instantiate(prefab, position, Quaternion.identity);
        if (floatingText != null)
        {
            floatingText.SetNumber(amount);
        }
    }
    
    /// <summary>
    /// 파티클 이펙트 생성
    /// </summary>
    private void SpawnParticleEffect(GameObject target, ParticleSystem prefab)
    {
        if (prefab == null)
            return;
            
        // 이펙트 생성
        Vector3 position = target.transform.position;
        ParticleSystem effect = Instantiate(prefab, position, Quaternion.identity);
        
        // 대상 추적 설정
        var targetFollower = effect.gameObject.AddComponent<TargetFollower>();
        targetFollower.SetTarget(target.transform);
        
        // 활성 이펙트 추적
        TrackEffect(target, effect);
    }
    
    /// <summary>
    /// 활성 이펙트 추적
    /// </summary>
    private void TrackEffect(GameObject target, ParticleSystem effect)
    {
        if (target == null || effect == null)
            return;
            
        if (!_activeEffects.ContainsKey(target))
        {
            _activeEffects[target] = new List<ParticleSystem>();
        }
        
        _activeEffects[target].Add(effect);
        
        // 자동 삭제 (파티클 완료 후)
        CleanupEffectWhenFinished(target, effect).Forget();
    }
    
    /// <summary>
    /// 이펙트 자동 정리
    /// </summary>
    private async UniTaskVoid CleanupEffectWhenFinished(GameObject target, ParticleSystem effect)
    {
        if (effect == null)
            return;
            
        // 파티클 시스템이 완료될 때까지 대기
        await UniTask.WaitUntil(() => !effect.IsAlive(true));
        
        // 이펙트 제거
        if (effect != null)
        {
            if (_activeEffects.ContainsKey(target))
            {
                _activeEffects[target].Remove(effect);
            }
            
            Destroy(effect.gameObject);
        }
    }
    
    /// <summary>
    /// 캔버스 그룹 페이드 아웃
    /// </summary>
    private async UniTask FadeOutCanvasGroup(CanvasGroup group, float duration)
    {
        if (group == null)
            return;
            
        float startTime = Time.time;
        float startAlpha = group.alpha;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            group.alpha = Mathf.Lerp(startAlpha, 0f, t);
            await UniTask.Yield();
        }
        
        group.alpha = 0f;
    }
    
    /// <summary>
    /// 텍스트 위치 계산
    /// </summary>
    private Vector3 GetTextPosition(GameObject target)
    {
        if (target == null)
            return Vector3.zero;
            
        // 타겟의 위치에서 약간 위로 오프셋
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.center + Vector3.up * renderer.bounds.extents.y;
        }
        
        // 렌더러가 없으면 기본 위치
        return target.transform.position + Vector3.up * 1.5f;
    }
```

[다음: FeedbackSystem_part2.md]