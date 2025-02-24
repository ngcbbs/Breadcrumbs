using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace day5_scrap {
    // 입력 액션 에셋을 위한 클래스
    public class PlayerInputActions : IInputActionCollection2, IDisposable {
        /// <summary>
        /// Provides access to the underlying asset instance.
        /// </summary>
        public InputActionAsset asset { get; }

        public InputAction Movement { get; }
        public InputAction Jump { get; }
        public InputAction Dash { get; }
        public InputAction Attack { get; }
        public InputAction LockOn { get; }
        public InputAction CameraLook { get; }

        public PlayerInputActions() {
            asset = InputActionAsset.FromJson(@"{
            ""name"": ""PlayerInputActions"",
            ""maps"": [
                {
                    ""name"": ""Player"",
                    ""id"": ""playerMap"",
                    ""actions"": [
                        {
                            ""name"": ""Movement"",
                            ""type"": ""Value"",
                            ""id"": ""movement"",
                            ""expectedControlType"": ""Vector2"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""Jump"",
                            ""type"": ""Button"",
                            ""id"": ""jump"",
                            ""expectedControlType"": ""Button"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""Dash"",
                            ""type"": ""Button"",
                            ""id"": ""dash"",
                            ""expectedControlType"": ""Button"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""Attack"",
                            ""type"": ""Button"",
                            ""id"": ""attack"",
                            ""expectedControlType"": ""Button"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""LockOn"",
                            ""type"": ""Button"",
                            ""id"": ""lockOn"",
                            ""expectedControlType"": ""Button"",
                            ""processors"": """",
                            ""interactions"": """"
                        },
                        {
                            ""name"": ""CameraLook"",
                            ""type"": ""Value"",
                            ""id"": ""cameraLook"",
                            ""expectedControlType"": ""Vector2"",
                            ""processors"": """",
                            ""interactions"": """"
                        }
                    ],
                    ""bindings"": [
                        {
                            ""name"": ""WASD"",
                            ""id"": ""wasdBinding"",
                            ""path"": ""2DVector"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Movement"",
                            ""isComposite"": true,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": ""up"",
                            ""id"": ""wasdUp"",
                            ""path"": ""<Keyboard>/w"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Movement"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": ""down"",
                            ""id"": ""wasdDown"",
                            ""path"": ""<Keyboard>/s"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Movement"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": ""left"",
                            ""id"": ""wasdLeft"",
                            ""path"": ""<Keyboard>/a"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Movement"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": ""right"",
                            ""id"": ""wasdRight"",
                            ""path"": ""<Keyboard>/d"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Movement"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""name"": """",
                            ""id"": ""jumpBinding"",
                            ""path"": ""<Keyboard>/space"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Jump"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": """",
                            ""id"": ""dashBinding"",
                            ""path"": ""<Keyboard>/leftShift"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Dash"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": """",
                            ""id"": ""attackBinding"",
                            ""path"": ""<Mouse>/leftButton"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""Attack"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": """",
                            ""id"": ""lockOnBinding"",
                            ""path"": ""<Keyboard>/q"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""LockOn"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        },
                        {
                            ""name"": """",
                            ""id"": ""cameraBinding"",
                            ""path"": ""<Mouse>/delta"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""groups"": """",
                            ""action"": ""CameraLook"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": false
                        }
                    ]
                }
            ]
        }"
        );

            var playerMap = asset.FindActionMap("Player");
            Movement = playerMap.FindAction("Movement");
            Jump = playerMap.FindAction("Jump");
            Dash = playerMap.FindAction("Dash");
            Attack = playerMap.FindAction("Attack");
            LockOn = playerMap.FindAction("LockOn");
            CameraLook = playerMap.FindAction("CameraLook");

            
        }

        

        ~PlayerInputActions() { }

        /// <summary>
        /// Destroys this asset and all associated <see cref="InputAction"/> instances.
        /// </summary>
        public void Dispose() {
            UnityEngine.Object.Destroy(asset);
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.bindingMask" />
        public InputBinding? bindingMask {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.devices" />
        public ReadOnlyArray<InputDevice>? devices {
            get => asset.devices;
            set => asset.devices = value;
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.controlSchemes" />
        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.Contains(InputAction)" />
        public bool Contains(InputAction action) {
            return asset.Contains(action);
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.GetEnumerator()" />
        public IEnumerator<InputAction> GetEnumerator() {
            return asset.GetEnumerator();
        }

        /// <inheritdoc cref="IEnumerable.GetEnumerator()" />
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.Enable()" />
        public void Enable() {
            asset.Enable();
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.Disable()" />
        public void Disable() {
            asset.Disable();
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.bindings" />
        public IEnumerable<InputBinding> bindings => asset.bindings;

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.FindAction(string, bool)" />
        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false) {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        /// <inheritdoc cref="UnityEngine.InputSystem.InputActionAsset.FindBinding(InputBinding, out InputAction)" />
        public int FindBinding(InputBinding bindingMask, out InputAction action) {
            return asset.FindBinding(bindingMask, out action);
        }
    }
}