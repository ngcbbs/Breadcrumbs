using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GamePortfolio.Core;

namespace GamePortfolio.UI {
    /// <summary>
    /// Manages all UI elements and screens in the game
    /// </summary>
    public class UIManager : Singleton<UIManager> {
        [Header("UI Screens")]
        [SerializeField]
        private GameObject mainMenuScreen;
        [SerializeField]
        private GameObject loadingScreen;
        [SerializeField]
        private GameObject gameUIScreen;
        [SerializeField]
        private GameObject pauseMenuScreen;
        [SerializeField]
        private GameObject gameOverScreen;
        [SerializeField]
        private GameObject settingsScreen;

        [Header("Loading Screen")]
        [SerializeField]
        private Slider loadingProgressBar;
        [SerializeField]
        private Text loadingText;
        [SerializeField]
        private Text tipText;
        [SerializeField]
        private string[] loadingTips;

        [Header("Game UI")]
        [SerializeField]
        private Text playerHealthText;
        [SerializeField]
        private Slider playerHealthSlider;
        [SerializeField]
        private Text playerLevelText;
        [SerializeField]
        private Text currentFloorText;
        [SerializeField]
        private Text goldText;
        [SerializeField]
        private GameObject minimap;

        [Header("Pause Menu")]
        [SerializeField]
        private Button resumeButton;
        [SerializeField]
        private Button settingsButton;
        [SerializeField]
        private Button mainMenuButton;
        [SerializeField]
        private Button quitButton;

        [Header("Game Over")]
        [SerializeField]
        private Text gameOverTitleText;
        [SerializeField]
        private Text gameOverSubtitleText;
        [SerializeField]
        private Text statisticsText;
        [SerializeField]
        private Button restartButton;
        [SerializeField]
        private Button gameOverMainMenuButton;

        [Header("Messages")]
        [SerializeField]
        private GameObject messagePanel;
        [SerializeField]
        private Text messageText;
        [SerializeField]
        private float messageDisplayTime = 3f;

        [Header("Connection")]
        [SerializeField]
        private GameObject connectingPanel;
        [SerializeField]
        private GameObject connectionErrorPanel;

        // Initialization status
        private bool isInitialized = false;

        // Coroutine for message display
        private Coroutine messageCoroutine;

        /// <summary>
        /// Initialize the UI manager
        /// </summary>
        public void Initialize() {
            if (isInitialized) return;

            // Hide all screens initially
            HideAllScreens();

            // Set up button listeners
            SetupButtonListeners();

            // Subscribe to game state changes
            if (GameManager.HasInstance) {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }

            isInitialized = true;
            Debug.Log("UIManager initialized");
        }

        private void OnDestroy() {
            // Unsubscribe from game state changes
            if (GameManager.HasInstance) {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        /// <summary>
        /// Hide all UI screens
        /// </summary>
        private void HideAllScreens() {
            SetActive(mainMenuScreen, false);
            SetActive(loadingScreen, false);
            SetActive(gameUIScreen, false);
            SetActive(pauseMenuScreen, false);
            SetActive(gameOverScreen, false);
            SetActive(settingsScreen, false);
            SetActive(messagePanel, false);
            SetActive(connectingPanel, false);
            SetActive(connectionErrorPanel, false);
        }

        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupButtonListeners() {
            // Pause menu buttons
            AddButtonListener(resumeButton, OnResumeButtonClicked);
            AddButtonListener(settingsButton, OnSettingsButtonClicked);
            AddButtonListener(mainMenuButton, OnMainMenuButtonClicked);
            AddButtonListener(quitButton, OnQuitButtonClicked);

            // Game over buttons
            AddButtonListener(restartButton, OnRestartButtonClicked);
            AddButtonListener(gameOverMainMenuButton, OnMainMenuButtonClicked);
        }

        /// <summary>
        /// Helper to add button listener with null check
        /// </summary>
        /// <param name="button">Button</param>
        /// <param name="action">Click action</param>
        private void AddButtonListener(Button button, Action action) {
            if (button != null) {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => action());
            }
        }

        /// <summary>
        /// Helper to set GameObject active state with null check
        /// </summary>
        /// <param name="obj">GameObject</param>
        /// <param name="active">Active state</param>
        private void SetActive(GameObject obj, bool active) {
            if (obj != null) {
                obj.SetActive(active);
            }
        }

        /// <summary>
        /// Handle game state changes
        /// </summary>
        /// <param name="newState">New game state</param>
        private void OnGameStateChanged(GameState newState) {
            switch (newState) {
                case GameState.MainMenu:
                    ShowMainMenu();
                    break;

                case GameState.Loading:
                    ShowLoadingScreen();
                    break;

                case GameState.Playing:
                    ShowGameUI();
                    break;

                case GameState.Paused:
                    ShowPauseMenu();
                    break;

                case GameState.GameOver:
                    ShowGameOverScreen();
                    break;
            }
        }

        /// <summary>
        /// Show main menu screen
        /// </summary>
        public void ShowMainMenu() {
            HideAllScreens();
            SetActive(mainMenuScreen, true);

            // Play menu music if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayMusic(0); // Assuming 0 is menu music
            }
        }

        /// <summary>
        /// Show loading screen
        /// </summary>
        public void ShowLoadingScreen() {
            HideAllScreens();
            SetActive(loadingScreen, true);

            // Reset progress bar
            if (loadingProgressBar != null) {
                loadingProgressBar.value = 0f;
            }

            // Show random tip
            if (tipText != null && loadingTips != null && loadingTips.Length > 0) {
                tipText.text = loadingTips[UnityEngine.Random.Range(0, loadingTips.Length)];
            }

            // Play loading music if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayMusic(1); // Assuming 1 is loading music
            }
        }

        /// <summary>
        /// Update loading progress
        /// </summary>
        /// <param name="progress">Progress value (0-1)</param>
        /// <param name="status">Status text</param>
        public void UpdateLoadingProgress(float progress, string status = null) {
            if (loadingProgressBar != null) {
                loadingProgressBar.value = progress;
            }

            if (loadingText != null && !string.IsNullOrEmpty(status)) {
                loadingText.text = status;
            }
        }

        /// <summary>
        /// Show game UI screen
        /// </summary>
        public void ShowGameUI() {
            HideAllScreens();
            SetActive(gameUIScreen, true);

            // Play game music if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayMusic(2); // Assuming 2 is gameplay music
            }
        }

        /// <summary>
        /// Show pause menu screen
        /// </summary>
        public void ShowPauseMenu() {
            SetActive(pauseMenuScreen, true);

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Pause");
            }
        }

        /// <summary>
        /// Show game over screen
        /// </summary>
        /// <param name="victory">Whether the player won</param>
        /// <param name="statistics">Game statistics</param>
        public void ShowGameOverScreen(bool victory = false, string statistics = null) {
            HideAllScreens();
            SetActive(gameOverScreen, true);

            // Set title based on victory/defeat
            if (gameOverTitleText != null) {
                gameOverTitleText.text = victory ? "Victory!" : "Game Over";
            }

            // Set subtitle
            if (gameOverSubtitleText != null) {
                gameOverSubtitleText.text =
                    victory ? "Congratulations! You've completed the dungeon!" : "You have been defeated...";
            }

            // Set statistics
            if (statisticsText != null) {
                statisticsText.text = string.IsNullOrEmpty(statistics) ? "No statistics available" : statistics;
            }

            // Play victory/defeat music if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayMusic(victory ? 3 : 4); // Assuming 3/4 is victory/defeat music
            }
        }

        /// <summary>
        /// Show settings screen
        /// </summary>
        public void ShowSettingsScreen() {
            SetActive(settingsScreen, true);

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Menu");
            }
        }

        /// <summary>
        /// Hide settings screen
        /// </summary>
        public void HideSettingsScreen() {
            SetActive(settingsScreen, false);

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Back");
            }
        }

        /// <summary>
        /// Show a message to the player
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="duration">Display duration (0 for default)</param>
        public void ShowMessage(string message, float duration = 0) {
            // Stop any existing message coroutine
            if (messageCoroutine != null) {
                StopCoroutine(messageCoroutine);
            }

            // Set message text
            if (messageText != null) {
                messageText.text = message;
            }

            // Show message panel
            SetActive(messagePanel, true);

            // Start coroutine to hide after duration
            float displayTime = duration > 0 ? duration : messageDisplayTime;
            messageCoroutine = StartCoroutine(HideMessageAfterDelay(displayTime));

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Notification");
            }
        }

        /// <summary>
        /// Coroutine to hide message after delay
        /// </summary>
        /// <param name="delay">Delay in seconds</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator HideMessageAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            SetActive(messagePanel, false);
            messageCoroutine = null;
        }

        /// <summary>
        /// Show connecting message
        /// </summary>
        public void ShowConnectingMessage() {
            SetActive(connectingPanel, true);
        }

        /// <summary>
        /// Hide connecting message
        /// </summary>
        public void HideConnectingMessage() {
            SetActive(connectingPanel, false);
        }

        /// <summary>
        /// Show connection error
        /// </summary>
        public void ShowConnectionError() {
            SetActive(connectionErrorPanel, true);

            // Auto-hide after a delay
            StartCoroutine(HideConnectionErrorAfterDelay(3f));
        }

        /// <summary>
        /// Hide connection error after delay
        /// </summary>
        /// <param name="delay">Delay in seconds</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator HideConnectionErrorAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            SetActive(connectionErrorPanel, false);
        }

        /// <summary>
        /// Update player health display
        /// </summary>
        /// <param name="currentHealth">Current health</param>
        /// <param name="maxHealth">Maximum health</param>
        public void UpdatePlayerHealth(int currentHealth, int maxHealth) {
            if (playerHealthText != null) {
                playerHealthText.text = $"{currentHealth}/{maxHealth}";
            }

            if (playerHealthSlider != null) {
                playerHealthSlider.maxValue = maxHealth;
                playerHealthSlider.value = currentHealth;
            }
        }

        /// <summary>
        /// Update player level display
        /// </summary>
        /// <param name="level">Current level</param>
        public void UpdatePlayerLevel(int level) {
            if (playerLevelText != null) {
                playerLevelText.text = $"Level {level}";
            }
        }

        /// <summary>
        /// Update current floor display
        /// </summary>
        /// <param name="floor">Current floor</param>
        public void UpdateCurrentFloor(int floor) {
            if (currentFloorText != null) {
                currentFloorText.text = $"Floor {floor}";
            }
        }

        /// <summary>
        /// Update gold display
        /// </summary>
        /// <param name="gold">Current gold</param>
        public void UpdateGold(int gold) {
            if (goldText != null) {
                goldText.text = $"{gold} Gold";
            }
        }

        /// <summary>
        /// Show/hide minimap
        /// </summary>
        /// <param name="show">Whether to show minimap</param>
        public void ToggleMinimap(bool show) {
            SetActive(minimap, show);
        }

        #region Button Handlers

        /// <summary>
        /// Resume button click handler
        /// </summary>
        private void OnResumeButtonClicked() {
            if (GameManager.HasInstance) {
                GameManager.Instance.ResumeGame();
            }

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }

        /// <summary>
        /// Settings button click handler
        /// </summary>
        private void OnSettingsButtonClicked() {
            ShowSettingsScreen();

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }

        /// <summary>
        /// Main menu button click handler
        /// </summary>
        private void OnMainMenuButtonClicked() {
            if (GameManager.HasInstance) {
                GameManager.Instance.QuitToMainMenu();
            }

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }

        /// <summary>
        /// Quit button click handler
        /// </summary>
        private void OnQuitButtonClicked() {
            if (GameManager.HasInstance) {
                GameManager.Instance.QuitGame();
            }

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }

        /// <summary>
        /// Restart button click handler
        /// </summary>
        private void OnRestartButtonClicked() {
            if (GameManager.HasInstance) {
                // Check game mode and restart accordingly
                if (GameManager.Instance.Settings.GameMode == GameMode.SinglePlayer) {
                    GameManager.Instance.StartSinglePlayerGame();
                } else {
                    GameManager.Instance.StartMultiplayerGame(
                        GameManager.Instance.Settings.ServerAddress,
                        GameManager.Instance.Settings.ServerPort
                    );
                }
            }

            // Play UI sound if audio manager available
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }

        #endregion
    }
}