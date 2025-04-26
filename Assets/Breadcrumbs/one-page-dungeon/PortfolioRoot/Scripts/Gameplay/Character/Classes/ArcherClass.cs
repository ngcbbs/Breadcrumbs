using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Character.Classes {
    public class ArcherClass : BaseCharacterClass {
        [Header("Archer Specific Stats")]
        [SerializeField]
        private float rangeBonusMultiplier = 1.3f;
        [SerializeField]
        private float critChance = 0.15f;
        [SerializeField]
        private float critDamageMultiplier = 2.0f;
        [SerializeField]
        private float dodgeChance = 0.1f;

        [Header("Archer Skills VFX")]
        [SerializeField]
        private GameObject powerShotVFX;
        [SerializeField]
        private GameObject multiShotVFX;
        [SerializeField]
        private GameObject explosiveArrowVFX;
        [SerializeField]
        private GameObject hawkeyeVFX;

        [Header("Archer Skill Sounds")]
        [SerializeField]
        private AudioClip powerShotSound;
        [SerializeField]
        private AudioClip multiShotSound;
        [SerializeField]
        private AudioClip explosiveArrowSound;
        [SerializeField]
        private AudioClip hawkeyeSound;

        [Header("Projectile Prefabs")]
        [SerializeField]
        private GameObject arrowPrefab;
        [SerializeField]
        private GameObject explosiveArrowPrefab;

        private bool isHawkeyeActive = false;
        private float hawkeyeDuration = 8f;
        private float hawkeyeCritBonus = 0.25f;

        protected override void Awake() {
            base.Awake();

            // Set archer class identity
            className = "Archer";
            classDescription = "A ranged specialist with high accuracy and critical hit potential.";
        }

        protected override void Start() {
            base.Start();

            // Initialize archer-specific skills
            InitializeArcherSkills();
        }

        private void InitializeArcherSkills() {
            // Clear existing skills list
            classSkills.Clear();

            // Add archer-specific skills
            classSkills.Add(new SkillData {
                skillName = "Power Shot",
                description = "Fire a powerful arrow that deals increased damage and has armor penetration.",
                skillType = SkillType.Attack,
                cooldown = 6f,
                executionTime = 0.8f,
                duration = 1.0f,
                staminaCost = 20f,
                power = 40f,
                range = 20f,
                damageType = DamageType.Physical,
                skillEffect = powerShotVFX
            });

            classSkills.Add(new SkillData {
                skillName = "Multi Shot",
                description = "Fire three arrows in a spread pattern, each dealing reduced damage.",
                skillType = SkillType.Attack,
                cooldown = 10f,
                executionTime = 0.5f,
                duration = 0.8f,
                staminaCost = 25f,
                power = 20f,
                range = 15f,
                damageType = DamageType.Physical,
                skillEffect = multiShotVFX
            });

            classSkills.Add(new SkillData {
                skillName = "Explosive Arrow",
                description = "Fire an arrow that explodes on impact, dealing area damage.",
                skillType = SkillType.Attack,
                cooldown = 15f,
                executionTime = 0.7f,
                duration = 1.0f,
                staminaCost = 30f,
                power = 25f,
                range = 18f,
                damageType = DamageType.Physical,
                skillEffect = explosiveArrowVFX
            });

            classSkills.Add(new SkillData {
                skillName = "Hawkeye",
                description = "Enter a focused state, increasing critical hit chance and attack range.",
                skillType = SkillType.Buff,
                cooldown = 25f,
                executionTime = 0.3f,
                duration = 8f,
                staminaCost = 20f,
                power = 0f,
                range = 0f,
                damageType = DamageType.None,
                skillEffect = hawkeyeVFX
            });
        }

        protected override void InitializeBaseStats() {
            // Apply archer-specific stat modifiers to the base stats
            if (playerStats != null) {
                // Archers have increased range and attack speed but less health
                playerStats.AddStatModifier(StatType.Health, baseHealth * 0.9f - baseHealth);
                playerStats.AddStatModifier(StatType.AttackRange, baseDexterity * 0.2f);
                playerStats.AddStatModifier(StatType.AttackSpeed, 0.2f);
            }
        }

        public override void ApplyClassEffects() {
            // Apply archer-specific effects
            if (playerStats != null) {
                // Archers have increased attack range
                playerStats.AddStatModifier(StatType.AttackRange, playerStats.AttackRange * (rangeBonusMultiplier - 1));
            }
        }

        public override bool TryUseClassSkill(int skillIndex) {
            if (skillIndex < 0 || skillIndex >= classSkills.Count)
                return false;

            switch (skillIndex) {
                case 0: // Power Shot
                    return CastPowerShot();
                case 1: // Multi Shot
                    return CastMultiShot();
                case 2: // Explosive Arrow
                    return CastExplosiveArrow();
                case 3: // Hawkeye
                    return CastHawkeye();
                default:
                    return false;
            }
        }

        private bool CastPowerShot() {
            if (playerCombat != null && playerCombat.UseSkill(0)) {
                StartCoroutine(PowerShotEffect());
                return true;
            }

            return false;
        }

        private IEnumerator PowerShotEffect() {
            // Play archer-specific power shot sound
            if (powerShotSound != null && GetComponent<AudioSource>() != null) {
                GetComponent<AudioSource>().PlayOneShot(powerShotSound);
            }

            // Spawn the arrow projectile
            if (arrowPrefab != null) {
                // Calculate spawn position (forward of the character)
                Vector3 spawnPosition = transform.position + transform.forward * 0.5f + Vector3.up * 1.5f;

                // Instantiate the arrow with forward direction
                GameObject arrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);

                // In a real implementation, we would configure the arrow to deal power shot damage
                // This might be done by getting an Arrow component and setting properties

                // For example:
                // Arrow arrowComponent = arrow.GetComponent<Arrow>();
                // if (arrowComponent != null)
                // {
                //     arrowComponent.damage = classSkills[0].power;
                //     arrowComponent.armorPenetration = 0.5f;
                //     arrowComponent.isCritical = true;
                // }
            }

            yield return null;
        }

        private bool CastMultiShot() {
            if (playerCombat != null && playerCombat.UseSkill(1)) {
                StartCoroutine(MultiShotEffect());
                return true;
            }

            return false;
        }

        private IEnumerator MultiShotEffect() {
            // Play archer-specific multi shot sound
            if (multiShotSound != null && GetComponent<AudioSource>() != null) {
                GetComponent<AudioSource>().PlayOneShot(multiShotSound);
            }

            if (arrowPrefab != null) {
                // Calculate spawn position (forward of the character)
                Vector3 spawnPosition = transform.position + transform.forward * 0.5f + Vector3.up * 1.5f;

                // Spawn center arrow
                GameObject centerArrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);

                // Spawn left arrow (spread angle)
                Quaternion leftRotation = transform.rotation * Quaternion.Euler(0, -15, 0);
                GameObject leftArrow = Instantiate(arrowPrefab, spawnPosition, leftRotation);

                // Spawn right arrow (spread angle)
                Quaternion rightRotation = transform.rotation * Quaternion.Euler(0, 15, 0);
                GameObject rightArrow = Instantiate(arrowPrefab, spawnPosition, rightRotation);

                // In a real implementation, we would configure each arrow with reduced damage
                // Similar to power shot, we would set properties on an Arrow component
            }

            yield return null;
        }

        private bool CastExplosiveArrow() {
            if (playerCombat != null && playerCombat.UseSkill(2)) {
                StartCoroutine(ExplosiveArrowEffect());
                return true;
            }

            return false;
        }

        private IEnumerator ExplosiveArrowEffect() {
            // Play archer-specific explosive arrow sound
            if (explosiveArrowSound != null && GetComponent<AudioSource>() != null) {
                GetComponent<AudioSource>().PlayOneShot(explosiveArrowSound);
            }

            if (explosiveArrowPrefab != null) {
                // Calculate spawn position (forward of the character)
                Vector3 spawnPosition = transform.position + transform.forward * 0.5f + Vector3.up * 1.5f;

                // Instantiate the explosive arrow with forward direction
                GameObject explosiveArrow = Instantiate(explosiveArrowPrefab, spawnPosition, transform.rotation);

                // In a real implementation, we would configure the arrow to explode on impact
                // This might involve setting properties on a specialized ExplosiveArrow component
            }

            yield return null;
        }

        private bool CastHawkeye() {
            if (playerCombat != null && playerCombat.UseSkill(3)) {
                StartCoroutine(HawkeyeEffect());
                return true;
            }

            return false;
        }

        private IEnumerator HawkeyeEffect() {
            // Play archer-specific hawkeye sound
            if (hawkeyeSound != null && GetComponent<AudioSource>() != null) {
                GetComponent<AudioSource>().PlayOneShot(hawkeyeSound);
            }

            // Apply hawkeye buff
            if (playerStats != null) {
                isHawkeyeActive = true;

                // Increase range and critical hit chance
                float rangeBonus = playerStats.AttackRange * 0.3f;

                playerStats.AddStatModifier(StatType.AttackRange, rangeBonus);

                // In a full implementation, we would also affect the critical hit chance
                // This might require adding a critical hit system

                // Visual indicator
                if (hawkeyeVFX != null) {
                    GameObject vfx = Instantiate(hawkeyeVFX, transform);
                    Destroy(vfx, hawkeyeDuration);
                }

                // After duration, remove the effects
                yield return new WaitForSeconds(hawkeyeDuration);

                isHawkeyeActive = false;
                playerStats.RemoveStatModifier(StatType.AttackRange, rangeBonus);

                // In a full implementation, we would also remove the critical hit chance bonus
            } else {
                yield return null;
            }
        }

        public override void OnLevelUp(int newLevel) {
            base.OnLevelUp(newLevel);

            // Apply archer-specific level up effects
            if (playerStats != null) {
                // Archers gain more dexterity per level
                float dexterityBonus = dexterityPerLevel * 1.2f;
                float attackSpeedBonus = 0.05f;

                // In a full implementation, we would update the dexterity stat
                playerStats.AddStatModifier(StatType.AttackSpeed, attackSpeedBonus);
            }

            // Unlock new skills based on level
            if (newLevel == 5 && classSkills.Count >= 2) {
                Debug.Log("todo: Unlock Multi Shot skill");
                //OnSkillUnlocked?.Invoke(classSkills[1]); // Multi Shot at level 5
            } else if (newLevel == 10 && classSkills.Count >= 3) {
                Debug.Log("todo: Unlock Explosive Arrow skill");
                //OnSkillUnlocked?.Invoke(classSkills[2]); // Explosive Arrow at level 10
            } else if (newLevel == 15 && classSkills.Count >= 4) {
                Debug.Log("todo: Unlock Hawkeye skill");
                //OnSkillUnlocked?.Invoke(classSkills[3]); // Hawkeye at level 15
            }
        }
    }
}