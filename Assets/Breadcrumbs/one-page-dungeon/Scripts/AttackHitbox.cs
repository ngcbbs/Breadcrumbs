using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public class AttackHitbox : MonoBehaviour {
        public enum HitboxShape {
            Box,
            Sphere,
            Capsule,
            Arc
        }

        public HitboxShape shape = HitboxShape.Box;

        public Vector3 size = Vector3.one;
        public float radius = 0.5f;
        public Vector3 offset = Vector3.zero;
        public float range = 1.0f;

        // Arc 전용 설정
        public float arcAngle = 90.0f;
        public float arcRange = 2.0f;
        
        public bool allowMultipleHits = false; // 콤보 히트 가능 여부

        public LayerMask targetLayer;
        private readonly HashSet<GameObject> _hitTargets = new HashSet<GameObject>();
        
        public float hitDelay = 0.5f;
        private Dictionary<GameObject, float> _hitTargetTimes = new Dictionary<GameObject, float>();

        public bool debugDraw = true;

        private const int kMaxColliders = 5;
        private Collider[] _colliders;

        private void Awake() {
            _colliders = new Collider[kMaxColliders];
        }

        public void ResetHitTargets() {
            _hitTargets.Clear();
            _hitTargetTimes.Clear();
        }

        public void CheckCollision() {
            var count = 0;
            var lastHitTime = 0f;
            
            if (shape == HitboxShape.Arc) {
                
                count = Physics.OverlapSphereNonAlloc(transform.position, arcRange, _colliders, targetLayer);

                for (var index = 0; index < count; index++) {
                    var col = _colliders[index];
                    if (!allowMultipleHits && _hitTargets.Contains(col.gameObject))
                        continue;

                    if (_hitTargetTimes.TryGetValue(col.gameObject, out lastHitTime) &&
                        Time.time - lastHitTime < hitDelay)
                        continue;

                    var dirToTarget = (col.transform.position - transform.position).normalized;
                    var distance = Vector3.Distance(transform.position, col.transform.position);
                    var angle = Vector3.Angle(transform.forward, dirToTarget);

                    if (angle <= arcAngle * 0.5f && distance <= arcRange) {
                        _hitTargets.Add(col.gameObject);
                        col.GetComponent<IDamageable>()?.OnDamage(10, dirToTarget);
                    }
                }
            }
            else {
                var worldPos = transform.position + transform.rotation * offset;

                switch (shape) {
                    case HitboxShape.Box:
                        count = Physics.OverlapBoxNonAlloc(worldPos, size * 0.5f, _colliders, transform.rotation, targetLayer);
                        break;

                    case HitboxShape.Sphere:
                        count = Physics.OverlapSphereNonAlloc(worldPos, radius, _colliders, targetLayer);
                        break;

                    case HitboxShape.Capsule:
                        var point1 = worldPos + Vector3.up * (range * 0.5f);
                        var point2 = worldPos - Vector3.up * (range * 0.5f);
                        count = Physics.OverlapCapsuleNonAlloc(point1, point2, radius, _colliders, targetLayer);
                        break;
                }

                for (var index = 0; index < count; index++) {
                    var col = _colliders[index];
                    if (!allowMultipleHits && _hitTargets.Contains(col.gameObject))
                        continue;
                    
                    if (_hitTargetTimes.TryGetValue(col.gameObject, out lastHitTime) &&
                        Time.time - lastHitTime < hitDelay)
                        continue;
                    
                    var dirToTarget = (col.transform.position - transform.position).normalized;

                    _hitTargets.Add(col.gameObject);
                    col.GetComponent<IDamageable>()?.OnDamage(10, dirToTarget); // 데미지 전달 인터페이스
                }
            }
        }

        private void OnDrawGizmos() {
            if (!debugDraw) return;

            Gizmos.color = Color.red;
            var worldPos = transform.position + transform.rotation * offset;

            switch (shape) {
                case HitboxShape.Box:
                    Gizmos.matrix = Matrix4x4.TRS(worldPos, transform.rotation, size);
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    break;

                case HitboxShape.Sphere:
                    Gizmos.DrawWireSphere(worldPos, radius);
                    break;

                case HitboxShape.Capsule:
                    var point1 = worldPos + Vector3.up * (range * 0.5f);
                    var point2 = worldPos - Vector3.up * (range * 0.5f);
                    Gizmos.DrawWireSphere(point1, radius);
                    Gizmos.DrawWireSphere(point2, radius);
                    break;

                case HitboxShape.Arc:
                    var forward = transform.forward * arcRange;
                    var leftRayRotation = Quaternion.Euler(0, -arcAngle * 0.5f, 0);
                    var rightRayRotation = Quaternion.Euler(0, arcAngle * 0.5f, 0);

                    var leftRay = leftRayRotation * forward;
                    var rightRay = rightRayRotation * forward;

                    Gizmos.DrawRay(transform.position, leftRay);
                    Gizmos.DrawRay(transform.position, rightRay);
                    Gizmos.DrawWireSphere(transform.position, arcRange);
                    break;
            }
            
            // todo: 피격 시점의 정보 디버그용으로 표시..
        }
    }
}
