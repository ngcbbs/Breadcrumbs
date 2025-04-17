using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.AISystem {
    public class PatrolBehavior : AIMovementBehaviorBase {
        private List<Vector3> _waypoints = new List<Vector3>();
        private int _currentWaypointIndex = 0;
        private float _waypointWaitTimer = 0f;
        private bool _isWaiting = false;

        public PatrolBehavior(AIMovementSettings settings) : base(settings) { }

        public override void Initialize(AIContextData context) {
            // 순찰 경로 설정 (여기서는 예시로 사각형 경로)
            _waypoints.Clear();
            Vector3 basePos = context.self.position;
            _waypoints.Add(basePos + new Vector3(5, 0, 5));
            _waypoints.Add(basePos + new Vector3(-5, 0, 5));
            _waypoints.Add(basePos + new Vector3(-5, 0, -5));
            _waypoints.Add(basePos + new Vector3(5, 0, -5));

            _currentWaypointIndex = 0;
            _isWaiting = false;
            _waypointWaitTimer = 0f;
        }

        public override float EvaluateSuitability(AIContextData context) {
            // 타겟이 없거나 매우 멀리 있을 때 순찰이 적합
            if (context.target == null) return 0.7f;

            float distanceToTarget = context.distanceToTarget;
            if (distanceToTarget > settings.detectionRadius * 1.2f) {
                return 0.6f;
            }

            // 기본적으로는 낮은 점수
            return 0.2f;
        }

        public override Vector3 CalculateDirection(AIContextData context) {
            if (_waypoints.Count == 0) return Vector3.zero;

            if (_isWaiting) {
                _waypointWaitTimer -= Time.deltaTime;
                if (_waypointWaitTimer <= 0) {
                    _isWaiting = false;
                    _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
                }

                return Vector3.zero;
            }

            Vector3 currentWaypoint = _waypoints[_currentWaypointIndex];
            Vector3 directionToWaypoint = currentWaypoint - context.self.position;
            float distanceToWaypoint = directionToWaypoint.magnitude;

            if (distanceToWaypoint < settings.waypointReachedDistance) {
                _isWaiting = true;
                _waypointWaitTimer = settings.waypointWaitTime;
                return Vector3.zero;
            }

            return directionToWaypoint.normalized;
        }

        public override void DrawGizmos(AIContextData context) {
            if (_waypoints.Count == 0) return;

            // 경로 그리기
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _waypoints.Count; i++) {
                int nextIndex = (i + 1) % _waypoints.Count;
                Gizmos.DrawLine(_waypoints[i], _waypoints[nextIndex]);
                Gizmos.DrawSphere(_waypoints[i], 0.3f);
            }

            // 현재 목표 지점 강조
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_waypoints[_currentWaypointIndex], 0.5f);
        }
    }
}