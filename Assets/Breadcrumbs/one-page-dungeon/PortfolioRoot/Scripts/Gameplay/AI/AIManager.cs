using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Core;

namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// Manager class for all AI entities in the game
    /// Handles optimization and coordination between AI agents
    /// </summary>
    public class AIManager : Singleton<AIManager> {
        [Header("Performance Settings")]
        [SerializeField]
        private float aiUpdateInterval = 0.2f;
        [SerializeField]
        private float maxDistanceForFullAI = 20f;
        [SerializeField]
        private float maxDistanceForReducedAI = 40f;
        [SerializeField]
        private int maxConcurrentActiveAI = 10;

        [Header("AI Group Behaviors")]
        [SerializeField]
        private bool enableGroupAwareness = true;
        [SerializeField]
        private float awarenessRadius = 10f;

        // List of all registered AI entities
        private List<BaseEnemyAI> registeredEnemies = new List<BaseEnemyAI>();

        // Dictionary to track the priority of each AI entity
        private Dictionary<BaseEnemyAI, float> aiPriorities = new Dictionary<BaseEnemyAI, float>();

        // Timer for staggered updates
        private float updateTimer = 0f;

        // Reference to player for distance calculations
        private Transform playerTransform;

        protected override void Awake() {
            base.Awake();

            // Get player reference
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                playerTransform = player.transform;
            }
        }

        private void Update() {
            updateTimer += Time.deltaTime;

            if (updateTimer >= aiUpdateInterval) {
                UpdateAIPriorities();
                updateTimer = 0f;
            }
        }

        /// <summary>
        /// Register an enemy AI with the manager
        /// </summary>
        public void RegisterEnemy(BaseEnemyAI enemy) {
            if (!registeredEnemies.Contains(enemy)) {
                registeredEnemies.Add(enemy);
                aiPriorities[enemy] = 0f;

                Debug.Log($"AI Manager: Registered enemy {enemy.gameObject.name}");
            }
        }

        /// <summary>
        /// Unregister an enemy AI from the manager
        /// </summary>
        public void UnregisterEnemy(BaseEnemyAI enemy) {
            if (registeredEnemies.Contains(enemy)) {
                registeredEnemies.Remove(enemy);

                if (aiPriorities.ContainsKey(enemy)) {
                    aiPriorities.Remove(enemy);
                }

                Debug.Log($"AI Manager: Unregistered enemy {enemy.gameObject.name}");
            }
        }

        /// <summary>
        /// Update AI priorities based on distance and other factors
        /// </summary>
        private void UpdateAIPriorities() {
            if (playerTransform == null || registeredEnemies.Count == 0)
                return;

            // Calculate priorities
            foreach (var enemy in registeredEnemies) {
                if (enemy == null) continue;

                float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
                float statePriority = CalculateStatePriority(enemy);
                float visibilityFactor = IsEnemyVisible(enemy) ? 2f : 1f;

                // Combined priority formula
                float priority = (100f / (distance + 1f)) * statePriority * visibilityFactor;

                aiPriorities[enemy] = priority;

                // Disable AI that are too far away
                if (distance > maxDistanceForReducedAI) {
                    enemy.enabled = false;
                } else {
                    enemy.enabled = true;
                }
            }

            // Sort enemies by priority
            registeredEnemies.Sort((a, b) => aiPriorities[b].CompareTo(aiPriorities[a]));

            // Limit active AI count
            for (int i = 0; i < registeredEnemies.Count; i++) {
                BaseEnemyAI enemy = registeredEnemies[i];

                if (i < maxConcurrentActiveAI) {
                    float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);

                    // Set full or reduced AI mode based on distance
                    enemy.enabled = true;

                    // Handle group awareness
                    if (enableGroupAwareness && enemy.IsTargetVisible) {
                        AlertNearbyEnemies(enemy);
                    }
                } else if (enemy.enabled) {
                    enemy.enabled = false;
                }
            }
        }

        /// <summary>
        /// Calculate priority based on AI state
        /// </summary>
        private float CalculateStatePriority(BaseEnemyAI enemy) {
            switch (enemy.GetCurrentStateType()) {
                case EnemyStateType.Attack:
                    return 5f;
                case EnemyStateType.Chase:
                    return 4f;
                case EnemyStateType.Retreat:
                    return 3f;
                case EnemyStateType.MaintainDistance:
                    return 2.5f;
                case EnemyStateType.Patrol:
                    return 1.5f;
                case EnemyStateType.Idle:
                default:
                    return 1f;
            }
        }

        /// <summary>
        /// Check if an enemy is visible to the player
        /// </summary>
        private bool IsEnemyVisible(BaseEnemyAI enemy) {
            // Simple implementation - can be enhanced with actual visibility checks
            float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
            return distance < maxDistanceForFullAI;
        }

        /// <summary>
        /// Alert nearby enemies when one enemy spots the player
        /// </summary>
        private void AlertNearbyEnemies(BaseEnemyAI alerter) {
            foreach (var enemy in registeredEnemies) {
                if (enemy == alerter || enemy == null)
                    continue;

                float distance = Vector3.Distance(enemy.transform.position, alerter.transform.position);

                if (distance <= awarenessRadius && enemy.GetCurrentStateType() == EnemyStateType.Idle ||
                    enemy.GetCurrentStateType() == EnemyStateType.Patrol) {
                    // Change state to chase
                    enemy.ChangeState(EnemyStateType.Chase);
                }
            }
        }

        /// <summary>
        /// Find the nearest enemy to a position
        /// </summary>
        public BaseEnemyAI GetNearestEnemy(Vector3 position, float maxDistance = float.MaxValue) {
            BaseEnemyAI nearest = null;
            float nearestDistance = maxDistance;

            foreach (var enemy in registeredEnemies) {
                if (enemy == null)
                    continue;

                float distance = Vector3.Distance(position, enemy.transform.position);

                if (distance < nearestDistance) {
                    nearest = enemy;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Get all enemies within a radius
        /// </summary>
        public List<BaseEnemyAI> GetEnemiesInRadius(Vector3 position, float radius) {
            List<BaseEnemyAI> enemiesInRadius = new List<BaseEnemyAI>();

            foreach (var enemy in registeredEnemies) {
                if (enemy == null)
                    continue;

                float distance = Vector3.Distance(position, enemy.transform.position);

                if (distance <= radius) {
                    enemiesInRadius.Add(enemy);
                }
            }

            return enemiesInRadius;
        }

        /// <summary>
        /// Draw gizmos for debugging
        /// </summary>
        private void OnDrawGizmosSelected() {
            if (playerTransform == null)
                return;

            // Draw max AI distances
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, maxDistanceForFullAI);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, maxDistanceForReducedAI);
        }
    }
}