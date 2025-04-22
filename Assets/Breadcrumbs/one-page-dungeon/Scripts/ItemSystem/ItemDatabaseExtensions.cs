using System.Collections.Generic;
using System.Linq;
using Breadcrumbs.CharacterSystem;

namespace Breadcrumbs.ItemSystem {
    /// <summary>
    /// 아이템 데이터베이스 확장: 추가 메서드 포함
    /// </summary>
    public static class ItemDatabaseExtensions {
        // 아이템 데이터베이스에서 모든 아이템 가져오기
        public static List<EquipmentItem> GetAllItems(this ItemDatabaseImpl database) {
            List<EquipmentItem> allItems = new List<EquipmentItem>();

            // 현재는 간소화된 구현으로, 실제로는 데이터베이스에서 모든 아이템 ID를 조회하고
            // 각각 GetItemById로 아이템 객체를 불러와야 함

            return allItems;
        }

        // 레벨 범위 내의 아이템 필터링
        public static List<EquipmentItem> GetItemsInLevelRange(this ItemDatabaseImpl database, int minLevel, int maxLevel) {
            return GetAllItems(database).Where(i => i.RequiredLevel >= minLevel && i.RequiredLevel <= maxLevel).ToList();
        }

        // 아이템 점수 범위 내의 아이템 필터링
        public static List<EquipmentItem> GetItemsByScoreRange(this ItemDatabaseImpl database, int minScore, int maxScore) {
            return GetAllItems(database).Where(i => {
                int score = i.CalculateItemScore();
                return score >= minScore && score <= maxScore;
            }).ToList();
        }
    }
}