using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    [CreateAssetMenu(fileName = "AIIdleMovementSetting", menuName = "Breadcrumbs/Tools/Create AIIdleMovementSetting")]
    public class AIIdleMovementSetting : ScriptableObject {
        [Header("Movement Settings")]
        public float moveSpeed = 2f;
        public float turnSpeed = 3f;

        [Header("Wandering Settings")]
        public float wanderRadius = 5f; // 중심점으로부터 이동할 수 있는 최대 반경
        public Vector3 areaCenter; // 이동 영역의 중심점
        public bool useTransformAsCenter = true; // true면 시작 위치를 중심으로 사용

        [Header("Direction Change Settings")]
        public float minDirectionChangeTime = 2f; // 방향을 바꾸는 최소 시간
        public float maxDirectionChangeTime = 5f; // 방향을 바꾸는 최대 시간

        [Header("Collision Avoidance")]
        public float raycastDistance = 1.5f; // 장애물 감지 거리
        public float separationDistance = 1.2f; // 다른 AI와 유지할 거리
        public LayerMask obstacleLayer; // 장애물 레이어

        [Header("Debug")]
        public bool showDebugVisuals = true; // 디버그 시각화 표시 여부
    }
}