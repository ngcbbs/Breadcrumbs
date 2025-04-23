using Breadcrumbs.Character.Services;
using Breadcrumbs.DependencyInjection;
using Breadcrumbs.EventSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Breadcrumbs.Character.Presentation
{
    /// <summary>
    /// Implementation of the character presenter for UI
    /// </summary>
    public class CharacterPresenter : MonoBehaviour, ICharacterPresenter
    {
        [Header("Character References")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterLevelText;
        [SerializeField] private TextMeshProUGUI characterClassText;
        [SerializeField] private Image characterPortrait;
        
        [Header("Stat References")]
        [SerializeField] private TextMeshProUGUI[] statTexts = new TextMeshProUGUI[6]; // Strength, Dexterity, etc.
        [SerializeField] private Button[] statIncrementButtons = new Button[6];
        [SerializeField] private TextMeshProUGUI statPointsText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider manaBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI manaText;
        
        [Header("Equipment References")]
        [SerializeField] private Transform equipmentSlotsContainer;
        [SerializeField] private GameObject equipmentSlotPrefab;
        
        [Header("Skills References")]
        [SerializeField] private Transform skillsContainer;
        [SerializeField] private GameObject skillItemPrefab;
        [SerializeField] private TextMeshProUGUI skillPointsText;
        
        [Header("Level Up Effects")]
        [SerializeField] private GameObject levelUpVfx;
        [SerializeField] private AudioClip levelUpSound;
        
        // Injected Services
        [Inject] private IStatService statService;
        [Inject] private ICharacterService characterService;
        
        // Properties
        public ICharacter Character { get; private set; }
        
        // Cached references
        private Dictionary<EquipmentSlot, GameObject> equipmentSlotObjects = new Dictionary<EquipmentSlot, GameObject>();
        private Dictionary<string, GameObject> skillObjects = new Dictionary<string, GameObject>();
        private AudioSource audioSource;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        /// <summary>
        /// Initializes the presenter with a character
        /// </summary>
        public void Initialize(ICharacter character)
        {
            if (character == null)
            {
                Debug.LogError("Cannot initialize presenter with null character");
                return;
            }
            
            // Unsubscribe from previous character events if any
            if (Character != null)
            {
                UnsubscribeFromCharacterEvents();
            }
            
            Character = character;
            
            // Subscribe to character events
            SubscribeToCharacterEvents();
            
            // Initialize UI elements
            InitializeUI();
            
            // Update all displays
            UpdateAllDisplays();
            
            Debug.Log($"Presenter initialized for character: {character.Name}");
        }
        
        /// <summary>
        /// Subscribe to character events
        /// </summary>
        private void SubscribeToCharacterEvents()
        {
            if (Character == null) return;
            
            Character.OnLevelUp += HandleLevelUp;
            Character.OnDamageTaken += (amount, type) => UpdateStatsDisplay();
            Character.OnHealed += _ => UpdateStatsDisplay();
            Character.OnItemEquipped += _ => UpdateEquipmentDisplay();
            Character.OnItemUnequipped += (_, __) => UpdateEquipmentDisplay();
            
            // Subscribe to stat changes
            if (Character.Stats != null)
            {
                Character.Stats.OnStatChanged += (statType, value) => 
                {
                    UpdateStatsDisplay();
                };
                
                Character.Stats.OnHealthChanged += _ => UpdateHealthDisplay();
                Character.Stats.OnManaChanged += _ => UpdateManaDisplay();
            }
        }
        
        /// <summary>
        /// Unsubscribe from character events
        /// </summary>
        private void UnsubscribeFromCharacterEvents()
        {
            if (Character == null) return;
            
            Character.OnLevelUp -= HandleLevelUp;
            // Unsubscribe from other events
        }
        
        /// <summary>
        /// Initialize UI elements
        /// </summary>
        private void InitializeUI()
        {
            // Set up stat increment buttons
            for (int i = 0; i < statIncrementButtons.Length; i++)
            {
                if (statIncrementButtons[i] != null)
                {
                    StatType statType = (StatType)i;
                    statIncrementButtons[i].onClick.RemoveAllListeners();
                    statIncrementButtons[i].onClick.AddListener(() => HandleStatButtonClick(statType));
                }
            }
            
            // Set up equipment slots
            InitializeEquipmentSlots();
            
            // Set up skills
            InitializeSkills();
        }
        
        /// <summary>
        /// Initialize equipment slots
        /// </summary>
        private void InitializeEquipmentSlots()
        {
            if (equipmentSlotsContainer == null || equipmentSlotPrefab == null) return;
            
            // Clear existing slots
            foreach (Transform child in equipmentSlotsContainer)
            {
                Destroy(child.gameObject);
            }
            
            equipmentSlotObjects.Clear();
            
            // Create slots for each equipment type
            foreach (EquipmentSlot slotType in Enum.GetValues(typeof(EquipmentSlot)))
            {
                GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
                slotObj.name = $"Slot_{slotType}";
                
                // Set up slot UI
                TextMeshProUGUI slotLabel = slotObj.GetComponentInChildren<TextMeshProUGUI>();
                if (slotLabel != null)
                {
                    slotLabel.text = slotType.ToString();
                }
                
                // Store reference
                equipmentSlotObjects[slotType] = slotObj;
                
                // Add unequip button functionality
                Button unequipButton = slotObj.GetComponentInChildren<Button>();
                if (unequipButton != null)
                {
                    EquipmentSlot capturedSlot = slotType; // Capture for lambda
                    unequipButton.onClick.RemoveAllListeners();
                    unequipButton.onClick.AddListener(() => HandleUnequipButton(capturedSlot));
                }
            }
        }
        
        /// <summary>
        /// Initialize skills UI
        /// </summary>
        private void InitializeSkills()
        {
            if (skillsContainer == null || skillItemPrefab == null) return;
            
            // Clear existing skills
            foreach (Transform child in skillsContainer)
            {
                Destroy(child.gameObject);
            }
            
            skillObjects.Clear();
            
            // Skills would typically be loaded from a skill service or the character
            // This is a simplified implementation for demonstration
            
            // For demonstration, we'll add placeholder skill items
            // In a real implementation, skills would be retrieved from the character
        }
        
        /// <summary>
        /// Update all displays
        /// </summary>
        private void UpdateAllDisplays()
        {
            UpdateCharacterDisplay();
            UpdateStatsDisplay();
            UpdateEquipmentDisplay();
            UpdateSkillsDisplay();
        }
        
        /// <summary>
        /// Updates the character display
        /// </summary>
        public void UpdateCharacterDisplay()
        {
            if (Character == null) return;
            
            // Update character info texts
            if (characterNameText != null)
                characterNameText.text = Character.Name;
                
            if (characterLevelText != null)
                characterLevelText.text = $"Level {Character.Level}";
                
            if (characterClassText != null)
                characterClassText.text = Character.ClassType.ToString();
                
            // Update character portrait
            if (characterPortrait != null)
            {
                // This would typically load a sprite based on character appearance
                // characterPortrait.sprite = ...
            }
        }
        
        /// <summary>
        /// Updates the stats display
        /// </summary>
        public void UpdateStatsDisplay()
        {
            if (Character == null || Character.Stats == null) return;
            
            // Update primary stats
            UpdatePrimaryStats();
            
            // Update health and mana
            UpdateHealthDisplay();
            UpdateManaDisplay();
            
            // Update available stat points
            UpdateStatPoints();
        }
        
        /// <summary>
        /// Update primary stats display
        /// </summary>
        private void UpdatePrimaryStats()
        {
            if (statTexts == null || Character == null || Character.Stats == null) return;
            
            // Update each primary stat
            for (int i = 0; i < statTexts.Length && i < 6; i++) // 6 primary stats
            {
                if (statTexts[i] != null)
                {
                    StatType statType = (StatType)i;
                    float statValue = Character.Stats.GetStat(statType);
                    statTexts[i].text = $"{statType}: {statValue:F0}";
                }
            }
            
            // Enable/disable stat increment buttons based on available points
            UpdateStatIncrementButtons();
        }
        
        /// <summary>
        /// Update health display
        /// </summary>
        private void UpdateHealthDisplay()
        {
            if (Character == null || Character.Stats == null) return;
            
            float currentHealth = Character.Stats.CurrentHealth;
            float maxHealth = Character.Stats.GetStat(StatType.MaxHealth);
            
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
        }
        
        /// <summary>
        /// Update mana display
        /// </summary>
        private void UpdateManaDisplay()
        {
            if (Character == null || Character.Stats == null) return;
            
            float currentMana = Character.Stats.CurrentMana;
            float maxMana = Character.Stats.GetStat(StatType.MaxMana);
            
            if (manaBar != null)
            {
                manaBar.maxValue = maxMana;
                manaBar.value = currentMana;
            }
            
            if (manaText != null)
            {
                manaText.text = $"{currentMana:F0}/{maxMana:F0}";
            }
        }
        
        /// <summary>
        /// Update stat points display
        /// </summary>
        private void UpdateStatPoints()
        {
            if (Character == null || statPointsText == null) return;
            
            int availablePoints = 0;
            
            // Get available points from service or character
            if (statService != null)
            {
                availablePoints = statService.GetAvailableStatPoints(Character);
            }
            
            statPointsText.text = $"Stat Points: {availablePoints}";
        }
        
        /// <summary>
        /// Update stat increment buttons
        /// </summary>
        private void UpdateStatIncrementButtons()
        {
            if (Character == null || statIncrementButtons == null) return;
            
            // Check if we have any points to spend
            bool hasPoints = false;
            
            if (statService != null)
            {
                hasPoints = statService.GetAvailableStatPoints(Character) > 0;
            }
            
            // Enable/disable buttons
            for (int i = 0; i < statIncrementButtons.Length; i++)
            {
                if (statIncrementButtons[i] != null)
                {
                    statIncrementButtons[i].interactable = hasPoints;
                }
            }
        }
        
        /// <summary>
        /// Updates the equipment display
        /// </summary>
        public void UpdateEquipmentDisplay()
        {
            if (Character == null) return;
            
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (equipmentSlotObjects.TryGetValue(slot, out GameObject slotObj))
                {
                    IEquipmentItem item = Character.GetEquippedItem(slot);
                    
                    // Update slot UI
                    Image itemIcon = slotObj.GetComponentInChildren<Image>();
                    TextMeshProUGUI itemName = slotObj.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
                    Button unequipButton = slotObj.GetComponentInChildren<Button>();
                    
                    if (item != null)
                    {
                        // Item is equipped
                        if (itemIcon != null)
                        {
                            // Load item icon
                            // itemIcon.sprite = item.Icon;
                            itemIcon.color = Color.white;
                        }
                        
                        if (itemName != null)
                        {
                            itemName.text = item.ItemName;
                        }
                        
                        if (unequipButton != null)
                        {
                            unequipButton.interactable = true;
                        }
                    }
                    else
                    {
                        // No item equipped
                        if (itemIcon != null)
                        {
                            itemIcon.sprite = null;
                            itemIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                        }
                        
                        if (itemName != null)
                        {
                            itemName.text = "Empty";
                        }
                        
                        if (unequipButton != null)
                        {
                            unequipButton.interactable = false;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the skills display
        /// </summary>
        public void UpdateSkillsDisplay()
        {
            if (Character == null) return;
            
            // Update skill point display
            UpdateSkillPoints();
            
            // In a real implementation, this would update skill items based on character's skills
            // For now, this is a placeholder
        }
        
        /// <summary>
        /// Update skill points display
        /// </summary>
        private void UpdateSkillPoints()
        {
            if (Character == null || skillPointsText == null) return;
            
            int skillPoints = 0;
            
            // todo: Get skill points from character
            
            skillPointsText.text = $"Skill Points: {skillPoints}";
        }
        
        /// <summary>
        /// Handles a stat button click
        /// </summary>
        private void HandleStatButtonClick(StatType statType)
        {
            if (Character == null) return;
            
            bool success = false;
            
            // Apply stat point using service if available
            if (statService != null)
            {
                success = statService.ApplyStatPoint(Character, statType);
            }
            
            if (success)
            {
                HandleStatPointApplied(statType);
            }
        }
        
        /// <summary>
        /// Handles an unequip button click
        /// </summary>
        private void HandleUnequipButton(EquipmentSlot slot)
        {
            if (Character == null) return;
            
            IEquipmentItem unequippedItem = null;
            
            // Unequip using service if available
            if (characterService != null)
            {
                unequippedItem = characterService.UnequipItem(Character, slot);
            }
            else
            {
                unequippedItem = Character.UnequipItem(slot);
            }
            
            if (unequippedItem != null)
            {
                Debug.Log($"Unequipped {unequippedItem.ItemName} from {slot}");
                
                // In a real implementation, this would add the item back to inventory
            }
        }
        
        /// <summary>
        /// Handles a stat point being applied
        /// </summary>
        public void HandleStatPointApplied(StatType statType)
        {
            UpdateStatsDisplay();
            
            // Play effect/sound
            Debug.Log($"Stat point applied to {statType}");
        }
        
        /// <summary>
        /// Handles a skill point being applied
        /// </summary>
        public void HandleSkillPointApplied(string skillId)
        {
            UpdateSkillsDisplay();
            
            // Play effect/sound
            Debug.Log($"Skill point applied to {skillId}");
        }
        
        /// <summary>
        /// Handles a level up
        /// </summary>
        public void HandleLevelUp(int newLevel)
        {
            // Update all displays
            UpdateAllDisplays();
            
            // Play level up effects
            PlayLevelUpEffects();
            
            Debug.Log($"Character leveled up to level {newLevel}!");
        }
        
        /// <summary>
        /// Play level up effects
        /// </summary>
        private void PlayLevelUpEffects()
        {
            // Play visual effect
            if (levelUpVfx != null)
            {
                GameObject vfx = Instantiate(levelUpVfx, transform);
                Destroy(vfx, 2f);
            }
            
            // Play sound
            if (audioSource != null && levelUpSound != null)
            {
                audioSource.PlayOneShot(levelUpSound);
            }
        }
    }
}
