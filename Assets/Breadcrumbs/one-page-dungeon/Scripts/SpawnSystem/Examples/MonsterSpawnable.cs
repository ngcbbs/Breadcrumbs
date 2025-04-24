using Breadcrumbs.EventSystem;
using Breadcrumbs.SpawnSystem.Events;
using UnityEngine;
using UnityEngine.AI;

namespace Breadcrumbs.SpawnSystem.Examples {
    /// <summary>
    /// Example implementation of ISpawnable for monster entities
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterSpawnable : EventBehaviour, ISpawnable {
        [Header("Spawnable Settings")]
        [SerializeField]
        private float health = 100f;
        [SerializeField]
        private float despawnDelay = 5f;
        [SerializeField]
        private GameObject deathEffect;

        // References
        private NavMeshAgent _agent;
        private SpawnPoint _spawnPoint;
        private Animator _animator;

        // Properties
        public GameObject SpawnableGameObject => gameObject;
        public bool IsAlive { get; private set; } = true;

        private SpawnManager _spawnManager;

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        private void Start() {
            _spawnManager = FindAnyObjectByType<SpawnManager>();
        }

        protected override void RegisterEventHandlers() { }

        /// <summary>
        /// Called when this monster is spawned
        /// </summary>
        public void OnSpawned(SpawnPoint spawnPoint) {
            _spawnPoint = spawnPoint;
            IsAlive = true;
            health = 100f; // Reset health

            // Enable components
            if (_agent != null) {
                _agent.enabled = true;
            }

            // Play spawn animation if available
            if (_animator != null) {
                _animator.SetTrigger("Spawn");
            }

            Debug.Log($"Monster spawned at {transform.position}");
        }

        /// <summary>
        /// Called when this monster is despawned
        /// </summary>
        public void OnDespawned() {
            IsAlive = false;

            // Disable components
            if (_agent != null) {
                _agent.enabled = false;
            }

            Debug.Log($"Monster despawned from {transform.position}");
        }

        /// <summary>
        /// Apply damage to this monster
        /// </summary>
        public void TakeDamage(float damage) {
            if (!IsAlive) return;

            health -= damage;

            // Play hit animation/effect
            if (_animator != null) {
                _animator.SetTrigger("Hit");
            }

            // Check if monster is dead
            if (health <= 0) {
                Die();
            }
        }

        /// <summary>
        /// Handle monster death
        /// </summary>
        private void Die() {
            if (!IsAlive) return;

            IsAlive = false;

            // Play death animation
            if (_animator != null) {
                _animator.SetTrigger("Death");
            }

            // Spawn death effect
            if (deathEffect != null) {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }

            // Stop movement
            if (_agent != null) {
                _agent.isStopped = true;
                _agent.enabled = false;
            }

            // Disable colliders
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders) {
                collider.enabled = false;
            }

            // Disable any AI scripts
            var aiComponents = GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in aiComponents) {
                if (component != this && component.GetType().Name.Contains("AI")) {
                    component.enabled = false;
                }
            }

            // Schedule despawn
            //Invoke("RequestDespawn", despawnDelay);
            Dispatch(new DespawnEvent(gameObject));
        }

        /// <summary>
        /// Request despawn from spawn manager
        /// </summary>
        private void RequestDespawn() {
            Dispatch(new DespawnEvent(gameObject));
        }
    }
}