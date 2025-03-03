using UnityEngine;
using UnityEngine.AI;

namespace Breadcrumbs.day12
{
    public class Sensor : MonoBehaviour
    {
        public float distance = 10f;  // 감지 거리 설정
        public string targetTag = "Enemy";
        
        public GameObject GetNearestTarget()
        {
            GameObject nearestTarget = null;
            float nearestDistance = float.MaxValue;
            
            GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
            
            foreach (GameObject target in targets)
            {
                float currentDistance = Vector3.Distance(transform.position, target.transform.position);
                if (currentDistance <= distance && currentDistance < nearestDistance)
                {
                    nearestDistance = currentDistance;
                    nearestTarget = target;
                }
            }
            
            return nearestTarget;
        }
    }
}