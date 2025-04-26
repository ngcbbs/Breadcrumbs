using UnityEngine;

namespace GamePortfolio.Gameplay.AI.Boss {
    /// <summary>
    /// State for boss area attack
    /// </summary>
    public class BossAreaAttackState : IEnemyState {
        private BossEnemyAI boss;

        public BossAreaAttackState(BossEnemyAI boss) {
            this.boss = boss;
        }

        public void EnterState() {
            // Stop movement
            boss.StopMovement();

            // Perform area attack
            boss.PerformAreaAttack();
        }

        public void UpdateState() {
            // Nothing to do during update, the attack coroutine handles everything
        }

        public void ExitState() {
            // Resume movement
            boss.ResumeMovement();
        }

        public EnemyStateType? CheckTransitions() {
            // Once area attack is complete, transition back to chase or attack
            if (!boss.IsAreaAttacking) {
                if (boss.IsTargetVisible) {
                    if (boss.IsTargetInAttackRange()) {
                        return EnemyStateType.Attack;
                    } else {
                        return EnemyStateType.Chase;
                    }
                } else if (boss.Target != null) {
                    // Target not visible but known position
                    return EnemyStateType.Chase;
                } else {
                    return EnemyStateType.Patrol;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// State for boss charge attack
    /// </summary>
    public class BossChargeState : IEnemyState {
        private BossEnemyAI boss;

        public BossChargeState(BossEnemyAI boss) {
            this.boss = boss;
        }

        public void EnterState() {
            // Perform charge attack
            boss.PerformChargeAttack();
        }

        public void UpdateState() {
            // Nothing to do during update, the charge coroutine handles everything
        }

        public void ExitState() {
            // Nothing special on exit
        }

        public EnemyStateType? CheckTransitions() {
            // Once charge is complete, transition to appropriate state
            if (!boss.IsCharging) {
                if (boss.IsVulnerable) {
                    return EnemyStateType.Vulnerable;
                } else if (boss.IsTargetVisible) {
                    if (boss.IsTargetInAttackRange()) {
                        return EnemyStateType.Attack;
                    } else {
                        return EnemyStateType.Chase;
                    }
                } else {
                    return EnemyStateType.Patrol;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// State for boss summon minions
    /// </summary>
    public class BossSummonState : IEnemyState {
        private BossEnemyAI boss;
        private float enterTime;

        public BossSummonState(BossEnemyAI boss) {
            this.boss = boss;
        }

        public void EnterState() {
            // Record entry time
            enterTime = Time.time;

            // Stop movement
            boss.StopMovement();

            // Summon minions
            boss.SummonMinions();
        }

        public void UpdateState() {
            // Nothing to do during update, the summon coroutine handles everything
        }

        public void ExitState() {
            // Resume movement
            boss.ResumeMovement();
        }

        public EnemyStateType? CheckTransitions() {
            // Force transition after a maximum duration (5 seconds)
            if (Time.time > enterTime + 5f) {
                if (boss.IsTargetVisible) {
                    if (boss.IsTargetInAttackRange()) {
                        return EnemyStateType.Attack;
                    } else {
                        return EnemyStateType.Chase;
                    }
                } else if (boss.Target != null) {
                    // Target not visible but known position
                    return EnemyStateType.Chase;
                } else {
                    return EnemyStateType.Patrol;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// State for boss vulnerable phase
    /// </summary>
    public class BossVulnerableState : IEnemyState {
        private BossEnemyAI boss;

        public BossVulnerableState(BossEnemyAI boss) {
            this.boss = boss;
        }

        public void EnterState() {
            // Already handled by EnterVulnerableState method and coroutine
        }

        public void UpdateState() {
            // Nothing to do during update, the vulnerable coroutine handles everything
        }

        public void ExitState() {
            // Nothing special on exit
        }

        public EnemyStateType? CheckTransitions() {
            // Once no longer vulnerable, transition to chase or attack
            if (!boss.IsVulnerable) {
                // In later phases, boss might immediately perform another special attack
                if (boss.CurrentPhase >= 2 && Random.value < 0.3f) {
                    if (Random.value < 0.5f) {
                        return EnemyStateType.AreaAttack;
                    } else {
                        return EnemyStateType.Charge;
                    }
                } else if (boss.IsTargetVisible) {
                    if (boss.IsTargetInAttackRange()) {
                        return EnemyStateType.Attack;
                    } else {
                        return EnemyStateType.Chase;
                    }
                } else {
                    return EnemyStateType.Patrol;
                }
            }

            return null;
        }
    }
}