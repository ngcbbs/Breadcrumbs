using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem.Strategies
{
    /// <summary>
    /// Random selection spawn strategy - selects a random prefab from a list to spawn
    /// </summary>
    public class RandomSelectionStrategy : ISpawnStrategy
    {
        private readonly List<GameObject> prefabOptions;
        private readonly List<float> weights;
        private readonly ISpawnStrategy innerStrategy;
        private float totalWeight;
        
        /// <summary>
        /// Constructor for random selection strategy
        /// </summary>
        /// <param name="prefabOptions">List of prefab options</param>
        /// <param name="weights">Optional weights for each prefab (null for equal weight)</param>
        /// <param name="innerStrategy">Strategy to use for actual spawning (defaults to DefaultSpawnStrategy)</param>
        public RandomSelectionStrategy(List<GameObject> prefabOptions, List<float> weights = null, ISpawnStrategy innerStrategy = null)
        {
            this.prefabOptions = prefabOptions;
            this.weights = weights;
            this.innerStrategy = innerStrategy ?? new Strategies.DefaultSpawnStrategy();
            
            // Calculate total weight
            if (weights != null && weights.Count == prefabOptions.Count)
            {
                totalWeight = 0f;
                for (int i = 0; i < weights.Count; i++)
                {
                    totalWeight += weights[i];
                }
            }
        }
        
        /// <summary>
        /// Executes the random selection strategy - picks a random prefab and then uses the inner strategy to spawn it
        /// </summary>
        public GameObject Execute(ISpawnPoint spawnPoint, GameObject prefab)
        {
            if (prefabOptions == null || prefabOptions.Count == 0 || spawnPoint == null)
            {
                Debug.LogError("Cannot spawn with empty prefab options or null spawn point");
                return null;
            }
            
            // Select a random prefab from the options
            GameObject selectedPrefab;
            
            if (weights != null && weights.Count == prefabOptions.Count)
            {
                // Weighted selection
                float randomValue = Random.Range(0f, totalWeight);
                float currentSum = 0f;
                
                selectedPrefab = prefabOptions[0]; // Default in case something goes wrong
                
                for (int i = 0; i < prefabOptions.Count; i++)
                {
                    currentSum += weights[i];
                    if (randomValue <= currentSum)
                    {
                        selectedPrefab = prefabOptions[i];
                        break;
                    }
                }
            }
            else
            {
                // Equal probability selection
                int randomIndex = Random.Range(0, prefabOptions.Count);
                selectedPrefab = prefabOptions[randomIndex];
            }
            
            // Use the inner strategy to do the actual spawning
            return innerStrategy.Execute(spawnPoint, selectedPrefab);
        }
    }
}
