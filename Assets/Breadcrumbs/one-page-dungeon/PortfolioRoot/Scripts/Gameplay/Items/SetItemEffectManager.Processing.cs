using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Gameplay.Items.Database;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.Gameplay.Items {
    /// <summary>
    /// Partial class for SetItemEffectManager - Item processing functionality
    /// </summary>
    public partial class SetItemEffectManager : MonoBehaviour {
        /// <summary>
        /// Process a potentially set item
        /// </summary>
        private void ProcessSetItem(Item item) {
            // Check if item is part of a set
            SetItemDefinition setDef = itemDatabase.GetSetForItem(item);

            if (setDef != null) {
                string setId = setDef.setId;

                // Create set entry if it doesn't exist yet
                if (!equippedSetItems.ContainsKey(setId)) {
                    equippedSetItems[setId] = new List<Item>();
                }

                // Add item to set tracking (avoid duplicates)
                if (!equippedSetItems[setId].Contains(item)) {
                    equippedSetItems[setId].Add(item);

                    if (showDebugInfo) {
                        Debug.Log(
                            $"Added {item.itemName} to set {setDef.setName} ({equippedSetItems[setId].Count}/{setDef.GetTotalPieces()} pieces)");
                    }
                }
            }
        }

        /// <summary>
        /// Remove an item from set tracking
        /// </summary>
        private void RemoveSetItem(Item item) {
            // Find the set this item belongs to
            foreach (var kvp in equippedSetItems) {
                string setId = kvp.Key;
                List<Item> items = kvp.Value;

                if (items.Contains(item)) {
                    items.Remove(item);

                    if (showDebugInfo) {
                        SetItemDefinition setDef = itemDatabase.GetSetDefinition(setId);
                        Debug.Log(
                            $"Removed {item.itemName} from set {setDef?.setName ?? setId} ({items.Count}/{setDef?.GetTotalPieces() ?? 0} pieces)");
                    }

                    // If no more items in this set, remove set entry
                    if (items.Count == 0) {
                        equippedSetItems.Remove(setId);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Update all set bonuses based on current equipment
        /// </summary>
        private void UpdateAllSetBonuses() {
            // First, remove all active bonuses
            RemoveAllSetBonuses();

            // Then apply bonuses for each set based on equipped pieces
            foreach (var kvp in equippedSetItems) {
                string setId = kvp.Key;
                List<Item> items = kvp.Value;

                // Get set definition
                SetItemDefinition setDef = itemDatabase.GetSetDefinition(setId);

                if (setDef != null) {
                    int pieceCount = items.Count;

                    // Get all applicable bonuses for this piece count
                    List<SetBonus> applicableBonuses = setDef.GetAllBonusesForPieceCount(pieceCount);

                    if (applicableBonuses.Count > 0) {
                        // Apply all bonuses
                        foreach (var bonus in applicableBonuses) {
                            ApplySetBonus(setId, bonus);
                        }

                        // Store active bonuses for this set
                        activeSetBonuses[setId] = applicableBonuses;

                        if (showDebugInfo) {
                            Debug.Log(
                                $"Applied {applicableBonuses.Count} bonuses for set {setDef.setName} ({pieceCount}/{setDef.GetTotalPieces()} pieces)");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all active set bonuses
        /// </summary>
        public List<(string setName, string bonusName, string description)> GetActiveSetBonuses() {
            List<(string, string, string)> bonusList = new List<(string, string, string)>();

            foreach (var kvp in activeSetBonuses) {
                string setId = kvp.Key;
                List<SetBonus> bonuses = kvp.Value;

                SetItemDefinition setDef = itemDatabase.GetSetDefinition(setId);
                string setName = setDef?.setName ?? setId;

                foreach (var bonus in bonuses) {
                    bonusList.Add((setName, bonus.bonusName, bonus.bonusDescription));
                }
            }

            return bonusList;
        }
    }
}