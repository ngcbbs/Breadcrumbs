using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GamePortfolio.Core;
using Grpc.Core;
using MagicOnion.Client;
using GamePortfolio.Network.Authority;
using GamePortfolio.Network.GameHub;
using GamePortfolio.Network.GameService;
//using GamePortfolio.Network.Party;
using MagicOnion;
using GameState = GamePortfolio.Network.GameHub.GameState;

namespace GamePortfolio.Network
{
    /// <summary>
    /// Manages network connections and communication using MagicOnion
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    {
        // Service and hub clients
        private GameService.IGameService gameService;
        private GameHub.IGameHub gameHub;
        
        // Channel for gRPC communication
        private GrpcChannelx channel;
        
        // Connection status
        private bool isConnected = false;
        
        // Player information
        [SerializeField] private string playerName = "Player";
        private string playerId = "";
        private Dictionary<string, PlayerInfo> connectedPlayers = new Dictionary<string, PlayerInfo>();
        
        // Server authority validation
        private ServerAuthorityValidator authorityValidator;
        
        // Network settings
        [Header("Network Settings")]
        [SerializeField] private float reconnectDelay = 5f;
        [SerializeField] private int maxReconnectAttempts = 5;
        [SerializeField] private bool autoReconnect = true;
        
        // Reconnection state
        private int reconnectAttempts = 0;
        private bool isReconnecting = false;
        
        // Hub receiver instance
        private GameHubReceiver hubReceiver;
        
        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<bool> OnConnectionStatusChanged;
        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;
        public event Action<string, Vector3, Quaternion> OnPlayerMoved;
        public event Action<string, PlayerAction, ActionResult> OnActionPerformed;
        public event Action<string, ValidationResult, Vector3?, Vector3?> OnValidationResultReceived;
        public event Action<GameState> OnGameStateChanged;
        public event Action<ChatMessage> OnMessageReceived;
        public event Action<string, PlayerStatus> OnPlayerStatusChanged;

        /// <summary>
        /// Get current connection status
        /// </summary>
        //public bool IsConnected => isConnected && gameHub != null && (gameHub.ConnectionState == ConnectionState.Connected);
        public bool IsConnected {
            get {
                Debug.Log("todo: fixme!!!1 IsConnected");
                return false;
            }
        }
        
        /// <summary>
        /// Get current player ID
        /// </summary>
        public string PlayerId => playerId;
        
        /// <summary>
        /// Get/set current player name
        /// </summary>
        public string PlayerName 
        { 
            get => playerName;
            set => playerName = value;
        }
        
        /// <summary>
        /// Get dictionary of connected players
        /// </summary>
        public Dictionary<string, PlayerInfo> ConnectedPlayers => connectedPlayers;
        
        /// <summary>
        /// Get game hub client
        /// </summary>
        public GameHub.IGameHub GameHubClient => gameHub;
        
        /// <summary>
        /// Get hub receiver
        /// </summary>
        public GameHubReceiver HubReceiver => hubReceiver;
        
        /// <summary>
        /// Get authority validator
        /// </summary>
        public ServerAuthorityValidator AuthorityValidator => authorityValidator;
        
        /// <summary>
        /// Initialize the NetworkManager
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            /*
            // Create authority validator
            authorityValidator = new ServerAuthorityValidator();
            
            // Create hub receiver
            hubReceiver = new GameHubReceiver(this);
            // */
        }
        
        /// <summary>
        /// Clean up on destroy
        /// </summary>
        private void OnDestroy()
        {
            // Ensure disconnection
            Disconnect();
        }

        public void Initialize() {
            // Create authority validator
            authorityValidator = new ServerAuthorityValidator();
            
            // Create hub receiver
            hubReceiver = new GameHubReceiver(this);
        }
        
        /// <summary>
        /// Connect to the server as a coroutine
        /// </summary>
        /// <param name="callback">Callback to invoke with connection result</param>
        /// <returns>IEnumerator for coroutine</returns>
        public IEnumerator ConnectToServerRoutine(Action<bool> callback = null)
        {
            // Get server address and port from game settings
            string serverAddress = "localhost";
            int serverPort = 12345;
            
            if (GameManager.HasInstance && GameManager.Instance.Settings != null)
            {
                serverAddress = GameManager.Instance.Settings.ServerAddress;
                serverPort = GameManager.Instance.Settings.ServerPort;
            }
            
            Debug.Log("<color=red>todo: fix connection settings</color>");
            /*
            // Create a task for async connection
            var connectionTask = await ConnectToServerAsync(serverAddress, serverPort);
            
            // Wait for connection to complete
            while (!connectionTask.IsCompleted)
            {
                yield return null;
            }
            
            // Handle result
            bool result = false;
            try
            {
                result = connectionTask.Result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection error: {ex.Message}");
            }
            
            callback?.Invoke(result);
            // */
            yield return null;
        }
        
        /// <summary>
        /// Connect to the server asynchronously
        /// </summary>
        /// <param name="serverAddress">Server address</param>
        /// <param name="serverPort">Server port</param>
        /// <returns>Task with connection result</returns>
        public async UniTask<bool> ConnectToServerAsync(string serverAddress, int serverPort)
        {
            try
            {
                // If already connected, disconnect first
                if (isConnected)
                {
                    await DisconnectAsync();
                }
                
                Debug.Log($"Connecting to server at {serverAddress}:{serverPort}");
                
                // Create channel
                channel = GrpcChannelx.ForAddress($"http://{serverAddress}:{serverPort}");
                
                // Create service client
                gameService = MagicOnionClient.Create<GameService.IGameService>(channel);
                
                // Create hub client and connect
                var hubClient = StreamingHubClient.ConnectAsync<GameHub.IGameHub, GameHub.IGameHubReceiver>(
                    channel, hubReceiver);
                    
                gameHub = await hubClient;
                
                // Update connection status
                isConnected = true;
                reconnectAttempts = 0;
                
                // Notify listeners
                OnConnectionStatusChanged?.Invoke(true);
                OnConnected?.Invoke();
                
                Debug.Log("Connected to server successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to connect to server: {ex.Message}");
                
                // Ensure clean up on failure
                await DisconnectAsync();
                
                return false;
            }
        }
        
        /// <summary>
        /// Create a service client of the specified type
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <returns>Service client</returns>
        public T CreateService<T>() where T : IService<T>
        {
            if (!isConnected || channel == null)
            {
                throw new InvalidOperationException("Cannot create service: Not connected to server");
            }
            
            return MagicOnionClient.Create<T>(channel);
        }
        
        /// <summary>
        /// Authenticate with the server
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="password">Password</param>
        /// <returns>Authentication response</returns>
        public async Task<AuthResponse> AuthenticateAsync(string userId, string password)
        {
            if (!isConnected || gameService == null)
            {
                Debug.LogError("Cannot authenticate: Not connected to server");
                return new AuthResponse { Success = false, ErrorMessage = "Not connected to server" };
            }
            
            try
            {
                AuthResponse response = await gameService.AuthenticateAsync(userId, password);
                
                if (response.Success)
                {
                    playerId = response.PlayerId;
                    Debug.Log($"Authenticated as {playerId}");
                }
                else
                {
                    Debug.LogWarning($"Authentication failed: {response.ErrorMessage}");
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Authentication error: {ex.Message}");
                return new AuthResponse { Success = false, ErrorMessage = ex.Message };
            }
        }
        
        /// <summary>
        /// Join a game session
        /// </summary>
        /// <param name="request">Join request</param>
        /// <returns>Join result</returns>
        public async Task<JoinResult> JoinGameAsync(JoinRequest request)
        {
            if (!isConnected || gameHub == null)
            {
                Debug.LogError("Cannot join game: Not connected to server");
                return new JoinResult { Success = false, ErrorMessage = "Not connected to server" };
            }
            
            try
            {
                JoinResult result = await gameHub.JoinGameAsync(request);
                
                if (result.Success)
                {
                    // Store connected players
                    connectedPlayers.Clear();
                    if (result.Players != null)
                    {
                        foreach (PlayerInfo player in result.Players)
                        {
                            connectedPlayers[player.PlayerId] = player;
                        }
                    }
                    
                    // Notify game state
                    OnGameStateChanged?.Invoke(result.CurrentState);
                    
                    Debug.Log($"Joined game successfully. {connectedPlayers.Count} players connected.");
                }
                else
                {
                    Debug.LogWarning($"Failed to join game: {result.ErrorMessage}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error joining game: {ex.Message}");
                
                // Check if it's a connection-related error
                if (IsConnectionError(ex))
                {
                    await HandleConnectionError();
                }
                
                return new JoinResult { Success = false, ErrorMessage = ex.Message };
            }
        }
        
        /// <summary>
        /// Update player position on the server
        /// </summary>
        /// <param name="position">Player position</param>
        /// <param name="rotation">Player rotation</param>
        /// <returns>Task</returns>
        public async Task UpdatePositionAsync(Vector3 position, Quaternion rotation)
        {
            if (!isConnected || gameHub == null)
            {
                Debug.LogWarning("Cannot update position: Not connected to server");
                return;
            }
            
            try
            {
                await gameHub.UpdatePositionAsync(position, rotation);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating position: {ex.Message}");
                
                // Check if it's a connection-related error
                if (IsConnectionError(ex))
                {
                    await HandleConnectionError();
                }
            }
        }
        
        /// <summary>
        /// Perform a game action on the server
        /// </summary>
        /// <param name="action">Action to perform</param>
        /// <returns>Action result</returns>
        public async Task<ActionResult> PerformActionAsync(PlayerAction action)
        {
            if (!isConnected || gameHub == null)
            {
                Debug.LogWarning("Cannot perform action: Not connected to server");
                return new ActionResult { Success = false, Message = "Not connected to server" };
            }
            
            try
            {
                return await gameHub.PerformActionAsync(action);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error performing action: {ex.Message}");
                
                // Check if it's a connection-related error
                if (IsConnectionError(ex))
                {
                    await HandleConnectionError();
                }
                
                return new ActionResult { Success = false, Message = ex.Message };
            }
        }
        
        /// <summary>
        /// Send a chat message
        /// </summary>
        /// <param name="message">Chat message</param>
        /// <returns>Task</returns>
        public async Task SendMessageAsync(string message, ChatMessageType type = ChatMessageType.Global, string targetId = null)
        {
            if (!isConnected || gameHub == null)
            {
                Debug.LogWarning("Cannot send message: Not connected to server");
                return;
            }
            
            try
            {
                ChatMessage chatMessage = new ChatMessage
                {
                    SenderId = playerId,
                    SenderName = playerName,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.UtcNow
                };
                
                await gameHub.SendMessageAsync(chatMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending message: {ex.Message}");
                
                // Check if it's a connection-related error
                if (IsConnectionError(ex))
                {
                    await HandleConnectionError();
                }
            }
        }
        
        /// <summary>
        /// Update player status
        /// </summary>
        /// <param name="status">New status</param>
        /// <returns>Task</returns>
        public async Task UpdateStatusAsync(PlayerStatus status)
        {
            if (!isConnected || gameHub == null)
            {
                Debug.LogWarning("Cannot update status: Not connected to server");
                return;
            }
            
            try
            {
                await gameHub.UpdateStatusAsync(status);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating status: {ex.Message}");
                
                // Check if it's a connection-related error
                if (IsConnectionError(ex))
                {
                    await HandleConnectionError();
                }
            }
        }
        
        /// <summary>
        /// Send a ping to the server (for latency measurement)
        /// </summary>
        /// <returns>Task</returns>
        public async Task PingAsync()
        {
            if (!isConnected || gameService == null)
            {
                return;
            }
            
            try
            {
                Debug.Log("todo: fixme ping");
                //await gameService.PingAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Ping error: {ex.Message}");
                
                // Check if it's a connection-related error
                if (IsConnectionError(ex))
                {
                    await HandleConnectionError();
                }
            }
        }
        
        /// <summary>
        /// Leave the current game session
        /// </summary>
        /// <returns>Task</returns>
        public async Task LeaveGameAsync()
        {
            if (!isConnected || gameHub == null)
            {
                return;
            }
            
            try
            {
                await gameHub.LeaveGameAsync();
                connectedPlayers.Clear();
                Debug.Log("Left game session");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error leaving game: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Disconnect from the server
        /// </summary>
        /// <returns>Task</returns>
        public async Task DisconnectAsync()
        {
            try
            {
                // Leave game if connected
                if (isConnected && gameHub != null)
                {
                    await LeaveGameAsync();
                }
                
                // Dispose hub
                if (gameHub != null)
                {
                    await gameHub.DisposeAsync();
                    gameHub = null;
                }
                
                // Dispose channel
                if (channel != null)
                {
                    await channel.ShutdownAsync();
                    channel = null;
                }
                
                // Reset state
                gameService = null;
                isConnected = false;
                playerId = "";
                connectedPlayers.Clear();
                
                // Notify listeners
                OnConnectionStatusChanged?.Invoke(false);
                OnDisconnected?.Invoke();
                
                Debug.Log("Disconnected from server");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during disconnect: {ex.Message}");
                
                // Force reset on error
                gameHub = null;
                channel = null;
                gameService = null;
                isConnected = false;
                
                // Notify listeners
                OnConnectionStatusChanged?.Invoke(false);
                OnDisconnected?.Invoke();
            }
        }
        
        /// <summary>
        /// Disconnect from the server without async
        /// </summary>
        public void Disconnect()
        {
            _ = DisconnectAsync();
        }
        
        /// <summary>
        /// Handle connection errors
        /// </summary>
        /// <returns>Task</returns>
        private async Task HandleConnectionError()
        {
            // Set connection state to disconnected
            isConnected = false;
            
            // Notify listeners
            OnConnectionStatusChanged?.Invoke(false);
            OnDisconnected?.Invoke();
            
            // Clean up resources
            await DisconnectAsync();
            
            Debug.Log("Connection lost. Cleaned up resources.");
            
            // Auto-reconnect if enabled
            if (autoReconnect && !isReconnecting && reconnectAttempts < maxReconnectAttempts)
            {
                isReconnecting = true;
                ReconnectRoutine().Forget();
                //StartCoroutine(ReconnectRoutine());
            }
        }
        
        /// <summary>
        /// Reconnect routine
        /// </summary>
        /// <returns>IEnumerator for coroutine</returns>
        private async UniTaskVoid ReconnectRoutine()
        {
            while (reconnectAttempts < maxReconnectAttempts)
            {
                reconnectAttempts++;
                
                Debug.Log($"Attempting to reconnect ({reconnectAttempts}/{maxReconnectAttempts})...");
                
                // Wait before reconnecting
                await UniTask.WaitForSeconds(reconnectDelay);
                
                // Get server address and port from game settings
                string serverAddress = "localhost";
                int serverPort = 12345;
                
                if (GameManager.HasInstance && GameManager.Instance.Settings != null)
                {
                    serverAddress = GameManager.Instance.Settings.ServerAddress;
                    serverPort = GameManager.Instance.Settings.ServerPort;
                }
                
                // Try to reconnect
                bool success = await ConnectToServerAsync(serverAddress, serverPort);
                
                if (success)
                {
                    // Reconnect successful
                    isReconnecting = false;
                    return;
                }
            }
            
            // Reconnection failed after max attempts
            isReconnecting = false;
            Debug.LogWarning("Reconnection failed after maximum attempts.");
        }
        
        /// <summary>
        /// Check if an exception is related to connection issues
        /// </summary>
        /// <param name="ex">Exception to check</param>
        /// <returns>True if it's a connection error</returns>
        private bool IsConnectionError(Exception ex)
        {
            // Check for common gRPC connection errors
            return ex is RpcException ||
                   ex.Message.Contains("Connection") ||
                   ex.Message.Contains("Broken") ||
                   ex.Message.Contains("timed out");
        }
        
        /// <summary>
        /// Receiver implementation for game hub events
        /// </summary>
        public class GameHubReceiver : GameHub.IGameHubReceiver
        {
            private readonly NetworkManager manager;
            
            public GameHubReceiver(NetworkManager manager)
            {
                this.manager = manager;
            }
            
            public void OnPlayerJoined(PlayerInfo player)
            {
                // Add to connected players
                manager.connectedPlayers[player.PlayerId] = player;
                
                Debug.Log($"Player joined: {player.PlayerName} ({player.PlayerId})");
                
                // Notify listeners
                manager.OnPlayerJoined?.Invoke(player);
            }
            
            public void OnPlayerLeft(string playerId)
            {
                // Remove from connected players
                if (manager.connectedPlayers.TryGetValue(playerId, out PlayerInfo player))
                {
                    manager.connectedPlayers.Remove(playerId);
                    Debug.Log($"Player left: {player.PlayerName} ({playerId})");
                }
                else
                {
                    Debug.Log($"Unknown player left: {playerId}");
                }
                
                // Notify listeners
                manager.OnPlayerLeft?.Invoke(playerId);
            }

            public void OnPlayerMoved(string playerId, Vector3 position, Quaternion rotation)
            {
                // Update player position in connected players
                if (manager.connectedPlayers.TryGetValue(playerId, out PlayerInfo player))
                {
                    player.Position = position;
                    player.Rotation = rotation;
                }
                
                // Notify listeners
                manager.OnPlayerMoved?.Invoke(playerId, position, rotation);
            }
            
            public void OnActionPerformed(string playerId, PlayerAction action, ActionResult result)
            {
                Debug.Log($"Player {playerId} performed action: {action.Type}");
                
                // Notify listeners
                manager.OnActionPerformed?.Invoke(playerId, action, result);
            }
            
            public void OnMessageReceived(ChatMessage message)
            {
                // Notify listeners
                manager.OnMessageReceived?.Invoke(message);
            }
            
            public void OnPlayerStatusChanged(string playerId, PlayerStatus status)
            {
                // Update player status in connected players
                if (manager.connectedPlayers.TryGetValue(playerId, out PlayerInfo player))
                {
                    player.Status = status;
                }
                
                // Notify listeners
                manager.OnPlayerStatusChanged?.Invoke(playerId, status);
            }
            
            public void OnGameStateChanged(GameState state)
            {
                // Notify listeners
                manager.OnGameStateChanged?.Invoke(state);
            }
        }
    }
}
