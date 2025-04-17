using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class Projectile : MonoBehaviour {
        private Transform _owner;
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private bool _hasHit = false;
        
        public void Initialize(Transform owner, Vector3 direction, float speed, float damage) {
            _owner = owner;
            _direction = direction;
            _speed = speed;
            _damage = damage;
        }
        
        private void Update() {
            if (_hasHit) return;
            
            // 이동 처리
            transform.position += _direction * _speed * Time.deltaTime;
        }
        
        private void OnTriggerEnter(Collider other) {
            if (_hasHit || other.transform == _owner) return;
            
            // 타겟에 데미지 적용
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.TakeDamage(_damage);
            }
            
            // 충돌 효과
            CreateImpactEffect();
            
            // 프로젝타일 제거
            _hasHit = true;
            Destroy(gameObject);
        }
        
        private void CreateImpactEffect() {
            // 여기에 충돌 파티클이나 사운드 효과 추가
            Debug.Log("Projectile impact!");
        }
    }
}