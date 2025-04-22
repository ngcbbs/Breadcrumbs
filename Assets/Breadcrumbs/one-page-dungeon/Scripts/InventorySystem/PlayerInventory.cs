using System;
using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.CharacterSystem {
    /// <summary>
    /// 플레이어 인벤토리 시스템 - 아이템 소지 및 관리
    /// </summary>
    public partial class PlayerInventory : MonoBehaviour, IItemOwner, IInventory, INetworkSyncableInventory {
        #region 필드 및 속성

        [Header("인벤토리 설정")]
        [SerializeField]
        private int inventorySize = 28; // 인벤토리 크기
        [SerializeField]
        private Transform dropPosition; // 아이템 드롭 위치

        [Header("추가 설정")]
        [SerializeField]
        private float maxWeight = 100f; // 최대 소지 무게
        [SerializeField]
        private int gold = 0; // 소지 골드

        // 인벤토리 슬롯 배열
        private InventorySlot[] inventorySlots;

        // 장비 슬롯 딕셔너리 (키: 장비 슬롯, 값: 장착된 아이템 데이터)
        private Dictionary<EquipmentSlot, InventorySlot> equipmentSlots = new Dictionary<EquipmentSlot, InventorySlot>();

        // 플레이어 캐릭터 참조
        private PlayerCharacter playerCharacter;

        // 속성 접근자
        public int InventorySize => inventorySlots.Length;
        public float MaxWeight => maxWeight;
        public int Gold => gold;

        #endregion

        #region Unity 라이프사이클 메서드

        private void Awake() {
            // 인벤토리 슬롯 초기화
            inventorySlots = new InventorySlot[inventorySize];
            for (int i = 0; i < inventorySize; i++) {
                inventorySlots[i] = new InventorySlot();
            }

            // 장비 슬롯 초기화
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot))) {
                equipmentSlots[slot] = new InventorySlot();
            }

            // 플레이어 캐릭터 참조 찾기
            playerCharacter = GetComponent<PlayerCharacter>();

            // 이벤트 구독 등록
            SubscribeToEvents();

            // 서비스 로케이터에 등록
            ServiceLocator.RegisterService<IInventory>(this);
        }

        private void OnDestroy() {
            // 서비스 로케이터에서 해제
            ServiceLocator.RegisterService<IInventory>(null);
            
            // 이벤트 구독 해제
            UnsubscribeFromEvents();
        }

        #endregion

        #region 이벤트 처리 메서드

        // 이벤트 구독
        private void SubscribeToEvents() {
            // 채팅또는 퀘스트 관련 아이템 이벤트 구독
            EventManager.Subscribe("Quest.ItemRequired", OnQuestItemRequired);
            
            // 스킬 사용으로 인한 아이템 강화 이벤트 구독
            EventManager.Subscribe("Skill.EnhanceItem", OnSkillEnhanceItem);
            
            // 전투 관련 전리품 이벤트 구독
            EventManager.Subscribe("Combat.LootGenerated", OnCombatLootGenerated);
            
            // 상점 거래 이벤트 구독
            EventManager.Subscribe("Shop.PurchaseItem", OnShopPurchaseItem);
            EventManager.Subscribe("Shop.SellItem", OnShopSellItem);
        }

        // 이벤트 구독 해제
        private void UnsubscribeFromEvents() {
            EventManager.Unsubscribe("Quest.ItemRequired", OnQuestItemRequired);
            EventManager.Unsubscribe("Skill.EnhanceItem", OnSkillEnhanceItem);
            EventManager.Unsubscribe("Combat.LootGenerated", OnCombatLootGenerated);
            EventManager.Unsubscribe("Shop.PurchaseItem", OnShopPurchaseItem);
            EventManager.Unsubscribe("Shop.SellItem", OnShopSellItem);
        }

        // 퀘스트 아이템 요구 이벤트 처리
        private void OnQuestItemRequired(object eventData) {
            if (eventData is QuestItemRequiredEventData data) {
                // 퀘스트에서 요구하는 아이템 확인
                // 1. 인벤토리에 있는지 확인
                bool hasItem = false;
                int itemCount = 0;
                
                // 인벤토리 전체 확인
                foreach (var slot in inventorySlots) {
                    if (!slot.IsEmpty() && slot.item.itemId == data.ItemId) {
                        hasItem = true;
                        itemCount += slot.quantity;
                    }
                }
                
                // 2. 있으면 퀘스트 완료 이벤트 발생
                if (hasItem && itemCount >= data.RequiredAmount) {
                    // 아이템 제거
                    RemoveItem(data.Item, data.RequiredAmount);
                    
                    // 퀘스트 완료 이벤트 발생
                    EventManager.Trigger("Quest.ItemDelivered", 
                        new QuestItemDeliveredEventData(data.QuestId, data.ItemId, data.RequiredAmount, playerCharacter));
                    
                    Debug.Log($"퀘스트 {data.QuestId}를 위한 아이템 {data.RequiredAmount}개 전달 완료");
                }
                else {
                    // 없으면 퀘스트 필요 아이템 알림
                    Debug.Log($"퀘스트 완료를 위해 {data.Item.itemName} {data.RequiredAmount}개가 필요합니다. 현재 소지 수량: {itemCount}");
                }
            }
        }
        
        // 스킬을 통한 아이템 강화 이벤트 처리
        private void OnSkillEnhanceItem(object eventData) {
            if (eventData is SkillEnhanceItemEventData data) {
                // 장착 아이템 확인 및 처리
                if (data.TargetSlot != EquipmentSlot.None) {
                    InventorySlot slot = equipmentSlots[data.TargetSlot];
                    if (slot.IsEmpty())
                        return;
                    // todo: fixme 아이템 타입 공용 처리가 필요함.
                    Debug.Log("<color=red>아이템 타입 공용 처리 필요함.</color>");
                    /*
                    if (slot.item is EquipmentItem equip) {
                        // 장비 아이템 강화 처리
                        // 실제 게임에서는 장비 아이템에 임시 강화 효과를 추가
                        
                        // 강화 이벤트 발생
                        EventManager.Trigger("Item.Enhanced", 
                            new ItemEnhancedEventData(equip, data.EnhancementType, data.Duration, playerCharacter));
                            
                        Debug.Log($"{data.TargetSlot} 슬롯의 {equip.itemName} 장비가 {data.Duration}초 동안 {data.EnhancementType} 방식으로 강화되었습니다.");
                    } else if (slot.item is ItemData itemData) {
                        // 일반 아이템 강화 처리
                        // 실제 게임에서는 일반 아이템에 임시 강화 효과를 추가

                    }
                    // */
                }
            }
        }
        
        // 전리품 생성 이벤트 처리
        private void OnCombatLootGenerated(object eventData) {
            if (eventData is CombatLootGeneratedEventData data) {
                // 전리품 자동 수집 처리
                bool inventoryFull = false;
                
                // 골드 획득
                if (data.Gold > 0) {
                    AddGold(data.Gold);
                }
                
                // 아이템 획득
                foreach (var lootItem in data.Items) {
                    // 공간 있는지 확인
                    if (CanOwnItem(lootItem.Item, lootItem.Quantity)) {
                        AddItem(lootItem.Item, lootItem.Quantity);
                    } else {
                        // 인벤토리가 가득 차면 필드에 드롭
                        inventoryFull = true;
                        EventManager.Trigger("Item.Drop", 
                            new ItemDropEventData(lootItem.Item, lootItem.Quantity, data.Position, playerCharacter));
                    }
                }
                
                if (inventoryFull) {
                    Debug.Log("인벤토리가 가득 차서 일부 아이템을 바닥에 떨어뜨렸습니다.");
                }
            }
        }
        
        // 상점 구매 이벤트 처리
        private void OnShopPurchaseItem(object eventData) {
            if (eventData is ShopPurchaseItemEventData data) {
                // 구매 로직 처리
                if (data.Buyer == playerCharacter) {
                    // 총 금액 계산
                    int totalCost = data.Item.buyPrice * data.Quantity;
                    
                    // 골드 있는지 확인
                    if (gold >= totalCost) {
                        // 공간 있는지 확인
                        if (CanOwnItem(data.Item, data.Quantity)) {
                            // 금액 차감 및 아이템 추가
                            SpendGold(totalCost);
                            AddItem(data.Item, data.Quantity);
                            
                            Debug.Log($"{data.Item.itemName} {data.Quantity}개를 {totalCost} 골드에 구매했습니다.");
                            
                            // 구매 완료 이벤트 발생
                            EventManager.Trigger("Shop.PurchaseCompleted", 
                                new ShopPurchaseCompletedEventData(data.Item, data.Quantity, totalCost, data.Buyer, data.Shop));
                        } else {
                            Debug.Log("인벤토리가 가득 차서 구매할 수 없습니다!");
                        }
                    } else {
                        Debug.Log($"골드가 부족합니다! 필요: {totalCost}, 보유: {gold}");
                    }
                }
            }
        }
        
        // 상점 판매 이벤트 처리
        private void OnShopSellItem(object eventData) {
            if (eventData is ShopSellItemEventData data) {
                // 판매 로직 처리
                if (data.Seller == playerCharacter) {
                    // 아이템 소지 여부 확인
                    bool hasItem = false;
                    int slotIndex = -1;
                    
                    // 인벤토리 검색
                    for (int i = 0; i < inventorySlots.Length; i++) {
                        if (!inventorySlots[i].IsEmpty() && 
                            inventorySlots[i].item == data.Item && 
                            inventorySlots[i].quantity >= data.Quantity) {
                            hasItem = true;
                            slotIndex = i;
                            break;
                        }
                    }
                    
                    if (hasItem) {
                        // 총 판매 금액 계산
                        int totalValue = data.Item.sellPrice * data.Quantity;
                        
                        // 아이템 제거 및 골드 획득
                        RemoveItemFromSlot(slotIndex, data.Quantity);
                        AddGold(totalValue);
                        
                        Debug.Log($"{data.Item.itemName} {data.Quantity}개를 {totalValue} 골드에 판매했습니다.");
                        
                        // 판매 완료 이벤트 발생
                        EventManager.Trigger("Shop.SellCompleted", 
                            new ShopSellCompletedEventData(data.Item, data.Quantity, totalValue, data.Seller, data.Shop));
                    } else {
                        Debug.Log("판매할 아이템이 충분하지 않습니다!");
                    }
                }
            }
        }
        
        #endregion

        #region 유틸리티 메서드

        // 인벤토리 슬롯 직접 액세스 (UI용)
        public InventorySlot GetInventorySlot(int index) {
            if (index < 0 || index >= inventorySlots.Length)
                return new InventorySlot();

            return inventorySlots[index];
        }

        // 장비 슬롯 직접 액세스 (UI용)
        public InventorySlot GetEquipmentSlot(EquipmentSlot slot) {
            if (equipmentSlots.TryGetValue(slot, out InventorySlot equipSlot))
                return equipSlot;

            return new InventorySlot();
        }

        // 골드 추가
        public void AddGold(int amount) {
            if (amount <= 0) return;

            gold += amount;

            // 이벤트 발생
            EventManager.Trigger("Gold.Changed", new GoldChangedEventData(amount, gold, playerCharacter));

            Debug.Log($"{amount} 골드를 획득했습니다. 현재 골드: {gold}");
        }

        // 골드 차감
        public bool SpendGold(int amount) {
            if (amount <= 0) return false;

            if (gold >= amount) {
                gold -= amount;

                // 이벤트 발생
                EventManager.Trigger("Gold.Changed", new GoldChangedEventData(-amount, gold, playerCharacter));

                Debug.Log($"{amount} 골드를 사용했습니다. 남은 골드: {gold}");
                return true;
            } else {
                Debug.Log($"골드가 부족합니다. 필요: {amount}, 현재: {gold}");
                return false;
            }
        }

        // 전체 무게 계산
        public float GetTotalWeight() {
            float totalWeight = 0f;

            // 인벤토리 아이템 무게 계산
            foreach (var slot in inventorySlots) {
                if (!slot.IsEmpty()) {
                    // 아이템 데이터에 무게 정보 추가 필요
                    // totalWeight += slot.item.weight * slot.quantity;
                    totalWeight += 0.5f * slot.quantity; // 임시 값
                }
            }

            // 장비 아이템 무게 계산
            foreach (var pair in equipmentSlots) {
                if (!pair.Value.IsEmpty()) {
                    // 아이템 데이터에 무게 정보 추가 필요
                    // totalWeight += pair.Value.item.weight;
                    totalWeight += 1.0f; // 임시 값
                }
            }

            return totalWeight;
        }

        // 사용된 슬롯 수 계산
        public int GetUsedSlotCount() {
            int count = 0;

            foreach (var slot in inventorySlots) {
                if (!slot.IsEmpty()) {
                    count++;
                }
            }

            return count;
        }

        #endregion
    }

    #if false
    #region 이벤트 데이터 클래스

    // 골드 변경 이벤트 데이터
    public class GoldChangedEventData {
        public int Amount { get; private set; }      // 변화량 (양수: 획득, 음수: 사용)
        public int CurrentGold { get; private set; } // 현재 총 골드량
        public PlayerCharacter Player { get; private set; } // 플레이어 참조

        public GoldChangedEventData(int amount, int currentGold, PlayerCharacter player) {
            Amount = amount;
            CurrentGold = currentGold;
            Player = player;
        }
    }

    // 인벤토리 슬롯 변경 이벤트 데이터
    public class InventorySlotChangedEventData {
        public int SlotIndex { get; private set; } // 변경된 슬롯 인덱스
        public IInventory Inventory { get; private set; } // 인벤토리 참조

        public InventorySlotChangedEventData(int slotIndex, IInventory inventory) {
            SlotIndex = slotIndex;
            Inventory = inventory;
        }
    }

    // 장비 변경 이벤트 데이터
    public class EquipmentChangedEventData {
        public EquipmentSlot Slot { get; private set; } // 변경된 장비 슬롯
        public IInventory Inventory { get; private set; } // 인벤토리 참조

        public EquipmentChangedEventData(EquipmentSlot slot, IInventory inventory) {
            Slot = slot;
            Inventory = inventory;
        }
    }

    // 아이템 획득 이벤트 데이터
    public class ItemPickupEventData {
        public ItemData Item { get; private set; }  // 획득한 아이템
        public int Quantity { get; private set; }  // 획득 수량
        public PlayerCharacter Player { get; private set; } // 플레이어 참조

        public ItemPickupEventData(ItemData item, int quantity, PlayerCharacter player) {
            Item = item;
            Quantity = quantity;
            Player = player;
        }
    }

    // 아이템 사용 이벤트 데이터
    public class ItemUsedEventData {
        public ItemData Item { get; private set; }  // 사용한 아이템
        public PlayerCharacter Player { get; private set; } // 플레이어 참조

        public ItemUsedEventData(ItemData item, PlayerCharacter player) {
            Item = item;
            Player = player;
        }
    }

    // 아이템 드롭 이벤트 데이터
    public class ItemDropEventData {
        public ItemData Item { get; private set; }  // 드롭한 아이템
        public int Quantity { get; private set; }  // 드롭 수량
        public Vector3 Position { get; private set; } // 드롭 위치
        public PlayerCharacter Player { get; private set; } // 플레이어 참조

        public ItemDropEventData(ItemData item, int quantity, Vector3 position, PlayerCharacter player) {
            Item = item;
            Quantity = quantity;
            Position = position;
            Player = player;
        }
    }

    // 아이템 소유권 이전 이벤트 데이터
    public class ItemTransferEventData {
        public ItemData Item { get; private set; }  // 이전된 아이템
        public int Quantity { get; private set; }  // 이전 수량
        public IItemOwner FromOwner { get; private set; } // 기존 소유자
        public IItemOwner ToOwner { get; private set; }   // 새 소유자

        public ItemTransferEventData(ItemData item, int quantity, IItemOwner fromOwner, IItemOwner toOwner) {
            Item = item;
            Quantity = quantity;
            FromOwner = fromOwner;
            ToOwner = toOwner;
        }
    }
    
    // 퀘스트 아이템 요구 이벤트 데이터
    public class QuestItemRequiredEventData {
        public string QuestId { get; private set; }  // 퀘스트 ID
        public string ItemId { get; private set; }   // 필요 아이템 ID
        public ItemData Item { get; private set; }   // 아이템 데이터
        public int RequiredAmount { get; private set; }  // 필요 수량

        public QuestItemRequiredEventData(string questId, string itemId, ItemData item, int requiredAmount) {
            QuestId = questId;
            ItemId = itemId;
            Item = item;
            RequiredAmount = requiredAmount;
        }
    }
    
    // 퀘스트 아이템 전달 완료 이벤트 데이터
    public class QuestItemDeliveredEventData {
        public string QuestId { get; private set; }  // 퀘스트 ID
        public string ItemId { get; private set; }   // 전달한 아이템 ID
        public int Amount { get; private set; }      // 전달한 수량
        public PlayerCharacter Player { get; private set; } // 플레이어

        public QuestItemDeliveredEventData(string questId, string itemId, int amount, PlayerCharacter player) {
            QuestId = questId;
            ItemId = itemId;
            Amount = amount;
            Player = player;
        }
    }
    
    // 스킬 아이템 강화 이벤트 데이터
    public class SkillEnhanceItemEventData {
        public EquipmentSlot TargetSlot { get; private set; }  // 대상 장비 슬롯
        public string EnhancementType { get; private set; }    // 강화 유형 (공격력, 방어력 등)
        public float Duration { get; private set; }            // 강화 지속시간
        public PlayerCharacter Caster { get; private set; }    // 시전자

        public SkillEnhanceItemEventData(EquipmentSlot targetSlot, string enhancementType, float duration, PlayerCharacter caster) {
            TargetSlot = targetSlot;
            EnhancementType = enhancementType;
            Duration = duration;
            Caster = caster;
        }
    }
    
    // 아이템 강화 완료 이벤트 데이터
    public class ItemEnhancedEventData {
        public ItemData Item { get; private set; }  // 강화된 아이템
        public string EnhancementType { get; private set; }  // 강화 유형
        public float Duration { get; private set; }  // 지속시간
        public PlayerCharacter Player { get; private set; }  // 플레이어

        public ItemEnhancedEventData(ItemData item, string enhancementType, float duration, PlayerCharacter player) {
            Item = item;
            EnhancementType = enhancementType;
            Duration = duration;
            Player = player;
        }
    }
    
    // 전리품 생성 이벤트 데이터
    public class CombatLootGeneratedEventData {
        public int Gold { get; private set; }  // 획득한 골드
        public List<LootItemData> Items { get; private set; }  // 획득한 아이템 목록
        public Vector3 Position { get; private set; }  // 생성 위치

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
    public class ShopPurchaseItemEventData {
        public ItemData Item { get; private set; }  // 구매 아이템
        public int Quantity { get; private set; }   // 구매 수량
        public PlayerCharacter Buyer { get; private set; }  // 구매자
        public object Shop { get; private set; }  // 상점 참조

        public ShopPurchaseItemEventData(ItemData item, int quantity, PlayerCharacter buyer, object shop) {
            Item = item;
            Quantity = quantity;
            Buyer = buyer;
            Shop = shop;
        }
    }
    
    // 상점 구매 완료 이벤트 데이터
    public class ShopPurchaseCompletedEventData {
        public ItemData Item { get; private set; }  // 구매한 아이템
        public int Quantity { get; private set; }   // 구매 수량
        public int TotalCost { get; private set; }  // 총 비용
        public PlayerCharacter Buyer { get; private set; }  // 구매자
        public object Shop { get; private set; }  // 상점 참조

        public ShopPurchaseCompletedEventData(ItemData item, int quantity, int totalCost, PlayerCharacter buyer, object shop) {
            Item = item;
            Quantity = quantity;
            TotalCost = totalCost;
            Buyer = buyer;
            Shop = shop;
        }
    }
    
    // 상점 판매 이벤트 데이터
    public class ShopSellItemEventData {
        public ItemData Item { get; private set; }  // 판매 아이템
        public int Quantity { get; private set; }   // 판매 수량
        public PlayerCharacter Seller { get; private set; }  // 판매자
        public object Shop { get; private set; }  // 상점 참조

        public ShopSellItemEventData(ItemData item, int quantity, PlayerCharacter seller, object shop) {
            Item = item;
            Quantity = quantity;
            Seller = seller;
            Shop = shop;
        }
    }
    
    // 상점 판매 완료 이벤트 데이터
    public class ShopSellCompletedEventData {
        public ItemData Item { get; private set; }  // 판매한 아이템
        public int Quantity { get; private set; }   // 판매 수량
        public int TotalValue { get; private set; }  // 총 판매가
        public PlayerCharacter Seller { get; private set; }  // 판매자
        public object Shop { get; private set; }  // 상점 참조

        public ShopSellCompletedEventData(ItemData item, int quantity, int totalValue, PlayerCharacter seller, object shop) {
            Item = item;
            Quantity = quantity;
            TotalValue = totalValue;
            Seller = seller;
            Shop = shop;
        }
    }

    #endregion
    #endif
}