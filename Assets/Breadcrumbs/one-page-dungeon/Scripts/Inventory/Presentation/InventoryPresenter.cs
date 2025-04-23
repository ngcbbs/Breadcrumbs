using System;
using Breadcrumbs.DependencyInjection;
using Breadcrumbs.EventSystem;
using Breadcrumbs.Inventory.Events;
using UnityEngine;

namespace Breadcrumbs.Inventory.Presentation
{
    /// <summary>
    /// 인벤토리 UI와 서비스를 연결하는 프리젠터 구현
    /// </summary>
    public class InventoryPresenter : EventBehaviour, IInventoryPresenter
    {
        [SerializeField] private Transform _inventoryUIRoot;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private GameObject _tooltipPrefab;
        [SerializeField] private GameObject _contextMenuPrefab;
        
        private GameObject _tooltipInstance;
        private GameObject _contextMenuInstance;
        private InventorySlotUI[,] _slotUIs;
        
        [Inject] private IInventoryService _inventoryService;
        
        private bool _isDragging;
        private int _dragStartX;
        private int _dragStartY;
        private GameObject _draggedItemVisual;

        private void Awake()
        {
            if (_inventoryUIRoot == null)
                _inventoryUIRoot = transform;
        }

        protected override void RegisterEventHandlers()
        {
            Register(typeof(ItemAddedEvent), OnItemAdded);
            Register(typeof(ItemRemovedEvent), OnItemRemoved);
            Register(typeof(ItemMovedEvent), OnItemMoved);
            Register(typeof(ItemUsedEvent), OnItemUsed);
            Register(typeof(InventoryFullEvent), OnInventoryFull);
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 인벤토리 UI 초기화
        /// </summary>
        public void Initialize()
        {
            if (_inventoryService == null)
            {
                Debug.LogError("InventoryService is not injected!");
                return;
            }
            
            // 기존 UI 슬롯 정리
            if (_slotContainer != null)
            {
                foreach (Transform child in _slotContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            int width = _inventoryService.Width;
            int height = _inventoryService.Height;
            
            // 슬롯 UI 배열 초기화
            _slotUIs = new InventorySlotUI[width, height];
            
            // UI 슬롯 생성
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    CreateSlotUI(x, y);
                }
            }
            
            RefreshUI();
        }

        /// <summary>
        /// 슬롯 UI 생성
        /// </summary>
        private void CreateSlotUI(int x, int y)
        {
            if (_slotPrefab == null || _slotContainer == null)
                return;
                
            GameObject slotGO = Instantiate(_slotPrefab, _slotContainer);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Initialize(x, y, this);
                _slotUIs[x, y] = slotUI;
            }
        }

        /// <summary>
        /// 인벤토리 UI 새로고침
        /// </summary>
        public void RefreshUI()
        {
            if (_inventoryService == null || _slotUIs == null)
                return;
                
            int width = _inventoryService.Width;
            int height = _inventoryService.Height;
            
            // 모든 슬롯 UI 업데이트
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    InventorySlot slot = _inventoryService.GetSlot(x, y);
                    if (slot != null && _slotUIs[x, y] != null)
                    {
                        _slotUIs[x, y].UpdateUI(slot);
                    }
                }
            }
        }

        /// <summary>
        /// 인벤토리 UI 표시
        /// </summary>
        public void Show()
        {
            if (_inventoryUIRoot != null)
            {
                _inventoryUIRoot.gameObject.SetActive(true);
                RefreshUI();
            }
        }

        /// <summary>
        /// 인벤토리 UI 숨기기
        /// </summary>
        public void Hide()
        {
            HideTooltip();
            HideContextMenu();
            
            if (_inventoryUIRoot != null)
            {
                _inventoryUIRoot.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 슬롯 클릭 처리
        /// </summary>
        public void OnSlotClicked(int x, int y)
        {
            InventorySlot slot = _inventoryService.GetSlot(x, y);
            if (slot == null)
                return;
                
            if (!slot.IsEmpty)
            {
                // 아이템 사용 (예: 장비나 소비 아이템)
                _inventoryService.UseItem(slot.RootX, slot.RootY);
            }
        }

        /// <summary>
        /// 슬롯 우클릭 처리
        /// </summary>
        public void OnSlotRightClicked(int x, int y)
        {
            InventorySlot slot = _inventoryService.GetSlot(x, y);
            if (slot == null || slot.IsEmpty)
                return;
                
            // 슬롯 위치에서 컨텍스트 메뉴 표시
            Vector2 screenPos = Input.mousePosition;
            ShowContextMenu(slot.Item, slot.RootX, slot.RootY, screenPos);
        }

        /// <summary>
        /// 드래그 시작 처리
        /// </summary>
        public void OnBeginDrag(int x, int y)
        {
            InventorySlot slot = _inventoryService.GetSlot(x, y);
            if (slot == null || slot.IsEmpty)
                return;
                
            _isDragging = true;
            _dragStartX = slot.RootX;
            _dragStartY = slot.RootY;
            
            // 드래그 중인 아이템 시각화
            CreateDraggedItemVisual(slot.Item);
        }

        /// <summary>
        /// 드래그 종료 처리
        /// </summary>
        public void OnEndDrag(int fromX, int fromY, int toX, int toY)
        {
            if (!_isDragging)
                return;
                
            _isDragging = false;
            DestroyDraggedItemVisual();
            
            // 동일한 위치이면 무시
            if (fromX == toX && fromY == toY)
                return;
                
            // 같은 아이템 타입이면 스택 합치기 시도
            InventorySlot fromSlot = _inventoryService.GetSlot(fromX, fromY);
            InventorySlot toSlot = _inventoryService.GetSlot(toX, toY);
            
            if (fromSlot == null)
                return;
                
            if (toSlot != null && !toSlot.IsEmpty)
            {
                // 스택 합치기 시도
                if (toSlot.Item.Id == fromSlot.Item.Id && toSlot.Item.IsStackable && fromSlot.Item.IsStackable)
                {
                    _inventoryService.MergeStacks(fromX, fromY, toX, toY);
                    return;
                }
            }
            
            // 일반 이동
            _inventoryService.MoveItem(fromX, fromY, toX, toY);
        }

        /// <summary>
        /// 드래그 중인 아이템 시각화 생성
        /// </summary>
        private void CreateDraggedItemVisual(IInventoryItem item)
        {
            // 드래그 중인 아이템 시각화 구현
            // (실제 구현은 프로젝트의 UI 시스템에 따라 달라질 수 있음)
        }

        /// <summary>
        /// 드래그 중인 아이템 시각화 제거
        /// </summary>
        private void DestroyDraggedItemVisual()
        {
            if (_draggedItemVisual != null)
            {
                Destroy(_draggedItemVisual);
                _draggedItemVisual = null;
            }
        }

        /// <summary>
        /// 툴팁 표시
        /// </summary>
        public void ShowTooltip(IInventoryItem item, Vector2 position)
        {
            if (item == null)
                return;
                
            HideTooltip();
            
            if (_tooltipPrefab != null)
            {
                _tooltipInstance = Instantiate(_tooltipPrefab, _inventoryUIRoot);
                
                // 툴팁 위치 설정
                RectTransform tooltipRect = _tooltipInstance.GetComponent<RectTransform>();
                if (tooltipRect != null)
                {
                    tooltipRect.position = position;
                }
                
                // 툴팁 내용 설정
                ItemTooltipUI tooltipUI = _tooltipInstance.GetComponent<ItemTooltipUI>();
                if (tooltipUI != null)
                {
                    tooltipUI.SetItem(item);
                }
            }
        }

        /// <summary>
        /// 툴팁 숨기기
        /// </summary>
        public void HideTooltip()
        {
            if (_tooltipInstance != null)
            {
                Destroy(_tooltipInstance);
                _tooltipInstance = null;
            }
        }

        /// <summary>
        /// 컨텍스트 메뉴 표시
        /// </summary>
        public void ShowContextMenu(IInventoryItem item, int x, int y, Vector2 position)
        {
            if (item == null)
                return;
                
            HideContextMenu();
            
            if (_contextMenuPrefab != null)
            {
                _contextMenuInstance = Instantiate(_contextMenuPrefab, _inventoryUIRoot);
                
                // 컨텍스트 메뉴 위치 설정
                RectTransform menuRect = _contextMenuInstance.GetComponent<RectTransform>();
                if (menuRect != null)
                {
                    menuRect.position = position;
                }
                
                // 컨텍스트 메뉴 내용 설정
                ItemContextMenuUI menuUI = _contextMenuInstance.GetComponent<ItemContextMenuUI>();
                if (menuUI != null)
                {
                    menuUI.Initialize(item, x, y, this);
                }
            }
        }

        /// <summary>
        /// 컨텍스트 메뉴 숨기기
        /// </summary>
        public void HideContextMenu()
        {
            if (_contextMenuInstance != null)
            {
                Destroy(_contextMenuInstance);
                _contextMenuInstance = null;
            }
        }

        #region 이벤트 핸들러
        private void OnItemAdded(IEvent @event)
        {
            if (@event is ItemAddedEvent addEvent)
            {
                RefreshUI();
                Debug.Log($"아이템 추가됨: {addEvent.Item.DisplayName} at ({addEvent.X}, {addEvent.Y})");
            }
        }

        private void OnItemRemoved(IEvent @event)
        {
            if (@event is ItemRemovedEvent removeEvent)
            {
                RefreshUI();
                Debug.Log($"아이템 제거됨: {removeEvent.Item.DisplayName} from ({removeEvent.X}, {removeEvent.Y})");
            }
        }

        private void OnItemMoved(IEvent @event)
        {
            if (@event is ItemMovedEvent moveEvent)
            {
                RefreshUI();
                Debug.Log($"아이템 이동됨: {moveEvent.Item.DisplayName} from ({moveEvent.OldX}, {moveEvent.OldY}) to ({moveEvent.NewX}, {moveEvent.NewY})");
            }
        }

        private void OnItemUsed(IEvent @event)
        {
            if (@event is ItemUsedEvent useEvent)
            {
                RefreshUI();
                
                string resultText = useEvent.Success ? "성공" : "실패";
                Debug.Log($"아이템 사용 {resultText}: {useEvent.Item.DisplayName}");
            }
        }

        private void OnInventoryFull(IEvent @event)
        {
            if (@event is InventoryFullEvent fullEvent)
            {
                Debug.LogWarning($"인벤토리 가득 참! 아이템을 추가할 수 없음: {fullEvent.RejectedItem.DisplayName}");
                // 인벤토리 가득 참 UI 표시
            }
        }
        #endregion
    }
}
