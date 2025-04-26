using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Gameplay.Items.Trading {
    /// <summary>
    /// Represents a trading partner, either an NPC or another player
    /// Provides interface for trading interaction
    /// </summary>
    public class TradePartner : MonoBehaviour {
        [Header("Trade Partner Info")]
        [SerializeField]
        private string traderName;
        [SerializeField]
        private Sprite traderIcon;
        [SerializeField]
        public bool isNPC = true;

        [Header("NPC Trader Settings")]
        [SerializeField]
        private bool isAlwaysWillingToTrade = true;
        [SerializeField]
        private int availableGold = 1000;
        [SerializeField]
        private List<NPCTradeItem> availableItems = new List<NPCTradeItem>();
        [SerializeField]
        private List<ItemType> acceptedItemTypes = new List<ItemType>();
        [SerializeField]
        private List<string> specialWantedItems = new List<string>(); // Item IDs

        [Header("Trade Value Settings")]
        [SerializeField]
        private float buyMultiplier = 1.5f; // NPC sells at higher price
        [SerializeField]
        private float sellMultiplier = 0.6f; // NPC buys at lower price

        // Properties
        public string TraderName => traderName;
        public Sprite TraderIcon => traderIcon;

        /// <summary>
        /// Initialize offers for an NPC trader
        /// </summary>
        public void InitializeTradeOffers(TradeManager tradeManager) {
            if (!isNPC)
                return;

            // NPCs don't automatically offer anything, they wait for the player to make offers
            // But we could implement here auto-offering of special items or deals
        }

        /// <summary>
        /// Evaluate a trade offer (for NPCs)
        /// </summary>
        public bool EvaluateTrade(List<TradeOffer> playerOffers, int playerGold) {
            if (!isNPC)
                return false; // Only NPCs auto-evaluate

            // If always willing to trade, accept any valid trade
            if (isAlwaysWillingToTrade) {
                return IsFairTrade(playerOffers, playerGold);
            }

            // More complex evaluation could go here for specific NPCs
            // For example, checking if the trade contains items they want
            // or if the deal is particularly good for them

            return false;
        }

        /// <summary>
        /// Check if a trade is considered fair
        /// </summary>
        private bool IsFairTrade(List<TradeOffer> playerOffers, int playerGold) {
            // Calculate the value of what player is offering
            int playerOfferValue = CalculateOffersValue(playerOffers, true) + playerGold;

            // Calculate the value of what NPC is offering
            List<TradeOffer> npcOffers = GetCurrentNPCOffers();
            int npcOfferValue = CalculateOffersValue(npcOffers, false) + GetOfferedGold();

            // Trade is fair if NPC is getting at least equal value
            return playerOfferValue >= npcOfferValue;
        }

        /// <summary>
        /// Calculate the value of offered items
        /// </summary>
        private int CalculateOffersValue(List<TradeOffer> offers, bool isPlayerOffers) {
            int totalValue = 0;

            foreach (var offer in offers) {
                float multiplier = isPlayerOffers ? sellMultiplier : buyMultiplier;
                totalValue += Mathf.RoundToInt(offer.item.buyPrice * offer.count * multiplier);
            }

            return totalValue;
        }

        /// <summary>
        /// Get the current offers from the NPC
        /// </summary>
        private List<TradeOffer> GetCurrentNPCOffers() {
            // This would be the items that the NPC has put into the trade
            // For now, return an empty list as a placeholder
            return new List<TradeOffer>();
        }

        /// <summary>
        /// Get the gold amount offered by the NPC
        /// </summary>
        private int GetOfferedGold() {
            // This would be the gold that the NPC has put into the trade
            // For now, return 0 as a placeholder
            return 0;
        }

        /// <summary>
        /// Check if NPC is willing to sell an item
        /// </summary>
        public bool IsWillingToSell(Item item) {
            if (!isNPC)
                return false;

            // Look for item in available items
            return availableItems.Exists(i => i.item.itemID == item.itemID && i.count > 0);
        }

        /// <summary>
        /// Check if NPC is willing to buy an item
        /// </summary>
        public bool IsWillingToBuy(Item item) {
            if (!isNPC)
                return false;

            // Check if NPC has enough gold
            if (availableGold < Mathf.RoundToInt(item.sellPrice * sellMultiplier))
                return false;

            // Check if item type is accepted
            if (acceptedItemTypes.Contains(item.itemType))
                return true;

            // Check if it's a specifically wanted item
            if (specialWantedItems.Contains(item.itemID))
                return true;

            return false;
        }

        /// <summary>
        /// Get the sale price for an item
        /// </summary>
        public int GetSalePrice(Item item) {
            if (!isNPC)
                return item.buyPrice;

            return Mathf.RoundToInt(item.buyPrice * buyMultiplier);
        }

        /// <summary>
        /// Get the purchase price for an item
        /// </summary>
        public int GetPurchasePrice(Item item) {
            if (!isNPC)
                return item.sellPrice;

            // Check if it's a specially wanted item
            if (specialWantedItems.Contains(item.itemID)) {
                // Pay more for wanted items
                return Mathf.RoundToInt(item.sellPrice * sellMultiplier * 1.5f);
            }

            return Mathf.RoundToInt(item.sellPrice * sellMultiplier);
        }

        /// <summary>
        /// Get available items for sale
        /// </summary>
        public List<NPCTradeItem> GetAvailableItems() {
            return new List<NPCTradeItem>(availableItems);
        }

        /// <summary>
        /// Get available gold for trading
        /// </summary>
        public int GetAvailableGold() {
            return availableGold;
        }
    }

    /// <summary>
    /// Represents an item that an NPC trader has available
    /// </summary>
    [System.Serializable]
    public class NPCTradeItem {
        public Item item;
        public int count;
        public bool isUnlimited;
        public bool isSpecialOffer;
        [Range(0.1f, 2f)]
        public float priceMultiplier = 1f; // Custom price multiplier for this specific item
    }
}