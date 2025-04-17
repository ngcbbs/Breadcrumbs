using UnityEngine;

namespace Breadcrumbs.AISystem {
    // AI 웨이포인트 시스템 (옵션)
    public class AIWaypointSystem : MonoBehaviour {
        public Transform[] waypoints;
        public bool isLooping = true;

        private int _currentIndex = 0;

        public Vector3 GetCurrentWaypoint() {
            if (waypoints == null || waypoints.Length == 0) return transform.position;
            return waypoints[_currentIndex].position;
        }

        public bool MoveToNextWaypoint() {
            if (waypoints == null || waypoints.Length == 0) return false;

            _currentIndex++;

            if (_currentIndex >= waypoints.Length) {
                if (isLooping) {
                    _currentIndex = 0;
                    return true;
                }

                _currentIndex = waypoints.Length - 1;
                return false;
            }

            return true;
        }

        private void OnDrawGizmos() {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = Color.yellow;

            for (int i = 0; i < waypoints.Length; i++) {
                if (waypoints[i] == null) continue;

                // 웨이포인트 위치 표시
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                // 다음 웨이포인트와 연결
                if (i < waypoints.Length - 1 && waypoints[i + 1] != null) {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
                // 루프라면 마지막과 첫 웨이포인트 연결
                else if (i == waypoints.Length - 1 && isLooping && waypoints[0] != null) {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }
        }
    }
}