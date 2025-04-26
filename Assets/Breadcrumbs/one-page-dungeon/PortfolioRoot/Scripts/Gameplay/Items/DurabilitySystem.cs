using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Enhanced durability system for tracking and managing item durability
    /// </summary>
    public class DurabilitySystem : MonoBehaviour
    {
        [Header("Durability Settings")]
        [SerializeField] private bool enableDurabilitySystem = true;
        [SerializeField] private bool enableDurabilityWarnings = true;
        [SerializeField] private bool allowZeroDurabilityUse = false;
        
        [Header("Warning Thresholds")]
        [SerializeField] private float lowDurabilityThreshold = 0.3f; // 30% of max durability
        [SerializeField] private float criticalDurabilityThreshold = 0.1f; // 10% of max durability
        
        [Header("Repair Settings")]
        [SerializeField] private bool enableAutoRepair = false;
        [SerializeField] private float autoRepairCostMultiplier = 1.5f; // Gold cost per durability point
        
        [Header("Loss Modifiers")]
        [SerializeField] private float combatDurabilityLossMultiplier = 1.0f;
        [SerializeField] private float movementDurabilityLossMultiplier = 0.5f;
        [SerializeField] private float blockingDurabilityLossMultiplier = 1.2f;
        
        // Events
        [Header("Events")]
        public UnityEvent<EquippableItem, float> OnDurabilityChanged;
        public UnityEvent<EquippableItem> OnItemBroken;
        public UnityEvent<EquippableItem> OnItemRepaired;
        public UnityEvent<EquippableItem> OnLowDurabilityWarning;
        public UnityEvent<EquippableItem> OnCriticalDurabilityWarning;
        
        // References
        private EquipmentManager equipmentManager;
        private Dictionary<EquipmentSlot, bool> lowDurabilityWarningShown = new Dictionary<EquipmentSlot, bool>();
        private Dictionary<EquipmentSlot, bool> criticalDurabilityWarningShown = new Dictionary<EquipmentSlot, bool>();
        
        private void Awake()
        {
            equipmentManager = GetComponent<EquipmentManager>();
            
            if (equipmentManager == null)
            {
                Debug.LogError("DurabilitySystem requires an EquipmentManager component!");
                enabled = false;
            }
            
            // Initialize warning trackers
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                lowDurabilityWarningShown[slot] = false;
                criticalDurabilityWarningShown[slot] = false;
            }
        }
        
        private void Start()
        {
            // Subscribe to equipment change events
            if (equipmentManager != null)
            {
                equipmentManager.OnItemEquipped.AddListener(OnItemEquipped);
                equipmentManager.OnItemUnequipped.AddListener(OnItemUnequipped);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (equipmentManager != null)
            {
                equipmentManager.OnItemEquipped.RemoveListener(OnItemEquipped);
                equipmentManager.OnItemUnequipped.RemoveListener(OnItemUnequipped);
            }
        }
        
        /// <summary>
        /// Apply durability loss to an item
        /// </summary>
        public void ApplyDurabilityLoss(EquipmentSlot slot, float amount, DurabilityLossType lossType = DurabilityLossType.Combat)
        {
            if (!enableDurabilitySystem)
                return;
                
            EquippableItem item = equipmentManager.GetEquippedItem(slot);
            
            if (item == null || !item.useDurability)
                return;
                
            // Apply loss type multiplier
            float adjustedAmount = amount;
            switch (lossType)
            {
                case DurabilityLossType.Combat:
                    adjustedAmount *= combatDurabilityLossMultiplier;
                    break;
                case DurabilityLossType.Movement:
                    adjustedAmount *= movementDurabilityLossMultiplier;
                    break;
                case DurabilityLossType.Blocking:
                    adjustedAmount *= blockingDurabilityLossMultiplier;
                    break;
            }
            
            // Apply durability loss
            float oldDurability = item.currentDurability;
            item.ApplyDurabilityLoss(adjustedAmount);
            
            // Check if item broke
            if (item.currentDurability <= 0)
            {
                HandleBrokenItem(slot, item);
            }
            else
            {
                // Check for warnings
                CheckDurabilityWarnings(slot, item);
            }
            
            // Notify of durability change
            if (oldDurability != item.currentDurability)
            {
                OnDurabilityChanged?.Invoke(item, item.currentDurability);
            }
        }
        
        /// <summary>
        /// Repair an item to its maximum durability
        /// </summary>
        public bool RepairItem(EquipmentSlot slot, float amount = -1)
        {
            EquippableItem item = equipmentManager.GetEquippedItem(slot);
            
            if (item == null || !item.useDurability)
                return false;
                
            // Full repair if amount is negative
            if (amount < 0)
            {
                amount = item.maxDurability - item.currentDurability;
            }
            
            // Apply repair
            float oldDurability = item.currentDurability;
            item.Repair(amount);
            
            // Reset warning flags
            lowDurabilityWarningShown[slot] = false;
            criticalDurabilityWarningShown[slot] = false;
            
            // Notify of durability change
            if (oldDurability != item.currentDurability)
            {
                OnDurabilityChanged?.Invoke(item, item.currentDurability);
                OnItemRepaired?.Invoke(item);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Repair all equipped items
        /// </summary>
        public void RepairAllItems()
        {
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                RepairItem(slot);
            }
        }
        
        /// <summary>
        /// Calculate the cost to repair an item
        /// </summary>
        public int CalculateRepairCost(EquipmentSlot slot)
        {
            EquippableItem item = equipmentManager.GetEquippedItem(slot);
            
            if (item == null || !item.useDurability)
                return 0;
                
            // Calculate based on missing durability
            float missingDurability = item.maxDurability - item.currentDurability;
            
            // Base cost on item value and missing durability
            float baseCost = missingDurability * autoRepairCostMultiplier;
            float valueFactor = item.buyPrice / 100f; // More expensive items cost more to repair
            
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * valueFactor));
        }
        
        /// <summary>
        /// Calculate the cost to repair all equipped items
        /// </summary>
        public int CalculateRepairAllCost()
        {
            int totalCost = 0;
            
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                totalCost += CalculateRepairCost(slot);
            }
            
            return totalCost;
        }
        
        /// <summary>
        /// Get the current durability percentage of an item
        /// </summary>
        public float GetDurabilityPercentage(EquipmentSlot slot)
        {
            EquippableItem item = equipmentManager.GetEquippedItem(slot);
            
            if (item == null || !item.useDurability || item.maxDurability <= 0)
                return 1f;
                
            return item.currentDurability / item.maxDurability;
        }
        
        /// <summary>
        /// Check if an item is in low or critical durability state
        /// </summary>
        private void CheckDurabilityWarnings(EquipmentSlot slot, EquippableItem item)
        {
            if (!enableDurabilityWarnings)
                return;
                
            float durabilityPercentage = item.currentDurability / item.maxDurability;
            
            // Check critical first (more severe)
            if (durabilityPercentage <= criticalDurabilityThreshold)
            {
                if (!criticalDurabilityWarningShown[slot])
                {
                    criticalDurabilityWarningShown[slot] = true;
                    OnCriticalDurabilityWarning?.Invoke(item);
                }
            }
            // Then check low durability
            else if (durabilityPercentage <= lowDurabilityThreshold)
            {
                if (!lowDurabilityWarningShown[slot])
                {
                    lowDurabilityWarningShown[slot] = true;
                    OnLowDurabilityWarning?.Invoke(item);
                }
            }
            // Reset warnings if durability rises above thresholds
            else
            {
                lowDurabilityWarningShown[slot] = false;
                criticalDurabilityWarningShown[slot] = false;
            }
        }
        
        /// <summary>
        /// Handle a broken item
        /// </summary>
        private void HandleBrokenItem(EquipmentSlot slot, EquippableItem item)
        {
            // Notify that item broke
            OnItemBroken?.Invoke(item);
            
            // Unequip the item if zero durability items aren't allowed
            if (!allowZeroDurabilityUse)
            {
                equipmentManager.UnequipItem(slot);
            }
            
            // Auto-repair if enabled
            if (enableAutoRepair)
            {
                // Calculate repair cost
                int repairCost = CalculateRepairCost(slot);
                
                // Check if player has enough gold
                InventoryManager inventory = GetComponent<InventoryManager>();
                if (inventory != null && inventory.Currency >= repairCost)
                {
                    // Deduct gold and repair
                    inventory.RemoveCurrency(repairCost);
                    RepairItem(slot);
                    
                    Debug.Log($"Auto-repaired {item.itemName} for {repairCost} gold");
                }
            }
        }
        
        /// <summary>
        /// Handle item equip event
        /// </summary>
        private void OnItemEquipped(EquippableItem item, EquipmentSlot slot)
        {
            if (item != null && item.useDurability)
            {
                // Reset warning flags
                lowDurabilityWarningShown[slot] = false;
                criticalDurabilityWarningShown[slot] = false;
                
                // Check durability state immediately
                CheckDurabilityWarnings(slot, item);
            }
        }
        
        /// <summary>
        /// Handle item unequip event
        /// </summary>
        private void OnItemUnequipped(EquippableItem item, EquipmentSlot slot)
        {
            // Reset warning flags
            lowDurabilityWarningShown[slot] = false;
            criticalDurabilityWarningShown[slot] = false;
        }
        
        /// <summary>
        /// Enable or disable the durability system
        /// </summary>
        public void SetDurabilitySystemEnabled(bool enabled)
        {
            enableDurabilitySystem = enabled;
        }
        
        /// <summary>
        /// Enable or disable durability warnings
        /// </summary>
        public void SetDurabilityWarningsEnabled(bool enabled)
        {
            enableDurabilityWarnings = enabled;
        }
        
        /// <summary>
        /// Enable or disable zero durability items
        /// </summary>
        public void SetAllowZeroDurabilityUse(bool allowed)
        {
            allowZeroDurabilityUse = allowed;
        }
    }
    
    /// <summary>
    /// Types of durability loss
    /// </summary>
    public enum DurabilityLossType
    {
        Combat,    // Attacking and receiving damage
        Movement,  // General movement and exploration
        Blocking,  // Blocking damage with shield or weapon
        Special    // Special abilities or environmental effects
    }
}