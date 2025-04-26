using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GamePortfolio.Network.GameHub;
using Cysharp.Threading.Tasks;

namespace GamePortfolio.Network.Matchmaking {
    /// <summary>
    /// Client-side matchmaking manager to interact with the matchmaking service
    /// </summary>
    public class MatchmakingManager : MonoBehaviour {
        // Singleton instance
        public static MatchmakingManager Instance { get; private set; }

        // Events
        public event Action<List<GameSessionInfo>> OnGameListUpdated;
        public event Action<bool, string> OnGameCreated;
        public event Action<bool, string, string> OnGameJoined;
        public event Action<GameSessionInfo> OnGameDetailsReceived;
        public event Action OnGameLeft;

        // Reference to the network manager
        [SerializeField]
        private NetworkManager networkManager;

        // Properties
        public string CurrentGameId { get; private set; }
        public bool IsInGame { get; private set; }
        public GameSessionInfo CurrentGameInfo { get; private set; }

        // Cached game list
        private List<GameSessionInfo> availableGames = new List<GameSessionInfo>();

        // Matchmaking service client
        private IMatchmakingService matchmakingService;

        // Auto-refresh timer
        [SerializeField]
        private float refreshInterval = 10f;
        private float lastRefreshTime;

        private void Awake() {
            // Singleton setup
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
                return;
            }

            // Find network manager if not assigned
            if (networkManager == null) {
                networkManager = FindObjectOfType<NetworkManager>();
            }
        }

        private void Start() {
            // Initialize the matchmaking service when NetworkManager is connected
            if (networkManager != null) {
                networkManager.OnConnected += InitializeMatchmakingService;
                networkManager.OnDisconnected += CleanupMatchmakingService;
            }
        }

        private void Update() {
            // Auto-refresh game list when in lobby
            if (!IsInGame && matchmakingService != null && networkManager.IsConnected) {
                if (Time.time - lastRefreshTime > refreshInterval) {
                    lastRefreshTime = Time.time;
                    RefreshGameList().Forget();
                }
            }
        }

        private void OnDestroy() {
            if (networkManager != null) {
                networkManager.OnConnected -= InitializeMatchmakingService;
                networkManager.OnDisconnected -= CleanupMatchmakingService;
            }
        }

        /// <summary>
        /// Initialize the matchmaking service client
        /// </summary>
        private void InitializeMatchmakingService() {
            try {
                // Create the matchmaking service client
                matchmakingService = networkManager.CreateService<IMatchmakingService>();
                Debug.Log("Matchmaking service initialized");

                // Auto-refresh available games
                RefreshGameList().Forget();
            } catch (Exception ex) {
                Debug.LogError($"Failed to initialize matchmaking service: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup matchmaking service on disconnect
        /// </summary>
        private void CleanupMatchmakingService() {
            matchmakingService = null;
            availableGames.Clear();
            OnGameListUpdated?.Invoke(availableGames);
            CurrentGameId = null;
            IsInGame = false;
            CurrentGameInfo = null;
        }

        /// <summary>
        /// Refresh the list of available games
        /// </summary>
        public async UniTaskVoid RefreshGameList() {
            try {
                if (matchmakingService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot refresh game list: Not connected to server");
                    return;
                }

                // Get the list of available games
                GameListResult result = await matchmakingService.ListGamesAsync();

                // Update the cached list
                availableGames = result.Games ?? new List<GameSessionInfo>();

                // Notify listeners
                OnGameListUpdated?.Invoke(availableGames);

                Debug.Log($"Game list refreshed: {availableGames.Count} games available");
            } catch (Exception ex) {
                Debug.LogError($"Failed to refresh game list: {ex.Message}");

                // Reset the list on error
                availableGames.Clear();
                OnGameListUpdated?.Invoke(availableGames);
            }
        }

        /// <summary>
        /// Create a new game session
        /// </summary>
        /// <param name="gameName">Game name</param>
        /// <param name="maxPlayers">Maximum number of players</param>
        /// <param name="gameMode">Game mode</param>
        /// <param name="dungeonSettings">Dungeon settings</param>
        /// <param name="password">Optional password for private games</param>
        public async UniTaskVoid CreateGame(string gameName, int maxPlayers, GameMode gameMode,
            DungeonSettingsData dungeonSettings, string password = null) {
            try {
                if (matchmakingService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot create game: Not connected to server");
                    OnGameCreated?.Invoke(false, "Not connected to server");
                    return;
                }

                // Create game request
                GameCreationRequest request = new GameCreationRequest {
                    PlayerId = networkManager.PlayerId,
                    GameName = gameName,
                    MaxPlayers = maxPlayers,
                    GameMode = gameMode,
                    DungeonSettings = dungeonSettings,
                    Password = password
                };

                // Call the service
                GameCreationResult result = await matchmakingService.CreateGameAsync(request);

                if (result.Success) {
                    // Store the game ID
                    CurrentGameId = result.GameId;
                    IsInGame = true;

                    // Get game details
                    await GetGameDetails(result.GameId);

                    // Notify listeners
                    OnGameCreated?.Invoke(true, result.GameId);

                    Debug.Log($"Game created successfully: {result.GameId}");
                } else {
                    // Notify listeners of failure
                    OnGameCreated?.Invoke(false, result.Message);

                    Debug.LogWarning($"Failed to create game: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error creating game: {ex.Message}");
                OnGameCreated?.Invoke(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Find a game using matchmaking
        /// </summary>
        /// <param name="skillLevel">Player skill level</param>
        /// <param name="skillTolerance">Skill level tolerance</param>
        /// <param name="preferredGameMode">Preferred game mode</param>
        /// <param name="preferredMaxPlayers">Preferred max players</param>
        public async UniTaskVoid FindGame(float skillLevel, float skillTolerance,
            GameMode preferredGameMode, int preferredMaxPlayers) {
            try {
                if (matchmakingService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot find game: Not connected to server");
                    OnGameJoined?.Invoke(false, null, "Not connected to server");
                    return;
                }

                // Create matchmaking request
                MatchmakingRequest request = new MatchmakingRequest {
                    PlayerId = networkManager.PlayerId,
                    PlayerSkillLevel = skillLevel,
                    SkillLevelTolerance = skillTolerance,
                    PreferredGameMode = preferredGameMode,
                    PreferredMaxPlayers = preferredMaxPlayers
                };

                // Call the service
                MatchmakingResult result = await matchmakingService.FindGameAsync(request);

                if (result.Success) {
                    // Store the game ID
                    CurrentGameId = result.GameId;
                    IsInGame = true;

                    // Get game details
                    await GetGameDetails(result.GameId);

                    // Join the game via GameHub
                    await JoinGameViaHub(result.GameId);

                    // Notify listeners
                    OnGameJoined?.Invoke(true, result.GameId, result.GameName);

                    Debug.Log($"Joined game via matchmaking: {result.GameId} - {result.GameName}");
                } else {
                    // Notify listeners of failure
                    OnGameJoined?.Invoke(false, null, result.Message);

                    Debug.LogWarning($"Failed to find game: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error finding game: {ex.Message}");
                OnGameJoined?.Invoke(false, null, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Join a specific game
        /// </summary>
        /// <param name="gameId">Game ID to join</param>
        /// <param name="password">Password for private games</param>
        public async UniTaskVoid JoinGame(string gameId, string password = null) {
            try {
                if (matchmakingService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot join game: Not connected to server");
                    OnGameJoined?.Invoke(false, null, "Not connected to server");
                    return;
                }

                // Get game details first
                GameSessionDetails details = await matchmakingService.GetGameDetailsAsync(gameId);

                if (!details.Success) {
                    OnGameJoined?.Invoke(false, null, details.Message);
                    Debug.LogWarning($"Failed to join game: {details.Message}");
                    return;
                }

                // Check if game is private and password is required
                if (details.GameInfo.IsPrivate && password != details.GameInfo.Password) {
                    OnGameJoined?.Invoke(false, null, "Incorrect password");
                    Debug.LogWarning("Failed to join game: Incorrect password");
                    return;
                }

                // Check if game is full
                if (details.GameInfo.PlayerCount >= details.GameInfo.MaxPlayers) {
                    OnGameJoined?.Invoke(false, null, "Game is full");
                    Debug.LogWarning("Failed to join game: Game is full");
                    return;
                }

                // Store the game info
                CurrentGameId = gameId;
                CurrentGameInfo = details.GameInfo;
                IsInGame = true;

                // Join the game via GameHub
                await JoinGameViaHub(gameId);

                // Notify listeners
                OnGameJoined?.Invoke(true, gameId, details.GameInfo.GameName);

                Debug.Log($"Joined game: {gameId} - {details.GameInfo.GameName}");
            } catch (Exception ex) {
                Debug.LogError($"Error joining game: {ex.Message}");
                OnGameJoined?.Invoke(false, null, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get details for a specific game
        /// </summary>
        /// <param name="gameId">Game ID</param>
        public async Task GetGameDetails(string gameId) {
            try {
                if (matchmakingService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot get game details: Not connected to server");
                    return;
                }

                // Get game details
                GameSessionDetails details = await matchmakingService.GetGameDetailsAsync(gameId);

                if (details.Success) {
                    // Update current game info
                    CurrentGameInfo = details.GameInfo;

                    // Notify listeners
                    OnGameDetailsReceived?.Invoke(details.GameInfo);

                    Debug.Log($"Retrieved details for game: {gameId}");
                } else {
                    Debug.LogWarning($"Failed to get game details: {details.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error getting game details: {ex.Message}");
            }
        }

        /// <summary>
        /// Join a game via the GameHub
        /// </summary>
        /// <param name="gameId">Game ID to join</param>
        private async Task JoinGameViaHub(string gameId) {
            try {
                // Create join request
                JoinRequest joinRequest = new JoinRequest {
                    DungeonId = gameId,
                    CharacterClass = "TODO: Fixme", //PlayerManager.Instance.CurrentCharacterClass,
                    CustomProperties = new Dictionary<string, object>()
                };

                // Join the game via GameHub
                JoinResult joinResult = await networkManager.GameHubClient.JoinGameAsync(joinRequest);

                if (!joinResult.Success) {
                    Debug.LogError($"Failed to join game via hub: {joinResult.ErrorMessage}");
                    throw new Exception(joinResult.ErrorMessage);
                }
            } catch (Exception ex) {
                Debug.LogError($"Error joining game via hub: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Leave the current game
        /// </summary>
        public async UniTaskVoid LeaveGame() {
            try {
                if (networkManager == null || !networkManager.IsConnected || !IsInGame) {
                    Debug.LogWarning("Cannot leave game: Not in a game or not connected");
                    return;
                }

                // Leave the game via GameHub
                await networkManager.GameHubClient.LeaveGameAsync();

                // Reset game state
                CurrentGameId = null;
                CurrentGameInfo = null;
                IsInGame = false;

                // Notify listeners
                OnGameLeft?.Invoke();

                Debug.Log("Left game successfully");
            } catch (Exception ex) {
                Debug.LogError($"Error leaving game: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the list of available games
        /// </summary>
        public List<GameSessionInfo> GetAvailableGames() {
            return new List<GameSessionInfo>(availableGames);
        }
    }
}