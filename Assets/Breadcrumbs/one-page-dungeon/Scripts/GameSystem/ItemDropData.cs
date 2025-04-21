using System;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 몬스터 처치 시 드롭할 아이템 정보
    /// </summary>
    [Serializable]
    public class ItemDropData {
        public ItemData item;
        public float dropChance; // 0.0 ~ 1.0
    }
}