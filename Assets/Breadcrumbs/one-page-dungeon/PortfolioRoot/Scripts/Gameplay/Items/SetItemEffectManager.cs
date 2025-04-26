using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Items.Database;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items {
    /// <summary>
    /// Manages set item effects for a character
    /// Tracks equipped set items and applies bonuses when thresholds are met
    /// </summary>
    public partial class SetItemEffectManager : MonoBehaviour {
        // References
        private EquipmentManager equipmentManager;
        private ItemDatabase itemDatabase;

        // Currently active set bonuses
        private Dictionary<string, List<SetBonus>> activeSetBonuses = new Dictionary<string, List<SetBonus>>();

        // Tracking equipped set pieces
        private Dictionary<string, List<Item>> equippedSetItems = new Dictionary<string, List<Item>>();

        // Visual effects for active sets
        private Dictionary<string, List<GameObject>> setVisualEffects = new Dictionary<string, List<GameObject>>();

        [Header("Debug")]
        [SerializeField]
        private bool showDebugInfo = false;

        private void Awake() {
            equipmentManager = GetComponent<EquipmentManager>();

            // Find item database - assume it's in Resources
            itemDatabase = Resources.Load<ItemDatabase>("ScriptableObjects/ItemDatabase");

            if (itemDatabase == null) {
                Debug.LogError("SetItemEffectManager could not find ItemDatabase in Resources!");
            }
        }

        private void Start() {
            // Subscribe to equipment change events
            if (equipmentManager != null) {
                equipmentManager.OnItemEquipped.AddListener(OnItemEquipped);
                equipmentManager.OnItemUnequipped.AddListener(OnItemUnequipped);
            }

            // Initialize with currently equipped items
            if (equipmentManager != null && itemDatabase != null) {
                InitializeFromCurrentEquipment();
            }
        }

        private void OnDestroy() {
            // Unsubscribe from events
            if (equipmentManager != null) {
                equipmentManager.OnItemEquipped.RemoveListener(OnItemEquipped);
                equipmentManager.OnItemUnequipped.RemoveListener(OnItemUnequipped);
            }

            // Clean up all active effects
            ClearAllSetEffects();
        }

        /// <summary>
        /// Initialize set tracking from current equipment
        /// </summary>
        private void InitializeFromCurrentEquipment() {
            // Clear existing data
            ClearAllSetEffects();
            equippedSetItems.Clear();

            // Check all equipped items
            var allEquippedItems = equipmentManager.GetAllEquippedItems();

            foreach (var kvp in allEquippedItems) {
                EquippableItem item = kvp.Value;

                if (item != null) {
                    ProcessSetItem(item);
                }
            }

            // Update all set bonuses
            UpdateAllSetBonuses();
        }

        /// <summary>
        /// Handle newly equipped items
        /// </summary>
        private void OnItemEquipped(EquippableItem item, EquipmentSlot slot) {
            if (item == null || itemDatabase == null)
                return;

            ProcessSetItem(item);
            UpdateAllSetBonuses();
        }

        /// <summary>
        /// Handle unequipped items
        /// </summary>
        private void OnItemUnequipped(EquippableItem item, EquipmentSlot slot) {
            if (item == null || itemDatabase == null)
                return;

            RemoveSetItem(item);
            UpdateAllSetBonuses();
        }

        /// <summary>
        /// Get all sets with at least one equipped piece
        /// </summary>
        public Dictionary<string, int> GetEquippedSetCounts() {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (var kvp in equippedSetItems) {
                result[kvp.Key] = kvp.Value.Count;
            }

            return result;
        }

        /// <summary>
        /// Get info about a set by ID
        /// </summary>
        public (string name, int equipped, int total) GetSetInfo(string setId) {
            SetItemDefinition setDef = itemDatabase.GetSetDefinition(setId);

            if (setDef != null && equippedSetItems.TryGetValue(setId, out var items)) {
                return (setDef.setName, items.Count, setDef.GetTotalPieces());
            }

            return (string.Empty, 0, 0);
        }
    }
}