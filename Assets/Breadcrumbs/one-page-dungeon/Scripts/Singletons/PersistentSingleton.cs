using UnityEngine;

namespace Breadcrumbs.Singletons {
    public class PersistentSingleton<T> : MonoBehaviour where T : Component {
        public bool autoUnparentOnAwake = true;

        protected static T instance;

        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance {
            get {
                if (instance is null) {
                    instance = FindAnyObjectByType<T>();

                    if (instance is null) {
                        var go = new GameObject($"[{typeof(T).Name}]");
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Awake에서 사용 할 때는 반드시 base.Awake()를 오버라이드 상태로 호출.
        /// </summary>
        protected virtual void Awake() {
            Initialize();
        }

        protected void Initialize() {
            if (!Application.isPlaying)
                return;

            if (autoUnparentOnAwake)
                transform.SetParent(null);

            if (instance == null) {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            } else {
                if (instance != null)
                    Destroy(gameObject);
            }
        }
    }
}