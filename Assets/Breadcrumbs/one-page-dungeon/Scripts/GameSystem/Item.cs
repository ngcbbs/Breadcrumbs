using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 아이템 클래스 예시
    /// </summary>
    public class Item : MonoBehaviour, ISpawnable {
        public ItemData itemData;

        public void OnSpawned(Vector3 position, Quaternion rotation) {
            Debug.Log($"{itemData.itemName}이(가) {position}에 스폰되었습니다.");
        }

        public void OnDespawned() {
            Debug.Log($"{itemData.itemName}이(가) 디스폰 되었습니다.");
        }

        private void OnTriggerEnter(Collider other) {
            // 플레이어와 충돌 시 획득 처리
            if (other.CompareTag("Player")) {
                PlayerInventory inventory = other.GetComponent<PlayerInventory>();
                if (inventory != null) {
                    inventory.AddItem(itemData);
                    SpawnManager.Instance.DespawnObject(gameObject);
                }
            }
        }
    }
}