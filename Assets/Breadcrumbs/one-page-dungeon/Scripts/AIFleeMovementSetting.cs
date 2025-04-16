using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    [CreateAssetMenu(fileName = "AIFleeMovementSetting", menuName = "Breadcrumbs/Tools/Create AIFleeMovementSetting")]
    public class AIFleeMovementSetting : ScriptableObject {
        [Header("Movement Settings")]
        public float fleeSpeed = 4f; // 도망 속도 (일반적으로 일반 이동보다 빠름)
        public float turnSpeed = 5f; // 회전 속도

        [Header("Flee Settings")]
        public float fleeRadius = 10f; // 최대 도망 거리
        public float minFleeDistance = 5f; // 최소 도망 거리

        [Header("Obstacle Avoidance")]
        public float raycastDistance = 2f; // 장애물 감지 거리
        public float separationDistance = 1.5f; // 다른 AI와 유지할 거리
        public LayerMask obstacleLayer; // 장애물 레이어

        [Header("Panic Settings")]
        public bool panicMovement = true; // 랜덤한 방향 전환 사용 여부
        public float panicDirectionChangeTime = 1f; // 패닉 상태에서 방향 전환 시간

        [Header("Debug")]
        public bool showDebugVisuals = true; // 디버그 시각화 여부
    }
}