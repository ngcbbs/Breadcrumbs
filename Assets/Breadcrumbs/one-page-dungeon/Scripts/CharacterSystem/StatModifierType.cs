namespace Breadcrumbs.CharacterSystem {
    public enum StatModifierType {
        Flat,                 // 고정값 추가
        PercentAdditive,      // 퍼센트 가산 (여러 %값이 합쳐짐)
        PercentMultiplicative // 퍼센트 승수 (각 %가 독립적으로 적용)
    }
}