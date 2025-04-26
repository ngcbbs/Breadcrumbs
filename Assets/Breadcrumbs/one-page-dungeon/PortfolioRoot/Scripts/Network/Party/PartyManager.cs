#if INCOMPLETE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using GamePortfolio.Network.GameHub;
using GamePortfolio.UI;

namespace GamePortfolio.Network.Party {
    /// <summary>
    /// Client-side party manager to interact with the party service
    /// </summary>
    public class PartyManager : MonoBehaviour {
        // Singleton instance
        public static PartyManager Instance { get; private set; }

        // Events
        public event Action<bool, string> OnPartyCreated;
        public event Action<PartyInfo> OnPartyUpdated;
        public event Action<List<PartyInvitation>> OnInvitationsUpdated;
        public event Action<string, string> OnInvitationReceived;
        public event Action<bool, string> OnPartyJoined;
        public event Action<bool, string> OnPartyLeft;
        public event Action<string, string> OnMemberJoined;
        public event Action<string, string> OnMemberLeft;
        public event Action<string, string> OnLeaderChanged;

        // Reference to the network manager
        [SerializeField]
        private NetworkManager networkManager;

        // Party service client
        private IPartyService partyService;

        // Properties
        public string CurrentPartyId { get; private set; }
        public bool IsInParty => !string.IsNullOrEmpty(CurrentPartyId);
        public PartyInfo CurrentParty { get; private set; }

        public bool IsPartyLeader => IsInParty && CurrentParty != null &&
                                     CurrentParty.LeaderId == networkManager.PlayerId;

        // Cached invitations
        private List<PartyInvitation> activeInvitations = new List<PartyInvitation>();

        // Auto-refresh timer
        [SerializeField]
        private float invitationRefreshInterval = 10f;
        private float lastInvitationRefreshTime;

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
            // Initialize the party service when NetworkManager is connected
            if (networkManager != null) {
                networkManager.OnConnected += InitializePartyService;
                networkManager.OnDisconnected += CleanupPartyService;

                // Register for game hub events
                if (networkManager.HubReceiver != null) {
                    Debug.Log("todo: fixme !! Register for game hub events");
                    /*
                    networkManager.HubReceiver.OnPlayerJoined += HandlePlayerJoined;
                    networkManager.HubReceiver.OnPlayerLeft += HandlePlayerLeft;
                    // */
                }
            }
        }

        private void Update() {
            // Auto-refresh invitations
            if (partyService != null && networkManager.IsConnected) {
                if (Time.time - lastInvitationRefreshTime > invitationRefreshInterval) {
                    lastInvitationRefreshTime = Time.time;
                    RefreshInvitations().Forget();
                }
            }
        }

        private void OnDestroy() {
            if (networkManager != null) {
                networkManager.OnConnected -= InitializePartyService;
                networkManager.OnDisconnected -= CleanupPartyService;

                // Unregister from game hub events
                if (networkManager.HubReceiver != null) {
                    Debug.Log("todo: fixme !! Unregister for game hub events");
                    /*
                    networkManager.HubReceiver.OnPlayerJoined -= HandlePlayerJoined;
                    networkManager.HubReceiver.OnPlayerLeft -= HandlePlayerLeft;
                    // */
                }
            }
        }

        /// <summary>
        /// Initialize the party service client
        /// </summary>
        private void InitializePartyService() {
            try {
                // Create the party service client
                partyService = networkManager.CreateService<IPartyService>();
                Debug.Log("Party service initialized");

                // Auto-refresh invitations
                RefreshInvitations().Forget();
            } catch (Exception ex) {
                Debug.LogError($"Failed to initialize party service: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup party service on disconnect
        /// </summary>
        private void CleanupPartyService() {
            partyService = null;
            CurrentPartyId = null;
            CurrentParty = null;
            activeInvitations.Clear();
            OnInvitationsUpdated?.Invoke(activeInvitations);
        }

        /// <summary>
        /// Handle player joined event from GameHub
        /// </summary>
        private void HandlePlayerJoined(PlayerInfo player) {
            // Only process if in a party and the player joined the same game
            if (IsInParty && networkManager.GameHubClient != null) {
                // Get current party details to see if the player joined our party
                GetPartyDetails(CurrentPartyId).AsUniTask().Forget();

                // Check if this player is in our party
                if (CurrentParty != null && CurrentParty.Members.Any(m => m.PlayerId == player.PlayerId)) {
                    OnMemberJoined?.Invoke(player.PlayerId, player.PlayerName);
                }
            }
        }

        /// <summary>
        /// Handle player left event from GameHub
        /// </summary>
        private void HandlePlayerLeft(string playerId) {
            // Only process if in a party
            if (IsInParty && CurrentParty != null) {
                // Check if the player was in our party
                PartyMember member = CurrentParty.Members.FirstOrDefault(m => m.PlayerId == playerId);
                if (member != null) {
                    // Update party details
                    GetPartyDetails(CurrentPartyId).AsUniTask().Forget();

                    OnMemberLeft?.Invoke(playerId, member.PlayerName);
                }
            }
        }

        /// <summary>
        /// Create a new party
        /// </summary>
        /// <param name="partyName">Party name</param>
        /// <param name="maxMembers">Maximum number of members</param>
        /// <param name="isPrivate">Whether the party is private</param>
        public async UniTaskVoid CreateParty(string partyName, int maxMembers, bool isPrivate) {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot create party: Not connected to server");
                    OnPartyCreated?.Invoke(false, "Not connected to server");
                    return;
                }

                if (IsInParty) {
                    Debug.LogWarning("Cannot create party: Already in a party");
                    OnPartyCreated?.Invoke(false, "Already in a party");
                    return;
                }

                // Create party request
                PartyCreationRequest request = new PartyCreationRequest {
                    LeaderId = networkManager.PlayerId,
                    LeaderName = networkManager.PlayerName,
                    PartyName = partyName,
                    MaxMembers = maxMembers,
                    IsPrivate = isPrivate,
                    LeaderClass = "Warrior" // todo: fixme !! PlayerManager.Instance.CurrentCharacterClass
                };

                // Call the service
                PartyCreationResult result = await partyService.CreatePartyAsync(request);

                if (result.Success) {
                    // Store the party ID
                    CurrentPartyId = result.PartyId;

                    // Get party details
                    await GetPartyDetails(result.PartyId);

                    // Notify listeners
                    OnPartyCreated?.Invoke(true, result.PartyId);

                    Debug.Log($"Party created successfully: {result.PartyId}");
                } else {
                    // Notify listeners of failure
                    OnPartyCreated?.Invoke(false, result.Message);

                    Debug.LogWarning($"Failed to create party: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error creating party: {ex.Message}");
                OnPartyCreated?.Invoke(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Invite a player to the party
        /// </summary>
        /// <param name="inviteeId">ID of the player to invite</param>
        /// <param name="inviteeName">Name of the player to invite</param>
        public async UniTaskVoid InvitePlayer(string inviteeId, string inviteeName) {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot invite player: Not connected to server");
                    UIManager.Instance.ShowMessage("Cannot invite player: Not connected to server");
                    return;
                }

                if (!IsInParty) {
                    Debug.LogWarning("Cannot invite player: Not in a party");
                    UIManager.Instance.ShowMessage("Cannot invite player: Not in a party");
                    return;
                }

                if (!IsPartyLeader) {
                    Debug.LogWarning("Cannot invite player: Not the party leader");
                    UIManager.Instance.ShowMessage("Only the party leader can send invitations");
                    return;
                }

                // Create invitation request
                PartyInvitationRequest request = new PartyInvitationRequest {
                    PartyId = CurrentPartyId,
                    InviterId = networkManager.PlayerId,
                    InviterName = networkManager.PlayerName,
                    InviteeId = inviteeId
                };

                // Call the service
                PartyInvitationResult result = await partyService.InviteToPartyAsync(request);

                if (result.Success) {
                    UIManager.Instance.ShowMessage($"Invitation sent to {inviteeName}");
                    Debug.Log($"Invitation sent to {inviteeId} ({inviteeName})");
                } else {
                    UIManager.Instance.ShowMessage($"Failed to invite player: {result.Message}");
                    Debug.LogWarning($"Failed to invite player: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error inviting player: {ex.Message}");
                UIManager.Instance.ShowMessage("Error sending invitation");
            }
        }

        /// <summary>
        /// Get active invitations for the current player
        /// </summary>
        public async UniTaskVoid RefreshInvitations() {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    return;
                }

                // Get invitations
                PartyInvitationListResult result = await partyService.GetInvitationsAsync(networkManager.PlayerId);

                // Check for new invitations
                if (result.Invitations != null) {
                    // Find new invitations (not in active list)
                    foreach (var invitation in result.Invitations) {
                        if (!activeInvitations.Any(i => i.InvitationId == invitation.InvitationId)) {
                            // New invitation received
                            OnInvitationReceived?.Invoke(invitation.InviterName, invitation.PartyName);
                        }
                    }

                    // Update cached list
                    activeInvitations = result.Invitations;

                    // Notify listeners
                    OnInvitationsUpdated?.Invoke(activeInvitations);
                }
            } catch (Exception ex) {
                Debug.LogError($"Error refreshing invitations: {ex.Message}");
            }
        }

        /// <summary>
        /// Respond to a party invitation
        /// </summary>
        /// <param name="invitationId">Invitation ID</param>
        /// <param name="accept">Whether to accept the invitation</param>
        public async UniTaskVoid RespondToInvitation(string invitationId, bool accept) {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot respond to invitation: Not connected to server");
                    OnPartyJoined?.Invoke(false, "Not connected to server");
                    return;
                }

                if (IsInParty && accept) {
                    Debug.LogWarning("Cannot accept invitation: Already in a party");
                    OnPartyJoined?.Invoke(false, "Already in a party");
                    return;
                }

                // Find the invitation
                PartyInvitation invitation = activeInvitations.FirstOrDefault(i => i.InvitationId == invitationId);
                if (invitation == null) {
                    Debug.LogWarning("Cannot respond to invitation: Invitation not found");
                    OnPartyJoined?.Invoke(false, "Invitation not found");
                    return;
                }

                // Create response request
                PartyResponseRequest request = new PartyResponseRequest {
                    InvitationId = invitationId,
                    PlayerId = networkManager.PlayerId,
                    PlayerName = networkManager.PlayerName,
                    Accept = accept,
                    CharacterClass = "Warrior" // todo: fixme !! PlayerManager.Instance.CurrentCharacterClass
                };

                // Call the service
                PartyResponseResult result = await partyService.RespondToInvitationAsync(request);

                // Remove invitation from local cache
                activeInvitations.RemoveAll(i => i.InvitationId == invitationId);
                OnInvitationsUpdated?.Invoke(activeInvitations);

                if (result.Success && accept) {
                    // Store party info
                    CurrentPartyId = result.PartyId;
                    CurrentParty = result.PartyInfo;

                    // Notify listeners
                    OnPartyJoined?.Invoke(true, "Successfully joined party");
                    OnPartyUpdated?.Invoke(CurrentParty);

                    Debug.Log($"Joined party: {result.PartyId}");
                } else if (result.Success && !accept) {
                    Debug.Log("Invitation declined");
                } else {
                    // Notify listeners of failure
                    OnPartyJoined?.Invoke(false, result.Message);

                    Debug.LogWarning($"Failed to join party: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error responding to invitation: {ex.Message}");
                OnPartyJoined?.Invoke(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Leave the current party
        /// </summary>
        public async UniTaskVoid LeaveParty() {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot leave party: Not connected to server");
                    OnPartyLeft?.Invoke(false, "Not connected to server");
                    return;
                }

                if (!IsInParty) {
                    Debug.LogWarning("Cannot leave party: Not in a party");
                    OnPartyLeft?.Invoke(false, "Not in a party");
                    return;
                }

                // Create leave request
                PartyLeaveRequest request = new PartyLeaveRequest {
                    PartyId = CurrentPartyId,
                    PlayerId = networkManager.PlayerId
                };

                // Call the service
                PartyLeaveResult result = await partyService.LeavePartyAsync(request);

                if (result.Success) {
                    // Clear party info
                    string oldPartyId = CurrentPartyId;
                    CurrentPartyId = null;
                    CurrentParty = null;

                    // Notify listeners
                    OnPartyLeft?.Invoke(true,
                        result.PartyDissolved ? "Party dissolved as you were the last member" : "Successfully left party");

                    Debug.Log($"Left party: {oldPartyId}, Dissolved: {result.PartyDissolved}");
                } else {
                    // Notify listeners of failure
                    OnPartyLeft?.Invoke(false, result.Message);

                    Debug.LogWarning($"Failed to leave party: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error leaving party: {ex.Message}");
                OnPartyLeft?.Invoke(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Kick a player from the party
        /// </summary>
        /// <param name="playerId">ID of the player to kick</param>
        public async UniTaskVoid KickPlayer(string playerId) {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot kick player: Not connected to server");
                    UIManager.Instance.ShowMessage("Cannot kick player: Not connected to server");
                    return;
                }

                if (!IsInParty) {
                    Debug.LogWarning("Cannot kick player: Not in a party");
                    UIManager.Instance.ShowMessage("Cannot kick player: Not in a party");
                    return;
                }

                if (!IsPartyLeader) {
                    Debug.LogWarning("Cannot kick player: Not the party leader");
                    UIManager.Instance.ShowMessage("Only the party leader can kick members");
                    return;
                }

                // Create kick request
                PartyKickRequest request = new PartyKickRequest {
                    PartyId = CurrentPartyId,
                    KickerId = networkManager.PlayerId,
                    TargetId = playerId
                };

                // Call the service
                PartyKickResult result = await partyService.KickFromPartyAsync(request);

                if (result.Success) {
                    // Update party details
                    await GetPartyDetails(CurrentPartyId);

                    UIManager.Instance.ShowMessage("Player kicked from party");
                    Debug.Log($"Kicked player from party: {playerId}");
                } else {
                    UIManager.Instance.ShowMessage($"Failed to kick player: {result.Message}");
                    Debug.LogWarning($"Failed to kick player: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error kicking player: {ex.Message}");
                UIManager.Instance.ShowMessage("Error kicking player");
            }
        }

        /// <summary>
        /// Transfer party leadership to another player
        /// </summary>
        /// <param name="newLeaderId">ID of the new leader</param>
        public async UniTaskVoid TransferLeadership(string newLeaderId) {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot transfer leadership: Not connected to server");
                    UIManager.Instance.ShowMessage("Cannot transfer leadership: Not connected to server");
                    return;
                }

                if (!IsInParty) {
                    Debug.LogWarning("Cannot transfer leadership: Not in a party");
                    UIManager.Instance.ShowMessage("Cannot transfer leadership: Not in a party");
                    return;
                }

                if (!IsPartyLeader) {
                    Debug.LogWarning("Cannot transfer leadership: Not the party leader");
                    UIManager.Instance.ShowMessage("Only the party leader can transfer leadership");
                    return;
                }

                // Create transfer request
                PartyTransferRequest request = new PartyTransferRequest {
                    PartyId = CurrentPartyId,
                    CurrentLeaderId = networkManager.PlayerId,
                    NewLeaderId = newLeaderId
                };

                // Call the service
                PartyTransferResult result = await partyService.TransferLeadershipAsync(request);

                if (result.Success) {
                    // Find the new leader name
                    string newLeaderName = "Unknown";
                    PartyMember member = CurrentParty?.Members.FirstOrDefault(m => m.PlayerId == newLeaderId);
                    if (member != null) {
                        newLeaderName = member.PlayerName;
                    }

                    // Update party details
                    await GetPartyDetails(CurrentPartyId);

                    // Notify listeners
                    OnLeaderChanged?.Invoke(newLeaderId, newLeaderName);

                    UIManager.Instance.ShowMessage($"Leadership transferred to {newLeaderName}");
                    Debug.Log($"Leadership transferred to {newLeaderId} ({newLeaderName})");
                } else {
                    UIManager.Instance.ShowMessage($"Failed to transfer leadership: {result.Message}");
                    Debug.LogWarning($"Failed to transfer leadership: {result.Message}");
                }
            } catch (Exception ex) {
                Debug.LogError($"Error transferring leadership: {ex.Message}");
                UIManager.Instance.ShowMessage("Error transferring leadership");
            }
        }

        /// <summary>
        /// Get party details
        /// </summary>
        /// <param name="partyId">Party ID</param>
        public async Task GetPartyDetails(string partyId) {
            try {
                if (partyService == null || !networkManager.IsConnected) {
                    Debug.LogWarning("Cannot get party details: Not connected to server");
                    return;
                }

                // Get party details
                PartyDetailsResult result = await partyService.GetPartyDetailsAsync(partyId);

                if (result.Success) {
                    // Update party info
                    CurrentParty = result.PartyInfo;

                    // Notify listeners
                    OnPartyUpdated?.Invoke(CurrentParty);

                    Debug.Log($"Retrieved details for party: {partyId}");
                } else {
                    Debug.LogWarning($"Failed to get party details: {result.Message}");

                    // If party no longer exists, clear current party
                    if (partyId == CurrentPartyId) {
                        CurrentPartyId = null;
                        CurrentParty = null;
                        OnPartyLeft?.Invoke(true, "Party no longer exists");
                    }
                }
            } catch (Exception ex) {
                Debug.LogError($"Error getting party details: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the list of active invitations
        /// </summary>
        public List<PartyInvitation> GetActiveInvitations() {
            return new List<PartyInvitation>(activeInvitations);
        }

        /// <summary>
        /// Set the ready status for the current player
        /// </summary>
        /// <param name="isReady">Ready status</param>
        public void SetReady(bool isReady) {
            if (IsInParty && CurrentParty != null) {
                // Find and update the current player's ready status
                PartyMember currentMember = CurrentParty.Members.FirstOrDefault(m => m.PlayerId == networkManager.PlayerId);
                if (currentMember != null) {
                    currentMember.IsReady = isReady;
                    OnPartyUpdated?.Invoke(CurrentParty);
                }
            }
        }

        /// <summary>
        /// Check if all party members are ready
        /// </summary>
        /// <returns>True if all members are ready</returns>
        public bool AreAllMembersReady() {
            if (!IsInParty || CurrentParty == null || CurrentParty.Members.Count == 0) {
                return false;
            }

            return CurrentParty.Members.All(m => m.IsReady);
        }
    }
}
#endif