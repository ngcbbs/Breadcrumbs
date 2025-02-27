using UnityEngine;

namespace Breadcrumbs.day8 {
    public class UnitController : MonoBehaviour
    {
        public Vector3 goal; // 목표 위치
        public float speed = 5f; // 이동 속도
        public float repulsionStrength = 10f; // 장애물 회피 강도
        public float attractionStrength = 5f; // 목표 지점으로 향하는 힘
        public float detectionRadius = 5f; // 장애물 감지 반경

        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            
            // Y축 회전 및 기울어짐 방지
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        void FixedUpdate()
        {
            Vector3 force = CalculatePotentialField();
            ApplyMovement(force);
        }

        public Vector3 CalculatePotentialField()
        {
            Vector3 totalForce = Vector3.zero;

            // 목표 지점으로의 흡인력 (Attractive Force)
            Vector3 goalDirection = (goal - transform.position).normalized;
            totalForce += goalDirection * attractionStrength;

            // 장애물로부터의 반발력 (Repulsive Force)
            Collider[] obstacles = Physics.OverlapSphere(transform.position, detectionRadius);
            foreach (Collider obs in obstacles)
            {
                if (obs.gameObject != gameObject) // 자기 자신 제외
                {
                    Vector3 direction = transform.position - obs.transform.position;
                    float distance = direction.magnitude;
                    if (distance > 0)
                    {
                        totalForce += (direction.normalized / distance) * repulsionStrength;
                    }
                }
            }

            return totalForce;
        }

        void ApplyMovement(Vector3 force)
        {
            Vector3 velocity = force.normalized * speed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
    }
}
