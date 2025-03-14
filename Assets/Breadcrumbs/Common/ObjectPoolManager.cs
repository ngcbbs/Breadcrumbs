using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Breadcrumbs.Common {
    /// <summary>
    /// 여러 타입의 오브젝트 풀을 관리하는 매니저 클래스
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour {
        // 싱글톤 인스턴스
        private static ObjectPoolManager _instance;
        public static ObjectPoolManager Instance {
            get {
                if (_instance == null) {
                    GameObject go = new GameObject("ObjectPoolManager");
                    _instance = go.AddComponent<ObjectPoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // 풀 설정 저장 클래스
        [Serializable]
        public class PoolConfig {
            public GameObject prefab;
            public int defaultCapacity = 10;
            public int maxSize = 100;
            public bool collectionChecks = true;
        }

        // 인스펙터에서 설정할 수 있는 초기 풀 설정 목록
        [SerializeField] private List<PoolConfig> initialPools = new List<PoolConfig>();

        // 타입별 풀 관리 사전
        private Dictionary<Type, IGenericPool> poolsByType = new Dictionary<Type, IGenericPool>();
        
        // 프리팹별 풀 관리 사전
        private Dictionary<GameObject, IGenericPool> poolsByPrefab = new Dictionary<GameObject, IGenericPool>();

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 초기 풀 생성
            InitializeInitialPools();
        }

        private void InitializeInitialPools() {
            foreach (var config in initialPools) {
                if (config.prefab == null) continue;
                
                // 프리팹에서 컴포넌트들을 확인하여 첫 번째 적합한 타입으로 풀 생성
                var allowTypes = new List<Type>();
                var components = config.prefab.GetComponents<Component>();
                foreach (var component in components) {
                    if (component is not IPoolable)
                        continue;
                    allowTypes.Add(component.GetType());
                }

                // 풀생성 규칙 (객체에 IPoolable 인터페이스를 상속받은 컴포넌트는 1개만..) // 여러개 등록이 필요한 경우가 있을까?
                if (allowTypes.Count is <= 0 or > 1) {
                    Debug.LogWarning("풀링 가능한 컴포넌트 타입은 1개만 허용합니다.");
                    continue;
                }
                
                var componentType =  allowTypes[0];
                CreatePool(componentType, config);
            }
        }

        /// <summary>
        /// 지정한 타입의 컴포넌트에 대한 풀을 생성합니다.
        /// </summary>
        public void CreatePool<T>(GameObject prefab, int defaultCapacity = 10, int maxSize = 100, bool collectionChecks = true) where T : Component {
            Type type = typeof(T);
            if (poolsByType.ContainsKey(type)) {
                Debug.LogWarning($"Pool for type {type.Name} already exists");
                return;
            }

            // 프리팹에 필요한 컴포넌트가 있는지 확인
            T component = prefab.GetComponent<T>();
            if (component == null) {
                Debug.LogError($"Prefab does not have component of type {type.Name}");
                return;
            }

            // 풀 설정 생성
            PoolConfig config = new PoolConfig {
                prefab = prefab,
                defaultCapacity = defaultCapacity,
                maxSize = maxSize,
                collectionChecks = collectionChecks
            };

            CreatePool(type, config);
        }
        
        /// <summary>
        /// 지정한 타입의 컴포넌트에 대한 풀을 생성합니다.
        /// </summary>
        public void CreatePool(Type type, GameObject prefab, int defaultCapacity = 10, int maxSize = 100, bool collectionChecks = true) {
            if (poolsByType.ContainsKey(type)) {
                Debug.LogWarning($"Pool for type {type.Name} already exists");
                return;
            }

            // 프리팹에 필요한 컴포넌트가 있는지 확인
            var component = prefab.GetComponent(type);
            if (component == null) {
                Debug.LogError($"Prefab does not have component of type {type.Name}");
                return;
            }

            // 풀 설정 생성
            PoolConfig config = new PoolConfig {
                prefab = prefab,
                defaultCapacity = defaultCapacity,
                maxSize = maxSize,
                collectionChecks = collectionChecks
            };

            CreatePool(type, config);
        }

        /// <summary>
        /// 타입과 설정을 통해 풀을 생성하는 내부 메서드
        /// </summary>
        private void CreatePool(Type type, PoolConfig config) {
            // 이미 등록된 프리팹인지 확인
            if (poolsByPrefab.ContainsKey(config.prefab)) {
                Debug.LogWarning($"Pool for prefab {config.prefab.name} already exists");
                return;
            }

            // 제네릭 풀 생성 메서드 호출
            Type poolType = typeof(GenericComponentPool<>).MakeGenericType(type);
            IGenericPool pool = (IGenericPool)Activator.CreateInstance(poolType, config, transform);

            // 타입과 프리팹별로 풀 저장
            poolsByType[type] = pool;
            poolsByPrefab[config.prefab] = pool;
        }

        /// <summary>
        /// 특정 타입의 오브젝트를 풀에서 가져옵니다.
        /// </summary>
        public T Get<T>(Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component {
            Type type = typeof(T);
            if (!poolsByType.TryGetValue(type, out IGenericPool pool)) {
                Debug.LogError($"No pool exists for type {type.Name}");
                return null;
            }

            GenericComponentPool<T> typedPool = pool as GenericComponentPool<T>;
            return typedPool.Get(position, rotation, parent);
        }

        /// <summary>
        /// 특정 프리팹의 오브젝트를 풀에서 가져옵니다.
        /// </summary>
        public GameObject GetFromPrefab(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null) {
            if (!poolsByPrefab.TryGetValue(prefab, out IGenericPool pool)) {
                Debug.LogError($"No pool exists for prefab {prefab.name}");
                return null;
            }

            return pool.GetGameObject(position, rotation, parent);
        }

        /// <summary>
        /// 특정 타입의 컴포넌트를 풀에 반환합니다.
        /// </summary>
        public void Release<T>(T component) where T : Component {
            Type type = typeof(T);
            if (!poolsByType.TryGetValue(type, out IGenericPool pool)) {
                Debug.LogError($"No pool exists for type {type.Name}");
                return;
            }
    
            if (pool is GenericComponentPool<T> typedPool)
                typedPool.Release(component);
        }

        /// <summary>
        /// GameObject를 풀에 반환합니다.
        /// </summary>
        public void Release(GameObject obj) {
            foreach (var pool in poolsByType.Values) {
                if (pool.Contains(obj)) {
                    pool.ReleaseGameObject(obj);
                    return;
                }
            }

            Debug.LogWarning($"No pool contains object {obj.name}");
        }

        /// <summary>
        /// 모든 풀을 초기화합니다.
        /// </summary>
        public void ClearAllPools() {
            foreach (var pool in poolsByType.Values) {
                pool.Clear();
            }
        }

        /// <summary>
        /// 특정 타입의 풀을 초기화합니다.
        /// </summary>
        public void ClearPool<T>() where T : Component {
            Type type = typeof(T);
            if (poolsByType.TryGetValue(type, out IGenericPool pool)) {
                pool.Clear();
            }
        }

        /// <summary>
        /// 특정 타입의 풀에 활성화 객체 수를반환.
        /// </summary>
        public int Count<T>() where T : Component {
            if (poolsByType.TryGetValue(typeof(T), out var pool)) 
                return pool.ActiveCount;
            return 0;
        }
    }
}