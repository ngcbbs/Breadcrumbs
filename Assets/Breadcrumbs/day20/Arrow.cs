using Breadcrumbs.Common;
using Breadcrumbs.day11;
using UnityEngine;

namespace Breadcrumbs.day20 {
    public class Arrow : MonoBehaviour, IPoolable {
        //public ArrowLauncher launcher;
        private float lifetime = 0f;
        private bool isActive = false;
        private const float MAX_LIFETIME = 5f;
        private bool _stuck = false;
        
        // {
        public float launchSpeed = 10f;
        public float gravity = 9.8f;
    
        private Vector3 initialVelocity; // 초기 속도
        private Vector3 currentVelocity; // 현재 속도
        private float elapsedTime; // 경과 시간

        private float drag = 0.01f;
        // }

        public void Activate(float speed, float drag, Vector3 direction) {
            this.drag = drag;
            launchSpeed = speed;
            initialVelocity = direction * launchSpeed;
            currentVelocity = initialVelocity;
            elapsedTime = 0f;
            
            lifetime = 0f;
            isActive = true;
            _stuck = false;
        }

        void Update() {
            if (isActive) {
                lifetime += Time.deltaTime;
                if (lifetime >= MAX_LIFETIME) {
                    ArrowPoolManager.Instance.Release(this);
                    isActive = false;
                }
            }

            if (_stuck)
                return;
            
            if (isActive) {
                if (transform.position.y < 0f) {
                    _stuck = true;
                    return;
                }
                
                elapsedTime += Time.deltaTime;
                
                // 중력 및 공기 저항 적용
                Vector3 gravityForce = Vector3.down * (gravity * Time.deltaTime);
                Vector3 dragForce = -currentVelocity.normalized * (currentVelocity.sqrMagnitude * drag * Time.deltaTime);
                currentVelocity += gravityForce + dragForce;

                // 위치 업데이트
                transform.position += currentVelocity * Time.deltaTime;

                if (currentVelocity.magnitude > 0.1f)
                {
                    /*
                    Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                    // */
                    transform.rotation = Quaternion.LookRotation(currentVelocity);
                }
            }
        }

        void OnCollisionEnter(Collision collision) {
            if (!isActive) return;

            // 바닥에 닿았을 때 (Layer 사용을 권장하지만 여기서는 태그로 예시)
            if (collision.gameObject.CompareTag("Ground")) {
                ArrowPoolManager.Instance.Release(this);
                isActive = false;
                _stuck = true;
                Debug.Log("스턱!");
            }
            // 적과 충돌했을 때
            else if (collision.gameObject.CompareTag("Enemy")) {
                Debug.Log($"Enemy {collision.gameObject.name} hit by arrow at {Time.time}");
                var enemy = collision.gameObject.GetComponent<EnemyUnit>();
                enemy?.Die();
                ArrowPoolManager.Instance.Release(this);
                isActive = false;
            }
        }

        public void OnSpawn() {
            Debug.Log($"OnSpawn {name}");
        }
        public void OnDespawn() {
            Debug.Log($"OnDespawn {name}");
        }
    }
}