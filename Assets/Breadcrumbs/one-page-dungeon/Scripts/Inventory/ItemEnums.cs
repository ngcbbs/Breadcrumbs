namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 아이템 타입
    /// </summary>
    public enum ItemType
    {
        None,
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Material,
        Quest,
        Miscellaneous
    }

    /// <summary>
    /// 아이템 희귀도
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Unique
    }
    
    /// <summary>
    /// 장비 슬롯 타입
    /// </summary>
    public enum EquipmentSlotType
    {
        None,
        Head,
        Body,
        Legs,
        Feet,
        MainHand,
        OffHand,
        Ring,
        Necklace,
        Gloves,
        Cape
    }
}
