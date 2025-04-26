using System;
using System.Collections.Generic;
using MessagePack;
using MagicOnion;
using GamePortfolio.Dungeon.Generation;

namespace GamePortfolio.Network.GameService {
    /// <summary>
    /// Interface for the game service using MagicOnion
    /// </summary>
    public interface IGameService : IService<IGameService> {
        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="password">Password</param>
        /// <returns>Authentication response</returns>
        UnaryResult<AuthResponse> AuthenticateAsync(string userId, string password);

        /// <summary>
        /// Get dungeon data by ID
        /// </summary>
        /// <param name="dungeonId">Dungeon ID</param>
        /// <returns>Dungeon data</returns>
        UnaryResult<DungeonData> GetDungeonDataAsync(string dungeonId);

        /// <summary>
        /// Save player progress
        /// </summary>
        /// <param name="progress">Player progress data</param>
        /// <returns>Success flag</returns>
        UnaryResult<bool> SavePlayerProgressAsync(PlayerProgress progress);

        /// <summary>
        /// Get available dungeons
        /// </summary>
        /// <returns>List of available dungeons</returns>
        UnaryResult<List<DungeonInfo>> GetAvailableDungeonsAsync();

        /// <summary>
        /// Get player statistics
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <returns>Player statistics</returns>
        UnaryResult<PlayerStats> GetPlayerStatsAsync(string playerId);
    }

    /// <summary>
    /// Authentication response data
    /// </summary>
    [MessagePackObject]
    public class AuthResponse {
        [Key(0)]
        public bool Success { get; set; }

        [Key(1)]
        public string PlayerId { get; set; }

        [Key(2)]
        public string PlayerName { get; set; }

        [Key(3)]
        public string ErrorMessage { get; set; }

        [Key(4)]
        public string AuthToken { get; set; }
    }

    /// <summary>
    /// Player progress data for saving
    /// </summary>
    [MessagePackObject]
    public class PlayerProgress {
        [Key(0)]
        public string PlayerId { get; set; }

        [Key(1)]
        public string DungeonId { get; set; }

        [Key(2)]
        public int Level { get; set; }

        [Key(3)]
        public int Experience { get; set; }

        [Key(4)]
        public int Gold { get; set; }

        [Key(5)]
        public List<string> CompletedDungeons { get; set; }

        [Key(6)]
        public Dictionary<string, int> ItemInventory { get; set; }

        [Key(7)]
        public Dictionary<string, int> Skills { get; set; }

        [Key(8)]
        public DateTime LastSaved { get; set; }
    }

    /// <summary>
    /// Dungeon information for listing
    /// </summary>
    [MessagePackObject]
    public class DungeonInfo {
        [Key(0)]
        public string DungeonId { get; set; }

        [Key(1)]
        public string Name { get; set; }

        [Key(2)]
        public string Description { get; set; }

        [Key(3)]
        public int Difficulty { get; set; }

        [Key(4)]
        public int RecommendedLevel { get; set; }

        [Key(5)]
        public int MaxPlayers { get; set; }

        [Key(6)]
        public int CurrentPlayers { get; set; }

        [Key(7)]
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Player statistics
    /// </summary>
    [MessagePackObject]
    public class PlayerStats {
        [Key(0)]
        public string PlayerId { get; set; }

        [Key(1)]
        public string PlayerName { get; set; }

        [Key(2)]
        public int TotalGamesPlayed { get; set; }

        [Key(3)]
        public int DungeonsCompleted { get; set; }

        [Key(4)]
        public int HighestLevel { get; set; }

        [Key(5)]
        public int TotalGold { get; set; }

        [Key(6)]
        public int TotalEnemiesDefeated { get; set; }

        [Key(7)]
        public int TotalDeaths { get; set; }

        [Key(8)]
        public Dictionary<string, int> ItemsFound { get; set; }

        [Key(9)]
        public DateTime LastActive { get; set; }
    }
}
