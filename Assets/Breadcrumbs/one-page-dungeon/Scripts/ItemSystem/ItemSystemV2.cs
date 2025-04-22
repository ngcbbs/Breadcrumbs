using System;
using System.Collections.Generic;
using System.Linq;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {

    #region 무기 구현

    

    /// <summary>
    /// 무기 효과: 원소 데미지
    /// </summary>
    public class ElementalDamageEffect : ItemEffectBase {
        public ElementType ElementType { get; private set; }
        public float DamagePercent { get; private set; }
        private StatModifier appliedModifier;

        public ElementalDamageEffect(ElementType elementType, float damagePercent) {
            ElementType = elementType;
            DamagePercent = damagePercent;
            EffectName = $"{elementType} Damage";
            Description = $"Deals additional {damagePercent:P0} {elementType} damage";
        }

        public override void Apply(PlayerCharacter character) {
            // 원소 데미지 증가 스탯 추가
            appliedModifier = new StatModifier(DamagePercent, StatModifierType.PercentAdditive, this);
            character.Stats.AddModifier(StatType.PhysicalAttack, appliedModifier);

            // 직업별 특수 효과 적용
            if (character.ClassType == ClassType.Mage && ElementType == ElementType.Fire) {
                // 마법사의 화염 스킬 강화
                BuffSystem.ApplyTemporaryBuff(character, StatType.MagicAttack, 0.1f, StatModifierType.PercentAdditive, 10f);
            }
        }

        public override void Remove(PlayerCharacter character) {
            character.Stats.RemoveAllModifiersFromSource(this);
        }

        public override IItemEffect Clone() {
            return new ElementalDamageEffect(ElementType, DamagePercent);
        }
    }

    #endregion

    #region 방어구 구현


    /// <summary>
    /// 방어구 효과: 물리 피해 감소
    /// </summary>
    public class PhysicalDamageReductionEffect : ItemEffectBase {
        public float ReductionPercent { get; private set; }

        public PhysicalDamageReductionEffect(float reductionPercent) {
            ReductionPercent = reductionPercent;
            EffectName = "Physical Damage Reduction";
            Description = $"Reduces physical damage taken by {reductionPercent:P0}";
        }

        public override void Apply(PlayerCharacter character) {
            StatModifier defenseBonus = new StatModifier(ReductionPercent, StatModifierType.PercentAdditive, this);
            character.Stats.AddModifier(StatType.PhysicalDefense, defenseBonus);
        }

        public override void Remove(PlayerCharacter character) {
            character.Stats.RemoveAllModifiersFromSource(this);
        }

        public override IItemEffect Clone() {
            return new PhysicalDamageReductionEffect(ReductionPercent);
        }
    }

    #endregion

    #region 액세서리 구현


    /// <summary>
    /// 액세서리 효과: 경험치 증가
    /// </summary>
    public class ExperienceBoostEffect : ItemEffectBase {
        public float BoostPercent { get; private set; }

        public ExperienceBoostEffect(float boostPercent) {
            BoostPercent = boostPercent;
            EffectName = "Experience Boost";
            Description = $"Increases experience gain by {boostPercent:P0}";
        }

        public override void Apply(PlayerCharacter character) {
            // 경험치 획득 향상은 이벤트로 처리하거나 캐릭터 속성으로 저장 필요
            // 예시 구현: 캐릭터에 경험치 부스트 등록
        }

        public override void Remove(PlayerCharacter character) {
            // 경험치 획득 향상 제거
        }

        public override IItemEffect Clone() {
            return new ExperienceBoostEffect(BoostPercent);
        }
    }

    #endregion

    #region 아이템 팩토리 및 데이터베이스

    /// <summary>
    /// 방어구 팩토리 클래스
    /// </summary>
    public class ArmorFactory : IItemFactory<ArmorItem> {
        private readonly ItemDatabaseImpl itemDatabase;

        public ArmorFactory(ItemDatabaseImpl itemDatabase) {
            this.itemDatabase = itemDatabase;
        }

        public ArmorItem CreateItem(string itemId) {
            EquipmentItem baseItem = itemDatabase.GetItemById(itemId);
            if (baseItem is ArmorItem armorItem) {
                return (ArmorItem)armorItem.Clone();
            }

            Debug.LogError($"Item with ID {itemId} is not an armor or does not exist");
            return null;
        }

        public ArmorItem CreateRandomItem(int itemLevel, ItemRarity minRarity = ItemRarity.Common) {
            // 기본 방어구 템플릿 생성
            ArmorItem armor = new ArmorItem {
                ItemId = $"ARM_{Guid.NewGuid().ToString().Substring(0, 8)}",
                ItemLevel = itemLevel,
                Rarity = GetRandomRarity(minRarity),
                ArmorType = GetRandomArmorType(),
                EquipSlot = GetRandomArmorSlot(),
                RequiredLevel = Mathf.Max(1, itemLevel - 5)
            };

            // 방어구 이름 설정
            armor.ItemName = $"{armor.Rarity} {armor.ArmorType} {armor.EquipSlot}";

            // 방어구 타입별 기본값 설정
            SetupArmorTypeDefaults(armor);

            // 레어리티에 따른 추가 스탯
            if (armor.Rarity >= ItemRarity.Uncommon) {
                AddRandomStats(armor);
            }

            // 특수 효과
            if (armor.Rarity >= ItemRarity.Rare) {
                AddRandomEffects(armor);
            }

            return armor;
        }

        private ItemRarity GetRandomRarity(ItemRarity minRarity) {
            // 확률에 따른 레어리티 결정
            float roll = UnityEngine.Random.value;

            if (roll < 0.01f && (int)minRarity <= (int)ItemRarity.Legendary)
                return ItemRarity.Legendary;
            if (roll < 0.05f && (int)minRarity <= (int)ItemRarity.Epic)
                return ItemRarity.Epic;
            if (roll < 0.15f && (int)minRarity <= (int)ItemRarity.Rare)
                return ItemRarity.Rare;
            if (roll < 0.35f && (int)minRarity <= (int)ItemRarity.Uncommon)
                return ItemRarity.Uncommon;

            return ItemRarity.Common;
        }

        private ArmorType GetRandomArmorType() {
            Array values = Enum.GetValues(typeof(ArmorType));
            return (ArmorType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        private EquipmentSlot GetRandomArmorSlot() {
            EquipmentSlot[] armorSlots = {
                EquipmentSlot.Head, EquipmentSlot.Chest, EquipmentSlot.Legs,
                EquipmentSlot.Feet, EquipmentSlot.Hands, EquipmentSlot.Shoulders
            };

            return armorSlots[UnityEngine.Random.Range(0, armorSlots.Length)];
        }

        private void SetupArmorTypeDefaults(ArmorItem armor) {
            float baseValue = armor.ItemLevel * 1.0f;

            // 방어구 타입별 배율
            float armorTypeMultiplier = 1.0f;
            switch (armor.ArmorType) {
                case ArmorType.Cloth:
                    armorTypeMultiplier = 0.6f;
                    armor.MovementPenalty = 0f;
                    break;
                case ArmorType.Leather:
                    armorTypeMultiplier = 0.8f;
                    armor.MovementPenalty = 0.02f;
                    break;
                case ArmorType.Mail:
                    armorTypeMultiplier = 1.2f;
                    armor.MovementPenalty = 0.05f;
                    break;
                case ArmorType.Plate:
                    armorTypeMultiplier = 1.5f;
                    armor.MovementPenalty = 0.1f;
                    break;
                case ArmorType.Robe:
                    armorTypeMultiplier = 0.5f;
                    armor.MovementPenalty = 0f;
                    break;
            }

            // 장비 슬롯별 배율
            float slotMultiplier = 1.0f;
            switch (armor.EquipSlot) {
                case EquipmentSlot.Chest:
                    slotMultiplier = 1.5f;
                    break;
                case EquipmentSlot.Legs:
                    slotMultiplier = 1.2f;
                    break;
                case EquipmentSlot.Head:
                    slotMultiplier = 1.0f;
                    break;
                case EquipmentSlot.Shoulders:
                    slotMultiplier = 0.9f;
                    break;
                case EquipmentSlot.Hands:
                case EquipmentSlot.Feet:
                    slotMultiplier = 0.7f;
                    break;
                default:
                    slotMultiplier = 0.5f;
                    break;
            }

            // 레어리티 보너스
            float rarityMultiplier = 1 + ((int)armor.Rarity * 0.2f);

            // 물리 방어력 계산
            armor.BaseDefense = baseValue * armorTypeMultiplier * slotMultiplier * rarityMultiplier;

            // 마법 방어력 계산 (방어구 타입에 따라 다름)
            float magicDefenseMultiplier = 1.0f;
            switch (armor.ArmorType) {
                case ArmorType.Cloth:
                    magicDefenseMultiplier = 1.3f;
                    break;
                case ArmorType.Robe:
                    magicDefenseMultiplier = 1.5f;
                    break;
                case ArmorType.Leather:
                    magicDefenseMultiplier = 1.0f;
                    break;
                case ArmorType.Mail:
                    magicDefenseMultiplier = 0.8f;
                    break;
                case ArmorType.Plate:
                    magicDefenseMultiplier = 0.6f;
                    break;
            }

            armor.MagicDefense = (baseValue * 0.7f) * magicDefenseMultiplier * slotMultiplier * rarityMultiplier;
        }

        private void AddRandomStats(ArmorItem armor) {
            // 방어구 타입에 따라 기본 스탯 추가
            switch (armor.ArmorType) {
                case ArmorType.Plate:
                    armor.AddStatModifier(StatType.Strength, 3 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    armor.AddStatModifier(StatType.Vitality, 5 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    break;

                case ArmorType.Mail:
                    armor.AddStatModifier(StatType.Strength, 2 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    armor.AddStatModifier(StatType.Vitality, 4 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    break;

                case ArmorType.Leather:
                    armor.AddStatModifier(StatType.Dexterity, 4 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    armor.AddStatModifier(StatType.Vitality, 2 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    break;

                case ArmorType.Cloth:
                    armor.AddStatModifier(StatType.Intelligence, 4 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    armor.AddStatModifier(StatType.Wisdom, 3 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    break;

                case ArmorType.Robe:
                    armor.AddStatModifier(StatType.Intelligence, 5 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    armor.AddStatModifier(StatType.Wisdom, 4 * ((int)armor.Rarity + 1), StatModifierType.Flat);
                    break;
            }

            // 랜덤 추가 스탯
            int statCount = GetRandomStatCount(armor.Rarity) - 2; // 기본 스탯 2개 제외
            for (int i = 0; i < statCount; i++) {
                AddRandomStat(armor);
            }
        }

        private int GetRandomStatCount(ItemRarity rarity) {
            switch (rarity) {
                case ItemRarity.Common:
                    return 2;
                case ItemRarity.Uncommon:
                    return UnityEngine.Random.Range(2, 4);
                case ItemRarity.Rare:
                    return UnityEngine.Random.Range(3, 5);
                case ItemRarity.Epic:
                    return UnityEngine.Random.Range(4, 6);
                case ItemRarity.Legendary:
                    return UnityEngine.Random.Range(5, 8);
                default:
                    return 2;
            }
        }

        private void AddRandomStat(ArmorItem armor) {
            // 추가 가능 스탯 목록
            StatType[] possibleStats = {
                StatType.Strength, StatType.Dexterity, StatType.Intelligence,
                StatType.Vitality, StatType.Wisdom, StatType.Luck,
                StatType.MaxHealth, StatType.MaxMana, StatType.HealthRegen,
                StatType.ManaRegen, StatType.PhysicalDefense, StatType.MagicDefense,
                StatType.MovementSpeed, StatType.CriticalChance
            };

            StatType statType = possibleStats[UnityEngine.Random.Range(0, possibleStats.Length)];

            // 스탯 값 계산
            float value;
            StatModifierType modType;

            switch (statType) {
                case StatType.MaxHealth:
                case StatType.MaxMana:
                    value = UnityEngine.Random.Range(10, 30) * ((int)armor.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.HealthRegen:
                case StatType.ManaRegen:
                    value = UnityEngine.Random.Range(0.5f, 2f) * ((int)armor.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.MovementSpeed:
                    value = UnityEngine.Random.Range(0.01f, 0.03f) * ((int)armor.Rarity + 1);
                    modType = StatModifierType.PercentAdditive;
                    break;

                case StatType.CriticalChance:
                    value = UnityEngine.Random.Range(0.005f, 0.01f) * ((int)armor.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                default:
                    value = UnityEngine.Random.Range(1, 4) * (1 + ((int)armor.Rarity * 0.3f));
                    modType = StatModifierType.Flat;
                    break;
            }

            armor.AddStatModifier(statType, value, modType);
        }

        private void AddRandomEffects(ArmorItem armor) {
            // 에픽 이상은 세트 효과 가능성
            if (armor.Rarity >= ItemRarity.Epic && UnityEngine.Random.value < 0.4f) {
                armor.HasSetBonus = true;
                armor.SetName = $"{armor.ArmorType} of the {GetRandomSetName()}";
            }

            // 물리 데미지 감소 효과 추가
            if (armor.ArmorType == ArmorType.Plate || armor.ArmorType == ArmorType.Mail) {
                float reductionPercent = 0.05f * ((int)armor.Rarity + 1) * 0.5f;
                armor.AddEffect(new PhysicalDamageReductionEffect(reductionPercent));
            }
        }

        private string GetRandomSetName() {
            string[] prefixes = { "Dragon", "Phoenix", "Celestial", "Ancient", "Eternal", "Fiery", "Frozen", "Arcane" };
            string[] suffixes = { "Guardian", "Warlord", "Knight", "Conqueror", "Mage", "Sage", "Champion", "Destroyer" };

            return
                $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {suffixes[UnityEngine.Random.Range(0, suffixes.Length)]}";
        }
    }

    /// <summary>
    /// 아이템 저장소: 아이템 타입별로 캐싱 및 조회 기능 제공
    /// </summary>
    public class ItemRepository<T> where T : EquipmentItem {
        private Dictionary<string, T> itemsById = new Dictionary<string, T>();

        public void RegisterItem(T item) {
            if (item != null && !string.IsNullOrEmpty(item.ItemId)) {
                itemsById[item.ItemId] = item;
            }
        }

        public T GetById(string id) {
            if (string.IsNullOrEmpty(id))
                return null;

            if (itemsById.TryGetValue(id, out T item))
                return item;

            return null;
        }

        public List<T> GetAll() {
            return new List<T>(itemsById.Values);
        }

        public List<T> FindByRarity(ItemRarity rarity) {
            return itemsById.Values.Where(item => item.Rarity == rarity).ToList();
        }

        public List<T> FindByLevel(int maxLevel) {
            return itemsById.Values.Where(item => item.RequiredLevel <= maxLevel).ToList();
        }

        public List<T> FindByClass(ClassType classType) {
            return itemsById.Values.Where(item =>
                item.ClassType == classType || item.ClassType == ClassType.None
            ).ToList();
        }

        public List<T> FindByCustomCriteria(Func<T, bool> predicate) {
            return itemsById.Values.Where(predicate).ToList();
        }
    }

    #endregion

}