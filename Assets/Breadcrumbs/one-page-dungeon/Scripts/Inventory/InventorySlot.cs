using System;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 인벤토리의 개별 슬롯 데이터를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class InventorySlot
    {
        [SerializeField] private IInventoryItem _item;
        [SerializeField] private int _x;
        [SerializeField] private int _y;
        [SerializeField] private int _rootX;
        [SerializeField] private int _rootY;

        public IInventoryItem Item => _item;
        public int X => _x;
        public int Y => _y;
        public int RootX => _rootX;
        public int RootY => _rootY;
        public bool IsEmpty => _item == null;
        public bool IsRootSlot => _x == _rootX && _y == _rootY;

        public InventorySlot(int x, int y)
        {
            _x = x;
            _y = y;
            _rootX = -1;
            _rootY = -1;
        }

        /// <summary>
        /// 슬롯에 아이템 설정
        /// </summary>
        public void SetItem(IInventoryItem item, int rootX, int rootY)
        {
            _item = item;
            _rootX = rootX;
            _rootY = rootY;
        }

        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        public void Clear()
        {
            _item = null;
            _rootX = -1;
            _rootY = -1;
        }

        /// <summary>
        /// 아이템이 있고 루트 슬롯인지 확인
        /// </summary>
        public bool HasItemAsRoot()
        {
            return !IsEmpty && IsRootSlot;
        }
    }
}
