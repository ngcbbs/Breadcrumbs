using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.Gameplay.Combat {
    /// <summary>
    /// Handles player combat actions and abilities
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerCombat : CombatSystem {
        [Header("Player Combat Settings")]
        [SerializeField]
        private float staminaCostPerAttack = 10f;
        [SerializeField]
        private float comboTimeWindow = 0.8f;
        [SerializeField]
        private int maxComboCount = 3;

        [Header("Abilities")]
        [SerializeField]
        private List<SkillData> availableSkills = new List<SkillData>();
        [SerializeField]
        private float skillCooldownMultiplier = 1f;

        [Header("Block Settings")]
        [SerializeField]
        private bool canBlock = true;
        [SerializeField]
        private float blockDamageReduction = 0.5f;
        [SerializeField]
        private float blockStaminaCost = 5f;
        [SerializeField]
        private float perfectBlockWindow = 0.2f;
        [SerializeField]
        private GameObject blockVFX;
        [SerializeField]
        private GameObject perfectBlockVFX;

        [Header("Dodge Settings")]
        [SerializeField]
        private bool canDodge = true;
        [SerializeField]
        private float dodgeDistance = 5f;
        [SerializeField]
        private float dodgeSpeed = 10f;
        [SerializeField]
        private float dodgeDuration = 0.5f;
        [SerializeField]
        private float dodgeInvulnerabilityTime = 0.3f;
        [SerializeField]
        private float dodgeStaminaCost = 20f;
        [SerializeField]
        private GameObject dodgeVFX;

        // Events
        [Header("Additional Events")]
        public UnityEvent<int> OnComboUpdated;
        public UnityEvent<int, float> OnSkillUsed;
        public UnityEvent OnBlockStart;
        public UnityEvent OnBlockEnd;
        public UnityEvent OnPerfectBlock;
        public UnityEvent OnDodgeStart;
        public UnityEvent OnDodgeEnd;

        // Components
        private HealthSystem healthSystem;
        private StaminaSystem staminaSystem;

        // Combo state
        private int currentCombo = 0;
        private float lastComboTime = -999f;
        private Coroutine comboResetCoroutine;

        // Block state
        private bool isBlocking = false;
        private float blockStartTime = -999f;

        // Dodge state
        private bool isDodging = false;
        private Vector3 dodgeDirection;
        private Coroutine dodgeCoroutine;

        // Skill cooldowns
        private Dictionary<int, float> skillLastUsedTime = new Dictionary<int, float>();

        protected override void Awake() {
            base.Awake();

            healthSystem = GetComponent<HealthSystem>();
            staminaSystem = GetComponent<StaminaSystem>();

            // Initialize skill cooldowns
            for (int i = 0; i < availableSkills.Count; i++) {
                skillLastUsedTime[i] = -999f;
            }
        }

        private void Update() {
            // Optional: Add input handling here for testing
            // In a real game, this would likely be driven by an input manager
        }

        /// <summary>
        /// Perform a basic attack with combo system
        /// </summary>
        public override void PerformAttack() {
            if (!CanAttack() || isDodging)
                return;

            // Check if we have enough stamina
            if (staminaSystem != null && !staminaSystem.UseStamina(staminaCostPerAttack))
                return;

            // Update combo
            if (Time.time <= lastComboTime + comboTimeWindow) {
                currentCombo = (currentCombo + 1) % (maxComboCount + 1);
                if (currentCombo == 0) currentCombo = 1; // Loop back to 1 instead of 0
            } else {
                currentCombo = 1;
            }

            // Send combo event
            OnComboUpdated?.Invoke(currentCombo);

            // Update combo timer
            lastComboTime = Time.time;

            // Cancel any existing combo reset
            if (comboResetCoroutine != null)
                StopCoroutine(comboResetCoroutine);

            // Schedule combo reset
            comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());

            // Set attack animation based on combo
            if (animator != null) {
                animator.SetInteger("ComboStep", currentCombo);
            }

            // Perform the attack with combo damage
            isAttacking = true;
            lastAttackTime = Time.time;

            // Attack started event
            OnAttackStarted?.Invoke();

            // Play attack sound
            if (audioSource != null && attackSound != null) {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(attackSound);
            }

            // Handle hit detection (in a real game, this would likely be called via animation events)
            HandleHitDetection();

            // Schedule end of attack
            StartCoroutine(EndAttackAfterDelay(attackCooldown * 0.7f));
        }

        /// <summary>
        /// Reset combo after window expires
        /// </summary>
        private IEnumerator ResetComboAfterDelay() {
            yield return new WaitForSeconds(comboTimeWindow);
            currentCombo = 0;
            OnComboUpdated?.Invoke(currentCombo);
        }

        /// <summary>
        /// Calculate damage with combo multiplier
        /// </summary>
        protected override float CalculateDamage() {
            // Apply combo multiplier (combo 1 = 1x, combo 2 = 1.2x, combo 3 = 1.5x)
            float comboMultiplier = 1f;

            switch (currentCombo) {
                case 2:
                    comboMultiplier = 1.2f;
                    break;
                case 3:
                    comboMultiplier = 1.5f;
                    break;
                default:
                    comboMultiplier = 1f;
                    break;
            }

            return base.CalculateDamage() * comboMultiplier;
        }

        /// <summary>
        /// Use a skill by index
        /// </summary>
        public bool UseSkill(int skillIndex) {
            if (skillIndex < 0 || skillIndex >= availableSkills.Count || isAttacking || isDodging)
                return false;

            SkillData skill = availableSkills[skillIndex];

            // Check cooldown
            if (!IsSkillReady(skillIndex))
                return false;

            // Check stamina
            if (staminaSystem != null && !staminaSystem.UseStamina(skill.staminaCost))
                return false;

            // Use the skill
            StartCoroutine(ExecuteSkill(skillIndex));

            // Update cooldown
            skillLastUsedTime[skillIndex] = Time.time;

            // Skill used event
            OnSkillUsed?.Invoke(skillIndex, skill.cooldown * skillCooldownMultiplier);

            return true;
        }

        /// <summary>
        /// Execute a skill's effects
        /// </summary>
        private IEnumerator ExecuteSkill(int skillIndex) {
            SkillData skill = availableSkills[skillIndex];

            // Block other actions
            isAttacking = true;

            // Play skill animation
            if (animator != null) {
                animator.SetTrigger("Skill" + (skillIndex + 1));
            }

            // Play skill effect
            if (skill.skillEffect != null) {
                Instantiate(skill.skillEffect, transform.position, transform.rotation);
            }

            // Wait for execution time
            yield return new WaitForSeconds(skill.executionTime);

            // Apply skill effects
            ApplySkillEffects(skill);

            // End attack state after full duration
            yield return new WaitForSeconds(skill.duration - skill.executionTime);

            isAttacking = false;
        }

        /// <summary>
        /// Apply a skill's effects based on type
        /// </summary>
        private void ApplySkillEffects(SkillData skill) {
            switch (skill.skillType) {
                case SkillType.Attack:
                    // Apply damage in area
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, skill.range, targetLayers);

                    foreach (var hitCollider in hitColliders) {
                        IDamageable target = hitCollider.GetComponent<IDamageable>();
                        if (target != null && target.IsAlive()) {
                            DealDamage(target, skill.power, skill.damageType);

                            // Apply status effect if any
                            // Implementation for status effects would go here
                        }
                    }

                    break;

                case SkillType.Heal:
                    // Heal self
                    if (healthSystem != null) {
                        healthSystem.Heal(skill.power);
                    }

                    break;

                case SkillType.Buff:
                    // Apply buff
                    // Implementation for buffs would go here
                    break;

                case SkillType.Utility:
                    // Utility skill effects
                    // Implementation for utility skills would go here
                    break;
            }
        }

        /// <summary>
        /// Check if a skill is off cooldown
        /// </summary>
        public bool IsSkillReady(int skillIndex) {
            if (skillIndex < 0 || skillIndex >= availableSkills.Count)
                return false;

            if (!skillLastUsedTime.ContainsKey(skillIndex))
                return true;

            float cooldown = availableSkills[skillIndex].cooldown * skillCooldownMultiplier;
            return Time.time >= skillLastUsedTime[skillIndex] + cooldown;
        }

        /// <summary>
        /// Get skill cooldown remaining
        /// </summary>
        public float GetSkillCooldownRemaining(int skillIndex) {
            if (skillIndex < 0 || skillIndex >= availableSkills.Count)
                return 0f;

            if (!skillLastUsedTime.ContainsKey(skillIndex))
                return 0f;

            float cooldown = availableSkills[skillIndex].cooldown * skillCooldownMultiplier;
            float timeRemaining = (skillLastUsedTime[skillIndex] + cooldown) - Time.time;

            return Mathf.Max(0f, timeRemaining);
        }

        /// <summary>
        /// Get skill cooldown percentage
        /// </summary>
        public float GetSkillCooldownPercentage(int skillIndex) {
            if (skillIndex < 0 || skillIndex >= availableSkills.Count)
                return 1f;

            float cooldown = availableSkills[skillIndex].cooldown * skillCooldownMultiplier;

            if (cooldown <= 0f || !skillLastUsedTime.ContainsKey(skillIndex))
                return 1f;

            float elapsedTime = Time.time - skillLastUsedTime[skillIndex];
            return Mathf.Clamp01(elapsedTime / cooldown);
        }

        /// <summary>
        /// Start blocking
        /// </summary>
        public void StartBlock() {
            if (!canBlock || isAttacking || isDodging || isBlocking)
                return;

            isBlocking = true;
            blockStartTime = Time.time;

            // Play block animation
            if (animator != null) {
                animator.SetBool("Blocking", true);
            }

            // Spawn block VFX
            if (blockVFX != null) {
                Instantiate(blockVFX, transform.position, transform.rotation);
            }

            // Reduce movement speed or other effects
            // Implementation would go here

            // Block event
            OnBlockStart?.Invoke();
        }

        /// <summary>
        /// End blocking
        /// </summary>
        public void EndBlock() {
            if (!isBlocking)
                return;

            isBlocking = false;

            // Update animation
            if (animator != null) {
                animator.SetBool("Blocking", false);
            }

            // Block end event
            OnBlockEnd?.Invoke();
        }

        /// <summary>
        /// Check if block is a perfect block (timing-based)
        /// </summary>
        private bool IsPerfectBlock() {
            return isBlocking && Time.time <= blockStartTime + perfectBlockWindow;
        }

        /// <summary>
        /// Perform a dodge
        /// </summary>
        public void Dodge(Vector3 direction) {
            if (!canDodge || isAttacking || isDodging || isBlocking)
                return;

            // Check stamina
            if (staminaSystem != null && !staminaSystem.UseStamina(dodgeStaminaCost))
                return;

            // Normalize direction with horizontal movement only
            dodgeDirection = new Vector3(direction.x, 0, direction.z).normalized;

            // If no direction provided, dodge backward
            if (dodgeDirection.magnitude < 0.1f) {
                dodgeDirection = -transform.forward;
            }

            // Start dodge
            if (dodgeCoroutine != null)
                StopCoroutine(dodgeCoroutine);

            dodgeCoroutine = StartCoroutine(DodgeRoutine());
        }

        /// <summary>
        /// Dodge movement routine
        /// </summary>
        private IEnumerator DodgeRoutine() {
            isDodging = true;

            // Play dodge animation
            if (animator != null) {
                animator.SetTrigger("Dodge");
            }

            // Spawn dodge VFX
            if (dodgeVFX != null) {
                Instantiate(dodgeVFX, transform.position, transform.rotation);
            }

            // Dodge start event
            OnDodgeStart?.Invoke();

            // Add invulnerability
            if (healthSystem != null) {
                healthSystem.AddInvulnerability(dodgeInvulnerabilityTime);
            }

            // Perform dodge movement
            float startTime = Time.time;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + dodgeDirection * dodgeDistance;

            // Check if target position is valid
            RaycastHit hit;
            if (Physics.Raycast(startPosition, dodgeDirection, out hit, dodgeDistance)) {
                // Adjust target position to avoid collision
                targetPosition = startPosition + dodgeDirection * (hit.distance - 0.5f);
            }

            // Move over time
            while (Time.time < startTime + dodgeDuration) {
                float t = (Time.time - startTime) / dodgeDuration;

                // Apply easing for smoother motion
                float easedT = t * t * (3f - 2f * t); // Smoothstep

                // Update position
                transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);

                yield return null;
            }

            // Ensure final position
            transform.position = targetPosition;

            // End dodge
            isDodging = false;

            // Dodge end event
            OnDodgeEnd?.Invoke();
        }

        /// <summary>
        /// Handle incoming damage with block mechanics
        /// </summary>
        public void OnDamageReceived(float amount, DamageType type, GameObject source) {
            if (isBlocking) {
                // Check for perfect block
                if (IsPerfectBlock()) {
                    // Perfect block - negate damage and trigger counter
                    if (perfectBlockVFX != null) {
                        Instantiate(perfectBlockVFX, transform.position, transform.rotation);
                    }

                    // Perfect block event
                    OnPerfectBlock?.Invoke();

                    // Counter attack
                    // Implementation would go here

                    // Consume stamina
                    if (staminaSystem != null) {
                        staminaSystem.UseStamina(blockStaminaCost * 0.5f);
                    }
                } else {
                    // Regular block - reduce damage
                    amount *= (1f - blockDamageReduction);

                    // Consume stamina
                    if (staminaSystem != null) {
                        staminaSystem.UseStamina(blockStaminaCost);
                    }
                }
            }

            // Apply damage
            if (healthSystem != null && amount > 0) {
                healthSystem.TakeDamage(amount, type, source);
            }
        }

        /// <summary>
        /// Check if the player is currently blocking
        /// </summary>
        public bool IsBlocking() {
            return isBlocking;
        }

        /// <summary>
        /// Check if the player is currently dodging
        /// </summary>
        public bool IsDodging() {
            return isDodging;
        }
    }

    /// <summary>
    /// Enumeration of skill types
    /// </summary>
    public enum SkillType {
        Attack,
        Heal,
        Buff,
        Utility
    }

    /// <summary>
    /// Data container for player skills
    /// </summary>
    [System.Serializable]
    public class SkillData {
        public string skillName;
        public string description;
        public Sprite icon;
        public SkillType skillType;
        public float cooldown = 10f;
        public float executionTime = 0.5f;
        public float duration = 1f;
        public float staminaCost = 20f;
        public float power = 25f;
        public float range = 5f;
        public DamageType damageType = DamageType.Physical;
        public GameObject skillEffect;
    }
}