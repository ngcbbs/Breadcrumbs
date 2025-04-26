using UnityEngine;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.UI.Components
{
    /// <summary>
    /// Combat log entry types
    /// </summary>
    public enum CombatLogEntryType
    {
        Damage,
        Healing,
        Buff,
        Debuff,
        Item,
        Miss,
        CombatState
    }
    
    /// <summary>
    /// Data structure for a combat log entry
    /// </summary>
    [System.Serializable]
    public class CombatLogEntry
    {
        // Basic info
        public CombatLogEntryType Type;
        public float Timestamp;
        
        // Entity information
        public string SourceName;
        public string TargetName;
        public bool IsPlayerInvolved;
        
        // Damage/healing info
        public int Amount;
        public DamageType DamageType;
        public bool IsCritical;
        public bool IsResisted;
        public bool IsVulnerable;
        
        // Effect info
        public string EffectName;
        public string EffectDescription;
        public float Duration;
        
        // Attack info
        public AttackType AttackType;
        
        // General message
        public string Message;
    }
}