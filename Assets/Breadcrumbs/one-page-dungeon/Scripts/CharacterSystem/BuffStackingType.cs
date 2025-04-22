namespace Breadcrumbs.CharacterSystem {
    public enum BuffStackingType {
        None,    // 중첩 불가 (동일 버프 동시 적용 불가)
        Refresh, // 갱신만 가능 (지속시간 초기화)
        Stack    // 중첩 가능 (효과 누적)
    }
}