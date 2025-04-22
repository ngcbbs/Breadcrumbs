using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class Skill {
        public SkillData data;
        public int skillLevel = 1;
        public float remainingCooldown = 0f;
        public int remainingCharges;

        private PlayerCharacter owner;

        public Skill(SkillData data, PlayerCharacter owner) {
            this.data = data;
            this.owner = owner;

            // 초기 차지 설정
            remainingCharges = data.chargeCount > 0 ? data.chargeCount : int.MaxValue;
        }

        // 스킬 사용 가능 여부 확인
        public bool CanUse() {
            return remainingCooldown <= 0 &&
                   remainingCharges > 0 &&
                   owner.Stats.CurrentMana >= data.CalculateManaCost(skillLevel);
        }

        // 스킬 사용
        public bool UseSkill(Transform target = null) {
            if (!CanUse())
                return false;

            // 마나 소모
            owner.Stats.CurrentMana -= data.CalculateManaCost(skillLevel);

            // 쿨다운 설정
            remainingCooldown = data.cooldown;

            // 차지 소모
            if (data.chargeCount > 0) {
                remainingCharges--;
            }

            // 스킬 효과 실행
            ExecuteSkillEffect(target);

            return true;
        }

        // 스킬 업데이트
        public void Update(float deltaTime) {
            // 쿨다운 감소
            if (remainingCooldown > 0) {
                remainingCooldown -= deltaTime;
            }
        }

        // 스킬 레벨업
        public void LevelUp() {
            skillLevel++;
        }

        // 스킬 효과 실행
        private void ExecuteSkillEffect(Transform target) {
            // 실제 게임에서는 이 부분을 확장하여 구현
            // 예시로 간단하게 구현

            float power = data.CalculatePower(skillLevel);
            Debug.Log($"Skill used: {data.skillName}, Power: {power}");

            // 스킬 타입에 따른 처리
            switch (data.skillType) {
                case SkillType.Active:
                    // 액티브 스킬 효과 (데미지, 치유 등)
                    if (data.elementType != ElementType.None) {
                        Debug.Log($"Elemental effect: {data.elementType}");
                    }

                    // 타겟 타입에 따른 처리
                    switch (data.targetType) {
                        case TargetType.SingleEnemy:
                            if (target != null) {
                                // 단일 적에게 데미지
                                Debug.Log($"Dealing {power} damage to {target.name}");
                            }

                            break;

                        case TargetType.Area:
                            // 범위 공격 처리
                            float range = data.baseRange;
                            Debug.Log($"Area effect with range {range} and power {power}");
                            break;

                        case TargetType.Self:
                            // 자기 자신에게 효과
                            Debug.Log($"Self effect with power {power}");
                            break;

                        // 기타 타겟 타입 처리
                    }

                    break;

                case SkillType.Buff:
                    // 버프 적용
                    float duration = 10f; // 예시 지속시간
                    Debug.Log($"Applying buff for {duration} seconds");
                    break;

                case SkillType.Toggle:
                    // 토글 스킬 처리
                    Debug.Log("Toggle skill effect");
                    break;

                // 기타 스킬 타입 처리
            }

            // 시각 효과 및 사운드 처리
            if (data.castEffect != null) {
                // 이펙트 생성
                // GameObject.Instantiate(data.castEffect, owner.transform.position, Quaternion.identity);
            }

            if (data.castSound != null) {
                // 사운드 재생
                // AudioSource.PlayClipAtPoint(data.castSound, owner.transform.position);
            }

            // 추가 효과 처리
            foreach (var effect in data.additionalEffects) {
                if (UnityEngine.Random.value < effect.chance) {
                    Debug.Log(
                        $"Additional effect triggered: {effect.effectName}, Power: {effect.power}, Duration: {effect.duration}s");
                }
            }
        }
    }
}