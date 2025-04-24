using Breadcrumbs.EventSystem;
using Breadcrumbs.SpawnSystem;
using UnityEngine;

namespace Breadcrumbs.Scripts {
    [DefaultExecutionOrder(-200)]
    public class GameManager : EventBehaviour {
        [SerializeField]
        private DifficultySettings startDifficultySettings;

        private SpawnManager _spawnManager;

        private void Awake() {
            InitializeAllSystems();
            Debug.Log("GameManager Initialized...");
        }

        private void Start() {
            _spawnManager = FindAnyObjectByType<SpawnManager>();
        }

        private void InitializeAllSystems() {
            if (_spawnManager != null) {
                _spawnManager.ChangeDifficulty(startDifficultySettings);
                _spawnManager.ActivateSpawnPointGroup("StartingArea");
            }
        }

        protected override void RegisterEventHandlers() { }
    }
}
