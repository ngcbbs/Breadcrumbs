using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class MageController : ClassController {
        private float manaEfficiency = 0.9f;  // 마나 소모 효율 (1보다 작을수록 효율적)
        private float spellCritBonus = 0.05f; // 추가 치명타 확률
        private List<ElementType> masteredElements = new List<ElementType>();

        public MageController(PlayerCharacter character) : base(character) { }

        public override void Initialize() {
            // 마법사 특수 패시브 적용
            float intelBonus = character.Level * 0.3f;
            StatModifier intelMod = new StatModifier(intelBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.Intelligence, intelMod);

            // 마법사 마법 공격력 보너스
            float magicAttackBonus = character.Level * 0.5f;
            StatModifier magicMod = new StatModifier(magicAttackBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.MagicAttack, magicMod);

            // 초기 원소 마스터리
            masteredElements.Add(ElementType.Fire);

            // 스펠 크리티컬 추가
            StatModifier critMod = new StatModifier(spellCritBonus, StatModifierType.Flat, this);
            stats.AddModifier(StatType.CriticalChance, critMod);
        }

        public override void Update(float deltaTime) {
            // 원소 마스터리에 따른 피해량 계산 등 필요하다면 구현
        }

        // 새로운 원소 마스터리 추가
        public void AddElementalMastery(ElementType element) {
            if (!masteredElements.Contains(element)) {
                masteredElements.Add(element);
                Debug.Log($"New element mastered: {element}");

                // 원소당 특수 버프 적용
                switch (element) {
                    case ElementType.Fire:
                        BuffSystem.ApplyPermanentBuff(character, StatType.MagicAttack, 0.05f, StatModifierType.PercentAdditive);
                        break;
                    case ElementType.Ice:
                        BuffSystem.ApplyPermanentBuff(character, StatType.CriticalChance, 0.03f, StatModifierType.Flat);
                        break;
                    case ElementType.Lightning:
                        BuffSystem.ApplyPermanentBuff(character, StatType.AttackSpeed, 0.05f, StatModifierType.PercentAdditive);
                        break;
                    case ElementType.Earth:
                        BuffSystem.ApplyPermanentBuff(character, StatType.PhysicalDefense, 0.1f,
                            StatModifierType.PercentAdditive);
                        break;
                }
            }
        }

        // 마나 효율 계산 (스킬 비용 감소)
        public float CalculateManaCost(float baseCost) {
            return baseCost * manaEfficiency;
        }

        public override void ActivateClassSpecial() {
            // 아케인 폭발 - 모든 마스터한 원소의 공격 발사
            foreach (ElementType element in masteredElements) {
                float damage = stats.GetStat(StatType.MagicAttack) * 1.2f;

                // 원소별 추가 효과
                switch (element) {
                    case ElementType.Fire:
                        // 화염 공격 + 지속 데미지
                        Debug.Log($"Arcane Explosion: Fire Damage {damage}");
                        break;
                    case ElementType.Ice:
                        // 얼음 공격 + 이동속도 감소
                        Debug.Log($"Arcane Explosion: Ice Damage {damage * 0.8f}");
                        break;
                    case ElementType.Lightning:
                        // 번개 공격 + 추가 대상 점프
                        Debug.Log($"Arcane Explosion: Lightning Damage {damage * 0.9f}");
                        break;
                    case ElementType.Earth:
                        // 대지 공격 + 넉백
                        Debug.Log($"Arcane Explosion: Earth Damage {damage * 1.1f}");
                        break;
                }
            }

            // 마나 소모
            stats.CurrentMana -= 50f;
        }

        public override void OnLevelUp() {
            // 레벨업 시 마나 효율 향상
            manaEfficiency -= 0.01f;
            manaEfficiency = Mathf.Max(0.7f, manaEfficiency); // 최대 30% 까지 감소

            // 지능 보너스 증가
            stats.AddBonus(StatType.Intelligence, 2.5f);
            stats.AddBonus(StatType.Wisdom, 1.5f);

            // 특정 레벨마다 새로운 원소 마스터리
            if (character.Level == 5)
                AddElementalMastery(ElementType.Ice);
            else if (character.Level == 10)
                AddElementalMastery(ElementType.Lightning);
            else if (character.Level == 15)
                AddElementalMastery(ElementType.Earth);
        }
    }
}