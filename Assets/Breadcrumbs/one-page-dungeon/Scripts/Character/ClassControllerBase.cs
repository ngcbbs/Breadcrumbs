using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Base implementation for class-specific controllers
    /// </summary>
    public abstract class ClassControllerBase : IClassController
    {
        /// <summary>
        /// The character this controller is attached to
        /// </summary>
        protected ICharacter character;
        
        /// <summary>
        /// The stat system of the character
        /// </summary>
        protected IStatSystem stats;
        
        /// <summary>
        /// The class type of this controller
        /// </summary>
        protected ClassType classType;
        
        /// <summary>
        /// Gets the character this controller is attached to
        /// </summary>
        public ICharacter Character => character;
        
        /// <summary>
        /// Gets the class type of this controller
        /// </summary>
        public ClassType ClassType => classType;
        
        /// <summary>
        /// Creates a new class controller for the given character
        /// </summary>
        /// <param name="character">The character to control</param>
        /// <param name="classType">The type of class</param>
        protected ClassControllerBase(ICharacter character, ClassType classType)
        {
            this.character = character;
            this.stats = character.Stats;
            this.classType = classType;
        }
        
        /// <summary>
        /// Initializes the class controller with class-specific settings
        /// </summary>
        public virtual void Initialize()
        {
            // Base initialization logic can go here
            Debug.Log($"Initializing {classType} controller for {character.Name}");
        }
        
        /// <summary>
        /// Updates class-specific logic (resource management, etc.)
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public virtual void Update(float deltaTime)
        {
            // Base update logic can go here
        }
        
        /// <summary>
        /// Activates the class's special ability
        /// </summary>
        /// <returns>True if the ability was successfully activated</returns>
        public abstract bool ActivateClassSpecial();
        
        /// <summary>
        /// Applies class-specific bonuses when leveling up
        /// </summary>
        public virtual void OnLevelUp()
        {
            // Base level up logic can go here
            Debug.Log($"{character.Name} has reached level {character.Level} as a {classType}");
        }
        
        /// <summary>
        /// Modifies damage based on class specialties
        /// </summary>
        /// <param name="baseDamage">The raw damage value</param>
        /// <param name="damageType">The type of damage</param>
        /// <returns>The modified damage value</returns>
        public virtual float ModifyDamageDealt(float baseDamage, DamageType damageType)
        {
            // By default, return the base damage value
            return baseDamage;
        }
        
        /// <summary>
        /// Modifies received damage based on class defenses
        /// </summary>
        /// <param name="incomingDamage">The raw damage value</param>
        /// <param name="damageType">The type of damage</param>
        /// <returns>The modified damage value</returns>
        public virtual float ModifyDamageReceived(float incomingDamage, DamageType damageType)
        {
            // By default, return the incoming damage value
            return incomingDamage;
        }
        
        /// <summary>
        /// Creates a stat modifier for the class
        /// </summary>
        /// <param name="value">The modifier value</param>
        /// <param name="type">The modifier type</param>
        /// <returns>A new stat modifier</returns>
        protected StatModifier CreateModifier(float value, StatModifierType type)
        {
            return new StatModifier(value, type, this);
        }
    }
}
