using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.CharacterSystem {
    /// <summary>
    /// 인벤토리 인터페이스
    /// </summary>
    public interface IInventory {
        /// <summary>
        /// 빈 슬롯 찾기
        /// </summary>
        int FindEmptySlot();

        /// <summary>
        /// 동일한 아이템이 있고 스택이 가능한 슬롯 찾기
        /// </summary>
        int FindStackableSlot(ItemData item);

        /// <summary>
        /// 아이템 추가 (자동 슬롯 선택)
        /// </summary>
        bool AddItem(ItemData item, int quantity);

        /// <summary>
        /// 아이템을 특정 슬롯에 추가
        /// </summary>
        bool AddItemToSlot(int slotIndex, ItemData item, int quantity);

        /// <summary>
        /// 아이템 제거
        /// </summary>
        bool RemoveItem(ItemData item, int quantity);

        /// <summary>
        /// 특정 슬롯에서 아이템 제거
        /// </summary>
        bool RemoveItemFromSlot(int slotIndex, int quantity);

        /// <summary>
        /// 슬롯 간 아이템 이동
        /// </summary>
        bool MoveItem(int fromSlotIndex, int toSlotIndex);

        /// <summary>
        /// 아이템 장착
        /// </summary>
        bool EquipItem(int inventorySlotIndex);

        /// <summary>
        /// 아이템 장착 해제
        /// </summary>
        bool UnequipItem(EquipmentSlot equipSlot);

        /// <summary>
        /// 아이템 사용
        /// </summary>
        bool UseItem(int slotIndex);

        /// <summary>
        /// 아이템 버리기
        /// </summary>
        bool DropItem(int slotIndex, int quantity);

        /// <summary>
        /// 인벤토리 자동 정렬
        /// </summary>
        void SortInventory(bool byRarity = true);

        /// <summary>
        /// 아이템 소유권 이전
        /// </summary>
        void TransferOwnership(IItemOwner newOwner, ItemData item, int quantity);

        /// <summary>
        /// 인벤토리 슬롯 직접 액세스 (UI용)
        /// </summary>
        InventorySlot GetInventorySlot(int index);

        /// <summary>
        /// 장비 슬롯 직접 액세스 (UI용)
        /// </summary>
        InventorySlot GetEquipmentSlot(EquipmentSlot slot);

        /// <summary>
        /// 아이템 소유 가능 여부 확인
        /// </summary>
        bool CanOwnItem(ItemData item, int quantity);
    }
}