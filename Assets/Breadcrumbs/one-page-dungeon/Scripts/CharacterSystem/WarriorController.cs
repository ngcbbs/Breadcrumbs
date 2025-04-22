using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class WarriorController : ClassController {
        private float rageMeter = 0f;
        private float maxRage = 100f;
        private float rageDecayRate = 5f;    // 초당 분노 감소량
        private float rageBuildupRate = 10f; // 피해량 비례 분노 증가 계수

        public float RageMeter => rageMeter;
        public float MaxRage => maxRage;

        public WarriorController(PlayerCharacter character) : base(character) { }

        public override void Initialize() {
            // 전사 특수 패시브 적용
            float strengthBonus = character.Level * 0.2f;
            StatModifier strengthMod = new StatModifier(strengthBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.Strength, strengthMod);

            // 전사 특화 방어력 보너스
            float defenseBonus = character.Level * 0.5f;
            StatModifier defenseMod = new StatModifier(defenseBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.PhysicalDefense, defenseMod);
        }

        public override void Update(float deltaTime) {
            // 분노 자연 감소
            if (rageMeter > 0) {
                rageMeter = Mathf.Max(0, rageMeter - rageDecayRate * deltaTime);
            }
        }

        // 데미지 받았을 때 분노 축적
        public void BuildRage(float damageTaken) {
            float rageGain = damageTaken * rageBuildupRate * 0.1f;
            rageMeter = Mathf.Min(maxRage, rageMeter + rageGain);
        }

        public override void ActivateClassSpecial() {
            // 베르세르크 - 분노 게이지에 비례한 공격력 증가 및 방어력 감소
            if (rageMeter >= 25f) // 최소 25의 분노 필요
            {
                float ragePercentage = rageMeter / maxRage;
                float attackBonus = 0.5f * ragePercentage;     // 최대 50% 증가
                float defensePenalty = 0.25f * ragePercentage; // 최대 25% 감소

                // 버프 적용 로직
                BuffSystem.ApplyTemporaryBuff(character, StatType.PhysicalAttack, attackBonus, StatModifierType.PercentAdditive,
                    10f); // 10초 지속
                BuffSystem.ApplyTemporaryBuff(character, StatType.PhysicalDefense, -defensePenalty,
                    StatModifierType.PercentAdditive, 10f);

                // 분노 소모
                rageMeter *= 0.5f; // 50% 분노 소모

                Debug.Log($"Berserker activated! Attack +{attackBonus * 100}%, Defense -{defensePenalty * 100}%");
            }
        }

        public override void OnLevelUp() {
            // 레벨업 시 분노 최대치 증가
            maxRage += 5f;

            // 추가 근력 보너스
            stats.AddBonus(StatType.Strength, 2f);
            stats.AddBonus(StatType.Vitality, 1.5f);
        }
    }
}