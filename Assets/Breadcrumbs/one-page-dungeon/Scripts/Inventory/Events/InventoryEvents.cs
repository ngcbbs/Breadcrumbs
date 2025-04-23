using Breadcrumbs.Character;
using Breadcrumbs.EventSystem;

namespace Breadcrumbs.Inventory.Events
{
    /// <summary>
    /// 아이템이 인벤토리에 추가될 때 발생하는 이벤트
    /// </summary>
    public class ItemAddedEvent : IEvent
    {
        public IInventoryItem Item { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public ItemAddedEvent(IInventoryItem item, int x, int y)
        {
            Item = item;
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// 아이템이 인벤토리에서 제거될 때 발생하는 이벤트
    /// </summary>
    public class ItemRemovedEvent : IEvent
    {
        public IInventoryItem Item { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public ItemRemovedEvent(IInventoryItem item, int x, int y)
        {
            Item = item;
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// 아이템이 인벤토리 내에서 이동될 때 발생하는 이벤트
    /// </summary>
    public class ItemMovedEvent : IEvent
    {
        public IInventoryItem Item { get; private set; }
        public int OldX { get; private set; }
        public int OldY { get; private set; }
        public int NewX { get; private set; }
        public int NewY { get; private set; }

        public ItemMovedEvent(IInventoryItem item, int oldX, int oldY, int newX, int newY)
        {
            Item = item;
            OldX = oldX;
            OldY = oldY;
            NewX = newX;
            NewY = newY;
        }
    }

    /// <summary>
    /// 아이템이 사용될 때 발생하는 이벤트
    /// </summary>
    public class ItemUsedEvent : IEvent
    {
        public IInventoryItem Item { get; private set; }
        public bool Success { get; private set; }

        public ItemUsedEvent(IInventoryItem item, bool success)
        {
            Item = item;
            Success = success;
        }
    }

    /// <summary>
    /// 장비 아이템이 장착될 때 발생하는 이벤트
    /// </summary>
    public class ItemEquippedEvent : IEvent
    {
        public IEquipmentItem Item { get; private set; }
        public ICharacter Character { get; private set; }
        public EquipmentSlotType SlotType { get; private set; }

        public ItemEquippedEvent(IEquipmentItem item, ICharacter character, EquipmentSlotType slotType)
        {
            Item = item;
            Character = character;
            SlotType = slotType;
        }
    }

    /// <summary>
    /// 장비 아이템이 해제될 때 발생하는 이벤트
    /// </summary>
    public class ItemUnequippedEvent : IEvent
    {
        public IEquipmentItem Item { get; private set; }
        public ICharacter Character { get; private set; }
        public EquipmentSlotType SlotType { get; private set; }

        public ItemUnequippedEvent(IEquipmentItem item, ICharacter character, EquipmentSlotType slotType)
        {
            Item = item;
            Character = character;
            SlotType = slotType;
        }
    }

    /// <summary>
    /// 인벤토리가 가득 찼을 때 발생하는 이벤트
    /// </summary>
    public class InventoryFullEvent : IEvent
    {
        public IInventoryItem RejectedItem { get; private set; }

        public InventoryFullEvent(IInventoryItem rejectedItem)
        {
            RejectedItem = rejectedItem;
        }
    }
}
