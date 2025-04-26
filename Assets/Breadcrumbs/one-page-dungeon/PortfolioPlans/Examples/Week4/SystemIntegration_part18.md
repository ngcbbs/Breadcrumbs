# 시스템 통합 예제 - Part 18

## 색맹 모드 효과 클래스

```csharp
using System;
using UnityEngine;

/// <summary>
/// 색맹 모드 효과 컴포넌트
/// 색맹 모드를 위한 포스트 프로세싱 효과를 적용합니다.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ColorBlindEffect : MonoBehaviour
{
    // 색맹 효과 적용을 위한 머티리얼
    [SerializeField] public Material material;
    
    // 색맹 타입 상수
    public const int TYPE_PROTANOPIA = 0;  // 적색맹
    public const int TYPE_DEUTERANOPIA = 1; // 녹색맹
    public const int TYPE_TRITANOPIA = 2;  // 청색맹
    public const int TYPE_ACHROMATOPSIA = 3; // 전색맹
    
    // 이미지 효과 적용
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            Graphics.Blit(source, destination, material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}

/// <summary>
/// 포스트 프로세싱 볼륨 확장 클래스
/// 접근성을 위한 포스트 프로세싱 효과 제어를 위한 메서드들을 제공합니다.
/// </summary>
public static class PostProcessingVolumeExtensions
{
    /// <summary>
    /// 특정 포스트 프로세싱 효과 비활성화
    /// </summary>
    public static void DisableEffect(this PostProcessingVolume volume, string effectName)
    {
        // PostProcessingVolume 클래스 구현에 따라 다름
        // 예시 코드
        var profile = volume.profile;
        if (profile != null)
        {
            var settings = profile.GetSetting(effectName);
            if (settings != null)
            {
                settings.enabled.value = false;
            }
        }
    }
    
    /// <summary>
    /// 특정 포스트 프로세싱 효과 활성화
    /// </summary>
    public static void EnableEffect(this PostProcessingVolume volume, string effectName)
    {
        // PostProcessingVolume 클래스 구현에 따라 다름
        // 예시 코드
        var profile = volume.profile;
        if (profile != null)
        {
            var settings = profile.GetSetting(effectName);
            if (settings != null)
            {
                settings.enabled.value = true;
            }
        }
    }
    
    /// <summary>
    /// 특정 포스트 프로세싱 효과 강도 조정
    /// </summary>
    public static void ReduceEffectIntensity(this PostProcessingVolume volume, string effectName, float multiplier)
    {
        // PostProcessingVolume 클래스 구현에 따라 다름
        // 예시 코드
        var profile = volume.profile;
        if (profile != null)
        {
            var settings = profile.GetSetting(effectName);
            if (settings != null && settings.enabled.value)
            {
                // 효과에 따라 다른 프로퍼티 조정
                switch (effectName)
                {
                    case "Bloom":
                        var bloom = settings as BloomSettings;
                        if (bloom != null)
                        {
                            bloom.intensity.value *= multiplier;
                        }
                        break;
                        
                    case "DepthOfField":
                        var dof = settings as DepthOfFieldSettings;
                        if (dof != null)
                        {
                            dof.focusDistance.value *= multiplier;
                        }
                        break;
                        
                    // 필요에 따라 다른 효과 추가
                }
            }
        }
    }
    
    /// <summary>
    /// 모든 포스트 프로세싱 효과 복원
    /// </summary>
    public static void RestoreEffects(this PostProcessingVolume volume)
    {
        // 원래 값으로 복원 (별도의 저장이 필요함)
        // 실제 구현에서는 원래 값을 저장하고 복원하는 로직 필요
    }
}
```
