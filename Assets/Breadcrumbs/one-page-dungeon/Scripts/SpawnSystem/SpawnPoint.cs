using System.Collections;
using System.Collections.Generic;
using Breadcrumbs.EventSystem;
using Breadcrumbs.SpawnSystem.Events;
using Breadcrumbs.SpawnSystem.Strategies;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// Enhanced spawn point that implements ISpawnPoint and uses the event system
    /// </summary>
    public class SpawnPoint : EventBehaviour, ISpawnPoint {
        [Header("Basic Settings")]
        [SerializeField] private SpawnableObjectType spawnType;
        [SerializeField] private GameObject spawnPrefab;

        [Header("Spawn Conditions")]
        [SerializeField] private SpawnTriggerType spawnTrigger = SpawnTriggerType.None;
        [SerializeField] private float spawnDelay = 0f;
        [SerializeField] private DifficultyLevel minimumDifficulty = DifficultyLevel.Beginner;
        [SerializeField] private bool respawnAfterDeath = true;
        [SerializeField] private float respawnTime = 30f;

        [Header("Spawn Properties")]
        [SerializeField] private Quaternion initialRotation = Quaternion.identity;
        [SerializeField] private float positionRandomRange = 0f;
        [SerializeField] private Bounds triggerArea = new Bounds(Vector3.zero, new Vector3(5, 5, 5));

        [Header("Spawn Limits")]
        [SerializeField] private int maxSpawnCount = 1;
        [SerializeField] private bool isActive = true;

        [Header("Spawn Strategy")]
        [SerializeField] private SpawnStrategyType strategyType = SpawnStrategyType.Default;
        [SerializeField] private int enemiesPerWave = 3;
        [SerializeField] private float timeBetweenSpawns = 0.5f;
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private int maxWaves = 3;

        // Internal state variables
        private int _currentSpawnCount = 0;
        private bool _triggerActivated = false;
        private float _lastSpawnTime = 0f;
        private List<GameObject> _spawnedObjects = new List<GameObject>();
        private ISpawnStrategy _spawnStrategy;

        #region Properties
        public SpawnTriggerType SpawnTrigger => spawnTrigger;
        public bool IsActive => isActive && _currentSpawnCount < maxSpawnCount;
        public Vector3 SpawnPosition => transform.position;
        public Quaternion SpawnRotation => initialRotation;
        public DifficultyLevel MinimumDifficulty => minimumDifficulty;
        
        /// <summary>
        /// Get the trigger area relative to this spawn point's position
        /// </summary>
        public Bounds GetTriggerArea() {
            return triggerArea;
        }
        
        /// <summary>
        /// Set the spawn rotation
        /// </summary>
        public void SetSpawnRotation(Quaternion rotation) {
            initialRotation = rotation;
        }
        #endregion

        private void Awake() {
            InitializeSpawnStrategy();
        }

        private void InitializeSpawnStrategy() {
            switch (strategyType) {
                case SpawnStrategyType.Wave:
                    _spawnStrategy = new WaveSpawnStrategy(enemiesPerWave, timeBetweenSpawns, timeBetweenWaves, maxWaves);
                    break;
                case SpawnStrategyType.RandomSelection:
                    // Example of how you might set up a random selection strategy
                    // In practice, you would likely configure this through the inspector
                    List<GameObject> options = new List<GameObject> { spawnPrefab };
                    _spawnStrategy = new RandomSelectionStrategy(options);
                    break;
                case SpawnStrategyType.Default:
                default:
                    _spawnStrategy = new DefaultSpawnStrategy();
                    break;
            }
        }

        protected override void RegisterEventHandlers() {
            Register(typeof(GameStartEvent), OnGameStart);
            Register(typeof(DifficultyChangedEvent), OnDifficultyChanged);
            Register(typeof(SpawnGroupActivatedEvent), OnSpawnGroupActivated);
            Register(typeof(SpawnGroupDeactivatedEvent), OnSpawnGroupDeactivated);
        }

        private void OnGameStart(IEvent evt) {
            // Any initialization logic when game starts
            if (spawnTrigger == SpawnTriggerType.GameStart) {
                TriggerSpawn();
            }
        }

        private void OnDifficultyChanged(IEvent evt) {
            var diffEvent = evt as DifficultyChangedEvent;
            if (diffEvent != null) {
                // Check if new difficulty meets our requirements
                if (MeetsDifficultyRequirement(diffEvent.NewDifficulty) && 
                    spawnTrigger == SpawnTriggerType.DifficultyChange) {
                    TriggerSpawn();
                }
            }
        }

        private void OnSpawnGroupActivated(IEvent evt) {
            var groupEvent = evt as SpawnGroupActivatedEvent;
            if (groupEvent != null) {
                // If this spawn point belongs to the activated group
                if (transform.parent != null && 
                    transform.parent.GetComponent<SpawnPointGroup>() != null && 
                    transform.parent.GetComponent<SpawnPointGroup>().GroupId == groupEvent.GroupId) {
                    SetActive(true);
                }
            }
        }

        private void OnSpawnGroupDeactivated(IEvent evt) {
            var groupEvent = evt as SpawnGroupDeactivatedEvent;
            if (groupEvent != null) {
                // If this spawn point belongs to the deactivated group
                if (transform.parent != null && 
                    transform.parent.GetComponent<SpawnPointGroup>() != null && 
                    transform.parent.GetComponent<SpawnPointGroup>().GroupId == groupEvent.GroupId) {
                    SetActive(false);
                }
            }
        }

        private void OnDrawGizmos() {
            // Visualize spawn area in editor
            Gizmos.color = isActive ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(transform.position + triggerArea.center, triggerArea.size);
            
            // Draw spawn direction arrow
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, initialRotation * Vector3.forward * 2f);
        }

        #region ISpawnPoint Implementation
        /// <summary>
        /// Check if spawn point meets the difficulty requirement
        /// </summary>
        public bool MeetsDifficultyRequirement(DifficultyLevel currentDifficulty) {
            return (int)currentDifficulty >= (int)minimumDifficulty;
        }

        /// <summary>
        /// Check if a position is within the trigger area
        /// </summary>
        public bool IsInTriggerArea(Vector3 position) {
            return triggerArea.Contains(position - transform.position);
        }

        /// <summary>
        /// Trigger the spawn process
        /// </summary>
        public GameObject TriggerSpawn() {
            if (!_triggerActivated && IsActive) {
                _triggerActivated = true;

                if (spawnDelay > 0) {
                    StartCoroutine(SpawnWithDelay());
                    return null; // Will spawn later
                } else {
                    return SpawnObject();
                }
            }
            return null;
        }

        /// <summary>
        /// Handle when an object spawned from this point is despawned
        /// </summary>
        public void HandleObjectDespawn(GameObject despawnedObject) {
            if (_spawnedObjects.Contains(despawnedObject)) {
                _spawnedObjects.Remove(despawnedObject);
                _currentSpawnCount--;

                // Call OnDespawned on the ISpawnable interface
                ISpawnable spawnable = despawnedObject.GetComponent<ISpawnable>();
                if (spawnable != null) {
                    spawnable.OnDespawned();
                }

                // Publish despawn event
                Dispatch(new DespawnEvent(despawnedObject));

                // Return object to pool
                despawnedObject.SetActive(false);
                ObjectPoolManager.Instance.ReturnObjectToPool(spawnPrefab, despawnedObject);

                // Schedule respawn if configured
                if (respawnAfterDeath && isActive) {
                    StartCoroutine(RespawnAfterDelay(respawnTime));
                }
            }
        }

        /// <summary>
        /// React to an event trigger
        /// </summary>
        public void OnEventTriggered(string eventKey) {
            if (spawnTrigger == SpawnTriggerType.Event && isActive) {
                TriggerSpawn();
            }
        }

        /// <summary>
        /// Set the active state of this spawn point
        /// </summary>
        public void SetActive(bool active) {
            isActive = active;
        }
        #endregion

        #region Spawn Implementation
        /// <summary>
        /// Execute spawn with delay
        /// </summary>
        private IEnumerator SpawnWithDelay() {
            yield return new WaitForSeconds(spawnDelay);
            SpawnObject();
            _triggerActivated = false;
        }

        /// <summary>
        /// Execute the spawn process
        /// </summary>
        private GameObject SpawnObject() {
            Vector3 spawnPosition = transform.position;

            // Apply random position if configured
            if (positionRandomRange > 0) {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-positionRandomRange, positionRandomRange),
                    0,
                    Random.Range(-positionRandomRange, positionRandomRange)
                );
                spawnPosition += randomOffset;
            }

            // Use the spawn strategy
            GameObject spawnedObject = _spawnStrategy.Execute(this, spawnPrefab);
            
            if (spawnedObject == null) {
                Debug.LogWarning($"Failed to spawn object from {gameObject.name}");
                return null;
            }

            // Process spawned object
            RegisterSpawnedObject(spawnedObject);
            
            return spawnedObject;
        }

        /// <summary>
        /// Register and configure a spawned object
        /// </summary>
        public void RegisterSpawnedObject(GameObject spawnedObject) {
            // Call OnSpawned on the ISpawnable interface
            ISpawnable spawnable = spawnedObject.GetComponent<ISpawnable>();
            if (spawnable != null) {
                spawnable.OnSpawned(this);
            }

            _spawnedObjects.Add(spawnedObject);
            _currentSpawnCount++;
            _lastSpawnTime = Time.time;
            
            // Publish spawn event
            Dispatch(new SpawnEvent(spawnedObject, this));
        }

        /// <summary>
        /// Handle respawn after delay
        /// </summary>
        private IEnumerator RespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);

            if (isActive && _currentSpawnCount < maxSpawnCount) {
                SpawnObject();
            }
        }
        #endregion
    }
}