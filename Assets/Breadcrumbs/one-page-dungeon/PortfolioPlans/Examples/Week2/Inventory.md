# 인벤토리 시스템 예제 코드

이 문서는 게임의 인벤토리 시스템의 구현을 보여줍니다. 아이템 관리, 스택 처리, 사용 및 장착 기능을 포함합니다.

## Inventory.cs

```csharp
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int inventorySize = 30;
    
    private InventorySlot[] slots;
    
    // 이벤트 정의
    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action<Item, int> OnItemUsed;
    public event Action OnInventoryChanged;
    
    // 캐릭터 참조
    [SerializeField] private Character character;
    
    private void Awake()
    {
        // 인벤토리 슬롯 초기화
        slots = new InventorySlot[inventorySize];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new InventorySlot();
        }
    }
    
    // 아이템 추가
    public bool AddItem(Item item, int count = 1)
    {
        if (item == null) return false;
        
        // 스택 가능한 아이템이라면 기존 스택에 추가 시도
        if (item.isStackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item != null && slots[i].item.IsEqual(item) && slots[i].count < slots[i].item.maxStackSize)
                {
                    // 동일 아이템이 이미 있고 스택이 가득 차지 않은 경우
                    int spaceInStack = slots[i].item.maxStackSize - slots[i].count;
                    int amountToAdd = Mathf.Min(count, spaceInStack);
                    
                    slots[i].count += amountToAdd;
                    count -= amountToAdd;
                    
                    // 모두 추가되었다면 성공 반환
                    if (count <= 0)
                    {
                        OnItemAdded?.Invoke(item);
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }
        
        // 새 슬롯에 추가 (남은 아이템이 있거나 스택 불가능한 경우)
        while (count > 0)
        {
            // 빈 슬롯 찾기
            int emptySlot = -1;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].isEmpty)
                {
                    emptySlot = i;
                    break;
                }
            }
            
            // 빈 슬롯이 없다면 인벤토리가 가득 찬 상태
            if (emptySlot == -1)
            {
                Debug.Log("Inventory is full");
                OnInventoryChanged?.Invoke();
                return count == 0; // 일부라도 추가되었다면 부분 성공
            }
            
            // 아이템 추가
            int amountToAdd = Mathf.Min(count, item.maxStackSize);
            slots[emptySlot].item = item;
            slots[emptySlot].count = amountToAdd;
            count -= amountToAdd;
        }
        
        OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    // 아이템 제거
    public bool RemoveItem(Item item, int count = 1)
    {
        if (item == null) return false;
        
        int remainingToRemove = count;
        
        // 제거할 아이템 찾기
        for (int i = 0; i < slots.Length && remainingToRemove > 0; i++)
        {
            if (slots[i].item != null && slots[i].item.IsEqual(item))
            {
                int amountToRemove = Mathf.Min(remainingToRemove, slots[i].count);
                slots[i].count -= amountToRemove;
                remainingToRemove -= amountToRemove;
                
                // 슬롯이 비었다면 아이템 참조도 제거
                if (slots[i].count <= 0)
                {
                    slots[i].item = null;
                    slots[i].count = 0;
                }
            }
        }
        
        // 모든 요청된 수량을 제거했는지 확인
        bool success = remainingToRemove <= 0;
        
        if (success)
        {
            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
        }
        
        return success;
    }
    
    // 아이템 사용
    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return false;
            
        InventorySlot slot = slots[slotIndex];
        
        if (slot.isEmpty)
            return false;
            
        // 아이템 사용 시도
        bool wasUsed = slot.item.Use(character);
        
        if (wasUsed)
        {
            // 소모성 아이템이면 수량 감소
            if (slot.item.itemType == ItemType.Consumable)
            {
                slot.count--;
                
                // 수량이 0이 되면 슬롯에서 제거
                if (slot.count <= 0)
                {
                    Item usedItem = slot.item;
                    slot.item = null;
                    slot.count = 0;
                    OnItemRemoved?.Invoke(usedItem);
                }
            }
            
            OnItemUsed?.Invoke(slot.item, slotIndex);
            OnInventoryChanged?.Invoke();
        }
        
        return wasUsed;
    }
    
    // 인벤토리 슬롯 배열 반환
    public InventorySlot[] GetSlots()
    {
        return slots;
    }
    
    // 슬롯 가져오기
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;
            
        return slots[index];
    }
    
    // 인벤토리에 특정 아이템이 있는지 확인
    public bool HasItem(Item item, int count = 1)
    {
        if (item == null) return false;
        
        int totalCount = 0;
        
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.item.IsEqual(item))
            {
                totalCount += slot.count;
                if (totalCount >= count)
                    return true;
            }
        }
        
        return false;
    }
    
    // 인벤토리 저장 (간단 구현)
    public InventorySaveData GetSaveData()
    {
        List<InventorySlotSaveData> slotData = new List<InventorySlotSaveData>();
        
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isEmpty)
            {
                slotData.Add(new InventorySlotSaveData
                {
                    slotIndex = i,
                    itemId = slots[i].item.name, // 아이템 식별자로 사용
                    count = slots[i].count
                });
            }
        }
        
        return new InventorySaveData
        {
            slots = slotData.ToArray()
        };
    }
    
    // 인벤토리 로드 (간단 구현)
    public void LoadFromSaveData(InventorySaveData saveData, ItemDatabase itemDatabase)
    {
        // 인벤토리 초기화
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].item = null;
            slots[i].count = 0;
        }
        
        // 저장된 데이터로 슬롯 채우기
        foreach (var slotData in saveData.slots)
        {
            Item item = itemDatabase.GetItemById(slotData.itemId);
            if (item != null && slotData.slotIndex >= 0 && slotData.slotIndex < slots.Length)
            {
                slots[slotData.slotIndex].item = item;
                slots[slotData.slotIndex].count = slotData.count;
            }
        }
        
        OnInventoryChanged?.Invoke();
    }
}

// 인벤토리 슬롯 클래스
[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int count;
    
    public bool isEmpty => item == null || count <= 0;
}

// 저장 데이터 구조체
[System.Serializable]
public struct InventorySaveData
{
    public InventorySlotSaveData[] slots;
}

[System.Serializable]
public struct InventorySlotSaveData
{
    public int slotIndex;
    public string itemId;
    public int count;
}
```

## EquipmentSystem.cs

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    [SerializeField] private Character character;
    
    // 장착 슬롯 배열
    private Item[] equippedItems;
    private GameObject[] equipmentInstances;
    
    // 장비 변경 이벤트
    public event Action<EquipmentSlot, Item> OnEquipmentChanged;
    
    private void Awake()
    {
        // 장비 슬롯 초기화
        int slotCount = Enum.GetValues(typeof(EquipmentSlot)).Length;
        equippedItems = new Item[slotCount];
        equipmentInstances = new GameObject[slotCount];
    }
    
    // 아이템 장착
    public bool EquipItem(Item item, EquipmentSlot slot)
    {
        if (item == null)
            return false;
            
        // 이미 장착된 아이템이 있으면 해제
        if (equippedItems[(int)slot] != null)
        {
            UnequipItem(slot);
        }
        
        // 새 아이템 장착
        equippedItems[(int)slot] = item;
        
        // 이벤트 발생
        OnEquipmentChanged?.Invoke(slot, item);
        
        return true;
    }
    
    // 아이템 해제
    public bool UnequipItem(EquipmentSlot slot)
    {
        Item oldItem = equippedItems[(int)slot];
        
        if (oldItem == null)
            return false;
            
        // 인벤토리에 추가 (실패 시 해제 취소)
        Inventory inventory = character.GetComponent<Inventory>();
        if (inventory != null)
        {
            bool addedToInventory = inventory.AddItem(oldItem);
            if (!addedToInventory)
            {
                Debug.Log("장비 해제 실패: 인벤토리 공간 부족");
                return false;
            }
        }
        
        // 장비 해제
        equippedItems[(int)slot] = null;
        
        // 이벤트 발생
        OnEquipmentChanged?.Invoke(slot, null);
        
        return true;
    }
    
    // 특정 슬롯의 장비 가져오기
    public Item GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems[(int)slot];
    }
    
    // 모든 장착 아이템 가져오기
    public Item[] GetAllEquippedItems()
    {
        return equippedItems;
    }
    
    // 장비 모델 인스턴스 설정
    public void SetEquipmentInstance(EquipmentSlot slot, GameObject instance)
    {
        equipmentInstances[(int)slot] = instance;
    }
    
    // 장비 모델 인스턴스 제거
    public void DestroyEquipmentInstance(EquipmentSlot slot)
    {
        if (equipmentInstances[(int)slot] != null)
        {
            Destroy(equipmentInstances[(int)slot]);
            equipmentInstances[(int)slot] = null;
        }
    }
}
```
