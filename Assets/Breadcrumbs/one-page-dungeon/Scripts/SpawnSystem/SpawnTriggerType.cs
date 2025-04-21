namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 트리거 타입 정의
    /// </summary>
    public enum SpawnTriggerType {
        None, // 특별한 트리거 없이 자동 스폰
        PlayerEnter, // 플레이어가 특정 영역에 진입 시 스폰
        Event, // 특정 이벤트 발생 시 스폰
        Timer, // 일정 시간마다 스폰
        BossDeath // 보스 처치 후 스폰
    }
}