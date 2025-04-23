using System;
using UnityEngine;
using Breadcrumbs.Character;
using Breadcrumbs.DependencyInjection;
using Breadcrumbs.EventSystem;
using Breadcrumbs.Inventory.Events;
using ItemEquippedEvent = Breadcrumbs.Inventory.Events.ItemEquippedEvent;
using ItemUnequippedEvent = Breadcrumbs.Inventory.Events.ItemUnequippedEvent;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 캐릭터의 인벤토리 관리를 담당하는 컴포넌트
    /// </summary>
    public class CharacterInventory : EventBehaviour
    {
        [SerializeField] private int _inventoryWidth = 10;
        [SerializeField] private int _inventoryHeight = 8;
        
        [SerializeField] private Transform _equipmentRoot;
        
        [Inject] private ICharacter _character;
        
        private IInventoryService _inventoryService;
        
        // 장착된 아이템 참조 딕셔너리 (슬롯 타입별)
        private IEquipmentItem[] _equippedItems;
        
        protected void Awake()
        {
            // 장착 아이템 배열 초기화
            _equippedItems = new IEquipmentItem[Enum.GetValues(typeof(EquipmentSlotType)).Length];
            
            // 인벤토리 서비스 가져오기
            _inventoryService = InventoryService.Instance;
            
            // 인벤토리 크기 설정
            if (_inventoryService != null)
            {
                _inventoryService.ResizeInventory(_inventoryWidth, _inventoryHeight);
            }
        }
        
        protected override void RegisterEventHandlers()
        {
            Register(typeof(ItemEquippedEvent), OnItemEquipped);
            Register(typeof(ItemUnequippedEvent), OnItemUnequipped);
        }
        
        private void Start()
        {
            if (_character == null)
            {
                Debug.LogError("Character reference not injected into CharacterInventory!");
            }
        }
        
        /// <summary>
        /// 아이템 획득
        /// </summary>
        public bool AddItem(IInventoryItem item)
        {
            if (_inventoryService == null || item == null)
                return false;
                
            return _inventoryService.AddItem(item);
        }
        
        /// <summary>
        /// 아이템 장착
        /// </summary>
        public bool EquipItem(IEquipmentItem item)
        {
            if (item == null || _character == null)
                return false;
            
            // 캐릭터 장착 처리
            if (item.Equip(_character))
            {
                // 기존에 장착된 아이템이 있으면 해제
                int slotIndex = (int)item.SlotType;
                IEquipmentItem oldItem = _equippedItems[slotIndex];
                
                if (oldItem != null)
                {
                    UnequipItem(oldItem.SlotType);
                }
                
                // 새 아이템 참조 저장
                _equippedItems[slotIndex] = item;
                
                // 장착 이벤트 발생
                Dispatch(new ItemEquippedEvent(item, _character, item.SlotType));
                
                // 아이템 3D 모델 처리 (옵션)
                AttachItemModel(item);
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 아이템 장착 해제
        /// </summary>
        public bool UnequipItem(EquipmentSlotType slotType)
        {
            int slotIndex = (int)slotType;
            
            if (slotIndex < 0 || slotIndex >= _equippedItems.Length)
                return false;
                
            IEquipmentItem item = _equippedItems[slotIndex];
            
            if (item == null || _character == null)
                return false;
                
            // 캐릭터 장착 해제 처리
            if (item.Unequip(_character))
            {
                // 아이템을 인벤토리로 되돌림
                if (_inventoryService != null && !_inventoryService.AddItem(item))
                {
                    // 인벤토리가 가득 찬 경우 처리
                    Debug.LogWarning($"인벤토리 공간 부족: {item.DisplayName}을(를) 장착 해제할 수 없음");
                    return false;
                }
                
                // 장착 아이템 참조 제거
                _equippedItems[slotIndex] = null;
                
                // 장착 해제 이벤트 발생
                Dispatch(new ItemUnequippedEvent(item, _character, slotType));
                
                // 아이템 3D 모델 제거 (옵션)
                DetachItemModel(item);
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 장착된 아이템 가져오기
        /// </summary>
        public IEquipmentItem GetEquippedItem(EquipmentSlotType slotType)
        {
            int slotIndex = (int)slotType;
            
            if (slotIndex < 0 || slotIndex >= _equippedItems.Length)
                return null;
                
            return _equippedItems[slotIndex];
        }
        
        /// <summary>
        /// 아이템 3D 모델 장착
        /// </summary>
        private void AttachItemModel(IEquipmentItem item)
        {
            if (item == null || _equipmentRoot == null)
                return;
                
            // 실제 구현에서는 아이템 데이터에서 3D 모델 프리팹을 로드하고 장착 위치에 인스턴스 생성
            // 예시 코드:
            // GameObject modelPrefab = Resources.Load<GameObject>(item.ModelPath);
            // if (modelPrefab != null)
            // {
            //     Transform attachPoint = GetAttachPointForSlot(item.SlotType);
            //     if (attachPoint != null)
            //     {
            //         GameObject instance = Instantiate(modelPrefab, attachPoint);
            //         instance.name = $"Model_{item.DisplayName}";
            //     }
            // }
        }
        
        /// <summary>
        /// 아이템 3D 모델 제거
        /// </summary>
        private void DetachItemModel(IEquipmentItem item)
        {
            if (item == null || _equipmentRoot == null)
                return;
                
            // 실제 구현에서는 장착된 아이템 모델을 찾아서 제거
            // 예시 코드:
            // Transform attachPoint = GetAttachPointForSlot(item.SlotType);
            // if (attachPoint != null)
            // {
            //     Transform modelTransform = attachPoint.Find($"Model_{item.DisplayName}");
            //     if (modelTransform != null)
            //     {
            //         Destroy(modelTransform.gameObject);
            //     }
            // }
        }
        
        /// <summary>
        /// 슬롯 유형에 따른 아이템 부착 위치 가져오기 (실제 구현 필요)
        /// </summary>
        private Transform GetAttachPointForSlot(EquipmentSlotType slotType)
        {
            if (_equipmentRoot == null)
                return null;
                
            // 슬롯 타입에 따른 부착 위치 반환
            // 예시:
            switch (slotType)
            {
                case EquipmentSlotType.Head:
                    return _equipmentRoot.Find("Head");
                case EquipmentSlotType.Body:
                    return _equipmentRoot.Find("Body");
                case EquipmentSlotType.MainHand:
                    return _equipmentRoot.Find("RightHand");
                case EquipmentSlotType.OffHand:
                    return _equipmentRoot.Find("LeftHand");
                // 나머지 슬롯에 대한 처리
                default:
                    return null;
            }
        }
        
        #region 이벤트 핸들러
        private void OnItemEquipped(IEvent @event)
        {
            if (@event is ItemEquippedEvent equipEvent)
            {
                Debug.Log($"아이템 장착됨: {equipEvent.Item.DisplayName} - {equipEvent.SlotType}");
                
                // 캐릭터 스탯 업데이트 등 추가 작업 수행
            }
        }
        
        private void OnItemUnequipped(IEvent @event)
        {
            if (@event is ItemUnequippedEvent unequipEvent)
            {
                Debug.Log($"아이템 장착 해제됨: {unequipEvent.Item.DisplayName} - {unequipEvent.SlotType}");
                
                // 캐릭터 스탯 업데이트 등 추가 작업 수행
            }
        }
        #endregion
    }
}
