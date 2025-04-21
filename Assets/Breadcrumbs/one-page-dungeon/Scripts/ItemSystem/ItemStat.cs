using System;

namespace Breadcrumbs.ItemSystem {
    [Serializable]
    public class ItemStat {
        public enum StatType {
            Attack,
            Defense,
            Health,
            Mana,
            CriticalChance,
            CriticalDamage,
            MoveSpeed,
            AttackSpeed
        }

        public StatType type;
        public float value;
    }
}