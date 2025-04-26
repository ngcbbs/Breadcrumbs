using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace GamePortfolio.Gameplay.Items
{
    /// <summary>
    /// Extends the equipment system with additional features and functionality
    /// </summary>
    public partial class EquipmentSystemExtension : MonoBehaviour
    {
        [Header("Equipment Sets")]
        [SerializeField] private List<EquipmentSet> equipmentSets = new List<EquipmentSet>();
        
        [Header("Transmog System")]
        [SerializeField] private bool enableTransmogSystem = true;
        [SerializeField] private Dictionary<EquipmentSlot, EquippableItem> transmogrifiedItems = new Dictionary<EquipmentSlot, EquippableItem>();
        
        [Header("Dual Wielding")]
        [SerializeField] private bool enableDualWielding = true;
        [SerializeField] private List<WeaponType> dualWieldableWeapons = new List<WeaponType>();
        [SerializeField] private float dualWieldDamageModifier = 0.75f; // Each weapon deals 75% of normal damage
        [SerializeField] private float dualWieldAttackSpeedModifier = 1.2f; // 20% faster attacks
        
        [Header("Enchantment System")]
        [SerializeField] private bool enableEnchantments = true;
        [SerializeField] private int maxEnchantmentsPerItem = 3;
        [SerializeField] private List<ItemEnchantment> availableEnchantments = new List<ItemEnchantment>();
        
        // Events
        [Header("Events")]
        public UnityEvent<EquipmentSet> OnEquipmentSetCompleted;
        public UnityEvent<EquipmentSet> OnEquipmentSetBroken;
        public UnityEvent<EquippableItem, EquippableItem> OnItemTransmogrified;
        public UnityEvent<EquippableItem, ItemEnchantment> OnItemEnchanted;
        
        // Reference to the base equipment manager
        private EquipmentManager equipmentManager;
        private Dictionary<string, bool> activeEquipmentSets = new Dictionary<string, bool>();
        private Dictionary<EquippableItem, List<ItemEnchantment>> itemEnchantments = new Dictionary<EquippableItem, List<ItemEnchantment>>();
        
        private void Awake()
        {
            equipmentManager = GetComponent<EquipmentManager>();
            
            if (equipmentManager == null)
            {
                Debug.LogError("EquipmentSystemExtension requires an EquipmentManager component!");
                enabled = false;
                return;
            }
            
            // Initialize dictionaries
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (!transmogrifiedItems.ContainsKey(slot))
                {
                    transmogrifiedItems[slot] = null;
                }
            }
        }
        
        private void Start()
        {
            // Subscribe to equipment manager events
            if (equipmentManager != null)
            {
                equipmentManager.OnItemEquipped.AddListener(OnItemEquipped);
                equipmentManager.OnItemUnequipped.AddListener(OnItemUnequipped);
            }
            
            // Initialize set tracking
            CheckAllEquipmentSets();
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
        /// Called when an item is equipped
        /// </summary>
        private void OnItemEquipped(EquippableItem item, EquipmentSlot slot)
        {
            // Check if equipment sets changed
            CheckAllEquipmentSets();
            
            // Apply transmog if available
            if (enableTransmogSystem && transmogrifiedItems.ContainsKey(slot) && transmogrifiedItems[slot] != null)
            {
                UpdateEquipmentVisual(slot);
            }
            
            // Check for dual wielding
            if (enableDualWielding && (slot == EquipmentSlot.MainHand || slot == EquipmentSlot.OffHand))
            {
                if (CanDualWield())
                {
                    ApplyDualWieldBonuses();
                }
                else
                {
                    RemoveDualWieldBonuses();
                }
            }
            
            // Apply enchantments
            if (enableEnchantments && item != null && itemEnchantments.ContainsKey(item))
            {
                foreach (var enchantment in itemEnchantments[item])
                {
                    ApplyEnchantmentEffect(item, enchantment);
                }
            }
        }
        
        /// <summary>
        /// Called when an item is unequipped
        /// </summary>
        private void OnItemUnequipped(EquippableItem item, EquipmentSlot slot)
        {
            // Check if equipment sets changed
            CheckAllEquipmentSets();
            
            // Check for dual wielding
            if (enableDualWielding && (slot == EquipmentSlot.MainHand || slot == EquipmentSlot.OffHand))
            {
                RemoveDualWieldBonuses();
            }
            
            // Remove enchantments
            if (enableEnchantments && item != null && itemEnchantments.ContainsKey(item))
            {
                foreach (var enchantment in itemEnchantments[item])
                {
                    RemoveEnchantmentEffect(item, enchantment);
                }
            }
        }
    }
}