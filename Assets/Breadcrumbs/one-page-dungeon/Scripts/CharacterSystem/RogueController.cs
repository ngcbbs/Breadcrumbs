using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class RogueController : ClassController {
        private float comboPoints = 0;
        private float maxComboPoints = 5;
        private float comboDecayTime = 6f; // 콤보 유지 시간(초)
        private float currentComboTimer = 0f;

        public float ComboPoints => comboPoints;
        public float MaxComboPoints => maxComboPoints;

        public RogueController(PlayerCharacter character) : base(character) { }

        public override void Initialize() {
            // 도적 특수 패시브 적용
            float dexBonus = character.Level * 0.3f;
            StatModifier dexMod = new StatModifier(dexBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.Dexterity, dexMod);

            // 도적 크리티컬 보너스
            float critBonus = character.Level * 0.2f;
            StatModifier critMod = new StatModifier(critBonus / 100f, StatModifierType.Flat, this); // % to decimal
            stats.AddModifier(StatType.CriticalChance, critMod);

            // 회피율 보너스
            float evasionBonus = character.Level * 0.1f;
            StatModifier evasionMod = new StatModifier(evasionBonus / 100f, StatModifierType.Flat, this);
            stats.AddModifier(StatType.Evasion, evasionMod);
        }

        public override void Update(float deltaTime) {
            // 콤보 타이머 관리
            if (comboPoints > 0 && currentComboTimer > 0) {
                currentComboTimer -= deltaTime;
                if (currentComboTimer <= 0) {
                    ResetCombo();
                }
            }
        }

        // 콤보 포인트 추가
        public void AddComboPoint() {
            if (comboPoints < maxComboPoints) {
                comboPoints += 1;
                currentComboTimer = comboDecayTime;
                Debug.Log($"Combo point added: {comboPoints}/{maxComboPoints}");
            }
        }

        // 콤보 초기화
        public void ResetCombo() {
            comboPoints = 0;
            currentComboTimer = 0;
            Debug.Log("Combo reset");
        }

        // 피니셔 데미지 계산
        public float CalculateFinisherDamage(float baseDamage) {
            return baseDamage * (1 + (comboPoints * 0.2f));
        }

        public override void ActivateClassSpecial() {
            // 암살 - 콤보 포인트에 비례한 치명적 일격
            if (comboPoints >= 1) {
                float damageMultiplier = 1 + (comboPoints * 0.3f); // 최대 2.5배 데미지

                // 피해량 계산
                float baseDamage = stats.GetStat(StatType.PhysicalAttack);
                float totalDamage = baseDamage * damageMultiplier;

                Debug.Log($"Assassination activated! Damage: {totalDamage} (x{damageMultiplier})");

                // 여기에 실제 공격 로직 구현

                // 콤보 초기화
                ResetCombo();
            } else {
                Debug.Log("Not enough combo points for Assassination");
            }
        }

        public override void OnLevelUp() {
            // 도적 레벨업 시 특수 보너스
            stats.AddBonus(StatType.Dexterity, 2.5f);
            stats.AddBonus(StatType.Luck, 1.5f);

            // 5레벨마다 최대 콤보 포인트 증가 (최대 8)
            if (character.Level % 5 == 0 && maxComboPoints < 8) {
                maxComboPoints += 1;
                Debug.Log($"Max combo points increased to {maxComboPoints}");
            }

            // 콤보 유지 시간 증가
            comboDecayTime += 0.2f;
        }
    }
}