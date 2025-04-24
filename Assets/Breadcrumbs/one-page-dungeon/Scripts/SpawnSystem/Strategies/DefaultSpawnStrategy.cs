using UnityEngine;

namespace Breadcrumbs.SpawnSystem.Strategies
{
    /// <summary>
    /// Default implementation of the spawn strategy
    /// </summary>
    public class DefaultSpawnStrategy : ISpawnStrategy
    {
        /// <summary>
        /// Executes the default spawn strategy - simply spawns the object at the spawn point
        /// </summary>
        public GameObject Execute(ISpawnPoint spawnPoint, GameObject prefab)
        {
            if (prefab == null || spawnPoint == null)
            {
                Debug.LogError("Cannot spawn null prefab or from null spawn point");
                return null;
            }
            
            // Get an object from the pool
            GameObject spawnedObj = ObjectPoolManager.Instance.GetObjectFromPool(prefab);
            
            if (spawnedObj == null)
            {
                Debug.LogWarning($"Failed to get object from pool for prefab: {prefab.name}");
                return null;
            }
            
            // Position and rotation
            spawnedObj.transform.position = spawnPoint.SpawnPosition;
            spawnedObj.transform.rotation = spawnPoint.SpawnRotation;
            
            return spawnedObj;
        }
    }
}
