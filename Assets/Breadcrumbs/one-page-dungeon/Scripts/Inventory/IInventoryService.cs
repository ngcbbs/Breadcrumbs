using System;
using System.Collections.Generic;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 인벤토리 관리 서비스를 정의하는 인터페이스
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// 인벤토리의 너비
        /// </summary>
        int Width { get; }
        
        /// <summary>
        /// 인벤토리의 높이
        /// </summary>
        int Height { get; }
        
        /// <summary>
        /// 인벤토리의 모든 슬롯 목록
        /// </summary>
        IReadOnlyList<InventorySlot> Slots { get; }
        
        /// <summary>
        /// 현재 보유 중인 모든 아이템 목록 (루트 슬롯만)
        /// </summary>
        IEnumerable<IInventoryItem> Items { get; }
        
        /// <summary>
        /// 특정 위치의 슬롯 가져오기
        /// </summary>
        InventorySlot GetSlot(int x, int y);
        
        /// <summary>
        /// 아이템 추가하기
        /// </summary>
        bool AddItem(IInventoryItem item, int x, int y);
        
        /// <summary>
        /// 아이템 추가하기 (자동 위치 결정)
        /// </summary>
        bool AddItem(IInventoryItem item);
        
        /// <summary>
        /// 아이템 제거하기
        /// </summary>
        IInventoryItem RemoveItem(int x, int y);
        
        /// <summary>
        /// 아이템 이동하기
        /// </summary>
        bool MoveItem(int fromX, int fromY, int toX, int toY);
        
        /// <summary>
        /// 아이템 사용하기
        /// </summary>
        bool UseItem(int x, int y);
        
        /// <summary>
        /// 아이템 스택 합치기
        /// </summary>
        bool MergeStacks(int fromX, int fromY, int toX, int toY);
        
        /// <summary>
        /// 아이템 스택 분할하기
        /// </summary>
        bool SplitStack(int x, int y, int amount, out IInventoryItem splitItem);
        
        /// <summary>
        /// 특정 조건을 만족하는 아이템 찾기
        /// </summary>
        IInventoryItem FindItem(Predicate<IInventoryItem> predicate);
        
        /// <summary>
        /// 특정 조건을 만족하는 모든 아이템 찾기
        /// </summary>
        List<IInventoryItem> FindAllItems(Predicate<IInventoryItem> predicate);
        
        /// <summary>
        /// 인벤토리에 아이템을 위한 공간이 있는지 확인
        /// </summary>
        bool HasSpaceForItem(IInventoryItem item);
        
        /// <summary>
        /// 빈 공간 찾기
        /// </summary>
        bool FindEmptySlot(IInventoryItem item, out int x, out int y);
        
        /// <summary>
        /// 인벤토리 초기화
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 인벤토리 사이즈 변경
        /// </summary>
        void ResizeInventory(int width, int height);
    }
}
