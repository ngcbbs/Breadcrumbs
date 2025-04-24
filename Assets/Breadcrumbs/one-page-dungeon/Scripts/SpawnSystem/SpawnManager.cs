using System.Collections.Generic;
using Breadcrumbs.EventSystem;
using Breadcrumbs.Singletons;
using Breadcrumbs.SpawnSystem.Events;
using Breadcrumbs.SpawnSystem.Strategies;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// Enhanced spawn manager with event system integration
    /// </summary>
    public class SpawnManager : EventBehaviour {
        [Header("Difficulty Settings")]
        [SerializeField] private DifficultySettings currentDifficultySettings;
        public DifficultyLevel CurrentDifficulty => currentDifficultySettings.difficultyLevel;
        public DifficultySettings CurrentDifficultySettings => currentDifficultySettings;

        [Header("Spawn Groups")]
        [SerializeField] private List<SpawnPointGroup> spawnPointGroups = new List<SpawnPointGroup>();

        [Header("References")]
        [SerializeField] private ObjectPoolManager poolManager;

        // Spawned objects tracking
        private Dictionary<GameObject, SpawnPoint> _spawnedObjects = new Dictionary<GameObject, SpawnPoint>();
        
        // Strategy cache
        private Dictionary<SpawnStrategyType, ISpawnStrategy> _strategies = new Dictionary<SpawnStrategyType, ISpawnStrategy>();

        private void Awake() {
            // Ensure we have an object pool manager
            if (poolManager == null) {
                poolManager = FindObjectOfType<ObjectPoolManager>();
                if (poolManager == null) {
                    var poolObj = new GameObject("Object Pool Manager");
                    poolManager = poolObj.AddComponent<ObjectPoolManager>();
                }
            }
           
            // Initialize strategies
            InitializeStrategies();
        }

        protected override void RegisterEventHandlers() {
            Register(typeof(SpawnEvent), OnSpawn);
            Register(typeof(DespawnEvent), OnDespawn);
        }

        private void Start() {
            // Initialize all spawn point groups
            foreach (var group in spawnPointGroups) {
                group.Initialize();
            }
            
            // Broadcast game start event
            EventBehaviour.EventHandler.Dispatch(new GameStartEvent());
        }
        
        /// <summary>
        /// Initialize spawn strategies
        /// </summary>
        private void InitializeStrategies() {
            _strategies[SpawnStrategyType.Default] = new DefaultSpawnStrategy();
            _strategies[SpawnStrategyType.Wave] = new WaveSpawnStrategy(); // Using default parameters
            // Add additional strategies as needed
        }

        /// <summary>
        /// Get a spawn strategy of the specified type
        /// </summary>
        public ISpawnStrategy GetSpawnStrategy(SpawnStrategyType type) {
            if (_strategies.TryGetValue(type, out var strategy)) {
                return strategy;
            }
            
            // Default to the default strategy if requested type is not found
            return _strategies[SpawnStrategyType.Default];
        }

        /// <summary>
        /// Register a spawned object with its spawn point
        /// </summary>
        public void RegisterSpawnedObject(SpawnPoint spawnPoint, GameObject spawnedObject) {
            _spawnedObjects[spawnedObject] = spawnPoint;
        }

        /// <summary>
        /// Handle despawning an object
        /// </summary>
        public void DespawnObject(GameObject obj) {
            if (_spawnedObjects.TryGetValue(obj, out SpawnPoint spawnPoint)) {
                // The spawn point will handle the event publication
                spawnPoint.HandleObjectDespawn(obj);
                _spawnedObjects.Remove(obj);
            }
        }

        /// <summary>
        /// Check player position to trigger spawn points
        /// </summary>
        public void CheckPlayerPositionForSpawn(Vector3 playerPosition) {
            foreach (var group in spawnPointGroups) {
                if (group.IsActive) {
                    foreach (var spawnPoint in group.SpawnPoints) {
                        if (spawnPoint.SpawnTrigger == SpawnTriggerType.PlayerEnter &&
                            spawnPoint.IsInTriggerArea(playerPosition) &&
                            spawnPoint.MeetsDifficultyRequirement(CurrentDifficulty)) {
                            spawnPoint.TriggerSpawn();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Trigger an event for spawn points
        /// </summary>
        public void TriggerEvent(SpawnTriggerType eventType, string eventId) {
            string eventKey = eventType.ToString();
            if (!string.IsNullOrEmpty(eventId)) {
                eventKey += "_" + eventId;
            }
            
            foreach (var group in spawnPointGroups) {
                if (group.IsActive) {
                    foreach (var spawnPoint in group.SpawnPoints) {
                        spawnPoint.OnEventTriggered(eventKey);
                    }
                }
            }
        }

        /// <summary>
        /// Trigger a named event for spawn points
        /// </summary>
        public void TriggerEvent(string eventName) {
            foreach (var group in spawnPointGroups) {
                if (group.IsActive) {
                    foreach (var spawnPoint in group.SpawnPoints) {
                        spawnPoint.OnEventTriggered(eventName);
                    }
                }
            }
        }

        /// <summary>
        /// Activate a spawn point group
        /// </summary>
        public void ActivateSpawnPointGroup(string groupId) {
            foreach (var group in spawnPointGroups) {
                if (group.GroupId == groupId) {
                    group.SetActive(true);
                    // Publish event
                    EventBehaviour.EventHandler.Dispatch(new SpawnGroupActivatedEvent(groupId));
                }
            }
        }

        /// <summary>
        /// Deactivate a spawn point group
        /// </summary>
        public void DeactivateSpawnPointGroup(string groupId) {
            foreach (var group in spawnPointGroups) {
                if (group.GroupId == groupId) {
                    group.SetActive(false);
                    // Publish event
                    EventBehaviour.EventHandler.Dispatch(new SpawnGroupDeactivatedEvent(groupId));
                }
            }
        }

        /// <summary>
        /// Change the game difficulty
        /// </summary>
        public void ChangeDifficulty(DifficultySettings newDifficultySettings) {
            currentDifficultySettings = newDifficultySettings;
            
            // Publish event
            EventBehaviour.EventHandler.Dispatch(new DifficultyChangedEvent(
                newDifficultySettings.difficultyLevel, 
                newDifficultySettings
            ));
            
            Debug.Log($"Difficulty changed to {newDifficultySettings.difficultyName}");
        }
        
        /// <summary>
        /// Event handler implementation
        /// </summary>
        public void OnSpawn(IEvent @event) {
            if (@event is SpawnEvent spawnEvent) {
                Debug.Log($"Spawn event received: {spawnEvent.SpawnedObject.name} at {spawnEvent.SpawnPoint.name}");
                RegisterSpawnedObject(spawnEvent.SpawnPoint, spawnEvent.SpawnedObject);
            } 
        }
        
        /// <summary>
        /// Event handler implementation
        /// </summary>
        public void OnDespawn(IEvent @event) {
            if (@event is DespawnEvent despawnEvent) {
                Debug.Log($"Despawn event received: {despawnEvent.DespawnedObject.name}");
                // Additional despawn handling here if needed
            }
        }
        
        #region Debug Tools
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        
        /// <summary>
        /// Show debug info in the inspector
        /// </summary>
        private void OnGUI() {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.Label("Spawn Manager Debug");
            GUILayout.Label($"Current Difficulty: {CurrentDifficulty}");
            GUILayout.Label($"Active Spawn Groups: {CountActiveGroups()}");
            GUILayout.Label($"Active Spawned Objects: {_spawnedObjects.Count}");
            
            if (GUILayout.Button("Trigger Game Start Event")) {
                EventBehaviour.EventHandler.Dispatch(new GameStartEvent());
            }
            
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Count active spawn groups
        /// </summary>
        private int CountActiveGroups() {
            int count = 0;
            foreach (var group in spawnPointGroups) {
                if (group.IsActive) count++;
            }
            return count;
        }
        #endregion
    }
}