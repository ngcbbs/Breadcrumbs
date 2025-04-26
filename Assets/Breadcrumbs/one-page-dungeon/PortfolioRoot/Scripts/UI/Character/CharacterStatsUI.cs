using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePortfolio.Gameplay.Character;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.UI.Character
{
    public class CharacterStatsUI : MonoBehaviour
    {
        [Header("Base Stats UI")]
        [SerializeField] private TMP_Text strengthText;
        [SerializeField] private TMP_Text dexterityText;
        [SerializeField] private TMP_Text intelligenceText;
        [SerializeField] private TMP_Text constitutionText;
        [SerializeField] private TMP_Text statPointsText;
        
        [Header("Derived Stats UI")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text staminaText;
        [SerializeField] private TMP_Text attackDamageText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text moveSpeedText;
        
        [Header("Special Stats UI")]
        [SerializeField] private TMP_Text critChanceText;
        [SerializeField] private TMP_Text critDamageText;
        [SerializeField] private TMP_Text dodgeChanceText;
        [SerializeField] private TMP_Text blockChanceText;
        [SerializeField] private TMP_Text lifeStealText;
        [SerializeField] private TMP_Text cooldownReductionText;
        
        [Header("Stat Increase Buttons")]
        [SerializeField] private Button strengthIncreaseButton;
        [SerializeField] private Button dexterityIncreaseButton;
        [SerializeField] private Button intelligenceIncreaseButton;
        [SerializeField] private Button constitutionIncreaseButton;
        
        private StatAllocationSystem statSystem;
        private SpecialStatSystem specialStatSystem;
        private PlayerStats playerStats;
        private CharacterLevelManager levelManager;
        
        private Dictionary<CharacterStat, Button> statButtons = new Dictionary<CharacterStat, Button>();
        
        private void Start()
        {
            FindComponents();
            SetupButtonListeners();
            InitializeUI();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void FindComponents()
        {
            statSystem = FindObjectOfType<StatAllocationSystem>();
            specialStatSystem = FindObjectOfType<SpecialStatSystem>();
            playerStats = FindObjectOfType<PlayerStats>();
            levelManager = FindObjectOfType<CharacterLevelManager>();
            
            if (statSystem == null)
                Debug.LogError("CharacterStatsUI: StatAllocationSystem not found!");
            
            if (specialStatSystem == null)
                Debug.LogError("CharacterStatsUI: SpecialStatSystem not found!");
            
            if (playerStats == null)
                Debug.LogError("CharacterStatsUI: PlayerStats not found!");
            
            if (levelManager == null)
                Debug.LogError("CharacterStatsUI: CharacterLevelManager not found!");
        }
        
        private void SetupButtonListeners()
        {
            statButtons[CharacterStat.Strength] = strengthIncreaseButton;
            statButtons[CharacterStat.Dexterity] = dexterityIncreaseButton;
            statButtons[CharacterStat.Intelligence] = intelligenceIncreaseButton;
            statButtons[CharacterStat.Constitution] = constitutionIncreaseButton;
            
            if (strengthIncreaseButton != null)
                strengthIncreaseButton.onClick.AddListener(() => AllocateStat(CharacterStat.Strength));
            
            if (dexterityIncreaseButton != null)
                dexterityIncreaseButton.onClick.AddListener(() => AllocateStat(CharacterStat.Dexterity));
            
            if (intelligenceIncreaseButton != null)
                intelligenceIncreaseButton.onClick.AddListener(() => AllocateStat(CharacterStat.Intelligence));
            
            if (constitutionIncreaseButton != null)
                constitutionIncreaseButton.onClick.AddListener(() => AllocateStat(CharacterStat.Constitution));
        }
        
        private void SubscribeToEvents()
        {
            if (statSystem != null)
                statSystem.OnStatChanged += OnBaseStatChanged;
            
            if (specialStatSystem != null)
                specialStatSystem.OnSpecialStatChanged += OnSpecialStatChanged;
            
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += OnHealthChanged;
                playerStats.OnStaminaChanged += OnStaminaChanged;
            }
            
            if (levelManager != null)
                levelManager.OnStatPointsChanged += OnStatPointsChanged;
        }
        
        private void UnsubscribeFromEvents()
        {
            if (statSystem != null)
                statSystem.OnStatChanged -= OnBaseStatChanged;
            
            if (specialStatSystem != null)
                specialStatSystem.OnSpecialStatChanged -= OnSpecialStatChanged;
            
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= OnHealthChanged;
                playerStats.OnStaminaChanged -= OnStaminaChanged;
            }
            
            if (levelManager != null)
                levelManager.OnStatPointsChanged -= OnStatPointsChanged;
        }
        
        private void InitializeUI()
        {
            UpdateBaseStats();
            UpdateDerivedStats();
            UpdateSpecialStats();
            UpdateStatPoints();
            UpdateStatButtonStates();
        }
        
        private void UpdateBaseStats()
        {
            if (statSystem == null)
                return;
            
            if (strengthText != null)
                strengthText.text = $"Strength: {statSystem.GetTotalStatValue(CharacterStat.Strength)}";
            
            if (dexterityText != null)
                dexterityText.text = $"Dexterity: {statSystem.GetTotalStatValue(CharacterStat.Dexterity)}";
            
            if (intelligenceText != null)
                intelligenceText.text = $"Intelligence: {statSystem.GetTotalStatValue(CharacterStat.Intelligence)}";
            
            if (constitutionText != null)
                constitutionText.text = $"Constitution: {statSystem.GetTotalStatValue(CharacterStat.Constitution)}";
        }
        
        private void UpdateDerivedStats()
        {
            if (playerStats == null)
                return;
            
            if (healthText != null)
                healthText.text = $"Health: {playerStats.CurrentHealth}/{playerStats.MaxHealth}";
            
            if (staminaText != null)
                staminaText.text = $"Stamina: {playerStats.CurrentStamina}/{playerStats.MaxStamina}";
            
            if (attackDamageText != null)
                attackDamageText.text = $"Attack: {playerStats.AttackDamage}";
            
            if (attackSpeedText != null)
                attackSpeedText.text = $"Attack Speed: {playerStats.AttackSpeed:F2}";
            
            if (moveSpeedText != null)
                moveSpeedText.text = $"Move Speed: {playerStats.MoveSpeed:F2}";
        }
        
        private void UpdateSpecialStats()
        {
            if (specialStatSystem == null)
                return;
            
            if (critChanceText != null)
            {
                float critChance = specialStatSystem.GetSpecialStat(SpecialStat.CriticalHitChance) * 100f;
                critChanceText.text = $"Crit Chance: {critChance:F1}%";
            }
            
            if (critDamageText != null)
            {
                float critDamage = specialStatSystem.GetSpecialStat(SpecialStat.CriticalHitDamage) * 100f;
                critDamageText.text = $"Crit Damage: {critDamage:F0}%";
            }
            
            if (dodgeChanceText != null)
            {
                float dodgeChance = specialStatSystem.GetSpecialStat(SpecialStat.DodgeChance) * 100f;
                dodgeChanceText.text = $"Dodge: {dodgeChance:F1}%";
            }
            
            if (blockChanceText != null)
            {
                float blockChance = specialStatSystem.GetSpecialStat(SpecialStat.BlockChance) * 100f;
                blockChanceText.text = $"Block: {blockChance:F1}%";
            }
            
            if (lifeStealText != null)
            {
                float lifeSteal = specialStatSystem.GetSpecialStat(SpecialStat.LifeSteal) * 100f;
                lifeStealText.text = $"Life Steal: {lifeSteal:F1}%";
            }
            
            if (cooldownReductionText != null)
            {
                float cdr = specialStatSystem.GetSpecialStat(SpecialStat.CooldownReduction) * 100f;
                cooldownReductionText.text = $"CDR: {cdr:F0}%";
            }
        }
        
        private void UpdateStatPoints()
        {
            if (levelManager == null)
                return;
            
            int points = levelManager.GetUnspentStatPoints();
            
            if (statPointsText != null)
                statPointsText.text = $"Stat Points: {points}";
        }
        
        private void UpdateStatButtonStates()
        {
            if (levelManager == null)
                return;
            
            int points = levelManager.GetUnspentStatPoints();
            bool hasPoints = points > 0;
            
            foreach (var statButton in statButtons)
            {
                if (statButton.Value != null)
                    statButton.Value.interactable = hasPoints;
            }
        }
        
        private void AllocateStat(CharacterStat stat)
        {
            if (statSystem == null)
                return;
            
            bool success = statSystem.AllocateStatPoint(stat);
            
            if (success)
            {
                UpdateStatPoints();
                UpdateStatButtonStates();
            }
        }
        
        private void OnBaseStatChanged(CharacterStat stat, int value)
        {
            UpdateBaseStats();
            UpdateDerivedStats();
            UpdateSpecialStats();
        }
        
        private void OnSpecialStatChanged(SpecialStat stat, float value)
        {
            UpdateSpecialStats();
        }
        
        private void OnHealthChanged(int current, int max)
        {
            if (healthText != null)
                healthText.text = $"Health: {current}/{max}";
        }
        
        private void OnStaminaChanged(int current, int max)
        {
            if (staminaText != null)
                staminaText.text = $"Stamina: {current}/{max}";
        }
        
        private void OnStatPointsChanged(int points)
        {
            UpdateStatPoints();
            UpdateStatButtonStates();
        }
    }
}
