using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GamePortfolio.Core;
using GamePortfolio.Network;
using QualityLevel = GamePortfolio.Core.QualityLevel;

namespace GamePortfolio.UI.Menus
{
    /// <summary>
    /// Handles the main menu functionality
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject singlePlayerPanel;
        [SerializeField] private GameObject multiplayerPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        
        [Header("Main Panel Buttons")]
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button multiplayerButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Single Player Panel")]
        [SerializeField] private Dropdown characterClassDropdown;
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private Dropdown difficultyDropdown;
        [SerializeField] private Button startSinglePlayerButton;
        [SerializeField] private Button backFromSinglePlayerButton;
        
        [Header("Multiplayer Panel")]
        [SerializeField] private InputField serverAddressInput;
        [SerializeField] private InputField serverPortInput;
        [SerializeField] private InputField multiplayerNameInput;
        [SerializeField] private Dropdown multiplayerCharacterClassDropdown;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button backFromMultiplayerButton;
        
        [Header("Settings Panel")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Dropdown qualityDropdown;
        [SerializeField] private Button applySettingsButton;
        [SerializeField] private Button backFromSettingsButton;
        
        [Header("Credits Panel")]
        [SerializeField] private Button backFromCreditsButton;
        
        private GameSettings gameSettings;
        
        private void Awake()
        {
            // Initialize panels
            HideAllPanels();
            SetActive(mainPanel, true);
            
            // Set up button listeners
            SetupButtonListeners();
        }
        
        private void Start()
        {
            // Load settings
            if (GameManager.HasInstance)
            {
                gameSettings = GameManager.Instance.Settings;
                LoadSettingsToUI();
            }
            
            // Play menu music
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayMusic(0); // Assuming 0 is menu music
            }
        }
        
        /// <summary>
        /// Hide all menu panels
        /// </summary>
        private void HideAllPanels()
        {
            SetActive(mainPanel, false);
            SetActive(singlePlayerPanel, false);
            SetActive(multiplayerPanel, false);
            SetActive(settingsPanel, false);
            SetActive(creditsPanel, false);
        }
        
        /// <summary>
        /// Set GameObject active state with null check
        /// </summary>
        /// <param name="obj">GameObject</param>
        /// <param name="active">Active state</param>
        private void SetActive(GameObject obj, bool active)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
        
        /// <summary>
        /// Set up button click listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            // Main panel
            AddButtonListener(singlePlayerButton, OnSinglePlayerButtonClicked);
            AddButtonListener(multiplayerButton, OnMultiplayerButtonClicked);
            AddButtonListener(settingsButton, OnSettingsButtonClicked);
            AddButtonListener(creditsButton, OnCreditsButtonClicked);
            AddButtonListener(quitButton, OnQuitButtonClicked);
            
            // Single player panel
            AddButtonListener(startSinglePlayerButton, OnStartSinglePlayerButtonClicked);
            AddButtonListener(backFromSinglePlayerButton, OnBackToMainMenuButtonClicked);
            
            // Multiplayer panel
            AddButtonListener(connectButton, OnConnectButtonClicked);
            AddButtonListener(backFromMultiplayerButton, OnBackToMainMenuButtonClicked);
            
            // Settings panel
            AddButtonListener(applySettingsButton, OnApplySettingsButtonClicked);
            AddButtonListener(backFromSettingsButton, OnBackToMainMenuButtonClicked);
            
            // Credits panel
            AddButtonListener(backFromCreditsButton, OnBackToMainMenuButtonClicked);
        }
        
        /// <summary>
        /// Helper to add button listener with null check
        /// </summary>
        /// <param name="button">Button</param>
        /// <param name="action">Click action</param>
        private void AddButtonListener(Button button, System.Action action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    action();
                    PlayButtonSound();
                });
            }
        }
        
        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayButtonSound()
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }
        
        /// <summary>
        /// Load settings from game settings to UI
        /// </summary>
        private void LoadSettingsToUI()
        {
            if (gameSettings == null) return;
            
            // Volume sliders
            if (masterVolumeSlider != null) masterVolumeSlider.value = gameSettings.MasterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = gameSettings.MusicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = gameSettings.SfxVolume;
            
            // Fullscreen toggle
            if (fullscreenToggle != null) fullscreenToggle.isOn = gameSettings.FullScreen;
            
            // Quality dropdown
            if (qualityDropdown != null) qualityDropdown.value = (int)gameSettings.QualityLevel;
            
            // Player name inputs
            if (playerNameInput != null) playerNameInput.text = gameSettings.PlayerName;
            if (multiplayerNameInput != null) multiplayerNameInput.text = gameSettings.PlayerName;
            
            // Character class dropdowns
            if (characterClassDropdown != null)
            {
                int index = characterClassDropdown.options.FindIndex(option => 
                    option.text == gameSettings.SelectedCharacterClass);
                if (index >= 0) characterClassDropdown.value = index;
            }
            
            if (multiplayerCharacterClassDropdown != null)
            {
                int index = multiplayerCharacterClassDropdown.options.FindIndex(option => 
                    option.text == gameSettings.SelectedCharacterClass);
                if (index >= 0) multiplayerCharacterClassDropdown.value = index;
            }
            
            // Server settings
            if (serverAddressInput != null) serverAddressInput.text = gameSettings.ServerAddress;
            if (serverPortInput != null) serverPortInput.text = gameSettings.ServerPort.ToString();
            
            // Difficulty dropdown
            if (difficultyDropdown != null) difficultyDropdown.value = (int)gameSettings.Difficulty;
        }
        
        /// <summary>
        /// Save settings from UI to game settings
        /// </summary>
        private void SaveSettingsFromUI()
        {
            if (gameSettings == null) return;
            
            // Volume settings
            if (masterVolumeSlider != null) gameSettings.MasterVolume = masterVolumeSlider.value;
            if (musicVolumeSlider != null) gameSettings.MusicVolume = musicVolumeSlider.value;
            if (sfxVolumeSlider != null) gameSettings.SfxVolume = sfxVolumeSlider.value;
            
            // Graphics settings
            if (fullscreenToggle != null) gameSettings.FullScreen = fullscreenToggle.isOn;
            if (qualityDropdown != null) gameSettings.QualityLevel = (QualityLevel)qualityDropdown.value;
            
            // Player settings
            if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
            {
                gameSettings.PlayerName = playerNameInput.text;
            }
            
            if (characterClassDropdown != null && characterClassDropdown.options.Count > 0)
            {
                gameSettings.SelectedCharacterClass = characterClassDropdown.options[characterClassDropdown.value].text;
            }
            
            // Difficulty
            if (difficultyDropdown != null)
            {
                gameSettings.Difficulty = (DifficultyLevel)difficultyDropdown.value;
            }
            
            // Apply settings
            gameSettings.ApplySettings();
        }
        
        /// <summary>
        /// Save multiplayer settings
        /// </summary>
        private void SaveMultiplayerSettings()
        {
            if (gameSettings == null) return;
            
            // Player name
            if (multiplayerNameInput != null && !string.IsNullOrEmpty(multiplayerNameInput.text))
            {
                gameSettings.PlayerName = multiplayerNameInput.text;
            }
            
            // Character class
            if (multiplayerCharacterClassDropdown != null && multiplayerCharacterClassDropdown.options.Count > 0)
            {
                gameSettings.SelectedCharacterClass = 
                    multiplayerCharacterClassDropdown.options[multiplayerCharacterClassDropdown.value].text;
            }
            
            // Server settings
            if (serverAddressInput != null && !string.IsNullOrEmpty(serverAddressInput.text))
            {
                gameSettings.ServerAddress = serverAddressInput.text;
            }
            
            if (serverPortInput != null && !string.IsNullOrEmpty(serverPortInput.text))
            {
                int port;
                if (int.TryParse(serverPortInput.text, out port))
                {
                    gameSettings.ServerPort = port;
                }
            }
        }
        
        #region Button Handlers
        
        /// <summary>
        /// Single player button click handler
        /// </summary>
        private void OnSinglePlayerButtonClicked()
        {
            HideAllPanels();
            SetActive(singlePlayerPanel, true);
        }
        
        /// <summary>
        /// Multiplayer button click handler
        /// </summary>
        private void OnMultiplayerButtonClicked()
        {
            HideAllPanels();
            SetActive(multiplayerPanel, true);
        }
        
        /// <summary>
        /// Settings button click handler
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            HideAllPanels();
            SetActive(settingsPanel, true);
            LoadSettingsToUI();
        }
        
        /// <summary>
        /// Credits button click handler
        /// </summary>
        private void OnCreditsButtonClicked()
        {
            HideAllPanels();
            SetActive(creditsPanel, true);
        }
        
        /// <summary>
        /// Quit button click handler
        /// </summary>
        private void OnQuitButtonClicked()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }
        
        /// <summary>
        /// Start single player button click handler
        /// </summary>
        private void OnStartSinglePlayerButtonClicked()
        {
            // Save settings
            SaveSettingsFromUI();
            
            // Start game
            if (GameManager.HasInstance)
            {
                GameManager.Instance.StartSinglePlayerGame();
            }
        }
        
        /// <summary>
        /// Connect button click handler
        /// </summary>
        private void OnConnectButtonClicked()
        {
            // Save multiplayer settings
            SaveMultiplayerSettings();
            
            // Start multiplayer game
            if (GameManager.HasInstance)
            {
                GameManager.Instance.StartMultiplayerGame(
                    gameSettings.ServerAddress,
                    gameSettings.ServerPort
                );
            }
        }
        
        /// <summary>
        /// Apply settings button click handler
        /// </summary>
        private void OnApplySettingsButtonClicked()
        {
            SaveSettingsFromUI();
        }
        
        /// <summary>
        /// Back to main menu button click handler
        /// </summary>
        private void OnBackToMainMenuButtonClicked()
        {
            HideAllPanels();
            SetActive(mainPanel, true);
        }
        
        #endregion
    }
}
