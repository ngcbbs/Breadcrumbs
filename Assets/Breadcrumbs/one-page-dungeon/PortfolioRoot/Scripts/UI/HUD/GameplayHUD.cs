using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Character;

namespace GamePortfolio.UI.HUD {
    /// <summary>
    /// Manages the in-game HUD (Heads-Up Display)
    /// </summary>
    public class GameplayHUD : MonoBehaviour {
        [Header("Player Status")]
        [SerializeField]
        private Text playerNameText;
        [SerializeField]
        private Slider healthSlider;
        [SerializeField]
        private Text healthText;
        [SerializeField]
        private Slider staminaSlider;
        [SerializeField]
        private Text staminaText;
        [SerializeField]
        private Text levelText;
        [SerializeField]
        private Text goldText;

        [Header("Dungeon Info")]
        [SerializeField]
        private Text floorText;
        [SerializeField]
        private Text roomCountText;
        [SerializeField]
        private Text timerText;
        [SerializeField]
        private GameObject minimapPanel;
        [SerializeField]
        private RawImage minimapImage;

        [Header("Inventory")]
        [SerializeField]
        private GameObject inventoryPanel;
        [SerializeField]
        private Transform itemSlotsContainer;
        [SerializeField]
        private GameObject itemSlotPrefab;

        [Header("Action Bar")]
        [SerializeField]
        private Transform actionBarContainer;
        [SerializeField]
        private GameObject actionSlotPrefab;

        [Header("Game Messages")]
        [SerializeField]
        private GameObject messagePanel;
        [SerializeField]
        private Text messageText;
        [SerializeField]
        private float messageDisplayTime = 3f;

        [Header("Buttons")]
        [SerializeField]
        private Button inventoryButton;
        [SerializeField]
        private Button mapButton;
        [SerializeField]
        private Button pauseButton;

        // Reference to player
        private PlayerStats playerStats;

        // Timer variables
        private float gameTimer = 0f;
        private bool timerRunning = false;

        // Message coroutine reference
        private Coroutine messageCoroutine;

        private void Awake() {
            // Hide panels initially
            SetActive(inventoryPanel, false);
            SetActive(messagePanel, false);

            // Setup button listeners
            SetupButtonListeners();
        }

        private void Start() {
            // Find player stats
            if (playerStats == null) {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null) {
                    playerStats = player.GetComponent<PlayerStats>();
                }
            }

            // Subscribe to player events if stats found
            if (playerStats != null) {
                playerStats.OnHealthChanged += UpdateHealth;
                playerStats.OnStaminaChanged += UpdateStamina;
                playerStats.OnLevelChanged += UpdateLevel;
                playerStats.OnGoldChanged += UpdateGold;
            }

            // Start game timer
            StartTimer();

            // Set player name
            if (playerNameText != null && GameManager.HasInstance && GameManager.Instance.Settings != null) {
                playerNameText.text = GameManager.Instance.Settings.PlayerName;
            }

            // Update status displays
            UpdateAllDisplays();
        }

        private void OnDestroy() {
            // Unsubscribe from player events
            if (playerStats != null) {
                playerStats.OnHealthChanged -= UpdateHealth;
                playerStats.OnStaminaChanged -= UpdateStamina;
                playerStats.OnLevelChanged -= UpdateLevel;
                playerStats.OnGoldChanged -= UpdateGold;
            }
        }

        private void Update() {
            // Update timer if running
            if (timerRunning) {
                gameTimer += Time.deltaTime;
                UpdateTimerDisplay();
            }

            // Check for keyboard shortcuts
            CheckShortcuts();
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
        /// Setup button click listeners
        /// </summary>
        private void SetupButtonListeners() {
            // Inventory button
            if (inventoryButton != null) {
                inventoryButton.onClick.AddListener(() => {
                    ToggleInventory();
                    PlayButtonSound();
                });
            }

            // Map button
            if (mapButton != null) {
                mapButton.onClick.AddListener(() => {
                    ToggleMinimap();
                    PlayButtonSound();
                });
            }

            // Pause button
            if (pauseButton != null) {
                pauseButton.onClick.AddListener(() => {
                    PauseGame();
                    PlayButtonSound();
                });
            }
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayButtonSound() {
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }

        /// <summary>
        /// Check for keyboard shortcuts
        /// </summary>
        private void CheckShortcuts() {
            // Inventory toggle (I key)
            if (Input.GetKeyDown(KeyCode.I)) {
                ToggleInventory();
            }

            // Map toggle (M key)
            if (Input.GetKeyDown(KeyCode.M)) {
                ToggleMinimap();
            }

            // Pause game (Escape key)
            if (Input.GetKeyDown(KeyCode.Escape)) {
                PauseGame();
            }
        }

        /// <summary>
        /// Update all status displays
        /// </summary>
        private void UpdateAllDisplays() {
            // Update player stats if available
            if (playerStats != null) {
                UpdateHealth(playerStats.CurrentHealth, playerStats.MaxHealth);
                UpdateStamina(playerStats.CurrentStamina, playerStats.MaxStamina);
                UpdateLevel(playerStats.Level);
                UpdateGold(playerStats.Gold);
            } else {
                // Default values if no player stats
                UpdateHealth(100, 100);
                UpdateStamina(100, 100);
                UpdateLevel(1);
                UpdateGold(0);
            }

            // Update dungeon info
            UpdateDungeonInfo(1, 0, 0);

            // Update timer display
            UpdateTimerDisplay();
        }

        /// <summary>
        /// Update health display
        /// </summary>
        /// <param name="current">Current health</param>
        /// <param name="max">Maximum health</param>
        public void UpdateHealth(int current, int max) {
            if (healthSlider != null) {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthText != null) {
                healthText.text = $"{current}/{max}";
            }
        }

        /// <summary>
        /// Update stamina display
        /// </summary>
        /// <param name="current">Current stamina</param>
        /// <param name="max">Maximum stamina</param>
        public void UpdateStamina(int current, int max) {
            if (staminaSlider != null) {
                staminaSlider.maxValue = max;
                staminaSlider.value = current;
            }

            if (staminaText != null) {
                staminaText.text = $"{current}/{max}";
            }
        }

        /// <summary>
        /// Update level display
        /// </summary>
        /// <param name="level">Player level</param>
        public void UpdateLevel(int level) {
            if (levelText != null) {
                levelText.text = $"Level {level}";
            }
        }

        /// <summary>
        /// Update gold display
        /// </summary>
        /// <param name="gold">Player gold</param>
        public void UpdateGold(int gold) {
            if (goldText != null) {
                goldText.text = $"{gold} Gold";
            }
        }

        /// <summary>
        /// Update dungeon information
        /// </summary>
        /// <param name="floor">Current floor</param>
        /// <param name="roomsExplored">Rooms explored</param>
        /// <param name="totalRooms">Total rooms</param>
        public void UpdateDungeonInfo(int floor, int roomsExplored, int totalRooms) {
            if (floorText != null) {
                floorText.text = $"Floor {floor}";
            }

            if (roomCountText != null) {
                roomCountText.text = $"Rooms: {roomsExplored}/{totalRooms}";
            }
        }

        /// <summary>
        /// Update timer display
        /// </summary>
        private void UpdateTimerDisplay() {
            if (timerText != null) {
                // Format time as HH:MM:SS
                int hours = Mathf.FloorToInt(gameTimer / 3600);
                int minutes = Mathf.FloorToInt((gameTimer % 3600) / 60);
                int seconds = Mathf.FloorToInt(gameTimer % 60);

                timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            }
        }

        /// <summary>
        /// Start the game timer
        /// </summary>
        public void StartTimer() {
            gameTimer = 0f;
            timerRunning = true;
            UpdateTimerDisplay();
        }

        /// <summary>
        /// Pause the game timer
        /// </summary>
        public void PauseTimer() {
            timerRunning = false;
        }

        /// <summary>
        /// Resume the game timer
        /// </summary>
        public void ResumeTimer() {
            timerRunning = true;
        }

        /// <summary>
        /// Toggle inventory panel
        /// </summary>
        public void ToggleInventory() {
            if (inventoryPanel != null) {
                bool newState = !inventoryPanel.activeSelf;
                SetActive(inventoryPanel, newState);

                // Play sound effect
                if (AudioManager.HasInstance) {
                    AudioManager.Instance.PlayUiSound(newState ? "Open" : "Close");
                }
            }
        }

        /// <summary>
        /// Toggle minimap
        /// </summary>
        public void ToggleMinimap() {
            if (minimapPanel != null) {
                bool newState = !minimapPanel.activeSelf;
                SetActive(minimapPanel, newState);

                // Play sound effect
                if (AudioManager.HasInstance) {
                    AudioManager.Instance.PlayUiSound(newState ? "Open" : "Close");
                }
            }
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame() {
            if (GameManager.HasInstance) {
                GameManager.Instance.PauseGame();
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

            // Play sound effect
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Notification");
            }
        }

        /// <summary>
        /// Hide message after delay
        /// </summary>
        /// <param name="delay">Delay in seconds</param>
        /// <returns>IEnumerator</returns>
        private IEnumerator HideMessageAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            SetActive(messagePanel, false);
            messageCoroutine = null;
        }

        /// <summary>
        /// Set minimap texture
        /// </summary>
        /// <param name="texture">Minimap texture</param>
        public void SetMinimapTexture(Texture texture) {
            if (minimapImage != null && texture != null) {
                minimapImage.texture = texture;
            }
        }
    }
}