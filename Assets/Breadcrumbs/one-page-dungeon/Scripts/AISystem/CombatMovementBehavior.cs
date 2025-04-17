using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class CombatMovementBehavior : AIMovementBehaviorBase {
        private Vector3[] _directions;
        private float[] _weights;
        private Collider[] _results;
        private bool _isStrafing = false;
        private float _strafingDirection = 1f;

        // 공격 관련 변수 추가
        private float _attackCooldown = 0f;
        private readonly float _attackRange;
        private bool _isAttacking = false;
        private float _attackTimer = 0f;

        public CombatMovementBehavior(AIMovementSettings settings) : base(settings) {
            _directions = new Vector3[] {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right,
                (Vector3.forward + Vector3.left).normalized,
                (Vector3.forward + Vector3.right).normalized,
                (Vector3.back + Vector3.left).normalized,
                (Vector3.back + Vector3.right).normalized
            };
            _weights = new float[_directions.Length];
            _results = new Collider[16];

            // 공격 설정 초기화
            _attackRange = settings.strafingRadius * settings.attackRangeRatio; // 0.75f; // 이동 설정에서 범위 가져오기
        }

        public override float EvaluateSuitability(AIContextData context) {
            // 타겟이 있고 그 타겟이 적절한 거리에 있으면 전투 이동이 적합
            if (context.target == null) return 0.2f;

            float distanceToTarget = context.distanceToTarget;

            // 타겟이 적절한 범위 내에 있을 때 높은 점수
            if (distanceToTarget < settings.detectionRadius) {
                // 너무 가까우면 회피나 대시가 더 적합할 수 있음
                if (distanceToTarget < settings.strafingRadius * 0.5f) {
                    return 0.5f;
                }

                return 0.8f;
            }

            return 0.3f; // 기본 적합도
        }

        public override Vector3 CalculateDirection(AIContextData context) {
            // 공격 처리
            UpdateAttackState(context);

            Transform self = context.self;
            Transform target = context.target;

            if (target == null) return Vector3.zero;

            float distanceToTarget = context.distanceToTarget;

            if (distanceToTarget < settings.strafingRadius) {
                _isStrafing = true;
            } else {
                _isStrafing = false;
            }

            // 변경된 부분: 공격 애니메이션 중 수행 단계에서만 정지하고
            // 나머지 시간은 약간의 움직임을 허용
            if (_isAttacking) {
                // 공격 타이밍 직전/직후에만 정지
                float attackMidPoint = settings.attackDuration / 2;
                if (_attackTimer > attackMidPoint - 0.1f && _attackTimer < attackMidPoint + 0.1f) {
                    return Vector3.zero;
                }
                // 나머지 시간은 느리게 움직임
                else if (_isStrafing) {
                    return GetStrafingDirection(context) * 0.3f; // 감소된 속도로 계속 움직임
                }
            }

            // 공격 범위 내 && 쿨다운 완료 => 공격 시작
            if (distanceToTarget <= _attackRange && _attackCooldown <= 0) {
                StartAttack(context);
                return Vector3.zero;
            }

            if (_isStrafing) {
                return GetStrafingDirection(context);
            } else {
                Vector3 bestDirection = Vector3.zero;
                float bestWeight = float.MinValue;

                for (int i = 0; i < _directions.Length; i++) {
                    _weights[i] = EvaluateDirection(_directions[i], context);
                    if (_weights[i] > bestWeight) {
                        bestWeight = _weights[i];
                        bestDirection = _directions[i];
                    }
                }

                Vector3 separation = GetSeparationVector(context);
                return (bestDirection + separation * 0.5f).normalized;
            }
        }

        // 공격 상태 업데이트
        private void UpdateAttackState(AIContextData context) {
            // 쿨다운 감소
            if (_attackCooldown > 0) {
                _attackCooldown -= Time.deltaTime;
            }

            // 공격 진행 중일 때
            if (_isAttacking) {
                _attackTimer -= Time.deltaTime;

                // 공격 타이밍 (공격 지속시간의 중간 지점)
                if (_attackTimer <= settings.attackDuration / 2 && _attackTimer > settings.attackDuration / 2 - Time.deltaTime) {
                    PerformAttack(context);
                    Debug.Log("basic 공격 처리!");
                }
                
                // 공격 종료
                if (_attackTimer <= 0) {
                    _isAttacking = false;
                    _attackCooldown = 2f; // 공격 후 쿨다운
            
                    // 추가: 공격 종료 후 바로 새로운 방향 결정을 위한 플래그 설정
                    // 필요에 따라 컨트롤러에 알림을 보내는 이벤트 추가 가능
                    Debug.Log($"{context.self.name} finished attack and resumed movement!");
                }
            }
        }

        // 공격 시작
        private void StartAttack(AIContextData context) {
            if (context.target == null || _isAttacking) return;

            _isAttacking = true;
            _attackTimer = settings.attackDuration;

            // 공격 방향으로 회전
            Vector3 dirToTarget = (context.target.position - context.self.position).normalized;
            dirToTarget.y = 0; // Y축 회전만 고려

            if (dirToTarget != Vector3.zero) {
                context.self.rotation = Quaternion.LookRotation(dirToTarget);
            }

            Debug.Log($"{context.self.name} starts basic attack!");
        }

        // 실제 공격 판정 적용
        private void PerformAttack(AIContextData context) {
            // 공격 범위 내 대상 검출
            Collider[] hitColliders = new Collider[5];
            Vector3 attackCenter = context.self.position + context.self.forward * (_attackRange * 0.5f);
            int hitCount = Physics.OverlapSphereNonAlloc(
                attackCenter,
                _attackRange * 0.5f,
                hitColliders,
                LayerMask.GetMask("Player") // , "Enemy", "NPC") // 적절한 레이어로 변경 필요
            );

            for (int i = 0; i < hitCount; i++) {
                if (hitColliders[i].transform != context.self) {
                    // 대상에게 데미지 적용
                    IDamageable damageable = hitColliders[i].GetComponent<IDamageable>();
                    if (damageable != null) {
                        damageable.TakeDamage(settings.attackDamage);
                        Debug.Log($"{context.self.name} hit {hitColliders[i].name} for {settings.attackDamage} damage!");
                    }

                    // 넉백 적용 (선택사항)
                    Rigidbody rb = hitColliders[i].GetComponent<Rigidbody>();
                    if (rb != null) {
                        Vector3 knockbackDir = (hitColliders[i].transform.position - context.self.position).normalized;
                        rb.AddForce(knockbackDir * 3f, ForceMode.Impulse);
                    }
                }
            }
        }

        // 기존 메서드들은 그대로 유지
        private float EvaluateDirection(Vector3 dir, AIContextData context) {
            // 기존 코드 유지
            Transform self = context.self;
            Transform target = context.target;

            Vector3 toTarget = (target.position - self.position).normalized;
            float targetWeight = Vector3.Dot(dir, toTarget);

            if (Physics.Raycast(self.position, dir, settings.raycastDistance)) {
                return -1f;
            }

            int size = Physics.OverlapSphereNonAlloc(
                self.position + dir,
                settings.separationDistance,
                _results
            );

            for (int i = 0; i < size; i++) {
                var col = _results[i];
                if (col.gameObject != self.gameObject && col.CompareTag("Enemy")) {
                    targetWeight -= 0.5f;
                }
            }

            float distanceToTarget = context.distanceToTarget;
            if (distanceToTarget < settings.detectionRadius) {
                if (dir == Vector3.left || dir == Vector3.right) {
                    targetWeight += 0.3f;
                } else if (dir == Vector3.forward || dir == Vector3.back) {
                    targetWeight -= 0.3f;
                }
            }

            return targetWeight;
        }

        private Vector3 GetSeparationVector(AIContextData context) {
            // 기존 코드 유지
            Transform self = context.self;
            Vector3 separation = Vector3.zero;

            foreach (Transform ally in context.allies) {
                Vector3 away = (self.position - ally.position).normalized;
                away = Quaternion.Euler(0, 30, 0) * away;
                separation += away;
            }

            return separation.normalized;
        }

        private Vector3 GetStrafingDirection(AIContextData context) {
            // 기존 코드 유지
            Transform self = context.self;
            Transform target = context.target;

            Vector3 toTarget = (target.position - self.position).normalized;
            Vector3 strafeDir = Vector3.Cross(Vector3.up, toTarget) * _strafingDirection;

            if (settings.changeStrafingDirection) {
                _strafingDirection = Mathf.Sign(Mathf.Sin(Time.time * settings.strafingSpeed));
            }

            return strafeDir;
        }

        // 기즈모에 공격 범위 추가
        public override void DrawGizmos(AIContextData context) {
            if (_directions == null || _weights == null) return;

            Vector3 pos = context.self.position;

            for (int i = 0; i < _directions.Length; i++) {
                float weight = _weights[i];
                Gizmos.color = weight > 0 ? Color.green : Color.red;
                float length = Mathf.Clamp(Mathf.Abs(weight), 0, 1) * 2f;
                Gizmos.DrawLine(pos, pos + _directions[i] * length);
            }

            // 공격 범위 시각화
            if (_isAttacking) {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Vector3 attackCenter = context.self.position + context.self.forward * (_attackRange * 0.5f);
                Gizmos.DrawSphere(attackCenter, _attackRange * 0.5f);
            } else {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.1f);
                Gizmos.DrawWireSphere(context.self.position, _attackRange);
            }
        }

        // IsActionInProgress 메서드 추가 - 전투 컨트롤러에서 사용
        public bool IsAttacking() {
            return _isAttacking;
        }
    }
}