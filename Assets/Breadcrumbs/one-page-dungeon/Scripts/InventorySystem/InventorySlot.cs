using System;
using Breadcrumbs.Core;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.InventorySystem {
    [Serializable]
    public class InventorySlot {
        public ItemData item; // 슬롯에 저장된 아이템 데이터
        public int quantity; // 아이템 수량

        public InventorySlot() {
            Clear();
        }

        // 슬롯이 비어있는지 확인
        public bool IsEmpty() {
            return item == null || quantity <= 0;
        }

        // 슬롯 초기화
        public void Clear() {
            item = null;
            quantity = 0;
        }

        // 아이템 추가 (수량만큼)
        public bool AddItem(ItemData itemData, int amount) {
            if (IsEmpty()) {
                item = itemData;
                quantity = amount;
                return true;
            } else if (item == itemData && item.maxStackSize > 1) {
                int newQuantity = quantity + amount;
                if (newQuantity <= item.maxStackSize) {
                    quantity = newQuantity;
                    return true;
                }
            }

            return false;
        }

        // 아이템 제거 (수량만큼)
        public bool RemoveItem(int amount) {
            if (IsEmpty() || amount > quantity)
                return false;

            quantity -= amount;
            if (quantity <= 0)
                Clear();

            return true;
        }

        // 슬롯 정보 복사
        public void CopyFrom(InventorySlot other) {
            this.item = other.item;
            this.quantity = other.quantity;
        }
    }
}