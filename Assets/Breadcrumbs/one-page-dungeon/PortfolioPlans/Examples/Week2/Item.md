# 아이템 시스템 예제 코드 (Part 1)

이 문서는 게임의 아이템 시스템 구현을 보여줍니다. 다양한 아이템 타입과 효과를 구현하고, 장비 및 인벤토리 시스템과 연결됩니다.

## Item.cs

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public ItemRarity rarity = ItemRarity.Common;
    public ItemType itemType = ItemType.Miscellaneous;
    
    [Header("스택 설정")]
    public bool isStackable = false;
    public int maxStackSize = 1;
    
    [Header("판매 정보")]
    public int buyPrice;
    public int sellPrice;
    
    // 아이템 사용 메서드
    public virtual bool Use(Character character)
    {
        // 기본 구현은 아무것도 하지 않음
        Debug.Log($"{itemName} 아이템 사용됨");
        return true; // 사용 성공
    }
    
    // 아이템 장착 메서드 (장비 아이템 전용)
    public virtual bool Equip(Character character)
    {
        // 기본 구현은 실패 반환
        Debug.Log($"{itemName}은(는) 장착할 수 없는 아이템입니다.");
        return false;
    }
    
    // 아이템 장착 해제 메서드 (장비 아이템 전용)
    public virtual bool Unequip(Character character)
    {
        // 기본 구현은 실패 반환
        return false;
    }
    
    // 아이템 획득 시 호출 메서드
    public virtual void OnPickup(Character character)
    {
        // 아이템 획득 시 필요한 처리
    }
    
    // 아이템 드롭 시 호출 메서드
    public virtual void OnDrop(Character character, Vector3 position)
    {
        // 아이템 드롭 시 필요한 처리
    }
    
    // 아이템 비교 (스택 가능 여부 등 확인용)
    public virtual bool IsEqual(Item other)
    {
        if (other == null)
            return false;
            
        // 기본적으로 ScriptableObject ID 비교
        return this == other;
    }
    
    // 툴팁 정보 제공
    public virtual string GetTooltipInfo()
    {
        string rarityColor = GetRarityColorHex();
        
        string tooltipText = $"<color={rarityColor}><b>{itemName}</b></color>\n";
        tooltipText += $"{GetItemTypeText()}\n\n";
        tooltipText += $"{description}\n";
        
        if (buyPrice > 0)
        {
            tooltipText += $"\n가격: {buyPrice} 골드";
        }
        
        return tooltipText;
    }
    
    // 희귀도에 따른 색상 코드
    private string GetRarityColorHex()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return "#FFFFFF"; // 하얀색
            case ItemRarity.Uncommon:
                return "#00FF00"; // 초록색
            case ItemRarity.Rare:
                return "#0080FF"; // 파란색
            case ItemRarity.Epic:
                return "#8000FF"; // 보라색
            case ItemRarity.Legendary:
                return "#FF8000"; // 주황색
            default:
                return "#FFFFFF";
        }
    }
    
    // 아이템 유형 텍스트
    private string GetItemTypeText()
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                return "무기";
            case ItemType.Armor:
                return "방어구";
            case ItemType.Accessory:
                return "장신구";
            case ItemType.Consumable:
                return "소비 아이템";
            case ItemType.Material:
                return "재료";
            case ItemType.Quest:
                return "퀘스트 아이템";
            case ItemType.Miscellaneous:
                return "기타";
            default:
                return "알 수 없음";
        }
    }
}

// 아이템 타입 열거형
public enum ItemType
{
    Weapon,
    Armor,
    Accessory,
    Consumable,
    Material,
    Quest,
    Miscellaneous
}

// 아이템 희귀도 열거형
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponItem : Item
{
    [Header("무기 속성")]
    public WeaponType weaponType;
    public int damage;
    public float attackSpeed;
    public float attackRange;
    
    [Header("추가 효과")]
    public ElementType elementType = ElementType.None;
    public int elementalDamage;
    
    [Header("장착 설정")]
    public EquipmentSlot equipSlot = EquipmentSlot.Weapon;
    public GameObject weaponPrefab; // 캐릭터에 장착될 모델
    public Vector3 equipPositionOffset;
    public Vector3 equipRotationOffset;
    
    // 생성자에서 기본 타입 설정
    private void OnValidate()
    {
        itemType = ItemType.Weapon;
        isStackable = false;
    }
    
    public override bool Equip(Character character)
    {
        if (character == null)
            return false;
            
        // 기존 무기 해제
        EquipmentSystem equipmentSystem = character.GetComponent<EquipmentSystem>();
        if (equipmentSystem != null)
        {
            // 해당 슬롯에 이미 장비가 있으면 해제
            Item equippedItem = equipmentSystem.GetEquippedItem(equipSlot);
            if (equippedItem != null)
            {
                equipmentSystem.UnequipItem(equipSlot);
            }
            
            // 새 무기 장착
            bool success = equipmentSystem.EquipItem(this, equipSlot);
            
            if (success)
            {
                // 무기 모델 생성 및 장착
                Transform equipPoint = character.GetEquipPoint(equipSlot);
                if (equipPoint != null && weaponPrefab != null)
                {
                    GameObject weaponInstance = Instantiate(weaponPrefab, equipPoint);
                    weaponInstance.transform.localPosition = equipPositionOffset;
                    weaponInstance.transform.localRotation = Quaternion.Euler(equipRotationOffset);
                    
                    // 무기 인스턴스 참조 저장
                    equipmentSystem.SetEquipmentInstance(equipSlot, weaponInstance);
                }
                
                // 캐릭터 스탯 업데이트
                CharacterStats stats = character.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    stats.AddWeaponDamage(damage);
                    stats.AddAttackSpeed(attackSpeed);
                }
                
                Debug.Log($"{character.name}이(가) {itemName}을(를) 장착했습니다.");
                return true;
            }
        }
        
        return false;
    }
    
    public override bool Unequip(Character character)
    {
        if (character == null)
            return false;
            
        EquipmentSystem equipmentSystem = character.GetComponent<EquipmentSystem>();
        if (equipmentSystem != null)
        {
            // 장비 해제
            bool success = equipmentSystem.UnequipItem(equipSlot);
            
            if (success)
            {
                // 무기 모델 제거
                equipmentSystem.DestroyEquipmentInstance(equipSlot);
                
                // 캐릭터 스탯 업데이트
                CharacterStats stats = character.GetComponent<CharacterStats>();
                if (stats != null)
                {
                    stats.RemoveWeaponDamage(damage);
                    stats.RemoveAttackSpeed(attackSpeed);
                }
                
                Debug.Log($"{character.name}이(가) {itemName}을(를) 해제했습니다.");
                return true;
            }
        }
        
        return false;
    }
    
    public override string GetTooltipInfo()
    {
        string baseTooltip = base.GetTooltipInfo();
        
        string weaponInfo = $"\n\n<color=#FFD700>공격력:</color> {damage}";
        
        if (attackSpeed > 0)
        {
            weaponInfo += $"\n<color=#FFD700>공격 속도:</color> {attackSpeed}";
        }
        
        if (attackRange > 0)
        {
            weaponInfo += $"\n<color=#FFD700>공격 범위:</color> {attackRange}";
        }
        
        if (elementType != ElementType.None && elementalDamage > 0)
        {
            string elementColor = GetElementColorHex();
            weaponInfo += $"\n<color={elementColor}>{GetElementTypeText()}:</color> +{elementalDamage}";
        }
        
        return baseTooltip + weaponInfo;
    }
    
    private string GetElementColorHex()
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return "#FF0000"; // 빨간색
            case ElementType.Ice:
                return "#00FFFF"; // 하늘색
            case ElementType.Lightning:
                return "#FFFF00"; // 노란색
            case ElementType.Poison:
                return "#00FF00"; // 초록색
            case ElementType.Holy:
                return "#FFFFFF"; // 하얀색
            default:
                return "#FFFFFF";
        }
    }
    
    private string GetElementTypeText()
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return "화염 대미지";
            case ElementType.Ice:
                return "냉기 대미지";
            case ElementType.Lightning:
                return "번개 대미지";
            case ElementType.Poison:
                return "독 대미지";
            case ElementType.Holy:
                return "신성 대미지";
            default:
                return "";
        }
    }
}

// 무기 타입 열거형
public enum WeaponType
{
    Sword,
    Axe,
    Mace,
    Dagger,
    Spear,
    Bow,
    Crossbow,
    Staff,
    Wand
}

// 원소 타입 열거형
public enum ElementType
{
    None,
    Fire,
    Ice,
    Lightning,
    Poison,
    Holy
}

using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class ConsumableItem : Item
{
    [Header("소비 효과")]
    public ConsumableType consumableType = ConsumableType.Healing;
    public int effectValue;
    public float effectDuration;
    public GameObject useEffect; // 사용 시 발생할 이펙트
    
    [Header("애니메이션")]
    public string useAnimationTrigger = "UsePotion";
    
    // 생성자에서 기본 타입 설정
    private void OnValidate()
    {
        itemType = ItemType.Consumable;
        isStackable = true;
        maxStackSize = 99;
    }
    
    public override bool Use(Character character)
    {
        if (character == null)
            return false;
            
        bool effectApplied = false;
        
        // 캐릭터 스탯에 효과 적용
        CharacterStats stats = character.GetComponent<CharacterStats>();
        if (stats != null)
        {
            switch (consumableType)
            {
                case ConsumableType.Healing:
                    stats.Heal(effectValue);
                    effectApplied = true;
                    break;
                    
                case ConsumableType.ManaRestore:
                    stats.RestoreMana(effectValue);
                    effectApplied = true;
                    break;
                    
                case ConsumableType.StaminaRestore:
                    stats.RestoreStamina(effectValue);
                    effectApplied = true;
                    break;
                    
                case ConsumableType.TemporaryBuff:
                    // 임시 버프 추가
                    BuffSystem buffSystem = character.GetComponent<BuffSystem>();
                    if (buffSystem != null)
                    {
                        Buff newBuff = new Buff
                        {
                            buffName = itemName,
                            icon = icon,
                            duration = effectDuration,
                            modifiers = GetBuffModifiers()
                        };
                        
                        buffSystem.AddBuff(newBuff);
                        effectApplied = true;
                    }
                    break;
                    
                case ConsumableType.StatusCure:
                    // 상태이상 치료
                    StatusEffectSystem statusSystem = character.GetComponent<StatusEffectSystem>();
                    if (statusSystem != null)
                    {
                        statusSystem.CureStatusEffect((StatusEffectType)effectValue);
                        effectApplied = true;
                    }
                    break;
            }
        }
        
        if (effectApplied)
        {
            // 사용 이펙트 생성
            if (useEffect != null)
            {
                Instantiate(useEffect, character.transform.position + Vector3.up, Quaternion.identity);
            }
            
            // 사용 애니메이션
            Animator animator = character.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(useAnimationTrigger))
            {
                animator.SetTrigger(useAnimationTrigger);
            }
            
            Debug.Log($"{character.name}이(가) {itemName}을(를) 사용했습니다.");
            return true; // 사용 성공
        }
        
        return false; // 사용 실패
    }
    
    // 버프 수정자 생성
    private StatModifier[] GetBuffModifiers()
    {
        // 아이템의 효과에 따라 다른 스탯 수정자 생성
        StatModifier[] modifiers = new StatModifier[1]; // 간단한 예시로 하나만 생성
        
        switch (consumableType)
        {
            case ConsumableType.TemporaryBuff when effectValue > 0:
                // 스탯 증가 버프
                modifiers[0] = new StatModifier(
                    StatType.Attack,
                    ModifierType.Additive,
                    effectValue,
                    itemName
                );
                break;
                
            default:
                // 기본 버프 (공격력 10% 증가)
                modifiers[0] = new StatModifier(
                    StatType.Attack,
                    ModifierType.PercentAdd,
                    10f,
                    itemName
                );
                break;
        }
        
        return modifiers;
    }
    
    public override string GetTooltipInfo()
    {
        string baseTooltip = base.GetTooltipInfo();
        
        string effectInfo = "\n\n<color=#00FFFF>효과:</color> ";
        
        switch (consumableType)
        {
            case ConsumableType.Healing:
                effectInfo += $"체력을 {effectValue} 회복합니다.";
                break;
                
            case ConsumableType.ManaRestore:
                effectInfo += $"마나를 {effectValue} 회복합니다.";
                break;
                
            case ConsumableType.StaminaRestore:
                effectInfo += $"스태미나를 {effectValue} 회복합니다.";
                break;
                
            case ConsumableType.TemporaryBuff:
                effectInfo += $"공격력이 {effectValue} 증가합니다. (지속시간: {effectDuration}초)";
                break;
                
            case ConsumableType.StatusCure:
                effectInfo += "상태이상을 치료합니다.";
                break;
        }
        
        return baseTooltip + effectInfo;
    }
}

// 소비 아이템 타입 열거형
public enum ConsumableType
{
    Healing,
    ManaRestore,
    StaminaRestore,
    TemporaryBuff,
    StatusCure
}

// 상태이상 타입 열거형 (ConsumableItem 참조용)
public enum StatusEffectType
{
    Poison = 1,
    Burn = 2,
    Freeze = 3,
    Stun = 4,
    Silence = 5,
    Blind = 6,
    All = 99
}
```

## ItemDatabase.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

// 아이템 데이터베이스 (ScriptableObject)
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> items = new List<Item>();
    
    // 캐싱용 딕셔너리
    private Dictionary<string, Item> itemById;
    
    // 초기화
    private void OnEnable()
    {
        InitializeDictionary();
    }
    
    // 딕셔너리 초기화
    private void InitializeDictionary()
    {
        itemById = new Dictionary<string, Item>();
        
        foreach (var item in items)
        {
            if (item != null && !string.IsNullOrEmpty(item.name))
            {
                // 중복 검사
                if (!itemById.ContainsKey(item.name))
                {
                    itemById.Add(item.name, item);
                }
                else
                {
                    Debug.LogWarning($"ItemDatabase: 중복 아이템 ID 발견: {item.name}");
                }
            }
        }
    }
    
    // ID로 아이템 가져오기
    public Item GetItemById(string id)
    {
        if (itemById == null)
        {
            InitializeDictionary();
        }
        
        if (itemById.TryGetValue(id, out Item item))
        {
            return item;
        }
        
        Debug.LogWarning($"ItemDatabase: 아이템을 찾을 수 없음: {id}");
        return null;
    }
    
    // 타입으로 아이템 가져오기
    public List<Item> GetItemsByType(ItemType type)
    {
        List<Item> result = new List<Item>();
        
        foreach (var item in items)
        {
            if (item != null && item.itemType == type)
            {
                result.Add(item);
            }
        }
        
        return result;
    }
    
    // 희귀도로 아이템 가져오기
    public List<Item> GetItemsByRarity(ItemRarity rarity)
    {
        List<Item> result = new List<Item>();
        
        foreach (var item in items)
        {
            if (item != null && item.rarity == rarity)
            {
                result.Add(item);
            }
        }
        
        return result;
    }
    
    // 모든 아이템 가져오기
    public List<Item> GetAllItems()
    {
        return new List<Item>(items);
    }
    
    // 아이템 추가 (에디터용)
    public void AddItem(Item item)
    {
        if (item != null && !items.Contains(item))
        {
            items.Add(item);
            
            // 딕셔너리 업데이트
            if (itemById != null && !itemById.ContainsKey(item.name))
            {
                itemById.Add(item.name, item);
            }
        }
    }
}
```
