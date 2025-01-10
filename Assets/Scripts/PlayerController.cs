using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, EclipseProtocol.IPlayerActions
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 2f;
    public float rollSpeed = 7f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraSensitivity = 1f;

    private CharacterController characterController;
    private Animator animator;
    private Vector3 velocity;
    private bool isRunning = false;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool isRolling = false;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private EclipseProtocol controls;
    private Vector3 cameraOffset;
    private bool isLanding = false;
    private bool jumpInput = false;
    private int currentJumpCount = 0;
    private int maxJumpCount = 2;

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
        controls.Player.Sprint.performed += ctx => OnSprint(ctx);
        controls.Player.Sprint.canceled += ctx => OnSprint(ctx);

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
    if (context.performed)
    {
        jumpInput = true; // Saut demandé
    }
    else if (context.canceled)
    {
        jumpInput = false; // Saut annulé
    }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouching = !isCrouching;
            animator.SetBool("isCrouching", isCrouching);
        }
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

    private void Update()
    {
        HandleMovement();
        HandleGravityAndJump();
        HandleAnimations();
        HandleCamera();
        UpdateAnimations();
    }

    private void HandleMovement()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
            move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
            move.y = 0f;

            if (characterController.isGrounded)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            float currentSpeed = walkSpeed;

            if (isSprinting)
            {
                currentSpeed = sprintSpeed;
            }
            else if (isRunning)
            {
                currentSpeed = runSpeed;
            }

            characterController.Move(move.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleGravityAndJump()
    {
        if (characterController.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
                currentJumpCount = 0;
            
                if (animator.GetBool("isFalling"))
                {
                    animator.SetBool("isFalling", false);
                    animator.SetTrigger("isLanding");
                }
            }

            if (jumpInput)
            {
                PerformJump();
            }
        }
        else
        {
            if (jumpInput && currentJumpCount < maxJumpCount)
            {
                PerformJump();
            }

            velocity.y += gravity * Time.deltaTime;

            if (velocity.y < 0 && !animator.GetBool("isFalling"))
            {
                animator.SetBool("isFalling", true);
            }
        }

        
        characterController.Move(velocity * Time.deltaTime);
    }

    void UpdateAnimations()
    {
        animator.SetFloat("Speed", movementInput.magnitude);
    }

    void PerformJump()
    {
    if (currentJumpCount < maxJumpCount)
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.SetTrigger("Jump");
        currentJumpCount++; // Incrémente le compteur de sauts
        jumpInput = false; // Réinitialise l'entrée de saut
    }
    }

    private void HandleAnimations()
    {
        if (animator != null)
        {
            float smoothTime = 0.1f;

            animator.SetFloat("PosX", Mathf.Lerp(animator.GetFloat("PosX"), movementInput.x, smoothTime));
            animator.SetFloat("PosY", Mathf.Lerp(animator.GetFloat("PosY"), movementInput.y, smoothTime));

            animator.SetBool("isRunning", isRunning && !isSprinting);
            animator.SetBool("isSprinting", isSprinting);
            animator.SetBool("isWalking", movementInput.magnitude > 0.1f && !isRunning && !isSprinting);
            animator.SetBool("isJumping", velocity.y > 0 && !characterController.isGrounded);
            animator.SetBool("isFalling", velocity.y < 0 && !characterController.isGrounded);

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Land"))
            {
                animator.ResetTrigger("isLanding");
            }
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
