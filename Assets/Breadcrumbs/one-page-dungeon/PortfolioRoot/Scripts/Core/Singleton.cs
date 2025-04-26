using UnityEngine;

namespace GamePortfolio.Core
{
    /// <summary>
    /// Generic singleton pattern implementation for MonoBehaviour classes.
    /// </summary>
    /// <typeparam name="T">Type of the singleton class.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        /// <summary>
        /// Gets the singleton instance. Creates a new instance if one doesn't exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[Singleton] Multiple instances of '{typeof(T)}' found. Should only have one.");
                            return instance;
                        }

                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T)} (Singleton)";

                            DontDestroyOnLoad(singletonObject);

                            Debug.Log($"[Singleton] Created new instance of '{typeof(T)}'.");
                        }
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// Check if the singleton instance exists without creating it
        /// </summary>
        public static bool HasInstance => instance != null && !applicationIsQuitting;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] Another instance of '{typeof(T)}' already exists. Destroying this duplicate.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
    }
}
