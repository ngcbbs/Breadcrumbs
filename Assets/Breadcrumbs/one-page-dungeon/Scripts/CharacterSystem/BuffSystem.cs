using System;
using System.Collections.Generic;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    // 향상된 BuffSystem 클래스 - 모든 버프 관련 기능 통합
    public static class BuffSystem {
        /// <summary>
        /// 버프 매니저 참조 가져오기
        /// </summary>
        private static IBuffManager GetBuffManager()
        {
            return ServiceLocator.GetService<IBuffManager>();
        }
        
        #region 기본 버프 적용 메서드

        // 임시 버프 적용
        public static ActiveBuff ApplyTemporaryBuff(PlayerCharacter target, StatType statType, float value,
            StatModifierType modType, float duration) {
            // 임시 BuffData 생성
            BuffData buffData = new BuffData {
                buffId = $"TempBuff_{statType}_{modType}",
                buffName = $"Temporary {statType} Modifier",
                description = $"Temporarily modifies {statType} by {value:+0.##;-0.##}",
                duration = duration,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            // 스탯 수정자 추가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = statType,
                value = value,
                type = modType
            });

            // 버프 매니저를 통해 적용
            return GetBuffManager().ApplyBuff(target, buffData, "TempBuff");
        }

        // 영구 버프 적용
        public static void ApplyPermanentBuff(PlayerCharacter target, StatType statType, float value, StatModifierType modType) {
            string source = "PermanentBuff";
            StatModifier modifier = new StatModifier(value, modType, source);
            target.Stats.AddModifier(statType, modifier);

            Debug.Log($"Applied permanent buff to {statType}: {value:+0.##;-0.##}");
        }

        // 디버프 적용
        public static ActiveBuff ApplyDebuff(PlayerCharacter target, StatType statType, float value, StatModifierType modType,
            float duration) {
            // 음수 값으로 변환하여 디버프로 적용
            float debuffValue = value < 0 ? value : -value;

            // 임시 BuffData 생성
            BuffData buffData = new BuffData {
                buffId = $"Debuff_{statType}_{modType}",
                buffName = $"Debuff {statType}",
                description = $"Reduces {statType} by {Mathf.Abs(debuffValue):0.##}",
                duration = duration,
                isDebuff = true,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            // 스탯 수정자 추가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = statType,
                value = debuffValue,
                type = modType
            });

            // 버프 매니저를 통해 적용
            return GetBuffManager().ApplyBuff(target, buffData, "Debuff");
        }

        #endregion

        #region 지속 효과 버프

        // 지속 치유 버프 적용
        public static ActiveBuff ApplyHealOverTime(PlayerCharacter target, float amountPerTick, float tickInterval,
            float duration) {
            // 임시 BuffData 생성
            BuffData buffData = new BuffData {
                buffId = "HealOverTime",
                buffName = "Healing",
                description = $"Heals {amountPerTick} health every {tickInterval} seconds",
                duration = duration,
                tickInterval = tickInterval,
                isDebuff = false,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            // 틱 효과 설정
            buffData.onTickEffect = (character, buff) => {
                float healAmount = amountPerTick;

                // 회복량 계산 (힐링 증가 효과 등 적용 가능)
                float wisdomBonus = 1f + (character.Stats.GetStat(StatType.Wisdom) * 0.01f);
                healAmount *= wisdomBonus;

                // 스택 수에 비례한 추가 회복
                healAmount *= buff.StackCount;

                character.Stats.CurrentHealth += healAmount;
                Debug.Log($"Heal Tick: +{healAmount:0.##} HP");

                // 힐링 시각/사운드 효과 (게임 내 구현하는 경우)
                // EffectManager.Instance.PlayEffect("HealEffect", character.transform.position);
            };

            // 버프 매니저를 통해 적용
            return GetBuffManager().ApplyBuff(target, buffData, "HealBuff");
        }

        // 지속 마나 회복 버프 적용
        public static ActiveBuff ApplyManaRegenBuff(PlayerCharacter target, float amountPerTick, float tickInterval,
            float duration) {
            BuffData buffData = new BuffData {
                buffId = "ManaRegenBuff",
                buffName = "Mana Regeneration",
                description = $"Restores {amountPerTick} mana every {tickInterval} seconds",
                duration = duration,
                tickInterval = tickInterval,
                isDebuff = false,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            buffData.onTickEffect = (character, buff) => {
                float manaAmount = amountPerTick;

                // 지능에 따른 보너스
                float intBonus = 1f + (character.Stats.GetStat(StatType.Intelligence) * 0.01f);
                manaAmount *= intBonus;

                character.Stats.CurrentMana += manaAmount;
                Debug.Log($"Mana Regen Tick: +{manaAmount:0.##} MP");
            };

            return GetBuffManager().ApplyBuff(target, buffData, "ManaRegenBuff");
        }

        // 지속 피해 디버프 적용
        public static ActiveBuff ApplyDamageOverTime(PlayerCharacter target, float amountPerTick, ElementType elementType,
            float tickInterval, float duration) {
            // 임시 BuffData 생성
            BuffData buffData = new BuffData {
                buffId = $"DamageOverTime_{elementType}",
                buffName = $"{elementType} Damage",
                description = $"Deals {amountPerTick} {elementType} damage every {tickInterval} seconds",
                duration = duration,
                tickInterval = tickInterval,
                isDebuff = true,
                stackingType = BuffStackingType.Stack,
                maxStacks = 3,
                iconSprite = null
            };

            // 틱 효과 설정
            buffData.onTickEffect = (character, buff) => {
                float damage = amountPerTick;

                // 스택에 따른 피해량 증가
                damage *= buff.StackCount;

                // 피해량 계산 (원소 저항 등 적용 가능)
                switch (elementType) {
                    case ElementType.Fire:
                        // 화염 피해는 체력 % 추가 피해
                        damage += character.Stats.GetStat(StatType.MaxHealth) * 0.01f;
                        break;
                    case ElementType.Ice:
                        // 빙결 효과 - 이동 속도 감소 추가
                        ApplyTemporaryBuff(character, StatType.MovementSpeed, -0.2f, StatModifierType.PercentAdditive, 2f);
                        break;
                    case ElementType.Lightning:
                        // 번개 피해는 약간의 확률로 추가 피해
                        if (UnityEngine.Random.value < 0.2f) {
                            damage *= 1.5f;
                            Debug.Log("Lightning critical effect triggered!");
                        }

                        break;
                    case ElementType.Earth:
                        // 대지 피해는 방어력 감소 효과 추가
                        ApplyTemporaryBuff(character, StatType.PhysicalDefense, -5f, StatModifierType.Flat, 3f);
                        break;
                    case ElementType.Shadow:
                        // 암흑 피해는 생명력 흡수 효과
                        PlayerCharacter caster = buff.Source as PlayerCharacter;
                        if (caster != null) {
                            caster.Stats.CurrentHealth += damage * 0.2f;
                            Debug.Log($"Life drain effect: +{damage * 0.2f:0.##} HP");
                        }

                        break;
                }

                character.Stats.CurrentHealth -= damage;
                Debug.Log($"{elementType} DoT Tick: -{damage:0.##} HP");

                // 원소별 시각 효과 (게임 내 구현 시)
                // EffectManager.Instance.PlayEffect($"{elementType}DamageEffect", character.transform.position);
            };

            // 버프 매니저를 통해 적용
            return GetBuffManager().ApplyBuff(target, buffData, "DamageBuff");
        }

        #endregion

        #region 복합/특수 버프

        // 복합 버프 적용 (여러 스탯 영향)
        public static ActiveBuff ApplyCompoundBuff(PlayerCharacter target, string buffName,
            Dictionary<StatType, float> statModifiers, float duration) {
            // 임시 BuffData 생성
            BuffData buffData = new BuffData {
                buffId = $"CompoundBuff_{buffName}",
                buffName = buffName,
                description = $"Enhances multiple stats for {duration} seconds",
                duration = duration,
                isDebuff = false,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            // 스탯 수정자 추가
            foreach (var statMod in statModifiers) {
                buffData.statModifiers.Add(new BuffStatModifier {
                    statType = statMod.Key,
                    value = statMod.Value,
                    type = StatModifierType.Flat // 기본값으로 설정, 필요에 따라 수정 가능
                });
            }

            // 버프 매니저를 통해 적용
            return GetBuffManager().ApplyBuff(target, buffData, buffName);
        }

        // 직업별 특수 버프 - 전사용 분노 버프
        public static ActiveBuff ApplyBerserkerRage(PlayerCharacter target, float powerBonus, float defensePenalty,
            float duration) {
            BuffData buffData = new BuffData {
                buffId = "BerserkerRage",
                buffName = "Berserker Rage",
                description = $"Increases attack by {powerBonus * 100}% but reduces defense by {defensePenalty * 100}%",
                duration = duration,
                isDebuff = false,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            // 공격력 증가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = StatType.PhysicalAttack,
                value = powerBonus,
                type = StatModifierType.PercentAdditive
            });

            // 방어력 감소
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = StatType.PhysicalDefense,
                value = -defensePenalty,
                type = StatModifierType.PercentAdditive
            });

            // 버프 적용 효과
            buffData.onApplyEffect = (character, buff) => {
                Debug.Log("Berserker Rage activated! Power surges through your body!");
                // 분노 시각 효과 및 사운드 추가
            };

            // 버프 종료 효과
            buffData.onRemoveEffect = (character, buff) => {
                Debug.Log("Berserker Rage fades. Your muscles relax.");
                // 추가 효과 구현 가능
            };

            return GetBuffManager().ApplyBuff(target, buffData, "ClassBuff");
        }

        // 직업별 특수 버프 - 마법사용 원소 증폭
        public static ActiveBuff ApplyElementalAmplification(PlayerCharacter target, ElementType elementType, float amplification,
            float duration) {
            BuffData buffData = new BuffData {
                buffId = $"ElementalAmp_{elementType}",
                buffName = $"{elementType} Amplification",
                description = $"Increases {elementType} damage by {amplification * 100}%",
                duration = duration,
                isDebuff = false,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            // 마법 공격력 증가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = StatType.MagicAttack,
                value = amplification * 0.5f, // 전체 마법 공격력에 작은 보너스
                type = StatModifierType.PercentAdditive
            });

            // 버프 적용 효과
            buffData.onApplyEffect = (character, buff) => {
                Debug.Log($"Your {elementType} magic resonates with increased power!");

                // 마법사 컨트롤러 참조 및 원소 마스터리 임시 증가
                if (character.ClassController is MageController mage) {
                    mage.AddElementalMastery(elementType);
                }
            };

            return GetBuffManager().ApplyBuff(target, buffData, "ClassBuff");
        }

        // 직업별 특수 버프 - 도적용 은신
        public static ActiveBuff ApplyStealth(PlayerCharacter target, float duration) {
            BuffData buffData = new BuffData {
                buffId = "Stealth",
                buffName = "Stealth",
                description = "Enters stealth, increasing critical chance and reducing enemy detection",
                duration = duration,
                isDebuff = false,
                stackingType = BuffStackingType.None,
                iconSprite = null
            };

            // 치명타 확률 증가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = StatType.CriticalChance,
                value = 0.2f, // +20% 치명타 확률
                type = StatModifierType.Flat
            });

            // 치명타 데미지 증가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = StatType.CriticalDamage,
                value = 0.5f, // +50% 치명타 데미지
                type = StatModifierType.Flat
            });

            // 버프 적용 효과
            buffData.onApplyEffect = (character, buff) => {
                Debug.Log("You fade into the shadows...");
                // 은신 시각 효과 적용

                // 도적 컨트롤러 참조 시 콤보 포인트 추가
                if (character.ClassController is RogueController rogue) {
                    rogue.AddComboPoint();
                }
            };

            // 버프 제거 효과
            buffData.onRemoveEffect = (character, buff) => { Debug.Log("You emerge from the shadows."); };

            return GetBuffManager().ApplyBuff(target, buffData, "ClassBuff");
        }

        // 직업별 특수 버프 - 성직자용 축복
        public static ActiveBuff ApplyDivineBlessing(PlayerCharacter target, float duration) {
            BuffData buffData = new BuffData {
                buffId = "DivineBlessing",
                buffName = "Divine Blessing",
                description = "A divine blessing that enhances healing and provides damage reduction",
                duration = duration,
                isDebuff = false,
                stackingType = BuffStackingType.Refresh,
                iconSprite = null
            };

            // 방어력 증가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = StatType.PhysicalDefense,
                value = 0.15f, // +15% 물리 방어력
                type = StatModifierType.PercentAdditive
            });

            // 마법 방어력 증가
            buffData.statModifiers.Add(new BuffStatModifier {
                statType = StatType.MagicDefense,
                value = 0.15f, // +15% 마법 방어력
                type = StatModifierType.PercentAdditive
            });

            // 틱 효과 설정 (매 2초마다 소량 체력 회복)
            buffData.tickInterval = 2f;
            buffData.onTickEffect = (character, buff) => {
                float healAmount = character.Stats.GetStat(StatType.MaxHealth) * 0.02f; // 최대 체력의 2%
                character.Stats.CurrentHealth += healAmount;
                Debug.Log($"Divine Blessing heals for {healAmount:0.##} HP");
            };

            return GetBuffManager().ApplyBuff(target, buffData, "ClassBuff");
        }

        #endregion

        #region 아이템 및 세트 효과 버프

        // 아이템 세트 버프 적용
        public static void ApplySetBonusBuff(PlayerCharacter target, string setName, int pieceCount) {
            // 세트별 보너스 정의
            switch (setName) {
                case "Warrior's Valor":
                    if (pieceCount >= 2) {
                        ApplyPermanentBuff(target, StatType.PhysicalDefense, 20f, StatModifierType.Flat);
                    }

                    if (pieceCount >= 4) {
                        ApplyPermanentBuff(target, StatType.PhysicalAttack, 0.1f, StatModifierType.PercentAdditive);
                    }

                    if (pieceCount >= 6) {
                        // 풀 세트 보너스: 특수 효과
                        // 구현 필요
                    }

                    break;

                case "Mage's Wisdom":
                    if (pieceCount >= 2) {
                        ApplyPermanentBuff(target, StatType.MaxMana, 50f, StatModifierType.Flat);
                    }

                    if (pieceCount >= 4) {
                        ApplyPermanentBuff(target, StatType.MagicAttack, 0.15f, StatModifierType.PercentAdditive);
                    }

                    if (pieceCount >= 6) {
                        // 풀 세트 보너스: 특수 효과
                        // 구현 필요
                    }

                    break;

                // 기타 세트 버프...
            }

            Debug.Log($"Applied {setName} set bonus for {pieceCount} pieces");
        }

        // 장비 특수 효과 프로크 버프
        public static ActiveBuff ApplyItemProcEffect(PlayerCharacter target, string effectName, float duration, object source) {
            // 프로크 효과별 처리
            switch (effectName) {
                case "불꽃 폭발":
                    return ApplyDamageOverTime(target, 10f, ElementType.Fire, 1f, duration);

                case "얼음 충격":
                    BuffData iceProc = new BuffData {
                        buffId = "IceProc",
                        buffName = "Ice Shock",
                        description = "Freezes enemies, reducing their movement and attack speed",
                        duration = duration,
                        isDebuff = true,
                        stackingType = BuffStackingType.Refresh
                    };

                    iceProc.statModifiers.Add(new BuffStatModifier {
                        statType = StatType.MovementSpeed,
                        value = -0.3f, // -30% 이동속도
                        type = StatModifierType.PercentAdditive
                    });

                    iceProc.statModifiers.Add(new BuffStatModifier {
                        statType = StatType.AttackSpeed,
                        value = -0.2f, // -20% 공격속도
                        type = StatModifierType.PercentAdditive
                    });

                    return GetBuffManager().ApplyBuff(target, iceProc, source);

                case "번개 쇼크":
                    return ApplyDamageOverTime(target, 8f, ElementType.Lightning, 0.5f, duration);

                case "생명력 흡수":
                    BuffData lifeSteal = new BuffData {
                        buffId = "LifeSteal",
                        buffName = "Life Steal",
                        description = "Steals life from enemies on hit",
                        duration = duration,
                        isDebuff = false,
                        stackingType = BuffStackingType.Refresh
                    };

                    lifeSteal.onTickEffect = (character, buff) => {
                        float healAmount = 5f;
                        character.Stats.CurrentHealth += healAmount;
                        Debug.Log($"Life Steal effect heals for {healAmount} HP");
                    };
                    lifeSteal.tickInterval = 1f;

                    return GetBuffManager().ApplyBuff(target, lifeSteal, source);

                // 기타 프로크 효과...
                default:
                    Debug.LogWarning($"Unknown proc effect: {effectName}");
                    return null;
            }
        }

        #endregion

        #region 버프 제거/관리 메서드

        // 특정 ID의 버프 제거
        public static bool RemoveBuff(PlayerCharacter target, string buffId) {
            return GetBuffManager().RemoveBuff(target, buffId);
        }

        // 모든 버프 제거
        public static void RemoveAllBuffs(PlayerCharacter target) {
            GetBuffManager().RemoveAllBuffs(target);
        }

        // 특정 소스의 모든 버프 제거
        public static void RemoveAllBuffsFromSource(PlayerCharacter target, object source) {
            GetBuffManager().RemoveAllBuffsFromSource(target, source);
        }

        // 특정 타입의 버프만 제거 (예: 디버프만 제거)
        public static void RemoveAllDebuffs(PlayerCharacter target) {
            List<ActiveBuff> buffs = GetBuffManager().GetAllBuffs(target);
            foreach (var buff in buffs) {
                if (buff.BuffData.isDebuff) {
                    GetBuffManager().RemoveBuff(target, buff.BuffData.buffId);
                }
            }
        }

        // 특정 스탯에 영향을 주는 버프만 제거
        public static void RemoveBuffsAffectingStat(PlayerCharacter target, StatType statType) {
            List<ActiveBuff> buffs = GetBuffManager().GetAllBuffs(target);
            foreach (var buff in buffs) {
                foreach (var statMod in buff.BuffData.statModifiers) {
                    if (statMod.statType == statType) {
                        GetBuffManager().RemoveBuff(target, buff.BuffData.buffId);
                        break; // 이미 해당 버프를 제거했으므로 내부 루프 종료
                    }
                }
            }
        }

        #endregion

        #region 버프 상태 확인 메서드

        // 특정 버프가 적용 중인지 확인
        public static bool HasBuff(PlayerCharacter target, string buffId) {
            return GetBuffManager().GetBuffById(target, buffId) != null;
        }

        // 특정 디버프가 적용 중인지 확인
        public static bool HasDebuff(PlayerCharacter target) {
            List<ActiveBuff> buffs = GetBuffManager().GetAllBuffs(target);
            foreach (var buff in buffs) {
                if (buff.BuffData.isDebuff) {
                    return true;
                }
            }

            return false;
        }

        // 특정 요소 타입의 디버프가 있는지 확인
        public static bool HasElementalDebuff(PlayerCharacter target, ElementType elementType) {
            List<ActiveBuff> buffs = GetBuffManager().GetAllBuffs(target);
            foreach (var buff in buffs) {
                if (buff.BuffData.isDebuff &&
                    buff.BuffData.buffId.Contains(elementType.ToString())) {
                    return true;
                }
            }

            return false;
        }

        // 버프 남은 시간 확인
        public static float GetBuffRemainingTime(PlayerCharacter target, string buffId) {
            ActiveBuff buff = GetBuffManager().GetBuffById(target, buffId);
            return buff != null ? buff.RemainingTime : 0f;
        }

        // 버프의 현재 스택 수 확인
        public static int GetBuffStackCount(PlayerCharacter target, string buffId) {
            ActiveBuff buff = GetBuffManager().GetBuffById(target, buffId);
            return buff != null ? buff.StackCount : 0;
        }

        #endregion

        #region UI 관련 메서드 (구현 필요)

        // 버프 아이콘 스프라이트 가져오기
        public static Sprite GetBuffIcon(string buffId) {
            // 기본적인 버프 아이콘 관리 구현
            // 실제 구현에서는 아이콘 매니저를 통해 처리
            return null;
        }

        // 모든 활성 버프 효과 정보 가져오기 (UI 표시용)
        public static List<BuffDisplayInfo> GetActiveBuffsInfo(PlayerCharacter target) {
            List<BuffDisplayInfo> result = new List<BuffDisplayInfo>();
            List<ActiveBuff> buffs = GetBuffManager().GetAllBuffs(target);

            foreach (var buff in buffs) {
                result.Add(new BuffDisplayInfo {
                    Name = buff.BuffData.buffName,
                    Description = buff.BuffData.description,
                    Icon = buff.BuffData.iconSprite,
                    RemainingTime = buff.RemainingTime,
                    IsDebuff = buff.BuffData.isDebuff,
                    StackCount = buff.StackCount
                });
            }

            return result;
        }

        // UI 표시용 버프 정보 클래스
        public class BuffDisplayInfo {
            public string Name;
            public string Description;
            public Sprite Icon;
            public float RemainingTime;
            public bool IsDebuff;
            public int StackCount;
        }

        #endregion
    }
}