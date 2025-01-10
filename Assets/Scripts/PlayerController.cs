using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, EclipseProtocol.IPlayerActions
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpHeight = 2f;
    public float rollSpeed = 7f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraSensitivity = 1f;

    private CharacterController characterController;
    private Animator animator;
    private Vector3 velocity;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool isRolling = false;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private EclipseProtocol controls;
    private Vector3 cameraOffset;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        controls = new EclipseProtocol();

        // Gestion des contrôles
        controls.Player.Look.performed += ctx => OnLook(ctx);
        controls.Player.Move.performed += ctx => OnMove(ctx);
        controls.Player.Move.canceled += ctx => OnMove(ctx);
        controls.Player.Jump.performed += ctx => OnJump(ctx);
        controls.Player.Crouch.performed += ctx => OnCrouch(ctx);
        controls.Player.Roll.performed += ctx => OnRoll(ctx);
        controls.Player.Fire.performed += ctx => OnFire(ctx);
        controls.Player.Run.performed += ctx => OnRun(ctx);
        controls.Player.Run.canceled += ctx => OnRun(ctx);

        cameraOffset = cameraTransform.position - transform.position;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            movementInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            movementInput = Vector2.zero;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("isJumping");
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
        animator.SetBool("isSprinting", isSprinting);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;
        animator.SetBool("isCrouching", isCrouching);
        characterController.height = isCrouching ? 1f : 2f;
        Debug.Log(isCrouching ? "Crouching" : "Standing");
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded && !isCrouching)
        {
            Vector3 rollDirection = new Vector3(movementInput.x, 0, movementInput.y);
            if (rollDirection.magnitude > 0.1f)
            {
                isRolling = true;
                animator.SetTrigger("isRolling");
                characterController.Move(rollDirection.normalized * rollSpeed * Time.deltaTime);
                isRolling = false;
            }
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Fire action triggered");
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;
            animator.SetBool("isRunning", true);
        }
        else if (context.canceled)
        {
            isSprinting = false;
            animator.SetBool("isRunning", false);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleAnimations();
        HandleCamera();
    }

    private void HandleMovement()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
            move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
            move.y = 0f;

            if (movementInput.y < 0)
            {
                move = -cameraTransform.forward * Mathf.Abs(movementInput.y);
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
            characterController.Move(move.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleAnimations()
    {
        if (animator != null)
        {
            float smoothTime = 0.1f;

            float currentPosX = animator.GetFloat("PosX");
            float currentPosY = animator.GetFloat("PosY");

            animator.SetFloat("PosX", Mathf.Lerp(currentPosX, movementInput.x, smoothTime));
            animator.SetFloat("PosY", Mathf.Lerp(currentPosY, movementInput.y, smoothTime));

            animator.SetBool("isCrouching", isCrouching);
            animator.SetBool("isSprinting", isSprinting);
            animator.SetBool("isJumping", !characterController.isGrounded);
            animator.SetBool("isRunning", movementInput.magnitude > 0.1f && !isSprinting);
        }
    }

    private void HandleCamera()
    {
        float horizontal = lookInput.x * cameraSensitivity;
        float vertical = lookInput.y * cameraSensitivity;

        cameraOffset = Quaternion.AngleAxis(horizontal, Vector3.up) * cameraOffset;

        Vector3 newPosition = transform.position + cameraOffset;
        cameraTransform.position = Vector3.Slerp(cameraTransform.position, newPosition, Time.deltaTime * 5f);
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}
