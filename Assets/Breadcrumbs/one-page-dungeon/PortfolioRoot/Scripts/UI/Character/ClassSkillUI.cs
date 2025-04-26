using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePortfolio.Gameplay.Character;
using GamePortfolio.Gameplay.Character.Classes;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.UI.Character
{
    public class ClassSkillUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform skillContainer;
        [SerializeField] private GameObject skillButtonPrefab;
        [SerializeField] private TMP_Text skillPointsText;
        
        [Header("Skill Details Panel")]
        [SerializeField] private GameObject skillDetailsPanel;
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text skillDescriptionText;
        [SerializeField] private TMP_Text skillLevelText;
        [SerializeField] private TMP_Text skillCooldownText;
        [SerializeField] private TMP_Text skillCostText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeDetailsButton;
        
        [Header("Components")]
        [SerializeField] private Image skillIconImage;
        [SerializeField] private Transform levelIndicatorsParent;
        [SerializeField] private GameObject levelIndicatorPrefab;
        
        private CharacterClassManager classManager;
        private SkillPointManager skillPointManager;
        private CharacterLevelManager levelManager;
        
        private List<SkillButtonController> skillButtons = new List<SkillButtonController>();
        private SkillData selectedSkill;
        private string selectedSkillName;
        private int maxSkillLevel = 5;
        
        private void Start()
        {
            FindComponents();
            SetupEventListeners();
            InitializeUI();
        }
        
        private void FindComponents()
        {
            classManager = FindObjectOfType<CharacterClassManager>();
            skillPointManager = FindObjectOfType<SkillPointManager>();
            levelManager = FindObjectOfType<CharacterLevelManager>();
            
            if (classManager == null)
                Debug.LogError("ClassSkillUI: CharacterClassManager not found!");
            
            if (skillPointManager == null)
                Debug.LogError("ClassSkillUI: SkillPointManager not found!");
            
            if (levelManager == null)
                Debug.LogError("ClassSkillUI: CharacterLevelManager not found!");
        }
        
        private void SetupEventListeners()
        {
            if (classManager != null)
                classManager.OnClassChanged += OnClassChanged;
            
            if (skillPointManager != null)
            {
                skillPointManager.OnSkillUnlocked += OnSkillUnlocked;
                skillPointManager.OnSkillLevelChanged += OnSkillLevelChanged;
            }
            
            if (levelManager != null)
                levelManager.OnSkillPointsChanged += OnSkillPointsChanged;
            
            if (closeDetailsButton != null)
                closeDetailsButton.onClick.AddListener(CloseSkillDetails);
            
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(UpgradeSelectedSkill);
        }
        
        private void OnDestroy()
        {
            if (classManager != null)
                classManager.OnClassChanged -= OnClassChanged;
            
            if (skillPointManager != null)
            {
                skillPointManager.OnSkillUnlocked -= OnSkillUnlocked;
                skillPointManager.OnSkillLevelChanged -= OnSkillLevelChanged;
            }
            
            if (levelManager != null)
                levelManager.OnSkillPointsChanged -= OnSkillPointsChanged;
            
            if (closeDetailsButton != null)
                closeDetailsButton.onClick.RemoveListener(CloseSkillDetails);
            
            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(UpgradeSelectedSkill);
        }
        
        private void InitializeUI()
        {
            UpdateSkillPointsText();
            
            if (skillDetailsPanel != null)
                skillDetailsPanel.SetActive(false);
            
            PopulateSkillButtons();
        }
        
        private void PopulateSkillButtons()
        {
            ClearSkillButtons();
            
            if (classManager == null || classManager.GetCurrentClass() == null)
                return;
            
            BaseCharacterClass currentClass = classManager.GetCurrentClass();
            List<SkillData> classSkills = currentClass.GetClassSkills();
            
            if (classSkills == null)
                return;
            
            for (int i = 0; i < classSkills.Count; i++)
            {
                SkillData skill = classSkills[i];
                
                if (skill == null)
                    continue;
                
                GameObject buttonObj = Instantiate(skillButtonPrefab, skillContainer);
                SkillButtonController buttonController = buttonObj.GetComponent<SkillButtonController>();
                
                if (buttonController != null)
                {
                    buttonController.Setup(skill, i);
                    buttonController.OnSkillSelected += OnSkillButtonSelected;
                    
                    bool isUnlocked = skillPointManager != null && skillPointManager.IsSkillUnlocked(skill.skillName);
                    int skillLevel = skillPointManager != null ? skillPointManager.GetSkillLevel(skill.skillName) : 0;
                    
                    buttonController.UpdateState(isUnlocked, skillLevel);
                    skillButtons.Add(buttonController);
                }
            }
        }
        
        private void ClearSkillButtons()
        {
            foreach (var button in skillButtons)
            {
                if (button != null)
                    button.OnSkillSelected -= OnSkillButtonSelected;
            }
            
            skillButtons.Clear();
            
            if (skillContainer != null)
            {
                foreach (Transform child in skillContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        private void OnSkillButtonSelected(SkillData skill, int index)
        {
            selectedSkill = skill;
            selectedSkillName = skill.skillName;
            ShowSkillDetails(skill);
        }
        
        private void ShowSkillDetails(SkillData skill)
        {
            if (skillDetailsPanel == null || skill == null)
                return;
            
            skillDetailsPanel.SetActive(true);
            
            if (skillNameText != null)
                skillNameText.text = skill.skillName;
            
            if (skillDescriptionText != null)
                skillDescriptionText.text = skill.description;
            
            if (skillIconImage != null && skill.icon != null)
                skillIconImage.sprite = skill.icon;
            
            int skillLevel = skillPointManager != null ? skillPointManager.GetSkillLevel(skill.skillName) : 0;
            bool isUnlocked = skillPointManager != null && skillPointManager.IsSkillUnlocked(skill.skillName);
            
            if (skillLevelText != null)
                skillLevelText.text = isUnlocked ? $"Level: {skillLevel}" : "Locked";
            
            if (skillCooldownText != null)
                skillCooldownText.text = $"Cooldown: {skill.cooldown}s";
            
            if (skillCostText != null)
                skillCostText.text = $"Cost: {skill.staminaCost} Stamina";
            
            UpdateSkillLevelIndicators(skillLevel);
            
            // Update upgrade button
            if (upgradeButton != null)
            {
                bool canUpgrade = isUnlocked && skillLevel < maxSkillLevel && levelManager != null && levelManager.GetUnspentSkillPoints() > 0;
                upgradeButton.interactable = canUpgrade;
            }
        }
        
        private void UpdateSkillLevelIndicators(int currentLevel)
        {
            if (levelIndicatorsParent == null || levelIndicatorPrefab == null)
                return;
            
            foreach (Transform child in levelIndicatorsParent)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < maxSkillLevel; i++)
            {
                GameObject indicator = Instantiate(levelIndicatorPrefab, levelIndicatorsParent);
                Image indicatorImage = indicator.GetComponent<Image>();
                
                if (indicatorImage != null)
                {
                    // Change color based on if level is achieved
                    if (i < currentLevel)
                    {
                        indicatorImage.color = Color.green;
                    }
                    else
                    {
                        indicatorImage.color = Color.gray;
                    }
                }
            }
        }
        
        private void CloseSkillDetails()
        {
            if (skillDetailsPanel != null)
                skillDetailsPanel.SetActive(false);
            
            selectedSkill = null;
            selectedSkillName = null;
        }
        
        private void UpgradeSelectedSkill()
        {
            if (skillPointManager == null || selectedSkillName == null)
                return;
            
            bool success = skillPointManager.UpgradeSkill(selectedSkillName);
            
            if (success)
            {
                // Update UI after upgrade
                ShowSkillDetails(selectedSkill);
                
                // Update skill button display
                foreach (var button in skillButtons)
                {
                    if (button.GetSkillName() == selectedSkillName)
                    {
                        int newLevel = skillPointManager.GetSkillLevel(selectedSkillName);
                        button.UpdateLevel(newLevel);
                        break;
                    }
                }
            }
        }
        
        private void OnClassChanged(BaseCharacterClass newClass)
        {
            CloseSkillDetails();
            PopulateSkillButtons();
        }
        
        private void OnSkillUnlocked(string skillName)
        {
            foreach (var button in skillButtons)
            {
                if (button.GetSkillName() == skillName)
                {
                    button.UpdateState(true, skillPointManager.GetSkillLevel(skillName));
                    break;
                }
            }
            
            if (selectedSkillName == skillName && selectedSkill != null)
            {
                ShowSkillDetails(selectedSkill);
            }
        }
        
        private void OnSkillLevelChanged(string skillName, int newLevel)
        {
            foreach (var button in skillButtons)
            {
                if (button.GetSkillName() == skillName)
                {
                    button.UpdateLevel(newLevel);
                    break;
                }
            }
            
            if (selectedSkillName == skillName && selectedSkill != null)
            {
                ShowSkillDetails(selectedSkill);
            }
        }
        
        private void OnSkillPointsChanged(int points)
        {
            UpdateSkillPointsText();
            
            if (selectedSkill != null && upgradeButton != null)
            {
                int skillLevel = skillPointManager != null ? skillPointManager.GetSkillLevel(selectedSkillName) : 0;
                bool isUnlocked = skillPointManager != null && skillPointManager.IsSkillUnlocked(selectedSkillName);
                
                bool canUpgrade = isUnlocked && skillLevel < maxSkillLevel && points > 0;
                upgradeButton.interactable = canUpgrade;
            }
        }
        
        private void UpdateSkillPointsText()
        {
            if (skillPointsText != null && levelManager != null)
            {
                int points = levelManager.GetUnspentSkillPoints();
                skillPointsText.text = $"Skill Points: {points}";
            }
        }
    }
}
