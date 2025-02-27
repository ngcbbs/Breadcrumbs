using RVO;
using UnityEngine;

namespace Breadcrumbs.day8 {
    public class ORCAAgent : MonoBehaviour
    {
        public float maxSpeed = 2.0f;
        public float neighborDist = 5.0f;
        public int maxNeighbors = 10;
        public float timeHorizon = 5.0f;
        public float radius = 1.0f;

        private int agentID;

        void Start()
        {
            agentID = Simulator.Instance.addAgent(new RVO.Vector2(transform.position.x, transform.position.z));
            Debug.Log($"ORCAAgent Id={agentID}");
            Simulator.Instance.setAgentDefaults(agentID,
                maxNeighbors, timeHorizon, 5, radius, maxSpeed, new RVO.Vector2(0, 0));
            Simulator.Instance.setAgentMaxSpeed(agentID, maxSpeed);
            Simulator.Instance.setAgentNeighborDist(agentID, neighborDist);
            Simulator.Instance.setAgentMaxNeighbors(agentID, maxNeighbors);
            Simulator.Instance.setAgentTimeHorizon(agentID, timeHorizon);
            Simulator.Instance.setAgentRadius(agentID, radius);
        }

        void Update()
        {
            Vector3 preferredVelocity = GetPreferredVelocity();
            Simulator.Instance.setAgentPrefVelocity(agentID, new RVO.Vector2(preferredVelocity.x, preferredVelocity.z));

            RVO.Vector2 newVelocity = Simulator.Instance.getAgentVelocity(agentID);
            transform.position += new Vector3(newVelocity.x(), 0, newVelocity.y()) * Time.deltaTime;
        }

        Vector3 GetPreferredVelocity()
        {
            // Potential Field로부터 힘을 받아서 ORCA의 선호 속도 계산
            Vector3 potentialFieldForce = GetComponent<UnitController>().CalculatePotentialField();
            return potentialFieldForce.normalized * maxSpeed;
        }
    }
}
