# 입력 시스템 예제 코드

이 파일은 플레이어 입력 처리 시스템의 핵심 구현을 보여줍니다. Unity의 Input System 패키지를 활용하여 다양한 입력 장치를 지원하며, 네트워크 동기화를 위한 입력 상태 캡처도 구현합니다.

## PlayerInputHandler.cs

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    // 입력 값
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isJumpPressed;
    private bool isSprintPressed;
    private bool isAttackPressed;
    private bool isInteractPressed;
    
    // Unity Input System의 Action 처리
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        playerController.SetMovementInput(moveInput);
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        playerController.SetLookInput(lookInput);
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        // 누른 순간에만 점프 처리
        if (context.performed)
        {
            isJumpPressed = true;
            playerController.Jump();
        }
        else if (context.canceled)
        {
            isJumpPressed = false;
        }
    }
    
    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprintPressed = context.ReadValueAsButton();
        playerController.SetSprinting(isSprintPressed);
    }
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isAttackPressed = true;
            playerController.Attack();
        }
        else if (context.canceled)
        {
            isAttackPressed = false;
        }
    }
    
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isInteractPressed = true;
            playerController.Interact();
        }
        else if (context.canceled)
        {
            isInteractPressed = false;
        }
    }
    
    // 입력 상태 캡처 (네트워크 전송용)
    public InputState CaptureInputState()
    {
        return new InputState
        {
            MoveDirection = moveInput,
            LookDirection = lookInput,
            IsJumping = isJumpPressed,
            IsSprinting = isSprintPressed,
            IsAttacking = isAttackPressed,
            IsInteracting = isInteractPressed
        };
    }
}
```

## InputState.cs

```csharp
using UnityEngine;
using MessagePack;

// 네트워크 전송을 위한 직렬화 가능한 입력 상태
[MessagePackObject]
public struct InputState
{
    [Key(0)]
    public Vector2 MoveDirection;
    
    [Key(1)]
    public Vector2 LookDirection;
    
    [Key(2)]
    public bool IsJumping;
    
    [Key(3)]
    public bool IsSprinting;
    
    [Key(4)]
    public bool IsAttacking;
    
    [Key(5)]
    public bool IsInteracting;
    
    [Key(6)]
    public ulong Timestamp;
}
```

## InputBuffer.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

public class InputBuffer
{
    private Queue<InputAction> buffer = new Queue<InputAction>();
    private float bufferTimeWindow = 0.2f; // 버퍼링 시간 윈도우 (초)
    
    // 버퍼에 입력 추가
    public void AddInput(InputActionType actionType)
    {
        buffer.Enqueue(new InputAction
        {
            Type = actionType,
            Timestamp = Time.time
        });
        
        // 오래된 입력 제거
        CleanupBuffer();
    }
    
    // 특정 입력이 버퍼에 있는지 확인하고 사용
    public bool ConsumeInput(InputActionType actionType)
    {
        CleanupBuffer();
        
        // 버퍼 확인
        foreach (var action in buffer)
        {
            if (action.Type == actionType)
            {
                // 입력 소비 (모든 동일 유형 입력 제거)
                RemoveActionType(actionType);
                return true;
            }
        }
        
        return false;
    }
    
    // 버퍼 정리 (오래된 입력 제거)
    private void CleanupBuffer()
    {
        while (buffer.Count > 0 && Time.time - buffer.Peek().Timestamp > bufferTimeWindow)
        {
            buffer.Dequeue();
        }
    }
    
    // 특정 타입의 모든 액션 제거
    private void RemoveActionType(InputActionType actionType)
    {
        Queue<InputAction> newBuffer = new Queue<InputAction>();
        
        foreach (var action in buffer)
        {
            if (action.Type != actionType)
            {
                newBuffer.Enqueue(action);
            }
        }
        
        buffer = newBuffer;
    }
    
    // 버퍼 초기화
    public void ClearBuffer()
    {
        buffer.Clear();
    }
}

// 입력 액션 구조체
public struct InputAction
{
    public InputActionType Type;
    public float Timestamp;
}

// 입력 액션 타입 열거형
public enum InputActionType
{
    Jump,
    Attack,
    Dash,
    Block,
    Skill1,
    Skill2,
    Skill3,
    Skill4,
    Interact
}
```

## PlayerController.cs (입력 처리 관련 부분)

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 입력 버퍼
    private InputBuffer inputBuffer = new InputBuffer();
    
    // 이동 관련 변수
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isSprinting;
    
    // 캐릭터 컴포넌트
    private CharacterController characterController;
    private Camera playerCamera;
    
    // 이동 설정
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;
    
    // 상태 변수
    private Vector3 playerVelocity;
    private bool isGrounded;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }
    
    private void Update()
    {
        // 접지 상태 체크
        isGrounded = characterController.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        
        // 이동 처리
        Move();
        
        // 중력 적용
        ApplyGravity();
    }
    
    // 이동 입력 설정
    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }
    
    // 시선 입력 설정
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }
    
    // 달리기 상태 설정
    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }
    
    // 점프 처리
    public void Jump()
    {
        inputBuffer.AddInput(InputActionType.Jump);
    }
    
    // 공격 처리
    public void Attack()
    {
        inputBuffer.AddInput(InputActionType.Attack);
    }
    
    // 상호작용 처리
    public void Interact()
    {
        inputBuffer.AddInput(InputActionType.Interact);
    }
    
    // 이동 적용
    private void Move()
    {
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        // 카메라 기준 이동 방향 계산
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        right.Normalize();
        
        Vector3 moveDirection = right * movementInput.x + forward * movementInput.y;
        
        // 이동 적용
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        
        // 회전 처리 (이동 방향으로 부드럽게 회전)
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 입력 버퍼에서 점프 확인 및 처리
        if (isGrounded && inputBuffer.ConsumeInput(InputActionType.Jump))
        {
            playerVelocity.y += Mathf.Sqrt(jumpForce * -3.0f * gravityValue);
        }
        
        // 공격 버퍼 처리
        if (inputBuffer.ConsumeInput(InputActionType.Attack))
        {
            PerformAttack();
        }
        
        // 상호작용 버퍼 처리
        if (inputBuffer.ConsumeInput(InputActionType.Interact))
        {
            PerformInteraction();
        }
    }
    
    // 중력 적용
    private void ApplyGravity()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }
    
    // 공격 수행
    private void PerformAttack()
    {
        // 전투 시스템에 공격 요청
        GetComponent<CombatSystem>()?.TryAttack();
    }
    
    // 상호작용 수행
    private void PerformInteraction()
    {
        // 상호작용 가능한 오브젝트 탐색 및 상호작용
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Interactable"));
        
        if (colliders.Length > 0)
        {
            // 가장 가까운 상호작용 오브젝트 찾기
            float closestDistance = float.MaxValue;
            IInteractable closestInteractable = null;
            
            foreach (var collider in colliders)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }
            
            // 상호작용 실행
            closestInteractable?.Interact(gameObject);
        }
    }
}
```

## PlayerInputActions.inputactions (Unity Input System 액션 맵)

```json
{
    "name": "PlayerInputActions",
    "maps": [
        {
            "name": "Player",
            "id": "f62a4b92-ef5e-4175-8f4c-c9075429d32c",
            "actions": [
                {
                    "name": "Move",
                    "type": "Value",
                    "id": "6bc1aaf4-b110-4ff7-891e-5b9fe6f32c4d",
                    "expectedControlType": "Vector2",
                    "processors": "",
                    "interactions": "",
                    "initialStateCheck": true
                },
                {
                    "name": "Look",
                    "type": "Value",
                    "id": "2690c379-f54d-45be-a724-414123833eb4",
                    "expectedControlType": "Vector2",
                    "processors": "",
                    "interactions": "",
                    "initialStateCheck": true
                },
                {
                    "name": "Jump",
                    "type": "Button",
                    "id": "8c4abdf8-4099-493a-aa1a-129acec7c3df",
                    "expectedControlType": "Button",
                    "processors": "",
                    "interactions": "",
                    "initialStateCheck": false
                },
                {
                    "name": "Sprint",
                    "type": "Button",
                    "id": "980e881e-182c-404c-8cbf-3d09fdb48fef",
                    "expectedControlType": "Button",
                    "processors": "",
                    "interactions": "",
                    "initialStateCheck": false
                },
                {
                    "name": "Attack",
                    "type": "Button",
                    "id": "4a5c2826-3326-4614-95f6-040be0df930e",
                    "expectedControlType": "Button",
                    "processors": "",
                    "interactions": "",
                    "initialStateCheck": false
                },
                {
                    "name": "Interact",
                    "type": "Button",
                    "id": "5863a44a-f5ab-4416-b9fd-a75769ce372b",
                    "expectedControlType": "Button",
                    "processors": "",
                    "interactions": "",
                    "initialStateCheck": false
                }
            ],
            "bindings": [
                {
                    "name": "WASD",
                    "id": "99c18e75-bbf1-4cf7-b037-a324bf786de3",
                    "path": "2DVector",
                    "interactions": "",
                    "processors": "",
                    "groups": "",
                    "action": "Move",
                    "isComposite": true,
                    "isPartOfComposite": false
                },
                {
                    "name": "up",
                    "id": "d74b3d5c-a8c9-4a1f-9c76-46e8c4566778",
                    "path": "<Keyboard>/w",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Move",
                    "isComposite": false,
                    "isPartOfComposite": true
                },
                {
                    "name": "down",
                    "id": "78f5b34a-2e43-48a1-88b4-5bbc3c62eaaf",
                    "path": "<Keyboard>/s",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Move",
                    "isComposite": false,
                    "isPartOfComposite": true
                },
                {
                    "name": "left",
                    "id": "45a08456-236a-462e-88a3-84b63df59225",
                    "path": "<Keyboard>/a",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Move",
                    "isComposite": false,
                    "isPartOfComposite": true
                },
                {
                    "name": "right",
                    "id": "0c6a5175-1f29-4de3-a6e5-151fe53b6a47",
                    "path": "<Keyboard>/d",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Move",
                    "isComposite": false,
                    "isPartOfComposite": true
                },
                {
                    "name": "",
                    "id": "978bfe49-cc26-4a3d-ab7b-7d7a29327403",
                    "path": "<Gamepad>/leftStick",
                    "interactions": "",
                    "processors": "",
                    "groups": "Gamepad",
                    "action": "Move",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "c1f7a91b-d0fd-4a62-997e-7fb9b69bf235",
                    "path": "<Gamepad>/rightStick",
                    "interactions": "",
                    "processors": "",
                    "groups": "Gamepad",
                    "action": "Look",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "8c8e490b-c610-4785-884f-f04217b23ca4",
                    "path": "<Mouse>/delta",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Look",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "143bb1cd-cc10-4eca-a2f0-a3664166fe91",
                    "path": "<Gamepad>/buttonSouth",
                    "interactions": "",
                    "processors": "",
                    "groups": "Gamepad",
                    "action": "Jump",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "05f6913d-c316-48b2-a6bb-e225f14c7960",
                    "path": "<Keyboard>/space",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Jump",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "8e25a5e5-37e7-4c7c-b29d-ef3833d2c16f",
                    "path": "<Keyboard>/leftShift",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Sprint",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "541d7374-39b8-421a-a9a5-0f1c87c61d7d",
                    "path": "<Gamepad>/leftTrigger",
                    "interactions": "",
                    "processors": "",
                    "groups": "Gamepad",
                    "action": "Sprint",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "29c0fc9f-e467-4e9a-a14f-d78ca257ded6",
                    "path": "<Mouse>/leftButton",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Attack",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "61d2fe99-8d78-4b28-8a62-a3e9e3b2f3b0",
                    "path": "<Gamepad>/rightShoulder",
                    "interactions": "",
                    "processors": "",
                    "groups": "Gamepad",
                    "action": "Attack",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "0c40ad3e-42d3-4e2c-9a1b-4d10f5cbf9a6",
                    "path": "<Keyboard>/e",
                    "interactions": "",
                    "processors": "",
                    "groups": "Keyboard&Mouse",
                    "action": "Interact",
                    "isComposite": false,
                    "isPartOfComposite": false
                },
                {
                    "name": "",
                    "id": "42b14004-d2e0-4a01-b6a3-5a43fe9e8ed6",
                    "path": "<Gamepad>/buttonWest",
                    "interactions": "",
                    "processors": "",
                    "groups": "Gamepad",
                    "action": "Interact",
                    "isComposite": false,
                    "isPartOfComposite": false
                }
            ]
        }
    ],
    "controlSchemes": [
        {
            "name": "Keyboard&Mouse",
            "bindingGroup": "Keyboard&Mouse",
            "devices": [
                {
                    "devicePath": "<Keyboard>",
                    "isOptional": false,
                    "isOR": false
                },
                {
                    "devicePath": "<Mouse>",
                    "isOptional": false,
                    "isOR": false
                }
            ]
        },
        {
            "name": "Gamepad",
            "bindingGroup": "Gamepad",
            "devices": [
                {
                    "devicePath": "<Gamepad>",
                    "isOptional": false,
                    "isOR": false
                }
            ]
        }
    ]
}
```

## IInteractable.cs (상호작용 인터페이스)

```csharp
public interface IInteractable
{
    string GetInteractionPrompt();
    bool CanInteract(GameObject actor);
    void Interact(GameObject actor);
}
```
