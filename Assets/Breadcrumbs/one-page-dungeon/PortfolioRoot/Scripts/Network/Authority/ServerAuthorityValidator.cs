using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Network.GameHub;

namespace GamePortfolio.Network.Authority {
    /// <summary>
    /// Server-side authority validation system to prevent cheating
    /// </summary>
    public class ServerAuthorityValidator {
        // Validation thresholds
        private readonly float positionTolerance = 5.0f;         // Maximum allowed position difference
        private readonly float speedTolerance = 15.0f;           // Maximum allowed speed (units/sec)
        private readonly float damageMultiplierTolerance = 2.0f; // Maximum allowed damage multiplier
        private readonly float cooldownTolerance = 0.1f;         // Cooldown tolerance in seconds

        // Player state tracking
        private Dictionary<string, PlayerValidationState> playerStates = new Dictionary<string, PlayerValidationState>();

        // Action cooldowns (action type -> cooldown in seconds)
        private Dictionary<ActionType, float> actionCooldowns = new Dictionary<ActionType, float> {
            { ActionType.Attack, 0.5f },
            { ActionType.CastSkill, 1.0f },
            { ActionType.UseItem, 0.3f }
        };

        // Cheating detection counts
        private Dictionary<string, CheatDetectionCount> cheatCounts = new Dictionary<string, CheatDetectionCount>();

        // Violation thresholds before action is taken
        private readonly int minorViolationThreshold = 5;
        private readonly int majorViolationThreshold = 3;

        /// <summary>
        /// Validate a player position update
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="newPosition">New position</param>
        /// <param name="timestamp">Timestamp of update</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidatePositionUpdate(string playerId, Vector3 newPosition,
            Vector3 newRotation, DateTime timestamp) {
            // Ensure player state exists
            EnsurePlayerState(playerId);
            PlayerValidationState state = playerStates[playerId];

            // Calculate time delta
            TimeSpan timeDelta = timestamp - state.LastUpdateTime;
            float deltaTime = (float)timeDelta.TotalSeconds;

            // Skip validation if first update or very long time since last update
            if (state.LastUpdateTime == DateTime.MinValue || deltaTime > 5.0f) {
                state.LastPosition = newPosition;
                state.LastRotation = newRotation;
                state.LastUpdateTime = timestamp;
                state.LastValidationResult = ValidationResult.Valid;
                return ValidationResult.Valid;
            }

            // Calculate movement distance and speed
            float distance = Vector3.Distance(state.LastPosition, newPosition);
            float speed = distance / deltaTime;

            ValidationResult result = ValidationResult.Valid;

            // Check for position jumps
            if (distance > positionTolerance && deltaTime < 1.0f) {
                result = ValidationResult.PositionJump;
                RecordViolation(playerId, ViolationType.PositionJump);
            }
            // Check for speed hacks
            else if (speed > speedTolerance) {
                result = ValidationResult.SpeedHack;
                RecordViolation(playerId, ViolationType.SpeedHack);
            }

            // Update player state
            state.LastPosition = newPosition;
            state.LastRotation = newRotation;
            state.LastUpdateTime = timestamp;
            state.LastValidationResult = result;

            return result;
        }

        /// <summary>
        /// Validate a player action
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="action">Action to validate</param>
        /// <param name="timestamp">Timestamp of action</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidateAction(string playerId, PlayerAction action, DateTime timestamp) {
            // Ensure player state exists
            EnsurePlayerState(playerId);
            PlayerValidationState state = playerStates[playerId];

            ValidationResult result = ValidationResult.Valid;

            // Check for action cooldown violations
            if (actionCooldowns.TryGetValue(action.Type, out float cooldown)) {
                TimeSpan timeSinceLastAction =
                    timestamp - state.LastActionTimes.GetValueOrDefault(action.Type, DateTime.MinValue);
                float actionDelta = (float)timeSinceLastAction.TotalSeconds;

                if (actionDelta < cooldown - cooldownTolerance) {
                    result = ValidationResult.CooldownViolation;
                    RecordViolation(playerId, ViolationType.CooldownViolation);
                }
            }

            // Check for action distance violations
            if (action.Type == ActionType.Attack || action.Type == ActionType.CastSkill) {
                // Validate target is in range
                if (!string.IsNullOrEmpty(action.TargetId) && action.Parameters != null &&
                    action.Parameters.TryGetValue("range", out object rangeObj)) {
                    float range;
                    if (float.TryParse(rangeObj.ToString(), out range)) {
                        // Get target position (would require server-side lookup in real implementation)
                        // For this example, we use position field for target position
                        Vector3 targetPosition = action.Position;
                        float distance = Vector3.Distance(state.LastPosition, targetPosition);

                        if (distance > range * 1.5f) // Allow 50% extra range for network conditions
                        {
                            result = ValidationResult.RangeViolation;
                            RecordViolation(playerId, ViolationType.RangeViolation);
                        }
                    }
                }

                // Validate damage if specified
                if (action.Parameters != null && action.Parameters.TryGetValue("damage", out object damageObj)) {
                    float damage;
                    if (float.TryParse(damageObj.ToString(), out damage)) {
                        // Get player's base damage (would require server-side player state in real implementation)
                        float baseDamage = 10.0f; // Example base damage

                        if (damage > baseDamage * damageMultiplierTolerance) {
                            result = ValidationResult.DamageHack;
                            RecordViolation(playerId, ViolationType.DamageHack);
                        }
                    }
                }
            }

            // Update action timestamp
            state.LastActionTimes[action.Type] = timestamp;

            return result;
        }

        /// <summary>
        /// Record a cheating violation
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="violationType">Type of violation</param>
        private void RecordViolation(string playerId, ViolationType violationType) {
            // Ensure cheat counts exist for player
            if (!cheatCounts.ContainsKey(playerId)) {
                cheatCounts[playerId] = new CheatDetectionCount();
            }

            CheatDetectionCount counts = cheatCounts[playerId];

            // Update violation counts
            switch (violationType) {
                case ViolationType.PositionJump:
                    counts.PositionJumpCount++;
                    break;

                case ViolationType.SpeedHack:
                    counts.SpeedHackCount++;
                    break;

                case ViolationType.CooldownViolation:
                    counts.CooldownViolationCount++;
                    break;

                case ViolationType.RangeViolation:
                    counts.RangeViolationCount++;
                    break;

                case ViolationType.DamageHack:
                    counts.DamageHackCount++;
                    break;
            }

            // Check if player should be flagged
            if (ShouldFlagPlayer(counts)) {
                FlagPlayerForReview(playerId, counts);
            }

            // Check if player should be kicked
            if (ShouldKickPlayer(counts)) {
                KickPlayer(playerId, counts);
            }
        }

        /// <summary>
        /// Check if a player should be flagged for review
        /// </summary>
        /// <param name="counts">Violation counts</param>
        /// <returns>True if player should be flagged</returns>
        private bool ShouldFlagPlayer(CheatDetectionCount counts) {
            return counts.PositionJumpCount >= minorViolationThreshold ||
                   counts.SpeedHackCount >= minorViolationThreshold ||
                   counts.CooldownViolationCount >= minorViolationThreshold ||
                   counts.RangeViolationCount >= minorViolationThreshold ||
                   counts.DamageHackCount >= minorViolationThreshold;
        }

        /// <summary>
        /// Check if a player should be kicked
        /// </summary>
        /// <param name="counts">Violation counts</param>
        /// <returns>True if player should be kicked</returns>
        private bool ShouldKickPlayer(CheatDetectionCount counts) {
            return counts.PositionJumpCount >= majorViolationThreshold ||
                   counts.SpeedHackCount >= majorViolationThreshold ||
                   counts.DamageHackCount >= majorViolationThreshold;
        }

        /// <summary>
        /// Flag a player for review
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="counts">Violation counts</param>
        private void FlagPlayerForReview(string playerId, CheatDetectionCount counts) {
            // Log for admin review
            Debug.LogWarning($"Player {playerId} flagged for potential cheating: " +
                             $"Position Jumps={counts.PositionJumpCount}, " +
                             $"Speed Hacks={counts.SpeedHackCount}, " +
                             $"Cooldown Violations={counts.CooldownViolationCount}, " +
                             $"Range Violations={counts.RangeViolationCount}, " +
                             $"Damage Hacks={counts.DamageHackCount}");

            // In a real implementation, this would log to a database or notify admins
        }

        /// <summary>
        /// Kick a player for cheating
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="counts">Violation counts</param>
        private void KickPlayer(string playerId, CheatDetectionCount counts) {
            Debug.LogWarning($"Player {playerId} kicked for cheating: " +
                             $"Position Jumps={counts.PositionJumpCount}, " +
                             $"Speed Hacks={counts.SpeedHackCount}, " +
                             $"Cooldown Violations={counts.CooldownViolationCount}, " +
                             $"Range Violations={counts.RangeViolationCount}, " +
                             $"Damage Hacks={counts.DamageHackCount}");

            // In a real implementation, this would disconnect the player
            // and potentially issue a temporary ban
        }

        /// <summary>
        /// Reset the violation counts for a player
        /// </summary>
        /// <param name="playerId">Player ID</param>
        public void ResetViolationCounts(string playerId) {
            if (cheatCounts.ContainsKey(playerId)) {
                cheatCounts.Remove(playerId);
            }
        }

        /// <summary>
        /// Get validation counts for a player
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <returns>Violation counts</returns>
        public CheatDetectionCount GetViolationCounts(string playerId) {
            if (cheatCounts.TryGetValue(playerId, out CheatDetectionCount counts)) {
                return counts;
            }

            return new CheatDetectionCount();
        }

        /// <summary>
        /// Ensure player state exists
        /// </summary>
        /// <param name="playerId">Player ID</param>
        private void EnsurePlayerState(string playerId) {
            if (!playerStates.ContainsKey(playerId)) {
                playerStates[playerId] = new PlayerValidationState();
            }
        }
    }

    /// <summary>
    /// Player validation state
    /// </summary>
    public class PlayerValidationState {
        public Vector3 LastPosition { get; set; } = Vector3.zero;
        public Vector3 LastRotation { get; set; } = Vector3.zero;
        public DateTime LastUpdateTime { get; set; } = DateTime.MinValue;
        public Dictionary<ActionType, DateTime> LastActionTimes { get; set; } = new Dictionary<ActionType, DateTime>();
        public ValidationResult LastValidationResult { get; set; } = ValidationResult.Valid;
    }

    /// <summary>
    /// Cheat detection count
    /// </summary>
    public class CheatDetectionCount {
        public int PositionJumpCount { get; set; }
        public int SpeedHackCount { get; set; }
        public int CooldownViolationCount { get; set; }
        public int RangeViolationCount { get; set; }
        public int DamageHackCount { get; set; }
    }

    /// <summary>
    /// Validation result enum
    /// </summary>
    public enum ValidationResult {
        Valid,
        PositionJump,
        SpeedHack,
        CooldownViolation,
        RangeViolation,
        DamageHack
    }

    /// <summary>
    /// Violation type enum
    /// </summary>
    public enum ViolationType {
        PositionJump,
        SpeedHack,
        CooldownViolation,
        RangeViolation,
        DamageHack
    }
}