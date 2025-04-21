using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    [CreateAssetMenu(fileName = "New Drop Table", menuName = "Breadcrumbs/Item System/Drop Table")]
    public class DropTable : ScriptableObject {
        [System.Serializable]
        public class DropEntry {
            public ItemData item;
            public int minQuantity = 1;
            public int maxQuantity = 1;

            // 난이도별 드롭 확률 (0 ~ 100%)
            public float normalDropChance = 0f;
            public float hardDropChance = 0f;
            public float hardcoreDropChance = 0f;

            // 주어진 난이도에 맞는 드롭 확률 반환
            public float GetDropChance(DungeonDifficulty difficulty) {
                switch (difficulty) {
                    case DungeonDifficulty.Normal: return normalDropChance;
                    case DungeonDifficulty.Hard: return hardDropChance;
                    case DungeonDifficulty.Hardcore: return hardcoreDropChance;
                    default: return normalDropChance;
                }
            }

            // 드롭할 아이템 수량 결정
            public int GetRandomQuantity() {
                return UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
            }
        }

        public List<DropEntry> dropEntries = new List<DropEntry>();
        public float goldDropChance = 100f; // 골드 드롭 확률
        public int minGold = 0; // 최소 골드
        public int maxGold = 10; // 최대 골드

        // 현재 난이도에 맞춰 아이템 드롭을 결정
        public List<(ItemData item, int quantity)> RollDrops(DungeonDifficulty difficulty) {
            List<(ItemData item, int quantity)> result = new List<(ItemData item, int quantity)>();

            // 각 아이템에 대해 드롭 확률 체크
            foreach (var entry in dropEntries) {
                float dropChance = entry.GetDropChance(difficulty);
                if (UnityEngine.Random.Range(0f, 100f) < dropChance) {
                    result.Add((entry.item, entry.GetRandomQuantity()));
                }
            }

            // 골드 드롭 확률 체크
            if (UnityEngine.Random.Range(0f, 100f) < goldDropChance) {
                int goldAmount = UnityEngine.Random.Range(minGold, maxGold + 1);
                if (goldAmount > 0) {
                    // 골드 아이템 데이터를 찾아 추가 (실제 구현 시 ItemDatabase 등에서 참조)
                    // result.Add((ItemDatabase.Instance.GetItemById("gold"), goldAmount));
                }
            }

            return result;
        }

        // 트랩 활성화로 인한 드롭률 보너스 적용 (선택 사항)
        public List<(ItemData item, int quantity)> RollDropsWithTrapBonus(DungeonDifficulty difficulty, float bonusMultiplier) {
            List<(ItemData item, int quantity)> result = new List<(ItemData item, int quantity)>();

            // 각 아이템에 대해 드롭 확률 체크 (보너스 적용)
            foreach (var entry in dropEntries) {
                float dropChance = entry.GetDropChance(difficulty) * bonusMultiplier;
                if (UnityEngine.Random.Range(0f, 100f) < dropChance) {
                    result.Add((entry.item, entry.GetRandomQuantity()));
                }
            }

            // 골드 드롭 확률 체크 (보너스 적용)
            if (UnityEngine.Random.Range(0f, 100f) < goldDropChance * bonusMultiplier) {
                int goldAmount = UnityEngine.Random.Range(minGold, maxGold + 1);
                if (goldAmount > 0) {
                    // 골드 아이템 데이터를 찾아 추가 (실제 구현 시 ItemDatabase 등에서 참조)
                    // result.Add((ItemDatabase.Instance.GetItemById("gold"), goldAmount));
                }
            }

            return result;
        }
    }
}