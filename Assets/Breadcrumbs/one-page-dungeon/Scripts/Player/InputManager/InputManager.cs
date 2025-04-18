using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Breadcrumbs.Player {
    [Serializable]
    public class InputBindingSaveData {
        public string actionName;
        public string bindingId;
        public string overridePath;
    }

    [Serializable]
    public class InputBindingsSaveData {
        public List<InputBindingSaveData> bindings = new List<InputBindingSaveData>();
    }

    public class InputManager : MonoBehaviour {
        public static event EventHandler<InputData> OnInput;
        private static InputManager _instance;

        [SerializeField] private bool loadBindingOverridesOnStart = true;
        [SerializeField] private string playerPrefsKey = "InputBindings";

        // 입력 시스템 관련 변수들
        private PlayerInputActions playerInputActions;
        private InputAction moveAction;
        private InputAction attackAction;
        private InputAction dashAction;

        private float forwardInput;
        private float rotationInput;
        private float strafeInput;
        private bool attackPressed;
        private bool dashPressed;

        // 리바인딩 관련 변수
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        
        // 플레이어 컨트롤러
        private PlayerController _playerController;

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

        private void Awake() {
            // 새 입력 시스템 초기화
            playerInputActions = new PlayerInputActions();

            if (loadBindingOverridesOnStart) {
                LoadBindingOverrides();
            }
        }

        private void OnEnable() {
            // 입력 액션들 활성화 및 이벤트 구독
            moveAction = playerInputActions.Player.Move;
            attackAction = playerInputActions.Player.Attack;
            dashAction = playerInputActions.Player.Dash;

            moveAction.Enable();
            attackAction.Enable();
            dashAction.Enable();

            // 버튼 액션에 대한 콜백 등록
            attackAction.performed += OnAttack;
            dashAction.performed += OnDash;
        }

        private void OnDisable() {
            // 리바인딩 작업 취소
            CancelRebind();

            // 입력 액션들 비활성화 및 이벤트 구독 해제
            moveAction.Disable();
            attackAction.Disable();
            dashAction.Disable();

            attackAction.performed -= OnAttack;
            dashAction.performed -= OnDash;
        }

        public void Initialized(PlayerController controller) {
            Debug.Log("InputManager initialized.");
            _playerController = controller;
        }

        private void OnAttack(InputAction.CallbackContext context) {
            attackPressed = context.performed;
        }

        private void OnDash(InputAction.CallbackContext context) {
            dashPressed = context.performed;
        }

        void Update() {
            // 이동 입력 값 가져오기
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            
            // 입력 데이터 구성
            InputData inputData = new InputData {
                forwardInput = moveInput.y,       // 전진/후진 (W/S)
                rotationInput = moveInput.x,      // 회전 (A/D)
                strafeInput = 0f,                // 필요한 경우 별도의 스트레이프 입력 추가 가능
                attackPressed = attackPressed,
                dashPressed = dashPressed
            };

            // 한 프레임에만 적용되도록 리셋
            attackPressed = false;
            dashPressed = false;

            // 이벤트 발생
            OnInput?.Invoke(this, inputData);
        }

        #region 키 리바인딩

        /// <summary>
        /// 지정된 액션의 특정 바인딩을 리바인딩 시작
        /// </summary>
        /// <param name="actionName">액션 이름 ("Move", "Attack", "Dash")</param>
        /// <param name="bindingIndex">바인딩 인덱스</param>
        /// <param name="onComplete">리바인딩 완료 시 콜백</param>
        /// <param name="onCancel">리바인딩 취소 시 콜백</param>
        public void StartRebinding(string actionName, int bindingIndex, Action onComplete = null, Action onCancel = null) {
            InputAction action = GetActionByName(actionName);
            
            if (action == null) {
                Debug.LogError($"Action with name {actionName} not found!");
                return;
            }

            // 진행 중인 리바인딩이 있으면 취소
            CancelRebind();

            // 액션 비활성화
            action.Disable();
            
            // 리바인딩 작업 시작
            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(operation => {
                    // 리바인딩 완료 후 액션 다시 활성화
                    action.Enable();
                    rebindOperation.Dispose();
                    rebindOperation = null;
                    SaveBindingOverrides();
                    onComplete?.Invoke();
                })
                .OnCancel(operation => {
                    // 리바인딩 취소 시 액션 다시 활성화
                    action.Enable();
                    rebindOperation.Dispose();
                    rebindOperation = null;
                    onCancel?.Invoke();
                })
                .Start();
        }

        /// <summary>
        /// 진행 중인 리바인딩 작업 취소
        /// </summary>
        public void CancelRebind() {
            if (rebindOperation != null) {
                rebindOperation.Cancel();
                rebindOperation = null;
            }
        }

        /// <summary>
        /// 특정 바인딩을 기본값으로 초기화
        /// </summary>
        /// <param name="actionName">액션 이름</param>
        /// <param name="bindingIndex">바인딩 인덱스</param>
        public void ResetBinding(string actionName, int bindingIndex) {
            InputAction action = GetActionByName(actionName);
            
            if (action == null) {
                Debug.LogError($"Action with name {actionName} not found!");
                return;
            }

            // 바인딩 초기화
            action.RemoveBindingOverride(bindingIndex);
            SaveBindingOverrides();
        }

        /// <summary>
        /// 모든 바인딩을 기본값으로 초기화
        /// </summary>
        public void ResetAllBindings() {
            foreach (var actionMap in playerInputActions.asset.actionMaps) {
                foreach (var action in actionMap.actions) {
                    for (int i = 0; i < action.bindings.Count; i++) {
                        action.RemoveBindingOverride(i);
                    }
                }
            }
            SaveBindingOverrides();
        }

        /// <summary>
        /// 액션 이름으로 InputAction 가져오기
        /// </summary>
        private InputAction GetActionByName(string actionName) {
            switch (actionName) {
                case "Move": return moveAction;
                case "Attack": return attackAction;
                case "Dash": return dashAction;
                default: return null;
            }
        }

        /// <summary>
        /// 바인딩 디스플레이 이름 가져오기
        /// </summary>
        public string GetBindingDisplayString(string actionName, int bindingIndex) {
            InputAction action = GetActionByName(actionName);
            
            if (action == null) {
                Debug.LogError($"Action with name {actionName} not found!");
                return string.Empty;
            }

            return action.GetBindingDisplayString(bindingIndex);
        }

        /// <summary>
        /// 특정 액션의 바인딩 개수 반환
        /// </summary>
        public int GetBindingCount(string actionName) {
            InputAction action = GetActionByName(actionName);
            
            if (action == null) {
                Debug.LogError($"Action with name {actionName} not found!");
                return 0;
            }

            return action.bindings.Count;
        }

        #endregion

        #region 바인딩 저장 및 로드

        /// <summary>
        /// 현재 바인딩 설정을 PlayerPrefs에 저장
        /// </summary>
        public void SaveBindingOverrides() {
            var saveData = new InputBindingsSaveData();

            foreach (var actionMap in playerInputActions.asset.actionMaps) {
                foreach (var action in actionMap.actions) {
                    for (int i = 0; i < action.bindings.Count; i++) {
                        var binding = action.bindings[i];
                        if (!string.IsNullOrEmpty(binding.overridePath)) {
                            saveData.bindings.Add(new InputBindingSaveData {
                                actionName = action.name,
                                bindingId = binding.id.ToString(),
                                overridePath = binding.overridePath
                            });
                        }
                    }
                }
            }

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(playerPrefsKey, json);
            PlayerPrefs.Save();
            Debug.Log("Input bindings saved.");
        }

        /// <summary>
        /// PlayerPrefs에서 바인딩 설정 로드
        /// </summary>
        public void LoadBindingOverrides() {
            if (!PlayerPrefs.HasKey(playerPrefsKey)) {
                Debug.Log("No saved input bindings found.");
                return;
            }

            string json = PlayerPrefs.GetString(playerPrefsKey);
            var saveData = JsonUtility.FromJson<InputBindingsSaveData>(json);

            foreach (var bindingData in saveData.bindings) {
                foreach (var actionMap in playerInputActions.asset.actionMaps) {
                    foreach (var action in actionMap.actions) {
                        if (action.name == bindingData.actionName) {
                            for (int i = 0; i < action.bindings.Count; i++) {
                                if (action.bindings[i].id.ToString() == bindingData.bindingId) {
                                    action.ApplyBindingOverride(i, bindingData.overridePath);
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("Input bindings loaded.");
        }

        #endregion
    }
}