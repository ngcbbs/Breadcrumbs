using System.Collections.Generic;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;

namespace Breadcrumbs.Core {
    /// <summary>
    /// 모든 장비 아이템이 공통적으로 구현해야 하는 인터페이스
    /// </summary>
    public interface IEquippable
    {
        string ItemId { get; }
        string ItemName { get; }
        EquipmentSlot EquipSlot { get; }
        ItemRarity Rarity { get; }
        int RequiredLevel { get; }
        
        // 장비 점수 계산
        int CalculateItemScore();
        
        // 캐릭터에게 적용할 스탯 수정자 목록
        List<StatModifier> GetStatModifiers();
        
        // 해당 캐릭터가 아이템 착용 가능한지 확인
        bool CanEquip(PlayerCharacter character);
        
        // 아이템 장착/해제 이벤트
        void OnEquipped(PlayerCharacter character);
        void OnUnequipped(PlayerCharacter character);
    }
}