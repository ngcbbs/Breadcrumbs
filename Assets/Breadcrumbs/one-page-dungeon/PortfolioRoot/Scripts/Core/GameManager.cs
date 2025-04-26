using System;
using System.Collections;
using GamePortfolio.Dungeon;
using GamePortfolio.Gameplay.Character;
using GamePortfolio.UI;
using GamePortfolio.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePortfolio.Core
{
    /// <summary>
    /// Central game manager that coordinates game systems and manages game state
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Core Systems")]
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private NetworkManager networkManager;
        
        /// <summary>
        /// Current game state
        /// </summary>
        public GameState CurrentState { get; private set; }
        
        /// <summary>
        /// Game settings
        /// </summary>
        public GameSettings Settings { get; private set; }
        
        /// <summary>
        /// Event triggered when game state changes
        /// </summary>
        public event Action<GameState> OnGameStateChanged;

        protected override void Awake()
        {
            base.Awake();
            
            // Initialize systems
            Initialize();
        }

        private void OnEnable()
        {
            // Subscribe to events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Handle scene loaded logic
            Debug.Log($"Scene loaded: {scene.name} with mode: {mode}");
            
            if (scene.name == "MainMenu")
            {
                ChangeState(GameState.MainMenu);
            }
            else if (scene.name == "Loading")
            {
                ChangeState(GameState.Loading);
            }
            else if (scene.name == "Dungeon")
            {
                StartCoroutine(LoadGameRoutine());
            }
        }

        /// <summary>
        /// Initialize the GameManager and all subsystems
        /// </summary>
        private void Initialize()
        {
            // Load game settings
            LoadGameSettings();
            
            // Initialize subsystems if references are set
            if (dungeonManager != null) dungeonManager.Initialize();
            if (playerManager != null) playerManager.Initialize();
            if (uiManager != null) uiManager.Initialize();
            if (networkManager != null) networkManager.Initialize();
            
            // Set initial state
            ChangeState(GameState.MainMenu);
        }

        /// <summary>
        /// Load game settings from ScriptableObject or PlayerPrefs
        /// </summary>
        private void LoadGameSettings()
        {
            // Load settings from Resources or create default
            GameSettings loadedSettings = Resources.Load<GameSettings>("ScriptableObjects/GameSettings");
            
            if (loadedSettings != null)
            {
                Settings = loadedSettings;
            }
            else
            {
                // Create default settings
                Settings = ScriptableObject.CreateInstance<GameSettings>();
                Settings.SetDefaults();
                
                Debug.LogWarning("GameSettings not found, using defaults");
            }
        }

        public void SaveSettings() {
            Debug.Log("todo: Save settings to PlayerPrefs or file");
        }

        /// <summary>
        /// Change the current game state
        /// </summary>
        /// <param name="newState">The new state to transition to</param>
        public void ChangeState(GameState newState)
        {
            // Skip if trying to change to the same state
            if (CurrentState == newState) return;
            
            // Handle exiting current state
            ExitState(CurrentState);
            
            // Update state
            GameState oldState = CurrentState;
            CurrentState = newState;
            
            // Handle entering new state
            EnterState(newState);
            
            // Notify listeners
            OnGameStateChanged?.Invoke(newState);
            
            Debug.Log($"Game state changed from {oldState} to {newState}");
        }

        /// <summary>
        /// Handle logic when exiting a state
        /// </summary>
        /// <param name="state">The state being exited</param>
        private void ExitState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    // Clean up main menu resources
                    break;
                case GameState.Playing:
                    // Pause gameplay systems
                    Time.timeScale = 0f;
                    break;
                case GameState.Paused:
                    // Resume time scale when leaving pause
                    Time.timeScale = 1f;
                    break;
                // Handle other states...
            }
        }

        /// <summary>
        /// Handle logic when entering a state
        /// </summary>
        /// <param name="state">The state being entered</param>
        private void EnterState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    // Show main menu UI
                    if (uiManager != null) uiManager.ShowMainMenu();
                    break;
                case GameState.Loading:
                    // Show loading screen
                    if (uiManager != null) uiManager.ShowLoadingScreen();
                    break;
                case GameState.Playing:
                    // Show game UI and ensure time scale is normal
                    if (uiManager != null) uiManager.ShowGameUI();
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    // Show pause menu and pause the game
                    if (uiManager != null) uiManager.ShowPauseMenu();
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    // Show game over screen
                    if (uiManager != null) uiManager.ShowGameOverScreen();
                    break;
            }
        }

        /// <summary>
        /// Routine to handle game loading sequence
        /// </summary>
        private IEnumerator LoadGameRoutine()
        {
            ChangeState(GameState.Loading);
            
            // Connect to server if in multiplayer mode
            if (Settings.GameMode == GameMode.Multiplayer)
            {
                yield return StartCoroutine(ConnectToServerRoutine());
            }
            
            // Generate dungeon
            if (dungeonManager != null)
            {
                yield return StartCoroutine(dungeonManager.GenerateDungeonRoutine());
            }
            
            // Spawn player(s)
            if (playerManager != null)
            {
                yield return StartCoroutine(playerManager.SpawnPlayersRoutine());
            }
            
            // Loading complete, start playing
            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Routine to handle server connection
        /// </summary>
        private IEnumerator ConnectToServerRoutine()
        {
            if (networkManager == null) yield break;
            
            // Show connecting message
            if (uiManager != null) uiManager.ShowConnectingMessage();
            
            // Connect to server
            bool connected = false;
            
            yield return StartCoroutine(networkManager.ConnectToServerRoutine((success) => {
                connected = success;
            }));
            
            if (!connected)
            {
                // Connection failed
                if (uiManager != null) uiManager.ShowConnectionError();
                
                // Wait for acknowledgment
                yield return new WaitForSeconds(2f);
                
                // Return to main menu
                SceneManager.LoadScene("MainMenu");
            }
        }

        /// <summary>
        /// Start a new single player game
        /// </summary>
        public void StartSinglePlayerGame()
        {
            Settings.GameMode = GameMode.SinglePlayer;
            SceneManager.LoadScene("Loading");
        }

        /// <summary>
        /// Start a new multiplayer game
        /// </summary>
        /// <param name="serverAddress">Server address to connect to</param>
        /// <param name="port">Server port</param>
        public void StartMultiplayerGame(string serverAddress, int port)
        {
            Settings.GameMode = GameMode.Multiplayer;
            Settings.ServerAddress = serverAddress;
            Settings.ServerPort = port;
            SceneManager.LoadScene("Loading");
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resume the game from paused state
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        /// <summary>
        /// Quit to main menu
        /// </summary>
        public void QuitToMainMenu()
        {
            // Disconnect from server if connected
            if (networkManager != null && Settings.GameMode == GameMode.Multiplayer)
            {
                networkManager.Disconnect();
            }
            
            // Load main menu scene
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Quit the application
        /// </summary>
        public void QuitGame()
        {
            // Disconnect from server if connected
            if (networkManager != null && Settings.GameMode == GameMode.Multiplayer)
            {
                networkManager.Disconnect();
            }
            
            // Quit application
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }

    /// <summary>
    /// Game state enumeration
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Game mode enumeration
    /// </summary>
    public enum GameMode
    {
        SinglePlayer,
        Multiplayer
    }
}
