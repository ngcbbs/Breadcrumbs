using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GamePortfolio.Network.GameHub;
using MagicOnion.Client;
using MessagePack;

namespace GamePortfolio.Network.Synchronization
{
    /// <summary>
    /// Manages network synchronization of game objects
    /// </summary>
    public class NetworkSynchronizer : MonoBehaviour
    {
        // Singleton instance
        public static NetworkSynchronizer Instance { get; private set; }
        
        // Reference to network manager
        [SerializeField] private NetworkManager networkManager;
        
        // Tracking dictionaries
        private Dictionary<string, NetworkEntityController> networkEntities = new Dictionary<string, NetworkEntityController>();
        private Dictionary<string, Transform> transformsByNetId = new Dictionary<string, Transform>();
        
        // Sync settings
        [Header("Synchronization Settings")]
        [SerializeField] private float positionSyncInterval = 0.1f; // 10 times per second
        [SerializeField] private float rotationSyncThreshold = 1.0f; // Degrees
        [SerializeField] private float positionSyncThreshold = 0.05f; // Units
        [SerializeField] private float maxExtrapolationTime = 0.5f; // Maximum time to extrapolate
        
        // Last sync time
        private float lastSyncTime;
        
        // Latency tracking
        private float averageLatency = 0.05f; // Start with 50ms
        private float latencyUpdateRate = 0.2f; // Smooth latency updates
        
        // Network simulation (testing only)
        [Header("Network Simulation (Dev Only)")]
        [SerializeField] private bool simulateNetwork = false;
        [SerializeField] private float simulatedLatency = 0.05f;
        [SerializeField] private float packetLossChance = 0.01f;
        [SerializeField] private float latencyVariance = 0.02f;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Find network manager if not assigned
            if (networkManager == null)
            {
                networkManager = FindObjectOfType<NetworkManager>();
            }
        }
        
        private void Start()
        {
            if (networkManager != null)
            {
                // Register with hub events
                if (networkManager.HubReceiver != null)
                {
                    Debug.Log("<color=red>todo: Registering network events</color>");
                    /*
                    networkManager.HubReceiver.OnPlayerMoved += OnPlayerMoved;
                    networkManager.HubReceiver.OnActionPerformed += OnActionPerformed;
                    // */
                }
                
                // Setup ping for latency measurement
                StartPingMeasurement();
            }
        }
        
        private void Update()
        {
            // Only synchronize if connected
            if (!networkManager.IsConnected || networkManager.GameHubClient == null)
                return;
                
            // Send local player position updates
            if (Time.time - lastSyncTime >= positionSyncInterval)
            {
                lastSyncTime = Time.time;
                SynchronizeLocalPlayerPosition();
            }
            
            // Update interpolation/extrapolation for remote entities
            UpdateRemoteEntityPositions();
        }
        
        private void OnDestroy()
        {
            if (networkManager != null && networkManager.HubReceiver != null)
            {
                Debug.Log("<color=red>todo: Unregistering network events</color>");
                /*
                networkManager.HubReceiver.OnPlayerMoved -= OnPlayerMoved;
                networkManager.HubReceiver.OnActionPerformed -= OnActionPerformed;
                // */
            }
            
            // Clear collections
            networkEntities.Clear();
            transformsByNetId.Clear();
        }
        
        /// <summary>
        /// Register a network entity with the synchronizer
        /// </summary>
        /// <param name="netEntity">Network entity to register</param>
        public void RegisterEntity(NetworkEntityController netEntity)
        {
            if (netEntity == null || string.IsNullOrEmpty(netEntity.NetworkId))
                return;
                
            string netId = netEntity.NetworkId;
            
            // Register in dictionaries
            networkEntities[netId] = netEntity;
            transformsByNetId[netId] = netEntity.transform;
            
            Debug.Log($"Registered network entity: {netId} ({netEntity.gameObject.name})");
        }
        
        /// <summary>
        /// Unregister a network entity from the synchronizer
        /// </summary>
        /// <param name="netId">Network ID of entity to unregister</param>
        public void UnregisterEntity(string netId)
        {
            if (string.IsNullOrEmpty(netId))
                return;
                
            networkEntities.Remove(netId);
            transformsByNetId.Remove(netId);
            
            Debug.Log($"Unregistered network entity: {netId}");
        }
        
        /// <summary>
        /// Get a network entity by ID
        /// </summary>
        /// <param name="netId">Network ID to look up</param>
        /// <returns>Network entity controller or null if not found</returns>
        public NetworkEntityController GetEntity(string netId)
        {
            if (string.IsNullOrEmpty(netId))
                return null;
                
            networkEntities.TryGetValue(netId, out NetworkEntityController entity);
            return entity;
        }
        
        /// <summary>
        /// Synchronize the local player's position to the server
        /// </summary>
        private async void SynchronizeLocalPlayerPosition()
        {
            try
            {
                // Find local player
                NetworkEntityController localPlayer = GetLocalPlayerController();
                if (localPlayer == null)
                    return;
                    
                // Get current transform
                Vector3 position = localPlayer.transform.position;
                Quaternion rotation = localPlayer.transform.rotation;
                
                // Check if position has changed enough to sync
                if (Vector3.Distance(position, localPlayer.LastSyncedPosition) < positionSyncThreshold &&
                    Quaternion.Angle(rotation, localPlayer.LastSyncedRotation) < rotationSyncThreshold)
                {
                    return; // Not enough change to warrant a sync
                }
                
                // Update last synced values
                localPlayer.LastSyncedPosition = position;
                localPlayer.LastSyncedRotation = rotation;
                
                // Apply network simulation (for testing only)
                if (simulateNetwork)
                {
                    // Simulate packet loss
                    if (UnityEngine.Random.value < packetLossChance)
                        return;
                        
                    // Simulate latency
                    float delay = simulatedLatency + UnityEngine.Random.Range(-latencyVariance, latencyVariance);
                    await Task.Delay((int)(delay * 1000));
                }
                
                // Send update to server
                if (networkManager.GameHubClient != null)
                {
                    await networkManager.GameHubClient.UpdatePositionAsync(position, rotation);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error synchronizing position: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Update positions of remote entities with interpolation/extrapolation
        /// </summary>
        private void UpdateRemoteEntityPositions()
        {
            float deltaTime = Time.deltaTime;
            
            foreach (var entity in networkEntities.Values)
            {
                // Skip local player
                if (entity.IsLocalPlayer)
                    continue;
                    
                // Only process active entities with sync data
                if (!entity.gameObject.activeInHierarchy || !entity.HasSyncData)
                    continue;
                
                // Update interpolation time
                entity.InterpolationTime += deltaTime;
                
                // Calculate interpolation factor
                float t = entity.InterpolationTime / entity.InterpolationDuration;
                
                // Limit extrapolation
                if (t > 1.0f + (maxExtrapolationTime / entity.InterpolationDuration))
                {
                    // Cap at max extrapolation
                    t = 1.0f + (maxExtrapolationTime / entity.InterpolationDuration);
                    entity.InterpolationTime = t * entity.InterpolationDuration;
                }
                
                // Interpolate position
                if (t <= 1.0f)
                {
                    // Standard interpolation
                    entity.transform.position = Vector3.Lerp(entity.StartPosition, entity.TargetPosition, t);
                }
                else
                {
                    // Extrapolation (continue in same direction)
                    float extraT = t - 1.0f;
                    Vector3 extrapolatedPos = entity.TargetPosition + 
                                              (entity.TargetPosition - entity.StartPosition) * extraT;
                    entity.transform.position = extrapolatedPos;
                }
                
                // Interpolate rotation (always use shortest path)
                entity.transform.rotation = Quaternion.Slerp(entity.StartRotation, entity.TargetRotation, t);
            }
        }
        
        /// <summary>
        /// Handle player moved event from server
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="position">New position</param>
        /// <param name="rotation">New rotation</param>
        private void OnPlayerMoved(string playerId, Vector3 position, Vector3 rotation)
        {
            // Don't process our own position updates
            if (playerId == networkManager.PlayerId)
                return;
                
            // Find the entity for this player
            NetworkEntityController entity = GetEntity(playerId);
            if (entity == null)
            {
                Debug.LogWarning($"Received position update for unknown player: {playerId}");
                return;
            }
            
            // Set new target position/rotation for interpolation
            entity.StartPosition = entity.transform.position;
            entity.StartRotation = entity.transform.rotation;
            entity.TargetPosition = position;
            entity.TargetRotation = Quaternion.Euler(rotation);
            
            // Reset interpolation timer based on network conditions
            entity.InterpolationTime = 0f;
            entity.InterpolationDuration = Mathf.Max(positionSyncInterval, averageLatency);
            entity.HasSyncData = true;
        }
        
        /// <summary>
        /// Handle action performed event from server
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="action">Action performed</param>
        /// <param name="result">Action result</param>
        private void OnActionPerformed(string playerId, PlayerAction action, ActionResult result)
        {
            // Find the entity for this player
            NetworkEntityController entity = GetEntity(playerId);
            if (entity == null)
            {
                Debug.LogWarning($"Received action for unknown player: {playerId}");
                return;
            }
            
            // Notify the entity of the action
            entity.OnNetworkActionReceived(action, result);
        }
        
        /// <summary>
        /// Get local player's network controller
        /// </summary>
        /// <returns>Local player network controller or null</returns>
        private NetworkEntityController GetLocalPlayerController()
        {
            foreach (var entity in networkEntities.Values)
            {
                if (entity.IsLocalPlayer)
                    return entity;
            }
            
            return null;
        }
        
        /// <summary>
        /// Start periodic ping measurements to track latency
        /// </summary>
        private async void StartPingMeasurement()
        {
            while (this != null && gameObject != null && gameObject.activeInHierarchy)
            {
                if (networkManager.IsConnected && networkManager.GameHubClient != null)
                {
                    try
                    {
                        // Measure ping time
                        DateTime start = DateTime.UtcNow;
                        await networkManager.PingAsync();
                        TimeSpan pingTime = DateTime.UtcNow - start;
                        
                        // Update average latency with smoothing
                        float currentLatency = (float)pingTime.TotalSeconds;
                        averageLatency = Mathf.Lerp(averageLatency, currentLatency, latencyUpdateRate);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Error measuring ping: {ex.Message}");
                    }
                }
                
                // Wait before next measurement
                await Task.Delay(2000); // Ping every 2 seconds
            }
        }
        
        /// <summary>
        /// Get the current estimated network latency
        /// </summary>
        /// <returns>Latency in seconds</returns>
        public float GetCurrentLatency()
        {
            return averageLatency;
        }
        
        /// <summary>
        /// Send a network action to the server
        /// </summary>
        /// <param name="action">Action to send</param>
        /// <returns>Action result from server</returns>
        public async Task<ActionResult> SendActionAsync(PlayerAction action)
        {
            if (!networkManager.IsConnected || networkManager.GameHubClient == null)
            {
                Debug.LogWarning("Cannot send action: Not connected to server");
                return new ActionResult { Success = false, Message = "Not connected" };
            }
            
            try
            {
                // Apply network simulation (for testing only)
                if (simulateNetwork)
                {
                    // Simulate packet loss
                    if (UnityEngine.Random.value < packetLossChance)
                    {
                        throw new TimeoutException("Simulated packet loss");
                    }
                    
                    // Simulate latency
                    float delay = simulatedLatency + UnityEngine.Random.Range(-latencyVariance, latencyVariance);
                    await Task.Delay((int)(delay * 1000));
                }
                
                // Send action to server
                ActionResult result = await networkManager.GameHubClient.PerformActionAsync(action);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending action: {ex.Message}");
                return new ActionResult { Success = false, Message = ex.Message };
            }
        }
    }
}
