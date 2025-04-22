#if false
using Breadcrumbs.ItemSystem;
using UnityEngine;

namespace Breadcrumbs.InventorySystem {
    public partial class PlayerInventory {
        [Header("Additional Settings")]
        [SerializeField]
        private float maxWeight = 100f;
        [SerializeField]
        private int gold = 0;

        // 속성 접근자
        public int InventorySize => inventorySlots.Length;
        public float MaxWeight => maxWeight;
        public int Gold => gold;

        // 인벤토리 슬롯 직접 액세스 (UI용)
        public InventorySlot GetInventorySlot(int index) {
            if (index < 0 || index >= inventorySlots.Length)
                return new InventorySlot();

            return inventorySlots[index];
        }

        // 장비 슬롯 직접 액세스 (UI용)
        public InventorySlot GetEquipmentSlot(EquipmentSlot slot) {
            if (equipmentSlots.TryGetValue(slot, out InventorySlot equipSlot))
                return equipSlot;

            return new InventorySlot();
        }

        // 아이템 분할
        public bool SplitItem(int slotIndex, int quantity) {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
                return false;

            InventorySlot slot = inventorySlots[slotIndex];
            if (slot.IsEmpty() || quantity <= 0 || quantity >= slot.quantity)
                return false;

            // 빈 슬롯 찾기
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1)
                return false;

            // 원래 슬롯에서 수량 감소
            ItemData item = slot.item;
            slot.quantity -= quantity;
            OnSlotChanged?.Invoke(slotIndex);

            // 새 슬롯에 아이템 추가
            inventorySlots[emptySlot].item = item;
            inventorySlots[emptySlot].quantity = quantity;
            OnSlotChanged?.Invoke(emptySlot);

            return true;
        }

        // 골드 추가
        public void AddGold(int amount) {
            if (amount <= 0) return;

            gold += amount;
            Debug.Log($"{amount} 골드를 획득했습니다. 현재 골드: {gold}");
        }

        // 골드 차감
        public bool SpendGold(int amount) {
            if (amount <= 0) return false;

            if (gold >= amount) {
                gold -= amount;
                Debug.Log($"{amount} 골드를 사용했습니다. 남은 골드: {gold}");
                return true;
            } else {
                Debug.Log($"골드가 부족합니다. 필요: {amount}, 현재: {gold}");
                return false;
            }
        }

        // 전체 무게 계산
        public float GetTotalWeight() {
            float totalWeight = 0f;

            // 인벤토리 아이템 무게 계산
            foreach (var slot in inventorySlots) {
                if (!slot.IsEmpty()) {
                    // 아이템 데이터에 무게 정보 추가 필요
                    // totalWeight += slot.item.weight * slot.quantity;
                    totalWeight += 0.5f * slot.quantity; // 임시 값
                }
            }

            // 장비 아이템 무게 계산
            foreach (var pair in equipmentSlots) {
                if (!pair.Value.IsEmpty()) {
                    // 아이템 데이터에 무게 정보 추가 필요
                    // totalWeight += pair.Value.item.weight;
                    totalWeight += 1.0f; // 임시 값
                }
            }

            return totalWeight;
        }

        // 사용된 슬롯 수 계산
        public int GetUsedSlotCount() {
            int count = 0;

            foreach (var slot in inventorySlots) {
                if (!slot.IsEmpty()) {
                    count++;
                }
            }

            return count;
        }

        // 아이템 직접 장착 (특정 장비 슬롯에)
        public bool EquipItem(ItemData item, EquipmentSlot targetSlot) {
            if (item == null || !item.IsEquipment())
                return false;

            // 장비 슬롯 검증
            if ((item.equipSlot != targetSlot) &&
                !(item.itemType == ItemType.Ring &&
                  (targetSlot == EquipmentSlot.Ring1 || targetSlot == EquipmentSlot.Ring2))) {
                return false;
            }

            // 장비 장착
            equipmentSlots[targetSlot].item = item;
            equipmentSlots[targetSlot].quantity = 1;
            OnEquipChanged?.Invoke(targetSlot);

            return true;
        }

        // 장비 아이템 장착 해제 후 바로 드롭
        public bool UnequipAndDropItem(EquipmentSlot equipSlot) {
            if (equipmentSlots[equipSlot].IsEmpty())
                return false;

            // 드롭할 아이템 정보
            ItemData item = equipmentSlots[equipSlot].item;
            int quantity = equipmentSlots[equipSlot].quantity;

            // 장비 슬롯 비우기
            equipmentSlots[equipSlot].Clear();
            OnEquipChanged?.Invoke(equipSlot);

            // 아이템 필드에 드롭
            if (dropPosition != null) {
                // 실제 게임에서는 아이템 드롭 매니저를 통해 처리
                //ItemDropManager.Instance.DropItem(dropPosition.position, item, quantity);
                Debug.Log($"{quantity}개의 {item.itemName}을(를) 버렸습니다.");
            }

            return true;
        }
    }
}
#endif