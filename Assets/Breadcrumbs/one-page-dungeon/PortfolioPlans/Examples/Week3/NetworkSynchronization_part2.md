# 네트워크 동기화 시스템 예제 코드 (Part 2)

## 예측 및 지연 보상 시스템

### PredictionSystem.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클라이언트 측 예측 및 서버 권한 동기화를 관리하는 시스템
/// </summary>
public class PredictionSystem : MonoBehaviour
{
    // 입력 이력
    private Queue<InputState> _inputHistory = new Queue<InputState>();
    
    // 상태 이력
    private Queue<PlayerState> _stateHistory = new Queue<PlayerState>();
    
    // 최대 이력 크기
    [SerializeField] private int _maxHistorySize = 60;
    
    // 동기화 컴포넌트
    [SerializeField] private PlayerSynchronizer _synchronizer;
    
    // 컨트롤러
    [SerializeField] private CharacterController _controller;
    
    // 이동 및 물리 설정
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = 9.8f;
    
    // 현재 입력
    private Vector2 _currentMoveInput;
    private Vector3 _velocity;
    
    private void Update()
    {
        // 입력 수집
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _currentMoveInput = moveInput;
        
        // 현재 입력 및 시간으로 상태 생성
        InputState inputState = new InputState
        {
            MoveInput = moveInput,
            Timestamp = Time.time
        };
        
        // 입력 상태 저장
        _inputHistory.Enqueue(inputState);
        if (_inputHistory.Count > _maxHistorySize)
            _inputHistory.Dequeue();
        
        // 로컬 예측 적용
        ApplyLocalPrediction(moveInput);
        
        // 현재 상태 저장
        PlayerState currentState = new PlayerState
        {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles,
            Timestamp = Time.time
        };
        
        _stateHistory.Enqueue(currentState);
        if (_stateHistory.Count > _maxHistorySize)
            _stateHistory.Dequeue();
    }
    
    private void ApplyLocalPrediction(Vector2 moveInput)
    {
        // 이동 방향 계산
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= _moveSpeed;
        
        // 중력 적용
        if (_controller.isGrounded)
        {
            _velocity.y = -0.5f;
        }
        else
        {
            _velocity.y -= _gravity * Time.deltaTime;
        }
        
        // 최종 이동 방향
        Vector3 finalMove = moveDirection + new Vector3(0, _velocity.y, 0);
        
        // 이동 적용
        _controller.Move(finalMove * Time.deltaTime);
    }
    
    /// <summary>
    /// 서버에서 수신한 상태로 리콘실리에이션 수행
    /// </summary>
    public void Reconcile(PlayerState serverState)
    {
        // 서버 상태와 로컬 상태의 차이 계산
        float positionError = Vector3.Distance(transform.position, serverState.Position);
        
        // 오차가 임계값을 초과하는 경우 리콘실리에이션 수행
        if (positionError > 0.5f)
        {
            // 서버 상태가 확정되는 시점 이후의 입력만 유지
            Queue<InputState> newInputs = new Queue<InputState>();
            while (_inputHistory.Count > 0)
            {
                InputState state = _inputHistory.Dequeue();
                if (state.Timestamp > serverState.Timestamp)
                {
                    newInputs.Enqueue(state);
                }
            }
            _inputHistory = newInputs;
            
            // 상태 이력도 정리
            Queue<PlayerState> newStates = new Queue<PlayerState>();
            while (_stateHistory.Count > 0)
            {
                PlayerState state = _stateHistory.Dequeue();
                if (state.Timestamp > serverState.Timestamp)
                {
                    newStates.Enqueue(state);
                }
            }
            _stateHistory = newStates;
            
            // 서버 상태로 위치 설정
            transform.position = serverState.Position;
            transform.rotation = Quaternion.Euler(serverState.Rotation);
            
            // 저장된 입력 기록을 다시 적용하여 현재 상태 예측
            foreach (InputState input in _inputHistory)
            {
                // 이전 입력 다시 적용
                ApplyLocalPrediction(input.MoveInput);
            }
        }
    }
}

/// <summary>
/// 입력 상태 정보
/// </summary>
public class InputState
{
    public Vector2 MoveInput { get; set; }
    public float Timestamp { get; set; }
}
```

## 이벤트 기반 동기화 시스템

### CombatEventSync.cs

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MessagePack;
using UnityEngine;

/// <summary>
/// 전투 이벤트 동기화를 담당하는 컴포넌트
/// </summary>
public class CombatEventSync : MonoBehaviour
{
    // 네트워크 서비스 인터페이스
    private ICombatEventService _eventService;
    
    // 이벤트 처리기 등록
    private Dictionary<CombatEventType, Action<CombatEvent>> _eventHandlers = 
        new Dictionary<CombatEventType, Action<CombatEvent>>();
    
    // 캐싱 이벤트 큐
    private Queue<CombatEvent> _eventQueue = new Queue<CombatEvent>();
    
    // 로컬 플레이어 정보
    [SerializeField] private string _playerId;
    [SerializeField] private bool _isLocalPlayer;
    
    // 이벤트 전송 타임아웃
    [SerializeField] private float _eventTimeout = 3f;
    
    private async void Start()
    {
        // MagicOnion 서비스 연결
        _eventService = await CombatEventServiceClient.CreateAsync(
            GrpcChannelProvider.GetChannel());
        
        // 이벤트 수신 시작
        ReceiveEventsAsync().Forget();
        
        // 대기 중인 이벤트 처리 시작
        ProcessEventQueueAsync().Forget();
    }
    
    /// <summary>
    /// 전투 이벤트 핸들러 등록
    /// </summary>
    public void RegisterEventHandler(CombatEventType eventType, Action<CombatEvent> handler)
    {
        if (_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] += handler;
        }
        else
        {
            _eventHandlers[eventType] = handler;
        }
    }
    
    /// <summary>
    /// 전투 이벤트 핸들러 해제
    /// </summary>
    public void UnregisterEventHandler(CombatEventType eventType, Action<CombatEvent> handler)
    {
        if (_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] -= handler;
        }
    }
    
    /// <summary>
    /// 전투 이벤트 전송
    /// </summary>
    public async UniTask<bool> SendCombatEventAsync(CombatEventType type, string targetId, 
        Dictionary<string, object> data)
    {
        if (!_isLocalPlayer)
            return false;
        
        try
        {
            CombatEvent combatEvent = new CombatEvent
            {
                Type = type,
                SourceId = _playerId,
                TargetId = targetId,
                Data = data,
                Timestamp = GetNetworkTime()
            };
            
            // 이벤트 ID 생성
            combatEvent.EventId = $"{_playerId}_{combatEvent.Timestamp}_{UnityEngine.Random.Range(0, 10000)}";
            
            // 타임아웃 설정하여 이벤트 전송
            using (var timeoutCancellation = new TimeoutTokenSource(_eventTimeout * 1000))
            {
                return await _eventService.PublishEventAsync(combatEvent)
                    .AttachExternalCancellation(timeoutCancellation.Token);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"전투 이벤트 전송 오류: {ex.Message}");
            return false;
        }
    }
```

[이전: NetworkSynchronization_part1.md] | [다음: NetworkSynchronization_part3.md]