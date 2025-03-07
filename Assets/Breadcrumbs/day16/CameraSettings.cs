using UnityEngine;

namespace Breadcrumbs.day16 {
    public class CameraSettings : MonoBehaviour {
        public ThirdPersonCamera cameraController;

        [SerializeField] private float fieldOfView = 60f;
        [SerializeField] private float trackingSensitivity = 10f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2f, -5f);

        private void OnValidate() {
            if (cameraController != null) {
                UpdateSettings();
            }
        }

        public void UpdateSettings() {
            cameraController.SetFieldOfView(fieldOfView);
            cameraController.SetTrackingSensitivity(trackingSensitivity);
            cameraController.SetOffset(cameraOffset);
        }
    }
}