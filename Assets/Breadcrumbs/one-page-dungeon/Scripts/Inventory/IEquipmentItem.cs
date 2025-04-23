using Breadcrumbs.Character;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 장비 아이템에 대한 인터페이스
    /// </summary>
    public interface IEquipmentItem : IInventoryItem
    {
        /// <summary>
        /// 장비 슬롯 타입
        /// </summary>
        EquipmentSlotType SlotType { get; }
        
        /// <summary>
        /// 장비 레벨
        /// </summary>
        int Level { get; }
        
        /// <summary>
        /// 현재 내구도
        /// </summary>
        int Durability { get; set; }
        
        /// <summary>
        /// 최대 내구도
        /// </summary>
        int MaxDurability { get; }
        
        /// <summary>
        /// 스탯 수정자 배열
        /// </summary>
        StatModifier[] StatModifiers { get; }
        
        /// <summary>
        /// 파괴 여부 (내구도가 0 이하인 경우)
        /// </summary>
        bool IsBroken { get; }
        
        /// <summary>
        /// 장비 장착
        /// </summary>
        bool Equip(ICharacter character);
        
        /// <summary>
        /// 장비 해제
        /// </summary>
        bool Unequip(ICharacter character);
        
        /// <summary>
        /// 장비 수리
        /// </summary>
        bool Repair(int amount);
        
        /// <summary>
        /// 장비 사용으로 인한 내구도 감소
        /// </summary>
        bool ReduceDurability(int amount);
    }
}
