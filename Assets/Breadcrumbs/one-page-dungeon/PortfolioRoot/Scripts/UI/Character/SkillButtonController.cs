using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePortfolio.Gameplay.Combat;

namespace GamePortfolio.UI.Character
{
    public class SkillButtonController : MonoBehaviour
    {
        [SerializeField] private Image skillIcon;
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text skillLevelText;
        [SerializeField] private Image lockOverlay;
        [SerializeField] private Button button;
        
        private SkillData skillData;
        private int skillIndex;
        private bool isUnlocked;
        private int skillLevel;
        
        public event Action<SkillData, int> OnSkillSelected;
        
        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            
            if (button != null)
                button.onClick.AddListener(OnButtonClicked);
        }
        
        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClicked);
        }
        
        public void Setup(SkillData data, int index)
        {
            skillData = data;
            skillIndex = index;
            
            if (skillNameText != null)
                skillNameText.text = data.skillName;
            
            if (skillIcon != null && data.icon != null)
                skillIcon.sprite = data.icon;
            
            UpdateState(false, 0);
        }
        
        public void UpdateState(bool unlocked, int level)
        {
            isUnlocked = unlocked;
            skillLevel = level;
            
            if (lockOverlay != null)
                lockOverlay.gameObject.SetActive(!isUnlocked);
            
            if (skillLevelText != null)
            {
                skillLevelText.text = isUnlocked ? $"Lv.{skillLevel}" : "Locked";
                skillLevelText.color = isUnlocked ? Color.white : Color.red;
            }
            
            if (button != null)
                button.interactable = isUnlocked;
            
            if (skillIcon != null)
                skillIcon.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        
        public void UpdateLevel(int level)
        {
            skillLevel = level;
            
            if (skillLevelText != null)
                skillLevelText.text = $"Lv.{skillLevel}";
        }
        
        private void OnButtonClicked()
        {
            if (skillData != null)
                OnSkillSelected?.Invoke(skillData, skillIndex);
        }
        
        public string GetSkillName()
        {
            return skillData != null ? skillData.skillName : string.Empty;
        }
    }
}
