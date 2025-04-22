namespace Breadcrumbs.CharacterSystem {
    // 장비 슬롯

    // 무기 타입

    // 방어구 타입

    // 아이템 희귀도

    // 장비 아이템 기본 클래스

    // 무기 아이템

    // 방어구 아이템

    // 액세서리 아이템

    // 장비 세트 데이터

    // 아이템 생성 팩토리
    public class ItemFactory {
        // 기본 아이템 프리셋 생성
        public static WeaponItem CreateWeapon(WeaponType type, int itemLevel, ItemRarity rarity) {
            WeaponItem weapon = new WeaponItem {
                itemId = $"WPN_{type}_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                itemName = $"{rarity} {type}",
                itemLevel = itemLevel,
                rarity = rarity,
                weaponType = type
            };

            // 기본 값 설정
            weapon.baseDamage = CalculateBaseDamage(type, itemLevel, rarity);
            weapon.attackSpeed = GetBaseAttackSpeed(type);
            weapon.range = GetWeaponRange(type);
            weapon.isTwoHanded = IsTwoHandedWeapon(type);

            // 레어리티에 따른 추가 스탯
            if (rarity >= ItemRarity.Uncommon)
                AddRandomWeaponStats(weapon, rarity);

            // 특수 효과
            if (rarity >= ItemRarity.Rare)
                AddRandomSpecialEffect(weapon, rarity);

            return weapon;
        }

        public static ArmorItem CreateArmor(ArmorType type, EquipmentSlot slot, int itemLevel, ItemRarity rarity) {
            ArmorItem armor = new ArmorItem {
                itemId = $"ARM_{type}_{slot}_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                itemName = $"{rarity} {type} {slot}",
                itemLevel = itemLevel,
                rarity = rarity,
                armorType = type,
                equipSlot = slot
            };

            // 기본 값 설정
            armor.baseDefense = CalculateBaseDefense(type, slot, itemLevel, rarity);
            armor.magicDefense = CalculateBaseMagicDefense(type, slot, itemLevel, rarity);

            // 방어구 타입별 특성
            SetArmorTypeStats(armor, type);

            // 레어리티에 따른 추가 스탯
            if (rarity >= ItemRarity.Uncommon)
                AddRandomArmorStats(armor, rarity);

            // 특수 효과
            if (rarity >= ItemRarity.Rare)
                AddRandomSpecialEffect(armor, rarity);

            return armor;
        }

        public static AccessoryItem CreateAccessory(EquipmentSlot slot, int itemLevel, ItemRarity rarity) {
            AccessoryItem accessory = new AccessoryItem {
                itemId = $"ACC_{slot}_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                itemName = $"{rarity} {slot}",
                itemLevel = itemLevel,
                rarity = rarity,
                equipSlot = slot
            };

            // 액세서리는 특별한 스탯만 가짐
            for (int i = 0; i < GetRandomStatCount(rarity); i++) {
                AddRandomStat(accessory, rarity);
            }

            // 유니크 효과 (확률적)
            if (rarity >= ItemRarity.Epic && UnityEngine.Random.value < 0.3f) {
                accessory.isUnique = true;
                // 특별한 유니크 효과 추가
            }

            // 발동 효과 (확률적)
            if (rarity >= ItemRarity.Rare && UnityEngine.Random.value < 0.4f) {
                accessory.hasProc = true;
                AddRandomSpecialEffect(accessory, rarity);
            }

            return accessory;
        }

        #region 장비 속성 계산 메서드

        // 무기 기본 데미지 계산
        private static float CalculateBaseDamage(WeaponType type, int itemLevel, ItemRarity rarity) {
            float baseValue = itemLevel * 1.5f;

            // 무기 타입별 기본값 조정
            switch (type) {
                case WeaponType.Dagger:
                    baseValue *= 0.7f;
                    break;
                case WeaponType.Sword:
                    baseValue *= 1.0f;
                    break;
                case WeaponType.Mace:
                case WeaponType.Axe:
                    baseValue *= 1.2f;
                    break;
                case WeaponType.Spear:
                    baseValue *= 1.1f;
                    break;
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    baseValue *= 1.0f;
                    break;
                case WeaponType.Staff:
                    baseValue *= 0.8f;
                    break;
                case WeaponType.Wand:
                    baseValue *= 0.6f;
                    break;
                case WeaponType.Shield:
                    baseValue *= 0.5f;
                    break;
            }

            // 레어리티 보너스
            float rarityMultiplier = 1 + ((int)rarity * 0.2f);

            return baseValue * rarityMultiplier;
        }

        // 무기 공격 속도 계산
        private static float GetBaseAttackSpeed(WeaponType type) {
            switch (type) {
                case WeaponType.Dagger:
                    return 1.8f;
                case WeaponType.Sword:
                    return 1.5f;
                case WeaponType.Axe:
                    return 1.2f;
                case WeaponType.Mace:
                    return 1.0f;
                case WeaponType.Spear:
                    return 1.3f;
                case WeaponType.Bow:
                    return 1.0f;
                case WeaponType.Crossbow:
                    return 0.8f;
                case WeaponType.Staff:
                    return 1.1f;
                case WeaponType.Wand:
                    return 1.3f;
                case WeaponType.Shield:
                    return 1.0f;
                default:
                    return 1.0f;
            }
        }

        // 무기 범위 계산
        private static float GetWeaponRange(WeaponType type) {
            switch (type) {
                case WeaponType.Dagger:
                    return 1.0f;
                case WeaponType.Sword:
                    return 1.5f;
                case WeaponType.Axe:
                case WeaponType.Mace:
                    return 1.2f;
                case WeaponType.Spear:
                    return 2.0f;
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    return 15.0f;
                case WeaponType.Staff:
                    return 10.0f;
                case WeaponType.Wand:
                    return 8.0f;
                case WeaponType.Shield:
                    return 1.0f;
                default:
                    return 1.0f;
            }
        }

        // 양손 무기 여부 확인
        private static bool IsTwoHandedWeapon(WeaponType type) {
            switch (type) {
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                case WeaponType.Staff:
                case WeaponType.Spear:
                    return true;
                default:
                    return false;
            }
        }

        // 방어구 물리 방어력 계산
        private static float CalculateBaseDefense(ArmorType type, EquipmentSlot slot, int itemLevel, ItemRarity rarity) {
            float baseValue = itemLevel * 1.0f;

            // 방어구 타입별 배율
            float armorTypeMultiplier = 1.0f;
            switch (type) {
                case ArmorType.Cloth:
                    armorTypeMultiplier = 0.6f;
                    break;
                case ArmorType.Leather:
                    armorTypeMultiplier = 0.8f;
                    break;
                case ArmorType.Mail:
                    armorTypeMultiplier = 1.2f;
                    break;
                case ArmorType.Plate:
                    armorTypeMultiplier = 1.5f;
                    break;
                case ArmorType.Robe:
                    armorTypeMultiplier = 0.5f;
                    break;
            }

            // 장비 슬롯별 배율
            float slotMultiplier = 1.0f;
            switch (slot) {
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
                case EquipmentSlot.Waist:
                    slotMultiplier = 0.8f;
                    break;
                case EquipmentSlot.Back:
                    slotMultiplier = 0.6f;
                    break;
                default:
                    slotMultiplier = 0.5f;
                    break;
            }

            // 레어리티 보너스
            float rarityMultiplier = 1 + ((int)rarity * 0.2f);

            return baseValue * armorTypeMultiplier * slotMultiplier * rarityMultiplier;
        }

        // 방어구 마법 방어력 계산
        private static float CalculateBaseMagicDefense(ArmorType type, EquipmentSlot slot, int itemLevel, ItemRarity rarity) {
            float baseValue = itemLevel * 0.7f;

            // 방어구 타입별 배율 (마법 방어력은 반대 경향)
            float armorTypeMultiplier = 1.0f;
            switch (type) {
                case ArmorType.Cloth:
                    armorTypeMultiplier = 1.3f;
                    break;
                case ArmorType.Robe:
                    armorTypeMultiplier = 1.5f;
                    break;
                case ArmorType.Leather:
                    armorTypeMultiplier = 1.0f;
                    break;
                case ArmorType.Mail:
                    armorTypeMultiplier = 0.8f;
                    break;
                case ArmorType.Plate:
                    armorTypeMultiplier = 0.6f;
                    break;
            }

            // 장비 슬롯별 배율은 물리 방어력과 동일
            float slotMultiplier = 1.0f;
            switch (slot) {
                case EquipmentSlot.Chest:
                    slotMultiplier = 1.5f;
                    break;
                case EquipmentSlot.Legs:
                    slotMultiplier = 1.2f;
                    break;
                case EquipmentSlot.Head:
                    slotMultiplier = 1.0f;
                    break;
                // ... 다른 슬롯들
            }

            // 레어리티 보너스
            float rarityMultiplier = 1 + ((int)rarity * 0.2f);

            return baseValue * armorTypeMultiplier * slotMultiplier * rarityMultiplier;
        }

        // 방어구 타입별 특성 설정
        private static void SetArmorTypeStats(ArmorItem armor, ArmorType type) {
            switch (type) {
                case ArmorType.Cloth:
                    armor.movementPenalty = 0f;
                    break;
                case ArmorType.Leather:
                    armor.movementPenalty = 0.02f;
                    break;
                case ArmorType.Mail:
                    armor.movementPenalty = 0.05f;
                    break;
                case ArmorType.Plate:
                    armor.movementPenalty = 0.1f;
                    break;
                case ArmorType.Robe:
                    armor.movementPenalty = 0f;
                    break;
            }
        }

        // 레어리티별 랜덤 스탯 개수
        private static int GetRandomStatCount(ItemRarity rarity) {
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

        // 랜덤 무기 스탯 추가
        private static void AddRandomWeaponStats(WeaponItem weapon, ItemRarity rarity) {
            // 무기 타입에 따른 기본 스탯 추가
            switch (weapon.weaponType) {
                case WeaponType.Sword:
                case WeaponType.Axe:
                case WeaponType.Mace:
                case WeaponType.Spear:
                    AddStatToItem(weapon, StatType.Strength, itemValueByRarity(5, rarity));
                    break;

                case WeaponType.Dagger:
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    AddStatToItem(weapon, StatType.Dexterity, itemValueByRarity(5, rarity));
                    break;

                case WeaponType.Staff:
                case WeaponType.Wand:
                    AddStatToItem(weapon, StatType.Intelligence, itemValueByRarity(5, rarity));
                    break;

                case WeaponType.Shield:
                    AddStatToItem(weapon, StatType.PhysicalDefense, itemValueByRarity(10, rarity));
                    break;
            }

            // 추가 랜덤 스탯
            int statCount = GetRandomStatCount(rarity);
            for (int i = 0; i < statCount; i++) {
                AddRandomStat(weapon, rarity);
            }

            // 레어 이상은 랜덤 속성 추가 가능성
            if (rarity >= ItemRarity.Rare && UnityEngine.Random.value < 0.3f) {
                ElementType[] elements = { ElementType.Fire, ElementType.Ice, ElementType.Lightning, ElementType.Earth };
                weapon.elementType = elements[UnityEngine.Random.Range(0, elements.Length)];
                weapon.elementalDamage = weapon.baseDamage * (0.1f + ((int)rarity - 2) * 0.1f); // 레어: 10%, 에픽: 20%, 레전더리: 30%
            }
        }

        // 랜덤 방어구 스탯 추가
        private static void AddRandomArmorStats(ArmorItem armor, ItemRarity rarity) {
            // 방어구 타입에 따른 기본 스탯 추가
            switch (armor.armorType) {
                case ArmorType.Plate:
                    AddStatToItem(armor, StatType.Strength, itemValueByRarity(3, rarity));
                    AddStatToItem(armor, StatType.Vitality, itemValueByRarity(5, rarity));
                    break;

                case ArmorType.Mail:
                    AddStatToItem(armor, StatType.Strength, itemValueByRarity(2, rarity));
                    AddStatToItem(armor, StatType.Vitality, itemValueByRarity(4, rarity));
                    break;

                case ArmorType.Leather:
                    AddStatToItem(armor, StatType.Dexterity, itemValueByRarity(4, rarity));
                    AddStatToItem(armor, StatType.Vitality, itemValueByRarity(2, rarity));
                    break;

                case ArmorType.Cloth:
                    AddStatToItem(armor, StatType.Intelligence, itemValueByRarity(4, rarity));
                    AddStatToItem(armor, StatType.Wisdom, itemValueByRarity(3, rarity));
                    break;

                case ArmorType.Robe:
                    AddStatToItem(armor, StatType.Intelligence, itemValueByRarity(5, rarity));
                    AddStatToItem(armor, StatType.Wisdom, itemValueByRarity(4, rarity));
                    break;
            }

            // 추가 랜덤 스탯
            int statCount = GetRandomStatCount(rarity) - 2; // 기본 스탯 2개 제외
            for (int i = 0; i < statCount; i++) {
                AddRandomStat(armor, rarity);
            }

            // 에픽 이상은 세트 효과 가능성
            if (rarity >= ItemRarity.Epic && UnityEngine.Random.value < 0.4f) {
                armor.hasSetBonus = true;
                armor.setName = $"{armor.armorType} of the {GetRandomSetName()}";
            }
        }

        // 장비에 스탯 추가
        private static void AddStatToItem(EquipmentItem item, StatType statType, float value) {
            EquipmentItem.ItemStat stat = new EquipmentItem.ItemStat {
                statType = statType,
                value = value,
                type = StatModifierType.Flat
            };

            item.stats.Add(stat);
        }

        // 랜덤 스탯 추가
        private static void AddRandomStat(EquipmentItem item, ItemRarity rarity) {
            // 추가 가능 스탯 목록
            StatType[] possibleStats = {
                StatType.Strength, StatType.Dexterity, StatType.Intelligence,
                StatType.Vitality, StatType.Wisdom, StatType.Luck,
                StatType.CriticalChance, StatType.CriticalDamage,
                StatType.AttackSpeed, StatType.MovementSpeed,
                StatType.PhysicalAttack, StatType.MagicAttack,
                StatType.PhysicalDefense, StatType.MagicDefense
            };

            StatType statType = possibleStats[UnityEngine.Random.Range(0, possibleStats.Length)];

            // 스탯 타입에 따라 다른 기본값과 타입
            float value;
            StatModifierType modType;

            switch (statType) {
                case StatType.CriticalChance:
                    value = UnityEngine.Random.Range(0.01f, 0.03f) * ((int)rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.CriticalDamage:
                    value = UnityEngine.Random.Range(0.05f, 0.1f) * ((int)rarity + 1);
                    modType = StatModifierType.Flat;
                    break;

                case StatType.AttackSpeed:
                case StatType.MovementSpeed:
                    value = UnityEngine.Random.Range(0.02f, 0.05f) * ((int)rarity + 1);
                    modType = StatModifierType.PercentAdditive;
                    break;

                default:
                    value = itemValueByRarity(UnityEngine.Random.Range(2, 6), rarity);
                    modType = StatModifierType.Flat;
                    break;
            }

            AddStatToItem(item, statType, value);
        }

        // 특수 효과 추가
        private static void AddRandomSpecialEffect(EquipmentItem item, ItemRarity rarity) {
            // 특수 효과 목록
            string[] effectNames = {
                "불꽃 폭발", "얼음 충격", "번개 쇼크", "독성 분출",
                "치유의 빛", "생명력 흡수", "마나 회복", "관통 타격"
            };

            // 발동 확률은 레어리티에 따라 증가
            float chance = 0.05f + ((int)rarity - 2) * 0.05f; // 레어: 5%, 에픽: 10%, 레전더리: 15%

            // 특수 효과 생성
            EquipmentItem.SpecialEffect effect = new EquipmentItem.SpecialEffect {
                effectName = effectNames[UnityEngine.Random.Range(0, effectNames.Length)],
                effectDescription = $"공격 시 {chance * 100}% 확률로 추가 효과 발동",
                activationChance = chance,
                visualEffect = null, // 실제 구현에서는 적절한 이펙트 프리팹 지정
                soundEffect = null   // 실제 구현에서는 적절한 사운드 지정
            };

            item.specialEffects.Add(effect);
        }

        // 아이템 레어리티별 스탯값 계산
        private static float itemValueByRarity(float baseValue, ItemRarity rarity) {
            return baseValue * (1 + ((int)rarity * 0.3f));
        }

        // 랜덤 세트 이름 생성
        private static string GetRandomSetName() {
            string[] prefixes = { "Dragon", "Phoenix", "Celestial", "Ancient", "Eternal", "Fiery", "Frozen", "Arcane" };
            string[] suffixes = { "Guardian", "Warlord", "Knight", "Conqueror", "Mage", "Sage", "Champion", "Destroyer" };

            return
                $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {suffixes[UnityEngine.Random.Range(0, suffixes.Length)]}";
        }

        #endregion
    }
}