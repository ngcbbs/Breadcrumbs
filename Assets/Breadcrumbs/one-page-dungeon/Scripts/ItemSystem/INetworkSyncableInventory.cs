namespace Breadcrumbs.ItemSystem {
    // 네트워크 인터페이스 - 인벤토리 동기화
    public interface INetworkSyncableInventory {
        void SyncAddItem(ItemData item, int quantity, int slotIndex);
        void SyncRemoveItem(int slotIndex, int quantity);
        void SyncMoveItem(int fromSlotIndex, int toSlotIndex);
        void SyncEquipItem(int inventorySlotIndex, EquipmentSlot equipSlot);
        void SyncUnequipItem(EquipmentSlot equipSlot);
        object GetInventorySyncState();
        void ApplyInventorySyncState(object state);
    }
}