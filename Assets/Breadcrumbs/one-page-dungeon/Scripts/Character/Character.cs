using System;
using System.Collections.Generic;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Skills;
using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Core implementation of the ICharacter interface
    /// </summary>
    public class Character : MonoBehaviour, ICharacter
    {
        [Header("Basic Information")]
        [SerializeField] private string characterName;
        [SerializeField] private GenderType gender;
        [SerializeField] private ClassType classType;
        [SerializeField] private ClassData classData;
        
        // Internal state
        private IStatSystem stats;
        private IClassController classController;
        private Dictionary<EquipmentSlot, IEquipmentItem> equippedItems = new Dictionary<EquipmentSlot, IEquipmentItem>();
        private HashSet<string> learnedSkills = new HashSet<string>();
        private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
        
        // Skill service reference
        private ISkillService skillService;
        
        // Events
        public event Action<int> OnLevelUp;
        public event Action<float, DamageType> OnDamageTaken;
        public event Action<float> OnHealed;
        public event Action<IEquipmentItem> OnItemEquipped;
        public event Action<IEquipmentItem, EquipmentSlot> OnItemUnequipped;
        
        // Properties
        public string Name => characterName;
        public int Level => stats.Level;
        public GenderType Gender => gender;
        public ClassType ClassType => classType;
        public IStatSystem Stats => stats;
        public bool IsAlive => stats.CurrentHealth > 0;
        
        private void Awake()
        {
            // Initialize stat system
            stats = new StatSystem();
            stats.OnLevelUp += HandleLevelUp;
        }
        
        private void Start()
        {
            // Look for dependency injection for skill service
            skillService = GetComponent<ISkillService>();
            if (skillService == null)
            {
                Debug.LogWarning("No ISkillService found. Skill functionality will be limited.");
            }
            
            Initialize();
        }
        
        private void Update()
        {
            // Update stats (regeneration, etc.)
            stats.UpdateStats(Time.deltaTime);
            
            // Update class controller
            classController?.Update(Time.deltaTime);
        }
        
        /// <summary>
        /// Initializes the character with class data
        /// </summary>
        public void Initialize()
        {
            if (classData == null)
            {
                Debug.LogError("Class data is not assigned!");
                return;
            }
            
            // Set base stats from class data
            stats.SetBaseStat(StatType.Strength, classData.baseStrength);
            stats.SetBaseStat(StatType.Dexterity, classData.baseDexterity);
            stats.SetBaseStat(StatType.Intelligence, classData.baseIntelligence);
            stats.SetBaseStat(StatType.Vitality, classData.baseVitality);
            stats.SetBaseStat(StatType.Wisdom, classData.baseWisdom);
            stats.SetBaseStat(StatType.Luck, classData.baseLuck);
            
            // Calculate derived stats
            stats.CalculateDerivedStats(classType);
            
            // Initialize health/mana
            stats.InitializeForCombat();
            
            // Create class controller based on class type
            classController = CreateClassController();
            classController?.Initialize();
            
            // Learn starting skills
            LearnStartingSkills();
            
            Debug.Log($"{characterName} initialized as a level {stats.Level} {classType}");
        }
        
        /// <summary>
        /// Creates the appropriate class controller
        /// </summary>
        private IClassController CreateClassController()
        {
            switch (classType)
            {
                case ClassType.Warrior:
                    return new WarriorController(this);
                case ClassType.Mage:
                    // TODO: Implement MageController
                    Debug.LogWarning("MageController not implemented yet.");
                    return null;
                case ClassType.Rogue:
                    // TODO: Implement RogueController
                    Debug.LogWarning("RogueController not implemented yet.");
                    return null;
                case ClassType.Cleric:
                    // TODO: Implement ClericController
                    Debug.LogWarning("ClericController not implemented yet.");
                    return null;
                default:
                    Debug.LogError($"Unknown class type: {classType}");
                    return null;
            }
        }
        
        /// <summary>
        /// Learn the starting skills for this class
        /// </summary>
        private void LearnStartingSkills()
        {
            if (classData == null || classData.startingSkills == null) return;
            
            foreach (var skill in classData.startingSkills)
            {
                LearnSkill(skill.skillId);
            }
        }
        
        /// <summary>
        /// Handles level up events
        /// </summary>
        private void HandleLevelUp(int newLevel)
        {
            // Apply class growth
            stats.AddBonus(StatType.Strength, classData.strengthGrowth);
            stats.AddBonus(StatType.Dexterity, classData.dexterityGrowth);
            stats.AddBonus(StatType.Intelligence, classData.intelligenceGrowth);
            stats.AddBonus(StatType.Vitality, classData.vitalityGrowth);
            stats.AddBonus(StatType.Wisdom, classData.wisdomGrowth);
            stats.AddBonus(StatType.Luck, classData.luckGrowth);
            
            // Recalculate derived stats
            stats.CalculateDerivedStats(classType);
            
            // Notify class controller
            classController?.OnLevelUp();
            
            // Trigger event
            OnLevelUp?.Invoke(newLevel);
            
            Debug.Log($"{characterName} has reached level {newLevel}!");
        }
        
        /// <summary>
        /// Gain experience points
        /// </summary>
        public bool GainExperience(int amount)
        {
            return stats.AddExperience(amount);
        }
        
        /// <summary>
        /// Take damage of a specific type
        /// </summary>
        public void TakeDamage(float amount, DamageType damageType = DamageType.Physical)
        {
            if (!IsAlive) return;
            
            // Let class controller modify incoming damage
            float modifiedDamage = amount;
            if (classController != null)
            {
                modifiedDamage = classController.ModifyDamageReceived(amount, damageType);
            }
            
            // Apply damage
            stats.CurrentHealth -= modifiedDamage;
            
            // Notify listeners
            OnDamageTaken?.Invoke(modifiedDamage, damageType);
            
            // Special class handling (e.g., warrior builds rage)
            if (classController is WarriorController warrior)
            {
                warrior.BuildRage(modifiedDamage);
            }
            
            // Check for death
            if (stats.CurrentHealth <= 0)
            {
                HandleDeath();
            }
            
            Debug.Log($"{characterName} took {modifiedDamage} {damageType} damage. Health: {stats.CurrentHealth}/{stats.GetStat(StatType.MaxHealth)}");
        }
        
        /// <summary>
        /// Restore health
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive) return;
            
            float oldHealth = stats.CurrentHealth;
            stats.CurrentHealth += amount;
            float healedAmount = stats.CurrentHealth - oldHealth;
            
            // Notify listeners
            OnHealed?.Invoke(healedAmount);
            
            Debug.Log($"{characterName} healed for {healedAmount}. Health: {stats.CurrentHealth}/{stats.GetStat(StatType.MaxHealth)}");
        }
        
        /// <summary>
        /// Handle character death
        /// </summary>
        private void HandleDeath()
        {
            Debug.Log($"{characterName} has died!");
            
            // Additional death handling logic can go here
        }
        
        #region Equipment
        
        /// <summary>
        /// Equip an item
        /// </summary>
        public bool EquipItem(IEquipmentItem item)
        {
            if (item == null) return false;
            
            // Check class and level requirements
            if (!CanEquipItem(item))
            {
                Debug.Log($"Cannot equip {item.ItemName}: requirements not met");
                return false;
            }
            
            // If there's already an item in this slot, unequip it first
            EquipmentSlot slot = item.EquipSlot;
            if (equippedItems.TryGetValue(slot, out IEquipmentItem currentItem))
            {
                UnequipItem(slot);
            }
            
            // Equip the new item
            equippedItems[slot] = item;
            
            // Apply item's effects
            item.OnEquip(this);
            
            // Notify listeners
            OnItemEquipped?.Invoke(item);
            
            Debug.Log($"{characterName} equipped {item.ItemName} in {slot} slot");
            
            // Recalculate stats
            stats.CalculateDerivedStats(classType);
            
            return true;
        }
        
        /// <summary>
        /// Unequip an item from a slot
        /// </summary>
        public IEquipmentItem UnequipItem(EquipmentSlot slot)
        {
            if (equippedItems.TryGetValue(slot, out IEquipmentItem item))
            {
                // Remove item from equipped items
                equippedItems.Remove(slot);
                
                // Remove item's effects
                item.OnUnequip(this);
                
                // Notify listeners
                OnItemUnequipped?.Invoke(item, slot);
                
                Debug.Log($"{characterName} unequipped {item.ItemName} from {slot} slot");
                
                // Recalculate stats
                stats.CalculateDerivedStats(classType);
                
                return item;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get the item equipped in a slot
        /// </summary>
        public IEquipmentItem GetEquippedItem(EquipmentSlot slot)
        {
            if (equippedItems.TryGetValue(slot, out IEquipmentItem item))
            {
                return item;
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if an item can be equipped
        /// </summary>
        private bool CanEquipItem(IEquipmentItem item)
        {
            // Check level requirement
            if (stats.Level < item.RequiredLevel)
            {
                return false;
            }
            
            // Check class requirement
            if (item.ClassRequirement != ClassType.None && item.ClassRequirement != classType)
            {
                return false;
            }
            
            // Check weapon/armor type compatibility
            // This could be expanded based on the item type system
            
            return true;
        }
        
        #endregion
        
        #region Skills
        
        /// <summary>
        /// Learn a new skill
        /// </summary>
        public bool LearnSkill(string skillId)
        {
            // Check if already learned
            if (learnedSkills.Contains(skillId))
            {
                Debug.Log($"{characterName} already knows skill: {skillId}");
                return false;
            }
            
            // Delegate to skill service if available
            if (skillService != null)
            {
                bool success = skillService.LearnSkill(this, skillId);
                if (success)
                {
                    learnedSkills.Add(skillId);
                    skillLevels[skillId] = 1;
                }
                return success;
            }
            
            // Fallback simple implementation
            learnedSkills.Add(skillId);
            skillLevels[skillId] = 1;
            Debug.Log($"{characterName} learned skill: {skillId}");
            return true;
        }
        
        /// <summary>
        /// Use a skill
        /// </summary>
        public bool UseSkill(string skillId, Transform target = null)
        {
            // Check if skill is learned
            if (!learnedSkills.Contains(skillId))
            {
                Debug.Log($"{characterName} doesn't know skill: {skillId}");
                return false;
            }
            
            // Delegate to skill service if available
            if (skillService != null)
            {
                return skillService.UseSkill(this, skillId, target);
            }
            
            // Fallback simple implementation
            Debug.Log($"{characterName} used skill: {skillId}");
            return true;
        }
        
        /// <summary>
        /// Upgrade a skill
        /// </summary>
        public bool UpgradeSkill(string skillId)
        {
            // Check if skill is learned
            if (!learnedSkills.Contains(skillId))
            {
                Debug.Log($"{characterName} doesn't know skill: {skillId}");
                return false;
            }
            
            // Delegate to skill service if available
            if (skillService != null)
            {
                bool success = skillService.UpgradeSkill(this, skillId);
                if (success && skillLevels.ContainsKey(skillId))
                {
                    skillLevels[skillId]++;
                }
                return success;
            }
            
            // Fallback simple implementation
            if (skillLevels.ContainsKey(skillId))
            {
                skillLevels[skillId]++;
                Debug.Log($"{characterName} upgraded skill: {skillId} to level {skillLevels[skillId]}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get the level of a skill
        /// </summary>
        public int GetSkillLevel(string skillId)
        {
            if (skillLevels.TryGetValue(skillId, out int level))
            {
                return level;
            }
            
            return 0;
        }
        
        #endregion
    }
}
