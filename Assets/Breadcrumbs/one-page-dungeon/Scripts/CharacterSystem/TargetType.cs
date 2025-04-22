namespace Breadcrumbs.CharacterSystem {
    public enum TargetType {
        Self,        // 자기 자신
        SingleAlly,  // 단일 아군
        SingleEnemy, // 단일 적
        AllAllies,   // 모든 아군
        AllEnemies,  // 모든 적
        Area,        // 지정 영역
        Cone,        // 부채꼴 영역
        Line         // 직선 영역
    }
}