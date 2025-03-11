using Breadcrumbs.day11;
using UnityEngine;

namespace Breadcrumbs.day19 {
    public class Arrow : MonoBehaviour {
        public ArrowLauncherWithPooling launcher;
        private float lifetime = 0f;
        private bool isActive = false;
        private const float MAX_LIFETIME = 2f; // 2초 후 비활성화

        public void Activate() {
            lifetime = 0f;
            isActive = true;
        }

        void Update() {
            if (isActive) {
                lifetime += Time.deltaTime;
                if (lifetime >= MAX_LIFETIME) {
                    launcher.DisableArrow(gameObject);
                    isActive = false;
                }
            }
        }

        void OnCollisionEnter(Collision collision) {
            if (!isActive) return;

            // 바닥에 닿았을 때 (Layer 사용을 권장하지만 여기서는 태그로 예시)
            if (collision.gameObject.CompareTag("Ground")) {
                launcher.DisableArrow(gameObject);
                isActive = false;
            }
            // 적과 충돌했을 때
            else if (collision.gameObject.CompareTag("Enemy")) {
                Debug.Log($"Enemy {collision.gameObject.name} hit by arrow at {Time.time}");
                var enemy = collision.gameObject.GetComponent<EnemyUnit>();
                enemy.Die();
                launcher.DisableArrow(gameObject);
                isActive = false;
            }
        }
    }
}