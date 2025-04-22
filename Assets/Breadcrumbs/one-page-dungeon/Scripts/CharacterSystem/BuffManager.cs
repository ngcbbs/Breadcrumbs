using System;
using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Core;

namespace Breadcrumbs.CharacterSystem {
    /// <summary>
    /// 버프 관리 시스템
    /// </summary>
    public class BuffManager : MonoBehaviour, IBuffManager {
        // 현재 적용 중인 모든 버프 (캐릭터 -> 버프 목록)
        private Dictionary<PlayerCharacter, List<ActiveBuff>> activeBuffs = new Dictionary<PlayerCharacter, List<ActiveBuff>>();

        private void Awake() {
            // 서비스 로케이터에 등록
            ServiceLocator.RegisterService<IBuffManager>(this);
        }

        private void OnDestroy() {
            // 필요한 정리 작업
        }

        private void Update() {
            UpdateAllBuffs();
        }

        // 모든 버프 업데이트
        private void UpdateAllBuffs() {
            foreach (var characterBuffs in activeBuffs) {
                PlayerCharacter character = characterBuffs.Key;
                List<ActiveBuff> buffs = characterBuffs.Value;

                // 제거할 버프를 저장할 리스트
                List<ActiveBuff> expiredBuffs = new List<ActiveBuff>();

                foreach (ActiveBuff buff in buffs) {
                    if (buff.IsTemporary) {
                        buff.RemainingTime -= Time.deltaTime;

                        // 주기적 효과 (틱) 처리
                        if (buff.HasTickEffect) {
                            buff.TickTimer -= Time.deltaTime;
                            if (buff.TickTimer <= 0) {
                                ApplyBuffTick(character, buff);
                                buff.TickTimer = buff.TickInterval;

                                // 이벤트 발생
                                EventManager.Trigger("Buff.Updated", new BuffAppliedEventData(character, buff));
                            }
                        }

                        // 버프 종료 조건 확인
                        if (buff.RemainingTime <= 0) {
                            expiredBuffs.Add(buff);
                        }
                    }
                }

                // 만료된 버프 제거
                foreach (ActiveBuff expiredBuff in expiredBuffs) {
                    RemoveBuffFromCharacter(character, expiredBuff.ID);
                }
            }
        }

        // 틱 효과 적용
        private void ApplyBuffTick(PlayerCharacter character, ActiveBuff buff) {
            // 예: 지속 데미지, 지속 힐링 등
            if (buff.TickEffect != null) {
                buff.TickEffect.Invoke(character, buff);
            }
        }

        // 버프 적용
        public ActiveBuff ApplyBuff(PlayerCharacter target, BuffData buffData, object source = null,
            float? customDuration = null) {
            if (target == null || buffData == null) return null;

            // 캐릭터 버프 목록 확인
            if (!activeBuffs.ContainsKey(target)) {
                activeBuffs[target] = new List<ActiveBuff>();
            }

            // 중복 버프 확인
            ActiveBuff existingBuff = activeBuffs[target].Find(b => b.BuffData.buffId == buffData.buffId);

            if (existingBuff != null) {
                if (buffData.stackingType == BuffStackingType.Refresh) {
                    // 기존 버프 갱신
                    existingBuff.RemainingTime = customDuration ?? buffData.duration;

                    // 이벤트 발생
                    EventManager.Trigger("Buff.Updated", new BuffAppliedEventData(target, existingBuff));

                    return existingBuff;
                } else if (buffData.stackingType == BuffStackingType.Stack && existingBuff.StackCount < buffData.maxStacks) {
                    // 스택 증가
                    existingBuff.StackCount++;
                    existingBuff.RemainingTime = customDuration ?? buffData.duration;

                    // 모든 스탯 수정자에 스택 효과 적용
                    ApplyBuffEffects(target, existingBuff);

                    // 이벤트 발생
                    EventManager.Trigger("Buff.Updated", new BuffAppliedEventData(target, existingBuff));

                    return existingBuff;
                } else if (buffData.stackingType == BuffStackingType.None) {
                    // 중복 불가 버프는 무시
                    return existingBuff;
                }
            }

            // 새 버프 생성
            ActiveBuff newBuff = new ActiveBuff {
                ID = Guid.NewGuid().ToString(),
                BuffData = buffData,
                RemainingTime = customDuration ?? buffData.duration,
                IsTemporary = true,
                Source = source,
                StackCount = 1,
                HasTickEffect = buffData.tickInterval > 0,
                TickInterval = buffData.tickInterval,
                TickTimer = buffData.tickInterval
            };

            // 버프 효과 적용
            ApplyBuffEffects(target, newBuff);

            // 활성 버프 목록에 추가
            activeBuffs[target].Add(newBuff);

            // 이벤트 발생
            EventManager.Trigger("Buff.Applied", new BuffAppliedEventData(target, newBuff));

            return newBuff;
        }

        // 버프 효과 적용 (스탯 수정자 등)
        private void ApplyBuffEffects(PlayerCharacter target, ActiveBuff buff) {
            BuffData buffData = buff.BuffData;
            float stackMultiplier = buff.StackCount;

            // 스탯 수정자 적용
            foreach (var statMod in buffData.statModifiers) {
                float value = statMod.value;

                // 스택에 비례해 효과 증가
                if (buffData.stackingType == BuffStackingType.Stack) {
                    value *= stackMultiplier;
                }

                StatModifier modifier = new StatModifier(value, statMod.type, buff.ID);
                target.Stats.AddModifier(statMod.statType, modifier);
            }

            // 초기 적용 효과 실행
            if (buffData.onApplyEffect != null && !buff.InitialEffectApplied) {
                buffData.onApplyEffect.Invoke(target, buff);
                buff.InitialEffectApplied = true;
            }

            // 틱 효과 설정
            if (buffData.tickInterval > 0) {
                buff.HasTickEffect = true;
                buff.TickEffect = buffData.onTickEffect;
            }
        }

        // 버프 제거
        public bool RemoveBuff(PlayerCharacter target, string buffId) {
            if (target == null || string.IsNullOrEmpty(buffId)) return false;

            if (activeBuffs.TryGetValue(target, out List<ActiveBuff> buffs)) {
                ActiveBuff buff = buffs.Find(b => b.BuffData.buffId == buffId);
                if (buff != null) {
                    return RemoveBuffFromCharacter(target, buff.ID);
                }
            }

            return false;
        }

        // 버프 제거 (내부 구현)
        private bool RemoveBuffFromCharacter(PlayerCharacter target, string buffInstanceId) {
            if (target == null || string.IsNullOrEmpty(buffInstanceId)) return false;

            if (activeBuffs.TryGetValue(target, out List<ActiveBuff> buffs)) {
                ActiveBuff buff = buffs.Find(b => b.ID == buffInstanceId);
                if (buff != null) {
                    // 버프 효과 제거
                    target.Stats.RemoveAllModifiersFromSource(buffInstanceId);

                    // 종료 효과 실행
                    if (buff.BuffData.onRemoveEffect != null) {
                        buff.BuffData.onRemoveEffect.Invoke(target, buff);
                    }

                    // 목록에서 제거
                    buffs.Remove(buff);

                    // 이벤트 발생
                    EventManager.Trigger("Buff.Removed", new BuffRemovedEventData(target, buff));

                    return true;
                }
            }

            return false;
        }

        // 특정 소스의 모든 버프 제거
        public void RemoveAllBuffsFromSource(PlayerCharacter target, object source) {
            if (target == null || source == null) return;

            if (activeBuffs.TryGetValue(target, out List<ActiveBuff> buffs)) {
                // 제거할 버프를 먼저 찾아서 리스트에 추가
                List<ActiveBuff> buffsToRemove = buffs.FindAll(b => b.Source == source);

                // 버프 제거
                foreach (var buff in buffsToRemove) {
                    RemoveBuffFromCharacter(target, buff.ID);
                }
            }
        }

        // 캐릭터의 모든 버프 제거
        public void RemoveAllBuffs(PlayerCharacter target) {
            if (target == null) return;

            if (activeBuffs.TryGetValue(target, out List<ActiveBuff> buffs)) {
                // 복사본을 만들어서 순회하며 제거
                List<ActiveBuff> allBuffs = new List<ActiveBuff>(buffs);

                foreach (var buff in allBuffs) {
                    RemoveBuffFromCharacter(target, buff.ID);
                }

                // 목록 자체를 클리어
                buffs.Clear();
            }
        }

        // 캐릭터가 갖고 있는 모든 버프 조회
        public List<ActiveBuff> GetAllBuffs(PlayerCharacter target) {
            if (target == null || !activeBuffs.TryGetValue(target, out var buff)) {
                return new List<ActiveBuff>();
            }

            return new List<ActiveBuff>(buff);
        }

        // 특정 ID의 버프 조회
        public ActiveBuff GetBuffById(PlayerCharacter target, string buffId) {
            if (target == null || !activeBuffs.TryGetValue(target, out var buff)) {
                return null;
            }

            return buff.Find(b => b.BuffData.buffId == buffId);
        }
    }
}