using UnityEngine;

namespace Breadcrumbs.AISystem {
    public enum AIThreatType {
        Projectile,
        MeleeAttack,
        AreaEffect,
        Environmental
    }
    
    // 위협 정보 저장 클래스
    public class AIThreat {
        public Transform source;
        public float dangerLevel;
        public Vector3 direction;
        public float distance;
        public AIThreatType threatType;

        public AIThreat(Transform source, float dangerLevel, AIThreatType threatType) {
            this.source = source;
            this.dangerLevel = dangerLevel;
            this.threatType = threatType;
        }
    }
}