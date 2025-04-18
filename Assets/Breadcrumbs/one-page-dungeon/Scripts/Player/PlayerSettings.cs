using UnityEngine;

namespace Breadcrumbs.Player {
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Breadcrumbs/Tools/Create PlayerSettings")]
    public class PlayerSettings : ScriptableObject {
        public float moveSpeed = 5f;
        public float backwardSpeedMultiplier = 0.7f; // 후진 속도 감속 비율
        public float rotationSpeed = 180f; // 회전 속도 (초당 각도)
        public float strafeSpeed = 3f; // 좌우 평행 이동 속도
        public float meleeAttackRange = 1.5f;
        public float dashForce = 10f;
    }
}