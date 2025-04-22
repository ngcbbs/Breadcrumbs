using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Core;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.CharacterSystem {
    public partial class PlayerInventory : MonoBehaviour, IItemOwner, IInventory, INetworkSyncableInventory {
        #region 네트워크 동기화 기능

        // 아이템 추가 동기화
        public void SyncAddItem(ItemData item, int quantity, int slotIndex) {
            // 네트워크를 통해 다른 클라이언트와 동기화하는 메서드
            if (slotIndex >= 0 && slotIndex < inventorySlots.Length) {
                AddItemToSlot(slotIndex, item, quantity);
            } else {
                AddItem(item, quantity);
            }
        }

        // 아이템 제거 동기화
        public void SyncRemoveItem(int slotIndex, int quantity) {
            // 네트워크를 통해 다른 클라이언트와 동기화하는 메서드
            RemoveItemFromSlot(slotIndex, quantity);
        }

        // 아이템 이동 동기화
        public void SyncMoveItem(int fromSlotIndex, int toSlotIndex) {
            // 네트워크를 통해 다른 클라이언트와 동기화하는 메서드
            MoveItem(fromSlotIndex, toSlotIndex);
        }

        // 아이템 장착 동기화
        public void SyncEquipItem(int inventorySlotIndex, EquipmentSlot equipSlot) {
            // 네트워크를 통해 다른 클라이언트와 동기화하는 메서드
            EquipItem(inventorySlotIndex);
        }

        // 아이템 장착 해제 동기화
        public void SyncUnequipItem(EquipmentSlot equipSlot) {
            // 네트워크를 통해 다른 클라이언트와 동기화하는 메서드
            UnequipItem(equipSlot);
        }

        // 인벤토리 전체 상태 가져오기
        public object GetInventorySyncState() {
            // 인벤토리 상태를 직렬화하여 전송할 데이터 생성
            List<InventorySyncData> syncData = new List<InventorySyncData>();

            // 인벤토리 슬롯 상태 추가
            for (int i = 0; i < inventorySlots.Length; i++) {
                if (!inventorySlots[i].IsEmpty()) {
                    syncData.Add(new InventorySyncData {
                        slotType = SlotType.Inventory,
                        slotIndex = i,
                        itemId = inventorySlots[i].item.itemId,
                        quantity = inventorySlots[i].quantity
                    });
                }
            }

            // 장비 슬롯 상태 추가
            foreach (var kvp in equipmentSlots) {
                if (!kvp.Value.IsEmpty()) {
                    syncData.Add(new InventorySyncData {
                        slotType = SlotType.Equipment,
                        slotIndex = (int)kvp.Key,
                        itemId = kvp.Value.item.itemId,
                        quantity = kvp.Value.quantity
                    });
                }
            }

            return syncData;
        }

        // 인벤토리 전체 상태 적용
        public void ApplyInventorySyncState(object state) {
            // 수신한 데이터로 인벤토리 상태 적용
            if (state is List<InventorySyncData> syncData) {
                // 모든 슬롯 초기화
                for (int i = 0; i < inventorySlots.Length; i++) {
                    inventorySlots[i].Clear();
                }

                foreach (var kvp in equipmentSlots) {
                    kvp.Value.Clear();
                }

                // 수신한 데이터로 슬롯 채우기
                foreach (var data in syncData) {
                    // 아이템 DB에서 아이템 데이터 찾기
                    ItemDatabase itemDatabase = ServiceLocator.GetService<ItemDatabase>();
                    // todo: fixme 아이템 타입 다른거 문제 수정 필요.
                    /*
                    ItemData item = itemDatabase?.GetItemById(data.itemId);

                    if (item != null) {
                        if (data.slotType == SlotType.Inventory) {
                            if (data.slotIndex >= 0 && data.slotIndex < inventorySlots.Length) {
                                inventorySlots[data.slotIndex].item = item;
                                inventorySlots[data.slotIndex].quantity = data.quantity;

                                // 이벤트 발생
                                EventManager.Trigger("Inventory.SlotChanged",
                                    new InventorySlotChangedEventData(data.slotIndex, this));
                            }
                        } else if (data.slotType == SlotType.Equipment) {
                            EquipmentSlot equipSlot = (EquipmentSlot)data.slotIndex;
                            if (equipmentSlots.ContainsKey(equipSlot)) {
                                equipmentSlots[equipSlot].item = item;
                                equipmentSlots[equipSlot].quantity = data.quantity;

                                // 이벤트 발생
                                EventManager.Trigger("Equipment.Changed", new EquipmentChangedEventData(equipSlot, this));
                            }
                        }
                    }
                    // */
                }

                Debug.Log("인벤토리 상태가 동기화되었습니다.");
            }
        }

        #endregion
    }
}