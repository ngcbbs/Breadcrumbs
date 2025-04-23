using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Controller for the Warrior class
    /// </summary>
    public class WarriorController : ClassControllerBase
    {
        private float rageMeter = 0f;
        private float maxRage = 100f;
        private float rageDecayRate = 5f;    // Rage lost per second
        private float rageBuildupRate = 10f; // Rage gained per damage point
        private float rageActionCost = 25f;  // Minimum rage needed for special
        
        /// <summary>
        /// Gets the current rage value
        /// </summary>
        public float RageMeter => rageMeter;
        
        /// <summary>
        /// Gets the maximum rage value
        /// </summary>
        public float MaxRage => maxRage;
        
        /// <summary>
        /// Creates a new warrior controller
        /// </summary>
        /// <param name="character">The character to control</param>
        public WarriorController(ICharacter character) : base(character, ClassType.Warrior)
        {
        }
        
        /// <summary>
        /// Initializes warrior-specific settings
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            // Apply warrior passive bonuses
            float strengthBonus = character.Level * 0.2f;
            stats.AddModifier(StatType.Strength, CreateModifier(strengthBonus, StatModifierType.Flat));
            
            // Apply physical defense bonus
            float defenseBonus = character.Level * 0.5f;
            stats.AddModifier(StatType.PhysicalDefense, CreateModifier(defenseBonus, StatModifierType.Flat));
            
            Debug.Log($"Warrior passive: +{strengthBonus} Strength, +{defenseBonus} Physical Defense");
        }
        
        /// <summary>
        /// Updates warrior-specific logic
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            
            // Natural rage decay
            if (rageMeter > 0)
            {
                rageMeter = Mathf.Max(0, rageMeter - rageDecayRate * deltaTime);
            }
        }
        
        /// <summary>
        /// Activates Berserker Rage - increases attack but reduces defense
        /// </summary>
        /// <returns>True if activated successfully</returns>
        public override bool ActivateClassSpecial()
        {
            // Check if enough rage is available
            if (rageMeter >= rageActionCost)
            {
                float ragePercentage = rageMeter / maxRage;
                float attackBonus = 0.5f * ragePercentage;     // Up to +50% attack
                float defensePenalty = 0.25f * ragePercentage; // Up to -25% defense
                
                // Apply buffs for 10 seconds
                BuffSystem.ApplyTemporaryBuff(character, StatType.PhysicalAttack, attackBonus, 
                    StatModifierType.PercentAdditive, 10f);
                    
                BuffSystem.ApplyTemporaryBuff(character, StatType.PhysicalDefense, -defensePenalty, 
                    StatModifierType.PercentAdditive, 10f);
                
                // Consume rage
                rageMeter *= 0.5f; // Use 50% of current rage
                
                Debug.Log($"Berserker Rage activated! Attack +{attackBonus*100}%, Defense -{defensePenalty*100}%");
                return true;
            }
            
            Debug.Log("Not enough rage to activate Berserker Rage.");
            return false;
        }
        
        /// <summary>
        /// Apply warrior-specific level up bonuses
        /// </summary>
        public override void OnLevelUp()
        {
            base.OnLevelUp();
            
            // Increase rage cap
            maxRage += 5f;
            
            // Add stat bonuses
            stats.AddBonus(StatType.Strength, 2f);
            stats.AddBonus(StatType.Vitality, 1.5f);
            
            Debug.Log($"Warrior level up bonuses: Max Rage +5, Strength +2, Vitality +1.5");
        }
        
        /// <summary>
        /// Modifies damage based on warrior specialties
        /// </summary>
        public override float ModifyDamageDealt(float baseDamage, DamageType damageType)
        {
            // Warriors deal more physical damage
            if (damageType == DamageType.Physical)
            {
                return baseDamage * 1.1f; // 10% bonus to physical damage
            }
            
            return base.ModifyDamageDealt(baseDamage, damageType);
        }
        
        /// <summary>
        /// Modifies received damage based on warrior defenses
        /// </summary>
        public override float ModifyDamageReceived(float incomingDamage, DamageType damageType)
        {
            // Warriors take less physical damage but more magic damage
            switch (damageType)
            {
                case DamageType.Physical:
                    return incomingDamage * 0.9f; // 10% resistance to physical
                    
                case DamageType.Magic:
                case DamageType.Fire:
                case DamageType.Ice:
                case DamageType.Lightning:
                    return incomingDamage * 1.1f; // 10% weakness to magical
                    
                default:
                    return base.ModifyDamageReceived(incomingDamage, damageType);
            }
        }
        
        /// <summary>
        /// Build rage when taking damage
        /// </summary>
        /// <param name="damageTaken">Amount of damage received</param>
        public void BuildRage(float damageTaken)
        {
            float rageGain = damageTaken * rageBuildupRate * 0.1f;
            rageMeter = Mathf.Min(maxRage, rageMeter + rageGain);
            
            Debug.Log($"Rage built: +{rageGain}, Current: {rageMeter}/{maxRage}");
        }
    }
}
