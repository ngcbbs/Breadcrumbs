using UnityEngine;
using UnityEngine.InputSystem;

namespace day4_scrap {
    public interface IInteractable
    {
        void Interact();
    }
    
    // Input Action 자동 생성 클래스 활용을 위한 인터페이스
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private int maxJumpCount = 2;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactionMask;

    // Components
    private CharacterController controller;
    private PlayerInput playerInput;
    private Camera playerCamera;
    
    // Movement variables
    private Vector3 moveDirection;
    private Vector3 velocity;
    private int currentJumpCount;
    private bool isGrounded;

    // Input actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction lookAction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerCamera = Camera.main;

        // Input actions 설정
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];
        lookAction = playerInput.actions["Look"];

        // 초기 설정
        currentJumpCount = maxJumpCount;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        jumpAction.performed += OnJump;
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        interactAction.performed -= OnInteract;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        // 이동 입력 받기
        Vector2 input = moveAction.ReadValue<Vector2>();
        
        // 카메라 기준으로 이동 방향 계산
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * input.y + right * input.x).normalized;
        
        // 이동 적용
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        
        // 좌우 회전만 처리 (상하 회전은 카메라에서 처리)
        transform.Rotate(Vector3.up, lookInput.x * rotationSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        // 지면 체크
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            currentJumpCount = maxJumpCount;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (currentJumpCount > 0)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            currentJumpCount--;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // Raycast로 상호작용 가능한 오브젝트 체크
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            interactable?.Interact();
        }
    }
}
}