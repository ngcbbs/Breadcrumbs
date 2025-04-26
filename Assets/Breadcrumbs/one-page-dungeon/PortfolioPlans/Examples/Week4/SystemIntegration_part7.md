# 시스템 통합 예제 - Part 7

## 시스템 인터페이스 정의

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 시스템 초기화 인터페이스
/// 모든 주요 시스템에서 구현해야 하는 공통 메서드를 정의합니다.
/// </summary>
public interface ISystemInitializer
{
    /// <summary>
    /// 시스템 초기화
    /// </summary>
    UniTask Initialize(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 시스템 종료
    /// </summary>
    void Shutdown();
}

/// <summary>
/// 시스템 상태 저장/로드 인터페이스
/// 상태 지속성이 필요한 시스템에서 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IStatePersistence
{
    /// <summary>
    /// 상태 저장
    /// </summary>
    void SaveState();
    
    /// <summary>
    /// 상태 로드
    /// </summary>
    UniTask LoadState(CancellationToken cancellationToken = default);
}

/// <summary>
/// 시스템 재설정 인터페이스
/// 시스템을 초기 상태로 되돌리는 기능이 필요한 시스템에서 구현합니다.
/// </summary>
public interface ISystemReset
{
    /// <summary>
    /// 시스템 재설정
    /// </summary>
    void Reset();
    
    /// <summary>
    /// 소프트 리셋 (부분 초기화)
    /// </summary>
    void SoftReset();
}

/// <summary>
/// 게임 상태 이벤트 핸들러 인터페이스
/// 게임 상태 변화에 반응해야 하는 시스템에서 구현합니다.
/// </summary>
public interface IGameStateHandler
{
    /// <summary>
    /// 게임 상태 변경 핸들러
    /// </summary>
    void HandleGameStateChanged(GameState newState);
}

/// <summary>
/// 네트워크 이벤트 핸들러 인터페이스
/// 네트워크 이벤트에 반응해야 하는 시스템에서 구현합니다.
/// </summary>
public interface INetworkEventHandler
{
    /// <summary>
    /// 네트워크 플레이어 참가 핸들러
    /// </summary>
    void HandleNetworkPlayerJoined(NetworkPlayerInfo playerInfo);
    
    /// <summary>
    /// 네트워크 플레이어 퇴장 핸들러
    /// </summary>
    void HandleNetworkPlayerLeft(string playerId);
    
    /// <summary>
    /// 네트워크 연결 해제 핸들러
    /// </summary>
    void HandleNetworkDisconnected(DisconnectReason reason);
}
```
