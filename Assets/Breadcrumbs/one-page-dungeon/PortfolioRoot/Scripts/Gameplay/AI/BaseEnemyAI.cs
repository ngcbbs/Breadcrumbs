using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// Base class for all enemy AI controllers implementing a state machine pattern
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Collider))]
    public abstract class BaseEnemyAI : MonoBehaviour {
        [Header("Detection Settings")]
        [SerializeField]
        protected float detectionRadius = 8f;
        [SerializeField]
        protected float fieldOfViewAngle = 90f;
        [SerializeField]
        protected Transform eyePosition;
        [SerializeField]
        protected LayerMask playerLayer;
        [SerializeField]
        protected LayerMask obstacleLayer;

        [Header("Combat Settings")]
        [SerializeField]
        protected float attackRange = 1.5f;
        [SerializeField]
        protected float attackCooldown = 2f;

        [Header("Movement Settings")]
        [SerializeField]
        protected float patrolRadius = 5f;
        [SerializeField]
        protected float patrolWaitTime = 2f;

        // Components
        protected NavMeshAgent navAgent;
        protected Animator animator;
        protected EnemyCombat combat;

        // State machine
        protected IEnemyState currentState;
        protected Dictionary<EnemyStateType, IEnemyState> availableStates;

        // Target tracking
        protected Transform target;
        protected bool isTargetVisible;
        protected float lastAttackTime;
        protected Vector3 lastKnownTargetPosition;

        // Patrol variables
        protected Vector3 initialPosition;
        protected Vector3 currentPatrolDestination;

        // getter
        public bool IsTargetVisible => isTargetVisible;
        public Transform Target => target;
        public float PatrolWaitTime {
            get => patrolWaitTime;
            set => patrolWaitTime = value;
        }
        public Vector3 LastKnownTargetPosition {
            get => lastKnownTargetPosition;
            set => lastKnownTargetPosition = value;
        }
        public Vector3 InitialPosition {
            get => initialPosition;
            set => initialPosition = value;
        }
        public Vector3 CurrentPatrolDestination {
            get => currentPatrolDestination;
            set => currentPatrolDestination = value;
        }

        /// <summary>
        /// Initialize the enemy AI components and state machine
        /// </summary>
        protected virtual void Awake() {
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            combat = GetComponent<EnemyCombat>();

            if (eyePosition == null)
                eyePosition = transform;

            initialPosition = transform.position;

            // Initialize state machine
            availableStates = new Dictionary<EnemyStateType, IEnemyState>();
            SetupStates();
            ChangeState(EnemyStateType.Idle);
        }

        /// <summary>
        /// Setup all available states for this enemy
        /// </summary>
        protected virtual void SetupStates() {
            // Override in derived classes to add specific states
            availableStates.Add(EnemyStateType.Idle, new IdleState(this));
            availableStates.Add(EnemyStateType.Patrol, new PatrolState(this));
            availableStates.Add(EnemyStateType.Chase, new ChaseState(this));
            availableStates.Add(EnemyStateType.Attack, new AttackState(this));
            availableStates.Add(EnemyStateType.Retreat, new RetreatState(this));
        }

        /// <summary>
        /// Update the current state and check for transitions
        /// </summary>
        protected virtual void Update() {
            if (currentState == null)
                return;

            // Update target detection
            DetectTarget();

            // Update current state
            currentState.UpdateState();

            // Check for state transitions
            var nextState = currentState.CheckTransitions();
            if (nextState.HasValue && nextState.Value != GetCurrentStateType()) {
                ChangeState(nextState.Value);
            }
        }

        /// <summary>
        /// Change the current state to a new state
        /// </summary>
        public void ChangeState(EnemyStateType newStateType) {
            if (currentState != null)
                currentState.ExitState();

            if (availableStates.TryGetValue(newStateType, out IEnemyState newState)) {
                currentState = newState;
                currentState.EnterState();

                // Log state change (can be removed in production)
                Debug.Log($"{gameObject.name} changed state to {newStateType}");
            } else {
                Debug.LogError($"State {newStateType} not found for {gameObject.name}");
            }
        }

        /// <summary>
        /// Get the current state type
        /// </summary>
        public EnemyStateType GetCurrentStateType() {
            foreach (var state in availableStates) {
                if (state.Value == currentState)
                    return state.Key;
            }

            return EnemyStateType.Idle; // Default
        }

        /// <summary>
        /// Detect potential targets within range and line of sight
        /// </summary>
        protected virtual void DetectTarget() {
            // Check for players in detection radius
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

            isTargetVisible = false;

            foreach (var hitCollider in hitColliders) {
                Transform potentialTarget = hitCollider.transform;

                // Check if target is in field of view
                if (IsInFieldOfView(potentialTarget)) {
                    // Check if there's a clear line of sight
                    if (HasLineOfSight(potentialTarget)) {
                        target = potentialTarget;
                        lastKnownTargetPosition = target.position;
                        isTargetVisible = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Check if a target is within the field of view angle
        /// </summary>
        protected bool IsInFieldOfView(Transform checkTarget) {
            Vector3 directionToTarget = (checkTarget.position - eyePosition.position).normalized;
            float angle = Vector3.Angle(eyePosition.forward, directionToTarget);

            return angle <= fieldOfViewAngle * 0.5f;
        }

        /// <summary>
        /// Check if there's a clear line of sight to the target
        /// </summary>
        protected bool HasLineOfSight(Transform checkTarget) {
            Vector3 directionToTarget = checkTarget.position - eyePosition.position;

            // Cast a ray to check for obstacles
            if (Physics.Raycast(eyePosition.position, directionToTarget.normalized,
                    out RaycastHit hit, directionToTarget.magnitude, obstacleLayer)) {
                // Hit something that's not the target
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the target is within attack range
        /// </summary>
        public bool IsTargetInAttackRange() {
            if (target == null)
                return false;

            float distance = Vector3.Distance(transform.position, target.position);
            return distance <= attackRange;
        }

        /// <summary>
        /// Check if enough time has passed to attack again
        /// </summary>
        public bool CanAttack() {
            return Time.time >= lastAttackTime + attackCooldown;
        }

        /// <summary>
        /// Perform an attack action
        /// </summary>
        public virtual void PerformAttack() {
            if (combat != null && target != null) {
                combat.Attack(target);
                lastAttackTime = Time.time;
            }
        }

        /// <summary>
        /// Generate a random patrol point around the initial position
        /// </summary>
        public Vector3 GetRandomPatrolPoint() {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += initialPosition;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas)) {
                return hit.position;
            }

            return initialPosition;
        }

        /// <summary>
        /// Move to a specified position using NavMeshAgent
        /// </summary>
        public void MoveToPosition(Vector3 position) {
            navAgent.SetDestination(position);
        }

        /// <summary>
        /// Stop the NavMeshAgent movement
        /// </summary>
        public void StopMovement() {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }

        /// <summary>
        /// Resume the NavMeshAgent movement
        /// </summary>
        public void ResumeMovement() {
            navAgent.isStopped = false;
        }

        /// <summary>
        /// Check if the NavMeshAgent has reached its destination
        /// </summary>
        public bool HasReachedDestination() {
            return !navAgent.pathPending &&
                   navAgent.remainingDistance <= navAgent.stoppingDistance &&
                   (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f);
        }

        /// <summary>
        /// Draw gizmos for debugging
        /// </summary>
        protected virtual void OnDrawGizmosSelected() {
            // Draw detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw field of view
            if (eyePosition != null) {
                Gizmos.color = Color.blue;
                Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle * 0.5f, Vector3.up) * eyePosition.forward;
                Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle * 0.5f, Vector3.up) * eyePosition.forward;

                Gizmos.DrawRay(eyePosition.position, fovLine1 * detectionRadius);
                Gizmos.DrawRay(eyePosition.position, fovLine2 * detectionRadius);
            }
        }
    }
}