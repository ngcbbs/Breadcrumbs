using System;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    /// <summary>
    /// 액세서리 팩토리 클래스
    /// </summary>
    public class AccessoryFactory : IItemFactory<AccessoryItem> {
        private readonly ItemDatabaseImpl itemDatabase;

        public AccessoryFactory(ItemDatabaseImpl itemDatabase) {
            this.itemDatabase = itemDatabase;
        }

        public AccessoryItem CreateItem(string itemId) {
            EquipmentItem baseItem = itemDatabase.GetItemById(itemId);
            if (baseItem is AccessoryItem accessoryItem) {
                return (AccessoryItem)accessoryItem.Clone();
            }

            Debug.LogError($"Item with ID {itemId} is not an accessory or does not exist");
            return null;
        }

        public AccessoryItem CreateRandomItem(int itemLevel, ItemRarity minRarity = ItemRarity.Common) {
            // 기본 액세서리 템플릿 생성
            AccessoryItem accessory = new AccessoryItem {
                ItemId = $"ACC_{Guid.NewGuid().ToString().Substring(0, 8)}",
                ItemLevel = itemLevel,
                Rarity = GetRandomRarity(minRarity),
                EquipSlot = GetRandomAccessorySlot(),
                RequiredLevel = Mathf.Max(1, itemLevel - 5)
            };

            // 액세서리 이름 설정
            accessory.ItemName = $"{accessory.Rarity} {accessory.EquipSlot}";

            // 스탯 추가
            for (int i = 0; i < GetRandomStatCount(accessory.Rarity); i++) {
                AddRandomStat(accessory);
            }

            // 유니크 효과 (확률적)
            if (accessory.Rarity >= ItemRarity.Epic && UnityEngine.Random.value < 0.3f) {
                accessory.IsUnique = true;
                AddUniqueEffect(accessory);
            }

            // 발동 효과 (확률적)
            if (accessory.Rarity >= ItemRarity.Rare && UnityEngine.Random.value < 0.4f) {
                accessory.HasProc = true;
                AddProcEffect(accessory);
            }

            // 패시브 능력 추가
            AddPassiveAbilities(accessory);

            return accessory;
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

        private EquipmentSlot GetRandomAccessorySlot() {
            EquipmentSlot[] accessorySlots = {
                EquipmentSlot.Neck, EquipmentSlot.Ring1, EquipmentSlot.Ring2,
                EquipmentSlot.Back, EquipmentSlot.Waist
            };

            return accessorySlots[UnityEngine.Random.Range(0, accessorySlots.Length)];
        }

        private int GetRandomStatCount(ItemRarity rarity) {
            switch (rarity) {
                case ItemRarity.Common:
                    return 1;
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

        private void AddRandomStat(AccessoryItem accessory) {
            // 추가 가능 스탯 목록
            StatType[] possibleStats = {
                StatType.Strength, StatType.Dexterity, StatType.Intelligence,
                StatType.Vitality, StatType.Wisdom, StatType.Luck,
                StatType.CriticalChance, StatType.CriticalDamage,
                StatType.PhysicalAttack, StatType.MagicAttack,
                StatType.PhysicalDefense, StatType.MagicDefense,
                StatType.MaxHealth, StatType.MaxMana
            };

            StatType statType = possibleStats[UnityEngine.Random.Range(0, possibleStats.Length)];

            // 스탯 값 계산
            float value;
            StatModifierType modType;

            switch (statType) {
                case StatType.CriticalChance:
                    value = UnityEngine.Random.Range(0.01f, 0.03f) * ((int)accessory.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.CriticalDamage:
                    value = UnityEngine.Random.Range(0.05f, 0.1f) * ((int)accessory.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.MaxHealth:
                case StatType.MaxMana:
                    value = UnityEngine.Random.Range(10, 25) * ((int)accessory.Rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                default:
                    value = UnityEngine.Random.Range(2, 5) * (1 + ((int)accessory.Rarity * 0.3f));
                    modType = StatModifierType.Flat;
                    break;
            }

            accessory.AddStatModifier(statType, value, modType);
        }

        private void AddUniqueEffect(AccessoryItem accessory) {
            // 유니크 효과 예시 (슬롯별 다른 효과)
            switch (accessory.EquipSlot) {
                case EquipmentSlot.Neck:
                    // 목걸이 - 경험치 획득 증가
                    accessory.AddEffect(new ExperienceBoostEffect(0.1f * ((int)accessory.Rarity - 2)));
                    break;

                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                    // 반지 - 재사용 대기시간 감소
                    // 실제 구현은 게임 시스템에 따라 달라짐
                    break;

                case EquipmentSlot.Back:
                    // 망토 - 회피율 증가
                    accessory.AddStatModifier(StatType.Evasion, 0.05f * ((int)accessory.Rarity - 2), StatModifierType.Flat);
                    break;

                case EquipmentSlot.Waist:
                    // 허리띠 - 이동 속도 증가
                    accessory.AddStatModifier(StatType.MovementSpeed, 0.05f * ((int)accessory.Rarity - 2),
                        StatModifierType.PercentAdditive);
                    break;
            }
        }

        private void AddProcEffect(AccessoryItem accessory) {
            // 발동 효과 예시 (장비 슬롯별 다른 효과)
            string[] effectNames = {
                "생명력 흡수", "마나 흡수", "얼음 폭발", "화염 폭발", "번개 연쇄", "회복 파동"
            };

            string effectName = effectNames[UnityEngine.Random.Range(0, effectNames.Length)];
            float chance = 0.05f + ((int)accessory.Rarity - 2) * 0.05f; // 레어: 5%, 에픽: 10%, 레전더리: 15%

            // 발동 효과 설명 추가
            accessory.Description += $"\n\n{chance * 100}% 확률로 {effectName} 효과 발동";

            // 실제 발동 효과는 게임 시스템에 따라 구현
        }

        private void AddPassiveAbilities(AccessoryItem accessory) {
            // 액세서리 패시브 능력 추가
            string[] abilityNames = {
                "마력 집중", "불굴의 의지", "신속한 회복", "마나 순환", "정밀한 일격", "치명적 약점 공략"
            };

            int abilityCount = Mathf.Max(1, (int)accessory.Rarity);

            for (int i = 0; i < abilityCount; i++) {
                string abilityName = abilityNames[UnityEngine.Random.Range(0, abilityNames.Length)];
                float value = 5f + ((int)accessory.Rarity * 3f);

                accessory.AddPassiveAbility(abilityName, $"{abilityName} 능력으로 특수 효과 발동 가능", value);
            }
        }
    }
}