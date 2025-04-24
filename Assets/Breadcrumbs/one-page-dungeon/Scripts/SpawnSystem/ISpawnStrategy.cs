using UnityEngine;

namespace Breadcrumbs.SpawnSystem
{
    /// <summary>
    /// Defines different spawn strategy types
    /// </summary>
    public enum SpawnStrategyType
    {
        Default,
        Wave,
        Conditional,
        RandomSelection,
        FixedInterval
    }
    
    /// <summary>
    /// Interface for spawn strategies
    /// </summary>
    public interface ISpawnStrategy
    {
        /// <summary>
        /// Executes the spawn strategy
        /// </summary>
        /// <param name="spawnPoint">The spawn point</param>
        /// <param name="prefab">The prefab to spawn</param>
        /// <returns>The spawned game object, or null if the spawn failed</returns>
        GameObject Execute(ISpawnPoint spawnPoint, GameObject prefab);
    }
}
