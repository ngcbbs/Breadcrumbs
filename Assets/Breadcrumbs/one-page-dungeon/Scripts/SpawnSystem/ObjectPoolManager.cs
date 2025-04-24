using System.Collections.Generic;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// Settings for an object pool
    /// </summary>
    [System.Serializable]
    public class ObjectPoolSettings {
        public GameObject prefab;
        public int initialSize = 5;
        public int maxSize = 20;
        public bool expandWhenFull = true;
        public float unusedDespawnTime = 60f;
    }

    /// <summary>
    /// Class to manage object pooling - separated from spawn manager
    /// </summary>
    public class ObjectPoolManager : PersistentSingleton<ObjectPoolManager> {
        [SerializeField]
        private List<ObjectPoolSettings> poolSettings = new List<ObjectPoolSettings>();

        private Dictionary<int, Queue<GameObject>> _objectPools = new Dictionary<int, Queue<GameObject>>();
        private Dictionary<GameObject, float> _lastUsedTime = new Dictionary<GameObject, float>();
        private Dictionary<int, ObjectPoolSettings> _poolSettingsMap = new Dictionary<int, ObjectPoolSettings>();

        protected override void Awake() {
            base.Awake();
            InitializePools();
        }

        /// <summary>
        /// Initialize object pools based on settings
        /// </summary>
        private void InitializePools() {
            foreach (var setting in poolSettings) {
                if (setting.prefab == null) continue;

                int prefabId = setting.prefab.GetInstanceID();
                _poolSettingsMap[prefabId] = setting;

                if (!_objectPools.ContainsKey(prefabId)) {
                    _objectPools[prefabId] = new Queue<GameObject>();
                }

                // Pre-instantiate objects based on initial size
                for (int i = 0; i < setting.initialSize; i++) {
                    GameObject obj = Instantiate(setting.prefab);
                    obj.name = setting.prefab.name + "_" + prefabId + "_" + i;
                    obj.SetActive(false);
                    _objectPools[prefabId].Enqueue(obj);
                    _lastUsedTime[obj] = Time.time;

                    // Set parent to this transform to keep hierarchy clean
                    obj.transform.SetParent(transform);
                }

                Debug.Log($"Initialized pool for {setting.prefab.name} with {setting.initialSize} objects");
            }
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        public GameObject GetObjectFromPool(GameObject prefab) {
            int prefabId = prefab.GetInstanceID();

            if (!_objectPools.ContainsKey(prefabId)) {
                _objectPools[prefabId] = new Queue<GameObject>();

                // Add default settings if not present
                if (!_poolSettingsMap.ContainsKey(prefabId)) {
                    _poolSettingsMap[prefabId] = new ObjectPoolSettings {
                        prefab = prefab,
                        initialSize = 5,
                        maxSize = 20,
                        expandWhenFull = true,
                        unusedDespawnTime = 60f
                    };
                }
            }

            GameObject obj;

            if (_objectPools[prefabId].Count > 0) {
                obj = _objectPools[prefabId].Dequeue();
            } else {
                var settings = _poolSettingsMap[prefabId];

                if (settings.expandWhenFull) {
                    obj = Instantiate(prefab);
                    obj.name = prefab.name + "_" + prefabId + "_" + Random.Range(1000, 9999);
                } else {
                    Debug.LogWarning($"Pool for {prefab.name} is full and not allowed to expand!");
                    return null;
                }
            }

            obj.SetActive(true);
            _lastUsedTime[obj] = Time.time;

            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void ReturnObjectToPool(GameObject prefab, GameObject obj) {
            if (obj == null) return;

            int prefabId = prefab.GetInstanceID();

            if (!_objectPools.ContainsKey(prefabId)) {
                _objectPools[prefabId] = new Queue<GameObject>();
            }

            // Get settings for this pool
            ObjectPoolSettings settings = _poolSettingsMap.ContainsKey(prefabId)
                ? _poolSettingsMap[prefabId]
                : new ObjectPoolSettings { prefab = prefab, maxSize = 20 };

            // Reset the object
            obj.SetActive(false);
            obj.transform.SetParent(transform);

            // Update last used time
            _lastUsedTime[obj] = Time.time;

            // Check pool size limit
            if (_objectPools[prefabId].Count < settings.maxSize) {
                _objectPools[prefabId].Enqueue(obj);
            } else {
                Debug.Log($"Pool for {prefab.name} is full. Destroying excess object.");
                Destroy(obj);
                if (_lastUsedTime.ContainsKey(obj)) {
                    _lastUsedTime.Remove(obj);
                }
            }
        }

        /// <summary>
        /// Manage unused objects in the pools
        /// </summary>
        private void Update() {
            float currentTime = Time.time;
            List<GameObject> objectsToRemove = new List<GameObject>();

            foreach (var poolSetting in poolSettings) {
                if (poolSetting.unusedDespawnTime <= 0) continue;

                int prefabId = poolSetting.prefab.GetInstanceID();
                if (_objectPools.TryGetValue(prefabId, out var pool)) {
                    // Create a temporary list to hold objects we want to keep
                    List<GameObject> objectsToKeep = new List<GameObject>();

                    // Check each object in the pool
                    while (pool.Count > poolSetting.initialSize) {
                        GameObject obj = pool.Dequeue();

                        if (_lastUsedTime.TryGetValue(obj, out float lastUsedTime)) {
                            // If the object has been unused for too long, destroy it
                            if (currentTime - lastUsedTime > poolSetting.unusedDespawnTime) {
                                objectsToRemove.Add(obj);
                                continue;
                            }
                        }

                        // Keep this object
                        objectsToKeep.Add(obj);
                    }

                    // Add the objects we want to keep back to the pool
                    foreach (var obj in objectsToKeep) {
                        pool.Enqueue(obj);
                    }
                }
            }

            // Destroy objects that have been unused for too long
            foreach (var obj in objectsToRemove) {
                _lastUsedTime.Remove(obj);
                Destroy(obj);
            }

            if (objectsToRemove.Count > 0) {
                Debug.Log($"Removed {objectsToRemove.Count} unused objects from pools");
            }
        }

        /// <summary>
        /// Configure a specific pool
        /// </summary>
        public void ConfigurePool(GameObject prefab, int initialSize, int maxSize, bool expandWhenFull = true,
            float unusedDespawnTime = 60f) {
            int prefabId = prefab.GetInstanceID();

            ObjectPoolSettings settings = new ObjectPoolSettings {
                prefab = prefab,
                initialSize = initialSize,
                maxSize = maxSize,
                expandWhenFull = expandWhenFull,
                unusedDespawnTime = unusedDespawnTime
            };

            _poolSettingsMap[prefabId] = settings;

            // If this is a new pool, initialize it
            if (!_objectPools.ContainsKey(prefabId)) {
                _objectPools[prefabId] = new Queue<GameObject>();

                for (int i = 0; i < initialSize; i++) {
                    GameObject obj = Instantiate(prefab);
                    obj.name = prefab.name + "_" + prefabId + "_" + i;
                    obj.SetActive(false);
                    _objectPools[prefabId].Enqueue(obj);
                    _lastUsedTime[obj] = Time.time;
                    obj.transform.SetParent(transform);
                }
            }

            // Update existing pool settings
            bool settingExists = false;
            for (int i = 0; i < poolSettings.Count; i++) {
                if (poolSettings[i].prefab == prefab) {
                    poolSettings[i] = settings;
                    settingExists = true;
                    break;
                }
            }

            // Add new setting if it doesn't exist
            if (!settingExists) {
                poolSettings.Add(settings);
            }
        }

        /// <summary>
        /// Get pool utilization rate
        /// </summary>
        public float GetPoolUtilizationRate(GameObject prefab) {
            int prefabId = prefab.GetInstanceID();

            if (!_objectPools.ContainsKey(prefabId) || !_poolSettingsMap.ContainsKey(prefabId)) {
                return 0f;
            }

            int pooledCount = _objectPools[prefabId].Count;
            int maxCount = _poolSettingsMap[prefabId].maxSize;

            return 1f - ((float)pooledCount / maxCount);
        }
    }
}