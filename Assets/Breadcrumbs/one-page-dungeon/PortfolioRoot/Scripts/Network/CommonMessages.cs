using System;
using System.Collections.Generic;
using MessagePack;

namespace GamePortfolio.Network {
/// <summary>
    /// Game creation request data
    /// </summary>
    [MessagePackObject]
    public class GameCreationRequest
    {
        [Key(0)]
        public string PlayerId { get; set; }
        
        [Key(1)]
        public string GameName { get; set; }
        
        [Key(2)]
        public int MaxPlayers { get; set; }
        
        [Key(3)]
        public GameMode GameMode { get; set; }
        
        [Key(4)]
        public DungeonSettingsData DungeonSettings { get; set; }
        
        [Key(5)]
        public string Password { get; set; } // Optional, for private games
    }
    
    /// <summary>
    /// Game creation result data
    /// </summary>
    [MessagePackObject]
    public class GameCreationResult
    {
        [Key(0)]
        public bool Success { get; set; }
        
        [Key(1)]
        public string GameId { get; set; }
        
        [Key(2)]
        public string Message { get; set; }
    }
    
    /// <summary>
    /// Matchmaking request data
    /// </summary>
    [MessagePackObject]
    public class MatchmakingRequest
    {
        [Key(0)]
        public string PlayerId { get; set; }
        
        [Key(1)]
        public float PlayerSkillLevel { get; set; }
        
        [Key(2)]
        public float SkillLevelTolerance { get; set; }
        
        [Key(3)]
        public GameMode PreferredGameMode { get; set; }
        
        [Key(4)]
        public int PreferredMaxPlayers { get; set; }
        
        [Key(5)]
        public string GamePassword { get; set; } // For joining private games
    }
    
    /// <summary>
    /// Matchmaking result data
    /// </summary>
    [MessagePackObject]
    public class MatchmakingResult
    {
        [Key(0)]
        public bool Success { get; set; }
        
        [Key(1)]
        public string GameId { get; set; }
        
        [Key(2)]
        public string GameName { get; set; }
        
        [Key(3)]
        public string Message { get; set; }
    }
    
    /// <summary>
    /// Game list result data
    /// </summary>
    [MessagePackObject]
    public class GameListResult
    {
        [Key(0)]
        public List<GameSessionInfo> Games { get; set; }
    }
    
    /// <summary>
    /// Game session details data
    /// </summary>
    [MessagePackObject]
    public class GameSessionDetails
    {
        [Key(0)]
        public bool Success { get; set; }
        
        [Key(1)]
        public GameSessionInfo GameInfo { get; set; }
        
        [Key(2)]
        public string Message { get; set; }
    }
    
    /// <summary>
    /// Game session info data
    /// </summary>
    [MessagePackObject]
    public class GameSessionInfo
    {
        [Key(0)]
        public string GameId { get; set; }
        
        [Key(1)]
        public string GameName { get; set; }
        
        [Key(2)]
        public string HostPlayerId { get; set; }
        
        [Key(3)]
        public int MaxPlayers { get; set; }
        
        [Key(4)]
        public int PlayerCount { get; set; }
        
        [Key(5)]
        public List<string> Players { get; set; }
        
        [Key(6)]
        public GameMode GameMode { get; set; }
        
        [Key(7)]
        public DungeonSettingsData DungeonSettings { get; set; }
        
        [Key(8)]
        public DateTime CreationTime { get; set; }
        
        [Key(9)]
        public GameSessionStatus Status { get; set; }
        
        [Key(10)]
        public bool IsPrivate { get; set; }
        
        [Key(11)]
        public string Password { get; set; }
        
        [Key(12)]
        public float AverageSkillLevel { get; set; }
    }
    
    /// <summary>
    /// Dungeon settings data for game creation
    /// </summary>
    [MessagePackObject]
    public class DungeonSettingsData
    {
        [Key(0)]
        public int Width { get; set; }
        
        [Key(1)]
        public int Height { get; set; }
        
        [Key(2)]
        public int Difficulty { get; set; }
        
        [Key(3)]
        public DungeonTheme Theme { get; set; }
        
        [Key(4)]
        public bool EnableTraps { get; set; }
        
        [Key(5)]
        public bool EnableElites { get; set; }
    }
    
    /// <summary>
    /// Game session status enumeration
    /// </summary>
    //[MessagePackObject]
    public enum GameSessionStatus
    {
        [Key(0)]
        Waiting,
        
        [Key(1)]
        InProgress,
        
        [Key(2)]
        Finished
    }
    
    /// <summary>
    /// Game mode enumeration
    /// </summary>
    //[MessagePackObject]
    public enum GameMode
    {
        [Key(0)]
        Standard,
        
        [Key(1)]
        Hardcore,
        
        [Key(2)]
        TimeAttack,
        
        [Key(3)]
        TeamBattle
    }
    
    /// <summary>
    /// Dungeon theme enumeration
    /// </summary>
    //[MessagePackObject]
    public enum DungeonTheme
    {
        [Key(0)]
        Castle,
        
        [Key(1)]
        Cave,
        
        [Key(2)]
        Crypt,
        
        [Key(3)]
        Sewer,
        
        [Key(4)]
        Forest
    }
}