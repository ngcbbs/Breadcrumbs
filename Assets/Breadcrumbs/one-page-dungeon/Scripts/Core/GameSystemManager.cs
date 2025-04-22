using UnityEngine;
using Breadcrumbs.Core;

namespace Breadcrumbs.Core {
    /// <summary>
    /// 게임 시스템 관리자 - 서비스 로케이터와 이벤트 시스템의 라이프사이클 관리
    /// </summary>
    public class GameSystemManager : MonoBehaviour {
        [Header("게임 설정")]
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        // 매니저 인스턴스
        private static GameSystemManager instance;

        private void Awake() {
            // 싱글톤 패턴 (이 클래스만 싱글톤으로 유지)
            if (instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;

            if (dontDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }

            // 서비스들 초기화
            InitializeServices();
        }

        private void OnDestroy() {
            if (instance == this) {
                // 이벤트 시스템 초기화
                EventManager.Reset();

                // 서비스 로케이터 초기화
                ServiceLocator.Reset();

                instance = null;
            }
        }

        /// <summary>
        /// 서비스 초기화
        /// </summary>
        private void InitializeServices() {
            // 필요한 매니저 컴포넌트들을 게임오브젝트에 추가
            EnsureManagerComponent<CharacterSystem.BuffManager>();

            // 기타 필요한 매니저들...

            Debug.Log("Game systems initialized");
        }

        /// <summary>
        /// 매니저 컴포넌트가 존재하는지 확인하고 없으면 추가
        /// </summary>
        private T EnsureManagerComponent<T>() where T : Component {
            T component = GetComponent<T>();
            if (component == null) {
                component = gameObject.AddComponent<T>();
                Debug.Log($"Added manager component: {typeof(T).Name}");
            }

            return component;
        }

        /// <summary>
        /// 씬 전환 시 호출되어 필요한 초기화 작업 수행
        /// </summary>
        public void OnSceneChanged() {
            // 필요한 경우 서비스 재등록
            // ReregisterServices();

            Debug.Log("Game systems updated for new scene");
        }
    }
}