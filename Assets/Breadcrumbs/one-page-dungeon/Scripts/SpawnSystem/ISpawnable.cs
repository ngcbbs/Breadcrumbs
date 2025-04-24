using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// Interface for spawnable objects
    /// </summary>
    public interface ISpawnable {
        /// <summary>
        /// Gets the GameObject associated with this spawnable
        /// </summary>
        GameObject SpawnableGameObject { get; }
        
        /// <summary>
        /// Called when the object is spawned
        /// </summary>
        /// <param name="spawnPoint">The spawn point that spawned this object</param>
        void OnSpawned(SpawnPoint spawnPoint);
        
        /// <summary>
        /// Called when the object is despawned
        /// </summary>
        void OnDespawned();
    }
}