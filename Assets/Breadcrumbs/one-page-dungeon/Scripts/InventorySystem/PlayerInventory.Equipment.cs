using UnityEngine;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.CharacterSystem {
    public partial class PlayerInventory : MonoBehaviour, IItemOwner, IInventory, INetworkSyncableInventory {
        #region 장비 관리 기능

        // 아이템 장착
        public bool EquipItem(int inventorySlotIndex) {
            if (inventorySlotIndex < 0 || inventorySlotIndex >= inventorySlots.Length)
                return false;

            InventorySlot slot = inventorySlots[inventorySlotIndex];
            if (slot.IsEmpty() || !slot.item.IsEquipment())
                return false;

            ItemData itemToEquip = slot.item;
            EquipmentSlot equipSlot = itemToEquip.equipSlot;

            // 특별 케이스: 반지 슬롯 처리
            if (itemToEquip.itemType == ItemType.Ring) {
                // 첫 번째 반지 슬롯이 비어있으면 거기에 장착
                if (equipmentSlots[EquipmentSlot.Ring1].IsEmpty())
                    equipSlot = EquipmentSlot.Ring1;
                // 아니면 두 번째 반지 슬롯에 장착
                else if (equipmentSlots[EquipmentSlot.Ring2].IsEmpty())
                    equipSlot = EquipmentSlot.Ring2;
                // 두 슬롯 모두 차 있으면 첫 번째 슬롯과 교체
                else
                    equipSlot = EquipmentSlot.Ring1;
            }

            // 이미 장착된 아이템이 있으면 교체
            if (!equipmentSlots[equipSlot].IsEmpty()) {
                ItemData equippedItem = equipmentSlots[equipSlot].item;
                int equippedQuantity = equipmentSlots[equipSlot].quantity;

                // 기존 장착 아이템 제거
                equipmentSlots[equipSlot].Clear();

                // 인벤토리 슬롯에 있던 아이템을 장비 슬롯으로 이동
                equipmentSlots[equipSlot].item = itemToEquip;
                equipmentSlots[equipSlot].quantity = slot.quantity;

                // 인벤토리 슬롯에 기존 장착 아이템 배치
                inventorySlots[inventorySlotIndex].item = equippedItem;
                inventorySlots[inventorySlotIndex].quantity = equippedQuantity;
            } else {
                // 장비 슬롯으로 아이템 이동
                equipmentSlots[equipSlot].item = itemToEquip;
                equipmentSlots[equipSlot].quantity = slot.quantity;

                // 인벤토리 슬롯 비우기
                inventorySlots[inventorySlotIndex].Clear();
            }

            // 이벤트 발생
            EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(inventorySlotIndex, this));
            EventManager.Trigger("Equipment.Changed", new EquipmentChangedEventData(equipSlot, this));

            Debug.Log($"{itemToEquip.itemName}을(를) {equipSlot} 슬롯에 장착했습니다.");

            return true;
        }

        // 아이템 장착 해제
        public bool UnequipItem(EquipmentSlot equipSlot) {
            if (equipmentSlots[equipSlot].IsEmpty())
                return false;

            // 인벤토리에 빈 슬롯 찾기
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1) {
                Debug.Log("인벤토리가 가득 차서 장비를 해제할 수 없습니다!");
                return false;
            }

            // 장비 슬롯에서 인벤토리로 이동
            inventorySlots[emptySlot].CopyFrom(equipmentSlots[equipSlot]);
            equipmentSlots[equipSlot].Clear();

            // 이벤트 발생
            EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(emptySlot, this));
            EventManager.Trigger("Equipment.Changed", new EquipmentChangedEventData(equipSlot, this));

            Debug.Log($"{equipSlot} 슬롯의 장비를 해제했습니다.");

            return true;
        }

        // 장비 아이템 장착 (직접 특정 장비 슬롯에)
        public bool EquipItem(ItemData item, EquipmentSlot targetSlot) {
            if (item == null || !item.IsEquipment())
                return false;

            // 장비 슬롯 검증
            if ((item.equipSlot != targetSlot) &&
                !(item.itemType == ItemType.Ring &&
                  (targetSlot == EquipmentSlot.Ring1 || targetSlot == EquipmentSlot.Ring2))) {
                Debug.Log($"{item.itemName}은(는) {targetSlot} 슬롯에 장착할 수 없습니다.");
                return false;
            }

            // 이미 장착된 아이템 해제
            InventorySlot currentItem = equipmentSlots[targetSlot];
            if (!currentItem.IsEmpty()) {
                // 먼저 인벤토리에 빈 슬롯 찾기
                int emptySlot = FindEmptySlot();
                if (emptySlot == -1) {
                    Debug.Log("인벤토리가 가득 차서 장비를 교체할 수 없습니다!");
                    return false;
                }

                // 기존 장비 인벤토리로 이동
                inventorySlots[emptySlot].CopyFrom(currentItem);
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(emptySlot, this));
            }

            // 장비 슬롯에 아이템 설정
            equipmentSlots[targetSlot].item = item;
            equipmentSlots[targetSlot].quantity = 1;

            // 이벤트 발생
            EventManager.Trigger("Equipment.Changed", new EquipmentChangedEventData(targetSlot, this));

            Debug.Log($"{item.itemName}을(를) {targetSlot} 슬롯에 장착했습니다.");

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

            // 이벤트 발생
            EventManager.Trigger("Equipment.Changed", new EquipmentChangedEventData(equipSlot, this));

            // 아이템 필드에 드롭
            if (dropPosition != null) {
                // 아이템 드롭 이벤트 발생
                EventManager.Trigger("Item.Drop", new ItemDropEventData(item, quantity, dropPosition.position, playerCharacter));

                Debug.Log($"{quantity}개의 {item.itemName}을(를) 버렸습니다.");
            }

            return true;
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

            // 이벤트 발생
            EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(slotIndex, this));

            // 새 슬롯에 아이템 추가
            inventorySlots[emptySlot].item = item;
            inventorySlots[emptySlot].quantity = quantity;

            // 이벤트 발생
            EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(emptySlot, this));

            Debug.Log($"{item.itemName} {quantity}개를 새 슬롯으로 분할했습니다.");

            return true;
        }

        #endregion
    }
}