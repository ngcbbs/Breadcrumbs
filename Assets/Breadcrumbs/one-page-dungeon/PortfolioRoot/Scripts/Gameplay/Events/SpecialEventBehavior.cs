using System.Collections;
using System.Collections.Generic;
using GamePortfolio.Gameplay.Character;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Events {
    /// <summary>
    /// Base class for special event behaviors
    /// </summary>
    public abstract class SpecialEventBehavior : MonoBehaviour {
        [Header("Event Settings")]
        [SerializeField]
        protected string eventName;
        [SerializeField]
        protected string eventDescription;
        [SerializeField]
        protected float eventDuration = 60f;
        [SerializeField]
        protected GameObject eventVFX;
        [SerializeField]
        protected AudioClip eventAmbientSound;

        // Event tracking
        protected SpecialEventData eventData;
        protected float eventStartTime;
        protected bool isEventActive = false;

        // Components
        protected AudioSource audioSource;

        protected virtual void Awake() {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && eventAmbientSound != null) {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = true;
                audioSource.spatialBlend = 1f;
            }
        }

        /// <summary>
        /// Initialize the event behavior with data
        /// </summary>
        public virtual void Initialize(SpecialEventData data) {
            eventData = data;
            eventName = data.displayName;
            eventDescription = data.description;
            eventDuration = data.duration;

            StartEvent();
        }

        /// <summary>
        /// Start the special event effects
        /// </summary>
        protected virtual void StartEvent() {
            isEventActive = true;
            eventStartTime = Time.time;

            // Start ambient sound
            if (audioSource != null && eventAmbientSound != null) {
                audioSource.clip = eventAmbientSound;
                audioSource.Play();
            }

            // Spawn VFX
            if (eventVFX != null) {
                Instantiate(eventVFX, transform.position, Quaternion.identity, transform);
            }
        }

        /// <summary>
        /// End the special event effects
        /// </summary>
        protected virtual void EndEvent() {
            isEventActive = false;

            // Stop ambient sound
            if (audioSource != null) {
                audioSource.Stop();
            }

            // Clean up - event object will be destroyed by the event system
        }

        protected virtual void Update() {
            // Check for event timeout
            if (isEventActive && Time.time >= eventStartTime + eventDuration) {
                EndEvent();
            }
        }

        /// <summary>
        /// Get remaining time of the event
        /// </summary>
        public float GetRemainingTime() {
            if (!isEventActive)
                return 0f;

            return Mathf.Max(0f, (eventStartTime + eventDuration) - Time.time);
        }

        /// <summary>
        /// Get progress percentage of the event (0-1)
        /// </summary>
        public float GetEventProgress() {
            if (!isEventActive)
                return 1f;

            return Mathf.Clamp01((Time.time - eventStartTime) / eventDuration);
        }
    }

    /// <summary>
    /// Healing shrine special event behavior
    /// </summary>
    public class HealingShrineEvent : SpecialEventBehavior {
        [Header("Healing Settings")]
        [SerializeField]
        private float healingRadius = 5f;
        [SerializeField]
        private float healingPerSecond = 10f;
        [SerializeField]
        private LayerMask targetLayers;
        [SerializeField]
        private GameObject healingParticles;

        private float lastHealTime = 0f;
        private float healInterval = 1f;

        protected override void Update() {
            base.Update();

            if (isEventActive && Time.time >= lastHealTime + healInterval) {
                HealNearbyPlayers();
                lastHealTime = Time.time;
            }
        }

        private void HealNearbyPlayers() {
            // Find players in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, healingRadius, targetLayers);

            foreach (var collider in colliders) {
                HealthSystem health = collider.GetComponent<HealthSystem>();
                if (health != null) {
                    // Apply healing
                    health.Heal(healingPerSecond * healInterval);

                    // Spawn healing particles
                    if (healingParticles != null) {
                        Instantiate(healingParticles, collider.transform.position + Vector3.up, Quaternion.identity);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, healingRadius);
        }
    }

    /// <summary>
    /// Horde battle special event behavior
    /// </summary>
    public class HordeBattleEvent : SpecialEventBehavior {
        [Header("Horde Settings")]
        [SerializeField]
        private GameObject[] enemyPrefabs;
        [SerializeField]
        private int maxEnemies = 10;
        [SerializeField]
        private float spawnInterval = 2f;
        [SerializeField]
        private float spawnRadius = 10f;
        [SerializeField]
        private LayerMask spawnObstacleLayers;

        private float nextSpawnTime = 0f;
        private List<GameObject> spawnedEnemies = new List<GameObject>();

        protected override void StartEvent() {
            base.StartEvent();

            nextSpawnTime = Time.time + 1f; // First spawn after a short delay
        }

        protected override void Update() {
            base.Update();

            if (isEventActive) {
                // Remove destroyed enemies from list
                spawnedEnemies.RemoveAll(e => e == null);

                // Spawn new enemies if needed
                if (Time.time >= nextSpawnTime && spawnedEnemies.Count < maxEnemies) {
                    SpawnEnemy();
                    nextSpawnTime = Time.time + spawnInterval;
                }
            }
        }

        private void SpawnEnemy() {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
                return;

            // Find a valid spawn position
            Vector3 spawnPosition = FindSpawnPosition();

            // Select random enemy prefab
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Spawn the enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            spawnedEnemies.Add(enemy);
        }

        private Vector3 FindSpawnPosition() {
            // Find player position
            Transform playerTransform = FindObjectOfType<PlayerController>()?.transform;

            if (playerTransform == null)
                return transform.position;

            // Try to find a valid position several times
            for (int i = 0; i < 10; i++) {
                // Random direction around player
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0;
                randomDirection.Normalize();

                // Calculate spawn position at spawn radius distance
                Vector3 spawnPosition = playerTransform.position + randomDirection * spawnRadius;

                // Ensure it's on navmesh
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas)) {
                    // Check line of sight to player (to avoid spawning directly visible)
                    if (!Physics.Linecast(hit.position, playerTransform.position, spawnObstacleLayers)) {
                        continue; // Try again if visible
                    }

                    return hit.position;
                }
            }

            // Fallback
            return transform.position;
        }

        protected override void EndEvent() {
            base.EndEvent();

            // Cleanup any remaining enemies if needed
            // For balance reasons, we often let enemies remain after the event ends
            // Uncomment to destroy all enemies when event ends
            /*
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            spawnedEnemies.Clear();
            */
        }
    }

    /// <summary>
    /// Treasure room special event behavior
    /// </summary>
    public class TreasureRoomEvent : SpecialEventBehavior {
        [Header("Treasure Settings")]
        [SerializeField]
        private GameObject[] treasurePrefabs;
        [SerializeField]
        private int minTreasures = 2;
        [SerializeField]
        private int maxTreasures = 5;
        [SerializeField]
        private float spawnRadius = 3f;

        private List<GameObject> spawnedTreasures = new List<GameObject>();

        protected override void StartEvent() {
            base.StartEvent();

            // Spawn treasures
            SpawnTreasures();
        }

        private void SpawnTreasures() {
            if (treasurePrefabs == null || treasurePrefabs.Length == 0)
                return;

            // Determine number of treasures to spawn
            int treasureCount = Random.Range(minTreasures, maxTreasures + 1);

            for (int i = 0; i < treasureCount; i++) {
                // Calculate spawn position
                float angle = i * (360f / treasureCount);
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Vector3 spawnPosition = transform.position + direction * (spawnRadius * 0.7f);

                // Ensure it's on navmesh
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 3f, UnityEngine.AI.NavMesh.AllAreas)) {
                    spawnPosition = hit.position;
                }

                // Select random treasure prefab
                GameObject treasurePrefab = treasurePrefabs[Random.Range(0, treasurePrefabs.Length)];

                // Spawn the treasure
                GameObject treasure = Instantiate(treasurePrefab, spawnPosition, Quaternion.identity);
                spawnedTreasures.Add(treasure);
            }
        }

        protected override void EndEvent() {
            base.EndEvent();

            // Do not destroy treasures when event ends
            // They remain for players to collect
        }
    }
}