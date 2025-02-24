using UnityEngine;

namespace day5_scrap {
    // 카메라 컨트롤러도 새 입력 시스템으로 변환
    public class ActionCamera : MonoBehaviour {
        public Transform target;
        public float distance = 5f;
        public float height = 2f;
        public float smoothSpeed = 10f;
        public float rotationSmoothSpeed = 5f;

        private float currentRotationX;
        private float currentRotationY;
        private PlayerInputActions playerInputs;

        private void Awake() {
            playerInputs = new PlayerInputActions();
        }

        private void OnEnable() {
            playerInputs.asset.Enable();
        }

        private void OnDisable() {
            playerInputs.asset.Disable();
        }

        private void LateUpdate() {
            if (target == null) return;

            Vector2 lookInput = playerInputs.CameraLook.ReadValue<Vector2>();

            currentRotationY += lookInput.x;
            currentRotationX -= lookInput.y;
            currentRotationX = Mathf.Clamp(currentRotationX, -30f, 60f);

            Vector3 targetPosition = target.position;
            targetPosition.y += height;

            Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + targetPosition;

            RaycastHit hit;
            if (Physics.Raycast(targetPosition, position - targetPosition, out hit, distance)) {
                position = hit.point;
            }

            transform.position = Vector3.Lerp(transform.position, position, smoothSpeed * Time.deltaTime);
            transform.LookAt(targetPosition);
        }

        private void OnDestroy() {
            playerInputs.Dispose();
        }
    }
}