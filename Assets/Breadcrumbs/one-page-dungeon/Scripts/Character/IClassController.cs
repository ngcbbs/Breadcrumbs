namespace Breadcrumbs.Character
{
    /// <summary>
    /// Interface for class-specific behavior controllers
    /// </summary>
    public interface IClassController
    {
        /// <summary>
        /// Gets the character this controller is attached to
        /// </summary>
        ICharacter Character { get; }
        
        /// <summary>
        /// Gets the class type of this controller
        /// </summary>
        ClassType ClassType { get; }
        
        /// <summary>
        /// Initializes the class controller with class-specific settings
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Updates class-specific logic (resource management, etc.)
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        void Update(float deltaTime);
        
        /// <summary>
        /// Activates the class's special ability
        /// </summary>
        /// <returns>True if the ability was successfully activated</returns>
        bool ActivateClassSpecial();
        
        /// <summary>
        /// Applies class-specific bonuses when leveling up
        /// </summary>
        void OnLevelUp();
        
        /// <summary>
        /// Modifies damage based on class specialties
        /// </summary>
        /// <param name="baseDamage">The raw damage value</param>
        /// <param name="damageType">The type of damage</param>
        /// <returns>The modified damage value</returns>
        float ModifyDamageDealt(float baseDamage, DamageType damageType);
        
        /// <summary>
        /// Modifies received damage based on class defenses
        /// </summary>
        /// <param name="incomingDamage">The raw damage value</param>
        /// <param name="damageType">The type of damage</param>
        /// <returns>The modified damage value</returns>
        float ModifyDamageReceived(float incomingDamage, DamageType damageType);
    }
}
