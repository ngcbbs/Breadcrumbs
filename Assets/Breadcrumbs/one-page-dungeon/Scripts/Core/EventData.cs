using Breadcrumbs.CharacterSystem;
using Breadcrumbs.ItemSystem;
using UnityEngine;

namespace Breadcrumbs.Core {
    public interface IEventData { }

    /// <summary>
    /// 아이템 획득 이벤트 데이터
    /// </summary>
    public class ItemPickupEventData : IEventData {
        public ItemData Item { get; private set; }
        public int Quantity { get; private set; }
        public PlayerCharacter Character { get; private set; }

        public ItemPickupEventData(ItemData item, int quantity,
            PlayerCharacter character) {
            Item = item;
            Quantity = quantity;
            Character = character;
        }
    }

    /// <summary>
    /// 버프 적용 이벤트 데이터
    /// </summary>
    public class BuffAppliedEventData : IEventData {
        public PlayerCharacter Target { get; private set; }
        public ActiveBuff Buff { get; private set; }

        public BuffAppliedEventData(PlayerCharacter target,
            ActiveBuff buff) {
            Target = target;
            Buff = buff;
        }
    }

    /// <summary>
    /// 버프 제거 이벤트 데이터
    /// </summary>
    public class BuffRemovedEventData : IEventData {
        public PlayerCharacter Target { get; private set; }
        public ActiveBuff Buff { get; private set; }

        public BuffRemovedEventData(PlayerCharacter target,
            ActiveBuff buff) {
            Target = target;
            Buff = buff;
        }
    }

    /// <summary>
    /// 레벨업 이벤트 데이터
    /// </summary>
    public class LevelUpEventData : IEventData {
        public PlayerCharacter Character { get; private set; }
        public int NewLevel { get; private set; }
        public int StatPointsGained { get; private set; }
        public int SkillPointsGained { get; private set; }

        public LevelUpEventData(PlayerCharacter character, int newLevel, int statPoints,
            int skillPoints) {
            Character = character;
            NewLevel = newLevel;
            StatPointsGained = statPoints;
            SkillPointsGained = skillPoints;
        }
    }

    /// <summary>
    /// 스킬 사용 이벤트 데이터
    /// </summary>
    public class SkillUsedEventData : IEventData {
        public PlayerCharacter Character { get; private set; }
        public SkillData Skill { get; private set; }
        public Transform Target { get; private set; }

        public SkillUsedEventData(PlayerCharacter character,
            SkillData skill, Transform target) {
            Character = character;
            Skill = skill;
            Target = target;
        }
    }

    /// <summary>
    /// 스킬 학습 이벤트 데이터
    /// </summary>
    public class SkillLearnedEventData : IEventData {
        public PlayerCharacter Character { get; private set; }
        public SkillData Skill { get; private set; }

        public SkillLearnedEventData(PlayerCharacter character, SkillData skill) {
            Character = character;
            Skill = skill;
        }
    }

    /// <summary>
    /// 스킬 레벨업 이벤트 데이터
    /// </summary>
    public class SkillLeveledUpEventData : IEventData {
        public PlayerCharacter Character { get; private set; }
        public SkillData Skill { get; private set; }
        public int NewLevel { get; private set; }

        public SkillLeveledUpEventData(PlayerCharacter character, SkillData skill, int newLevel) {
            Character = character;
            Skill = skill;
            NewLevel = newLevel;
        }
    }

    public class InventorySlotChangedEventData : IEventData {
        public int SlotIndex { get; private set; }
        public PlayerInventory Inventory { get; private set; }

        public InventorySlotChangedEventData(int slotIndex, PlayerInventory inventory) {
            SlotIndex = slotIndex;
            Inventory = inventory;
        }
    }

    public class EquipmentChangedEventData : IEventData {
        public EquipmentSlot Slot { get; private set; }
        public PlayerInventory Inventory { get; private set; }

        public EquipmentChangedEventData(EquipmentSlot slot, PlayerInventory inventory) {
            Slot = slot;
            Inventory = inventory;
        }
    }

    public class ItemUsedEventData : IEventData {
        public ItemData Item { get; private set; }
        public PlayerCharacter Character { get; private set; }

        public ItemUsedEventData(ItemData item, PlayerCharacter character) {
            Item = item;
            Character = character;
        }
    }

    public class ItemDropEventData : IEventData {
        public ItemData Item { get; private set; }
        public int Quantity { get; private set; }
        public Vector3 Position { get; private set; }
        public PlayerCharacter Character { get; private set; }

        public ItemDropEventData(ItemData item, int quantity, Vector3 position, PlayerCharacter character) {
            Item = item;
            Quantity = quantity;
            Position = position;
            Character = character;
        }
    }

    public class ItemTransferEventData : IEventData {
        public ItemData Item { get; private set; }
        public int Quantity { get; private set; }
        public IItemOwner Source { get; private set; }
        public IItemOwner Target { get; private set; }

        public ItemTransferEventData(ItemData item, int quantity, IItemOwner source, IItemOwner target) {
            Item = item;
            Quantity = quantity;
            Source = source;
            Target = target;
        }
    }

    public class GoldChangedEventData : IEventData {
        public int Amount { get; private set; }
        public int CurrentGold { get; private set; }
        public PlayerCharacter Character { get; private set; }

        public GoldChangedEventData(int amount, int currentGold, PlayerCharacter character) {
            Amount = amount;
            CurrentGold = currentGold;
            Character = character;
        }
    }
}