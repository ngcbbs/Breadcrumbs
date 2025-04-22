using System.Collections.Generic;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    [CreateAssetMenu(fileName = "New Item", menuName = "Breadcrumbs/Item System/Item Data")]
    public class ItemData : ScriptableObject {
        [Header("Basic Info")]
        public string itemId; // 고유 ID
        public string itemName; // 아이템 이름
        public string description; // 설명
        public ItemType itemType; // 아이템 타입
        public ItemRarity rarity; // 희귀도
        public Sprite icon; // 아이콘
        public GameObject prefab; // 필드에 드롭될 때 보여질 프리팹
        public int maxStackSize = 1; // 최대 중첩 수 (장비는 1, 소모품/재료는 999 등)
        public bool isAutoPickup = false; // 자동 획득 여부

        [Header("Equipment Info")]
        public EquipmentSlot equipSlot; // 장착 슬롯 (장비일 경우만 사용)
        public int durability = 100; // 내구도 (장비일 경우만 사용)

        [Header("Stats")]
        public List<ItemStat> stats = new List<ItemStat>(); // 아이템 스탯 목록

        [Header("Drop Rates")]
        public Dictionary<DungeonDifficulty, float> dropRates = new Dictionary<DungeonDifficulty, float>(); // 난이도별 드롭률

        // 희귀도별 색상 정보 (Static)
        public static readonly Dictionary<ItemRarity, Color> RarityColors = new Dictionary<ItemRarity, Color> {
            { ItemRarity.Common, Color.white },
            { ItemRarity.Uncommon, new Color(0.2f, 0.8f, 0.2f) }, // 그린
            { ItemRarity.Rare, new Color(0.2f, 0.2f, 1.0f) }, // 블루
            { ItemRarity.Epic, new Color(0.8f, 0.2f, 0.8f) }, // 퍼플
            { ItemRarity.Legendary, new Color(1.0f, 0.8f, 0.0f) } // 골드/레전더리
        };

        // 아이템이 장비인지 확인
        public bool IsEquipment() {
            return itemType is ItemType.Weapon or ItemType.Helmet or ItemType.Armor or ItemType.Pants or ItemType.Boots
                or ItemType.Gloves or ItemType.Necklace or ItemType.Ring;
        }

        // 아이템이 소모품인지 확인
        public bool IsConsumable() {
            return itemType == ItemType.Consumable;
        }

        // 아이템의 희귀도 색상 반환
        public Color GetRarityColor() {
            return RarityColors[rarity];
        }
    }
}