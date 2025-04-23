using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Breadcrumbs.Inventory.Presentation
{
    /// <summary>
    /// 아이템 컨텍스트 메뉴 UI 컴포넌트
    /// </summary>
    public class ItemContextMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _useButton;
        [SerializeField] private TextMeshProUGUI _useButtonText;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Button _dropButton;
        [SerializeField] private Button _splitStackButton;
        [SerializeField] private Button _closeButton;
        
        private IInventoryItem _item;
        private int _itemX;
        private int _itemY;
        private IInventoryPresenter _presenter;
        private IInventoryService _inventoryService;
        
        /// <summary>
        /// 컨텍스트 메뉴 초기화
        /// </summary>
        public void Initialize(IInventoryItem item, int x, int y, IInventoryPresenter presenter)
        {
            _item = item;
            _itemX = x;
            _itemY = y;
            _presenter = presenter;
            _inventoryService = InventoryService.Instance;
            
            SetupButtons();
        }
        
        /// <summary>
        /// 버튼 설정
        /// </summary>
        private void SetupButtons()
        {
            if (_item == null || _presenter == null)
                return;
            
            // 사용 버튼 설정
            if (_useButton != null)
            {
                bool canUse = _item.ItemType == ItemType.Consumable || 
                            (_item is ConsumableItem consumable && !consumable.IsOnCooldown && consumable.HasUses);
                
                _useButton.gameObject.SetActive(canUse);
                
                if (canUse && _useButtonText != null)
                {
                    _useButtonText.text = "사용";
                    _useButton.onClick.AddListener(OnUseButtonClicked);
                }
            }
            
            // 장착 버튼 설정
            if (_equipButton != null)
            {
                bool canEquip = _item is IEquipmentItem && 
                              !(_item as IEquipmentItem).IsBroken;
                
                _equipButton.gameObject.SetActive(canEquip);
                
                if (canEquip)
                {
                    _equipButton.onClick.AddListener(OnEquipButtonClicked);
                }
            }
            
            // 장착 해제 버튼 설정 (이 구현에서는 단순화를 위해 비활성화)
            if (_unequipButton != null)
            {
                _unequipButton.gameObject.SetActive(false);
            }
            
            // 버리기 버튼 설정
            if (_dropButton != null)
            {
                _dropButton.gameObject.SetActive(_item.CanDrop);
                
                if (_item.CanDrop)
                {
                    _dropButton.onClick.AddListener(OnDropButtonClicked);
                }
            }
            
            // 스택 분할 버튼 설정
            if (_splitStackButton != null)
            {
                bool canSplit = _item.IsStackable && _item.StackCount > 1;
                
                _splitStackButton.gameObject.SetActive(canSplit);
                
                if (canSplit)
                {
                    _splitStackButton.onClick.AddListener(OnSplitStackButtonClicked);
                }
            }
            
            // 닫기 버튼 설정
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }
        
        /// <summary>
        /// 사용 버튼 클릭 처리
        /// </summary>
        private void OnUseButtonClicked()
        {
            if (_inventoryService != null)
            {
                _inventoryService.UseItem(_itemX, _itemY);
            }
            
            CloseMenu();
        }
        
        /// <summary>
        /// 장착 버튼 클릭 처리
        /// </summary>
        private void OnEquipButtonClicked()
        {
            // 실제 장착 로직은 구현해야 함
            // 예: EquipmentService.Instance.EquipItem(_item as IEquipmentItem);
            
            Debug.Log($"장비 장착: {_item.DisplayName}");
            
            CloseMenu();
        }
        
        /// <summary>
        /// 버리기 버튼 클릭 처리
        /// </summary>
        private void OnDropButtonClicked()
        {
            if (_inventoryService != null)
            {
                IInventoryItem droppedItem = _inventoryService.RemoveItem(_itemX, _itemY);
                
                if (droppedItem != null)
                {
                    Debug.Log($"아이템 버림: {droppedItem.DisplayName}");
                    
                    // 여기에서 필드에 아이템 드롭 로직을 추가할 수 있음
                    // 예: ItemDropManager.Instance.DropItemInWorld(droppedItem, playerPosition);
                }
            }
            
            CloseMenu();
        }
        
        /// <summary>
        /// 스택 분할 버튼 클릭 처리
        /// </summary>
        private void OnSplitStackButtonClicked()
        {
            if (_inventoryService != null && _item.IsStackable && _item.StackCount > 1)
            {
                // 스택 분할 처리
                // 실제 구현에서는 사용자 입력을 받아 분할할 수량을 정해야 함
                int halfStack = _item.StackCount / 2;
                
                if (_inventoryService.SplitStack(_itemX, _itemY, halfStack, out IInventoryItem splitItem))
                {
                    Debug.Log($"스택 분할: {_item.DisplayName} (남은 수량: {_item.StackCount}, 분할 수량: {halfStack})");
                }
            }
            
            CloseMenu();
        }
        
        /// <summary>
        /// 닫기 버튼 클릭 처리
        /// </summary>
        private void OnCloseButtonClicked()
        {
            CloseMenu();
        }
        
        /// <summary>
        /// 메뉴 닫기
        /// </summary>
        private void CloseMenu()
        {
            if (_presenter != null)
            {
                _presenter.HideContextMenu();
            }
        }
        
        /// <summary>
        /// 컴포넌트 비활성화 시 이벤트 리스너 제거
        /// </summary>
        private void OnDisable()
        {
            if (_useButton != null)
                _useButton.onClick.RemoveAllListeners();
                
            if (_equipButton != null)
                _equipButton.onClick.RemoveAllListeners();
                
            if (_unequipButton != null)
                _unequipButton.onClick.RemoveAllListeners();
                
            if (_dropButton != null)
                _dropButton.onClick.RemoveAllListeners();
                
            if (_splitStackButton != null)
                _splitStackButton.onClick.RemoveAllListeners();
                
            if (_closeButton != null)
                _closeButton.onClick.RemoveAllListeners();
        }
    }
}
