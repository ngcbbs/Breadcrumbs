using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    [CreateAssetMenu(fileName = "AIStateTransitionSettings", menuName = "Breadcrumbs/Tools/Create AIStateTransitionSettings")]
    public class AIStateTransitionSettings : ScriptableObject
    {
        [Header("Detection Settings")]
        public float detectionRadius = 8f;      // 플레이어 감지 반경
        public float loseInterestRadius = 12f;  // 관심을 잃는 거리
        public float loseInterestDelay = 3f;    // 관심을 잃기까지의 시간 (초)
        
        [Header("Combat Settings")]
        public float combatMinDistance = 2f;    // 전투 최소 거리
        public float combatMaxDistance = 6f;    // 전투 최대 거리
        
        [Header("Flee Settings")]
        public float fleeMinDuration = 3f;      // 최소 도망 시간
        public float fleeMaxDuration = 6f;      // 최대 도망 시간
        public float fleeReturnCombatDistance = 9f; // 도망 중 전투로 돌아올 거리
        public float fleeForceMinDuration = 1.5f; // 최소 강제 도망 시간 (바로 전투로 돌아가지 않도록)
        
        [Header("Debug")]
        public bool showDebugVisuals = true;    // 디버그 시각화 여부
    }
}