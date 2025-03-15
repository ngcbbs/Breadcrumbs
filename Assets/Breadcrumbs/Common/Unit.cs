using UnityEngine;

namespace Breadcrumbs.Common {
    public class Unit : MonoBehaviour, IPoolable {
        public void Initialize() {
        }
        public void OnSpawn() {
            //Debug.Log($"Unit({name}) Spawn");
        }
        public void OnDespawn() {
            //Debug.Log($"Unit({name}) Despawn");
        }
    }
}
