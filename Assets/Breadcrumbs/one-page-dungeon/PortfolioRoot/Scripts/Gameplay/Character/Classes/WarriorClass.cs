using System.Collections;
using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Character.Classes
{
    public class WarriorClass : BaseCharacterClass
    {
        [Header("Warrior Specific Stats")]
        [SerializeField] private float armorBonus = 20f;
        [SerializeField] private float critChance = 0.1f;
        [SerializeField] private float critDamageMultiplier = 1.5f;
        
        [Header("Warrior Skills VFX")]
        [SerializeField] private GameObject whirlwindVFX;
        [SerializeField] private GameObject battleCryVFX;
        [SerializeField] private GameObject shieldBashVFX;
        [SerializeField] private GameObject berserkerRageVFX;
        
        [Header("Warrior Skill Sounds")]
        [SerializeField] private AudioClip whirlwindSound;
        [SerializeField] private AudioClip battleCrySound;
        [SerializeField] private AudioClip shieldBashSound;
        [SerializeField] private AudioClip berserkerRageSound;
        
        private bool isBerserkerRageActive = false;
        private float berserkerRageDuration = 10f;
        private float berserkerRageDamageBonus = 0.5f;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Set warrior class identity
            className = "Warrior";
            classDescription = "A melee specialist with high defense and powerful close-range attacks.";
        }
        
        protected override void Start()
        {
            base.Start();
            
            // Initialize warrior-specific skills
            InitializeWarriorSkills();
        }
        
        private void InitializeWarriorSkills()
        {
            // Clear existing skills list
            classSkills.Clear();
            
            // Add warrior-specific skills
            classSkills.Add(new SkillData
            {
                skillName = "Whirlwind",
                description = "Spin in a circle, dealing damage to all enemies within range.",
                skillType = SkillType.Attack,
                cooldown = 8f,
                executionTime = 0.5f,
                duration = 1.2f,
                staminaCost = 25f,
                power = 30f,
                range = 4f,
                damageType = DamageType.Physical,
                skillEffect = whirlwindVFX
            });
            
            classSkills.Add(new SkillData
            {
                skillName = "Battle Cry",
                description = "Increase attack power and intimidate nearby enemies.",
                skillType = SkillType.Buff,
                cooldown = 15f,
                executionTime = 0.3f,
                duration = 0.5f,
                staminaCost = 15f,
                power = 0f,
                range = 8f,
                damageType = DamageType.None,
                skillEffect = battleCryVFX
            });
            
            classSkills.Add(new SkillData
            {
                skillName = "Shield Bash",
                description = "Stun an enemy and deal moderate damage.",
                skillType = SkillType.Attack,
                cooldown = 10f,
                executionTime = 0.3f,
                duration = 0.8f,
                staminaCost = 20f,
                power = 20f,
                range = 2f,
                damageType = DamageType.Physical,
                skillEffect = shieldBashVFX
            });
            
            classSkills.Add(new SkillData
            {
                skillName = "Berserker Rage",
                description = "Enter a rage state, increasing damage but decreasing defense.",
                skillType = SkillType.Buff,
                cooldown = 30f,
                executionTime = 0.5f,
                duration = 10f,
                staminaCost = 30f,
                power = 0f,
                range = 0f,
                damageType = DamageType.None,
                skillEffect = berserkerRageVFX
            });
        }
        
        protected override void InitializeBaseStats()
        {
            // Apply warrior-specific stat modifiers to the base stats
            if (playerStats != null)
            {
                // Warriors have bonus health and strength
                playerStats.AddStatModifier(StatType.Health, baseHealth * 1.2f - baseHealth);
                playerStats.AddStatModifier(StatType.AttackDamage, baseStrength * 0.5f);
            }
        }
        
        public override void ApplyClassEffects()
        {
            // Apply warrior-specific effects
            if (playerStats != null)
            {
                // Warriors have additional armor
                // In a full implementation, we would add the armor stat to the player stats
            }
        }
        
        public override bool TryUseClassSkill(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= classSkills.Count)
                return false;
                
            switch (skillIndex)
            {
                case 0: // Whirlwind
                    return CastWhirlwind();
                case 1: // Battle Cry
                    return CastBattleCry();
                case 2: // Shield Bash
                    return CastShieldBash();
                case 3: // Berserker Rage
                    return CastBerserkerRage();
                default:
                    return false;
            }
        }
        
        private bool CastWhirlwind()
        {
            if (playerCombat != null && playerCombat.UseSkill(0))
            {
                StartCoroutine(WhirlwindEffect());
                return true;
            }
            return false;
        }
        
        private IEnumerator WhirlwindEffect()
        {
            // Play warrior-specific whirlwind sound
            if (whirlwindSound != null && GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().PlayOneShot(whirlwindSound);
            }
            
            // Perform whirlwind attack logic
            // In a real implementation, this would damage all enemies in a radius and
            // potentially apply a status effect
            
            yield return new WaitForSeconds(1.2f);
        }
        
        private bool CastBattleCry()
        {
            if (playerCombat != null && playerCombat.UseSkill(1))
            {
                StartCoroutine(BattleCryEffect());
                return true;
            }
            return false;
        }
        
        private IEnumerator BattleCryEffect()
        {
            // Play warrior-specific battle cry sound
            if (battleCrySound != null && GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().PlayOneShot(battleCrySound);
            }
            
            // Apply a temporary attack boost
            if (playerStats != null)
            {
                float damageBoost = 10f;
                playerStats.AddStatModifier(StatType.AttackDamage, damageBoost);
                
                // After duration, remove the boost
                yield return new WaitForSeconds(5f);
                
                playerStats.RemoveStatModifier(StatType.AttackDamage, damageBoost);
            }
            else
            {
                yield return null;
            }
        }
        
        private bool CastShieldBash()
        {
            if (playerCombat != null && playerCombat.UseSkill(2))
            {
                StartCoroutine(ShieldBashEffect());
                return true;
            }
            return false;
        }
        
        private IEnumerator ShieldBashEffect()
        {
            // Play warrior-specific shield bash sound
            if (shieldBashSound != null && GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().PlayOneShot(shieldBashSound);
            }
            
            // Shield bash would stun an enemy
            // In a real implementation, this would:
            // 1. Raycast forward to find an enemy
            // 2. Apply damage
            // 3. Apply a stun status effect
            
            yield return null;
        }
        
        private bool CastBerserkerRage()
        {
            if (playerCombat != null && playerCombat.UseSkill(3))
            {
                StartCoroutine(BerserkerRageEffect());
                return true;
            }
            return false;
        }
        
        private IEnumerator BerserkerRageEffect()
        {
            // Play warrior-specific berserker rage sound
            if (berserkerRageSound != null && GetComponent<AudioSource>() != null)
            {
                GetComponent<AudioSource>().PlayOneShot(berserkerRageSound);
            }
            
            // Apply the berserker rage effect
            if (playerStats != null)
            {
                isBerserkerRageActive = true;
                
                // Increase damage but decrease defense
                float damageBoost = baseStrength * berserkerRageDamageBonus;
                float armorPenalty = -armorBonus * 0.5f;
                
                playerStats.AddStatModifier(StatType.AttackDamage, damageBoost);
                
                // In a real implementation, we would also affect the armor stat
                
                // Visual indicator
                if (berserkerRageVFX != null)
                {
                    GameObject vfx = Instantiate(berserkerRageVFX, transform);
                    Destroy(vfx, berserkerRageDuration);
                }
                
                // After duration, remove the effects
                yield return new WaitForSeconds(berserkerRageDuration);
                
                isBerserkerRageActive = false;
                playerStats.RemoveStatModifier(StatType.AttackDamage, damageBoost);
                
                // In a real implementation, we would also remove the armor penalty
            }
            else
            {
                yield return null;
            }
        }
        
        public override void OnLevelUp(int newLevel)
        {
            base.OnLevelUp(newLevel);
            
            // Apply warrior-specific level up effects
            if (playerStats != null)
            {
                // Warriors gain more strength per level
                float strengthBonus = strengthPerLevel * 1.2f;
                float healthBonus = healthPerLevel * 1.1f;
                
                playerStats.AddStatModifier(StatType.Health, healthBonus);
                
                // In a full implementation, we would also update the strength stat
            }
            
            // Unlock new skills based on level
            if (newLevel == 5 && classSkills.Count >= 2)
            {
                Debug.Log("todo: WarriorClass.OnLevelUp: Unlocking Battle Cry skill");
                //OnSkillUnlocked?.Invoke(classSkills[1]); // Battle Cry at level 5
            }
            else if (newLevel == 10 && classSkills.Count >= 3)
            {
                Debug.Log("todo: WarriorClass.OnLevelUp: Unlocking Shield Bash skill");
                //OnSkillUnlocked?.Invoke(classSkills[2]); // Shield Bash at level 10
            }
            else if (newLevel == 15 && classSkills.Count >= 4)
            {
                Debug.Log("todo: WarriorClass.OnLevelUp: Unlocking Berserker Rage skill");
                //OnSkillUnlocked?.Invoke(classSkills[3]); // Berserker Rage at level 15
            }
        }
    }
}
