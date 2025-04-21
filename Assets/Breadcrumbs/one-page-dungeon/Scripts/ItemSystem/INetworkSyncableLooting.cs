using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    // 네트워크 인터페이스 - 루팅 동기화
    public interface INetworkSyncableLooting {
        void SyncItemDrop(Vector3 position, ItemData item, int quantity);
        void SyncItemPickup(int itemInstanceId, IItemOwner owner);
    }
}