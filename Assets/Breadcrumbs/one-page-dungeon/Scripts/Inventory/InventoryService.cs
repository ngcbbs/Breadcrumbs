using System;
using System.Collections.Generic;
using System.Linq;
using Breadcrumbs.Inventory.Events;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 인벤토리 관리 서비스 구현
    /// 싱글톤 패턴을 사용하여 전역 접근 가능
    /// </summary>
    public class InventoryService : PersistentSingleton<InventoryService>, IInventoryService
    {
        [SerializeField] private InventoryData _inventoryData;
        
        public int Width => _inventoryData.Width;
        public int Height => _inventoryData.Height;
        public IReadOnlyList<InventorySlot> Slots => _inventoryData.Slots;
        
        // 루트 슬롯만 반환하는 프로퍼티
        public IEnumerable<IInventoryItem> Items => 
            _inventoryData.Slots
                .Where(slot => slot != null && !slot.IsEmpty && slot.IsRootSlot)
                .Select(slot => slot.Item)
                .Distinct();

        protected override void Awake()
        {
            base.Awake();
            
            // 인벤토리 데이터 초기화
            if (_inventoryData == null)
            {
                _inventoryData = new InventoryData(10, 8);
            }
        }

        /// <summary>
        /// 인벤토리의 크기 변경
        /// </summary>
        public void ResizeInventory(int width, int height)
        {
            // 인벤토리 크기 변경 전에 모든 아이템 제거
            Clear();
            
            // 새 인벤토리 데이터 생성
            _inventoryData = new InventoryData(width, height);
        }

        /// <summary>
        /// 특정 좌표의 슬롯 가져오기
        /// </summary>
        public InventorySlot GetSlot(int x, int y)
        {
            return _inventoryData.GetSlot(x, y);
        }

        /// <summary>
        /// 아이템을 지정한 위치에 추가
        /// </summary>
        public bool AddItem(IInventoryItem item, int x, int y)
        {
            if (item == null)
                return false;

            // 같은 아이템이 이미 있고 스택 가능한 경우 스택 합치기 시도
            if (item.IsStackable)
            {
                var existingItem = FindItem(i => 
                    i.Id == item.Id && 
                    i.IsStackable && 
                    i.StackCount < i.MaxStackCount);
                
                if (existingItem != null)
                {
                    int remainingCount = existingItem.AddToStack(item.StackCount);
                    if (remainingCount <= 0)
                    {
                        // 모든 수량이 합쳐짐
                        return true;
                    }
                    
                    // 남은 수량으로 아이템 업데이트
                    item.StackCount = remainingCount;
                }
            }

            // 해당 위치에 아이템 배치 시도
            if (_inventoryData.PlaceItem(item, x, y))
            {
                // todo: fixme event handler
                /*
                // 이벤트 발생
                EventHandler.Instance.Dispatch(new ItemAddedEvent(item, x, y));
                // */
                return true;
            }

            // todo: fixme event handler
            /*
            // 인벤토리 가득 참 이벤트 발생
            EventHandler.Instance.Dispatch(new InventoryFullEvent(item));
            // */
            return false;
        }

        /// <summary>
        /// 아이템 자동 위치에 추가
        /// </summary>
        public bool AddItem(IInventoryItem item)
        {
            if (item == null)
                return false;

            // 스택 가능한 아이템이면 기존 스택에 추가 시도
            if (item.IsStackable)
            {
                var existingItem = FindItem(i => 
                    i.Id == item.Id && 
                    i.IsStackable && 
                    i.StackCount < i.MaxStackCount);
                
                if (existingItem != null)
                {
                    // 해당 아이템의 루트 슬롯 찾기
                    InventorySlot rootSlot = Slots.FirstOrDefault(s => 
                        s.Item == existingItem && s.IsRootSlot);
                    
                    if (rootSlot != null)
                    {
                        int remainingCount = existingItem.AddToStack(item.StackCount);
                        if (remainingCount <= 0)
                        {
                            // todo: fixme event handler
                            /*
                            // 모든 수량이 합쳐짐
                            EventHandler.Instance.Dispatch(new ItemAddedEvent(existingItem, rootSlot.RootX, rootSlot.RootY));
                            // */
                            return true;
                        }
                        
                        // 남은 수량으로 아이템 업데이트
                        item.StackCount = remainingCount;
                    }
                }
            }

            // 빈 슬롯 찾기
            if (_inventoryData.FindEmptySlot(item, out int x, out int y))
            {
                return AddItem(item, x, y);
            }

            // todo: fixme event handler
            /*
            // 인벤토리 가득 참 이벤트 발생
            EventHandler.Instance.Dispatch(new InventoryFullEvent(item));
            // */
            return false;
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public IInventoryItem RemoveItem(int x, int y)
        {
            InventorySlot slot = GetSlot(x, y);
            if (slot == null || slot.IsEmpty)
                return null;

            // 아이템 제거
            IInventoryItem item = _inventoryData.RemoveItem(slot.RootX, slot.RootY);
            if (item != null)
            {
                // todo: fixme event handler
                /*
                // 이벤트 발생
                EventHandler.Instance.Dispatch(new ItemRemovedEvent(item, slot.RootX, slot.RootY));
                // */
            }
            
            return item;
        }

        /// <summary>
        /// 아이템 이동
        /// </summary>
        public bool MoveItem(int fromX, int fromY, int toX, int toY)
        {
            // 출발 슬롯 확인
            InventorySlot fromSlot = GetSlot(fromX, fromY);
            if (fromSlot == null || fromSlot.IsEmpty)
                return false;

            // 아이템과 루트 슬롯 위치 가져오기
            IInventoryItem item = fromSlot.Item;
            int rootX = fromSlot.RootX;
            int rootY = fromSlot.RootY;

            // 아이템 제거
            IInventoryItem removedItem = RemoveItem(rootX, rootY);
            if (removedItem == null)
                return false;

            // 새 위치에 아이템 배치 시도
            if (_inventoryData.PlaceItem(item, toX, toY))
            {
                // todo: fixme event handler
                /*
                // 이벤트 발생
                EventHandler.Instance.Dispatch(new ItemMovedEvent(item, rootX, rootY, toX, toY));
                // */
                return true;
            }
            else
            {
                // 아이템 배치 실패 시 원래 위치에 복원
                _inventoryData.PlaceItem(item, rootX, rootY);
                return false;
            }
        }

        /// <summary>
        /// 아이템 사용
        /// </summary>
        public bool UseItem(int x, int y)
        {
            InventorySlot slot = GetSlot(x, y);
            if (slot == null || slot.IsEmpty)
                return false;

            // 아이템 가져오기
            IInventoryItem item = slot.Item;
            if (item == null)
                return false;

            // 아이템 사용
            bool success = item.Use();

            // todo: fixme event handler
            /*
            // 이벤트 발생
            EventHandler.Instance.Dispatch(new ItemUsedEvent(item, success));
            // */

            // 소비 아이템이고 스택이 비었으면 제거
            if (success && item.IsStackable && item.StackCount <= 0)
            {
                RemoveItem(slot.RootX, slot.RootY);
            }

            return success;
        }

        /// <summary>
        /// 아이템 스택 합치기
        /// </summary>
        public bool MergeStacks(int fromX, int fromY, int toX, int toY)
        {
            // 출발 슬롯과 목적지 슬롯 확인
            InventorySlot fromSlot = GetSlot(fromX, fromY);
            InventorySlot toSlot = GetSlot(toX, toY);
            
            if (fromSlot == null || toSlot == null || 
                fromSlot.IsEmpty || toSlot.IsEmpty)
                return false;

            // 루트 슬롯 찾기
            InventorySlot fromRootSlot = GetSlot(fromSlot.RootX, fromSlot.RootY);
            InventorySlot toRootSlot = GetSlot(toSlot.RootX, toSlot.RootY);
            
            if (fromRootSlot == null || toRootSlot == null)
                return false;

            // 아이템 가져오기
            IInventoryItem fromItem = fromRootSlot.Item;
            IInventoryItem toItem = toRootSlot.Item;
            
            if (fromItem == null || toItem == null ||
                fromItem.Id != toItem.Id || !fromItem.IsStackable || !toItem.IsStackable)
                return false;

            // 스택 합치기
            int remainingCount = toItem.AddToStack(fromItem.StackCount);
            
            if (remainingCount <= 0)
            {
                // 모든 수량이 합쳐짐, 출발 아이템 제거
                RemoveItem(fromSlot.RootX, fromSlot.RootY);
                return true;
            }
            else
            {
                // 일부만 합쳐짐, 남은 수량으로 출발 아이템 업데이트
                fromItem.StackCount = remainingCount;
                return true;
            }
        }

        /// <summary>
        /// 아이템 스택 분할
        /// </summary>
        public bool SplitStack(int x, int y, int amount, out IInventoryItem splitItem)
        {
            splitItem = null;
            
            // 슬롯 확인
            InventorySlot slot = GetSlot(x, y);
            if (slot == null || slot.IsEmpty)
                return false;

            // 루트 슬롯 찾기
            InventorySlot rootSlot = GetSlot(slot.RootX, slot.RootY);
            if (rootSlot == null)
                return false;

            // 아이템 가져오기
            IInventoryItem item = rootSlot.Item;
            if (item == null || !item.IsStackable || item.StackCount <= amount)
                return false;

            // 같은 타입의 새 아이템 생성 (구체적인 구현은 아이템 팩토리에 따라 다름)
            // 여기서는 예시로 단순화
            // 실제 구현에서는 아이템 복제 로직 또는 팩토리를 사용해야 함
            // splitItem = ItemFactory.CreateItem(item.Id, amount);
            
            // 기존 아이템에서 수량 감소
            item.StackCount -= amount;
            
            // 분리된 아이템 추가
            if (splitItem != null)
            {
                return AddItem(splitItem);
            }
            
            return false;
        }

        /// <summary>
        /// 아이템 검색
        /// </summary>
        public IInventoryItem FindItem(Predicate<IInventoryItem> predicate)
        {
            return _inventoryData.FindItem(predicate);
        }

        /// <summary>
        /// 여러 아이템 검색
        /// </summary>
        public List<IInventoryItem> FindAllItems(Predicate<IInventoryItem> predicate)
        {
            return _inventoryData.FindAllItems(predicate);
        }

        /// <summary>
        /// 아이템을 위한 공간이 있는지 확인
        /// </summary>
        public bool HasSpaceForItem(IInventoryItem item)
        {
            if (item == null)
                return false;

            // 스택 가능한 아이템이면 기존 스택에 추가 가능한지 확인
            if (item.IsStackable)
            {
                var existingItem = FindItem(i => 
                    i.Id == item.Id && 
                    i.IsStackable && 
                    i.StackCount < i.MaxStackCount);
                
                if (existingItem != null)
                {
                    // 기존 스택에 추가 가능한 수량 계산
                    int availableSpace = existingItem.MaxStackCount - existingItem.StackCount;
                    if (availableSpace >= item.StackCount)
                    {
                        return true;
                    }
                }
            }

            // 빈 공간 찾기
            return _inventoryData.FindEmptySlot(item, out _, out _);
        }

        /// <summary>
        /// 빈 슬롯 찾기
        /// </summary>
        public bool FindEmptySlot(IInventoryItem item, out int x, out int y)
        {
            return _inventoryData.FindEmptySlot(item, out x, out y);
        }

        /// <summary>
        /// 인벤토리 초기화
        /// </summary>
        public void Clear()
        {
            // 모든 아이템 제거 전에 각 아이템에 대한 이벤트 발생
            foreach (var slot in _inventoryData.Slots)
            {
                if (!slot.IsEmpty && slot.IsRootSlot)
                {
                    // todo: fixme event handler
                    /*
                    EventHandler.Instance.Dispatch(new ItemRemovedEvent(slot.Item, slot.X, slot.Y));
                    // */
                }
            }
            
            _inventoryData.Clear();
        }
    }
}
