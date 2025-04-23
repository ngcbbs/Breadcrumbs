using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 인벤토리의 순수 데이터 컨테이너 클래스
    /// </summary>
    [Serializable]
    public class InventoryData
    {
        [SerializeField] private int _width = 10;
        [SerializeField] private int _height = 8;
        [SerializeField] private List<InventorySlot> _slots = new List<InventorySlot>();
        
        public int Width => _width;
        public int Height => _height;
        public IReadOnlyList<InventorySlot> Slots => _slots;

        public InventoryData(int width, int height)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            _slots.Clear();
            
            // 2D 그리드 구조로 슬롯 생성
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _slots.Add(new InventorySlot(x, y));
                }
            }
        }

        /// <summary>
        /// 특정 위치에 있는 슬롯 반환
        /// </summary>
        public InventorySlot GetSlot(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return null;

            int index = y * _width + x;
            return _slots[index];
        }

        /// <summary>
        /// 아이템을 놓을 수 있는지 확인
        /// </summary>
        public bool CanPlaceItem(IInventoryItem item, int x, int y)
        {
            if (item == null || x < 0 || y < 0)
                return false;

            // 아이템이 인벤토리 범위를 벗어나는지 확인
            if (x + item.Width > _width || y + item.Height > _height)
                return false;

            // 해당 위치에 다른 아이템이 이미 있는지 확인
            for (int itemY = 0; itemY < item.Height; itemY++)
            {
                for (int itemX = 0; itemX < item.Width; itemX++)
                {
                    InventorySlot slot = GetSlot(x + itemX, y + itemY);
                    if (slot == null || !slot.IsEmpty)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 아이템을 인벤토리에 배치
        /// </summary>
        public bool PlaceItem(IInventoryItem item, int x, int y)
        {
            if (!CanPlaceItem(item, x, y))
                return false;

            // 아이템 배치
            for (int itemY = 0; itemY < item.Height; itemY++)
            {
                for (int itemX = 0; itemX < item.Width; itemX++)
                {
                    InventorySlot slot = GetSlot(x + itemX, y + itemY);
                    slot.SetItem(item, x, y);
                }
            }

            return true;
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        public IInventoryItem RemoveItem(int x, int y)
        {
            // 해당 위치에 아이템이 있는지 확인
            InventorySlot slot = GetSlot(x, y);
            if (slot == null || slot.IsEmpty)
                return null;

            // 아이템의 참조 저장
            IInventoryItem item = slot.Item;
            if (item == null)
                return null;

            // 아이템이 차지하는 모든 슬롯 비우기
            int rootX = slot.RootX;
            int rootY = slot.RootY;

            for (int itemY = 0; itemY < item.Height; itemY++)
            {
                for (int itemX = 0; itemX < item.Width; itemX++)
                {
                    InventorySlot targetSlot = GetSlot(rootX + itemX, rootY + itemY);
                    if (targetSlot != null)
                    {
                        targetSlot.Clear();
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// 특정 조건을 만족하는 아이템 찾기
        /// </summary>
        public IInventoryItem FindItem(Predicate<IInventoryItem> predicate)
        {
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.IsRootSlot && predicate(slot.Item))
                {
                    return slot.Item;
                }
            }
            return null;
        }

        /// <summary>
        /// 특정 조건을 만족하는 모든 아이템 찾기
        /// </summary>
        public List<IInventoryItem> FindAllItems(Predicate<IInventoryItem> predicate)
        {
            List<IInventoryItem> result = new List<IInventoryItem>();
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.IsRootSlot && predicate(slot.Item))
                {
                    result.Add(slot.Item);
                }
            }
            return result;
        }

        /// <summary>
        /// 인벤토리의 빈 슬롯 위치 찾기
        /// </summary>
        public bool FindEmptySlot(IInventoryItem item, out int x, out int y)
        {
            x = -1;
            y = -1;

            if (item == null)
                return false;

            // 모든 가능한 위치를 검사
            for (int posY = 0; posY <= _height - item.Height; posY++)
            {
                for (int posX = 0; posX <= _width - item.Width; posX++)
                {
                    if (CanPlaceItem(item, posX, posY))
                    {
                        x = posX;
                        y = posY;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 모든 아이템 제거
        /// </summary>
        public void Clear()
        {
            foreach (var slot in _slots)
            {
                slot.Clear();
            }
        }
    }
}
