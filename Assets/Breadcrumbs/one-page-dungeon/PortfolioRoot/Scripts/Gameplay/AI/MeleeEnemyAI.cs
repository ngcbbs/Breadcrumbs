using UnityEngine;

namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// AI controller for melee enemies
    /// </summary>
    public class MeleeEnemyAI : BaseEnemyAI {
        [Header("Melee-Specific Settings")]
        [SerializeField]
        private float chargeDistance = 3f;
        [SerializeField]
        private float chargeCooldown = 5f;
        [SerializeField]
        private float chargeSpeed = 8f;

        // Charge attack variables
        private float lastChargeTime;
        private bool isCharging;

        protected override void Awake() {
            base.Awake();

            // Register with AI Manager
            AIManager.Instance.RegisterEnemy(this);
        }

        protected override void SetupStates() {
            base.SetupStates();

            // Add melee-specific states or override existing ones
            availableStates[EnemyStateType.Attack] = new MeleeAttackState(this);
        }

        /// <summary>
        /// Perform a charge attack
        /// </summary>
        public void PerformChargeAttack() {
            if (Time.time < lastChargeTime + chargeCooldown || target == null)
                return;

            isCharging = true;
            lastChargeTime = Time.time;

            // Store original speed and set charge speed
            float originalSpeed = navAgent.speed;
            navAgent.speed = chargeSpeed;

            // Set destination to target position plus a bit further to ensure momentum
            Vector3 chargeDirection = (target.position - transform.position).normalized;
            Vector3 chargeDestination = target.position + chargeDirection * 2f;
            MoveToPosition(chargeDestination);

            // Reset speed after delay
            Invoke("EndChargeAttack", 1.5f);
        }

        /// <summary>
        /// End the charge attack
        /// </summary>
        private void EndChargeAttack() {
            isCharging = false;
            navAgent.speed = navAgent.speed / 2f; // Return to normal speed
        }

        /// <summary>
        /// Check if charge attack is available
        /// </summary>
        public bool CanChargeAttack() {
            return Time.time >= lastChargeTime + chargeCooldown;
        }

        /// <summary>
        /// Enhanced melee attack state with charge attack
        /// </summary>
        private class MeleeAttackState : AttackState {
            private MeleeEnemyAI meleeOwner;
            private bool chargeDecided;

            public MeleeAttackState(BaseEnemyAI owner) : base(owner) {
                this.meleeOwner = owner as MeleeEnemyAI;
            }

            public override void EnterState() {
                base.EnterState();
                chargeDecided = false;
            }

            public override void UpdateState() {
                if (!chargeDecided && meleeOwner != null && meleeOwner.target != null) {
                    float distanceToTarget = Vector3.Distance(meleeOwner.transform.position, meleeOwner.target.position);

                    // Decide whether to use charge attack
                    if (distanceToTarget > meleeOwner.chargeDistance && meleeOwner.CanChargeAttack()) {
                        meleeOwner.PerformChargeAttack();
                        chargeDecided = true;
                    } else {
                        base.UpdateState();
                    }
                } else {
                    base.UpdateState();
                }
            }
        }

        private void OnDestroy() {
            // Unregister from AI Manager
            if (AIManager.Instance != null) {
                AIManager.Instance.UnregisterEnemy(this);
            }
        }
    }
}