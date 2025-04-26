# 시스템 통합 예제 - Part 15

## 접근성 관리자 클래스

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 접근성 관리자 클래스
/// 게임의 접근성 기능을 관리합니다.
/// </summary>
public class AccessibilityManager : MonoBehaviour, ISystemInitializer
{
    private static AccessibilityManager _instance;
    public static AccessibilityManager Instance => _instance;
    
    [Header("Color Blind Mode")]
    [SerializeField] private Material _colorBlindMaterial;
    [SerializeField] private string _colorTypeProperty = "_ColorBlindType";
    [SerializeField] private string _colorStrengthProperty = "_ColorBlindStrength";
    
    [Header("Reduce Motion")]
    [SerializeField] private float _reducedMotionMultiplier = 0.3f;
    
    [Header("Large Text")]
    [SerializeField] private float _largeTextScale = 1.3f;
    
    // 현재 상태
    private bool _colorBlindModeEnabled = false;
    private int _colorBlindType = 0;
    private bool _reduceMotionEnabled = false;
    private bool _largeTextEnabled = false;
    
    // 효과 카메라
    private Camera _effectCamera;
    
    // 이벤트
    public event Action<bool, int> OnColorBlindModeChanged;
    public event Action<bool> OnReduceMotionChanged;
    public event Action<bool> OnLargeTextChanged;
    
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
    }
    
    /// <summary>
    /// 시스템 초기화
    /// </summary>
    public async UniTask Initialize(CancellationToken cancellationToken = default)
    {
        // 효과 카메라 검색 또는 생성
        SetupEffectCamera();
        
        // 색맹 모드 셰이더 초기화
        InitializeColorBlindShader();
        
        await UniTask.CompletedTask;
    }
    
    /// <summary>
    /// 시스템 종료
    /// </summary>
    public void Shutdown()
    {
        // 추가적인 정리 작업이 필요하면 여기에 구현
    }
    
    /// <summary>
    /// 효과 카메라 설정
    /// </summary>
    private void SetupEffectCamera()
    {
        // 이미 효과 카메라가 있는지 확인
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras)
        {
            if (camera.CompareTag("EffectCamera"))
            {
                _effectCamera = camera;
                break;
            }
        }
        
        // 효과 카메라가 없으면 생성
        if (_effectCamera == null)
        {
            GameObject effectCameraObj = new GameObject("AccessibilityEffectCamera");
            effectCameraObj.tag = "EffectCamera";
            
            _effectCamera = effectCameraObj.AddComponent<Camera>();
            _effectCamera.clearFlags = CameraClearFlags.Nothing;
            _effectCamera.cullingMask = 0; // 아무것도 렌더링하지 않음
            _effectCamera.depth = 99; // 다른 카메라 위에 그려짐
            _effectCamera.useOcclusionCulling = false;
            _effectCamera.allowHDR = false;
            _effectCamera.allowMSAA = false;
            
            effectCameraObj.transform.parent = transform;
        }
    }
```
