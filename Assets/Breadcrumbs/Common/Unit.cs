using UnityEngine;

namespace Breadcrumbs.Common {
    public class Unit : MonoBehaviour, IPoolable {
        public void Initialize() {
        }
        public void OnSpawn() {
            Debug.Log($"Unit({name}) Spawn");
        }
        public void OnDespawn() {
            Debug.Log($"Unit({name}) Despawn");
        }
    }

    // 버전 관리하는 입장에서는 별로 안좋은 방식인듯~
    /*
    public class Unit : MonoBehaviour, IPoolable {
        private bool alive;
        private float lifeTime;
        public void Initialize() {
            lifeTime = 1f;
            alive = true;
        }

        private void Update() {
            if (!alive)
                return;
            
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f) {
                alive = false;
                ObjectPoolManager.Instance.Release(this);
            }
        }

        // um...
        public void OnSpawn() {
            Debug.Log($"Unit({name}) Spawn");
        }
        public void OnDespawn() {
            Debug.Log($"Unit({name}) Despawn");
        }
    }
    // */
}
