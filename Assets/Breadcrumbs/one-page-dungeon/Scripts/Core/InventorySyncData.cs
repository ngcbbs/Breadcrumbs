namespace Breadcrumbs.Core {
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
}