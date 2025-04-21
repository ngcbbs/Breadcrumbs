using Breadcrumbs.InventorySystem;

namespace Breadcrumbs.ItemSystem {
    public static class DragAndDropData {
        // 드래그 중인 아이템 관련 정보
        public static PlayerInventory.SlotType SourceSlotType { get; private set; }
        public static int SourceSlotIndex { get; private set; }
        public static ItemData DraggedItem { get; private set; }
        public static int DraggedQuantity { get; private set; }

        // 드래그 중인지 여부
        public static bool IsDragging { get; private set; }

        // 드래그 시작
        public static void StartDrag(PlayerInventory.SlotType slotType, int slotIndex, ItemData item, int quantity) {
            SourceSlotType = slotType;
            SourceSlotIndex = slotIndex;
            DraggedItem = item;
            DraggedQuantity = quantity;
            IsDragging = true;
        }

        // 드래그 종료
        public static void EndDrag() {
            IsDragging = false;
            DraggedItem = null;
        }
    }
}