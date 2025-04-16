using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    [CreateAssetMenu(fileName = "AICombatMovementSetting", menuName = "Breadcrumbs/Tools/Create AICombatMovementSetting")]
    public class AICombatMovementSetting : ScriptableObject {
        public float raycastDistance = 1f; // 최소값 1f
        public float moveSpeed = 3f;
        public float turnSpeed = 5f;
        public float detectionRadius = 5f;
        public float separationDistance = 1.5f;
        public float strafingRadius = 3f; // 스트래핑 시작 거리
        public float strafingSpeed = 2f; // 회전 속도
        public bool changeStrafingDirection = true;
    }
}