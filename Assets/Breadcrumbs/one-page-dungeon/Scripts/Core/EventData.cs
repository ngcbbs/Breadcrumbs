using System.Collections.Generic;
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

    // 퀘스트 아이템 요구 이벤트 데이터
    public class QuestItemRequiredEventData : IEventData {
        public string QuestId { get; private set; }     // 퀘스트 ID
        public string ItemId { get; private set; }      // 필요 아이템 ID
        public ItemData Item { get; private set; }      // 아이템 데이터
        public int RequiredAmount { get; private set; } // 필요 수량

        public QuestItemRequiredEventData(string questId, string itemId, ItemData item, int requiredAmount) {
            QuestId = questId;
            ItemId = itemId;
            Item = item;
            RequiredAmount = requiredAmount;
        }
    }

    // 퀘스트 아이템 전달 완료 이벤트 데이터
    public class QuestItemDeliveredEventData : IEventData {
        public string QuestId { get; private set; }         // 퀘스트 ID
        public string ItemId { get; private set; }          // 전달한 아이템 ID
        public int Amount { get; private set; }             // 전달한 수량
        public PlayerCharacter Player { get; private set; } // 플레이어

        public QuestItemDeliveredEventData(string questId, string itemId, int amount, PlayerCharacter player) {
            QuestId = questId;
            ItemId = itemId;
            Amount = amount;
            Player = player;
        }
    }

    // 상점 판매 완료 이벤트 데이터
    public class ShopSellCompletedEventData : IEventData {
        public ItemData Item { get; private set; }          // 판매한 아이템
        public int Quantity { get; private set; }           // 판매 수량
        public int TotalValue { get; private set; }         // 총 판매가
        public PlayerCharacter Seller { get; private set; } // 판매자
        public object Shop { get; private set; }            // 상점 참조

        public ShopSellCompletedEventData(ItemData item, int quantity, int totalValue, PlayerCharacter seller, object shop) {
            Item = item;
            Quantity = quantity;
            TotalValue = totalValue;
            Seller = seller;
            Shop = shop;
        }
    }
    
    // 상점 판매 이벤트 데이터
    public class ShopSellItemEventData : IEventData {
        public ItemData Item { get; private set; }          // 판매 아이템
        public int Quantity { get; private set; }           // 판매 수량
        public PlayerCharacter Seller { get; private set; } // 판매자
        public object Shop { get; private set; }            // 상점 참조

        public ShopSellItemEventData(ItemData item, int quantity, PlayerCharacter seller, object shop) {
            Item = item;
            Quantity = quantity;
            Seller = seller;
            Shop = shop;
        }
    }
    
    // 상점 구매 완료 이벤트 데이터
    public class ShopPurchaseCompletedEventData : IEventData {
        public ItemData Item { get; private set; }         // 구매한 아이템
        public int Quantity { get; private set; }          // 구매 수량
        public int TotalCost { get; private set; }         // 총 비용
        public PlayerCharacter Buyer { get; private set; } // 구매자
        public object Shop { get; private set; }           // 상점 참조

        public ShopPurchaseCompletedEventData(ItemData item, int quantity, int totalCost, PlayerCharacter buyer, object shop) {
            Item = item;
            Quantity = quantity;
            TotalCost = totalCost;
            Buyer = buyer;
            Shop = shop;
        }
    }
    
    // 스킬 아이템 강화 이벤트 데이터
    public class SkillEnhanceItemEventData  : IEventData{
        public EquipmentSlot TargetSlot { get; private set; } // 대상 장비 슬롯
        public string EnhancementType { get; private set; }   // 강화 유형 (공격력, 방어력 등)
        public float Duration { get; private set; }           // 강화 지속시간
        public PlayerCharacter Caster { get; private set; }   // 시전자

        public SkillEnhanceItemEventData(EquipmentSlot targetSlot, string enhancementType, float duration, PlayerCharacter caster) {
            TargetSlot = targetSlot;
            EnhancementType = enhancementType;
            Duration = duration;
            Caster = caster;
        }
    }
    
    // 아이템 강화 완료 이벤트 데이터
    public class ItemEnhancedEventData : IEventData{
        public ItemData Item { get; private set; }          // 강화된 아이템
        public string EnhancementType { get; private set; } // 강화 유형
        public float Duration { get; private set; }         // 지속시간
        public PlayerCharacter Player { get; private set; } // 플레이어

        public ItemEnhancedEventData(ItemData item, string enhancementType, float duration, PlayerCharacter player) {
            Item = item;
            EnhancementType = enhancementType;
            Duration = duration;
            Player = player;
        }
    }
    
    // 전리품 생성 이벤트 데이터
    public class CombatLootGeneratedEventData : IEventData {
        public int Gold { get; private set; }                 // 획득한 골드
        public List<LootItemData> Items { get; private set; } // 획득한 아이템 목록
        public Vector3 Position { get; private set; }         // 생성 위치

        public CombatLootGeneratedEventData(int gold, List<LootItemData> items, Vector3 position) {
            Gold = gold;
            Items = items;
            Position = position;
        }
        
        // 내부 클래스: 전리품 아이템 데이터
        public class LootItemData {
            public ItemData Item { get; private set; }
            public int Quantity { get; private set; }
            
            public LootItemData(ItemData item, int quantity) {
                Item = item;
                Quantity = quantity;
            }
        }
    }
    
    // 상점 구매 이벤트 데이터
    public class ShopPurchaseItemEventData : IEventData{
        public ItemData Item { get; private set; }         // 구매 아이템
        public int Quantity { get; private set; }          // 구매 수량
        public PlayerCharacter Buyer { get; private set; } // 구매자
        public object Shop { get; private set; }           // 상점 참조

        public ShopPurchaseItemEventData(ItemData item, int quantity, PlayerCharacter buyer, object shop) {
            Item = item;
            Quantity = quantity;
            Buyer = buyer;
            Shop = shop;
        }
    }
}