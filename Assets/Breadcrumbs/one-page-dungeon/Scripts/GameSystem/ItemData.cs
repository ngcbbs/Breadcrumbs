using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰할 아이템의 기본 정보를 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "DungeonGame/ItemData")]
    public class ItemData : ScriptableObject {
        public string itemName;
        public GameObject itemPrefab;
        public ItemRarity rarity;
        public DifficultyLevel minimumDifficulty;
    }
}