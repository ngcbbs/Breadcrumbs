using UnityEngine;

namespace Breadcrumbs.SpawnSystem
{
    /// <summary>
    /// Interface for spawn points
    /// </summary>
    public interface ISpawnPoint
    {
        /// <summary>
        /// Returns whether the spawn point is active
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Returns the spawn position
        /// </summary>
        Vector3 SpawnPosition { get; }
        
        /// <summary>
        /// Returns the spawn rotation
        /// </summary>
        Quaternion SpawnRotation { get; }
        
        /// <summary>
        /// Gets the difficulty requirement for this spawn point
        /// </summary>
        DifficultyLevel MinimumDifficulty { get; }
        
        /// <summary>
        /// Triggers a spawn at this spawn point
        /// </summary>
        /// <returns>The spawned game object, or null if the spawn failed</returns>
        GameObject TriggerSpawn();
        
        /// <summary>
        /// Handles an object being despawned that was spawned from this point
        /// </summary>
        /// <param name="obj">The object being despawned</param>
        void HandleObjectDespawn(GameObject obj);
        
        /// <summary>
        /// Checks if a position is in the trigger area of this spawn point
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if the position is in the trigger area</returns>
        bool IsInTriggerArea(Vector3 position);
        
        /// <summary>
        /// Checks if this spawn point meets the difficulty requirement
        /// </summary>
        /// <param name="currentDifficulty">The current difficulty level</param>
        /// <returns>True if the difficulty requirement is met</returns>
        bool MeetsDifficultyRequirement(DifficultyLevel currentDifficulty);
        
        /// <summary>
        /// Called when an event is triggered that this spawn point might be interested in
        /// </summary>
        /// <param name="eventKey">The event key</param>
        void OnEventTriggered(string eventKey);
    }
}
