using UnityEngine;

namespace Breadcrumbs.day16 {
    public class ThirdPersonCamera : MonoBehaviour {
        [Header("Target Settings")] public Transform target;
        public Vector3 offset = new Vector3(0f, 2f, -5f);

        [Header("Distance Settings")] public float minDistance = 2f;
        public float maxDistance = 10f;
        public float currentDistance = 5f;
        public float distanceSmooth = 5f;

        [Header("Tracking Settings")] public float trackingSpeed = 10f;
        public float predictionFactor = 0.5f;

        [Header("Rotation Settings")] public float rotationSpeed = 3f;
        public float verticalAngleMin = -80f;
        public float verticalAngleMax = 80f;

        [Header("Obstacle Settings")] public LayerMask obstacleLayer;
        public float obstacleBuffer = 0.5f;

        private float currentX = 0f;
        private float currentY = 0f;
        private Vector3 velocity = Vector3.zero;
        private Vector3 lastTargetPosition;

        private void Start() {
            if (target == null) {
                Debug.LogError("Camera target not set!");
                enabled = false;
                return;
            }

            lastTargetPosition = target.position;
        }

        private void LateUpdate() {
            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput() {
            // Rotation control
            if (Input.GetMouseButton(1)) // Right mouse button
            {
                currentX += Input.GetAxis("Mouse X") * rotationSpeed;
                currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
                currentY = Mathf.Clamp(currentY, verticalAngleMin, verticalAngleMax);
            }

            // Distance control (mouse scroll)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            currentDistance -= scroll * distanceSmooth;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        private void UpdateCameraPosition() {
            // Calculate target velocity for prediction
            Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;
            lastTargetPosition = target.position;

            // Calculate desired position
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 desiredPosition = target.position + (rotation * (offset.normalized * currentDistance));

            // Add prediction
            Vector3 predictedPosition = desiredPosition + (targetVelocity * predictionFactor);

            // Obstacle detection
            Vector3 finalPosition = CheckObstacles(predictedPosition);

            // Smooth movement
            transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref velocity, 1f / trackingSpeed);

            // Look at target
            transform.LookAt(target.position);
        }

        private Vector3 CheckObstacles(Vector3 desiredPosition) {
            RaycastHit hit;
            Vector3 direction = desiredPosition - target.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(target.position, direction.normalized, out hit, distance, obstacleLayer)) {
                // 충돌 지점에서 타겟 방향으로 currentDistance를 유지하며 새 위치 계산
                Vector3 obstacleAdjustedDirection = (hit.point - target.position).normalized;
                Vector3 newPosition = target.position + obstacleAdjustedDirection * currentDistance;

                // 오프셋 높이 유지
                newPosition.y = target.position.y + offset.y;

                // 충돌 지점까지의 거리가 currentDistance보다 작으면 장애물 앞에 위치
                float distanceToHit = Vector3.Distance(target.position, hit.point);
                if (distanceToHit < currentDistance) {
                    Vector3 hitPosition = hit.point + (hit.normal * obstacleBuffer);
                    // 여전히 오프셋 높이 유지
                    hitPosition.y = target.position.y + offset.y;
                    return hitPosition;
                }

                return newPosition;
            }

            return desiredPosition;
        }

        // Public methods for UI/settings
        public void SetFieldOfView(float fov) {
            Camera.main.fieldOfView = Mathf.Clamp(fov, 20f, 90f);
        }

        public void SetTrackingSensitivity(float sensitivity) {
            trackingSpeed = Mathf.Clamp(sensitivity, 1f, 20f);
        }

        public void SetOffset(Vector3 newOffset) {
            offset = newOffset;
        }
    }
}

