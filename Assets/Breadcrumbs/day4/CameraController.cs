using UnityEngine;
using UnityEngine.InputSystem;

namespace day4_scrap {
// 카메라 컨트롤러
    public class CameraController : MonoBehaviour {
        [SerializeField] private float sensitivity = 2f;
        [SerializeField] private float maxVerticalAngle = 80f;

        [Header("Camera Follow Settings")] [SerializeField]
        private Transform target;

        [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);
        [SerializeField] private float smoothSpeed = 10f;

        private PlayerInput playerInput;
        private InputAction lookAction;
        private float verticalRotation;

        private void Awake() {
            playerInput = GetComponentInParent<PlayerInput>();
            lookAction = playerInput.actions["Look"];
        }

        private void LateUpdate() {
            HandleRotation();
            FollowTarget();
        }

        private void HandleRotation() {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            // 상하 회전
            verticalRotation -= lookInput.y * sensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

            // 카메라 회전 적용
            transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        private void FollowTarget() {
            Vector3 desiredPosition = target.position + target.rotation * offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}