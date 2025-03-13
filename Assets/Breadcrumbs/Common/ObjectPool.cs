using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Breadcrumbs.Common {
    public class CommonObjectPool<T> : MonoBehaviour where T : Component {
        [Serializable]
        public class PoolConfig {
            public GameObject prefab;
            public int defaultCapacity = 10;
            public int maxSize = 100;
            public bool collectionChecks = true;
        }

        // 풀 설정
        public PoolConfig config;

        // 내부 ObjectPool 인스턴스
        private ObjectPool<GameObject> pool;

        // 생성된 오브젝트와 컴포넌트 매핑
        private Dictionary<GameObject, T> componentCache = new Dictionary<GameObject, T>();

        // 현재 활성화된 오브젝트 수
        private int activeCount = 0;

        // 풀 크기 관련 데이터
        public int DefaultCapacity => config.defaultCapacity;
        public int ActiveCount => activeCount;
        public int TotalCreated => activeCount + (pool?.CountInactive ?? 0);

        protected virtual void Awake() {
            // 프리팹에 필요한 컴포넌트가 있는지 확인
            if (config.prefab != null) {
                T component = config.prefab.GetComponent<T>();
                if (component == null) {
                    Debug.LogError($"Prefab does not have component of type {typeof(T).Name}");
                }
            }

            // 풀 생성
            InitializePool();
        }

        private void InitializePool() {
            pool = new ObjectPool<GameObject>(
                createFunc: CreatePooledItem,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnedToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: config.collectionChecks,
                defaultCapacity: config.defaultCapacity,
                maxSize: config.maxSize
            );
        }

        private GameObject CreatePooledItem() {
            GameObject obj = Instantiate(config.prefab);

            // 가독성을 위해 풀 이름 붙이기
            obj.name = $"{typeof(T).Name}_{componentCache.Count}";

            // 컴포넌트 캐싱
            T component = obj.GetComponent<T>();
            if (component != null) {
                componentCache[obj] = component;
            }

            // 풀 크기 초과 감지
            if (TotalCreated >= config.defaultCapacity) {
                Debug.LogWarning(
                    $"Pool size exceeded for {typeof(T).Name}. Default: {config.defaultCapacity}, Current: {TotalCreated}");
            }

            return obj;
        }

        private void OnTakeFromPool(GameObject obj) {
            obj.SetActive(true);
            activeCount++;

            if (componentCache.TryGetValue(obj, out T component) && component is IPoolable poolable) {
                poolable.OnSpawn();
            }
        }

        private void OnReturnedToPool(GameObject obj) {
            obj.SetActive(false);
            activeCount--;

            if (componentCache.TryGetValue(obj, out T component) && component is IPoolable poolable) {
                poolable.OnDespawn();
            }
        }

        private void OnDestroyPoolObject(GameObject obj) {
            if (componentCache.ContainsKey(obj)) {
                componentCache.Remove(obj);
            }

            Destroy(obj);
        }

        /// <summary>
        /// 풀에서 오브젝트를 가져오고 해당 컴포넌트 반환
        /// </summary>
        public T Get(Vector3 position = default, Quaternion rotation = default, Transform parent = null) {
            GameObject obj = pool.Get();

            // 위치, 회전, 부모 설정
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(parent, false);

            return componentCache[obj];
        }

        /// <summary>
        /// 컴포넌트로 오브젝트를 풀에 반환
        /// </summary>
        public void Release(T component) {
            if (component == null) return;

            var go = component.gameObject;

            if (componentCache.ContainsValue(component)) {
                pool.Release(go);
            }
            else {
                Debug.LogWarning($"Trying to release an object that wasn't created by this pool: {go.name}");
            }
        }

        /// <summary>
        /// GameObject로 오브젝트를 풀에 반환
        /// </summary>
        public void Release(GameObject obj) {
            if (obj == null) return;

            if (componentCache.ContainsKey(obj)) {
                pool.Release(obj);
            }
            else {
                Debug.LogWarning($"Trying to release an object that wasn't created by this pool: {obj.name}");
            }
        }

        /// <summary>
        /// 풀의 모든 오브젝트를 해제
        /// </summary>
        public void Clear() {
            pool.Clear();
            componentCache.Clear();
            activeCount = 0;
        }
    }
}
