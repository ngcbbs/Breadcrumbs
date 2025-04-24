using UnityEngine;
using Breadcrumbs.EventSystem;
using Breadcrumbs.SpawnSystem.Events;
using Breadcrumbs.SpawnSystem.Strategies;

namespace Breadcrumbs.SpawnSystem.Examples {
    /// <summary>
    /// Demo script that shows how to use the spawn system
    /// </summary>
    public class SpawnSystemDemo : EventBehaviour {
        [Header("Demo Settings")]
        [SerializeField]
        private GameObject defaultPrefab;
        [SerializeField]
        private Transform playerTransform;
        [SerializeField]
        private float checkPlayerPositionInterval = 0.5f;

        [Header("Manual Spawn")]
        [SerializeField]
        private Transform spawnPosition;
        [SerializeField]
        private SpawnStrategyType strategyType = SpawnStrategyType.Default;

        [Header("Wave Settings")]
        [SerializeField]
        private int enemiesPerWave = 5;
        [SerializeField]
        private float timeBetweenSpawns = 0.5f;
        [SerializeField]
        private float timeBetweenWaves = 3f;
        [SerializeField]
        private int maxWaves = 3;

        [Header("Random Settings")]
        [SerializeField]
        private GameObject[] prefabOptions;
        [SerializeField]
        private float[] prefabWeights;

        private float _lastPlayerCheckTime = 0f;

        private SpawnManager _spawnManager;

        private void Start() {
            _spawnManager = FindAnyObjectByType<SpawnManager>();

            // Find player if not set
            if (playerTransform == null) {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) {
                    playerTransform = player.transform;
                }
            }

            // Default spawn position to this transform if not set
            if (spawnPosition == null) {
                spawnPosition = transform;
            }

            Debug.Log("Spawn System Demo initialized. Ready to spawn!");
        }

        protected override void RegisterEventHandlers() {
            Register(typeof(SpawnEvent), OnSpawnEvent);
            Register(typeof(DespawnEvent), OnDespawnEvent);
            Register(typeof(GameStartEvent), OnGameStart);
        }

        private void OnSpawnEvent(IEvent evt) {
            var spawnEvent = evt as SpawnEvent;
            if (spawnEvent == null) return;

            Debug.Log($"Demo received spawn event: {spawnEvent.SpawnedObject.name}");
        }

        private void OnDespawnEvent(IEvent evt) {
            var despawnEvent = evt as DespawnEvent;
            if (despawnEvent == null) return;

            Debug.Log($"Demo received despawn event: {despawnEvent.DespawnedObject.name}");
        }

        private void OnGameStart(IEvent evt) {
            Debug.Log("Game started! Spawning initial enemies...");

            // Demonstrate spawning on game start
            if (_spawnManager != null) {
                // 기본 이벤트 시스템 사용하면 안되나? 확인해보자.
                _spawnManager.TriggerEvent(SpawnTriggerType.GameStart, null);
            }
        }

        private void Update() {
            // Check player position at regular intervals
            if (playerTransform != null && Time.time - _lastPlayerCheckTime > checkPlayerPositionInterval) {
                _lastPlayerCheckTime = Time.time;
                if (_spawnManager != null) {
                    _spawnManager.CheckPlayerPositionForSpawn(playerTransform.position);
                }
            }
        }

        /// <summary>
        /// Demonstrate spawning with the default strategy
        /// </summary>
        public void SpawnWithDefaultStrategy() {
            if (defaultPrefab == null) {
                Debug.LogError("Default prefab not set!");
                return;
            }

            var strategy = new DefaultSpawnStrategy();
            var spawnPoint = new DemoSpawnPoint(spawnPosition.position, Quaternion.identity);

            GameObject spawnedObject = strategy.Execute(spawnPoint, defaultPrefab);

            if (spawnedObject != null) {
                Debug.Log($"Manually spawned {spawnedObject.name} with default strategy");

                // Register with spawn manager
                if (_spawnManager != null)
                    _spawnManager.RegisterSpawnedObject(null, spawnedObject);

                // Dispatch spawn event
                Dispatch(new SpawnEvent(spawnedObject, null));
            }
        }

        /// <summary>
        /// Demonstrate spawning with the wave strategy
        /// </summary>
        public void SpawnWithWaveStrategy() {
            if (defaultPrefab == null) {
                Debug.LogError("Default prefab not set!");
                return;
            }

            var strategy = new WaveSpawnStrategy(enemiesPerWave, timeBetweenSpawns, timeBetweenWaves, maxWaves);
            var spawnPoint = new DemoSpawnPoint(spawnPosition.position, Quaternion.identity);

            GameObject spawnedObject = strategy.Execute(spawnPoint, defaultPrefab);

            if (spawnedObject != null) {
                Debug.Log($"Started wave spawn with {spawnedObject.name} as first enemy");

                // Register with spawn manager
                if (_spawnManager != null)
                    _spawnManager.RegisterSpawnedObject(null, spawnedObject);

                // Dispatch spawn event
                Dispatch(new SpawnEvent(spawnedObject, null));
            }
        }

        /// <summary>
        /// Demonstrate spawning with the random selection strategy
        /// </summary>
        public void SpawnWithRandomStrategy() {
            if (prefabOptions == null || prefabOptions.Length == 0) {
                Debug.LogError("Prefab options not set!");
                return;
            }

            // Create a list of prefabs and weights
            var prefabs = new System.Collections.Generic.List<GameObject>(prefabOptions);
            var weights = prefabWeights != null && prefabWeights.Length == prefabOptions.Length
                ? new System.Collections.Generic.List<float>(prefabWeights)
                : null;

            var strategy = new RandomSelectionStrategy(prefabs, weights);
            var spawnPoint = new DemoSpawnPoint(spawnPosition.position, Quaternion.identity);

            // Use any prefab as parameter - the strategy will override it
            GameObject spawnedObject = strategy.Execute(spawnPoint, prefabOptions[0]);

            if (spawnedObject != null) {
                Debug.Log($"Randomly selected and spawned {spawnedObject.name}");

                // Register with spawn manager
                if (_spawnManager != null)
                    _spawnManager.RegisterSpawnedObject(null, spawnedObject);

                // Dispatch spawn event
                Dispatch(new SpawnEvent(spawnedObject, null));
            }
        }

        /// <summary>
        /// Demo class that implements ISpawnPoint for manual spawning demonstrations
        /// </summary>
        private class DemoSpawnPoint : ISpawnPoint {
            private Vector3 _position;
            private Quaternion _rotation;

            public DemoSpawnPoint(Vector3 position, Quaternion rotation) {
                _position = position;
                _rotation = rotation;
            }

            public bool IsActive => true;
            public Vector3 SpawnPosition => _position;
            public Quaternion SpawnRotation => _rotation;
            public DifficultyLevel MinimumDifficulty => DifficultyLevel.Beginner;

            public GameObject TriggerSpawn() {
                return null;
            }

            public void HandleObjectDespawn(GameObject obj) { }

            public bool IsInTriggerArea(Vector3 position) {
                return false;
            }

            public bool MeetsDifficultyRequirement(DifficultyLevel currentDifficulty) {
                return true;
            }

            public void OnEventTriggered(string eventKey) { }
        }
    }
}