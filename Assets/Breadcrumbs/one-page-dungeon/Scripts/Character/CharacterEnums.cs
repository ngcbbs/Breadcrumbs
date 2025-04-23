namespace Breadcrumbs.Character
{
    /// <summary>
    /// Defines damage types in the game
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magic,
        Fire,
        Ice,
        Lightning,
        Earth,
        Holy,
        Shadow,
        True // Ignores resistances
    }
    
    /// <summary>
    /// Defines character genders
    /// </summary>
    public enum GenderType
    {
        Male,
        Female,
        Other
    }
    
    /// <summary>
    /// Defines character classes
    /// </summary>
    public enum ClassType
    {
        None,
        Warrior,
        Mage,
        Rogue,
        Cleric
    }
    
    /// <summary>
    /// Defines equipment slots
    /// </summary>
    public enum EquipmentSlot
    {
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        Shoulders,
        Waist,
        Back,
        Neck,
        Ring1,
        Ring2,
        MainHand,
        OffHand
    }
    
    /// <summary>
    /// Defines stat types
    /// </summary>
    public enum StatType
    {
        // Primary stats
        Strength,     // Physical power
        Dexterity,    // Agility and precision
        Intelligence, // Mental power
        Vitality,     // Health and endurance
        Wisdom,       // Magic endurance and regeneration
        Luck,         // Critical chance and random bonuses
        
        // Derived stats
        MaxHealth,       // Maximum health points
        MaxMana,         // Maximum mana points
        HealthRegen,     // Health regeneration per second
        ManaRegen,       // Mana regeneration per second
        PhysicalAttack,  // Physical damage
        MagicAttack,     // Magical damage
        PhysicalDefense, // Physical damage reduction
        MagicDefense,    // Magic damage reduction
        AttackSpeed,     // Attack rate
        MovementSpeed,   // Movement velocity
        CriticalChance,  // Chance for critical hits
        CriticalDamage,  // Critical hit multiplier
        Accuracy,        // Chance to hit
        Evasion,         // Chance to dodge
    }
}
