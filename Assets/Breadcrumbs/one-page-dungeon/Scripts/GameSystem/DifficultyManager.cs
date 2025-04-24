using Breadcrumbs.EventSystem;
using Breadcrumbs.Singletons;
using Breadcrumbs.SpawnSystem;
using Breadcrumbs.SpawnSystem.Events;
using UnityEngine;

namespace Breadcrumbs.GameSystem {
    /// <summary>
    /// Manager for game difficulty settings that works with the event system
    /// </summary>
    public class DifficultyManager : EventBehaviour {
        [Header("Settings")]
        [SerializeField]
        private DifficultySettings[] availableDifficulties;
        [SerializeField]
        private DifficultySettings defaultDifficulty;

        private DifficultySettings _currentDifficulty;

        public DifficultySettings CurrentDifficulty => _currentDifficulty;
        public DifficultyLevel CurrentDifficultyLevel => _currentDifficulty?.difficultyLevel ?? DifficultyLevel.Beginner;

        private void Awake() {
            // Set up the default difficulty
            if (defaultDifficulty != null) {
                _currentDifficulty = defaultDifficulty;
            } else if (availableDifficulties != null && availableDifficulties.Length > 0) {
                _currentDifficulty = availableDifficulties[0];
            }
        }

        protected override void RegisterEventHandlers() {
            Register(typeof(GameStartEvent), OnGameStart);
        }

        private void Start() {
            Dispatch(new DifficultyChangedEvent(
                _currentDifficulty.difficultyLevel,
                _currentDifficulty
            ));
        }

        /// <summary>
        /// Change the game difficulty
        /// </summary>
        public void ChangeDifficulty(DifficultyLevel level) {
            foreach (var difficulty in availableDifficulties) {
                if (difficulty.difficultyLevel == level) {
                    _currentDifficulty = difficulty;

                    // Publish event
                    Dispatch(new DifficultyChangedEvent(
                        _currentDifficulty.difficultyLevel,
                        _currentDifficulty
                    ));

                    Debug.Log($"Difficulty changed to {_currentDifficulty.difficultyName}");
                    return;
                }
            }

            Debug.LogWarning($"Difficulty level {level} not found!");
        }

        /// <summary>
        /// Change to the next difficulty level
        /// </summary>
        public void IncreaseDifficulty() {
            if (_currentDifficulty == null || availableDifficulties.Length <= 1) return;

            int currentIndex = System.Array.IndexOf(availableDifficulties, _currentDifficulty);
            if (currentIndex < 0 || currentIndex >= availableDifficulties.Length - 1) return;

            ChangeDifficulty(availableDifficulties[currentIndex + 1].difficultyLevel);
        }

        /// <summary>
        /// Change to the previous difficulty level
        /// </summary>
        public void DecreaseDifficulty() {
            if (_currentDifficulty == null || availableDifficulties.Length <= 1) return;

            int currentIndex = System.Array.IndexOf(availableDifficulties, _currentDifficulty);
            if (currentIndex <= 0) return;

            ChangeDifficulty(availableDifficulties[currentIndex - 1].difficultyLevel);
        }

        /// <summary>
        /// Get a specific difficulty settings by level
        /// </summary>
        public DifficultySettings GetDifficultySettings(DifficultyLevel level) {
            foreach (var difficulty in availableDifficulties) {
                if (difficulty.difficultyLevel == level) {
                    return difficulty;
                }
            }

            return defaultDifficulty;
        }
        
        /// <summary>
        /// Event handler for game start
        /// </summary>
        private void OnGameStart(IEvent @event) {
            if (@event is GameStartEvent) {
                Dispatch(new DifficultyChangedEvent(
                    _currentDifficulty.difficultyLevel,
                    _currentDifficulty
                ));
            }
        }
    }
}