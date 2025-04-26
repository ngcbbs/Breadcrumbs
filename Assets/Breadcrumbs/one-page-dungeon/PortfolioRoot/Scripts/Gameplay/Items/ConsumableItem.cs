using UnityEngine;
using System.Collections.Generic;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Types of consumable items
    /// </summary>
    public enum ConsumableType
    {
        HealthPotion,
        ManaPotion,
        StaminaPotion,
        Food,
        Elixir,
        Scroll,
        Throwable,
        Trap
    }
    
    /// <summary>
    /// Class representing consumable items that can be used for various effects
    /// </summary>
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
    public class ConsumableItem : Item
    {
        [Header("Consumable Properties")]
        public ConsumableType consumableType;
        public bool isConsumedOnUse = true;
        public float cooldown = 0f;
        
        [Header("Effect Settings")]
        public List<ConsumableEffect> effects = new List<ConsumableEffect>();
        public float effectDuration = 0f;
        public bool applyOverTime = false;
        public float tickRate = 1f;
        
        [Header("Consumable Effects")]
        public GameObject consumeEffect;
        public GameObject persistentEffect;
        
        /// <summary>
        /// Use the consumable item
        /// </summary>
        public override bool Use(GameObject user)
        {
            // Check if on cooldown
            InventoryManager inventory = user.GetComponent<InventoryManager>();
            if (inventory != null && inventory.IsItemOnCooldown(this))
            {
                Debug.Log($"Cannot use {itemName} - on cooldown");
                return false;
            }
            
            // Apply effects
            bool effectApplied = ApplyEffects(user);
            
            if (effectApplied)
            {
                // Base visual and sound effects
                base.Use(user);
                
                // Consume effect
                if (consumeEffect != null)
                {
                    Instantiate(consumeEffect, user.transform.position, user.transform.rotation);
                }
                
                // Persistent effect
                if (persistentEffect != null && effectDuration > 0f)
                {
                    GameObject effect = Instantiate(persistentEffect, user.transform.position, user.transform.rotation);
                    effect.transform.SetParent(user.transform);
                    
                    // Destroy after duration
                    Object.Destroy(effect, effectDuration);
                }
                
                // Handle cooldown
                if (cooldown > 0f && inventory != null)
                {
                    inventory.StartItemCooldown(this, cooldown);
                }
                
                // Return whether item should be consumed
                return isConsumedOnUse;
            }
            
            return false;
        }
        
        /// <summary>
        /// Apply all effects of the consumable
        /// </summary>
        protected virtual bool ApplyEffects(GameObject user)
        {
            if (effects.Count == 0)
                return false;
                
            bool anyEffectApplied = false;
            
            foreach (var effect in effects)
            {
                if (applyOverTime && effectDuration > 0f)
                {
                    // Start coroutine for over-time effects
                    StatusEffectManager statusManager = user.GetComponent<StatusEffectManager>();
                    if (statusManager != null)
                    {
                        statusManager.ApplyStatusEffect(new StatusEffect
                        {
                            name = effect.effectName,
                            duration = effectDuration,
                            tickRate = tickRate,
                            effectType = (StatusEffectType)effect.effectType,
                            value = effect.value,
                            icon = icon
                        });
                        
                        anyEffectApplied = true;
                    }
                    else
                    {
                        // Fallback if no status manager - apply once
                        anyEffectApplied |= ApplySingleEffect(user, effect);
                    }
                }
                else
                {
                    // Instant effect
                    anyEffectApplied |= ApplySingleEffect(user, effect);
                }
            }
            
            return anyEffectApplied;
        }
        
        /// <summary>
        /// Apply a single effect
        /// </summary>
        protected virtual bool ApplySingleEffect(GameObject user, ConsumableEffect effect)
        {
            switch (effect.effectType)
            {
                case ConsumableEffectType.RestoreHealth:
                    HealthSystem health = user.GetComponent<HealthSystem>();
                    if (health != null)
                    {
                        health.Heal(effect.value);
                        return true;
                    }
                    break;
                    
                case ConsumableEffectType.RestoreMana:
                    // Mana system would be implemented in a full game
                    Debug.Log($"Restored {effect.value} mana to {user.name}");
                    return true;
                    
                case ConsumableEffectType.RestoreStamina:
                    StaminaSystem stamina = user.GetComponent<StaminaSystem>();
                    if (stamina != null)
                    {
                        stamina.AddStamina(effect.value);
                        return true;
                    }
                    break;
                    
                case ConsumableEffectType.TemporaryDamageBoost:
                    // Apply stat buff
                    CharacterStats stats = user.GetComponent<CharacterStats>();
                    if (stats != null)
                    {
                        stats.AddTemporaryStatModifier(StatType.PhysicalDamage, ModifierType.Percentage, effect.value, effectDuration);
                        return true;
                    }
                    break;
                    
                case ConsumableEffectType.TemporaryDefenseBoost:
                    // Apply stat buff
                    CharacterStats defStats = user.GetComponent<CharacterStats>();
                    if (defStats != null)
                    {
                        defStats.AddTemporaryStatModifier(StatType.Defense, ModifierType.Percentage, effect.value, effectDuration);
                        return true;
                    }
                    break;
                    
                case ConsumableEffectType.TemporarySpeedBoost:
                    // Apply stat buff
                    CharacterStats speedStats = user.GetComponent<CharacterStats>();
                    if (speedStats != null)
                    {
                        speedStats.AddTemporaryStatModifier(StatType.MovementSpeed, ModifierType.Percentage, effect.value, effectDuration);
                        return true;
                    }
                    break;
                    
                case ConsumableEffectType.Cure:
                    // Remove negative status effects
                    StatusEffectManager status = user.GetComponent<StatusEffectManager>();
                    if (status != null)
                    {
                        status.RemoveAllNegativeEffects();
                        return true;
                    }
                    break;
                    
                case ConsumableEffectType.AreaDamage:
                    // Apply area damage around user
                    Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, effect.radius, effect.targetLayers);
                    
                    bool hitAny = false;
                    foreach (var hitCollider in hitColliders)
                    {
                        // Skip user
                        if (hitCollider.gameObject == user)
                            continue;
                            
                        IDamageable target = hitCollider.GetComponent<IDamageable>();
                        if (target != null)
                        {
                            target.TakeDamage(effect.value, effect.damageType, user);
                            hitAny = true;
                        }
                    }
                    
                    return hitAny;
                    
                case ConsumableEffectType.Teleport:
                    // Could implement teleportation logic here
                    Debug.Log($"Teleport effect not fully implemented");
                    return true;
                    
                case ConsumableEffectType.Invulnerability:
                    HealthSystem invHealth = user.GetComponent<HealthSystem>();
                    if (invHealth != null)
                    {
                        invHealth.AddInvulnerability(effectDuration);
                        return true;
                    }
                    break;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get tooltip text with effect descriptions
        /// </summary>
        public override string GetTooltipText()
        {
            string baseTooltip = base.GetTooltipText();
            
            string effectsText = "\n<color=lime>Effects:</color>\n";
            
            foreach (var effect in effects)
            {
                effectsText += $"{effect.effectName}: {effect.description}\n";
            }
            
            if (effectDuration > 0f)
            {
                effectsText += $"\nDuration: {effectDuration} seconds\n";
                
                if (applyOverTime)
                {
                    effectsText += $"Applies over time\n";
                }
            }
            
            if (cooldown > 0f)
            {
                effectsText += $"Cooldown: {cooldown} seconds\n";
            }
            
            return baseTooltip + effectsText;
        }
    }
    
    /// <summary>
    /// Types of effects consumables can have
    /// </summary>
    public enum ConsumableEffectType
    {
        RestoreHealth,
        RestoreMana,
        RestoreStamina,
        TemporaryDamageBoost,
        TemporaryDefenseBoost,
        TemporarySpeedBoost,
        Cure,
        AreaDamage,
        Teleport,
        Invulnerability
    }
    
    /// <summary>
    /// Data container for consumable effects
    /// </summary>
    [System.Serializable]
    public class ConsumableEffect
    {
        public string effectName;
        [TextArea(2, 4)]
        public string description;
        public ConsumableEffectType effectType;
        public float value;
        
        // For area effects
        public float radius = 5f;
        public LayerMask targetLayers;
        public DamageType damageType = DamageType.Physical;
    }
}
