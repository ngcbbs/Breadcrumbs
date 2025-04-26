using System;
using GamePortfolio.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.UI.Components
{
    /// <summary>
    /// UI component for a single combat log entry
    /// </summary>
    public class CombatLogEntryUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Text entryText;
        [SerializeField] private Text timestampText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject expandedPanel;
        [SerializeField] private Text detailsText;
        
        [Header("Type Icons")]
        [SerializeField] private Sprite damageIcon;
        [SerializeField] private Sprite criticalIcon;
        [SerializeField] private Sprite healingIcon;
        [SerializeField] private Sprite buffIcon;
        [SerializeField] private Sprite debuffIcon;
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private Sprite missIcon;
        [SerializeField] private Sprite systemIcon;
        
        [Header("Type Colors")]
        [SerializeField] private Color damageColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color criticalColor = new Color(1.0f, 0.0f, 0.0f, 0.6f);
        [SerializeField] private Color healingColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color buffColor = new Color(0.2f, 0.6f, 0.8f, 0.5f);
        [SerializeField] private Color debuffColor = new Color(0.8f, 0.2f, 0.8f, 0.5f);
        [SerializeField] private Color itemColor = new Color(0.8f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color missColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color systemColor = new Color(0.3f, 0.3f, 0.8f, 0.5f);
        
        private CombatLogEntry entry;
        private bool isExpanded = false;
        
        /// <summary>
        /// Set the entry data and update UI
        /// </summary>
        public void SetEntry(CombatLogEntry logEntry)
        {
            entry = logEntry;
            
            // Set entry text
            if (entryText != null)
            {
                entryText.text = FormatEntryText(entry);
                
                // Set color based on critical
                if (entry.IsCritical)
                {
                    entryText.color = Color.red;
                    entryText.fontStyle = FontStyle.Bold;
                }
            }
            
            // Set timestamp
            if (timestampText != null)
            {
                float gameTime = entry.Timestamp;
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timestampText.text = $"{minutes:00}:{seconds:00}";
            }
            
            // Set background color based on type
            if (backgroundImage != null)
            {
                backgroundImage.color = GetEntryColor(entry);
            }
            
            // Set icon based on type
            if (iconImage != null)
            {
                iconImage.sprite = GetEntryIcon(entry);
            }
            
            // Hide expanded panel initially
            if (expandedPanel != null)
            {
                expandedPanel.SetActive(false);
            }
            
            // Set details text
            if (detailsText != null)
            {
                detailsText.text = FormatDetailsText(entry);
            }
        }
        
        /// <summary>
        /// Format the main entry text
        /// </summary>
        private string FormatEntryText(CombatLogEntry entry)
        {
            switch (entry.Type)
            {
                case CombatLogEntryType.Damage:
                    string criticalText = entry.IsCritical ? " <color=red>CRITICAL</color>" : "";
                    return $"{entry.SourceName} deals {entry.Amount} {entry.DamageType.ToString()} damage to {entry.TargetName}{criticalText}";
                    
                case CombatLogEntryType.Healing:
                    string critHealText = entry.IsCritical ? " <color=lime>CRITICAL</color>" : "";
                    return $"{entry.SourceName} heals {entry.TargetName} for {entry.Amount}{critHealText}";
                    
                case CombatLogEntryType.Buff:
                    return $"{entry.SourceName} applies {entry.EffectName} to {entry.TargetName}";
                    
                case CombatLogEntryType.Debuff:
                    return $"{entry.SourceName} afflicts {entry.TargetName} with {entry.EffectName}";
                    
                case CombatLogEntryType.Item:
                    return $"{entry.SourceName} uses {entry.EffectName}";
                    
                case CombatLogEntryType.Miss:
                    return $"{entry.SourceName}'s {GetAttackTypeText(entry.AttackType)} misses {entry.TargetName}";
                    
                case CombatLogEntryType.CombatState:
                    return $"<color=yellow>{entry.Message}</color>";
                    
                default:
                    return "Unknown combat event";
            }
        }
        
        /// <summary>
        /// Format detailed description text
        /// </summary>
        private string FormatDetailsText(CombatLogEntry entry)
        {
            switch (entry.Type)
            {
                case CombatLogEntryType.Damage:
                    string resistText = entry.IsResisted ? "Partially resisted" : "";
                    string vulnerableText = entry.IsVulnerable ? "Vulnerability exploited" : "";
                    
                    return $"Damage Type: {entry.DamageType}\nAmount: {entry.Amount}{(entry.IsCritical ? " (Critical)" : "")}\n{resistText}\n{vulnerableText}";
                    
                case CombatLogEntryType.Healing:
                    return $"Healing Amount: {entry.Amount}{(entry.IsCritical ? " (Critical)" : "")}";
                    
                case CombatLogEntryType.Buff:
                case CombatLogEntryType.Debuff:
                    return $"{entry.EffectName}\nDuration: {entry.Duration}s\n\n{entry.EffectDescription}";
                    
                case CombatLogEntryType.Item:
                    return $"{entry.EffectName}\n\n{entry.EffectDescription}";
                    
                case CombatLogEntryType.Miss:
                    return $"Attack Type: {GetAttackTypeText(entry.AttackType)}\nResult: Miss";
                    
                default:
                    return "";
            }
        }
        
        /// <summary>
        /// Get color for entry based on type
        /// </summary>
        private Color GetEntryColor(CombatLogEntry entry)
        {
            switch (entry.Type)
            {
                case CombatLogEntryType.Damage:
                    return entry.IsCritical ? criticalColor : damageColor;
                    
                case CombatLogEntryType.Healing:
                    return healingColor;
                    
                case CombatLogEntryType.Buff:
                    return buffColor;
                    
                case CombatLogEntryType.Debuff:
                    return debuffColor;
                    
                case CombatLogEntryType.Item:
                    return itemColor;
                    
                case CombatLogEntryType.Miss:
                    return missColor;
                    
                case CombatLogEntryType.CombatState:
                    return systemColor;
                    
                default:
                    return Color.gray;
            }
        }
        
        /// <summary>
        /// Get icon for entry based on type
        /// </summary>
        private Sprite GetEntryIcon(CombatLogEntry entry)
        {
            switch (entry.Type)
            {
                case CombatLogEntryType.Damage:
                    return entry.IsCritical ? criticalIcon : damageIcon;
                    
                case CombatLogEntryType.Healing:
                    return healingIcon;
                    
                case CombatLogEntryType.Buff:
                    return buffIcon;
                    
                case CombatLogEntryType.Debuff:
                    return debuffIcon;
                    
                case CombatLogEntryType.Item:
                    return itemIcon;
                    
                case CombatLogEntryType.Miss:
                    return missIcon;
                    
                case CombatLogEntryType.CombatState:
                    return systemIcon;
                    
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Get text description for attack type
        /// </summary>
        private string GetAttackTypeText(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Melee:
                    return "melee attack";
                case AttackType.Ranged:
                    return "ranged attack";
                case AttackType.Spell:
                    return "spell";
                case AttackType.Special:
                    return "special attack";
                default:
                    return "attack";
            }
        }
        
        /// <summary>
        /// Toggle expanded state
        /// </summary>
        private void ToggleExpanded()
        {
            isExpanded = !isExpanded;
            
            if (expandedPanel != null)
            {
                expandedPanel.SetActive(isExpanded);
            }
            
            // Play sound
            PlaySound(isExpanded ? "Expand" : "Collapse");
        }
        
        /// <summary>
        /// Handle pointer click
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // Toggle expanded view on click
            ToggleExpanded();
        }
        
        /// <summary>
        /// Handle pointer enter
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Highlight entry
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(
                    backgroundImage.color.r + 0.2f,
                    backgroundImage.color.g + 0.2f,
                    backgroundImage.color.b + 0.2f,
                    backgroundImage.color.a
                );
            }
        }
        
        /// <summary>
        /// Handle pointer exit
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // Reset highlight
            if (backgroundImage != null)
            {
                backgroundImage.color = GetEntryColor(entry);
            }
        }
        
        /// <summary>
        /// Play UI sound if audio manager is available
        /// </summary>
        private void PlaySound(string soundName)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound(soundName);
            }
        }
    }
}