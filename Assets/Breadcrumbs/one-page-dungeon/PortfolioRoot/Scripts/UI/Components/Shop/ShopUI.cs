#if INCOMPLETE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.UI.Components
{
    /// <summary>
    /// Manages the shop user interface for buying and selling items
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Shop UI Components")]
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private Transform playerInventoryContainer;
        [SerializeField] private GameObject inventoryItemPrefab;
        [SerializeField] private Text shopNameText;
        [SerializeField] private Text playerGoldText;
        [SerializeField] private Text shopDescriptionText;
        [SerializeField] private Button closeButton;
        [SerializeField] private TabGroup categoryTabs;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button buyAllButton;
        [SerializeField] private Button sellAllButton;
        [SerializeField] private Slider discountSlider;
        [SerializeField] private ItemDetailsPanel detailsPanel;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        
        [Header("Shop Settings")]
        [SerializeField] private float buyMarkup = 1.2f;  // 20% markup for buying
        [SerializeField] private float sellDiscount = 0.5f;  // 50% discount when selling
        [SerializeField] private float reputationDiscount = 0.1f;  // 10% discount for max reputation
        
        private List<ShopItemData> shopInventory = new List<ShopItemData>();
        private List<InventoryItem> playerInventory = new List<InventoryItem>();
        private ShopItemData selectedShopItem;
        private InventoryItem selectedInventoryItem;
        private ShopData currentShop;
        private int playerReputation = 0;
        private int playerGold = 0;
        private ShopMode currentMode = ShopMode.Buy;
        private ItemCategory currentCategory = ItemCategory.All;
        
        // Shop mode enum
        private enum ShopMode
        {
            Buy,
            Sell
        }
        
        private void Awake()
        {
            // Set up button listeners
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseShop);
            }
            
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(BuySelectedItem);
            }
            
            if (sellButton != null)
            {
                sellButton.onClick.AddListener(SellSelectedItem);
            }
            
            if (buyAllButton != null)
            {
                buyAllButton.onClick.AddListener(BuyAllItems);
            }
            
            if (sellAllButton != null)
            {
                sellAllButton.onClick.AddListener(SellAllItems);
            }
            
            // Set initial state
            buyButton.interactable = false;
            sellButton.interactable = false;
            buyAllButton.interactable = false;
            sellAllButton.interactable = false;
            
            // Set up category tabs if available
            if (categoryTabs != null)
            {
                categoryTabs.OnTabSelected += OnCategoryTabSelected;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (categoryTabs != null)
            {
                categoryTabs.OnTabSelected -= OnCategoryTabSelected;
            }
        }
        
        /// <summary>
        /// Initialize the shop UI with shop data
        /// </summary>
        public void Initialize(ShopData shopData, int playerGoldAmount, int reputationLevel)
        {
            currentShop = shopData;
            playerReputation = reputationLevel;
            playerGold = playerGoldAmount;
            
            // Set shop details
            if (shopNameText != null)
            {
                shopNameText.text = shopData.ShopName;
            }
            
            if (shopDescriptionText != null)
            {
                shopDescriptionText.text = shopData.Description;
            }
            
            if (playerGoldText != null)
            {
                playerGoldText.text = $"{playerGold} Gold";
            }
            
            // Set discount slider based on reputation
            if (discountSlider != null)
            {
                float discount = CalculateReputationDiscount();
                discountSlider.value = discount;
            }
            
            // Set shop inventory
            shopInventory = shopData.Inventory;
            
            // Refresh UI
            RefreshShopInventory();
            SetMode(ShopMode.Buy);
            
            // Default to first category
            if (categoryTabs != null && categoryTabs.TabButtons.Count > 0)
            {
                categoryTabs.OnTabSelected.Invoke(0);
            }
            else
            {
                // No tabs, show all items
                FilterItemsByCategory(ItemCategory.All);
            }
            
            // Get player inventory
            LoadPlayerInventory();
        }
        
        /// <summary>
        /// Load player inventory items
        /// </summary>
        private void LoadPlayerInventory()
        {
            // This would be retrieved from the player's inventory system
            // For now, using a placeholder
            if (playerInventory == null)
            {
                playerInventory = new List<InventoryItem>();
            }
            
            // Refresh UI with loaded inventory
            RefreshPlayerInventory();
        }
        
        /// <summary>
        /// Refresh shop inventory display
        /// </summary>
        private void RefreshShopInventory()
        {
            if (shopItemsContainer == null || shopItemPrefab == null)
                return;
                
            // Clear existing items
            foreach (Transform child in shopItemsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create shop items
            foreach (ShopItemData item in shopInventory)
            {
                // Skip if not in current category
                if (currentCategory != ItemCategory.All && item.Category != currentCategory)
                    continue;
                
                GameObject itemObject = Instantiate(shopItemPrefab, shopItemsContainer);
                ShopItemUI itemUI = itemObject.GetComponent<ShopItemUI>();
                
                if (itemUI != null)
                {
                    // Calculate price with markup/discount
                    float discount = CalculateReputationDiscount();
                    int adjustedPrice = currentMode == ShopMode.Buy ? 
                        Mathf.RoundToInt(item.BasePrice * buyMarkup * (1f - discount)) : 
                        Mathf.RoundToInt(item.BasePrice * sellDiscount);
                    
                    // Initialize shop item UI
                    itemUI.Initialize(item, adjustedPrice, item.Quantity);
                    
                    // Add click event
                    itemUI.OnItemSelected += () => OnShopItemSelected(item);
                }
            }
            
            // Update button states
            UpdateActionButtonStates();
        }
        
        /// <summary>
        /// Refresh player inventory display
        /// </summary>
        private void RefreshPlayerInventory()
        {
            if (playerInventoryContainer == null || inventoryItemPrefab == null)
                return;
                
            // Clear existing items
            foreach (Transform child in playerInventoryContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create inventory items
            foreach (InventoryItem item in playerInventory)
            {
                // Skip if not in current category
                if (currentCategory != ItemCategory.All && item.Category != currentCategory)
                    continue;
                
                GameObject itemObject = Instantiate(inventoryItemPrefab, playerInventoryContainer);
                InventoryItemUI itemUI = itemObject.GetComponent<InventoryItemUI>();
                
                if (itemUI != null)
                {
                    // Calculate price with discount
                    int sellPrice = Mathf.RoundToInt(item.BasePrice * sellDiscount);
                    
                    // Initialize inventory item UI
                    itemUI.Initialize(item, sellPrice, item.Quantity);
                    
                    // Add click event
                    itemUI.OnItemSelected += () => OnInventoryItemSelected(item);
                }
            }
            
            // Update button states
            UpdateActionButtonStates();
        }
        
        /// <summary>
        /// Set shop mode (buy or sell)
        /// </summary>
        private void SetMode(ShopMode mode)
        {
            currentMode = mode;
            
            // Update UI based on mode
            if (currentMode == ShopMode.Buy)
            {
                // Show shop inventory, hide player inventory
                if (shopItemsContainer != null && shopItemsContainer.parent is RectTransform shopPanel)
                {
                    shopPanel.gameObject.SetActive(true);
                }
                
                if (playerInventoryContainer != null && playerInventoryContainer.parent is RectTransform inventoryPanel)
                {
                    inventoryPanel.gameObject.SetActive(false);
                }
                
                // Update button visibility
                if (buyButton != null) buyButton.gameObject.SetActive(true);
                if (sellButton != null) sellButton.gameObject.SetActive(false);
                if (buyAllButton != null) buyAllButton.gameObject.SetActive(true);
                if (sellAllButton != null) sellAllButton.gameObject.SetActive(false);
            }
            else // Sell mode
            {
                // Hide shop inventory, show player inventory
                if (shopItemsContainer != null && shopItemsContainer.parent is RectTransform shopPanel)
                {
                    shopPanel.gameObject.SetActive(false);
                }
                
                if (playerInventoryContainer != null && playerInventoryContainer.parent is RectTransform inventoryPanel)
                {
                    inventoryPanel.gameObject.SetActive(true);
                }
                
                // Update button visibility
                if (buyButton != null) buyButton.gameObject.SetActive(false);
                if (sellButton != null) sellButton.gameObject.SetActive(true);
                if (buyAllButton != null) buyAllButton.gameObject.SetActive(false);
                if (sellAllButton != null) sellAllButton.gameObject.SetActive(true);
            }
            
            // Clear selection
            ClearSelection();
            
            // Refresh inventory display
            RefreshShopInventory();
            RefreshPlayerInventory();
        }
        
        /// <summary>
        /// Handle category tab selection
        /// </summary>
        private void OnCategoryTabSelected(int tabIndex)
        {
            // Map tab index to item category
            ItemCategory category = ItemCategory.All;
            
            switch (tabIndex)
            {
                case 0:
                    category = ItemCategory.All;
                    break;
                case 1:
                    category = ItemCategory.Weapon;
                    break;
                case 2:
                    category = ItemCategory.Armor;
                    break;
                case 3:
                    category = ItemCategory.Accessory;
                    break;
                case 4:
                    category = ItemCategory.Consumable;
                    break;
                case 5:
                    category = ItemCategory.Quest;
                    break;
                case 6:
                    category = ItemCategory.Material;
                    break;
                default:
                    category = ItemCategory.All;
                    break;
            }
            
            FilterItemsByCategory(category);
        }
        
        /// <summary>
        /// Filter items by category
        /// </summary>
        private void FilterItemsByCategory(ItemCategory category)
        {
            currentCategory = category;
            
            // Refresh displays with new filter
            RefreshShopInventory();
            RefreshPlayerInventory();
        }
        
        /// <summary>
        /// Handle shop item selection
        /// </summary>
        private void OnShopItemSelected(ShopItemData item)
        {
            selectedShopItem = item;
            selectedInventoryItem = null;
            
            // Show item details
            if (detailsPanel != null)
            {
                detailsPanel.ShowItemDetails(item);
            }
            
            // Update button states
            UpdateActionButtonStates();
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Select");
            }
        }
        
        /// <summary>
        /// Handle inventory item selection
        /// </summary>
        private void OnInventoryItemSelected(InventoryItem item)
        {
            selectedInventoryItem = item;
            selectedShopItem = null;
            
            // Show item details
            if (detailsPanel != null)
            {
                detailsPanel.ShowItemDetails(item);
            }
            
            // Update button states
            UpdateActionButtonStates();
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Select");
            }
        }
        
        /// <summary>
        /// Clear current selection
        /// </summary>
        private void ClearSelection()
        {
            selectedShopItem = null;
            selectedInventoryItem = null;
            
            // Clear details panel
            if (detailsPanel != null)
            {
                detailsPanel.ClearDetails();
            }
            
            // Update button states
            UpdateActionButtonStates();
        }
        
        /// <summary>
        /// Update action button states based on selection
        /// </summary>
        private void UpdateActionButtonStates()
        {
            if (currentMode == ShopMode.Buy)
            {
                // Buy mode
                if (buyButton != null)
                {
                    bool canBuy = selectedShopItem != null && 
                                  playerGold >= GetAdjustedPrice(selectedShopItem) &&
                                  selectedShopItem.Quantity > 0;
                    buyButton.interactable = canBuy;
                }
                
                if (buyAllButton != null)
                {
                    bool hasAffordableItems = false;
                    foreach (ShopItemData item in shopInventory)
                    {
                        if (item.Quantity > 0 && playerGold >= GetAdjustedPrice(item) &&
                            (currentCategory == ItemCategory.All || item.Category == currentCategory))
                        {
                            hasAffordableItems = true;
                            break;
                        }
                    }
                    buyAllButton.interactable = hasAffordableItems;
                }
            }
            else
            {
                // Sell mode
                if (sellButton != null)
                {
                    sellButton.interactable = selectedInventoryItem != null && selectedInventoryItem.Quantity > 0;
                }
                
                if (sellAllButton != null)
                {
                    bool hasSellableItems = playerInventory.Count > 0;
                    sellAllButton.interactable = hasSellableItems;
                }
            }
        }
        
        /// <summary>
        /// Calculate discount based on reputation
        /// </summary>
        private float CalculateReputationDiscount()
        {
            // Map reputation (typically 0-100) to a discount percentage (0-10%)
            float maxReputation = 100f;
            float normalizedRep = Mathf.Clamp(playerReputation, 0, maxReputation) / maxReputation;
            return normalizedRep * reputationDiscount;
        }
        
        /// <summary>
        /// Get price adjusted for reputation and mode
        /// </summary>
        private int GetAdjustedPrice(ShopItemData item)
        {
            if (currentMode == ShopMode.Buy)
            {
                float discount = CalculateReputationDiscount();
                return Mathf.RoundToInt(item.BasePrice * buyMarkup * (1f - discount));
            }
            else
            {
                return Mathf.RoundToInt(item.BasePrice * sellDiscount);
            }
        }
        
        /// <summary>
        /// Buy the selected item
        /// </summary>
        private void BuySelectedItem()
        {
            if (selectedShopItem == null || selectedShopItem.Quantity <= 0)
                return;
                
            int price = GetAdjustedPrice(selectedShopItem);
            
            if (playerGold < price)
            {
                // Not enough gold
                ShowMessage("Not enough gold!");
                return;
            }
            
            // Ask for confirmation
            if (confirmationDialog != null)
            {
                confirmationDialog.Show(
                    $"Buy {selectedShopItem.ItemName} for {price} gold?",
                    () => ExecutePurchase(selectedShopItem, price),
                    null
                );
            }
            else
            {
                // No confirmation dialog, proceed directly
                ExecutePurchase(selectedShopItem, price);
            }
        }
        
        /// <summary>
        /// Execute the purchase of an item
        /// </summary>
        private void ExecutePurchase(ShopItemData item, int price)
        {
            // Deduct gold
            playerGold -= price;
            
            // Update gold display
            if (playerGoldText != null)
            {
                playerGoldText.text = $"{playerGold} Gold";
            }
            
            // Reduce shop quantity
            item.Quantity--;
            
            // Add to player inventory
            AddItemToPlayerInventory(item);
            
            // Refresh UI
            RefreshShopInventory();
            
            // Play purchase sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Purchase");
            }
            
            // Show success message
            ShowMessage($"Purchased {item.ItemName}!");
        }
        
        /// <summary>
        /// Add an item to the player's inventory
        /// </summary>
        private void AddItemToPlayerInventory(ShopItemData shopItem)
        {
            // Check if item already exists in inventory
            foreach (InventoryItem invItem in playerInventory)
            {
                if (invItem.ItemID == shopItem.ItemID)
                {
                    // Increase quantity of existing item
                    invItem.Quantity++;
                    RefreshPlayerInventory();
                    return;
                }
            }
            
            // Create new inventory item
            InventoryItem newItem = new InventoryItem
            {
                ItemID = shopItem.ItemID,
                ItemName = shopItem.ItemName,
                Description = shopItem.Description,
                Icon = shopItem.Icon,
                BasePrice = shopItem.BasePrice,
                Category = shopItem.Category,
                Rarity = shopItem.Rarity,
                Quantity = 1
            };
            
            // Add to inventory
            playerInventory.Add(newItem);
            
            // Refresh UI
            RefreshPlayerInventory();
        }
        
        /// <summary>
        /// Sell the selected inventory item
        /// </summary>
        private void SellSelectedItem()
        {
            if (selectedInventoryItem == null || selectedInventoryItem.Quantity <= 0)
                return;
                
            int price = Mathf.RoundToInt(selectedInventoryItem.BasePrice * sellDiscount);
            
            // Ask for confirmation
            if (confirmationDialog != null)
            {
                confirmationDialog.Show(
                    $"Sell {selectedInventoryItem.ItemName} for {price} gold?",
                    () => ExecuteSale(selectedInventoryItem, price),
                    null
                );
            }
            else
            {
                // No confirmation dialog, proceed directly
                ExecuteSale(selectedInventoryItem, price);
            }
        }
        
        /// <summary>
        /// Execute the sale of an item
        /// </summary>
        private void ExecuteSale(InventoryItem item, int price)
        {
            // Add gold
            playerGold += price;
            
            // Update gold display
            if (playerGoldText != null)
            {
                playerGoldText.text = $"{playerGold} Gold";
            }
            
            // Reduce inventory quantity
            item.Quantity--;
            if (item.Quantity <= 0)
            {
                playerInventory.Remove(item);
                
                // Clear selection if sold out
                if (selectedInventoryItem == item)
                {
                    ClearSelection();
                }
            }
            
            // Add to shop inventory if appropriate
            if (currentShop != null && currentShop.WillBuyItem(item))
            {
                bool itemExistsInShop = false;
                
                // Check if item already exists in shop
                foreach (ShopItemData shopItem in shopInventory)
                {
                    if (shopItem.ItemID == item.ItemID)
                    {
                        shopItem.Quantity++;
                        itemExistsInShop = true;
                        break;
                    }
                }
                
                // Add as new shop item if it doesn't exist
                if (!itemExistsInShop)
                {
                    ShopItemData newShopItem = new ShopItemData
                    {
                        ItemID = item.ItemID,
                        ItemName = item.ItemName,
                        Description = item.Description,
                        Icon = item.Icon,
                        BasePrice = item.BasePrice,
                        Category = item.Category,
                        Rarity = item.Rarity,
                        Quantity = 1
                    };
                    
                    shopInventory.Add(newShopItem);
                }
            }
            
            // Refresh UI
            RefreshPlayerInventory();
            RefreshShopInventory();
            
            // Play sale sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Sale");
            }
            
            // Show success message
            ShowMessage($"Sold {item.ItemName} for {price} gold!");
        }
        
        /// <summary>
        /// Buy all affordable items
        /// </summary>
        private void BuyAllItems()
        {
            int totalCost = 0;
            int totalItems = 0;
            List<ShopItemData> itemsToBuy = new List<ShopItemData>();
            
            // Calculate total cost and collect items
            foreach (ShopItemData item in shopInventory)
            {
                if (item.Quantity > 0 && 
                    (currentCategory == ItemCategory.All || item.Category == currentCategory))
                {
                    int price = GetAdjustedPrice(item);
                    if (playerGold >= totalCost + price)
                    {
                        totalCost += price;
                        totalItems++;
                        itemsToBuy.Add(item);
                    }
                }
            }
            
            if (totalItems == 0)
            {
                ShowMessage("No items available to buy!");
                return;
            }
            
            // Ask for confirmation
            if (confirmationDialog != null)
            {
                confirmationDialog.Show(
                    $"Buy {totalItems} items for {totalCost} gold?",
                    () => ExecuteBulkPurchase(itemsToBuy),
                    null
                );
            }
            else
            {
                // No confirmation dialog, proceed directly
                ExecuteBulkPurchase(itemsToBuy);
            }
        }
        
        /// <summary>
        /// Execute bulk purchase of items
        /// </summary>
        private void ExecuteBulkPurchase(List<ShopItemData> items)
        {
            int totalCost = 0;
            
            foreach (ShopItemData item in items)
            {
                int price = GetAdjustedPrice(item);
                playerGold -= price;
                totalCost += price;
                
                // Reduce shop quantity
                item.Quantity--;
                
                // Add to player inventory
                AddItemToPlayerInventory(item);
            }
            
            // Update gold display
            if (playerGoldText != null)
            {
                playerGoldText.text = $"{playerGold} Gold";
            }
            
            // Refresh UI
            RefreshShopInventory();
            
            // Play purchase sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("BulkPurchase");
            }
            
            // Show success message
            ShowMessage($"Purchased {items.Count} items for {totalCost} gold!");
        }
        
        /// <summary>
        /// Sell all items in inventory
        /// </summary>
        private void SellAllItems()
        {
            int totalValue = 0;
            int totalItems = 0;
            List<InventoryItem> itemsToSell = new List<InventoryItem>();
            
            // Calculate total value and collect items
            foreach (InventoryItem item in playerInventory)
            {
                if (currentCategory == ItemCategory.All || item.Category == currentCategory)
                {
                    int price = Mathf.RoundToInt(item.BasePrice * sellDiscount) * item.Quantity;
                    totalValue += price;
                    totalItems += item.Quantity;
                    itemsToSell.Add(item);
                }
            }
            
            if (totalItems == 0)
            {
                ShowMessage("No items available to sell!");
                return;
            }
            
            // Ask for confirmation
            if (confirmationDialog != null)
            {
                confirmationDialog.Show(
                    $"Sell {totalItems} items for {totalValue} gold?",
                    () => ExecuteBulkSale(itemsToSell),
                    null
                );
            }
            else
            {
                // No confirmation dialog, proceed directly
                ExecuteBulkSale(itemsToSell);
            }
        }
        
        /// <summary>
        /// Execute bulk sale of items
        /// </summary>
        private void ExecuteBulkSale(List<InventoryItem> items)
        {
            int totalValue = 0;
            int totalItems = 0;
            
            foreach (InventoryItem item in new List<InventoryItem>(items))
            {
                int price = Mathf.RoundToInt(item.BasePrice * sellDiscount) * item.Quantity;
                playerGold += price;
                totalValue += price;
                totalItems += item.Quantity;
                
                // Add to shop inventory if appropriate
                if (currentShop != null && currentShop.WillBuyItem(item))
                {
                    bool itemExistsInShop = false;
                    
                    // Check if item already exists in shop
                    foreach (ShopItemData shopItem in shopInventory)
                    {
                        if (shopItem.ItemID == item.ItemID)
                        {
                            shopItem.Quantity += item.Quantity;
                            itemExistsInShop = true;
                            break;
                        }
                    }
                    
                    // Add as new shop item if it doesn't exist
                    if (!itemExistsInShop)
                    {
                        ShopItemData newShopItem = new ShopItemData
                        {
                            ItemID = item.ItemID,
                            ItemName = item.ItemName,
                            Description = item.Description,
                            Icon = item.Icon,
                            BasePrice = item.BasePrice,
                            Category = item.Category,
                            Rarity = item.Rarity,
                            Quantity = item.Quantity
                        };
                        
                        shopInventory.Add(newShopItem);
                    }
                }
                
                // Remove from player inventory
                playerInventory.Remove(item);
            }
            
            // Update gold display
            if (playerGoldText != null)
            {
                playerGoldText.text = $"{playerGold} Gold";
            }
            
            // Clear selection
            ClearSelection();
            
            // Refresh UI
            RefreshPlayerInventory();
            RefreshShopInventory();
            
            // Play sale sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("BulkSale");
            }
            
            // Show success message
            ShowMessage($"Sold {totalItems} items for {totalValue} gold!");
        }
        
        /// <summary>
        /// Close the shop
        /// </summary>
        private void CloseShop()
        {
            // Notify listeners that shop is closing
            OnShopClosed?.Invoke(playerGold);
            
            // Hide the shop UI
            gameObject.SetActive(false);
            
            // Play UI sound if audio manager available
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayUiSound("Close");
            }
        }
        
        /// <summary>
        /// Show a message to the player
        /// </summary>
        private void ShowMessage(string message)
        {
            // Use UI manager to show message if available
            if (UIManager.HasInstance)
            {
                UIManager.Instance.ShowMessage(message);
            }
            else
            {
                Debug.Log($"Shop: {message}");
            }
        }
        
        // Event for when shop is closed
        public System.Action<int> OnShopClosed;
    }
}
#endif