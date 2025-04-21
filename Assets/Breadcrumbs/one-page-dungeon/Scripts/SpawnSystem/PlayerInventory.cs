using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 플레이어 인벤토리 예시
    /// </summary>
    public class PlayerInventory : MonoBehaviour {
        private List<ItemData> items = new List<ItemData>();

        public void AddItem(ItemData item) {
            items.Add(item);
            Debug.Log($"플레이어가 {item.itemName}을(를) 획득했습니다.");
        }
    }
}