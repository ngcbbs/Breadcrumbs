using Breadcrumbs.day11;
using UnityEngine;

namespace Breadcrumbs.day19 {
    public class Arrow : MonoBehaviour {
        public ArrowLauncherWithPooling launcher;
        private float lifetime = 0f;
        private bool isActive = false;
        private const float MAX_LIFETIME = 5f; // 2초 후 비활성화
        
        private Vector3 _lastPosition;
        private Vector3 _velocity;
        private bool _stuck = false;

        public void Activate() {
            lifetime = 0f;
            isActive = true;
            _lastPosition = transform.position;
            _velocity = Vector3.zero;
            _stuck = false;
        }

        void Update() {
            if (_stuck)
                return;
            if (isActive) {
                _velocity = (transform.position - _lastPosition) / Time.deltaTime;
                
                if (_velocity.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_velocity);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
                
                lifetime += Time.deltaTime;
                if (lifetime >= MAX_LIFETIME) {
                    launcher.DisableArrow(gameObject);
                    isActive = false;
                }

                _lastPosition = transform.position;
            }
        }

        void OnCollisionEnter(Collision collision) {
            if (!isActive) return;

            // 바닥에 닿았을 때 (Layer 사용을 권장하지만 여기서는 태그로 예시)
            if (collision.gameObject.CompareTag("Ground")) {
                launcher.DisableArrow(gameObject);
                isActive = false;
                _stuck = true;
            }
            // 적과 충돌했을 때
            else if (collision.gameObject.CompareTag("Enemy")) {
                Debug.Log($"Enemy {collision.gameObject.name} hit by arrow at {Time.time}");
                var enemy = collision.gameObject.GetComponent<EnemyUnit>();
                enemy?.Die();
                launcher.DisableArrow(gameObject);
                isActive = false;
            }
        }
    }
}