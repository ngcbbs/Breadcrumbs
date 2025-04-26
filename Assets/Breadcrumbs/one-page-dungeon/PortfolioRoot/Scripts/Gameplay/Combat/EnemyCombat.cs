using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.AI;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// Handles enemy combat actions and attacks
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyCombat : CombatSystem {
        [Header("Enemy Combat Settings")]
        [SerializeField]
        private float aggressionLevel = 0.5f;
        [SerializeField]
        private float criticalHealthThreshold = 0.3f;
        [SerializeField]
        private bool canEnrage = true;

        [Header("Attack Variations")]
        [SerializeField]
        private List<AttackVariation> attackVariations = new List<AttackVariation>();
        [SerializeField]
        private float attackVariationCooldown = 10f;

        [Header("Special Attacks")]
        [SerializeField]
        private SpecialAttack specialAttack;
        [SerializeField]
        private float specialAttackHealthThreshold = 0.5f;
        [SerializeField]
        private float specialAttackCooldown = 20f;
        [SerializeField]
        private float enrageHealthThreshold = 0.3f;

        // Components
        private HealthSystem healthSystem;
        private BaseEnemyAI enemyAI;

        // State tracking
        private bool isEnraged = false;
        private float lastSpecialAttackTime = -999f;
        private float lastAttackVariationTime = -999f;
        private Dictionary<int, float> attackVariationLastUsed = new Dictionary<int, float>();

        protected override void Awake() {
            base.Awake();

            healthSystem = GetComponent<HealthSystem>();
            enemyAI = GetComponent<BaseEnemyAI>();

            // Initialize attack variation tracking
            for (int i = 0; i < attackVariations.Count; i++) {
                attackVariationLastUsed[i] = -999f;
            }

            // Subscribe to health events
            if (healthSystem != null) {
                healthSystem.OnHealthChanged.AddListener(OnHealthChanged);
            }
        }

        /// <summary>
        /// Track health changes for enrage and other mechanics
        /// </summary>
        private void OnHealthChanged(float current, float max) {
            float healthPercentage = current / max;

            // Check for enrage
            if (canEnrage && !isEnraged && healthPercentage <= enrageHealthThreshold) {
                EnterEnragedState();
            }
        }

        /// <summary>
        /// Enter enraged state with enhanced attack capabilities
        /// </summary>
        private void EnterEnragedState() {
            isEnraged = true;

            // Increase damage and speed
            baseDamage *= 1.5f;
            attackCooldown *= 0.7f;

            // Visual feedback
            if (animator != null) {
                animator.SetBool("Enraged", true);
            }

            // Play enrage effect
            PlayEnrageEffect();

            Debug.Log($"{gameObject.name} has entered enraged state!");
        }

        /// <summary>
        /// Play visual and audio effects for enrage
        /// </summary>
        private void PlayEnrageEffect() {
            // Particle effect for enrage could be added here
            if (attackVFX != null) {
                GameObject effect = Instantiate(attackVFX, transform.position, Quaternion.identity);
                effect.transform.localScale = Vector3.one * 2f; // Bigger effect

                // Could set color or other properties
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null) {
                    var main = ps.main;
                    main.startColor = Color.red;
                }
            }

            // Play enrage sound
            if (audioSource != null && attackSound != null) {
                audioSource.pitch = 0.8f;  // Lower pitch for more menacing sound
                audioSource.volume = 1.5f; // Louder
                audioSource.PlayOneShot(attackSound);

                // Reset audio properties after a delay
                StartCoroutine(ResetAudioProperties());
            }
        }

        /// <summary>
        /// Reset audio properties after a delay
        /// </summary>
        private IEnumerator ResetAudioProperties() {
            yield return new WaitForSeconds(2f);

            if (audioSource != null) {
                audioSource.pitch = 1f;
                audioSource.volume = 1f;
            }
        }

        /// <summary>
        /// Perform an attack with chance for variations
        /// </summary>
        public override void PerformAttack() {
            if (!CanAttack())
                return;

            // Check for special attack
            if (CanUseSpecialAttack()) {
                PerformSpecialAttack();
                return;
            }

            // Check for attack variation
            int variationIndex = ChooseAttackVariation();
            if (variationIndex >= 0) {
                PerformAttackVariation(variationIndex);
                return;
            }

            // Perform standard attack
            base.PerformAttack();
        }

        /// <summary>
        /// Choose an attack variation based on conditions and cooldowns
        /// </summary>
        private int ChooseAttackVariation() {
            if (attackVariations.Count == 0 || Time.time < lastAttackVariationTime + attackVariationCooldown)
                return -1;

            List<int> availableVariations = new List<int>();

            for (int i = 0; i < attackVariations.Count; i++) {
                AttackVariation variation = attackVariations[i];

                // Check individual cooldown
                if (attackVariationLastUsed.ContainsKey(i) &&
                    Time.time < attackVariationLastUsed[i] + variation.cooldown)
                    continue;

                // Check conditions
                bool conditionsMet = true;

                // Health-based condition
                if (variation.useHealthThreshold) {
                    float healthPercentage = healthSystem != null ? healthSystem.CurrentHealth / healthSystem.MaxHealth : 1f;

                    if (variation.healthThresholdType == ThresholdType.Below) {
                        conditionsMet &= healthPercentage <= variation.healthThreshold;
                    } else {
                        conditionsMet &= healthPercentage >= variation.healthThreshold;
                    }
                }

                // Distance-based condition
                if (variation.useDistanceThreshold && enemyAI != null && enemyAI.Target != null) {
                    float distance = Vector3.Distance(transform.position, enemyAI.Target.position);

                    if (variation.distanceThresholdType == ThresholdType.Below) {
                        conditionsMet &= distance <= variation.distanceThreshold;
                    } else {
                        conditionsMet &= distance >= variation.distanceThreshold;
                    }
                }

                if (conditionsMet) {
                    // Add to available variations, weighted by probability
                    for (int j = 0; j < variation.probability * 100; j++) {
                        availableVariations.Add(i);
                    }
                }
            }

            // Choose a random variation from available ones
            if (availableVariations.Count > 0) {
                return availableVariations[Random.Range(0, availableVariations.Count)];
            }

            return -1;
        }

        /// <summary>
        /// Perform a specific attack variation
        /// </summary>
        private void PerformAttackVariation(int variationIndex) {
            if (variationIndex < 0 || variationIndex >= attackVariations.Count)
                return;

            AttackVariation variation = attackVariations[variationIndex];

            isAttacking = true;
            lastAttackTime = Time.time;
            lastAttackVariationTime = Time.time;
            attackVariationLastUsed[variationIndex] = Time.time;

            // Play animation
            if (animator != null && !string.IsNullOrEmpty(variation.animationTrigger)) {
                animator.SetTrigger(variation.animationTrigger);
            }

            // Play effect
            if (variation.attackEffect != null) {
                Instantiate(variation.attackEffect, transform.position, transform.rotation);
            }

            // Play sound
            if (audioSource != null && variation.attackSound != null) {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(variation.attackSound);
            }

            // Execute attack
            StartCoroutine(ExecuteAttackVariation(variation));
        }

        /// <summary>
        /// Execute attack variation over time
        /// </summary>
        private IEnumerator ExecuteAttackVariation(AttackVariation variation) {
            // Wait for wind-up
            yield return new WaitForSeconds(variation.windUpTime);

            // Apply damage
            if (variation.attackType == AttackType.Area) {
                // Area attack
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, variation.range, targetLayers);

                foreach (var hitCollider in hitColliders) {
                    IDamageable target = hitCollider.GetComponent<IDamageable>();
                    if (target != null && target.IsAlive()) {
                        DealDamage(target, variation.damage, variation.damageType);
                    }
                }
            } else {
                // Directional attack
                Vector3 direction = transform.forward;

                if (enemyAI != null && enemyAI.Target != null) {
                    direction = (enemyAI.Target.position - transform.position).normalized;
                    direction.y = 0;
                }

                RaycastHit[] hits = Physics.SphereCastAll(transform.position, variation.width * 0.5f,
                    direction, variation.range, targetLayers);

                foreach (var hit in hits) {
                    IDamageable target = hit.collider.GetComponent<IDamageable>();
                    if (target != null && target.IsAlive()) {
                        DealDamage(target, variation.damage, variation.damageType);
                    }
                }
            }

            // Wait for cooldown
            yield return new WaitForSeconds(variation.cooldownTime);

            isAttacking = false;
        }

        /// <summary>
        /// Check if special attack can be used
        /// </summary>
        private bool CanUseSpecialAttack() {
            if (specialAttack.attackEffect == null || Time.time < lastSpecialAttackTime + specialAttackCooldown)
                return false;

            // Check health threshold
            if (healthSystem != null) {
                float healthPercentage = healthSystem.CurrentHealth / healthSystem.MaxHealth;

                if (healthPercentage > specialAttackHealthThreshold)
                    return false;
            }

            // Additional conditions like distance could be added here

            // Random chance based on aggression
            return Random.value < (aggressionLevel * 0.5f);
        }

        /// <summary>
        /// Perform special attack
        /// </summary>
        private void PerformSpecialAttack() {
            isAttacking = true;
            lastAttackTime = Time.time;
            lastSpecialAttackTime = Time.time;

            // Play animation
            if (animator != null && !string.IsNullOrEmpty(specialAttack.animationTrigger)) {
                animator.SetTrigger(specialAttack.animationTrigger);
            }

            // Play effect
            if (specialAttack.attackEffect != null) {
                Instantiate(specialAttack.attackEffect, transform.position, transform.rotation);
            }

            // Play sound
            if (audioSource != null && specialAttack.attackSound != null) {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(specialAttack.attackSound);
            }

            // Execute attack
            StartCoroutine(ExecuteSpecialAttack());
        }

        /// <summary>
        /// Execute special attack over time
        /// </summary>
        private IEnumerator ExecuteSpecialAttack() {
            // Special attack logic could be implemented here
            yield return new WaitForSeconds(specialAttack.windUpTime);

            // Apply damage in a large area
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, specialAttack.range, targetLayers);

            foreach (var hitCollider in hitColliders) {
                IDamageable target = hitCollider.GetComponent<IDamageable>();
                if (target != null && target.IsAlive()) {
                    DealDamage(target, specialAttack.damage, specialAttack.damageType);

                    // Apply knockback
                    Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                    if (rb != null) {
                        Vector3 direction = (hitCollider.transform.position - transform.position).normalized;
                        rb.AddForce(direction * specialAttack.knockbackForce, ForceMode.Impulse);
                    }
                }
            }

            // Wait for cooldown
            yield return new WaitForSeconds(specialAttack.cooldownTime);

            isAttacking = false;
        }

        /// <summary>
        /// Calculate damage with aggression and enrage factors
        /// </summary>
        protected override float CalculateDamage() {
            float damage = base.CalculateDamage();

            // Apply aggression level
            damage *= (0.8f + aggressionLevel * 0.4f);

            // Apply critical health bonus
            if (healthSystem != null) {
                float healthPercentage = healthSystem.CurrentHealth / healthSystem.MaxHealth;

                if (healthPercentage <= criticalHealthThreshold) {
                    damage *= 1.3f; // Desperate enemies hit harder
                }
            }

            return damage;
        }

        /// <summary>
        /// Attack a specific target
        /// </summary>
        public void Attack(Transform target) {
            if (target == null || !CanAttack())
                return;

            // Face target
            Vector3 direction = target.position - transform.position;
            direction.y = 0;

            if (direction.magnitude > 0.1f) {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Perform attack
            PerformAttack();
        }
    }

    /// <summary>
    /// Types of attack variations
    /// </summary>
    public enum AttackType {
        Area,        // Attack in all directions
        Directional, // Attack in a specific direction

        // 추가
        Melee,   // Melee attack
        Ranged,  // Ranged attack
        Spell,   // Spell attack
        Special, // Special attack

    }

    /// <summary>
    /// Types of thresholds for conditions
    /// </summary>
    public enum ThresholdType {
        Below, // Value must be below threshold
        Above  // Value must be above threshold
    }

    /// <summary>
    /// Data container for attack variations
    /// </summary>
    [System.Serializable]
    public class AttackVariation {
        [Header("Basic Settings")]
        public string name = "Attack Variation";
        public float damage = 15f;
        public DamageType damageType = DamageType.Physical;
        public AttackType attackType = AttackType.Directional;
        public float probability = 0.3f; // 0-1 chance of using this variation

        [Header("Range Settings")]
        public float range = 3f; // How far the attack reaches
        public float width = 1f; // Width of directional attack

        [Header("Timing Settings")]
        public float windUpTime = 0.5f; // Time before damage is applied
        public float cooldownTime = 1f; // Time after attack before next action
        public float cooldown = 15f;    // Time before this variation can be used again

        [Header("Conditions")]
        public bool useHealthThreshold = false;
        public ThresholdType healthThresholdType = ThresholdType.Below;
        public float healthThreshold = 0.5f;

        public bool useDistanceThreshold = false;
        public ThresholdType distanceThresholdType = ThresholdType.Below;
        public float distanceThreshold = 5f;

        [Header("Effects")]
        public string animationTrigger = "AttackSpecial";
        public GameObject attackEffect;
        public AudioClip attackSound;
    }

    /// <summary>
    /// Data container for special attacks
    /// </summary>
    [System.Serializable]
    public class SpecialAttack {
        [Header("Basic Settings")]
        public string name = "Special Attack";
        public float damage = 30f;
        public DamageType damageType = DamageType.Physical;
        public float range = 5f;

        [Header("Timing")]
        public float windUpTime = 1f;
        public float cooldownTime = 2f;

        [Header("Effects")]
        public string animationTrigger = "SpecialAttack";
        public GameObject attackEffect;
        public AudioClip attackSound;
        public float knockbackForce = 10f;
    }
}