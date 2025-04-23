using System;
using Breadcrumbs.Character;
using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 장비 아이템 클래스
    /// </summary>
    [Serializable]
    public class EquipmentItem : InventoryItemBase, IEquipmentItem
    {
        [SerializeField] private EquipmentSlotType _slotType;
        [SerializeField] private int _level;
        [SerializeField] private int _durability;
        [SerializeField] private int _maxDurability;
        [SerializeField] private StatModifier[] _statModifiers;

        public EquipmentSlotType SlotType => _slotType;
        public int Level => _level;
        public int Durability 
        { 
            get => _durability; 
            set => _durability = Mathf.Clamp(value, 0, MaxDurability);
        }
        public int MaxDurability => _maxDurability;
        public StatModifier[] StatModifiers => _statModifiers;
        public bool IsBroken => Durability <= 0;

        public EquipmentItem(string id, string displayName, string description, Sprite icon, 
            ItemType itemType, ItemRarity rarity, EquipmentSlotType slotType, int level, 
            int durability, int maxDurability, StatModifier[] statModifiers = null, 
            int width = 1, int height = 1) 
            : base(id, displayName, description, icon, itemType, rarity, width, height, false, 1)
        {
            _slotType = slotType;
            _level = Mathf.Max(1, level);
            _maxDurability = Mathf.Max(1, maxDurability);
            _durability = Mathf.Clamp(durability, 0, _maxDurability);
            _statModifiers = statModifiers ?? new StatModifier[0];
        }

        /// <summary>
        /// 장비 장착 처리
        /// </summary>
        public virtual bool Equip(ICharacter character)
        {
            if (character == null || IsBroken)
                return false;

            // 상속 클래스에서 구체적인 장착 로직 구현
            return true;
        }

        /// <summary>
        /// 장비 해제 처리
        /// </summary>
        public virtual bool Unequip(ICharacter character)
        {
            if (character == null)
                return false;

            // 상속 클래스에서 구체적인 해제 로직 구현
            return true;
        }

        /// <summary>
        /// 장비 수리
        /// </summary>
        public virtual bool Repair(int amount)
        {
            if (amount <= 0)
                return false;

            if (Durability >= MaxDurability)
                return false;

            Durability += amount;
            return true;
        }

        /// <summary>
        /// 장비 사용 시 내구도 감소
        /// </summary>
        public virtual bool ReduceDurability(int amount)
        {
            if (amount <= 0)
                return false;

            Durability -= amount;
            return true;
        }
    }
}
