using UnityEngine;

namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// Base class for all enemy states to avoid code duplication
    /// </summary>
    public abstract class BaseEnemyState : IEnemyState {
        protected BaseEnemyAI owner;

        public BaseEnemyState(BaseEnemyAI owner) {
            this.owner = owner;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract EnemyStateType? CheckTransitions();
    }

    /// <summary>
    /// Idle state for the enemy
    /// </summary>
    public class IdleState : BaseEnemyState {
        private float idleTimer;
        private float idleDuration;

        public IdleState(BaseEnemyAI owner) : base(owner) { }

        public override void EnterState() {
            owner.StopMovement();
            idleTimer = 0f;
            idleDuration = Random.Range(2f, 4f);
        }

        public override void UpdateState() {
            idleTimer += Time.deltaTime;
        }

        public override void ExitState() {
            owner.ResumeMovement();
        }

        public override EnemyStateType? CheckTransitions() {
            // If the target is visible, chase it
            if (owner.IsTargetVisible) {
                return EnemyStateType.Chase;
            }

            // After idle duration, start patrolling
            if (idleTimer >= idleDuration) {
                return EnemyStateType.Patrol;
            }

            return null;
        }
    }

    /// <summary>
    /// Patrol state for the enemy
    /// </summary>
    public class PatrolState : BaseEnemyState {
        private float patrolWaitTimer;
        private bool isWaiting;

        public PatrolState(BaseEnemyAI owner) : base(owner) { }

        public override void EnterState() {
            // Set a random patrol destination
            owner.CurrentPatrolDestination = owner.GetRandomPatrolPoint();
            owner.MoveToPosition(owner.CurrentPatrolDestination);
            isWaiting = false;
            patrolWaitTimer = 0f;
        }

        public override void UpdateState() {
            if (isWaiting) {
                patrolWaitTimer += Time.deltaTime;

                // Resume patrolling after wait time
                if (patrolWaitTimer >= owner.PatrolWaitTime) {
                    isWaiting = false;
                    owner.CurrentPatrolDestination = owner.GetRandomPatrolPoint();
                    owner.MoveToPosition(owner.CurrentPatrolDestination);
                }
            } else if (owner.HasReachedDestination()) {
                // Start waiting at patrol point
                isWaiting = true;
                patrolWaitTimer = 0f;
                owner.StopMovement();
            }
        }

        public override void ExitState() { }

        public override EnemyStateType? CheckTransitions() {
            // If the target is visible, chase it
            if (owner.IsTargetVisible) {
                return EnemyStateType.Chase;
            }

            return null;
        }
    }

    /// <summary>
    /// Chase state for the enemy
    /// </summary>
    public class ChaseState : BaseEnemyState {
        private float targetLostTimer;
        private const float targetLostThreshold = 5f;

        public ChaseState(BaseEnemyAI owner) : base(owner) { }

        public override void EnterState() {
            targetLostTimer = 0f;
        }

        public override void UpdateState() {
            if (owner.IsTargetVisible) {
                // Update destination to follow target
                owner.MoveToPosition(owner.LastKnownTargetPosition);
                targetLostTimer = 0f;
            } else {
                // Target lost, keep moving to last known position
                targetLostTimer += Time.deltaTime;
            }
        }

        public override void ExitState() { }

        public override EnemyStateType? CheckTransitions() {
            // If in attack range, switch to attack
            if (owner.IsTargetInAttackRange() && owner.CanAttack()) {
                return EnemyStateType.Attack;
            }

            // If target is lost for too long, go back to patrolling
            if (targetLostTimer >= targetLostThreshold) {
                return EnemyStateType.Patrol;
            }

            return null;
        }
    }

    /// <summary>
    /// Attack state for the enemy
    /// </summary>
    public class AttackState : BaseEnemyState {
        private bool hasAttacked;

        public AttackState(BaseEnemyAI owner) : base(owner) { }

        public override void EnterState() {
            owner.StopMovement();
            hasAttacked = false;

            // Rotate towards target
            if (owner.Target != null) {
                Vector3 lookDirection = owner.Target.position - owner.transform.position;
                lookDirection.y = 0;
                owner.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        public override void UpdateState() {
            if (!hasAttacked && owner.CanAttack()) {
                owner.PerformAttack();
                hasAttacked = true;
            }
        }

        public override void ExitState() {
            owner.ResumeMovement();
        }

        public override EnemyStateType? CheckTransitions() {
            // After attacking, immediately transition back to chase
            if (hasAttacked) {
                return EnemyStateType.Chase;
            }

            // If target moved out of range, go back to chase
            if (!owner.IsTargetInAttackRange()) {
                return EnemyStateType.Chase;
            }

            return null;
        }
    }

    /// <summary>
    /// Retreat state for the enemy
    /// </summary>
    public class RetreatState : BaseEnemyState {
        private float retreatTimer;
        private const float retreatDuration = 3f;
        private Vector3 retreatDirection;

        public RetreatState(BaseEnemyAI owner) : base(owner) { }

        public override void EnterState() {
            retreatTimer = 0f;

            // Calculate retreat direction away from target
            if (owner.Target != null) {
                retreatDirection = owner.transform.position - owner.Target.position;
                retreatDirection.y = 0;
                retreatDirection.Normalize();

                // Calculate retreat position
                Vector3 retreatPosition = owner.transform.position + retreatDirection * 8f;
                owner.MoveToPosition(retreatPosition);
            } else {
                // Retreat towards initial position if no target
                owner.MoveToPosition(owner.InitialPosition);
            }
        }

        public override void UpdateState() {
            retreatTimer += Time.deltaTime;
        }

        public override void ExitState() { }

        public override EnemyStateType? CheckTransitions() {
            // After retreat duration or reaching destination, go back to idle
            if (retreatTimer >= retreatDuration || owner.HasReachedDestination()) {
                return EnemyStateType.Idle;
            }

            return null;
        }
    }

    /// <summary>
    /// MaintainDistance state for ranged enemies
    /// </summary>
    public class MaintainDistanceState : BaseEnemyState {
        private float optimalDistance;
        private float movementTimer;
        private const float repositionInterval = 1.5f;

        public MaintainDistanceState(BaseEnemyAI owner, float optimalDistance = 5f) : base(owner) {
            this.optimalDistance = optimalDistance;
        }

        public override void EnterState() {
            movementTimer = 0f;
        }

        public override void UpdateState() {
            movementTimer += Time.deltaTime;

            if (owner.Target != null && movementTimer >= repositionInterval) {
                // Calculate current distance to target
                float currentDistance = Vector3.Distance(owner.transform.position, owner.Target.position);

                if (Mathf.Abs(currentDistance - optimalDistance) > 1f) {
                    Vector3 directionToTarget = (owner.Target.position - owner.transform.position).normalized;
                    Vector3 targetPosition;

                    if (currentDistance < optimalDistance) {
                        // Too close, move away
                        targetPosition = owner.transform.position - directionToTarget * (optimalDistance - currentDistance);
                    } else {
                        // Too far, move closer
                        targetPosition = owner.transform.position + directionToTarget * (currentDistance - optimalDistance);
                    }

                    owner.MoveToPosition(targetPosition);
                    movementTimer = 0f;
                }
            }
        }

        public override void ExitState() { }

        public override EnemyStateType? CheckTransitions() {
            // If target is too close, retreat
            if (owner.IsTargetInAttackRange()) {
                return EnemyStateType.Retreat;
            }

            // If can attack and in good position, attack
            if (owner.CanAttack() && owner.IsTargetVisible) {
                return EnemyStateType.Attack;
            }

            // If target is lost, go back to patrolling
            if (!owner.IsTargetVisible) {
                return EnemyStateType.Patrol;
            }

            return null;
        }
    }
}