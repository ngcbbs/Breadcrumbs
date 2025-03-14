using UnityEngine;

namespace Breadcrumbs.Common {
    /// <summary>
    /// 풀 인터페이스 - 여러 타입의 풀을 일관되게 관리하기 위한 인터페이스
    /// </summary>
    public interface IGenericPool {
        GameObject GetGameObject(Vector3 position, Quaternion rotation, Transform parent);
        void ReleaseGameObject(GameObject obj);
        bool Contains(GameObject obj);
        void Clear();
        int ActiveCount { get; }
    }
}