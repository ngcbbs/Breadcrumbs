using UnityEngine;

namespace Breadcrumbs.day18 {
    public class SmearEffectShaderController : MonoBehaviour
    {
        public Material smearMaterial;
        public float smearStrength = 0.05f; // Smear 강도 조절
        private Vector3 lastWorldPosition;
        private float lastRotationZ;

        void Start()
        {
            lastWorldPosition = transform.position; // 월드 좌표 저장
            lastRotationZ = transform.eulerAngles.z;
        }

        void Update()
        {
            // 이동 방향 계산 (월드 기준)
            Vector3 worldVelocity = (transform.position - lastWorldPosition) / Time.deltaTime;
            lastWorldPosition = transform.position;

            // 회전 속도 계산
            //float rotationSpeed = (transform.eulerAngles.z - lastRotationZ) / Time.deltaTime;
            float rotation = transform.eulerAngles.z;
        

            // Smear 방향을 반대로 설정 (월드 이동 방향 반대)
            Vector4 smearDir = new Vector4(-worldVelocity.x, -worldVelocity.y, 0, 0) * smearStrength;

            // 쉐이더에 값 전달
            smearMaterial.SetVector("_SmearDirection", smearDir);
            smearMaterial.SetFloat("_PreviousRotation", rotation-15f);
            smearMaterial.SetFloat("_Rotation", rotation);
        
            lastRotationZ = rotation;
        }
    }
}