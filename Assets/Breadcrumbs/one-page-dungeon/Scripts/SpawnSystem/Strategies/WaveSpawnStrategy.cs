using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem.Strategies
{
    /// <summary>
    /// Wave spawn strategy - spawns multiple objects in waves
    /// </summary>
    public class WaveSpawnStrategy : ISpawnStrategy
    {
        private int currentWave = 0;
        private int spawnedInCurrentWave = 0;
        
        private readonly int enemiesPerWave;
        private readonly float timeBetweenSpawns;
        private readonly float timeBetweenWaves;
        private readonly int maxWaves;
        
        /// <summary>
        /// Constructor for wave spawn strategy
        /// </summary>
        /// <param name="enemiesPerWave">Number of enemies per wave</param>
        /// <param name="timeBetweenSpawns">Time between individual spawns</param>
        /// <param name="timeBetweenWaves">Time between waves</param>
        /// <param name="maxWaves">Maximum number of waves (0 for infinite)</param>
        public WaveSpawnStrategy(int enemiesPerWave = 3, float timeBetweenSpawns = 0.5f, float timeBetweenWaves = 5f, int maxWaves = 3)
        {
            this.enemiesPerWave = enemiesPerWave;
            this.timeBetweenSpawns = timeBetweenSpawns;
            this.timeBetweenWaves = timeBetweenWaves;
            this.maxWaves = maxWaves;
        }
        
        /// <summary>
        /// Executes the wave spawn strategy - this will spawn the first enemy and start a coroutine for the rest
        /// </summary>
        public GameObject Execute(ISpawnPoint spawnPoint, GameObject prefab)
        {
            if (prefab == null || spawnPoint == null)
            {
                Debug.LogError("Cannot spawn null prefab or from null spawn point");
                return null;
            }
            
            // We need a MonoBehaviour to run the coroutine
            var spawnPointMono = spawnPoint as MonoBehaviour;
            if (spawnPointMono == null)
            {
                Debug.LogError("Wave spawn strategy requires a MonoBehaviour spawn point");
                return null;
            }
            
            // Don't start a new wave if we're already spawning or if we've reached the max waves
            if (spawnedInCurrentWave > 0 || (maxWaves > 0 && currentWave >= maxWaves))
            {
                return null;
            }
            
            // Start a new wave
            currentWave++;
            spawnedInCurrentWave = 0;
            
            // Start the coroutine to spawn the wave
            spawnPointMono.StartCoroutine(SpawnWave(spawnPoint, prefab));
            
            // Get the first enemy from the pool
            GameObject spawnedObj = ObjectPoolManager.Instance.GetObjectFromPool(prefab);
            
            if (spawnedObj == null)
            {
                Debug.LogWarning($"Failed to get object from pool for prefab: {prefab.name}");
                return null;
            }
            
            // Position and rotation
            spawnedObj.transform.position = spawnPoint.SpawnPosition;
            spawnedObj.transform.rotation = spawnPoint.SpawnRotation;
            
            spawnedInCurrentWave++;
            
            return spawnedObj;
        }
        
        /// <summary>
        /// Coroutine to spawn a wave of enemies
        /// </summary>
        private IEnumerator SpawnWave(ISpawnPoint spawnPoint, GameObject prefab)
        {
            // First enemy is already spawned in Execute
            
            // Spawn the rest of the wave
            for (int i = 1; i < enemiesPerWave; i++)
            {
                yield return new WaitForSeconds(timeBetweenSpawns);
                
                GameObject spawnedObj = ObjectPoolManager.Instance.GetObjectFromPool(prefab);
                
                if (spawnedObj != null)
                {
                    spawnedObj.transform.position = spawnPoint.SpawnPosition;
                    spawnedObj.transform.rotation = spawnPoint.SpawnRotation;
                    
                    // Notify the spawn point about the spawned object
                    if (spawnPoint is SpawnPoint sp)
                    {
                        sp.RegisterSpawnedObject(spawnedObj);
                    }
                    
                    spawnedInCurrentWave++;
                }
            }
            
            // If we have more waves to spawn, wait and then reset
            if (maxWaves == 0 || currentWave < maxWaves)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
                spawnedInCurrentWave = 0; // This will allow next execute to start a new wave
            }
        }
    }
}
