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
    public float groundYOffset = 0.1f;
    public LayerMask groundMask;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraSensitivity = 1f;
    public float aimFOV = 40f;
    public float normalFOV = 60f;
    public float aimSmoothSpeed = 10f;
    public Vector3 aimOffset = new Vector3(0, 1.5f, -2f);
    public float minVerticalAngle = -60f;
    public float maxVerticalAngle = 60f;

    [Header("VFX Settings")]
    public ParticleSystem sprintVFX; // Reference to the sprint effect
    public ParticleSystem jumpVFX;

    private CharacterController characterController;
    private StateManager stateManager;
    private Animator animator;
    private Vector3 velocity;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private EclipseProtocol controls;
    private Vector3 cameraOffset;
    private float verticalAngle = 0f;
    private bool isRunning, isSprinting, isCrouching, jumpInput;

    // Fixed missing variables
    private bool isRolling = false;
    private bool isAiming = false;
    private InventoryManager inventoryManager;

    private int currentJumpCount = 0;
    private PlayerStats playerStats;
    private BoostItem nearbyBoostItem;
    private UIManager uiManager;
    public GameObject inventoryPanel;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        uiManager = FindObjectOfType<UIManager>();

        controls = new EclipseProtocol();
        stateManager = new StateManager();

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
        controls.Player.Inventory.performed += OnInventory;
        controls.Player.Aim.performed += ctx => OnAim(ctx);
        controls.Player.Aim.canceled += ctx => OnAim(ctx);

        cameraOffset = cameraTransform.position - transform.position;
    }

    private void OnEnable() => controls.Enable();

    private void OnDisable() => controls.Disable();

    private bool IsGrounded()
{
    // Raycast pour vérifier si le joueur est proche du sol
    Vector3 groundCheck = transform.position + Vector3.down * groundYOffset;
    return Physics.CheckSphere(groundCheck, 0.2f, groundMask);
}

    public void OnAim(InputAction.CallbackContext context)
    {
        isAiming = context.performed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
        movementInput = context.ReadValue<Vector2>();
        }
    else if (context.canceled)
       {
        // Reset input when movement is canceled
        movementInput = Vector2.zero;
       }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.performed;

            if (jumpVFX != null)
    {
        if (jumpInput)
        {
            jumpVFX.Play(); // Start the VFX
        }
        else
        {
            jumpVFX.Stop(); // Stop the VFX
        }
    }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;

        if (sprintVFX != null)
    {
        if (isSprinting)
        {
            sprintVFX.Play(); // Start the VFX
        }
        else
        {
            sprintVFX.Stop(); // Stop the VFX
        }
    }
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

    public void OnInventory(InputAction.CallbackContext context)
    {
        ToggleInventory();
    }

    private void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
        else
        {
            Debug.LogError("InventoryPanel is not assigned in the Inspector!");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && nearbyBoostItem != null)
        {
            InteractWithBoostItem();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleGravityAndJump();
        HandleCamera();
        HandleStamina();
        HandleAnimations();

        if (nearbyBoostItem != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            InteractWithBoostItem();
        }
    }

    /// <summary>
/// Dynamically adjusts animator layers between Locomotion and Shooting.
/// </summary>
private void HandleLayerSwitching()
{
    if (animator != null)
    {
        if (isAiming)
        {
            // Activate the Shooting layer
            animator.SetLayerWeight(animator.GetLayerIndex("Shooting"), 1f);
            animator.SetLayerWeight(animator.GetLayerIndex("Base Layer"), 0f);
        }
        else
        {
            // Revert to the Base Layer
            animator.SetLayerWeight(animator.GetLayerIndex("Shooting"), 0f);
            animator.SetLayerWeight(animator.GetLayerIndex("Base Layer"), 1f);
        }
    }
}

    private void HandleStamina()
    {
        stateManager.UpdateStamina(playerStats);

        if (playerStats.currentStamina <= 0)
        {
            stateManager.ForceStopSprinting();
        }
    }

    private void HandleMovement()
    {
            if (movementInput.magnitude > 0.1f) // Only move if there's meaningful input
    {
        // Calculate movement direction relative to the camera
        Vector3 moveDirection = cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x;
        moveDirection.y = 0f; // Ignore vertical movement to keep movement on a flat plane
        moveDirection.Normalize();

        // Rotate the player to face the movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Smooth rotation

        // Calculate speed based on the player's state
        float currentSpeed = stateManager.GetCurrentSpeed(walkSpeed, runSpeed, sprintSpeed);

        // Apply movement
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }
    }

    private void HandleGravityAndJump()
    {
            if (IsGrounded())
    {
        velocity.y = -2f; // Empêche l'accumulation de gravité
        currentJumpCount = 0; // Réinitialise le compteur de sauts

        if (jumpInput)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Calcul de la force de saut
            currentJumpCount++;
            jumpInput = false; // Réinitialise l'entrée de saut

            // Joue le VFX et l'animation
            if (jumpVFX != null) jumpVFX.Play();
            if (animator != null) animator.SetTrigger("Jump");
        }

        // Réinitialise les paramètres d'animation au sol
        animator.SetBool("isFalling", false);
        animator.SetBool("isLanding", true);
    }
    else
    {
        // Applique la gravité en l'air
        velocity.y += gravity * Time.deltaTime;

        // Gère les états de saut et de chute
        animator.SetBool("isFalling", velocity.y < 0);
        animator.SetBool("isLanding", false);

        // Empêche le double saut
        if (currentJumpCount >= 1)
        {
            jumpInput = false;
        }
    }

    // Applique le mouvement vertical
    characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleCamera()
    {
            // Ensure lookInput values are valid before applying rotation
    if (lookInput.sqrMagnitude > 0.01f) // Apply rotation only if there's significant input
    {
        // Horizontal rotation (Yaw)
        float horizontal = lookInput.x * cameraSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, horizontal);

        // Vertical rotation (Pitch)
        float vertical = lookInput.y * cameraSensitivity * Time.deltaTime;
        verticalAngle = Mathf.Clamp(verticalAngle - vertical, minVerticalAngle, maxVerticalAngle);

        // Apply vertical rotation to the camera
        Quaternion cameraRotation = Quaternion.Euler(verticalAngle, transform.eulerAngles.y, 0);
        cameraTransform.rotation = cameraRotation;
    }

    // Smoothly adjust FOV for aiming
    float targetFOV = isAiming ? aimFOV : normalFOV;
    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime * aimSmoothSpeed);

    // Adjust camera position for aiming
    Vector3 targetPosition = isAiming ? transform.position + aimOffset : transform.position + cameraOffset;
    cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * aimSmoothSpeed);
    }

    private void HandleAnimations()
    {
    if (animator != null)
    {
        // Normaliser l'entrée pour éviter des valeurs inattendues
        Vector2 normalizedInput = movementInput.magnitude > 0.1f ? movementInput.normalized : Vector2.zero;

        // Mise à jour des paramètres du Blend Tree
        animator.SetFloat("PosX", normalizedInput.x);
        animator.SetFloat("PosY", normalizedInput.y);

        // Gérer les états "Jumping", "Falling", et "Landing"
        if (IsGrounded())
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);

            if (animator.GetBool("isLanding"))
            {
                // Réinitialiser après l'atterrissage
                animator.SetBool("isLanding", false);
            }
        }
        else
        {
            animator.SetBool("isJumping", velocity.y > 0);
            animator.SetBool("isFalling", velocity.y < 0);

            // Activer "Landing" uniquement si la chute est terminée
            if (velocity.y <= 0 && !IsGrounded())
            {
                animator.SetBool("isLanding", true);
            }
        }

        // Mise à jour des autres états d'animation
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isSprinting", isSprinting);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isRolling", isRolling);
    }
    }

    private void InteractWithBoostItem()
    {
        if (nearbyBoostItem != null)
        {
            nearbyBoostItem.ApplyBoost(playerStats, this);
            Destroy(nearbyBoostItem.gameObject);
            nearbyBoostItem = null;
            uiManager?.ToggleInteractionPrompt(false);
        }
    }

public class StateManager
{
    private bool isRunning;
    private bool isSprinting;
    private bool isAiming;

    public void SetRunning(bool value) => isRunning = value;

    public void SetSprinting(bool value) => isSprinting = value;

    public void SetAiming(bool value) => isAiming = value;

    public float GetCurrentSpeed(float walkSpeed, float runSpeed, float sprintSpeed)
    {
        if (isSprinting) return sprintSpeed;
        if (isRunning) return runSpeed;
        return walkSpeed;
    }

    public void UpdateStamina(PlayerStats playerStats)
    {
        if (playerStats.currentStamina <= 0)
        {
            ForceStopSprinting();
        }
    }

    public void ForceStopSprinting()
    {
        isRunning = false;
        isSprinting = false;
    }
}
}
