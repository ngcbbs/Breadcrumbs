namespace Breadcrumbs.Character
{
    /// <summary>
    /// Types of stat modifications
    /// </summary>
    public enum StatModifierType {
        Flat = 100,                  // Add/subtract a flat value
        PercentAdditive = 200,       // Add percentage modifiers (stacks additively)
        PercentMultiplicative = 300, // Apply percentage modifiers (stacks multiplicatively)
    }

    /// <summary>
    /// Represents a modification to a character stat
    /// </summary>
    public class StatModifier
    {
        /// <summary>
        /// The value of the modification
        /// </summary>
        public float Value { get; private set; }
        
        /// <summary>
        /// The type of modification
        /// </summary>
        public StatModifierType Type { get; private set; }
        
        /// <summary>
        /// The order in which this modifier should be applied (based on type by default)
        /// </summary>
        public int Order { get; private set; }
        
        /// <summary>
        /// The source of this modifier (item, skill, buff, etc.)
        /// </summary>
        public object Source { get; private set; }
        
        /// <summary>
        /// Creates a stat modifier with a custom order
        /// </summary>
        public StatModifier(float value, StatModifierType type, int order, object source)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }
        
        /// <summary>
        /// Creates a stat modifier with an order based on its type
        /// </summary>
        public StatModifier(float value, StatModifierType type, object source)
            : this(value, type, (int)type, source)
        {
        }
    }
}
