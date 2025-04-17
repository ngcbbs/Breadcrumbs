using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class MeleeAttackBehavior : AICombatBehaviorBase {
        private float _attackTimer = 0f;
        private float _cooldownTimer = 0f;
        private bool _isAttacking = false;
        private Collider[] _hitColliders = new Collider[5];
        
        public MeleeAttackBehavior(AICombatSettings settings) : base(settings) { }
        
        public override float EvaluateSuitability(AIContextData context) {
            // 공격 중이거나 쿨다운 중이면 적합도 반환
            if (_isAttacking) return 0.95f;
            if (_cooldownTimer > 0) return 0.1f;
            
            // 타겟이 없으면 부적합
            if (context.target == null) return 0f;
            
            // 근접 공격 범위 내에 있는지 확인
            float distanceToTarget = context.distanceToTarget;
            bool inMeleeRange = distanceToTarget <= settings.meleeAttackRange;
            
            // 컨텍스트에 근접 공격 범위 정보 저장
            context.customData["inMeleeRange"] = inMeleeRange;
            context.customData["meleeAttackRange"] = settings.meleeAttackRange;
            
            // 공격 범위 내에 있으면 높은 적합도
            return inMeleeRange ? 0.9f : 0.2f;
        }
        
        public override void Execute(AIContextData context) {
            if (_isAttacking) {
                _attackTimer -= Time.deltaTime;
                
                // 공격 지속 시간이 절반 지났을 때 데미지 판정
                if (_attackTimer <= settings.meleeAttackDuration / 2 && _attackTimer > settings.meleeAttackDuration / 2 - Time.deltaTime) {
                    PerformDamage(context);
                }
                
                // 공격 종료
                if (_attackTimer <= 0) {
                    _isAttacking = false;
                    _cooldownTimer = settings.meleeAttackCooldown;
                }
            }
            else if (_cooldownTimer > 0) {
                _cooldownTimer -= Time.deltaTime;
            }
            else if (context.target != null && (bool)context.customData["inMeleeRange"]) {
                StartAttack(context);
            }
        }
        
        private void StartAttack(AIContextData context) {
            _isAttacking = true;
            _attackTimer = settings.meleeAttackDuration;
            
            // 여기서 공격 애니메이션이나 효과 재생
            Debug.Log($"{context.self.name} performs melee attack!");
            
            // 공격을 위해 타겟을 향해 회전
            Vector3 dirToTarget = (context.target.position - context.self.position).normalized;
            dirToTarget.y = 0; // Y축 회전만 고려
            
            if (dirToTarget != Vector3.zero) {
                context.self.rotation = Quaternion.LookRotation(dirToTarget);
            }
        }
        
        private void PerformDamage(AIContextData context) {
            // 공격 범위 내의 대상 검출
            Vector3 attackCenter = context.self.position + context.self.forward * (settings.meleeAttackRange * 0.5f);
            int hitCount = Physics.OverlapSphereNonAlloc(
                attackCenter,
                settings.meleeAttackRange * 0.5f,
                _hitColliders,
                settings.meleeAttackLayers
            );
            
            for (int i = 0; i < hitCount; i++) {
                if (_hitColliders[i].transform != context.self) {
                    // 대상에게 데미지 적용
                    IDamageable damageable = _hitColliders[i].GetComponent<IDamageable>();
                    if (damageable != null) {
                        damageable.TakeDamage(settings.meleeAttackDamage);
                    }
                    
                    // 넉백 적용
                    Rigidbody rb = _hitColliders[i].GetComponent<Rigidbody>();
                    if (rb != null) {
                        Vector3 knockbackDir = (_hitColliders[i].transform.position - context.self.position).normalized;
                        rb.AddForce(knockbackDir * settings.meleeAttackKnockback, ForceMode.Impulse);
                    }
                }
            }
        }
        
        public override bool IsActionInProgress() {
            return _isAttacking;
        }
        
        public override void DrawGizmos(AIContextData context) {
            if (_isAttacking) {
                // 공격 범위 시각화
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Vector3 attackCenter = context.self.position + context.self.forward * (settings.meleeAttackRange * 0.5f);
                Gizmos.DrawSphere(attackCenter, settings.meleeAttackRange * 0.5f);
            }
            else {
                // 공격 가능 범위 시각화
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.1f);
                Gizmos.DrawWireSphere(context.self.position, settings.meleeAttackRange);
            }
        }
    }
}