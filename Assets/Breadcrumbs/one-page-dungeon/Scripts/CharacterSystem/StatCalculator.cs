namespace Breadcrumbs.CharacterSystem {
    public static class StatCalculator {
        // 스탯 간 관계를 정의하여 파생 스탯 계산
        public static void CalculateDerivedStats(CharacterStats stats, ClassType classType) {
            // 기본 파생 스탯 계산
            CalculateHealth(stats, classType);
            CalculateMana(stats, classType);
            CalculateAttackPower(stats, classType);
            CalculateDefense(stats, classType);
            CalculateSecondaryStats(stats, classType);
        }

        // 최대 체력 계산
        private static void CalculateHealth(CharacterStats stats, ClassType classType) {
            float vitality = stats.GetStat(StatType.Vitality);
            float strength = stats.GetStat(StatType.Strength);
            float baseHealth = 100;
            float healthPerLevel = 10;

            // 클래스별 추가 보정
            float classMultiplier = 1.0f;
            switch (classType) {
                case ClassType.Warrior:
                    classMultiplier = 1.2f;
                    break;
                case ClassType.Mage:
                    classMultiplier = 0.8f;
                    break;
                case ClassType.Rogue:
                    classMultiplier = 0.9f;
                    break;
                case ClassType.Cleric:
                    classMultiplier = 1.1f;
                    break;
            }

            // 체력 = 기본값 + (비탈리티 * 10) + (근력 * 2) + (레벨 * 체력증가량) * 클래스보정
            float maxHealth = (baseHealth + (vitality * 10) + (strength * 2) + (stats.Level * healthPerLevel)) * classMultiplier;
            stats.SetBaseStat(StatType.MaxHealth, maxHealth);

            // 체력 재생 = 비탈리티 * 0.1 + 레벨 * 0.2
            stats.SetBaseStat(StatType.HealthRegen, vitality * 0.1f + stats.Level * 0.2f);
        }

        // 최대 마나 계산
        private static void CalculateMana(CharacterStats stats, ClassType classType) {
            float intelligence = stats.GetStat(StatType.Intelligence);
            float wisdom = stats.GetStat(StatType.Wisdom);
            float baseMana = 50;
            float manaPerLevel = 5;

            // 클래스별 추가 보정
            float classMultiplier = 1.0f;
            switch (classType) {
                case ClassType.Warrior:
                    classMultiplier = 0.7f;
                    break;
                case ClassType.Mage:
                    classMultiplier = 1.5f;
                    break;
                case ClassType.Rogue:
                    classMultiplier = 0.8f;
                    break;
                case ClassType.Cleric:
                    classMultiplier = 1.3f;
                    break;
            }

            // 마나 = 기본값 + (지능 * 8) + (지혜 * 4) + (레벨 * 마나증가량) * 클래스보정
            float maxMana = (baseMana + (intelligence * 8) + (wisdom * 4) + (stats.Level * manaPerLevel)) * classMultiplier;
            stats.SetBaseStat(StatType.MaxMana, maxMana);

            // 마나 재생 = 지혜 * 0.2 + 지능 * 0.1 + 레벨 * 0.1
            stats.SetBaseStat(StatType.ManaRegen, wisdom * 0.2f + intelligence * 0.1f + stats.Level * 0.1f);
        }

        // 공격력 계산
        private static void CalculateAttackPower(CharacterStats stats, ClassType classType) {
            float strength = stats.GetStat(StatType.Strength);
            float dexterity = stats.GetStat(StatType.Dexterity);
            float intelligence = stats.GetStat(StatType.Intelligence);

            // 클래스별 공격력 계산
            switch (classType) {
                case ClassType.Warrior:
                    // 물리 공격력 = 근력 * 2 + 레벨 * 2
                    stats.SetBaseStat(StatType.PhysicalAttack, strength * 2 + stats.Level * 2);
                    // 마법 공격력 = 지능 * 0.5 + 레벨 * 0.5
                    stats.SetBaseStat(StatType.MagicAttack, intelligence * 0.5f + stats.Level * 0.5f);
                    break;

                case ClassType.Mage:
                    // 물리 공격력 = 근력 * 0.5 + 레벨 * 0.5
                    stats.SetBaseStat(StatType.PhysicalAttack, strength * 0.5f + stats.Level * 0.5f);
                    // 마법 공격력 = 지능 * 2.5 + 레벨 * 2
                    stats.SetBaseStat(StatType.MagicAttack, intelligence * 2.5f + stats.Level * 2);
                    break;

                case ClassType.Rogue:
                    // 물리 공격력 = 민첩 * 1.5 + 근력 * 1 + 레벨 * 1.5
                    stats.SetBaseStat(StatType.PhysicalAttack, dexterity * 1.5f + strength * 1 + stats.Level * 1.5f);
                    // 마법 공격력 = 지능 * 1 + 레벨 * 0.7
                    stats.SetBaseStat(StatType.MagicAttack, intelligence * 1 + stats.Level * 0.7f);
                    break;

                case ClassType.Cleric:
                    // 물리 공격력 = 근력 * 1.2 + 레벨 * 1
                    stats.SetBaseStat(StatType.PhysicalAttack, strength * 1.2f + stats.Level * 1);
                    // 마법 공격력 = 지능 * 1.8 + 레벨 * 1.5
                    stats.SetBaseStat(StatType.MagicAttack, intelligence * 1.8f + stats.Level * 1.5f);
                    break;
            }
        }

        // 방어력 계산
        private static void CalculateDefense(CharacterStats stats, ClassType classType) {
            float vitality = stats.GetStat(StatType.Vitality);
            float strength = stats.GetStat(StatType.Strength);
            float intelligence = stats.GetStat(StatType.Intelligence);
            float wisdom = stats.GetStat(StatType.Wisdom);

            // 클래스별 방어력 계산
            switch (classType) {
                case ClassType.Warrior:
                    // 물리 방어력 = 비탈리티 * 1 + 근력 * 0.5 + 레벨 * 1.5
                    stats.SetBaseStat(StatType.PhysicalDefense, vitality * 1 + strength * 0.5f + stats.Level * 1.5f);
                    // 마법 방어력 = 지혜 * 0.7 + 레벨 * 0.5
                    stats.SetBaseStat(StatType.MagicDefense, wisdom * 0.7f + stats.Level * 0.5f);
                    break;

                case ClassType.Mage:
                    // 물리 방어력 = 비탈리티 * 0.5 + 레벨 * 0.5
                    stats.SetBaseStat(StatType.PhysicalDefense, vitality * 0.5f + stats.Level * 0.5f);
                    // 마법 방어력 = 지혜 * 1 + 지능 * 0.5 + 레벨 * 1
                    stats.SetBaseStat(StatType.MagicDefense, wisdom * 1 + intelligence * 0.5f + stats.Level * 1);
                    break;

                case ClassType.Rogue:
                    // 물리 방어력 = 비탈리티 * 0.7 + 레벨 * 0.8
                    stats.SetBaseStat(StatType.PhysicalDefense, vitality * 0.7f + stats.Level * 0.8f);
                    // 마법 방어력 = 지혜 * 0.8 + 레벨 * 0.6
                    stats.SetBaseStat(StatType.MagicDefense, wisdom * 0.8f + stats.Level * 0.6f);
                    break;

                case ClassType.Cleric:
                    // 물리 방어력 = 비탈리티 * 0.8 + 근력 * 0.2 + 레벨 * 1
                    stats.SetBaseStat(StatType.PhysicalDefense, vitality * 0.8f + strength * 0.2f + stats.Level * 1);
                    // 마법 방어력 = 지혜 * 1.2 + 레벨 * 1.2
                    stats.SetBaseStat(StatType.MagicDefense, wisdom * 1.2f + stats.Level * 1.2f);
                    break;
            }
        }

        // 보조 스탯 계산
        private static void CalculateSecondaryStats(CharacterStats stats, ClassType classType) {
            float dexterity = stats.GetStat(StatType.Dexterity);
            float luck = stats.GetStat(StatType.Luck);

            // 클래스별 보조 스탯 계산
            switch (classType) {
                case ClassType.Warrior:
                    stats.SetBaseStat(StatType.AttackSpeed, 1.0f + dexterity * 0.003f);
                    stats.SetBaseStat(StatType.MovementSpeed, 5.0f + dexterity * 0.01f);
                    stats.SetBaseStat(StatType.CriticalChance, 0.05f + luck * 0.001f);
                    stats.SetBaseStat(StatType.CriticalDamage, 1.5f + luck * 0.003f);
                    stats.SetBaseStat(StatType.Accuracy, 0.9f + dexterity * 0.002f);
                    stats.SetBaseStat(StatType.Evasion, 0.05f + dexterity * 0.002f);
                    break;

                case ClassType.Mage:
                    stats.SetBaseStat(StatType.AttackSpeed, 0.8f + dexterity * 0.002f);
                    stats.SetBaseStat(StatType.MovementSpeed, 4.5f + dexterity * 0.01f);
                    stats.SetBaseStat(StatType.CriticalChance, 0.08f + luck * 0.001f);
                    stats.SetBaseStat(StatType.CriticalDamage, 1.7f + luck * 0.003f);
                    stats.SetBaseStat(StatType.Accuracy, 0.95f + dexterity * 0.001f);
                    stats.SetBaseStat(StatType.Evasion, 0.03f + dexterity * 0.001f);
                    break;

                case ClassType.Rogue:
                    stats.SetBaseStat(StatType.AttackSpeed, 1.2f + dexterity * 0.004f);
                    stats.SetBaseStat(StatType.MovementSpeed, 5.5f + dexterity * 0.015f);
                    stats.SetBaseStat(StatType.CriticalChance, 0.1f + luck * 0.002f);
                    stats.SetBaseStat(StatType.CriticalDamage, 1.8f + luck * 0.004f);
                    stats.SetBaseStat(StatType.Accuracy, 0.92f + dexterity * 0.003f);
                    stats.SetBaseStat(StatType.Evasion, 0.08f + dexterity * 0.003f);
                    break;

                case ClassType.Cleric:
                    stats.SetBaseStat(StatType.AttackSpeed, 0.9f + dexterity * 0.002f);
                    stats.SetBaseStat(StatType.MovementSpeed, 4.8f + dexterity * 0.01f);
                    stats.SetBaseStat(StatType.CriticalChance, 0.06f + luck * 0.001f);
                    stats.SetBaseStat(StatType.CriticalDamage, 1.6f + luck * 0.002f);
                    stats.SetBaseStat(StatType.Accuracy, 0.93f + dexterity * 0.001f);
                    stats.SetBaseStat(StatType.Evasion, 0.04f + dexterity * 0.001f);
                    break;
            }
        }
    }
}