using System;
using UnityEngine;

namespace Breadcrumbs.Player {
    public class InputManager : MonoBehaviour {
        public static event EventHandler<InputData> OnInput;
        private static InputManager _instance;

        public static InputManager Instance {
            get {
                if (_instance == null) {
                    _instance = FindAnyObjectByType<InputManager>();
                    if (_instance == null) {
                        GameObject go = new GameObject("InputManager");
                        _instance = go.AddComponent<InputManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        public void Initialized() {
            Debug.Log("just Initialized.");
        }

        void Update() {
            InputData inputData = new InputData {
                // todo: 새 입력 시스템 적용 필요.
                forwardInput = Input.GetAxisRaw("Vertical"), // Vertical 키 (W/S 또는 방향키 위/아래)
                rotationInput = Input.GetAxisRaw("Horizontal"), // Horizontal 키 (A/D 또는 방향키 좌/우)
                strafeInput = 0f, // Input.GetAxisRaw("Horizontal Strafe"), // 별도의 좌우 평행 이동 키 설정 필요
                attackPressed = Input.GetKeyDown(KeyCode.Space),
                dashPressed = Input.GetKeyDown(KeyCode.LeftShift)
            };
            OnInput?.Invoke(this, inputData);
        }
    }
}