namespace Breadcrumbs.CharacterSystem {
    public class StatModifier {
        public float Value;           // 수정자 값
        public StatModifierType Type; // 수정자 타입
        public int Order;             // 적용 순서
        public object Source;         // 수정자 출처 (아이템, 스킬 등)

        public StatModifier(float value, StatModifierType type, int order, object source) {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }

        // 기본 생성자 (순서는 수정자 타입으로 결정)
        public StatModifier(float value, StatModifierType type, object source)
            : this(value, type, (int)type, source) { }
    }
}