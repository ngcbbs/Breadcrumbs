using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.AI.Boss
{
    [RequireComponent(typeof(EnemyCombat))]
    public class BossEnemyAI : BaseEnemyAI
    {
        [Header("Boss Settings")]
        [SerializeField] protected int maxPhaseCount = 3;
        [SerializeField] protected float[] phaseThresholds = new float[] { 0.7f, 0.4f, 0.2f }; // percentage of health when phase changes
        [SerializeField] protected float enrageThreshold = 0.3f; // percentage of health when boss becomes enraged
        [SerializeField] protected float aoeAttackCooldown = 8f;
        [SerializeField] protected float summonCooldown = 15f;
        [SerializeField] protected float healthRegenRate = 0.0f; // Health regen per second during specific phases
        [SerializeField] protected GameObject minion1Prefab;
        [SerializeField] protected GameObject minion2Prefab;
        [SerializeField] protected int maxMinionCount = 5;
        [SerializeField] protected GameObject aoePrefab;
        [SerializeField] protected float aoeRadius = 5f;
        [SerializeField] protected float aoeDamage = 20f;
        [SerializeField] protected GameObject vulnerabilityEffect;
        
        // Phase tracking
        protected int currentPhase = 0;
        protected bool isEnraged = false;
        protected float lastAoeAttackTime = -999f;
        protected float lastSummonTime = -999f;
        protected List<GameObject> activeMinions = new List<GameObject>();
        protected HealthSystem healthSystem;
        
        // Special attack flags
        protected bool isVulnerable = false;
        protected bool isCharging = false;
        protected bool isAreaAttacking = false;
        protected bool isResting = false;
        
        protected override void Awake()
        {
            base.Awake();
            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogError($"BossEnemyAI on {gameObject.name} requires a HealthSystem component");
            }
        }
        
        protected override void SetupStates()
        {
            base.SetupStates();
            
            // Add boss-specific states
            availableStates.Add(EnemyStateType.AreaAttack, new BossAreaAttackState(this));
            availableStates.Add(EnemyStateType.Charge, new BossChargeState(this));
            availableStates.Add(EnemyStateType.Summon, new BossSummonState(this));
            availableStates.Add(EnemyStateType.Vulnerable, new BossVulnerableState(this));
        }
        
        protected override void Update()
        {
            base.Update();
            
            // Check for phase transitions
            CheckPhaseTransition();
            
            // Check for enrage state
            CheckEnrageState();
            
            // Health regeneration during specific phases if needed
            if (currentPhase == 2 && healthRegenRate > 0 && !isVulnerable)
            {
                if (healthSystem != null)
                {
                    healthSystem.Heal(healthRegenRate * Time.deltaTime);
                }
            }
        }
        
        protected void CheckPhaseTransition()
        {
            if (healthSystem == null || currentPhase >= maxPhaseCount)
                return;
                
            float healthPercentage = healthSystem.HealthPercentage;
            
            // Check if health is below the next phase threshold
            if (currentPhase < phaseThresholds.Length && 
                healthPercentage <= phaseThresholds[currentPhase])
            {
                TransitionToNextPhase();
            }
        }
        
        protected void CheckEnrageState()
        {
            if (healthSystem == null || isEnraged)
                return;
                
            float healthPercentage = healthSystem.HealthPercentage;
            
            // Check if health is below the enrage threshold
            if (healthPercentage <= enrageThreshold)
            {
                EnterEnragedState();
            }
        }
        
        /// <summary>
        /// Transition to the next phase with special effects and behaviors
        /// </summary>
        protected virtual void TransitionToNextPhase()
        {
            currentPhase++;
            
            // Temporarily stop current behaviors
            StopAllCoroutines();
            navAgent.isStopped = true;
            
            // Special transition effects
            StartCoroutine(PhaseTransitionRoutine());
            
            Debug.Log($"{gameObject.name} transitioned to Phase {currentPhase}");
        }
        
        protected IEnumerator PhaseTransitionRoutine()
        {
            // Phase transition animation/vfx
            if (animator != null)
            {
                animator.SetTrigger("PhaseChange");
            }
            
            // Brief invulnerability during transition
            if (healthSystem != null)
            {
                // 2 seconds of invulnerability
                healthSystem.AddInvulnerability(2f);
            }
            
            // Wait for transition duration
            yield return new WaitForSeconds(2f);
            
            // Adjust boss stats based on new phase
            AdjustStatsForPhase(currentPhase);
            
            // Resume movement
            navAgent.isStopped = false;
            
            // Special behavior based on phase
            switch (currentPhase)
            {
                case 1:
                    // Phase 1 special behavior - e.g., more aggressive
                    patrolWaitTime *= 0.7f; // Shorter patrol waits
                    attackCooldown *= 0.9f; // Slightly faster attacks
                    break;
                case 2:
                    // Phase 2 special behavior - e.g., summon minions
                    SummonMinions();
                    break;
                case 3:
                    // Phase 3 special behavior - e.g., unstoppable rage
                    EnterEnragedState(); // Force enrage on final phase if not already
                    break;
            }
        }
        
        /// <summary>
        /// Adjust boss stats based on current phase
        /// </summary>
        protected virtual void AdjustStatsForPhase(int phase)
        {
            switch (phase)
            {
                case 1:
                    combat.BaseDamage *= 1.2f;
                    navAgent.speed *= 1.1f;
                    break;
                case 2:
                    combat.BaseDamage *= 1.3f;
                    attackRange *= 1.2f;
                    break;
                case 3:
                    combat.BaseDamage *= 1.5f;
                    navAgent.speed *= 1.2f;
                    attackCooldown *= 0.7f;
                    break;
            }
        }
        
        /// <summary>
        /// Enter the enraged state with increased damage and speed
        /// </summary>
        protected virtual void EnterEnragedState()
        {
            if (isEnraged)
                return;
                
            isEnraged = true;
            
            // Visual indicator of enraged state
            if (animator != null)
            {
                animator.SetBool("Enraged", true);
            }
            
            // Add particle effect for enrage
            ParticleSystem[] existingParticles = GetComponentsInChildren<ParticleSystem>();
            foreach (var particles in existingParticles)
            {
                var main = particles.main;
                main.startColor = Color.red;
                main.startSize = main.startSize.constant * 1.5f;
            }
            
            // Increase stats
            combat.BaseDamage *= 1.5f;
            navAgent.speed *= 1.3f;
            attackCooldown *= 0.6f;
            
            Debug.Log($"{gameObject.name} entered enraged state");
        }
        
        /// <summary>
        /// Perform an area of effect attack
        /// </summary>
        public virtual void PerformAreaAttack()
        {
            if (Time.time < lastAoeAttackTime + aoeAttackCooldown)
                return;
                
            lastAoeAttackTime = Time.time;
            isAreaAttacking = true;
            
            StartCoroutine(AreaAttackRoutine());
        }
        
        protected IEnumerator AreaAttackRoutine()
        {
            // Stop movement
            navAgent.isStopped = true;
            
            // Play charge-up animation
            if (animator != null)
            {
                animator.SetTrigger("AreaAttack");
            }
            
            // Charge-up delay
            yield return new WaitForSeconds(1.5f);
            
            // Spawn AOE effect
            if (aoePrefab != null)
            {
                GameObject aoeInstance = Instantiate(aoePrefab, transform.position, Quaternion.identity);
                
                // Scale to match AOE radius
                aoeInstance.transform.localScale = new Vector3(
                    aoeRadius * 2f, 
                    aoeInstance.transform.localScale.y, 
                    aoeRadius * 2f
                );
                
                // Destroy after effect duration
                Destroy(aoeInstance, 5f);
            }
            
            // Apply damage to targets in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius, playerLayer);
            foreach (var hitCollider in hitColliders)
            {
                IDamageable target = hitCollider.GetComponent<IDamageable>();
                if (target != null && target.IsAlive())
                {
                    combat.DealDamage(target, aoeDamage, DamageType.Magical);
                }
            }
            
            // Resume movement after short delay
            yield return new WaitForSeconds(1f);
            navAgent.isStopped = false;
            isAreaAttacking = false;
        }
        
        /// <summary>
        /// Summon minions to assist the boss
        /// </summary>
        public virtual void SummonMinions()
        {
            if (Time.time < lastSummonTime + summonCooldown)
                return;
                
            // Clear out any destroyed minions from the list
            activeMinions.RemoveAll(minion => minion == null);
            
            // Check if we've reached the max minion count
            if (activeMinions.Count >= maxMinionCount)
                return;
                
            lastSummonTime = Time.time;
            
            StartCoroutine(SummonMinionRoutine());
        }
        
        protected IEnumerator SummonMinionRoutine()
        {
            // Stop movement during summon
            navAgent.isStopped = true;
            
            // Play summon animation
            if (animator != null)
            {
                animator.SetTrigger("Summon");
            }
            
            // Summon delay
            yield return new WaitForSeconds(1.5f);
            
            // Number of minions to summon (more in later phases)
            int minionsToSummon = Mathf.Min(
                maxMinionCount - activeMinions.Count,
                currentPhase + 1
            );
            
            for (int i = 0; i < minionsToSummon; i++)
            {
                // Select which minion type to spawn
                GameObject minionPrefab = (Random.value > 0.5f) ? minion1Prefab : minion2Prefab;
                
                // Skip if prefab is missing
                if (minionPrefab == null)
                    continue;
                    
                // Calculate spawn position (random point around boss)
                Vector3 spawnOffset = Random.insideUnitSphere * 3f;
                spawnOffset.y = 0;
                Vector3 spawnPos = transform.position + spawnOffset;
                
                // Ensure position is on navmesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
                {
                    // Spawn the minion
                    GameObject minion = Instantiate(minionPrefab, hit.position, Quaternion.identity);
                    
                    // Add to active minions list
                    activeMinions.Add(minion);
                    
                    // Short delay between spawns
                    yield return new WaitForSeconds(0.5f);
                }
            }
            
            // Resume movement
            navAgent.isStopped = false;
        }
        
        /// <summary>
        /// Enter a vulnerable state after specific attacks or phases
        /// </summary>
        public virtual void EnterVulnerableState(float duration)
        {
            if (isVulnerable)
                return;
                
            StartCoroutine(VulnerableStateRoutine(duration));
        }
        
        protected IEnumerator VulnerableStateRoutine(float duration)
        {
            isVulnerable = true;
            
            // Stop movement
            navAgent.isStopped = true;
            
            // Visual indicator of vulnerable state
            if (animator != null)
            {
                animator.SetTrigger("Stun");
                animator.SetBool("IsStunned", true);
            }
            
            // Spawn vulnerability effect
            GameObject vulnerabilityInstance = null;
            if (vulnerabilityEffect != null)
            {
                vulnerabilityInstance = Instantiate(
                    vulnerabilityEffect, 
                    transform.position + Vector3.up * 2f, 
                    Quaternion.identity, 
                    transform
                );
            }
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // End vulnerable state
            isVulnerable = false;
            
            if (animator != null)
            {
                animator.SetBool("IsStunned", false);
            }
            
            // Destroy vulnerability effect
            if (vulnerabilityInstance != null)
            {
                Destroy(vulnerabilityInstance);
            }
            
            // Resume movement
            navAgent.isStopped = false;
        }
        
        /// <summary>
        /// Perform a charging attack toward the target
        /// </summary>
        public virtual void PerformChargeAttack()
        {
            if (isCharging || target == null)
                return;
                
            StartCoroutine(ChargeAttackRoutine());
        }
        
        protected IEnumerator ChargeAttackRoutine()
        {
            isCharging = true;
            
            // Store original nav agent properties
            float originalSpeed = navAgent.speed;
            float originalAcceleration = navAgent.acceleration;
            float originalStoppingDistance = navAgent.stoppingDistance;
            bool originalAutoBreaking = navAgent.autoBraking;
            
            // Play charge start animation
            if (animator != null)
            {
                animator.SetTrigger("Charge");
            }
            
            // Charge preparation
            yield return new WaitForSeconds(1f);
            
            // Boost speed for charge
            navAgent.speed = originalSpeed * 3f;
            navAgent.acceleration = originalAcceleration * 2f;
            navAgent.stoppingDistance = 0.5f;
            navAgent.autoBraking = false;
            
            // Target position for charge (extend beyond player to maintain momentum)
            Vector3 chargeDirection = (target.position - transform.position).normalized;
            Vector3 chargeTarget = target.position + chargeDirection * 5f;
            
            // Start charge movement
            navAgent.SetDestination(chargeTarget);
            
            // Track entities hit during this charge to avoid hitting the same target multiple times
            HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
            
            // Maximum charge time
            float chargeStartTime = Time.time;
            float maxChargeDuration = 3f;
            
            // Charge loop - continue until we reach destination or max time
            while (Time.time - chargeStartTime < maxChargeDuration && !HasReachedDestination())
            {
                // Damage any targets in path
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.5f, playerLayer);
                
                foreach (var hitCollider in hitColliders)
                {
                    IDamageable target = hitCollider.GetComponent<IDamageable>();
                    if (target != null && target.IsAlive() && !hitTargets.Contains(target))
                    {
                        // Apply charge damage
                        combat.DealDamage(target, combat.BaseDamage * 2f, DamageType.Physical);
                        
                        // Apply knockback
                        Rigidbody targetRb = hitCollider.GetComponent<Rigidbody>();
                        if (targetRb != null)
                        {
                            targetRb.AddForce(chargeDirection * 10f, ForceMode.Impulse);
                        }
                        
                        // Add to hit targets
                        hitTargets.Add(target);
                    }
                }
                
                yield return null;
            }
            
            // Restore original nav agent properties
            navAgent.speed = originalSpeed;
            navAgent.acceleration = originalAcceleration;
            navAgent.stoppingDistance = originalStoppingDistance;
            navAgent.autoBraking = originalAutoBreaking;
            
            // Reset paths
            navAgent.ResetPath();
            
            // Enter brief vulnerable state after charge
            if (currentPhase < 3) // Not vulnerable in final phase
            {
                EnterVulnerableState(3f);
            }
            else
            {
                // Just a short rest in final phase
                isResting = true;
                yield return new WaitForSeconds(1f);
                isResting = false;
            }
            
            isCharging = false;
        }
        
        // Getters for boss states
        public bool IsEnraged => isEnraged;
        public bool IsVulnerable => isVulnerable;
        public bool IsCharging => isCharging;
        public bool IsAreaAttacking => isAreaAttacking;
        public bool IsResting => isResting;
        public int CurrentPhase => currentPhase;
        
        public bool CanAreaAttack()
        {
            return Time.time >= lastAoeAttackTime + aoeAttackCooldown;
        }
        
        public bool CanSummon()
        {
            return Time.time >= lastSummonTime + summonCooldown && 
                  activeMinions.Count < maxMinionCount;
        }
        
        /// <summary>
        /// Override base attack to include special attacks based on phase
        /// </summary>
        public override void PerformAttack()
        {
            if (combat == null || target == null || isVulnerable)
                return;
                
            // Check for special attacks based on phase and conditions
            if (currentPhase >= 1 && CanAreaAttack() && Random.value < 0.3f)
            {
                // 30% chance to use AOE attack in Phase 1+
                PerformAreaAttack();
            }
            else if (currentPhase >= 2 && Random.value < 0.2f && Vector3.Distance(transform.position, target.position) > 8f)
            {
                // 20% chance to charge in Phase 2+ if target is far enough
                PerformChargeAttack();
            }
            else if (currentPhase >= 2 && CanSummon() && Random.value < 0.15f)
            {
                // 15% chance to summon in Phase 2+
                SummonMinions();
            }
            else
            {
                // Normal attack
                base.PerformAttack();
                
                // Enraged bosses have a chance for a quick follow-up attack
                if (isEnraged && Random.value < 0.4f)
                {
                    StartCoroutine(EnragedFollowupAttack());
                }
            }
        }
        
        protected IEnumerator EnragedFollowupAttack()
        {
            // Short delay before follow-up
            yield return new WaitForSeconds(0.5f);
            
            // Perform a quick second attack
            if (combat != null && target != null)
            {
                combat.Attack(target);
            }
        }
    }
}