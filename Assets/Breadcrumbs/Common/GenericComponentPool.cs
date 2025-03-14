using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Breadcrumbs.Common {
    /// <summary>
    /// 특정 타입의 컴포넌트를 관리하는 제네릭 풀 클래스
    /// </summary>
    public class GenericComponentPool<T> : IGenericPool where T : Component {
        // 풀 설정
        private ObjectPoolManager.PoolConfig config;
        
        // 부모 Transform
        private Transform poolParent;
        
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

        public GenericComponentPool(ObjectPoolManager.PoolConfig config, Transform parent) {
            this.config = config;
            this.poolParent = parent;
            
            // 풀 생성
            InitializePool();
        }

        private void InitializePool() {
            // 풀 부모 오브젝트 생성
            GameObject poolContainer = new GameObject($"{typeof(T).Name}_Pool");
            poolContainer.transform.SetParent(poolParent);
            
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
            GameObject obj = UnityEngine.Object.Instantiate(config.prefab);
            
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
            
            UnityEngine.Object.Destroy(obj);
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
        /// IGenericPool 인터페이스 구현 - GameObject를 반환
        /// </summary>
        public GameObject GetGameObject(Vector3 position, Quaternion rotation, Transform parent) {
            GameObject obj = pool.Get();
            
            // 위치, 회전, 부모 설정
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(parent, false);
            
            return obj;
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
        /// IGenericPool 인터페이스 구현 - GameObject를 풀에 반환
        /// </summary>
        public void ReleaseGameObject(GameObject obj) {
            if (obj == null) return;
            
            if (componentCache.ContainsKey(obj)) {
                pool.Release(obj);
            }
            else {
                Debug.LogWarning($"Trying to release an object that wasn't created by this pool: {obj.name}");
            }
        }

        /// <summary>
        /// 오브젝트가 이 풀에 속하는지 확인
        /// </summary>
        public bool Contains(GameObject obj) {
            return componentCache.ContainsKey(obj);
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