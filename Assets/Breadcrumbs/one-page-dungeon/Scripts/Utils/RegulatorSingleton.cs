using UnityEngine;

namespace Breadcrumbs.Utils {
    public class RegulatorSingleton<T> : MonoBehaviour where T : Component {
        protected static T instance;

        public static bool HasInstance => instance != null;

        public float InitializationTime { get; private set; }

        public static T Instance {
            get {
                if (instance == null) {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        var go = new GameObject($"[{typeof(T).Name}]") {
                            hideFlags = HideFlags.HideAndDontSave
                        };
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

        protected virtual void Initialize() {
            if (!Application.isPlaying) return;
            InitializationTime = Time.time;
            DontDestroyOnLoad(gameObject);

            T[] oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (T old in oldInstances) {
                if (old.GetComponent<RegulatorSingleton<T>>().InitializationTime < InitializationTime) {
                    Destroy(old.gameObject);
                }
            }

            if (instance == null) {
                instance = this as T;
            }
        }
    }
}