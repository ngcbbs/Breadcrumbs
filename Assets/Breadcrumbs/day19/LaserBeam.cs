using Breadcrumbs.day11;
using UnityEngine;

namespace Breadcrumbs.day19 {
    public class LaserBeam : MonoBehaviour {
        public LineRenderer lineRenderer;
        public Transform firePoint;
        public float maxDistance = 100f; // 최대 사거리
        public float laserDuration = 0.1f; // 레이저 표시 시간

        private float timer;

        void Start() {
            lineRenderer.positionCount = 2; // 시작점과 끝점
            lineRenderer.enabled = false;
        }

        void Update() {
            if (Input.GetMouseButtonDown(1)) {
                ShootLaser();
            }

            if (lineRenderer.enabled) {
                timer -= Time.deltaTime;
                if (timer <= 0)
                    lineRenderer.enabled = false;
            }
        }

        void ShootLaser() {
            RaycastHit hit;
            Vector3 endPoint;

            if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, maxDistance)) {
                endPoint = hit.point; // 충돌 지점
                if (hit.collider.tag == "Enemy") {
                    var enemyUnit = hit.collider.GetComponent<EnemyUnit>();
                    enemyUnit.Die();
                    Debug.Log(hit.collider.name);
                }
            }
            else {
                endPoint = firePoint.position + firePoint.forward * maxDistance; // 최대 거리
            }

            // Line Renderer로 레이저 그리기
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.enabled = true;
            timer = laserDuration;
        }
    }
}