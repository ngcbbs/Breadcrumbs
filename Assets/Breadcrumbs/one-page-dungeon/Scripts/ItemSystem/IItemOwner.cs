namespace Breadcrumbs.ItemSystem {
    // 네트워크 인터페이스 - 아이템 소유권 관리
    public interface IItemOwner {
        bool CanOwnItem(ItemData item, int quantity);
        bool AddItem(ItemData item, int quantity);
        bool TakeItem(ItemData item, int quantity);
        void TransferOwnership(IItemOwner newOwner, ItemData item, int quantity);
    }
}