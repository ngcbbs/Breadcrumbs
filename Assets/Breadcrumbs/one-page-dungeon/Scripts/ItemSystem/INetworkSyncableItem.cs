namespace Breadcrumbs.ItemSystem {
    // 네트워크 인터페이스 - 아이템 동기화
    public interface INetworkSyncableItem {
        object GetSyncState();
        void ApplySyncState(object state);
    }
}