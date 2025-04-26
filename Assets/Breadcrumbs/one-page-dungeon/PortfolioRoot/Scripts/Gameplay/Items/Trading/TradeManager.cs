using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GamePortfolio.Gameplay.Items.Trading {
    /// <summary>
    /// Manages trading between players or between player and NPC
    /// Handles the trade state, validation, and execution
    /// </summary>
    public class TradeManager : MonoBehaviour {
        [Header("Settings")]
        [SerializeField]
        private bool allowTradeWithNPCs = true;
        [SerializeField]
        private bool allowTradeWithPlayers = true;
        [SerializeField]
        private float maxTradeDistance = 5f;
        [SerializeField]
        private int maxItemsPerTrade = 10;

        [Header("Trade Log")]
        [SerializeField]
        private int maxLogEntries = 20;

        // Events
        [Header("Events")]
        public UnityEvent<TradePartner> OnTradeStarted;
        public UnityEvent<TradePartner, bool> OnTradeEnded; // bool is success or not
        public UnityEvent<Item, int, TradePartner> OnItemOffered;
        public UnityEvent<Item, int, TradePartner> OnItemRemoved;
        public UnityEvent<int, TradePartner> OnGoldOffered;
        public UnityEvent<TradePartner> OnTradeAccepted;
        public UnityEvent<TradeTransaction> OnTradeCompleted;

        // Trade state
        private TradeState currentTradeState = TradeState.Inactive;
        private TradePartner currentTradePartner;
        private List<TradeOffer> playerOffers = new List<TradeOffer>();
        private List<TradeOffer> partnerOffers = new List<TradeOffer>();
        private int playerOfferedGold;
        private int partnerOfferedGold;
        private bool playerHasAccepted;
        private bool partnerHasAccepted;

        // Trade history
        private List<TradeTransaction> tradeHistory = new List<TradeTransaction>();

        // References
        private InventoryManager playerInventory;

        private void Awake() {
            playerInventory = GetComponent<InventoryManager>();

            if (playerInventory == null) {
                Debug.LogError("TradeManager requires an InventoryManager component!");
                enabled = false;
            }
        }

        /// <summary>
        /// Start a trade with a player or NPC
        /// </summary>
        public bool StartTrade(TradePartner partner) {
            // Check if we're already in a trade
            if (currentTradeState != TradeState.Inactive) {
                Debug.LogWarning("Cannot start a new trade while another trade is active");
                return false;
            }

            // Check if trade partner is valid
            if (partner == null) {
                Debug.LogWarning("Trade partner is null");
                return false;
            }

            // Check trade type permission
            if (partner.isNPC && !allowTradeWithNPCs) {
                Debug.LogWarning("Trading with NPCs is disabled");
                return false;
            } else if (!partner.isNPC && !allowTradeWithPlayers) {
                Debug.LogWarning("Trading with players is disabled");
                return false;
            }

            // Check distance
            if (Vector3.Distance(transform.position, partner.transform.position) > maxTradeDistance) {
                Debug.LogWarning("Trade partner is too far away");
                return false;
            }

            // Initialize trade state
            currentTradePartner = partner;
            currentTradeState = TradeState.Negotiating;
            playerOffers.Clear();
            partnerOffers.Clear();
            playerOfferedGold = 0;
            partnerOfferedGold = 0;
            playerHasAccepted = false;
            partnerHasAccepted = false;

            // Notify trade started
            OnTradeStarted?.Invoke(partner);

            // If trading with NPC, let them initialize their offers
            if (partner.isNPC) {
                partner.InitializeTradeOffers(this);
            }

            return true;
        }

        /// <summary>
        /// End the current trade
        /// </summary>
        public void EndTrade(bool successful) {
            if (currentTradeState == TradeState.Inactive)
                return;

            TradePartner partner = currentTradePartner;

            // Return all offered items to their owners
            if (!successful) {
                ReturnAllOfferedItems();
            }

            // Reset trade state
            currentTradeState = TradeState.Inactive;
            currentTradePartner = null;

            // Notify trade ended
            OnTradeEnded?.Invoke(partner, successful);
        }

        /// <summary>
        /// Offer an item for trade
        /// </summary>
        public bool OfferItem(Item item, int count = 1) {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return false;

            // Reset acceptance state when offers change
            playerHasAccepted = false;
            partnerHasAccepted = false;

            // Check if item exists in player inventory
            if (!playerInventory.HasItem(item, count)) {
                Debug.LogWarning("Player does not have enough of the offered item");
                return false;
            }

            // Check if we're at the max items limit
            if (playerOffers.Count >= maxItemsPerTrade) {
                Debug.LogWarning("Maximum items per trade reached");
                return false;
            }

            // Check if we're already offering this item
            TradeOffer existingOffer = playerOffers.Find(o => o.item.itemID == item.itemID);
            if (existingOffer != null) {
                // Update existing offer
                existingOffer.count += count;

                // Remove item from inventory
                playerInventory.RemoveItem(item, count);

                // Notify
                OnItemOffered?.Invoke(item, existingOffer.count, currentTradePartner);

                return true;
            }

            // Create new offer
            TradeOffer newOffer = new TradeOffer {
                item = item,
                count = count
            };

            // Add to offers
            playerOffers.Add(newOffer);

            // Remove item from inventory
            playerInventory.RemoveItem(item, count);

            // Notify
            OnItemOffered?.Invoke(item, count, currentTradePartner);

            return true;
        }

        /// <summary>
        /// Remove an offered item from the trade
        /// </summary>
        public bool RemoveOfferedItem(Item item, int count = 1) {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return false;

            // Reset acceptance state when offers change
            playerHasAccepted = false;
            partnerHasAccepted = false;

            // Find the offered item
            TradeOffer existingOffer = playerOffers.Find(o => o.item.itemID == item.itemID);
            if (existingOffer == null) {
                Debug.LogWarning("Item is not currently offered");
                return false;
            }

            // Check if count is valid
            if (count > existingOffer.count) {
                count = existingOffer.count; // Limit to available
            }

            // Update offer
            existingOffer.count -= count;

            // Return item to inventory
            playerInventory.AddItem(item, count);

            // Remove offer if count reaches 0
            if (existingOffer.count <= 0) {
                playerOffers.Remove(existingOffer);
            }

            // Notify
            OnItemRemoved?.Invoke(item, count, currentTradePartner);

            return true;
        }

        /// <summary>
        /// Offer gold for trade
        /// </summary>
        public bool OfferGold(int amount) {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return false;

            // Reset acceptance state when offers change
            playerHasAccepted = false;
            partnerHasAccepted = false;

            // Check if player has enough gold
            if (playerInventory.Currency < amount + playerOfferedGold) {
                Debug.LogWarning("Player does not have enough gold");
                return false;
            }

            // Update offered gold
            int additionalGold = amount - playerOfferedGold;
            playerOfferedGold = amount;

            // Remove gold from inventory
            if (additionalGold > 0) {
                playerInventory.RemoveCurrency(additionalGold);
            } else if (additionalGold < 0) {
                playerInventory.AddCurrency(-additionalGold);
            }

            // Notify
            OnGoldOffered?.Invoke(amount, currentTradePartner);

            return true;
        }

        /// <summary>
        /// Partner offers an item
        /// </summary>
        public void AddPartnerOffer(Item item, int count) {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return;

            // Reset acceptance state when offers change
            playerHasAccepted = false;
            partnerHasAccepted = false;

            // Check if partner is already offering this item
            TradeOffer existingOffer = partnerOffers.Find(o => o.item.itemID == item.itemID);
            if (existingOffer != null) {
                // Update existing offer
                existingOffer.count += count;
                return;
            }

            // Create new offer
            TradeOffer newOffer = new TradeOffer {
                item = item,
                count = count
            };

            // Add to offers
            partnerOffers.Add(newOffer);
        }

        /// <summary>
        /// Partner removes an offered item
        /// </summary>
        public void RemovePartnerOffer(Item item, int count) {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return;

            // Reset acceptance state when offers change
            playerHasAccepted = false;
            partnerHasAccepted = false;

            // Find the offered item
            TradeOffer existingOffer = partnerOffers.Find(o => o.item.itemID == item.itemID);
            if (existingOffer == null) {
                return;
            }

            // Update offer
            existingOffer.count -= count;

            // Remove offer if count reaches 0
            if (existingOffer.count <= 0) {
                partnerOffers.Remove(existingOffer);
            }
        }

        /// <summary>
        /// Partner offers gold
        /// </summary>
        public void SetPartnerOfferedGold(int amount) {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return;

            // Reset acceptance state when offers change
            playerHasAccepted = false;
            partnerHasAccepted = false;

            // Update offered gold
            partnerOfferedGold = amount;
        }

        /// <summary>
        /// Accept the current trade
        /// </summary>
        public void AcceptTrade() {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return;

            // Mark player as accepted
            playerHasAccepted = true;

            // If NPC partner, have them decide to accept or not
            if (currentTradePartner.isNPC) {
                partnerHasAccepted = currentTradePartner.EvaluateTrade(playerOffers, playerOfferedGold);
            }

            // Check if both parties have accepted
            if (playerHasAccepted && partnerHasAccepted) {
                ExecuteTrade();
            } else {
                // Just notify acceptance
                OnTradeAccepted?.Invoke(currentTradePartner);
            }
        }

        /// <summary>
        /// Partner accepts the trade
        /// </summary>
        public void PartnerAcceptsTrade() {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return;

            // Mark partner as accepted
            partnerHasAccepted = true;

            // Check if both parties have accepted
            if (playerHasAccepted && partnerHasAccepted) {
                ExecuteTrade();
            }
        }

        /// <summary>
        /// Execute the agreed trade
        /// </summary>
        private void ExecuteTrade() {
            if (currentTradeState != TradeState.Negotiating || currentTradePartner == null)
                return;

            // Change state to prevent interference
            currentTradeState = TradeState.Executing;

            // Create transaction record
            TradeTransaction transaction = new TradeTransaction {
                partner = currentTradePartner,
                timestamp = System.DateTime.Now,
                playerOfferedItems = new List<TradeOffer>(playerOffers),
                partnerOfferedItems = new List<TradeOffer>(partnerOffers),
                playerOfferedGold = playerOfferedGold,
                partnerOfferedGold = partnerOfferedGold
            };

            // Add transaction to history
            AddToTradeHistory(transaction);

            // Add partner's items to player inventory
            foreach (var offer in partnerOffers) {
                playerInventory.AddItem(offer.item, offer.count);
            }

            // Add gold to player inventory
            if (partnerOfferedGold > 0) {
                playerInventory.AddCurrency(partnerOfferedGold);
            }

            // For NPCs, no need to actually add player's items to their inventory
            // For player-to-player trading, would need to transfer to the other player here

            // Notify completion
            OnTradeCompleted?.Invoke(transaction);

            // End trade successfully
            EndTrade(true);
        }

        /// <summary>
        /// Return all offered items to their respective owners
        /// </summary>
        private void ReturnAllOfferedItems() {
            // Return player's offered items
            foreach (var offer in playerOffers) {
                playerInventory.AddItem(offer.item, offer.count);
            }

            // Return player's offered gold
            if (playerOfferedGold > 0) {
                playerInventory.AddCurrency(playerOfferedGold);
            }

            // No need to handle partner's items for NPCs
            // For player-to-player trading, would need to return the other player's items too
        }

        /// <summary>
        /// Add a transaction to the trade history
        /// </summary>
        private void AddToTradeHistory(TradeTransaction transaction) {
            tradeHistory.Add(transaction);

            // Limit history size
            while (tradeHistory.Count > maxLogEntries) {
                tradeHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get the current trade state
        /// </summary>
        public TradeState GetTradeState() {
            return currentTradeState;
        }

        /// <summary>
        /// Get the current trade partner
        /// </summary>
        public TradePartner GetTradePartner() {
            return currentTradePartner;
        }

        /// <summary>
        /// Get player's current offers
        /// </summary>
        public List<TradeOffer> GetPlayerOffers() {
            return new List<TradeOffer>(playerOffers);
        }

        /// <summary>
        /// Get partner's current offers
        /// </summary>
        public List<TradeOffer> GetPartnerOffers() {
            return new List<TradeOffer>(partnerOffers);
        }

        /// <summary>
        /// Get player's offered gold
        /// </summary>
        public int GetPlayerOfferedGold() {
            return playerOfferedGold;
        }

        /// <summary>
        /// Get partner's offered gold
        /// </summary>
        public int GetPartnerOfferedGold() {
            return partnerOfferedGold;
        }

        /// <summary>
        /// Get the trade history
        /// </summary>
        public List<TradeTransaction> GetTradeHistory() {
            return new List<TradeTransaction>(tradeHistory);
        }
    }

    /// <summary>
    /// States of the trading system
    /// </summary>
    public enum TradeState {
        Inactive,    // No active trade
        Negotiating, // Trade in progress, items can be offered/withdrawn
        Executing    // Trade being executed, no more changes allowed
    }

    /// <summary>
    /// Represents an offer of an item in a trade
    /// </summary>
    [System.Serializable]
    public class TradeOffer {
        public Item item;
        public int count;
    }

    /// <summary>
    /// Record of a completed trade transaction
    /// </summary>
    [System.Serializable]
    public class TradeTransaction {
        public TradePartner partner;
        public System.DateTime timestamp;
        public List<TradeOffer> playerOfferedItems = new List<TradeOffer>();
        public List<TradeOffer> partnerOfferedItems = new List<TradeOffer>();
        public int playerOfferedGold;
        public int partnerOfferedGold;
    }
}