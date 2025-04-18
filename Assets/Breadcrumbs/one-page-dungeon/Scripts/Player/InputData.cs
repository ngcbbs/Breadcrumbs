using UnityEngine;

namespace Breadcrumbs.Player {
    // 16 바이트 이내가 최적?
    public struct InputData {
        public float forwardInput; // 전진 (1) / 후진 (-1)
        public float rotationInput; // 좌회전 (-1) / 우회전 (1)
        public float strafeInput; // 좌 평행 이동 (-1) / 우 평행 이동 (1)
        public bool attackPressed;
        public bool dashPressed;

        public bool IsMoving => forwardInput != 0f || rotationInput != 0f || strafeInput != 0f;
        public string MoveInput => $"{forwardInput}, {rotationInput}, {strafeInput}";
        public Vector2 moveDirection => new Vector2(forwardInput, rotationInput);
    }
}