# 시스템 통합 예제 - Part 16

## 접근성 관리자 클래스 (계속)

```csharp
    /// <summary>
    /// 색맹 모드 셰이더 초기화
    /// </summary>
    private void InitializeColorBlindShader()
    {
        if (_colorBlindMaterial != null)
        {
            // 색맹 모드 비활성화 상태로 시작
            _colorBlindMaterial.SetFloat(_colorStrengthProperty, 0f);
            
            // 효과 카메라에 포스트 프로세싱 효과 추가
            if (_effectCamera != null)
            {
                // Unity의 레거시 이미지 효과 시스템 사용
                var colorBlindEffect = _effectCamera.gameObject.AddComponent<ColorBlindEffect>();
                colorBlindEffect.material = _colorBlindMaterial;
                colorBlindEffect.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("Color blind material is not assigned!");
        }
    }
    
    /// <summary>
    /// 색맹 모드 설정
    /// </summary>
    public void SetColorBlindMode(bool enable, int type = 0)
    {
        _colorBlindModeEnabled = enable;
        _colorBlindType = Mathf.Clamp(type, 0, 3);
        
        if (_effectCamera != null)
        {
            var colorBlindEffect = _effectCamera.GetComponent<ColorBlindEffect>();
            if (colorBlindEffect != null)
            {
                colorBlindEffect.enabled = _colorBlindModeEnabled;
                
                if (_colorBlindModeEnabled)
                {
                    // 색맹 타입 설정
                    _colorBlindMaterial.SetFloat(_colorTypeProperty, _colorBlindType);
                    
                    // 강도 설정 (1.0 = 100%)
                    _colorBlindMaterial.SetFloat(_colorStrengthProperty, 1.0f);
                }
            }
        }
        
        // 이벤트 발생
        OnColorBlindModeChanged?.Invoke(_colorBlindModeEnabled, _colorBlindType);
    }
    
    /// <summary>
    /// 움직임 감소 설정
    /// </summary>
    public void SetReduceMotion(bool reduce)
    {
        _reduceMotionEnabled = reduce;
        
        // 모든 애니메이터 및 파티클 시스템에 설정 적용
        ApplyReduceMotionToAnimators();
        ApplyReduceMotionToParticleSystems();
        ApplyReduceMotionToCameraEffects();
        
        // 이벤트 발생
        OnReduceMotionChanged?.Invoke(_reduceMotionEnabled);
    }
    
    /// <summary>
    /// 애니메이터에 움직임 감소 적용
    /// </summary>
    private void ApplyReduceMotionToAnimators()
    {
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators)
        {
            if (_reduceMotionEnabled)
            {
                // 애니메이션 속도 감소
                animator.speed = _reducedMotionMultiplier;
            }
            else
            {
                // 원래 속도로 복원
                animator.speed = 1.0f;
            }
        }
    }
    
    /// <summary>
    /// 파티클 시스템에 움직임 감소 적용
    /// </summary>
    private void ApplyReduceMotionToParticleSystems()
    {
        ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            
            if (_reduceMotionEnabled)
            {
                // 파티클 시스템 속도 및 크기 감소
                main.simulationSpeed = _reducedMotionMultiplier;
                main.startSizeMultiplier *= 0.7f;
                
                // 일부 효과 비활성화
                if (ps.CompareTag("VisualEffect") || ps.CompareTag("IntensiveEffect"))
                {
                    ps.gameObject.SetActive(false);
                }
            }
            else
            {
                // 원래 설정으로 복원
                main.simulationSpeed = 1.0f;
                main.startSizeMultiplier /= 0.7f;
                
                // 비활성화된 효과 다시 활성화
                if (ps.CompareTag("VisualEffect") || ps.CompareTag("IntensiveEffect"))
                {
                    ps.gameObject.SetActive(true);
                }
            }
        }
    }
```
