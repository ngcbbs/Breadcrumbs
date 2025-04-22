using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class ClericController : ClassController {
        private float divineEnergy = 0f;
        private float maxDivineEnergy = 100f;
        private float energyBuildRate = 5f; // 초당 신성 에너지 축적량

        public float DivineEnergy => divineEnergy;
        public float MaxDivineEnergy => maxDivineEnergy;

        public ClericController(PlayerCharacter character) : base(character) { }

        public override void Initialize() {
            // 성직자 특수 패시브 적용
            float wisdomBonus = character.Level * 0.3f;
            StatModifier wisdomMod = new StatModifier(wisdomBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.Wisdom, wisdomMod);

            // 체력 재생 보너스
            float regenBonus = character.Level * 0.2f;
            StatModifier regenMod = new StatModifier(regenBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.HealthRegen, regenMod);
        }

        public override void Update(float deltaTime) {
            // 신성 에너지 축적
            if (divineEnergy < maxDivineEnergy) {
                divineEnergy = Mathf.Min(maxDivineEnergy, divineEnergy + energyBuildRate * deltaTime);
            }
        }

        // 힐링 계산
        public float CalculateHealing(float baseAmount) {
            float wisdom = stats.GetStat(StatType.Wisdom);
            float healingPower = 1 + (wisdom * 0.01f);
            return baseAmount * healingPower;
        }

        // 신성 에너지 소모
        public bool UseDivineEnergy(float amount) {
            if (divineEnergy >= amount) {
                divineEnergy -= amount;
                return true;
            }

            return false;
        }

        public override void ActivateClassSpecial() {
            // 신성한 광휘 - 주변 아군 치유 및 적 피해
            float energyCost = 50f;

            if (UseDivineEnergy(energyCost)) {
                float healAmount = CalculateHealing(stats.GetStat(StatType.MagicAttack) * 1.5f);
                float damageAmount = stats.GetStat(StatType.MagicAttack) * 0.8f;

                Debug.Log($"Divine Radiance! Healing: {healAmount}, Damage: {damageAmount}");

                // 여기에 실제 치유 및 공격 로직 구현
                // 예: 주변 아군 모두 치유, 적에게 피해
            } else {
                Debug.Log("Not enough divine energy!");
            }
        }

        public override void OnLevelUp() {
            // 성직자 레벨업 보너스
            stats.AddBonus(StatType.Wisdom, 2f);
            stats.AddBonus(StatType.Intelligence, 1.5f);
            stats.AddBonus(StatType.Vitality, 1f);

            // 신성 에너지 최대치 증가
            maxDivineEnergy += 5f;

            // 에너지 축적 속도 향상
            energyBuildRate += 0.2f;
        }
    }
}