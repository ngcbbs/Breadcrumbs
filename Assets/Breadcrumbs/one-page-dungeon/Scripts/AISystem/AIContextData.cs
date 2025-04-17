using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.AISystem {
    // AI가 판단할 때 필요한 컨텍스트 정보
    public class AIContextData {
        public Transform self;
        public Transform target;
        public float distanceToTarget;
        public Vector3 currentVelocity;
        public Vector3 currentDirection;
        public Dictionary<string, object> customData = new Dictionary<string, object>();
        // 위협, 아군, 환경 등의 정보도 추가 가능

        public List<AIThreat> threats = new List<AIThreat>();
        public List<Transform> allies = new List<Transform>();

        public AIContextData(Transform self, Transform target) {
            this.self = self;
            this.target = target;
            UpdateData();
        }

        public void UpdateData() {
            if (target != null) {
                distanceToTarget = Vector3.Distance(self.position, target.position);
            }
        }
    }
}