using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Static utility class for applying buffs to characters
    /// This is a simplified version for the refactored system
    /// In a complete implementation, this would be replaced with a more robust buff system
    /// </summary>
    public static class BuffSystem
    {
        /// <summary>
        /// Applies a temporary buff to a character
        /// </summary>
        /// <param name="character">The character to buff</param>
        /// <param name="statType">The stat to modify</param>
        /// <param name="value">The modifier value</param>
        /// <param name="modType">The modifier type</param>
        /// <param name="duration">Duration in seconds</param>
        /// <returns>A buff identifier (can be used to track active buffs)</returns>
        public static string ApplyTemporaryBuff(ICharacter character, StatType statType, float value, 
            StatModifierType modType, float duration)
        {
            // Create a unique ID for this buff
            string buffId = $"Buff_{statType}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            
            // Create the modifier
            StatModifier modifier = new StatModifier(value, modType, buffId);
            
            // Apply the modifier
            character.Stats.AddModifier(statType, modifier);
            
            // Schedule removal after duration
            GameObject serviceObject = new GameObject($"BuffTimer_{buffId}");
            BuffTimer timer = serviceObject.AddComponent<BuffTimer>();
            timer.Initialize(character, statType, buffId, duration);
            
            Debug.Log($"Applied temporary {statType} buff: {value} for {duration} seconds");
            
            return buffId;
        }
        
        /// <summary>
        /// Applies a permanent buff to a character
        /// </summary>
        /// <param name="character">The character to buff</param>
        /// <param name="statType">The stat to modify</param>
        /// <param name="value">The modifier value</param>
        /// <param name="modType">The modifier type</param>
        /// <returns>The source object for the buff</returns>
        public static object ApplyPermanentBuff(ICharacter character, StatType statType, float value, StatModifierType modType)
        {
            // Create a source object for the buff
            object source = new PermanentBuffSource($"{statType}_{value}");
            
            // Create the modifier
            StatModifier modifier = new StatModifier(value, modType, source);
            
            // Apply the modifier
            character.Stats.AddModifier(statType, modifier);
            
            Debug.Log($"Applied permanent {statType} buff: {value}");
            
            return source;
        }
        
        /// <summary>
        /// Removes all buffs from a character with the given source
        /// </summary>
        /// <param name="character">The character</param>
        /// <param name="source">The buff source to remove</param>
        public static void RemoveBuff(ICharacter character, object source)
        {
            character.Stats.RemoveAllModifiersFromSource(source);
        }
        
        /// <summary>
        /// Helper class to identify permanent buff sources
        /// </summary>
        private class PermanentBuffSource
        {
            public string BuffId { get; }
            
            public PermanentBuffSource(string buffId)
            {
                BuffId = buffId;
            }
            
            public override string ToString()
            {
                return $"PermanentBuff[{BuffId}]";
            }
        }
    }
    
    /// <summary>
    /// Helper MonoBehaviour to manage buff duration
    /// </summary>
    public class BuffTimer : MonoBehaviour
    {
        private ICharacter character;
        private StatType statType;
        private string buffId;
        private float duration;
        private float timeRemaining;
        
        /// <summary>
        /// Initializes the buff timer
        /// </summary>
        public void Initialize(ICharacter character, StatType statType, string buffId, float duration)
        {
            this.character = character;
            this.statType = statType;
            this.buffId = buffId;
            this.duration = duration;
            this.timeRemaining = duration;
            
            // Don't destroy on scene load
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0)
            {
                // Remove the buff
                character.Stats.RemoveAllModifiersFromSource(buffId);
                
                // Clean up
                Destroy(gameObject);
            }
        }
    }
}
