using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.CharacterSystem {
    public partial class PlayerInventory : MonoBehaviour, IItemOwner, IInventory, INetworkSyncableInventory {
        #region 아이템 관리 기능

        // 빈 슬롯 인덱스 찾기
        public int FindEmptySlot() {
            for (int i = 0; i < inventorySlots.Length; i++) {
                if (inventorySlots[i].IsEmpty()) {
                    return i;
                }
            }

            return -1; // 빈 슬롯 없음
        }

        // 동일한 아이템이 있고 스택이 가능한 슬롯 찾기
        public int FindStackableSlot(ItemData item) {
            if (item.maxStackSize <= 1) return -1;

            for (int i = 0; i < inventorySlots.Length; i++) {
                if (!inventorySlots[i].IsEmpty() &&
                    inventorySlots[i].item == item &&
                    inventorySlots[i].quantity < item.maxStackSize) {
                    return i;
                }
            }

            return -1; // 스택 가능한 슬롯 없음
        }

        // 아이템 추가 (자동 슬롯 선택)
        public bool AddItem(ItemData item, int quantity) {
            if (item == null || quantity <= 0) return false;

            // 스택 가능한 슬롯 찾기
            int stackSlot = FindStackableSlot(item);
            if (stackSlot != -1) {
                int currentQuantity = inventorySlots[stackSlot].quantity;
                int spaceLeft = item.maxStackSize - currentQuantity;

                if (spaceLeft >= quantity) {
                    // 전부 추가 가능
                    inventorySlots[stackSlot].quantity += quantity;

                    // 이벤트 발생 (슬롯 변경)
                    EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(stackSlot, this));

                    // 이벤트 발생 (아이템 획득)
                    EventManager.Trigger("Item.Pickup", new ItemPickupEventData(item, quantity, playerCharacter));

                    return true;
                } else {
                    // 일부만 추가하고 나머지는 새 슬롯에
                    inventorySlots[stackSlot].quantity = item.maxStackSize;

                    // 이벤트 발생 (슬롯 변경)
                    EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(stackSlot, this));

                    // 나머지 수량 계산
                    int remainingQuantity = quantity - spaceLeft;
                    return AddItem(item, remainingQuantity); // 재귀 호출로 나머지 처리
                }
            }

            // 새 슬롯에 추가
            int emptySlot = FindEmptySlot();
            if (emptySlot != -1) {
                if (quantity <= item.maxStackSize) {
                    // 한 슬롯에 모두 추가 가능
                    inventorySlots[emptySlot].item = item;
                    inventorySlots[emptySlot].quantity = quantity;

                    // 이벤트 발생 (슬롯 변경)
                    EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(emptySlot, this));

                    // 이벤트 발생 (아이템 획득)
                    EventManager.Trigger("Item.Pickup", new ItemPickupEventData(item, quantity, playerCharacter));

                    return true;
                } else {
                    // 최대 스택까지만 추가하고 나머지 재귀 처리
                    inventorySlots[emptySlot].item = item;
                    inventorySlots[emptySlot].quantity = item.maxStackSize;

                    // 이벤트 발생 (슬롯 변경)
                    EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(emptySlot, this));

                    // 나머지 수량 계산
                    int remainingQuantity = quantity - item.maxStackSize;
                    return AddItem(item, remainingQuantity); // 재귀 호출로 나머지 처리
                }
            }

            // 빈 슬롯이 없으면 추가 실패
            Debug.Log("인벤토리가 가득 찼습니다!");
            return false;
        }

        // 아이템을 특정 슬롯에 추가
        public bool AddItemToSlot(int slotIndex, ItemData item, int quantity) {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return false;
            if (item == null || quantity <= 0) return false;

            // 빈 슬롯이면 바로 추가
            if (inventorySlots[slotIndex].IsEmpty()) {
                inventorySlots[slotIndex].item = item;
                inventorySlots[slotIndex].quantity = quantity;

                // 이벤트 발생
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(slotIndex, this));

                return true;
            }
            // 같은 아이템이면 스택 시도
            else if (inventorySlots[slotIndex].item == item) {
                int newQuantity = inventorySlots[slotIndex].quantity + quantity;
                if (newQuantity <= item.maxStackSize) {
                    inventorySlots[slotIndex].quantity = newQuantity;

                    // 이벤트 발생
                    EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(slotIndex, this));

                    return true;
                }
            }

            return false;
        }

        // 아이템 제거
        public bool RemoveItem(ItemData item, int quantity) {
            for (int i = 0; i < inventorySlots.Length; i++) {
                if (!inventorySlots[i].IsEmpty() && inventorySlots[i].item == item) {
                    if (inventorySlots[i].quantity >= quantity) {
                        inventorySlots[i].RemoveItem(quantity);

                        // 이벤트 발생
                        EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(i, this));

                        return true;
                    } else {
                        // 현재 슬롯의 수량을 모두 제거하고 남은 수량 계산
                        int removedQuantity = inventorySlots[i].quantity;
                        inventorySlots[i].Clear();

                        // 이벤트 발생
                        EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(i, this));

                        // 나머지 수량 제거 시도
                        int remainingQuantity = quantity - removedQuantity;
                        if (remainingQuantity > 0) {
                            return RemoveItem(item, remainingQuantity); // 재귀 호출
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        // 특정 슬롯에서 아이템 제거
        public bool RemoveItemFromSlot(int slotIndex, int quantity) {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return false;
            if (inventorySlots[slotIndex].IsEmpty() || quantity <= 0) return false;

            bool result = inventorySlots[slotIndex].RemoveItem(quantity);
            if (result) {
                // 이벤트 발생
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(slotIndex, this));
            }

            return result;
        }

        // 슬롯 간 아이템 이동
        public bool MoveItem(int fromSlotIndex, int toSlotIndex) {
            if (fromSlotIndex < 0 || fromSlotIndex >= inventorySlots.Length ||
                toSlotIndex < 0 || toSlotIndex >= inventorySlots.Length ||
                fromSlotIndex == toSlotIndex) {
                return false;
            }

            // 출발 슬롯이 비어있으면 이동 불가
            if (inventorySlots[fromSlotIndex].IsEmpty()) {
                return false;
            }

            // 도착 슬롯이 비어있는 경우 - 단순 이동
            if (inventorySlots[toSlotIndex].IsEmpty()) {
                inventorySlots[toSlotIndex].CopyFrom(inventorySlots[fromSlotIndex]);
                inventorySlots[fromSlotIndex].Clear();

                // 이벤트 발생
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(fromSlotIndex, this));
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(toSlotIndex, this));

                return true;
            }
            // 같은 아이템이고 스택 가능한 경우 - 합치기 시도
            else if (inventorySlots[fromSlotIndex].item == inventorySlots[toSlotIndex].item &&
                     inventorySlots[fromSlotIndex].item.maxStackSize > 1) {
                int totalQuantity = inventorySlots[fromSlotIndex].quantity + inventorySlots[toSlotIndex].quantity;
                int maxStack = inventorySlots[toSlotIndex].item.maxStackSize;

                if (totalQuantity <= maxStack) {
                    // 전부 합치기 가능
                    inventorySlots[toSlotIndex].quantity = totalQuantity;
                    inventorySlots[fromSlotIndex].Clear();
                } else {
                    // 일부만 합치고 나머지는 원래 슬롯에
                    inventorySlots[toSlotIndex].quantity = maxStack;
                    inventorySlots[fromSlotIndex].quantity = totalQuantity - maxStack;
                }

                // 이벤트 발생
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(fromSlotIndex, this));
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(toSlotIndex, this));

                return true;
            }
            // 다른 아이템인 경우 - 위치 교환
            else {
                InventorySlot temp = new InventorySlot();
                temp.CopyFrom(inventorySlots[toSlotIndex]);
                inventorySlots[toSlotIndex].CopyFrom(inventorySlots[fromSlotIndex]);
                inventorySlots[fromSlotIndex].CopyFrom(temp);

                // 이벤트 발생
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(fromSlotIndex, this));
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(toSlotIndex, this));

                return true;
            }
        }

        // 아이템 사용
        public bool UseItem(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
                return false;

            InventorySlot slot = inventorySlots[slotIndex];
            if (slot.IsEmpty())
                return false;

            ItemData item = slot.item;

            // 장비 아이템은 장착
            if (item.IsEquipment()) {
                return EquipItem(slotIndex);
            }
            // 소모품은 효과 적용 후 수량 감소
            else if (item.IsConsumable()) {
                // 이벤트 발생 (아이템 사용)
                EventManager.Trigger("Item.Used", new ItemUsedEventData(item, playerCharacter));

                // 수량 감소
                slot.RemoveItem(1);

                // 이벤트 발생 (슬롯 변경)
                EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(slotIndex, this));

                return true;
            }

            return false;
        }

        // 아이템 버리기
        public bool DropItem(int slotIndex, int quantity) {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
                return false;

            InventorySlot slot = inventorySlots[slotIndex];
            if (slot.IsEmpty() || quantity <= 0 || quantity > slot.quantity)
                return false;

            ItemData itemToDrop = slot.item;

            // 아이템 필드에 드롭
            if (dropPosition != null) {
                // 아이템 드롭 이벤트 발생
                EventManager.Trigger("Item.Drop",
                    new ItemDropEventData(itemToDrop, quantity, dropPosition.position, playerCharacter));

                Debug.Log($"{quantity}개의 {itemToDrop.itemName}을(를) 버렸습니다.");
            }

            // 인벤토리에서 아이템 제거
            slot.RemoveItem(quantity);

            // 이벤트 발생
            EventManager.Trigger("Inventory.SlotChanged", new InventorySlotChangedEventData(slotIndex, this));

            return true;
        }

        // 인벤토리 자동 정렬
        public void SortInventory(bool byRarity = true) {
            // 임시 리스트에 모든 아이템 저장
            List<(ItemData item, int quantity)> allItems = new List<(ItemData item, int quantity)>();

            // 모든 슬롯에서 아이템 수집
            for (int i = 0; i < inventorySlots.Length; i++) {
                if (!inventorySlots[i].IsEmpty()) {
                    allItems.Add((inventorySlots[i].item, inventorySlots[i].quantity));
                    inventorySlots[i].Clear();
                }
            }

            // 정렬 기준에 따라 정렬
            if (byRarity) {
                // 희귀도 높은 순, 같은 희귀도면 아이템 타입별
                allItems.Sort((a, b) => {
                    if (a.item.rarity != b.item.rarity)
                        return b.item.rarity.CompareTo(a.item.rarity);
                    else
                        return a.item.itemType.CompareTo(b.item.itemType);
                });
            } else {
                // 아이템 타입별, 같은 타입이면 희귀도 높은 순
                allItems.Sort((a, b) => {
                    if (a.item.itemType != b.item.itemType)
                        return a.item.itemType.CompareTo(b.item.itemType);
                    else
                        return b.item.rarity.CompareTo(a.item.rarity);
                });
            }

            // 정렬된 아이템을 다시 인벤토리에 배치
            foreach (var item in allItems) {
                AddItem(item.item, item.quantity);
            }

            // 이벤트 발생
            EventManager.Trigger("Inventory.Sorted", this);

            Debug.Log("인벤토리 정렬 완료");
        }

        // 아이템을 소유할 수 있는지 확인
        public bool CanOwnItem(ItemData item, int quantity) {
            if (item == null || quantity <= 0) return false;

            // 스택 가능한 아이템의 경우 기존 스택에 추가 가능한지 확인
            int stackSlot = FindStackableSlot(item);
            if (stackSlot != -1) {
                int currentQuantity = inventorySlots[stackSlot].quantity;
                int spaceLeft = item.maxStackSize - currentQuantity;

                if (spaceLeft >= quantity)
                    return true;
                else
                    quantity -= spaceLeft; // 남은 수량만 새 슬롯 필요
            }

            // 필요한 빈 슬롯 수 계산
            int requiredSlots = Mathf.CeilToInt((float)quantity / item.maxStackSize);
            int emptySlots = 0;

            for (int i = 0; i < inventorySlots.Length; i++) {
                if (inventorySlots[i].IsEmpty())
                    emptySlots++;

                if (emptySlots >= requiredSlots)
                    return true;
            }

            return false;
        }

        // 아이템 제거 (IItemOwner 인터페이스 구현)
        public bool TakeItem(ItemData item, int quantity) {
            return RemoveItem(item, quantity);
        }

        // 아이템 소유권 이전
        public void TransferOwnership(IItemOwner newOwner, ItemData item, int quantity) {
            if (newOwner == null || item == null || quantity <= 0)
                return;

            // 아이템 소유권 이전이 가능한지 확인
            if (newOwner.CanOwnItem(item, quantity) && this.TakeItem(item, quantity)) {
                newOwner.AddItem(item, quantity);

                // 이벤트 발생
                EventManager.Trigger("Item.Transfer", new ItemTransferEventData(item, quantity, this, newOwner));

                Debug.Log($"{quantity}개의 {item.itemName}을(를) 이전했습니다.");
            } else {
                Debug.Log("아이템 소유권 이전 실패");
            }
        }

        #endregion
    }
}