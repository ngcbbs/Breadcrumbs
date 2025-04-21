using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 가능한 오브젝트의 인터페이스
    /// </summary>
    public interface ISpawnable {
        void OnSpawned(Vector3 position, Quaternion rotation);
        void OnDespawned();
    }
}