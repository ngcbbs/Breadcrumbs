using System;
using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.InventorySystem {
    // 플레이어 인벤토리 클래스
    public partial class PlayerInventory : MonoBehaviour, IItemOwner, INetworkSyncableInventory {
        [Header("Inventory Settings")]
        [SerializeField]
        private int inventorySize = 28; // 인벤토리 크기
        [SerializeField]
        private Transform dropPosition; // 아이템 드롭 위치

        // 인벤토리 슬롯 배열
        private InventorySlot[] inventorySlots;

        // 장비 슬롯 딕셔너리 (키: 장비 슬롯, 값: 장착된 아이템 데이터)
        private Dictionary<EquipmentSlot, InventorySlot> equipmentSlots = new Dictionary<EquipmentSlot, InventorySlot>();

        // 이벤트 정의
        public event Action<int> OnSlotChanged; // 슬롯 내용 변경 이벤트
        public event Action<EquipmentSlot> OnEquipChanged; // 장비 슬롯 변경 이벤트
        public event Action<ItemData> OnItemPickedUp; // 아이템 획득 이벤트

        private void Awake() {
            // 인벤토리 슬롯 초기화
            inventorySlots = new InventorySlot[inventorySize];
            for (int i = 0; i < inventorySize; i++) {
                inventorySlots[i] = new InventorySlot();
            }

            // 장비 슬롯 초기화
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot))) {
                equipmentSlots[slot] = new InventorySlot();
            }
        }

        #region 인벤토리 기본 기능

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
                    OnSlotChanged?.Invoke(stackSlot);
                    OnItemPickedUp?.Invoke(item);
                    return true;
                } else {
                    // 일부만 추가하고 나머지는 새 슬롯에
                    inventorySlots[stackSlot].quantity = item.maxStackSize;
                    OnSlotChanged?.Invoke(stackSlot);

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
                    OnSlotChanged?.Invoke(emptySlot);
                    OnItemPickedUp?.Invoke(item);
                    return true;
                } else {
                    // 최대 스택까지만 추가하고 나머지 재귀 처리
                    inventorySlots[emptySlot].item = item;
                    inventorySlots[emptySlot].quantity = item.maxStackSize;
                    OnSlotChanged?.Invoke(emptySlot);

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
                OnSlotChanged?.Invoke(slotIndex);
                return true;
            }
            // 같은 아이템이면 스택 시도
            else if (inventorySlots[slotIndex].item == item) {
                int newQuantity = inventorySlots[slotIndex].quantity + quantity;
                if (newQuantity <= item.maxStackSize) {
                    inventorySlots[slotIndex].quantity = newQuantity;
                    OnSlotChanged?.Invoke(slotIndex);
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
                        OnSlotChanged?.Invoke(i);
                        return true;
                    } else {
                        // 현재 슬롯의 수량을 모두 제거하고 남은 수량 계산
                        int removedQuantity = inventorySlots[i].quantity;
                        inventorySlots[i].Clear();
                        OnSlotChanged?.Invoke(i);

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
                OnSlotChanged?.Invoke(slotIndex);
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

                OnSlotChanged?.Invoke(fromSlotIndex);
                OnSlotChanged?.Invoke(toSlotIndex);
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

                OnSlotChanged?.Invoke(fromSlotIndex);
                OnSlotChanged?.Invoke(toSlotIndex);
                return true;
            }
            // 다른 아이템인 경우 - 위치 교환
            else {
                InventorySlot temp = new InventorySlot();
                temp.CopyFrom(inventorySlots[toSlotIndex]);
                inventorySlots[toSlotIndex].CopyFrom(inventorySlots[fromSlotIndex]);
                inventorySlots[fromSlotIndex].CopyFrom(temp);

                OnSlotChanged?.Invoke(fromSlotIndex);
                OnSlotChanged?.Invoke(toSlotIndex);
                return true;
            }
        }

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
            if (itemToEquip.itemType is ItemType.Ring && equipSlot is EquipmentSlot.Ring1 or EquipmentSlot.Ring2) {
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

            OnSlotChanged?.Invoke(inventorySlotIndex);
            OnEquipChanged?.Invoke(equipSlot);

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

            OnSlotChanged?.Invoke(emptySlot);
            OnEquipChanged?.Invoke(equipSlot);

            return true;
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
                // 여기서 아이템 효과를 적용하는 로직 추가
                Debug.Log($"아이템 사용: {item.itemName}");

                // 수량 감소
                slot.RemoveItem(1);
                OnSlotChanged?.Invoke(slotIndex);

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
                // 실제 게임에서는 아이템 드롭 매니저를 통해 처리
                // ItemDropManager.Instance.DropItem(dropPosition.position, itemToDrop, quantity);
                Debug.Log($"{quantity}개의 {itemToDrop.itemName}을(를) 버렸습니다.");
            }

            // 인벤토리에서 아이템 제거
            slot.RemoveItem(quantity);
            OnSlotChanged?.Invoke(slotIndex);

            return true;
        }

        // 인벤토리 자동 정렬 (선택 사항)
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
        }

        #endregion

        #region IItemOwner 인터페이스 구현

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
                Debug.Log($"{quantity}개의 {item.itemName}을(를) 이전했습니다.");
            } else {
                Debug.Log("아이템 소유권 이전 실패");
            }
        }

        #endregion

        #region INetworkSyncableInventory 인터페이스 구현

        // 아이템 추가 동기화
        public void SyncAddItem(ItemData item, int quantity, int slotIndex) {
            // 네트워크를 통해 다른 클라이언트와 동기화하는 메서드
            // 실제 네트워크 구현은 특정 네트워크 솔루션에 맞게 별도 개발 필요
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
                    // ItemData item = ItemDatabase.Instance.GetItemById(data.itemId);
                    ItemData item = null; // 실제 구현에서는 ItemDatabase 사용

                    if (item != null) {
                        if (data.slotType == SlotType.Inventory) {
                            if (data.slotIndex >= 0 && data.slotIndex < inventorySlots.Length) {
                                inventorySlots[data.slotIndex].item = item;
                                inventorySlots[data.slotIndex].quantity = data.quantity;
                                OnSlotChanged?.Invoke(data.slotIndex);
                            }
                        } else if (data.slotType == SlotType.Equipment) {
                            EquipmentSlot equipSlot = (EquipmentSlot)data.slotIndex;
                            if (equipmentSlots.ContainsKey(equipSlot)) {
                                equipmentSlots[equipSlot].item = item;
                                equipmentSlots[equipSlot].quantity = data.quantity;
                                OnEquipChanged?.Invoke(equipSlot);
                            }
                        }
                    }
                }
            }
        }

        // 동기화 데이터 구조
        [System.Serializable]
        public enum SlotType {
            Inventory,
            Equipment
        }

        [System.Serializable]
        public class InventorySyncData {
            public SlotType slotType;
            public int slotIndex;
            public string itemId;
            public int quantity;
        }

        #endregion
    }
}