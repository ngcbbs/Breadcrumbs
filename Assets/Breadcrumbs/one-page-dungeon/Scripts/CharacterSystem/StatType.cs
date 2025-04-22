namespace Breadcrumbs.CharacterSystem {
    public enum StatType {
        // 기본 스탯
        Strength,     // 근력
        Dexterity,    // 민첩
        Intelligence, // 지능
        Vitality,     // 체력
        Wisdom,       // 지혜
        Luck,         // 행운

        // 파생 스탯
        MaxHealth,       // 최대 체력
        MaxMana,         // 최대 마나
        HealthRegen,     // 체력 재생
        ManaRegen,       // 마나 재생
        PhysicalAttack,  // 물리 공격력
        MagicAttack,     // 마법 공격력
        PhysicalDefense, // 물리 방어력
        MagicDefense,    // 마법 방어력
        AttackSpeed,     // 공격 속도
        MovementSpeed,   // 이동 속도
        CriticalChance,  // 치명타 확률
        CriticalDamage,  // 치명타 데미지
        Accuracy,        // 명중률
        Evasion,         // 회피율
    }
}