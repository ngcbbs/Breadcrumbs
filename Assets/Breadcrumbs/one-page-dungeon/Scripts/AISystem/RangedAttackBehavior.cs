using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class RangedAttackBehavior : AICombatBehaviorBase {
        private float _attackTimer = 0f;
        private float _cooldownTimer = 0f;
        private bool _isCharging = false;
        private bool _isFiring = false;
        
        public RangedAttackBehavior(AICombatSettings settings) : base(settings) { }
        
        public override float EvaluateSuitability(AIContextData context) {
            // 이미 공격 중이거나 쿨다운 중이면 적합도 반환
            if (_isCharging || _isFiring) return 0.95f;
            if (_cooldownTimer > 0) return 0.1f;
            
            // 타겟이 없으면 부적합
            if (context.target == null) return 0f;
            
            float distanceToTarget = context.distanceToTarget;
            
            // 근접 공격 범위보다 멀고 원거리 공격 범위 내에 있는지 확인
            bool inMeleeRange = (bool)context.customData["inMeleeRange"];
            bool inRangedRange = distanceToTarget <= settings.rangedAttackRange;
            
            // 컨텍스트에 원거리 공격 범위 정보 저장
            context.customData["inRangedRange"] = inRangedRange;
            context.customData["rangedAttackRange"] = settings.rangedAttackRange;
            
            // 시야 확인
            bool hasLineOfSight = HasLineOfSightToTarget(context);
            
            // 원거리 공격 적합도 계산
            if (inRangedRange && !inMeleeRange && hasLineOfSight) {
                // 거리가 적당하면 높은 적합도
                float optimalRangeFactor = 1f - Mathf.Abs((distanceToTarget / settings.rangedAttackRange) - 0.7f);
                return 0.85f * optimalRangeFactor;
            }
            
            return 0.1f;
        }
        
        private bool HasLineOfSightToTarget(AIContextData context) {
            if (context.target == null) return false;
            
            Vector3 dirToTarget = context.target.position - context.self.position;
            float distanceToTarget = dirToTarget.magnitude;
            
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(
                context.self.position + Vector3.up * 0.5f, // 약간 위에서 시작
                dirToTarget.normalized, 
                out hit, 
                distanceToTarget,
                ~settings.rangedAttackLayers // 타겟 레이어를 제외한 모든 것
            );
            
            // 타겟까지 장애물이 없거나, 맞은 객체가 타겟이면 시야 확보
            return !hitSomething || hit.transform == context.target;
        }
        
        public override void Execute(AIContextData context) {
            if (_isCharging) {
                _attackTimer -= Time.deltaTime;
                
                // 차징 완료 시 발사
                if (_attackTimer <= 0) {
                    FireProjectile(context);
                    _isCharging = false;
                    _isFiring = true;
                    _attackTimer = 0.2f; // 발사 후 짧은 딜레이
                }
            }
            else if (_isFiring) {
                _attackTimer -= Time.deltaTime;
                
                if (_attackTimer <= 0) {
                    _isFiring = false;
                    _cooldownTimer = settings.rangedAttackCooldown;
                }
            }
            else if (_cooldownTimer > 0) {
                _cooldownTimer -= Time.deltaTime;
            }
            else if (context.target != null && (bool)context.customData["inRangedRange"] && 
                     !(bool)context.customData["inMeleeRange"] && HasLineOfSightToTarget(context)) {
                StartCharging(context);
            }
        }
        
        private void StartCharging(AIContextData context) {
            _isCharging = true;
            _attackTimer = settings.rangedAttackChargeTime;
            
            // 여기서 차징 애니메이션이나 효과 재생
            Debug.Log($"{context.self.name} charges ranged attack!");
            
            // 공격을 위해 타겟을 향해 회전
            Vector3 dirToTarget = (context.target.position - context.self.position).normalized;
            dirToTarget.y = 0; // Y축 회전만 고려
            
            if (dirToTarget != Vector3.zero) {
                context.self.rotation = Quaternion.LookRotation(dirToTarget);
            }
        }
        
        private void FireProjectile(AIContextData context) {
            if (context.target == null || settings.projectilePrefab == null) return;
            
            // 발사 위치 결정
            Vector3 spawnPosition;
            if (settings.projectileSpawnPoint != null) {
                spawnPosition = settings.projectileSpawnPoint.position;
            } else {
                spawnPosition = context.self.position + Vector3.up * 1.5f + context.self.forward * 0.5f;
            }
            
            // 발사 방향 계산 (약간의 예측 조준 포함)
            Vector3 targetPosition = PredictTargetPosition(context);
            Vector3 direction = (targetPosition - spawnPosition).normalized;
            
            // 프로젝타일 생성
            GameObject projectile = GameObject.Instantiate(settings.projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
            
            // 프로젝타일 속성 설정
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent != null) {
                projectileComponent.Initialize(context.self, direction, settings.rangedAttackProjectileSpeed, settings.rangedAttackDamage);
            }
            else {
                // 프로젝타일 컴포넌트가 없으면 기본 Rigidbody 사용
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.linearVelocity = direction * settings.rangedAttackProjectileSpeed;
                }
                
                // 10초 후 자동 파괴
                GameObject.Destroy(projectile, 10f);
            }
            
            Debug.Log($"{context.self.name} fires projectile!");
        }
        
        private Vector3 PredictTargetPosition(AIContextData context) {
            if (context.target == null) return Vector3.zero;
            
            // 타겟이 Rigidbody를 가지고 있으면 속도 예측
            Rigidbody targetRb = context.target.GetComponent<Rigidbody>();
            if (targetRb != null && !targetRb.isKinematic) {
                Vector3 targetPosition = context.target.position;
                Vector3 targetVelocity = targetRb.linearVelocity;
                
                // 프로젝타일 도달 시간 추정
                float distance = Vector3.Distance(context.self.position, targetPosition);
                float timeToReachTarget = distance / settings.rangedAttackProjectileSpeed;
                
                // 예측 위치 계산
                return targetPosition + targetVelocity * timeToReachTarget;
            }
            
            // 예측할 수 없으면 현재 위치 반환
            return context.target.position;
        }
        
        public override bool IsActionInProgress() {
            return _isCharging || _isFiring;
        }
        
        public override void DrawGizmos(AIContextData context) {
            if (_isCharging || _isFiring) {
                // 공격 중인 경우 타겟까지의 경로 표시
                Gizmos.color = _isCharging ? Color.yellow : Color.red;
                
                Vector3 startPos = context.self.position + Vector3.up * 1.5f;
                Vector3 targetPos = context.target != null ? PredictTargetPosition(context) : startPos + context.self.forward * 10f;
                
                Gizmos.DrawLine(startPos, targetPos);
                Gizmos.DrawWireSphere(targetPos, 0.3f);
            }
            else {
                // 공격 가능 범위 시각화
                Gizmos.color = new Color(1f, 0.8f, 0f, 0.1f);
                Gizmos.DrawWireSphere(context.self.position, settings.rangedAttackRange);
            }
        }
    }
}