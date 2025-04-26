using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Network;
using GamePortfolio.Network.GameHub;
using GameMode = GamePortfolio.Core.GameMode;

namespace GamePortfolio.Gameplay.Character {
    /// <summary>
    /// Manages player characters in the game world
    /// </summary>
    public class PlayerManager : Singleton<PlayerManager> {
        [Header("Player Prefabs")]
        [SerializeField]
        private GameObject[] characterPrefabs;

        [Header("Spawn Settings")]
        [SerializeField]
        private Transform defaultSpawnPoint;
        [SerializeField]
        private bool randomizeSpawnRotation = true;

        [Header("Remote Player Settings")]
        [SerializeField]
        private float positionSmoothTime = 0.1f;
        [SerializeField]
        private float rotationSmoothTime = 0.1f;

        // Dictionary of spawned players (player ID -> GameObject)
        private Dictionary<string, GameObject> spawnedPlayers = new Dictionary<string, GameObject>();

        // Reference to local player
        private GameObject localPlayer;
        private string localPlayerId;

        // Initialization status
        private bool isInitialized = false;

        /// <summary>
        /// Initialize the player manager
        /// </summary>
        public void Initialize() {
            if (isInitialized) return;

            // Subscribe to network events
            if (NetworkManager.HasInstance) {
                NetworkManager instance = NetworkManager.Instance;

                instance.OnPlayerJoined += OnPlayerJoined;
                instance.OnPlayerLeft += OnPlayerLeft;
                instance.OnPlayerMoved += OnPlayerMoved;
            }

            isInitialized = true;
            Debug.Log("PlayerManager initialized");
        }

        private void OnDestroy() {
            // Unsubscribe from network events
            if (NetworkManager.HasInstance) {
                NetworkManager instance = NetworkManager.Instance;

                instance.OnPlayerJoined -= OnPlayerJoined;
                instance.OnPlayerLeft -= OnPlayerLeft;
                instance.OnPlayerMoved -= OnPlayerMoved;
            }
        }

        /// <summary>
        /// Spawn the local player
        /// </summary>
        /// <param name="characterClass">Character class name</param>
        /// <param name="position">Spawn position (optional)</param>
        /// <param name="rotation">Spawn rotation (optional)</param>
        /// <returns>Spawned player GameObject</returns>
        public GameObject SpawnLocalPlayer(string characterClass, Vector3? position = null, Quaternion? rotation = null) {
            // Use default spawn point if position not specified
            Vector3 spawnPosition = position ?? (defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero);

            // Use default or random rotation if not specified
            Quaternion spawnRotation;
            if (rotation.HasValue) {
                spawnRotation = rotation.Value;
            } else if (randomizeSpawnRotation) {
                spawnRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            } else if (defaultSpawnPoint != null) {
                spawnRotation = defaultSpawnPoint.rotation;
            } else {
                spawnRotation = Quaternion.identity;
            }

            // Find appropriate prefab
            GameObject prefab = GetCharacterPrefab(characterClass);

            // Spawn player
            localPlayer = Instantiate(prefab, spawnPosition, spawnRotation);

            // Set up as local player
            PlayerController controller = localPlayer.GetComponent<PlayerController>();
            if (controller != null) {
                controller.SetIsLocalPlayer(true);

                // Subscribe to position changes for network sync
                controller.OnPositionChanged += OnLocalPlayerPositionChanged;
            }

            // Store local player ID if connected to network
            if (NetworkManager.HasInstance) {
                localPlayerId = NetworkManager.Instance.PlayerId;

                // Add to spawned players dictionary
                if (!string.IsNullOrEmpty(localPlayerId)) {
                    spawnedPlayers[localPlayerId] = localPlayer;
                }
            }

            Debug.Log($"Local player spawned as {characterClass}");
            return localPlayer;
        }

        /// <summary>
        /// Spawn players routine for coroutine usage
        /// </summary>
        /// <returns>IEnumerator for coroutine</returns>
        public IEnumerator SpawnPlayersRoutine() {
            // Get character class from settings
            string characterClass = "Warrior"; // Default

            if (GameManager.HasInstance && GameManager.Instance.Settings != null) {
                characterClass = GameManager.Instance.Settings.SelectedCharacterClass;
            }

            // Spawn local player
            SpawnLocalPlayer(characterClass);

            // In multiplayer mode, wait for remote players
            if (GameManager.HasInstance &&
                GameManager.Instance.Settings != null &&
                GameManager.Instance.Settings.GameMode == GameMode.Multiplayer) {
                // Wait a moment for network to connect
                yield return new WaitForSeconds(0.5f);

                // Spawn any existing remote players from network manager
                if (NetworkManager.HasInstance) {
                    foreach (var player in NetworkManager.Instance.ConnectedPlayers) {
                        // Skip local player
                        if (player.Key == localPlayerId) continue;

                        // Spawn remote player
                        SpawnRemotePlayer(player.Value);
                    }
                }
            }

            yield return null;
        }

        /// <summary>
        /// Spawn a remote player
        /// </summary>
        /// <param name="playerInfo">Remote player info</param>
        /// <returns>Spawned player GameObject</returns>
        private GameObject SpawnRemotePlayer(PlayerInfo playerInfo) {
            // Skip if already spawned
            if (spawnedPlayers.ContainsKey(playerInfo.PlayerId)) {
                return spawnedPlayers[playerInfo.PlayerId];
            }

            // Find appropriate prefab
            GameObject prefab = GetCharacterPrefab(playerInfo.CharacterClass);

            // Spawn player at their current position
            GameObject playerObject = Instantiate(
                prefab,
                playerInfo.Position,
                playerInfo.Rotation
            );

            // Set up as remote player
            PlayerController controller = playerObject.GetComponent<PlayerController>();
            if (controller != null) {
                controller.SetIsLocalPlayer(false);
            }

            // Add name label
            AddPlayerNameLabel(playerObject, playerInfo.PlayerName);

            // Add to spawned players dictionary
            spawnedPlayers[playerInfo.PlayerId] = playerObject;

            Debug.Log($"Remote player {playerInfo.PlayerName} spawned");
            return playerObject;
        }

        /// <summary>
        /// Get character prefab by class name
        /// </summary>
        /// <param name="characterClass">Character class name</param>
        /// <returns>Character prefab</returns>
        private GameObject GetCharacterPrefab(string characterClass) {
            // Default to first prefab if none available
            if (characterPrefabs == null || characterPrefabs.Length == 0) {
                Debug.LogWarning("No character prefabs assigned. Using default cube.");
                return GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            // Try to find matching prefab by name
            foreach (GameObject prefab in characterPrefabs) {
                if (prefab.name.Contains(characterClass)) {
                    return prefab;
                }
            }

            // Return first prefab as fallback
            return characterPrefabs[0];
        }

        /// <summary>
        /// Add a name label above a player
        /// </summary>
        /// <param name="playerObject">Player GameObject</param>
        /// <param name="playerName">Player name</param>
        private void AddPlayerNameLabel(GameObject playerObject, string playerName) {
            // Create a world space UI for the name
            GameObject labelObj = new GameObject("NameLabel");
            labelObj.transform.SetParent(playerObject.transform);
            labelObj.transform.localPosition = new Vector3(0, 2, 0); // Position above head

            // Add TextMesh component
            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = playerName;
            textMesh.fontSize = 24;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.LowerCenter;

            // Make text face camera always
            labelObj.AddComponent<FaceCamera>();
        }

        /// <summary>
        /// Handle remote player joining
        /// </summary>
        /// <param name="playerInfo">Player info</param>
        private void OnPlayerJoined(PlayerInfo playerInfo) {
            // Skip local player
            if (playerInfo.PlayerId == localPlayerId) return;

            // Spawn the remote player
            SpawnRemotePlayer(playerInfo);
        }

        /// <summary>
        /// Handle remote player leaving
        /// </summary>
        /// <param name="playerId">Player ID</param>
        private void OnPlayerLeft(string playerId) {
            // Skip local player
            if (playerId == localPlayerId) return;

            // Remove from spawned players
            if (spawnedPlayers.TryGetValue(playerId, out GameObject playerObject)) {
                Destroy(playerObject);
                spawnedPlayers.Remove(playerId);
                Debug.Log($"Remote player {playerId} despawned");
            }
        }

        /// <summary>
        /// Handle remote player movement
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="position">New position</param>
        /// <param name="rotation">New rotation</param>
        private void OnPlayerMoved(string playerId, Vector3 position, Quaternion rotation) {
            // Skip local player
            if (playerId == localPlayerId) return;

            // Update remote player position/rotation
            if (spawnedPlayers.TryGetValue(playerId, out GameObject playerObject)) {
                PlayerController controller = playerObject.GetComponent<PlayerController>();
                if (controller != null) {
                    controller.SetPositionAndRotation(position, rotation);
                } else {
                    // Fallback if controller not available
                    playerObject.transform.position = position;
                    playerObject.transform.rotation = rotation;
                }
            }
        }

        /// <summary>
        /// Handle local player position changes
        /// </summary>
        /// <param name="position">New position</param>
        /// <param name="rotation">New rotation</param>
        private void OnLocalPlayerPositionChanged(Vector3 position, Quaternion rotation) {
            // Update position on network
            if (NetworkManager.HasInstance && NetworkManager.Instance.IsConnected) {
                _ = NetworkManager.Instance.UpdatePositionAsync(position, rotation);
            }
        }

        /// <summary>
        /// Simple component to make an object face the camera
        /// </summary>
        private class FaceCamera : MonoBehaviour {
            private Camera mainCamera;

            private void Start() {
                mainCamera = Camera.main;
            }

            private void LateUpdate() {
                if (mainCamera != null) {
                    transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                        mainCamera.transform.rotation * Vector3.up);
                }
            }
        }
    }
}