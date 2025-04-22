using System;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    /// <summary>
    /// 무기 팩토리 클래스
    /// </summary>
    public class WeaponFactory : IItemFactory<WeaponItem> {
        private readonly ItemDatabaseImpl itemDatabase;

        public WeaponFactory(ItemDatabaseImpl itemDatabase) {
            this.itemDatabase = itemDatabase;
        }

        public WeaponItem CreateItem(string itemId) {
            EquipmentItem baseItem = itemDatabase.GetItemById(itemId);
            if (baseItem is WeaponItem weaponItem) {
                return (WeaponItem)weaponItem.Clone();
            }

            Debug.LogError($"Item with ID {itemId} is not a weapon or does not exist");
            return null;
        }

        public WeaponItem CreateRandomItem(int itemLevel, ItemRarity minRarity = ItemRarity.Common) {
            // 기본 무기 템플릿 생성
            WeaponItem weapon = new WeaponItem {
                ItemId = $"WPN_{Guid.NewGuid().ToString().Substring(0, 8)}",
                ItemLevel = itemLevel,
                Rarity = GetRandomRarity(minRarity),
                WeaponType = GetRandomWeaponType(),
                RequiredLevel = Mathf.Max(1, itemLevel - 5)
            };

            // 무기 이름 설정
            weapon.ItemName = $"{weapon.Rarity} {weapon.WeaponType}";

            // 무기 타입별 기본값 설정
            SetupWeaponTypeDefaults(weapon);

            // 레어리티에 따른 추가 스탯
            if (weapon.Rarity >= ItemRarity.Uncommon) {
                AddRandomStats(weapon);
            }

            // 특수 효과
            if (weapon.Rarity >= ItemRarity.Rare) {
                AddRandomEffects(weapon);
            }

            return weapon;
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

        private WeaponType GetRandomWeaponType() {
            Array values = Enum.GetValues(typeof(WeaponType));
            return (WeaponType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        private void SetupWeaponTypeDefaults(WeaponItem weapon) {
            float baseValue = weapon.ItemLevel * 1.5f;

            // 무기 타입별 기본값 설정
            switch (weapon.WeaponType) {
                case WeaponType.Dagger:
                    weapon.BaseDamage = baseValue * 0.7f;
                    weapon.AttackSpeed = 1.8f;
                    weapon.Range = 1.0f;
                    weapon.IsTwoHanded = false;
                    break;
                case WeaponType.Sword:
                    weapon.BaseDamage = baseValue * 1.0f;
                    weapon.AttackSpeed = 1.5f;
                    weapon.Range = 1.5f;
                    weapon.IsTwoHanded = false;
                    break;
                case WeaponType.Axe:
                    weapon.BaseDamage = baseValue * 1.2f;
                    weapon.AttackSpeed = 1.2f;
                    weapon.Range = 1.2f;
                    weapon.IsTwoHanded = false;
                    break;
                case WeaponType.Mace:
                    weapon.BaseDamage = baseValue * 1.2f;
                    weapon.AttackSpeed = 1.0f;
                    weapon.Range = 1.2f;
                    weapon.IsTwoHanded = false;
                    break;
                case WeaponType.Spear:
                    weapon.BaseDamage = baseValue * 1.1f;
                    weapon.AttackSpeed = 1.3f;
                    weapon.Range = 2.0f;
                    weapon.IsTwoHanded = true;
                    break;
                case WeaponType.Bow:
                    weapon.BaseDamage = baseValue * 1.0f;
                    weapon.AttackSpeed = 1.0f;
                    weapon.Range = 15.0f;
                    weapon.IsTwoHanded = true;
                    break;
                case WeaponType.Crossbow:
                    weapon.BaseDamage = baseValue * 1.0f;
                    weapon.AttackSpeed = 0.8f;
                    weapon.Range = 15.0f;
                    weapon.IsTwoHanded = true;
                    break;
                case WeaponType.Staff:
                    weapon.BaseDamage = baseValue * 0.8f;
                    weapon.AttackSpeed = 1.1f;
                    weapon.Range = 10.0f;
                    weapon.IsTwoHanded = true;
                    break;
                case WeaponType.Wand:
                    weapon.BaseDamage = baseValue * 0.6f;
                    weapon.AttackSpeed = 1.3f;
                    weapon.Range = 8.0f;
                    weapon.IsTwoHanded = false;
                    break;
                case WeaponType.Shield:
                    weapon.BaseDamage = baseValue * 0.5f;
                    weapon.AttackSpeed = 1.0f;
                    weapon.Range = 1.0f;
                    weapon.IsTwoHanded = false;
                    break;
            }

            // 장비 슬롯 설정
            weapon.EquipSlot = weapon.IsTwoHanded ? EquipmentSlot.MainHand : EquipmentSlot.MainHand;

            // 레어리티 승수 적용
            float rarityMultiplier = 1 + ((int)weapon.Rarity * 0.2f);
            weapon.BaseDamage *= rarityMultiplier;
        }

        private void AddRandomStats(WeaponItem weapon) {
            // 무기 타입에 따라 기본 스탯 추가
            switch (weapon.WeaponType) {
                case WeaponType.Sword:
                case WeaponType.Axe:
                case WeaponType.Mace:
                case WeaponType.Spear:
                    weapon.AddStatModifier(StatType.Strength, 5 * ((int)weapon.Rarity + 1), StatModifierType.Flat);
                    break;

                case WeaponType.Dagger:
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    weapon.AddStatModifier(StatType.Dexterity, 5 * ((int)weapon.Rarity + 1), StatModifierType.Flat);
                    break;

                case WeaponType.Staff:
                case WeaponType.Wand:
                    weapon.AddStatModifier(StatType.Intelligence, 5 * ((int)weapon.Rarity + 1), StatModifierType.Flat);
                    break;

                case WeaponType.Shield:
                    weapon.AddStatModifier(StatType.PhysicalDefense, 10 * ((int)weapon.Rarity + 1), StatModifierType.Flat);
                    break;
            }

            // 랜덤 추가 스탯
            int statCount = GetRandomStatCount(weapon.Rarity);
            for (int i = 0; i < statCount; i++) {
                AddRandomStat(weapon);
            }
        }

        private int GetRandomStatCount(ItemRarity rarity) {
            switch (rarity) {
                case ItemRarity.Common:
                    return 0;
                case ItemRarity.Uncommon:
                    return UnityEngine.Random.Range(1, 3);
                case ItemRarity.Rare:
                    return UnityEngine.Random.Range(2, 4);
                case ItemRarity.Epic:
                    return UnityEngine.Random.Range(3, 5);
                case ItemRarity.Legendary:
                    return UnityEngine.Random.Range(4, 7);
                default:
                    return 1;
            }
        }

        private void AddRandomStat(WeaponItem weapon) {
            // 추가 가능 스탯 목록
            StatType[] possibleStats = {
                StatType.Strength, StatType.Dexterity, StatType.Intelligence,
                StatType.Vitality, StatType.Wisdom, StatType.Luck,
                StatType.CriticalChance, StatType.CriticalDamage,
                StatType.AttackSpeed, StatType.MovementSpeed,
                StatType.PhysicalAttack, StatType.MagicAttack
            };

            StatType statType = possibleStats[UnityEngine.Random.Range(0, possibleStats.Length)];

            // 스탯 타입에 따라 다른 기본값과 타입
            float value;
            StatModifierType modType;

            switch (statType) {
                case StatType.CriticalChance:
                    value = UnityEngine.Random.Range(0.01f, 0.03f) * ((int)weapon.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.CriticalDamage:
                    value = UnityEngine.Random.Range(0.05f, 0.1f) * ((int)weapon.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.AttackSpeed:
                case StatType.MovementSpeed:
                    value = UnityEngine.Random.Range(0.02f, 0.05f) * ((int)weapon.Rarity + 1);
                    modType = StatModifierType.PercentAdditive;
                    break;

                default:
                    value = UnityEngine.Random.Range(2, 6) * (1 + ((int)weapon.Rarity * 0.3f));
                    modType = StatModifierType.Flat;
                    break;
            }

            weapon.AddStatModifier(statType, value, modType);
        }

        private void AddRandomEffects(WeaponItem weapon) {
            // 레어 이상은 원소 속성 추가 가능성
            if (weapon.Rarity >= ItemRarity.Rare && UnityEngine.Random.value < 0.3f) {
                ElementType[] elements = { ElementType.Fire, ElementType.Ice, ElementType.Lightning, ElementType.Earth };
                weapon.ElementType = elements[UnityEngine.Random.Range(0, elements.Length)];
                weapon.ElementalDamage = weapon.BaseDamage * (0.1f + ((int)weapon.Rarity - 2) * 0.1f);

                // 원소 데미지 효과 추가
                float damagePercent = 0.1f + ((int)weapon.Rarity - 2) * 0.1f; // 레어: 10%, 에픽: 20%, 레전더리: 30%
                weapon.AddEffect(new ElementalDamageEffect(weapon.ElementType, damagePercent));
            }

            // 특수 이펙트 이름
            string[] effectNames = {
                "불꽃 폭발", "얼음 충격", "번개 쇼크", "독성 분출",
                "치유의 빛", "생명력 흡수", "마나 회복", "관통 타격"
            };

            // 특수 이펙트 추가 (확률적)
            if (UnityEngine.Random.value < 0.3f) {
                string effectName = effectNames[UnityEngine.Random.Range(0, effectNames.Length)];
                float chance = 0.05f + ((int)weapon.Rarity - 2) * 0.05f; // 레어: 5%, 에픽: 10%, 레전더리: 15%

                // 특수 효과 추가 (실제 효과는 게임 시스템에 따라 다름)
                // 예: 칼 - 생명력 흡수, 지팡이 - 마나 재생 등
            }
        }
    }
}