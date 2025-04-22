using System;

namespace Breadcrumbs.ItemSystem {
    [Serializable]
    public class ItemStat {
        public enum Status {
            Attack,
            Defense,
            Health,
            Mana,
            CriticalChance,
            CriticalDamage,
            MoveSpeed,
            AttackSpeed
        }

        public Status type;
        public float value;
    }
}