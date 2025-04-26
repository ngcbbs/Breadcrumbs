#if INCOMPLETE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Items;

namespace GamePortfolio.UI {
    /// <summary>
    /// Main controller for the inventory interface
    /// Handles inventory grid, equipment slots, and drag & drop operations
    /// </summary>
    public class InventoryUI : MonoBehaviour {
        [Header("Inventory Panel")]
        [SerializeField]
        private GameObject inventoryPanel;
        [SerializeField]
        private RectTransform inventoryGridContainer;
        [SerializeField]
        private GameObject itemSlotPrefab;
        [SerializeField]
        private int inventoryColumns = 5;
        [SerializeField]
        private int inventoryRows = 6;
        [SerializeField]
        private float slotSize = 80f;
        [SerializeField]
        private float slotSpacing = 10f;

        [Header("Equipment Panel")]
        [SerializeField]
        private RectTransform equipmentPanel;
        [SerializeField]
        private ItemSlotUI weaponSlot;
        [SerializeField]
        private ItemSlotUI helmetSlot;
        [SerializeField]
        private ItemSlotUI chestSlot;
        [SerializeField]
        private ItemSlotUI glovesSlot;
        [SerializeField]
        private ItemSlotUI legsSlot;
        [SerializeField]
        private ItemSlotUI bootsSlot;
        [SerializeField]
        private ItemSlotUI accessory1Slot;
        [SerializeField]
        private ItemSlotUI accessory2Slot;

        [Header("Item Details")]
        [SerializeField]
        private ItemDetailsPanel itemDetailsPanel;

        [Header("UI Elements")]
        [SerializeField]
        private TMP_Text goldText;
        [SerializeField]
        private TMP_Text weightText;
        [SerializeField]
        private Slider weightSlider;
        [SerializeField]
        private Button sortButton;
        [SerializeField]
        private TMP_Dropdown sortDropdown;
        [SerializeField]
        private Button closeButton;

        // Drag and drop
        private ItemSlotUI currentDraggedSlot;
        private RectTransform draggedItemUI;
        private CanvasGroup draggedCanvasGroup;
        private Canvas parentCanvas;
        private Vector3 dragOffset;

        // References
        private InventoryManager inventoryManager;
        private EquipmentManager equipmentManager;
        private List<ItemSlotUI> inventorySlots = new List<ItemSlotUI>();
        private ItemSlotUI selectedSlot;

        private void Awake() {
            parentCanvas = GetComponentInParent<Canvas>();

            // Find managers
            inventoryManager = FindObjectOfType<InventoryManager>();
            equipmentManager = FindObjectOfType<EquipmentManager>();

            // Setup initial state
            if (inventoryPanel != null) {
                inventoryPanel.SetActive(false);
            }

            // Create dragged item UI
            CreateDraggedItemUI();

            // Set up button listeners
            SetupButtonListeners();
        }

        private void Start() {
            // Create inventory grid
            CreateInventoryGrid();

            // Set up equipment slots
            SetupEquipmentSlots();

            // Subscribe to events
            if (inventoryManager != null) {
                inventoryManager.OnInventoryChanged += RefreshInventory;
                inventoryManager.OnGoldChanged += UpdateGold;
                inventoryManager.OnWeightChanged += UpdateWeight;
            }

            if (equipmentManager != null) {
                equipmentManager.OnEquipmentChanged += RefreshEquipment;
            }

            // Initial refresh
            RefreshInventory();
            RefreshEquipment();
            UpdateGold(inventoryManager != null ? inventoryManager.Gold : 0);
            UpdateWeight(inventoryManager != null ? inventoryManager.CurrentWeight : 0,
                inventoryManager != null ? inventoryManager.MaxWeight : 100);
        }

        private void OnDestroy() {
            // Unsubscribe from events
            if (inventoryManager != null) {
                inventoryManager.OnInventoryChanged -= RefreshInventory;
                inventoryManager.OnGoldChanged -= UpdateGold;
                inventoryManager.OnWeightChanged -= UpdateWeight;
            }

            if (equipmentManager != null) {
                equipmentManager.OnEquipmentChanged -= RefreshEquipment;
            }
        }

        private void Update() {
            // Update dragged item position if dragging
            if (currentDraggedSlot != null && draggedItemUI != null) {
                Vector2 mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    Input.mousePosition, parentCanvas.worldCamera,
                    out mousePosition);

                draggedItemUI.position = parentCanvas.transform.TransformPoint(mousePosition) + dragOffset;
            }
        }

        /// <summary>
        /// Create all inventory slot UIs
        /// </summary>
        private void CreateInventoryGrid() {
            if (inventoryGridContainer == null || itemSlotPrefab == null)
                return;

            // Clear existing slots
            foreach (Transform child in inventoryGridContainer) {
                Destroy(child.gameObject);
            }

            inventorySlots.Clear();

            // Set grid container size
            float containerWidth = (slotSize * inventoryColumns) + (slotSpacing * (inventoryColumns - 1));
            float containerHeight = (slotSize * inventoryRows) + (slotSpacing * (inventoryRows - 1));
            inventoryGridContainer.sizeDelta = new Vector2(containerWidth, containerHeight);

            // Create grid of slots
            for (int y = 0; y < inventoryRows; y++) {
                for (int x = 0; x < inventoryColumns; x++) {
                    // Calculate position
                    float posX = (x * slotSize) + (x * slotSpacing);
                    float posY = -(y * slotSize) - (y * slotSpacing);

                    // Create slot
                    GameObject slotObj = Instantiate(itemSlotPrefab, inventoryGridContainer);
                    RectTransform slotRect = slotObj.GetComponent<RectTransform>();
                    slotRect.anchoredPosition = new Vector2(posX, posY);
                    slotRect.sizeDelta = new Vector2(slotSize, slotSize);

                    // Set up slot
                    ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();
                    if (slotUI != null) {
                        int index = (y * inventoryColumns) + x;
                        slotUI.Initialize(index, SlotType.Inventory);
                        slotUI.OnSlotClicked += OnSlotClicked;
                        slotUI.OnBeginDrag += OnBeginDrag;
                        slotUI.OnDrag += OnDrag;
                        slotUI.OnEndDrag += OnEndDrag;
                        slotUI.OnDrop += OnDrop;

                        inventorySlots.Add(slotUI);
                    }
                }
            }
        }

        /// <summary>
        /// Set up equipment slots with proper slot types
        /// </summary>
        private void SetupEquipmentSlots() {
            SetupEquipmentSlot(weaponSlot, SlotType.Weapon);
            SetupEquipmentSlot(helmetSlot, SlotType.Helmet);
            SetupEquipmentSlot(chestSlot, SlotType.Chest);
            SetupEquipmentSlot(glovesSlot, SlotType.Gloves);
            SetupEquipmentSlot(legsSlot, SlotType.Legs);
            SetupEquipmentSlot(bootsSlot, SlotType.Boots);
            SetupEquipmentSlot(accessory1Slot, SlotType.Accessory);
            SetupEquipmentSlot(accessory2Slot, SlotType.Accessory);
        }

        /// <summary>
        /// Set up a single equipment slot
        /// </summary>
        private void SetupEquipmentSlot(ItemSlotUI slot, SlotType slotType) {
            if (slot == null) return;

            slot.Initialize(-1, slotType); // Equipment slots use negative indices
            slot.OnSlotClicked += OnSlotClicked;
            slot.OnBeginDrag += OnBeginDrag;
            slot.OnDrag += OnDrag;
            slot.OnEndDrag += OnEndDrag;
            slot.OnDrop += OnDrop;
        }

        /// <summary>
        /// Create UI for dragged items
        /// </summary>
        private void CreateDraggedItemUI() {
            GameObject draggedObj = new GameObject("DraggedItem");
            draggedObj.transform.SetParent(transform);

            draggedItemUI = draggedObj.AddComponent<RectTransform>();
            draggedItemUI.sizeDelta = new Vector2(slotSize, slotSize);

            Image image = draggedObj.AddComponent<Image>();
            image.raycastTarget = false;

            draggedCanvasGroup = draggedObj.AddComponent<CanvasGroup>();
            draggedCanvasGroup.alpha = 0.8f;
            draggedCanvasGroup.blocksRaycasts = false;

            draggedObj.SetActive(false);
        }

        /// <summary>
        /// Set up button listeners
        /// </summary>
        private void SetupButtonListeners() {
            if (closeButton != null) {
                closeButton.onClick.AddListener(Hide);
            }

            if (sortButton != null) {
                sortButton.onClick.AddListener(SortInventory);
            }
        }

        /// <summary>
        /// Refresh the inventory display
        /// </summary>
        private void RefreshInventory() {
            if (inventoryManager == null)
                return;

            // Clear all inventory slots first
            foreach (ItemSlotUI slot in inventorySlots) {
                slot.ClearItem();
            }

            // Populate slots with items
            List<Item> items = inventoryManager.GetAllItems();
            for (int i = 0; i < items.Count && i < inventorySlots.Count; i++) {
                inventorySlots[i].SetItem(items[i]);
            }

            // Update selected item details if needed
            if (selectedSlot != null && selectedSlot.SlotType == SlotType.Inventory) {
                UpdateItemDetails(selectedSlot);
            }
        }

        /// <summary>
        /// Refresh the equipment display
        /// </summary>
        private void RefreshEquipment() {
            if (equipmentManager == null)
                return;

            // Update weapon slot
            UpdateEquipmentSlot(weaponSlot, equipmentManager.GetEquippedItem(EquipmentSlot.Weapon));

            // Update armor slots
            UpdateEquipmentSlot(helmetSlot, equipmentManager.GetEquippedItem(EquipmentSlot.Helmet));
            UpdateEquipmentSlot(chestSlot, equipmentManager.GetEquippedItem(EquipmentSlot.Chest));
            UpdateEquipmentSlot(glovesSlot, equipmentManager.GetEquippedItem(EquipmentSlot.Gloves));
            UpdateEquipmentSlot(legsSlot, equipmentManager.GetEquippedItem(EquipmentSlot.Legs));
            UpdateEquipmentSlot(bootsSlot, equipmentManager.GetEquippedItem(EquipmentSlot.Boots));

            // Update accessory slots
            UpdateEquipmentSlot(accessory1Slot, equipmentManager.GetEquippedItem(EquipmentSlot.Accessory1));
            UpdateEquipmentSlot(accessory2Slot, equipmentManager.GetEquippedItem(EquipmentSlot.Accessory2));

            // Update selected item details if needed
            if (selectedSlot != null && selectedSlot.SlotType != SlotType.Inventory) {
                UpdateItemDetails(selectedSlot);
            }
        }

        /// <summary>
        /// Update a specific equipment slot
        /// </summary>
        private void UpdateEquipmentSlot(ItemSlotUI slotUI, Item item) {
            if (slotUI == null) return;

            if (item != null) {
                slotUI.SetItem(item);
            } else {
                slotUI.ClearItem();
            }
        }

        /// <summary>
        /// Update the gold amount display
        /// </summary>
        private void UpdateGold(int amount) {
            if (goldText != null) {
                goldText.text = amount.ToString() + " Gold";
            }
        }

        /// <summary>
        /// Update the weight display
        /// </summary>
        private void UpdateWeight(float currentWeight, float maxWeight) {
            if (weightText != null) {
                weightText.text = $"{currentWeight:F1}/{maxWeight:F1}";
            }

            if (weightSlider != null) {
                weightSlider.maxValue = maxWeight;
                weightSlider.value = currentWeight;

                // Change color based on weight ratio
                float ratio = currentWeight / maxWeight;
                Color weightColor = Color.green;

                if (ratio > 0.9f) {
                    weightColor = Color.red;
                } else if (ratio > 0.7f) {
                    weightColor = Color.yellow;
                }

                Image fillImage = weightSlider.fillRect.GetComponent<Image>();
                if (fillImage != null) {
                    fillImage.color = weightColor;
                }
            }
        }

        /// <summary>
        /// Update the item details panel with the selected item
        /// </summary>
        private void UpdateItemDetails(ItemSlotUI slotUI) {
            if (itemDetailsPanel == null) return;

            Item item = slotUI.CurrentItem;

            if (item == null) {
                itemDetailsPanel.Hide();
                return;
            }

            // Show item details
            itemDetailsPanel.ShowItemDetails(item, slotUI.SlotType);

            // Set up comparison if needed
            if (slotUI.SlotType == SlotType.Inventory && equipmentManager != null && item is EquippableItem equippableItem) {
                // Find the equipped item of the same type to compare with
                EquipmentSlot equipSlot = GetEquipmentSlotFromItem(equippableItem);
                Item equippedItem = equipmentManager.GetEquippedItem(equipSlot);

                if (equippedItem != null) {
                    // Show comparison
                    itemDetailsPanel.ShowComparison(equippedItem, item);
                } else {
                    // Hide comparison
                    itemDetailsPanel.HideComparison();
                }
            } else {
                // Hide comparison
                itemDetailsPanel.HideComparison();
            }

            // Setup action buttons based on item type and slot
            SetupActionButtons(slotUI);
        }

        /// <summary>
        /// Setup action buttons for the selected item
        /// </summary>
        private void SetupActionButtons(ItemSlotUI slotUI) {
            if (itemDetailsPanel == null) return;

            Item item = slotUI.CurrentItem;

            if (item == null) {
                itemDetailsPanel.HideAllButtons();
                return;
            }

            // Setup use button
            itemDetailsPanel.SetupUseButton(item is ConsumableItem, () => UseSelectedItem());

            // Setup equip button
            bool isEquippable = item is EquippableItem;
            bool isAlreadyEquipped = slotUI.SlotType != SlotType.Inventory;
            itemDetailsPanel.SetupEquipButton(isEquippable && !isAlreadyEquipped, () => EquipSelectedItem());

            // Setup drop button
            itemDetailsPanel.SetupDropButton(true, () => DropSelectedItem());
        }

        /// <summary>
        /// Sort inventory by the selected criteria
        /// </summary>
        private void SortInventory() {
            if (inventoryManager == null || sortDropdown == null)
                return;

            SortType sortType = (SortType)sortDropdown.value;
            inventoryManager.SortInventory(sortType);

            // Play sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Sort");
            }
        }

        /// <summary>
        /// Use the currently selected item
        /// </summary>
        private void UseSelectedItem() {
            if (selectedSlot == null || selectedSlot.CurrentItem == null)
                return;

            if (selectedSlot.CurrentItem is ConsumableItem) {
                // Use the consumable item
                inventoryManager.UseItem(selectedSlot.SlotIndex);

                // Play sound
                if (AudioManager.HasInstance) {
                    AudioManager.Instance.PlaySfx("UseItem");
                }
            }
        }

        /// <summary>
        /// Equip the currently selected item
        /// </summary>
        private void EquipSelectedItem() {
            if (selectedSlot == null || selectedSlot.CurrentItem == null ||
                equipmentManager == null || selectedSlot.SlotType != SlotType.Inventory)
                return;

            if (selectedSlot.CurrentItem is EquippableItem equippable) {
                // Equip the item
                equipmentManager.EquipItem(equippable);

                // Play sound
                if (AudioManager.HasInstance) {
                    AudioManager.Instance.PlaySfx("EquipItem");
                }
            }
        }

        /// <summary>
        /// Drop the currently selected item
        /// </summary>
        private void DropSelectedItem() {
            if (selectedSlot == null || selectedSlot.CurrentItem == null)
                return;

            // Show confirmation dialog here if needed

            if (selectedSlot.SlotType == SlotType.Inventory) {
                // Drop from inventory
                inventoryManager.DropItem(selectedSlot.SlotIndex);
            } else {
                // Unequip from equipment slot
                EquipmentSlot equipSlot = GetEquipmentSlotFromSlotType(selectedSlot.SlotType);
                equipmentManager.UnequipItem(equipSlot);
            }

            // Hide details panel
            itemDetailsPanel.Hide();

            // Play sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("DropItem");
            }

            selectedSlot = null;
        }

        /// <summary>
        /// Get the equipment slot from item
        /// </summary>
        private EquipmentSlot GetEquipmentSlotFromItem(EquippableItem item) {
            if (item is WeaponItem)
                return EquipmentSlot.Weapon;

            return item.EquipSlot;
        }

        /// <summary>
        /// Get the equipment slot from slot type
        /// </summary>
        private EquipmentSlot GetEquipmentSlotFromSlotType(SlotType slotType) {
            switch (slotType) {
                case SlotType.Weapon:
                    return EquipmentSlot.Weapon;
                case SlotType.Helmet:
                    return EquipmentSlot.Helmet;
                case SlotType.Chest:
                    return EquipmentSlot.Chest;
                case SlotType.Gloves:
                    return EquipmentSlot.Gloves;
                case SlotType.Legs:
                    return EquipmentSlot.Legs;
                case SlotType.Boots:
                    return EquipmentSlot.Boots;
                case SlotType.Accessory:
                    // For simplicity, assume first accessory slot
                    // In a real implementation, we'd need to know which accessory slot this is
                    return EquipmentSlot.Accessory1;
                default:
                    return EquipmentSlot.None;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Event handler for slot click
        /// </summary>
        private void OnSlotClicked(ItemSlotUI slot) {
            // Update selection
            if (selectedSlot != null) {
                selectedSlot.SetSelected(false);
            }

            // Toggle selection if same slot
            if (selectedSlot == slot) {
                selectedSlot = null;
                itemDetailsPanel.Hide();
            } else {
                selectedSlot = slot;
                slot.SetSelected(true);

                // Show item details
                UpdateItemDetails(slot);
            }

            // Play sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Click");
            }
        }

        /// <summary>
        /// Event handler for beginning dragging
        /// </summary>
        private void OnBeginDrag(ItemSlotUI slot) {
            if (slot.CurrentItem == null)
                return;

            currentDraggedSlot = slot;

            // Setup dragged item UI
            if (draggedItemUI != null) {
                draggedItemUI.gameObject.SetActive(true);

                // Set sprite
                Image image = draggedItemUI.GetComponent<Image>();
                if (image != null) {
                    image.sprite = slot.CurrentItem.Icon;
                }

                // Set initial position
                Vector2 mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    Input.mousePosition, parentCanvas.worldCamera,
                    out mousePosition);

                draggedItemUI.position = parentCanvas.transform.TransformPoint(mousePosition);

                // Set drag offset so cursor is in center of item
                dragOffset = Vector3.zero;
            }

            // Reduce opacity of original slot
            slot.SetDragState(true);
        }

        /// <summary>
        /// Event handler for dragging
        /// </summary>
        private void OnDrag(ItemSlotUI slot) {
            // Handled in Update
        }

        /// <summary>
        /// Event handler for end dragging
        /// </summary>
        private void OnEndDrag(ItemSlotUI slot) {
            if (draggedItemUI != null) {
                draggedItemUI.gameObject.SetActive(false);
            }

            // Reset opacity of original slot
            slot.SetDragState(false);

            currentDraggedSlot = null;
        }

        /// <summary>
        /// Event handler for drop
        /// </summary>
        private void OnDrop(ItemSlotUI targetSlot) {
            if (currentDraggedSlot == null || currentDraggedSlot.CurrentItem == null)
                return;

            // Handle different drop scenarios

            // Inventory to inventory (swap or stack)
            if (currentDraggedSlot.SlotType == SlotType.Inventory && targetSlot.SlotType == SlotType.Inventory) {
                inventoryManager.SwapItems(currentDraggedSlot.SlotIndex, targetSlot.SlotIndex);
            }
            // Inventory to equipment
            else if (currentDraggedSlot.SlotType == SlotType.Inventory && targetSlot.SlotType != SlotType.Inventory) {
                EquipItemIntoSlot(currentDraggedSlot.CurrentItem as EquippableItem, targetSlot.SlotType);
            }
            // Equipment to inventory
            else if (currentDraggedSlot.SlotType != SlotType.Inventory && targetSlot.SlotType == SlotType.Inventory) {
                UnequipItem(currentDraggedSlot.SlotType);
            }
            // Equipment to equipment (swap)
            else if (currentDraggedSlot.SlotType != SlotType.Inventory && targetSlot.SlotType != SlotType.Inventory) {
                // First unequip from current slot
                EquipmentSlot sourceSlot = GetEquipmentSlotFromSlotType(currentDraggedSlot.SlotType);
                EquipmentSlot targetEquipSlot = GetEquipmentSlotFromSlotType(targetSlot.SlotType);

                if (equipmentManager.CanEquipToSlot(currentDraggedSlot.CurrentItem as EquippableItem, targetEquipSlot)) {
                    equipmentManager.SwapEquippedItems(sourceSlot, targetEquipSlot);
                }
            }

            // Play sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Drop");
            }
        }

        #endregion

        /// <summary>
        /// Equip an item into a specific equipment slot
        /// </summary>
        private void EquipItemIntoSlot(EquippableItem item, SlotType slotType) {
            if (item == null || equipmentManager == null)
                return;

            // Convert slot type to equipment slot
            EquipmentSlot equipSlot = GetEquipmentSlotFromSlotType(slotType);

            // Check if item can be equipped in this slot
            if (equipmentManager.CanEquipToSlot(item, equipSlot)) {
                equipmentManager.EquipItem(item);
            }
        }

        /// <summary>
        /// Unequip an item from a specific slot type
        /// </summary>
        private void UnequipItem(SlotType slotType) {
            if (equipmentManager == null)
                return;

            // Convert slot type to equipment slot
            EquipmentSlot equipSlot = GetEquipmentSlotFromSlotType(slotType);

            // Unequip
            equipmentManager.UnequipItem(equipSlot);
        }

        /// <summary>
        /// Show the inventory UI
        /// </summary>
        public void Show() {
            if (inventoryPanel != null) {
                inventoryPanel.SetActive(true);
            }

            // Refresh displays
            RefreshInventory();
            RefreshEquipment();

            // Play sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Open");
            }
        }

        /// <summary>
        /// Hide the inventory UI
        /// </summary>
        public void Hide() {
            if (inventoryPanel != null) {
                inventoryPanel.SetActive(false);
            }

            // Clear selection
            if (selectedSlot != null) {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }

            // Hide details panel
            if (itemDetailsPanel != null) {
                itemDetailsPanel.Hide();
            }

            // Play sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlayUiSound("Close");
            }
        }

        /// <summary>
        /// Toggle the inventory UI visibility
        /// </summary>
        public void Toggle() {
            if (inventoryPanel != null) {
                if (inventoryPanel.activeSelf) {
                    Hide();
                } else {
                    Show();
                }
            }
        }
    }

    /// <summary>
    /// Criteria for sorting inventory
    /// </summary>
    public enum SortType {
        Name,
        Type,
        Rarity,
        Value,
        Weight
    }
}
#endif