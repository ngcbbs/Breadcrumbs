using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class Stat {
        [SerializeField]
        private float baseValue; // 기본값
        [SerializeField]
        private float bonusValue;                                        // 장비나 버프로 인한 보너스
        private List<StatModifier> modifiers = new List<StatModifier>(); // 수정자들

        // 프로퍼티
        public float BaseValue {
            get => baseValue;
            set => baseValue = value;
        }
        public float BonusValue {
            get => bonusValue;
            set => bonusValue = value;
        }

        // 최종 값 계산 (각 수정자 적용)
        public float Value {
            get {
                float finalValue = baseValue + bonusValue;
                float percentAdditive = 0;
                float percentMultiplicative = 1;

                // 수정자 적용 (순서대로)
                foreach (StatModifier mod in modifiers) {
                    switch (mod.Type) {
                        case StatModifierType.Flat:
                            finalValue += mod.Value;
                            break;
                        case StatModifierType.PercentAdditive:
                            percentAdditive += mod.Value;
                            break;
                        case StatModifierType.PercentMultiplicative:
                            percentMultiplicative *= (1 + mod.Value);
                            break;
                    }
                }

                // 퍼센트 가산 보너스 적용
                finalValue *= (1 + percentAdditive);

                // 퍼센트 승수 보너스 적용
                finalValue *= percentMultiplicative;

                return finalValue;
            }
        }

        // 생성자
        public Stat(float baseValue = 0) {
            this.baseValue = baseValue;
            this.bonusValue = 0;
        }

        // 수정자 추가
        public void AddModifier(StatModifier modifier) {
            modifiers.Add(modifier);
            // 우선순위로 정렬
            modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        // 수정자 제거
        public bool RemoveModifier(StatModifier modifier) {
            return modifiers.Remove(modifier);
        }

        // 특정 소스의 모든 수정자 제거
        public bool RemoveAllModifiersFromSource(object source) {
            bool removed = false;

            for (int i = modifiers.Count - 1; i >= 0; i--) {
                if (modifiers[i].Source == source) {
                    modifiers.RemoveAt(i);
                    removed = true;
                }
            }

            return removed;
        }

        // 모든 수정자 초기화
        public void ClearModifiers() {
            modifiers.Clear();
        }
    }
}