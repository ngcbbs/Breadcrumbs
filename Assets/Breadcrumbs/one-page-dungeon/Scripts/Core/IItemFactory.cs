using Breadcrumbs.CharacterSystem;

namespace Breadcrumbs.Core {
    /// <summary>
    /// 아이템 팩토리 인터페이스
    /// </summary>
    public interface IItemFactory<T> where T : EquipmentItem {
        T CreateItem(string itemId);
        T CreateRandomItem(int itemLevel, ItemRarity minRarity = ItemRarity.Common);
    }
}