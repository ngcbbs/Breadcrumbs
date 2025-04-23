using System;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 기본 인벤토리 아이템 구현 클래스
    /// </summary>
    [Serializable]
    public abstract class InventoryItemBase : IInventoryItem
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _width = 1;
        [SerializeField] private int _height = 1;
        [SerializeField] private bool _isStackable;
        [SerializeField] private int _stackCount = 1;
        [SerializeField] private int _maxStackCount = 99;
        [SerializeField] private ItemType _itemType;
        [SerializeField] private ItemRarity _rarity;
        [SerializeField] private bool _canDrop = true;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public int Width => _width;
        public int Height => _height;
        public bool IsStackable => _isStackable;
        public int StackCount 
        { 
            get => _stackCount; 
            set => _stackCount = Mathf.Clamp(value, 0, MaxStackCount);
        }
        public int MaxStackCount => _maxStackCount;
        public ItemType ItemType => _itemType;
        public ItemRarity Rarity => _rarity;
        public bool CanDrop => _canDrop;

        protected InventoryItemBase(string id, string displayName, string description, Sprite icon, 
            ItemType itemType, ItemRarity rarity, int width = 1, int height = 1, bool isStackable = false, int maxStackCount = 1)
        {
            _id = id;
            _displayName = displayName;
            _description = description;
            _icon = icon;
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _isStackable = isStackable;
            _stackCount = 1;
            _maxStackCount = isStackable ? maxStackCount : 1;
            _itemType = itemType;
            _rarity = rarity;
        }

        /// <summary>
        /// 아이템 사용 메서드
        /// </summary>
        public virtual bool Use()
        {
            // 기본 구현은 아무것도 하지 않고 사용 성공을 반환합니다.
            // 상속 클래스에서 재정의하여 구체적인 기능 구현
            return true;
        }

        /// <summary>
        /// 스택에 아이템 추가
        /// </summary>
        /// <returns>최대 스택 초과로 남은 수량</returns>
        public int AddToStack(int count)
        {
            if (!IsStackable || count <= 0)
                return count;

            int newCount = StackCount + count;
            
            if (newCount <= MaxStackCount)
            {
                StackCount = newCount;
                return 0;
            }
            else
            {
                int overflow = newCount - MaxStackCount;
                StackCount = MaxStackCount;
                return overflow;
            }
        }

        /// <summary>
        /// 스택에서 아이템 제거
        /// </summary>
        /// <returns>스택이 비었으면 true 반환</returns>
        public bool RemoveFromStack(int count)
        {
            if (count <= 0)
                return false;

            StackCount -= count;
            return StackCount <= 0;
        }

        public override string ToString()
        {
            return IsStackable 
                ? $"{DisplayName} x{StackCount}" 
                : DisplayName;
        }
    }
}
