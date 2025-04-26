# 네트워크 동기화 시스템 예제 코드 (Part 3)

## 이벤트 기반 동기화 시스템 (계속)

### CombatEventSync.cs (계속)

```csharp
    /// <summary>
    /// 전투 이벤트 수신 처리
    /// </summary>
    private async UniTask ReceiveEventsAsync()
    {
        try
        {
            // 전투 이벤트 구독
            await foreach (var combatEvent in _eventService.OnEventOccurredAsync())
            {
                // 이벤트를 큐에 추가
                _eventQueue.Enqueue(combatEvent);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"전투 이벤트 수신 오류: {ex.Message}");
            // 재연결 로직
            await UniTask.Delay(1000);
            ReceiveEventsAsync().Forget();
        }
    }
    
    /// <summary>
    /// 큐에 있는 이벤트 처리
    /// </summary>
    private async UniTask ProcessEventQueueAsync()
    {
        while (true)
        {
            // 큐에 이벤트가 있으면 처리
            if (_eventQueue.Count > 0)
            {
                var combatEvent = _eventQueue.Dequeue();
                
                // 자신의 이벤트도 처리 (서버에서 확인된 경우)
                // 이벤트 핸들러 호출
                if (_eventHandlers.TryGetValue(combatEvent.Type, out var handler))
                {
                    handler?.Invoke(combatEvent);
                }
            }
            
            await UniTask.Yield();
        }
    }
    
    // 네트워크 시간 가져오기 (서버와 동기화된 시간)
    private float GetNetworkTime()
    {
        return Time.time; // 실제로는 서버와 동기화된 시간이 필요
    }
}

/// <summary>
/// 전투 이벤트 타입
/// </summary>
public enum CombatEventType
{
    Attack,
    Damage,
    Death,
    Heal,
    StatusEffect,
    ItemUse,
    SkillUse
}

/// <summary>
/// 전투 이벤트 데이터
/// </summary>
[MessagePackObject]
public class CombatEvent
{
    [Key(0)]
    public string EventId { get; set; }
    
    [Key(1)]
    public CombatEventType Type { get; set; }
    
    [Key(2)]
    public string SourceId { get; set; }
    
    [Key(3)]
    public string TargetId { get; set; }
    
    [Key(4)]
    public Dictionary<string, object> Data { get; set; }
    
    [Key(5)]
    public float Timestamp { get; set; }
}
```

### ICombatEventService.cs

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicOnion;

/// <summary>
/// 전투 이벤트 서비스 인터페이스
/// </summary>
public interface ICombatEventService : IService<ICombatEventService>
{
    // 이벤트 발행
    Task<bool> PublishEventAsync(CombatEvent combatEvent);
    
    // 이벤트 구독
    IAsyncEnumerable<CombatEvent> OnEventOccurredAsync();
}
```

## 던전 상태 동기화 시스템

### DungeonStateSync.cs

```csharp
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MessagePack;
using UnityEngine;

/// <summary>
/// 던전 상태 동기화를 담당하는 컴포넌트
/// </summary>
public class DungeonStateSync : MonoBehaviour
{
    // 네트워크 서비스 인터페이스
    private IDungeonSyncService _dungeonService;
    
    // 던전 상태
    [SerializeField] private string _dungeonId;
    
    // 던전 관리자
    [SerializeField] private OnePageDungeon _dungeonManager;
    
    // 권한 관련
    [SerializeField] private bool _hasAuthority;
    
    // 동기화 간격
    [SerializeField] private float _syncInterval = 5f;
    
    private async void Start()
    {
        // MagicOnion 서비스 연결
        _dungeonService = await DungeonSyncServiceClient.CreateAsync(
            GrpcChannelProvider.GetChannel());
        
        // 던전 상태 수신 시작
        ReceiveDungeonUpdatesAsync().Forget();
        
        // 권한이 있는 경우 주기적 상태 전송 시작
        if (_hasAuthority)
        {
            InvokeRepeating(nameof(SyncDungeonState), 0f, _syncInterval);
        }
    }
    
    /// <summary>
    /// 던전 상태 동기화
    /// </summary>
    private async void SyncDungeonState()
    {
        if (!_hasAuthority || _dungeonManager == null)
            return;
        
        try
        {
            // 던전 상태 생성
            DungeonState state = new DungeonState
            {
                DungeonId = _dungeonId,
                DungeonSeed = _dungeonManager.Seed,
                DungeonLayout = _dungeonManager.SerializeLayoutState(),
                EntitiesState = _dungeonManager.SerializeEntitiesState(),
                DoorsState = _dungeonManager.SerializeDoorsState(),
                ItemsState = _dungeonManager.SerializeItemsState(),
                Timestamp = GetNetworkTime()
            };
            
            // 상태 전송
            await _dungeonService.UpdateDungeonStateAsync(state);
            
            Debug.Log($"던전 상태 동기화 완료: {_dungeonId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"던전 상태 동기화 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 던전 상태 수신
    /// </summary>
    private async UniTask ReceiveDungeonUpdatesAsync()
    {
        try
        {
            // 던전 상태 구독
            await foreach (var state in _dungeonService.OnDungeonStateUpdatedAsync(_dungeonId))
            {
                ProcessDungeonState(state);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"던전 상태 수신 오류: {ex.Message}");
            // 재연결 로직
            await UniTask.Delay(1000);
            ReceiveDungeonUpdatesAsync().Forget();
        }
    }
    
    /// <summary>
    /// 던전 상태 처리
    /// </summary>
    private void ProcessDungeonState(DungeonState state)
    {
        if (_dungeonManager == null)
            return;
            
        // 권한이 있는 경우 무시 (자신이 호스트)
        if (_hasAuthority)
            return;
            
        Debug.Log($"던전 상태 수신: {state.DungeonId}, 타임스탬프: {state.Timestamp}");
        
        // 던전 시드 설정 (초기 상태에만)
        if (_dungeonManager.Seed == 0)
        {
            _dungeonManager.Seed = state.DungeonSeed;
            _dungeonManager.GenerateDungeon();
        }
        
        // 던전 레이아웃 상태 적용
        _dungeonManager.DeserializeLayoutState(state.DungeonLayout);
        
        // 엔티티 상태 적용
        _dungeonManager.DeserializeEntitiesState(state.EntitiesState);
        
        // 문 상태 적용
        _dungeonManager.DeserializeDoorsState(state.DoorsState);
        
        // 아이템 상태 적용
        _dungeonManager.DeserializeItemsState(state.ItemsState);
    }
    
    // 네트워크 시간 가져오기 (서버와 동기화된 시간)
    private float GetNetworkTime()
    {
        return Time.time; // 실제로는 서버와 동기화된 시간이 필요
    }
}

/// <summary>
/// 던전 상태 정보
/// </summary>
[MessagePackObject]
public class DungeonState
{
    [Key(0)]
    public string DungeonId { get; set; }
    
    [Key(1)]
    public int DungeonSeed { get; set; }
    
    [Key(2)]
    public byte[] DungeonLayout { get; set; }
    
    [Key(3)]
    public byte[] EntitiesState { get; set; }
    
    [Key(4)]
    public byte[] DoorsState { get; set; }
    
    [Key(5)]
    public byte[] ItemsState { get; set; }
    
    [Key(6)]
    public float Timestamp { get; set; }
}

/// <summary>
/// 던전 동기화 서비스 인터페이스
/// </summary>
public interface IDungeonSyncService : IService<IDungeonSyncService>
{
    // 던전 상태 업데이트
    Task<bool> UpdateDungeonStateAsync(DungeonState state);
    
    // 던전 상태 구독
    IAsyncEnumerable<DungeonState> OnDungeonStateUpdatedAsync(string dungeonId);
}
```

## 네트워크 동기화 최적화 기법

### 대역폭 관리

1. **상태 변경 감지**: 충분한 변화가 있을 때만 상태 업데이트 전송
2. **델타 압축**: 전체 상태가 아닌 변경된 부분만 전송
3. **우선순위 기반 동기화**: 플레이어와 가까운 객체는 더 자주/정확하게 동기화
4. **관심 영역 필터링**: 플레이어 시야 밖의 객체는 더 낮은 빈도로 동기화

### 지연 보상 전략

1. **클라이언트 예측**: 서버 확인을 기다리지 않고 사용자 입력 즉시 적용
2. **리콘실리에이션**: 서버에서 확정된 상태가 클라이언트 예측과 다를 경우 보정
3. **지연 시뮬레이션**: 서버 상태 도착 시간 예측하여 중간 상태 시뮬레이션
4. **시간 동기화**: 클라이언트와 서버 간 시간 차이 보정

### 네트워크 상태 모니터링

1. **핑/지연 시간 추적**: 평균 및 변동성 모니터링
2. **패킷 손실 감지**: 재전송 및 복구 메커니즘 활성화
3. **대역폭 사용량 모니터링**: 실시간 조정을 통한 최적화
4. **성능 메트릭 수집**: 문제 지점 식별 및 최적화

[이전: NetworkSynchronization_part2.md]