using UnityEngine;

namespace Breadcrumbs.Common {
    public class Unit : MonoBehaviour, IPoolable {
        // um...
        public void OnSpawn() {
            Debug.Log($"Unit({name}) Spawn");
        }
        public void OnDespawn() {
            Debug.Log($"Unit({name}) Despawn");
        }
    }
}
