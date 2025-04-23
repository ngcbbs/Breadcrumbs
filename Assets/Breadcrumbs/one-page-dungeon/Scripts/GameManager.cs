using Breadcrumbs.Singletons;
using Breadcrumbs.SpawnSystem;
using UnityEngine;

namespace Breadcrumbs.Scripts {
    [DefaultExecutionOrder(-200)]
    public class GameManager : PersistentSingleton<GameManager> {
        [SerializeField]
        private DifficultySettings startDifficultySettings;
        
        protected override void Awake() {
            base.Awake();
            InitializeAllSystems();
            Debug.Log("GameManager Initialized...");
        }

        private void InitializeAllSystems() {
            SpawnManager.Instance.ChangeDifficulty(startDifficultySettings);
        
            // 시작 영역 활성화 (테스트)
            SpawnManager.Instance.ActivateSpawnPointGroup("StartingArea");
        }
    }
}
