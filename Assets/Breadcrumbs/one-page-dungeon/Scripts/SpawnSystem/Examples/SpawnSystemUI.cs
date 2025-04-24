using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Breadcrumbs.GameSystem;
using Breadcrumbs.SpawnSystem.Events;
using Breadcrumbs.EventSystem;

namespace Breadcrumbs.SpawnSystem.Examples {
    /// <summary>
    /// UI controller for the spawn system demo
    /// </summary>
    public class SpawnSystemUI : EventBehaviour {
        [Header("UI Elements")]
        [SerializeField]
        private Button triggerSpawnButton;
        [SerializeField]
        private Button triggerWaveButton;
        [SerializeField]
        private Button activateGroupButton;
        [SerializeField]
        private Button deactivateGroupButton;
        [SerializeField]
        private Button increaseDifficultyButton;
        [SerializeField]
        private Button decreaseDifficultyButton;

        [SerializeField]
        private TMP_InputField groupIdInput;
        [SerializeField]
        private TMP_Text statusText;
        [SerializeField]
        private TMP_Text difficultyText;
        [SerializeField]
        private TMP_Text activeGroupsText;
        [SerializeField]
        private TMP_Text spawnedObjectsText;

        [Header("References")]
        [SerializeField]
        private SpawnSystemDemo demo;

        private DifficultyManager _difficultyManager;

        private int _spawnCount = 0;
        private int _despawnCount = 0;
        
        private SpawnManager _spawnManager;

        private void Start() {
            _spawnManager = FindAnyObjectByType<SpawnManager>();
            
            // Set up button listeners
            if (triggerSpawnButton != null)
                triggerSpawnButton.onClick.AddListener(OnTriggerSpawnClicked);

            if (triggerWaveButton != null)
                triggerWaveButton.onClick.AddListener(OnTriggerWaveClicked);

            if (activateGroupButton != null)
                activateGroupButton.onClick.AddListener(OnActivateGroupClicked);

            if (deactivateGroupButton != null)
                deactivateGroupButton.onClick.AddListener(OnDeactivateGroupClicked);

            if (increaseDifficultyButton != null)
                increaseDifficultyButton.onClick.AddListener(OnIncreaseDifficultyClicked);

            if (decreaseDifficultyButton != null)
                decreaseDifficultyButton.onClick.AddListener(OnDecreaseDifficultyClicked);

            // Find demo if not set
            if (demo == null)
                demo = FindAnyObjectByType<SpawnSystemDemo>();

            if (_difficultyManager == null)
                _difficultyManager = FindAnyObjectByType<DifficultyManager>();

            // Initial update
            UpdateUI();
        }

        protected override void RegisterEventHandlers() {
            Register(typeof(SpawnEvent), OnSpawnEvent);
            Register(typeof(DespawnEvent), OnDespawnEvent);
            Register(typeof(DifficultyChangedEvent), OnDifficultyChanged);
            Register(typeof(SpawnGroupActivatedEvent), OnSpawnGroupActivated);
            Register(typeof(SpawnGroupDeactivatedEvent), OnSpawnGroupDeactivated);
        }

        private void Update() {
            // Update UI at regular intervals
            if (Time.frameCount % 30 == 0) // Every 30 frames
            {
                UpdateUI();
            }
        }

        private void UpdateUI() {
            // Update difficulty text
            if (difficultyText != null && _difficultyManager != null) {
                var diff = _difficultyManager.CurrentDifficulty;
                difficultyText.text = diff != null
                    ? $"Difficulty: {diff.difficultyName} ({diff.difficultyLevel})"
                    : "Difficulty: Not Set";
            }

            // Update status text
            if (statusText != null) {
                statusText.text = $"Spawned: {_spawnCount} | Despawned: {_despawnCount}";
            }

            // Update active groups text
            if (activeGroupsText != null) {
                int activeGroups = CountActiveGroups();
                activeGroupsText.text = $"Active Groups: {activeGroups}";
            }

            // Update spawned objects text
            if (spawnedObjectsText != null) {
                int spawnedObjects = CountSpawnedObjects();
                spawnedObjectsText.text = $"Active Objects: {spawnedObjects}";
            }
        }

        // Event handlers
        private void OnSpawnEvent(IEvent evt) {
            var spawnEvent = evt as SpawnEvent;
            if (spawnEvent == null) return;

            _spawnCount++;
            UpdateUI();
        }

        private void OnDespawnEvent(IEvent evt) {
            var despawnEvent = evt as DespawnEvent;
            if (despawnEvent == null) return;

            _despawnCount++;
            UpdateUI();
        }

        private void OnDifficultyChanged(IEvent evt) {
            UpdateUI();
        }

        private void OnSpawnGroupActivated(IEvent evt) {
            UpdateUI();
        }

        private void OnSpawnGroupDeactivated(IEvent evt) {
            UpdateUI();
        }

        // Button handlers
        private void OnTriggerSpawnClicked() {
            if (demo != null) {
                demo.SpawnWithDefaultStrategy();
            }
        }

        private void OnTriggerWaveClicked() {
            if (demo != null) {
                demo.SpawnWithWaveStrategy();
            }
        }

        private void OnActivateGroupClicked() {
            string groupId = groupIdInput != null ? groupIdInput.text : "";

            if (string.IsNullOrEmpty(groupId)) {
                Debug.LogWarning("No group ID entered!");
                return;
            }

            if (_spawnManager != null) {
                _spawnManager.ActivateSpawnPointGroup(groupId);
                Debug.Log($"Activated spawn group: {groupId}");
            }
        }

        private void OnDeactivateGroupClicked() {
            string groupId = groupIdInput != null ? groupIdInput.text : "";

            if (string.IsNullOrEmpty(groupId)) {
                Debug.LogWarning("No group ID entered!");
                return;
            }

            if (_spawnManager != null) {
                _spawnManager.DeactivateSpawnPointGroup(groupId);
                Debug.Log($"Deactivated spawn group: {groupId}");
            }
        }

        private void OnIncreaseDifficultyClicked() {
            if (_difficultyManager != null) {
                _difficultyManager.IncreaseDifficulty();
            }
        }

        private void OnDecreaseDifficultyClicked() {
            if (_difficultyManager != null) {
                _difficultyManager.DecreaseDifficulty();
            }
        }

        // Helper methods
        private int CountActiveGroups() {
            int count = 0;
            SpawnPointGroup[] groups = FindObjectsByType<SpawnPointGroup>(FindObjectsSortMode.None);

            foreach (var group in groups) {
                if (group.IsActive) {
                    count++;
                }
            }

            return count;
        }

        private int CountSpawnedObjects() {
            // This is an approximation - actual spawned objects tracking is in the SpawnManager
            MonsterSpawnable[] monsters = FindObjectsByType<MonsterSpawnable>(FindObjectsSortMode.None);
            return monsters.Length;
        }
    }
}
