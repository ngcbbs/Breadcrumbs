# 네트워크 동기화 시스템 예제 코드 (Part 1)

## 상태 동기화 시스템

### PlayerSynchronizer.cs

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MessagePack;
using UnityEngine;

/// <summary>
/// 플레이어 상태 동기화를 담당하는 컴포넌트
/// </summary>
public class PlayerSynchronizer : MonoBehaviour
{
    // 네트워크 서비스 인터페이스
    private IPlayerSyncService _syncService;
    
    // 플레이어 식별자
    [SerializeField] private string _playerId;
    
    // 동기화 관련 컴포넌트
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Animator _animator;
    
    // 동기화 설정
    [SerializeField] private float _positionSyncThreshold = 0.1f;
    [SerializeField] private float _rotationSyncThreshold = 5.0f;
    [SerializeField] private float _syncInterval = 0.1f;
    
    // 로컬 상태 캐싱
    private Vector3 _lastSyncedPosition;
    private Quaternion _lastSyncedRotation;
    private Dictionary<string, float> _lastSyncedAnimParams = new Dictionary<string, float>();
    
    // 네트워크 상태 캐싱
    private Vector3 _targetNetPosition;
    private Quaternion _targetNetRotation;
    private Dictionary<string, float> _targetNetAnimParams = new Dictionary<string, float>();
    
    // 보간 관련 변수
    [SerializeField] private float _positionLerpSpeed = 15f;
    [SerializeField] private float _rotationLerpSpeed = 15f;
    
    // 로컬 플레이어 여부
    [SerializeField] private bool _isLocalPlayer;
    
    // 지연 보상 관련 변수
    private float _averagePing = 0f;
    private Queue<float> _pingSamples = new Queue<float>(10);
    
    private void Awake()
    {
        // 초기 상태 설정
        _lastSyncedPosition = transform.position;
        _lastSyncedRotation = transform.rotation;
        _targetNetPosition = transform.position;
        _targetNetRotation = transform.rotation;
        
        // 로컬 플레이어가 아닌 경우 컨트롤러 비활성화
        if (!_isLocalPlayer && _characterController != null)
        {
            _characterController.enabled = false;
        }
    }
    
    private async void Start()
    {
        // MagicOnion 서비스 연결
        _syncService = await PlayerSyncServiceClient.CreateAsync(
            GrpcChannelProvider.GetChannel());
        
        // 플레이어 등록
        await _syncService.RegisterPlayerAsync(_playerId, transform.position, transform.rotation.eulerAngles);
        
        // 주기적 동기화 시작
        StartCoroutine(SyncRoutine());
        
        // 네트워크 상태 수신 시작
        ReceiveStateUpdatesAsync().Forget();
    }
    
    private System.Collections.IEnumerator SyncRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(_syncInterval);
        
        while (true)
        {
            if (_isLocalPlayer)
            {
                // 로컬 플레이어인 경우 상태 전송
                SendStateUpdate();
            }
            
            yield return wait;
        }
    }
    
    private void SendStateUpdate()
    {
        // 위치나 회전이 충분히 변경된 경우에만 전송
        if (Vector3.Distance(transform.position, _lastSyncedPosition) > _positionSyncThreshold ||
            Quaternion.Angle(transform.rotation, _lastSyncedRotation) > _rotationSyncThreshold)
        {
            // 현재 애니메이션 파라미터 수집
            Dictionary<string, float> currentAnimParams = CollectAnimatorParameters();
            
            // 상태 전송
            PlayerState state = new PlayerState
            {
                PlayerId = _playerId,
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles,
                AnimationParameters = currentAnimParams,
                Timestamp = GetNetworkTime()
            };
            
            _syncService.UpdateStateAsync(state).Forget();
            
            // 마지막 동기화 상태 업데이트
            _lastSyncedPosition = transform.position;
            _lastSyncedRotation = transform.rotation;
            _lastSyncedAnimParams = currentAnimParams;
        }
    }
    
    private async UniTask ReceiveStateUpdatesAsync()
    {
        try
        {
            // 다른 플레이어 상태 구독
            await foreach (var state in _syncService.OnStateUpdatedAsync())
            {
                // 자신의 상태는 무시
                if (state.PlayerId == _playerId)
                    continue;
                
                // 네트워크에서 받은 상태 처리
                ProcessReceivedState(state);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"상태 업데이트 수신 오류: {ex.Message}");
            // 재연결 로직
            await UniTask.Delay(1000);
            ReceiveStateUpdatesAsync().Forget();
        }
    }
    
    private void ProcessReceivedState(PlayerState state)
    {
        // 타겟 위치 및 회전 설정
        _targetNetPosition = state.Position;
        _targetNetRotation = Quaternion.Euler(state.Rotation);
        
        // 애니메이션 파라미터 업데이트
        foreach (var param in state.AnimationParameters)
        {
            _targetNetAnimParams[param.Key] = param.Value;
        }
        
        // 지연 시간 계산 업데이트
        UpdatePingAverage(GetNetworkTime() - state.Timestamp);
    }
    
    private void Update()
    {
        if (!_isLocalPlayer)
        {
            // 위치 보간
            transform.position = Vector3.Lerp(transform.position, _targetNetPosition, 
                _positionLerpSpeed * Time.deltaTime);
            
            // 회전 보간
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetNetRotation, 
                _rotationLerpSpeed * Time.deltaTime);
            
            // 애니메이션 파라미터 보간 및 적용
            ApplyAnimationParameters();
        }
    }
    
    private Dictionary<string, float> CollectAnimatorParameters()
    {
        Dictionary<string, float> parameters = new Dictionary<string, float>();
        
        if (_animator != null)
        {
            // 모든 float 파라미터 수집
            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Float)
                {
                    parameters[param.name] = _animator.GetFloat(param.name);
                }
                else if (param.type == AnimatorControllerParameterType.Bool)
                {
                    parameters[param.name] = _animator.GetBool(param.name) ? 1f : 0f;
                }
                else if (param.type == AnimatorControllerParameterType.Int)
                {
                    parameters[param.name] = _animator.GetInteger(param.name);
                }
            }
        }
        
        return parameters;
    }
    
    private void ApplyAnimationParameters()
    {
        if (_animator != null)
        {
            foreach (var param in _targetNetAnimParams)
            {
                // 애니메이터 파라미터 가져오기
                AnimatorControllerParameter animParam = Array.Find(_animator.parameters, 
                    p => p.name == param.Key);
                
                if (animParam != null)
                {
                    switch (animParam.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            _animator.SetFloat(param.Key, param.Value);
                            break;
                        case AnimatorControllerParameterType.Bool:
                            _animator.SetBool(param.Key, param.Value > 0.5f);
                            break;
                        case AnimatorControllerParameterType.Int:
                            _animator.SetInteger(param.Key, Mathf.RoundToInt(param.Value));
                            break;
                    }
                }
            }
        }
    }
    
    private void UpdatePingAverage(float pingTime)
    {
        // 핑 샘플 업데이트
        _pingSamples.Enqueue(pingTime);
        if (_pingSamples.Count > 10)
            _pingSamples.Dequeue();
        
        // 평균 계산
        float sum = 0;
        foreach (float sample in _pingSamples)
        {
            sum += sample;
        }
        _averagePing = sum / _pingSamples.Count;
    }
    
    // 네트워크 시간 가져오기 (서버와 동기화된 시간)
    private float GetNetworkTime()
    {
        return Time.time; // 실제로는 서버와 동기화된 시간이 필요
    }
}

// 플레이어 상태 정의
[MessagePackObject]
public class PlayerState
{
    [Key(0)]
    public string PlayerId { get; set; }
    
    [Key(1)]
    public Vector3 Position { get; set; }
    
    [Key(2)]
    public Vector3 Rotation { get; set; }
    
    [Key(3)]
    public Dictionary<string, float> AnimationParameters { get; set; }
    
    [Key(4)]
    public float Timestamp { get; set; }
}
```

### IPlayerSyncService.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicOnion;
using UnityEngine;

/// <summary>
/// 플레이어 동기화 서비스 인터페이스
/// </summary>
public interface IPlayerSyncService : IService<IPlayerSyncService>
{
    // 플레이어 등록
    Task<bool> RegisterPlayerAsync(string playerId, Vector3 position, Vector3 rotation);
    
    // 플레이어 상태 업데이트
    Task<bool> UpdateStateAsync(PlayerState state);
    
    // 플레이어 상태 업데이트 구독
    IAsyncEnumerable<PlayerState> OnStateUpdatedAsync();
    
    // 플레이어 연결 해제
    Task<bool> UnregisterPlayerAsync(string playerId);
}
```

[다음: NetworkSynchronization_part2.md]