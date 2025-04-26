using System;
using System.Collections.Generic;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using UnityEngine;

namespace GamePortfolio.Network.Matchmaking
{
    /// <summary>
    /// Interface for the matchmaking service
    /// </summary>
    public interface IMatchmakingService : IService<IMatchmakingService>
    {
        /// <summary>
        /// Create a new game session
        /// </summary>
        /// <param name="request">Game creation request</param>
        /// <returns>Game creation result</returns>
        UnaryResult<GameCreationResult> CreateGameAsync(GameCreationRequest request);
        
        /// <summary>
        /// Find a game session to join
        /// </summary>
        /// <param name="request">Matchmaking request</param>
        /// <returns>Matchmaking result</returns>
        UnaryResult<MatchmakingResult> FindGameAsync(MatchmakingRequest request);
        
        /// <summary>
        /// Get available game sessions
        /// </summary>
        /// <returns>List of available game sessions</returns>
        UnaryResult<GameListResult> ListGamesAsync();
        
        /// <summary>
        /// Get details of a specific game session
        /// </summary>
        /// <param name="gameId">Game session ID</param>
        /// <returns>Game session details</returns>
        UnaryResult<GameSessionDetails> GetGameDetailsAsync(string gameId);
    }
    #if ITZ_SERVER_SIDE
    /// <summary>
    /// Implementation of the matchmaking service
    /// </summary>
    public class MatchmakingService : ServiceBase<IMatchmakingService>, IMatchmakingService
    {
        private static readonly Dictionary<string, GameSessionInfo> ActiveGames = new Dictionary<string, GameSessionInfo>();
        private static readonly object LockObject = new object();

        /// <summary>
        /// Create a new game session
        /// </summary>
        /// <param name="request">Game creation request</param>
        /// <returns>Game creation result</returns>
        public UnaryResult<GameCreationResult> CreateGameAsync(GameCreationRequest request)
        {
            // Generate a unique game ID
            string gameId = Guid.NewGuid().ToString();
            
            // Create game session info
            GameSessionInfo gameInfo = new GameSessionInfo
            {
                GameId = gameId,
                GameName = request.GameName,
                HostPlayerId = request.PlayerId,
                MaxPlayers = request.MaxPlayers,
                GameMode = request.GameMode,
                DungeonSettings = request.DungeonSettings,
                Password = request.Password,
                IsPrivate = !string.IsNullOrEmpty(request.Password),
                CreationTime = DateTime.UtcNow,
                PlayerCount = 1, // Host is the first player
                Players = new List<string> { request.PlayerId },
                Status = GameSessionStatus.Waiting
            };
            
            // Add to active games
            lock (LockObject)
            {
                ActiveGames[gameId] = gameInfo;
            }
            
            // Log game creation
            Logger.Debug($"Game created: {gameId}, Name: {request.GameName}, Host: {request.PlayerId}");
            
            // Return result
            return new GameCreationResult
            {
                Success = true,
                GameId = gameId,
                Message = "Game created successfully"
            }.AsUnaryResult();
        }
        
        /// <summary>
        /// Find a game session to join based on matchmaking criteria
        /// </summary>
        /// <param name="request">Matchmaking request</param>
        /// <returns>Matchmaking result</returns>
        public UnaryResult<MatchmakingResult> FindGameAsync(MatchmakingRequest request)
        {
            List<GameSessionInfo> candidateGames = new List<GameSessionInfo>();
            
            // Find candidate games that match criteria
            lock (LockObject)
            {
                foreach (var game in ActiveGames.Values)
                {
                    // Skip games that don't match basic criteria
                    if (game.Status != GameSessionStatus.Waiting || 
                        game.PlayerCount >= game.MaxPlayers || 
                        game.IsPrivate || 
                        game.GameMode != request.PreferredGameMode)
                    {
                        continue;
                    }
                    
                    // Check if skill level is compatible
                    if (Math.Abs(game.AverageSkillLevel - request.PlayerSkillLevel) > request.SkillLevelTolerance)
                    {
                        continue;
                    }
                    
                    candidateGames.Add(game);
                }
            }
            
            // Sort candidates by best match
            candidateGames.Sort((a, b) => 
            {
                // Prioritize games with more players but not full
                int playerCountDiffA = a.MaxPlayers - a.PlayerCount;
                int playerCountDiffB = b.MaxPlayers - b.PlayerCount;
                
                if (playerCountDiffA != playerCountDiffB)
                {
                    return playerCountDiffA.CompareTo(playerCountDiffB);
                }
                
                // Then by skill level match
                float skillDiffA = Math.Abs(a.AverageSkillLevel - request.PlayerSkillLevel);
                float skillDiffB = Math.Abs(b.AverageSkillLevel - request.PlayerSkillLevel);
                
                return skillDiffA.CompareTo(skillDiffB);
            });
            
            // Get best match or return no match
            if (candidateGames.Count > 0)
            {
                GameSessionInfo bestMatch = candidateGames[0];
                
                // Reserve a spot in the game
                lock (LockObject)
                {
                    if (ActiveGames.TryGetValue(bestMatch.GameId, out GameSessionInfo game))
                    {
                        if (game.PlayerCount < game.MaxPlayers)
                        {
                            game.PlayerCount++;
                            game.Players.Add(request.PlayerId);
                            
                            // Update average skill level
                            float totalSkill = game.AverageSkillLevel * (game.PlayerCount - 1) + request.PlayerSkillLevel;
                            game.AverageSkillLevel = totalSkill / game.PlayerCount;
                            
                            return new MatchmakingResult
                            {
                                Success = true,
                                GameId = game.GameId,
                                GameName = game.GameName,
                                Message = "Game found"
                            }.AsUnaryResult();
                        }
                    }
                }
            }
            
            // No suitable game found, return failure
            return new MatchmakingResult
            {
                Success = false,
                Message = "No suitable game found"
            }.AsUnaryResult();
        }
        
        /// <summary>
        /// List all available game sessions
        /// </summary>
        /// <returns>List of game sessions</returns>
        public UnaryResult<GameListResult> ListGamesAsync()
        {
            List<GameSessionInfo> games = new List<GameSessionInfo>();
            
            lock (LockObject)
            {
                foreach (var game in ActiveGames.Values)
                {
                    // Don't include private games in the list
                    if (!game.IsPrivate && game.Status == GameSessionStatus.Waiting)
                    {
                        // Create a copy without sensitive info like password
                        games.Add(new GameSessionInfo
                        {
                            GameId = game.GameId,
                            GameName = game.GameName,
                            HostPlayerId = game.HostPlayerId,
                            MaxPlayers = game.MaxPlayers,
                            PlayerCount = game.PlayerCount,
                            GameMode = game.GameMode,
                            CreationTime = game.CreationTime,
                            Status = game.Status,
                            IsPrivate = false,
                            Password = null // Don't include password
                        });
                    }
                }
            }
            
            return new GameListResult
            {
                Games = games
            }.AsUnaryResult();
        }
        
        /// <summary>
        /// Get details of a specific game session
        /// </summary>
        /// <param name="gameId">Game session ID</param>
        /// <returns>Game session details</returns>
        public UnaryResult<GameSessionDetails> GetGameDetailsAsync(string gameId)
        {
            lock (LockObject)
            {
                if (ActiveGames.TryGetValue(gameId, out GameSessionInfo game))
                {
                    return new GameSessionDetails
                    {
                        Success = true,
                        GameInfo = new GameSessionInfo
                        {
                            GameId = game.GameId,
                            GameName = game.GameName,
                            HostPlayerId = game.HostPlayerId,
                            MaxPlayers = game.MaxPlayers,
                            PlayerCount = game.PlayerCount,
                            Players = game.Players,
                            GameMode = game.GameMode,
                            DungeonSettings = game.DungeonSettings,
                            CreationTime = game.CreationTime,
                            Status = game.Status,
                            IsPrivate = game.IsPrivate,
                            Password = null // Don't include password
                        }
                    }.AsUnaryResult();
                }
            }
            
            return new GameSessionDetails
            {
                Success = false,
                Message = "Game not found"
            }.AsUnaryResult();
        }
        
        /// <summary>
        /// Internal method to remove inactive games (called periodically by cleanup job)
        /// </summary>
        internal static void CleanupInactiveGames()
        {
            DateTime cutoffTime = DateTime.UtcNow.AddMinutes(-30); // Games older than 30 minutes
            List<string> gamesToRemove = new List<string>();
            
            lock (LockObject)
            {
                foreach (var gameEntry in ActiveGames)
                {
                    if (gameEntry.Value.CreationTime < cutoffTime && 
                        gameEntry.Value.Status == GameSessionStatus.Waiting)
                    {
                        gamesToRemove.Add(gameEntry.Key);
                    }
                }
                
                foreach (string gameId in gamesToRemove)
                {
                    ActiveGames.Remove(gameId);
                    Logger.Debug($"Removed inactive game: {gameId}");
                }
            }
        }
    }
    #endif
}
