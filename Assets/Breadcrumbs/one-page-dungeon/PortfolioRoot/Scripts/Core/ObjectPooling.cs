using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Core
{
    /// <summary>
    /// Object pool implementation for efficient object reuse
    /// </summary>
    /// <typeparam name="T">Type of objects to pool. Must be a Component.</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Queue<T> inactiveObjects = new Queue<T>();
        private readonly List<T> activeObjects = new List<T>();
        private readonly Transform poolContainer;
        private readonly int initialSize;
        private readonly int maxSize;
        private readonly bool expandable;

        /// <summary>
        /// Number of inactive objects available in the pool
        /// </summary>
        public int InactiveCount => inactiveObjects.Count;

        /// <summary>
        /// Number of active objects currently in use
        /// </summary>
        public int ActiveCount => activeObjects.Count;

        /// <summary>
        /// Total number of objects managed by this pool
        /// </summary>
        public int TotalCount => InactiveCount + ActiveCount;

        /// <summary>
        /// Creates a new object pool
        /// </summary>
        /// <param name="prefab">Prefab to instantiate and pool</param>
        /// <param name="initialSize">Initial number of objects to create</param>
        /// <param name="maxSize">Maximum number of objects in the pool</param>
        /// <param name="expandable">Whether the pool can expand beyond initialSize</param>
        /// <param name="parentTransform">Optional parent transform for pooled objects</param>
        public ObjectPool(T prefab, int initialSize, int maxSize = int.MaxValue, bool expandable = true, Transform parentTransform = null)
        {
            this.prefab = prefab;
            this.initialSize = Mathf.Max(0, initialSize);
            this.maxSize = Mathf.Max(this.initialSize, maxSize);
            this.expandable = expandable;

            // Create container for pooled objects
            GameObject container = new GameObject($"{typeof(T).Name} Pool");
            if (parentTransform != null)
            {
                container.transform.SetParent(parentTransform);
            }
            poolContainer = container.transform;

            // Initialize pool with objects
            Initialize();
        }

        /// <summary>
        /// Initialize the pool with the initial objects
        /// </summary>
        private void Initialize()
        {
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        /// <summary>
        /// Create a new pooled object
        /// </summary>
        /// <returns>The newly created object</returns>
        private T CreateNewObject()
        {
            T newObject = UnityEngine.Object.Instantiate(prefab, poolContainer);
            newObject.gameObject.SetActive(false);
            inactiveObjects.Enqueue(newObject);
            return newObject;
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        /// <returns>An available object from the pool</returns>
        public T Get()
        {
            T obj;

            if (inactiveObjects.Count == 0)
            {
                // No inactive objects available
                if (expandable && TotalCount < maxSize)
                {
                    // Create new object if pool can expand and hasn't reached max size
                    obj = CreateNewObject();
                }
                else
                {
                    Debug.LogWarning($"Object pool for {typeof(T).Name} depleted. Consider increasing the pool size.");
                    return null;
                }
            }
            else
            {
                // Get next inactive object
                obj = inactiveObjects.Dequeue();
            }

            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// Get an object from the pool and set its position and rotation
        /// </summary>
        /// <param name="position">Position to set</param>
        /// <param name="rotation">Rotation to set</param>
        /// <returns>An available object from the pool</returns>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        /// <param name="obj">Object to return</param>
        public void Release(T obj)
        {
            if (obj == null)
            {
                Debug.LogError($"Trying to release a null object to {typeof(T).Name} pool.");
                return;
            }

            if (activeObjects.Contains(obj))
            {
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(poolContainer);
                activeObjects.Remove(obj);
                inactiveObjects.Enqueue(obj);
            }
            else
            {
                Debug.LogWarning($"Trying to release an object that isn't managed by this {typeof(T).Name} pool.");
            }
        }

        /// <summary>
        /// Return all active objects to the pool
        /// </summary>
        public void ReleaseAll()
        {
            while (activeObjects.Count > 0)
            {
                Release(activeObjects[0]);
            }
        }

        /// <summary>
        /// Clear the pool and destroy all objects
        /// </summary>
        public void Clear()
        {
            ReleaseAll();
            
            // Destroy all inactive objects
            foreach (T obj in inactiveObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            
            inactiveObjects.Clear();
        }
    }
}
