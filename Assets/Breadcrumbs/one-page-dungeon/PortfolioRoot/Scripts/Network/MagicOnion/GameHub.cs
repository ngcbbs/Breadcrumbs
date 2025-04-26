using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;
using UnityEngine;
using GamePortfolio.Dungeon.Generation;

namespace GamePortfolio.Network.GameHub {
    /// <summary>
    /// Interface for the game hub using MagicOnion
    /// </summary>
    public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver> {
        /// <summary>
        /// Join a game session
        /// </summary>
        /// <param name="request">Join request</param>
        /// <returns>Join result</returns>
        Task<JoinResult> JoinGameAsync(JoinRequest request);

        /// <summary>
        /// Update player position
        /// </summary>
        /// <param name="position">Player position</param>
        /// <param name="rotation">Player rotation</param>
        /// <returns>Task</returns>
        Task UpdatePositionAsync(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Perform a player action
        /// </summary>
        /// <param name="action">Action to perform</param>
        /// <returns>Action result</returns>
        Task<ActionResult> PerformActionAsync(PlayerAction action);

        /// <summary>
        /// Leave the current game session
        /// </summary>
        /// <returns>Task</returns>
        Task LeaveGameAsync();

        /// <summary>
        /// Send a chat message
        /// </summary>
        /// <param name="message">Chat message</param>
        /// <returns>Task</returns>
        Task SendMessageAsync(ChatMessage message);

        /// <summary>
        /// Update player status
        /// </summary>
        /// <param name="status">New player status</param>
        /// <returns>Task</returns>
        Task UpdateStatusAsync(PlayerStatus status);
    }

    /// <summary>
    /// Interface for receiving game hub events
    /// </summary>
    public interface IGameHubReceiver {
        /// <summary>
        /// Called when a player joins the game
        /// </summary>
        /// <param name="player">Player info</param>
        void OnPlayerJoined(PlayerInfo player);

        /// <summary>
        /// Called when a player leaves the game
        /// </summary>
        /// <param name="playerId">Player ID</param>
        void OnPlayerLeft(string playerId);

        /// <summary>
        /// Called when a player moves
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="position">New position</param>
        /// <param name="rotation">New rotation</param>
        void OnPlayerMoved(string playerId, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Called when a player performs an action
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="action">Action performed</param>
        /// <param name="result">Action result</param>
        void OnActionPerformed(string playerId, PlayerAction action, ActionResult result);

        /// <summary>
        /// Called when a chat message is received
        /// </summary>
        /// <param name="message">Chat message</param>
        void OnMessageReceived(ChatMessage message);

        /// <summary>
        /// Called when a player's status changes
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="status">New status</param>
        void OnPlayerStatusChanged(string playerId, PlayerStatus status);

        /// <summary>
        /// Called when the game state changes
        /// </summary>
        /// <param name="state">New game state</param>
        void OnGameStateChanged(GameState state);
    }

    /// <summary>
    /// Join request data
    /// </summary>
    [MessagePackObject]
    public class JoinRequest {
        [Key(0)]
        public string DungeonId { get; set; }

        [Key(1)]
        public string CharacterClass { get; set; }

        [Key(2)]
        public Dictionary<string, object> CustomProperties { get; set; }
    }

    /// <summary>
    /// Join result data
    /// </summary>
    [MessagePackObject]
    public class JoinResult {
        [Key(0)]
        public bool Success { get; set; }

        [Key(1)]
        public string ErrorMessage { get; set; }

        [Key(2)]
        public List<PlayerInfo> Players { get; set; }

        [Key(3)]
        public DungeonData DungeonData { get; set; }

        [Key(4)]
        public GameState CurrentState { get; set; }
    }

    /// <summary>
    /// Player information
    /// </summary>
    [MessagePackObject]
    public class PlayerInfo {
        [Key(0)]
        public string PlayerId { get; set; }

        [Key(1)]
        public string PlayerName { get; set; }

        [Key(2)]
        public string CharacterClass { get; set; }

        [Key(3)]
        public Vector3 Position { get; set; }

        [Key(4)]
        public Quaternion Rotation { get; set; }

        [Key(5)]
        public int Level { get; set; }

        [Key(6)]
        public int Health { get; set; }

        [Key(7)]
        public int MaxHealth { get; set; }

        [Key(8)]
        public PlayerStatus Status { get; set; }
    }

    /// <summary>
    /// Player action data
    /// </summary>
    [MessagePackObject]
    public class PlayerAction {
        [Key(0)]
        public ActionType Type { get; set; }

        [Key(1)]
        public string TargetId { get; set; }

        [Key(2)]
        public Vector3 Position { get; set; }

        [Key(3)]
        public Dictionary<string, object> Parameters { get; set; }
    }

    /// <summary>
    /// Action result data
    /// </summary>
    [MessagePackObject]
    public class ActionResult {
        [Key(0)]
        public bool Success { get; set; }

        [Key(1)]
        public string Message { get; set; }

        [Key(2)]
        public Dictionary<string, object> Results { get; set; }
    }

    /// <summary>
    /// Chat message data
    /// </summary>
    [MessagePackObject]
    public class ChatMessage {
        [Key(0)]
        public string SenderId { get; set; }

        [Key(1)]
        public string SenderName { get; set; }

        [Key(2)]
        public string Message { get; set; }

        [Key(3)]
        public ChatMessageType Type { get; set; }

        [Key(4)]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Player status enumeration
    /// </summary>
    //[MessagePackObject]
    public enum PlayerStatus {
        [Key(0)]
        Online,

        [Key(1)]
        AFK,

        [Key(2)]
        Combat,

        [Key(3)]
        Dead,

        [Key(4)]
        Trading
    }

    /// <summary>
    /// Game state enumeration
    /// </summary>
    //[MessagePackObject]
    public enum GameState {
        [Key(0)]
        Waiting,

        [Key(1)]
        Starting,

        [Key(2)]
        InProgress,

        [Key(3)]
        Ending,

        [Key(4)]
        Complete
    }

    /// <summary>
    /// Action type enumeration
    /// </summary>
    //[MessagePackObject]
    public enum ActionType {
        [Key(0)]
        Attack,

        [Key(1)]
        UseItem,

        [Key(2)]
        CastSkill,

        [Key(3)]
        Interact,

        [Key(4)]
        Pickup,

        [Key(5)]
        Drop
    }

    /// <summary>
    /// Chat message type enumeration
    /// </summary>
    //[MessagePackObject]
    public enum ChatMessageType {
        [Key(0)]
        Global,

        [Key(1)]
        Party,

        [Key(2)]
        Whisper,

        [Key(3)]
        System
    }
}