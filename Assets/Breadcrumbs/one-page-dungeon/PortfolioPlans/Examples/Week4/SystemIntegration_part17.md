# 시스템 통합 예제 - Part 17

## 접근성 관리자 클래스 (계속)

```csharp
    /// <summary>
    /// 카메라 효과에 움직임 감소 적용
    /// </summary>
    private void ApplyReduceMotionToCameraEffects()
    {
        // 카메라 흔들림 효과 컴포넌트 찾기
        CameraShake[] cameraShakes = FindObjectsOfType<CameraShake>();
        foreach (CameraShake shake in cameraShakes)
        {
            if (_reduceMotionEnabled)
            {
                // 흔들림 강도 감소
                shake.SetIntensityMultiplier(_reducedMotionMultiplier);
            }
            else
            {
                // 원래 강도로 복원
                shake.SetIntensityMultiplier(1.0f);
            }
        }
        
        // 포스트 프로세싱 효과 조정
        var postProcessingVolume = FindObjectOfType<PostProcessingVolume>();
        if (postProcessingVolume != null)
        {
            if (_reduceMotionEnabled)
            {
                // 모션 블러 비활성화
                postProcessingVolume.DisableEffect("MotionBlur");
                
                // 피사계 심도 강도 감소
                postProcessingVolume.ReduceEffectIntensity("DepthOfField", _reducedMotionMultiplier);
                
                // 블룸 강도 감소
                postProcessingVolume.ReduceEffectIntensity("Bloom", _reducedMotionMultiplier);
            }
            else
            {
                // 효과 복원
                postProcessingVolume.RestoreEffects();
            }
        }
    }
    
    /// <summary>
    /// 큰 텍스트 설정
    /// </summary>
    public void SetLargeText(bool large)
    {
        _largeTextEnabled = large;
        
        // UI 매니저에 설정 적용
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetLargeTextMode(_largeTextEnabled, _largeTextScale);
        }
        
        // 이벤트 발생
        OnLargeTextChanged?.Invoke(_largeTextEnabled);
    }
    
    /// <summary>
    /// 장면 변경 시 설정 다시 적용
    /// </summary>
    public void ReapplySettings()
    {
        // 색맹 모드 설정 다시 적용
        SetColorBlindMode(_colorBlindModeEnabled, _colorBlindType);
        
        // 움직임 감소 설정 다시 적용
        SetReduceMotion(_reduceMotionEnabled);
        
        // 큰 텍스트 설정 다시 적용
        SetLargeText(_largeTextEnabled);
    }
    
    /// <summary>
    /// 높은 대비 UI 활성화
    /// </summary>
    public void SetHighContrastUI(bool enable)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHighContrastMode(enable);
        }
    }
    
    /// <summary>
    /// 자막 활성화
    /// </summary>
    public void SetSubtitles(bool enable)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetSubtitlesEnabled(enable);
        }
    }
    
    /// <summary>
    /// 자막 크기 설정
    /// </summary>
    public void SetSubtitleSize(float size)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetSubtitleSize(size);
        }
    }
    
    /// <summary>
    /// 다중 버튼 누름 방지 설정
    /// </summary>
    public void SetPreventMultipleButtonPresses(bool enable)
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetPreventMultipleButtonPresses(enable);
        }
    }
}
```
