using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Breadcrumbs.Inventory.Presentation
{
    /// <summary>
    /// 아이템 툴팁 UI 컴포넌트
    /// </summary>
    public class ItemTooltipUI : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _itemTypeText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _statsText;
        
        // 장비 아이템 전용 필드
        [SerializeField] private GameObject _equipmentStatsPanel;
        [SerializeField] private TextMeshProUGUI _durabilityText;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        /// <summary>
        /// 툴팁에 아이템 정보 설정
        /// </summary>
        public void SetItem(IInventoryItem item)
        {
            if (item == null)
                return;
                
            // 기본 아이템 정보 설정
            if (_itemIcon != null)
            {
                _itemIcon.sprite = item.Icon;
            }
            
            if (_itemNameText != null)
            {
                _itemNameText.text = item.DisplayName;
                
                // 희귀도에 따라 이름 색상 설정
                _itemNameText.color = GetRarityColor(item.Rarity);
            }
            
            if (_itemTypeText != null)
            {
                _itemTypeText.text = GetItemTypeString(item.ItemType);
            }
            
            if (_rarityText != null)
            {
                _rarityText.text = GetRarityString(item.Rarity);
                _rarityText.color = GetRarityColor(item.Rarity);
            }
            
            if (_descriptionText != null)
            {
                _descriptionText.text = item.Description;
            }
            
            // 장비 아이템인 경우 추가 정보 표시
            if (item is IEquipmentItem equipItem)
            {
                if (_equipmentStatsPanel != null)
                {
                    _equipmentStatsPanel.SetActive(true);
                }
                
                if (_durabilityText != null)
                {
                    _durabilityText.text = $"내구도: {equipItem.Durability}/{equipItem.MaxDurability}";
                    
                    // 내구도가 낮으면 색상 변경
                    float durabilityPercent = (float)equipItem.Durability / equipItem.MaxDurability;
                    if (durabilityPercent < 0.3f)
                    {
                        _durabilityText.color = Color.red;
                    }
                    else if (durabilityPercent < 0.6f)
                    {
                        _durabilityText.color = Color.yellow;
                    }
                    else
                    {
                        _durabilityText.color = Color.white;
                    }
                }
                
                if (_levelText != null)
                {
                    _levelText.text = $"레벨: {equipItem.Level}";
                }
                
                if (_statsText != null)
                {
                    string statsString = "";
                    
                    foreach (var stat in equipItem.StatModifiers)
                    {
                        // 스탯 수정자 타입에 따라 표시 형식 변경
                        string valueStr = "";
                        
                        switch (stat.Type)
                        {
                            case StatModifierType.Flat:
                                valueStr = stat.Value >= 0 ? $"+{stat.Value}" : $"{stat.Value}";
                                break;
                            case StatModifierType.PercentAdd:
                            case StatModifierType.PercentMult:
                                valueStr = stat.Value >= 0 ? $"+{stat.Value * 100}%" : $"{stat.Value * 100}%";
                                break;
                        }
                        
                        statsString += $"{stat.StatName}: {valueStr}\n";
                    }
                    
                    _statsText.text = statsString;
                }
            }
            else
            {
                // 장비 아이템이 아닌 경우 장비 스탯 패널 숨김
                if (_equipmentStatsPanel != null)
                {
                    _equipmentStatsPanel.SetActive(false);
                }
                
                // 스택 가능한 아이템인 경우 스택 정보 표시
                if (item.IsStackable && _statsText != null)
                {
                    _statsText.text = $"수량: {item.StackCount}/{item.MaxStackCount}";
                }
                else if (_statsText != null)
                {
                    _statsText.text = "";
                }
            }
        }
        
        /// <summary>
        /// 아이템 타입 문자열 반환
        /// </summary>
        private string GetItemTypeString(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon: return "무기";
                case ItemType.Armor: return "방어구";
                case ItemType.Accessory: return "장신구";
                case ItemType.Consumable: return "소비 아이템";
                case ItemType.Material: return "재료";
                case ItemType.Quest: return "퀘스트 아이템";
                case ItemType.Miscellaneous: return "기타";
                default: return "알 수 없음";
            }
        }
        
        /// <summary>
        /// 희귀도 문자열 반환
        /// </summary>
        private string GetRarityString(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return "일반";
                case ItemRarity.Uncommon: return "고급";
                case ItemRarity.Rare: return "희귀";
                case ItemRarity.Epic: return "영웅";
                case ItemRarity.Legendary: return "전설";
                case ItemRarity.Unique: return "고유";
                default: return "알 수 없음";
            }
        }
        
        /// <summary>
        /// 희귀도에 따른 색상 반환
        /// </summary>
        private Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return Color.white;
                case ItemRarity.Uncommon: return Color.green;
                case ItemRarity.Rare: return Color.blue;
                case ItemRarity.Epic: return new Color(0.5f, 0f, 0.5f); // 보라색
                case ItemRarity.Legendary: return new Color(1f, 0.5f, 0f); // 주황색
                case ItemRarity.Unique: return Color.red;
                default: return Color.white;
            }
        }
    }
}
