using System.Collections;
using System.Collections.Generic;
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
    public Vector3 offset = new Vector3(0, 1.5f, -3f);
    public float rotationSpeed = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    [Header("Target Settings")]
    public Transform target;

    [Header("VFX Settings")]
    public ParticleSystem sprintVFX;
    public ParticleSystem jumpVFX;

    [Header("Inventory Settings")]
    public GameObject inventoryPanel;

    private float currentZoom = 5f;
    private float yawInput = 0f;
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
    private bool isRolling = false;
    private bool isAiming = false;
    private InventoryManager inventoryManager;
    private PlayerStats playerStats;
    private BoostItem nearbyBoostItem;
    private UIManager uiManager;
    private InputAction mouseXAction;
    private InputAction mouseYAction;
    private InputAction mouseScrollAction;

    private PlayerInput playerInput; // Définition manquante ajoutée
    private int currentJumpCount = 0; // Définition manquante ajoutée

    private void Awake()
    {
                // Initialize state manager
        stateManager = new StateManager();

        // Get required components
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        playerInput = GetComponent<PlayerInput>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        uiManager = FindObjectOfType<UIManager>();

        // Validate components
        if (characterController == null)
            Debug.LogError("CharacterController component is missing!");
        if (playerStats == null)
            Debug.LogError("PlayerStats component is missing!");
        if (playerInput == null)
            Debug.LogError("PlayerInput component is missing!");

        // Initialize controls
        controls = new EclipseProtocol();
        InitializeInputActions();

        // Camera setup
        if (cameraTransform == null)
        {
            Debug.LogWarning("CameraTransform is not assigned. Using Camera.main.");
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
                Debug.LogError("Main Camera is missing!");
        }

        // Target setup
        if (target == null)
        {
            Debug.LogWarning("Target is not assigned. Using self transform.");
            target = transform;
        }

        // Calculate camera offset
        if (cameraTransform != null)
            cameraOffset = cameraTransform.position - transform.position;
    }

    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
    }

    private void InitializeInputActions()
    {
        mouseXAction = controls.Player.MouseX;
        mouseYAction = controls.Player.MouseY;
        mouseScrollAction = controls.Player.MouseScroll;

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
    }

    private bool IsGrounded()
    {
        Vector3 groundCheck = transform.position + Vector3.down * (characterController.height / 2 + groundYOffset);
        bool grounded = Physics.CheckSphere(groundCheck, 0.2f, groundMask);
        return grounded;
    }


    public void OnInteract(InputAction.CallbackContext context)
{
    // Implémentation de l'action OnInteract
    if (context.performed)
    {
        Debug.Log("Interact action triggered.");
        // Ajouter ici la logique d'interaction
    }
}

public void OnLookHorizontal(InputAction.CallbackContext context)
{
    // Implémentation de l'action OnLookHorizontal
    if (context.performed)
    {
        Debug.Log($"Horizontal Look: {context.ReadValue<float>()}");
        // Ajouter ici la logique pour la rotation horizontale
    }
}

public void OnZoom(InputAction.CallbackContext context)
{
    // Implémentation de l'action OnZoom
    if (context.performed)
    {
        Debug.Log($"Zoom: {context.ReadValue<float>()}");
        // Ajouter ici la logique pour gérer le zoom
    }
}

public void OnMouseX(InputAction.CallbackContext context)
{
    // Implémentation de l'action OnMouseX
    if (context.performed)
    {
        Debug.Log($"Mouse X Movement: {context.ReadValue<float>()}");
    }
}

public void OnMouseY(InputAction.CallbackContext context)
{
    // Implémentation de l'action OnMouseY
    if (context.performed)
    {
        Debug.Log($"Mouse Y Movement: {context.ReadValue<float>()}");
    }
}

public void OnMouseScroll(InputAction.CallbackContext context)
{
    // Implémentation de l'action OnMouseScroll
    if (context.performed)
    {
        Debug.Log($"Mouse Scroll: {context.ReadValue<float>()}");
    }
}


    public void OnAim(InputAction.CallbackContext context)
    {
        isAiming = context.performed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.performed ? context.ReadValue<Vector2>() : Vector2.zero;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
            Vector2 rawInput = context.ReadValue<Vector2>();

    // Appliquer un seuil pour éviter le bruit
    const float threshold = 0.05f;
    lookInput = rawInput.magnitude > threshold ? rawInput : Vector2.zero;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.performed;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
        if (sprintVFX != null)
        {
            if (isSprinting)
                sprintVFX.Play();
            else
                sprintVFX.Stop();
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
            animator?.SetBool("isCrouching", isCrouching);
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded() && !isCrouching)
        {
            Vector3 rollDirection = new Vector3(movementInput.x, 0, movementInput.y);
            if (rollDirection.magnitude > 0.1f)
            {
                isRolling = true;
                animator?.SetTrigger("isRolling");
                characterController.Move(rollDirection.normalized * rollSpeed * Time.deltaTime);
                isRolling = false;
            }
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
            Debug.Log("Fire action triggered");
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

    // Rotation horizontale uniquement si la souris bouge significativement
    float mouseX = mouseXAction.ReadValue<float>();
    if (Mathf.Abs(mouseX) > 0.01f) // Seuil pour éviter les erreurs de mouvement minimal
    {
        yawInput += mouseX * rotationSpeed * Time.deltaTime;
    }

    // Gestion du zoom
    float scrollInput = mouseScrollAction.ReadValue<float>();
    currentZoom = Mathf.Clamp(currentZoom - scrollInput * zoomSpeed, minZoom, maxZoom);
    }
    }

    private void LateUpdate()
    {
            // Mise à jour de la position de la caméra
    Vector3 desiredPosition = target.position + offset * currentZoom;
    cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, Time.deltaTime * aimSmoothSpeed);

    // Appliquer la rotation horizontale au joueur
    transform.rotation = Quaternion.Euler(0f, yawInput, 0f);

    // Rotation verticale de la caméra
    cameraTransform.LookAt(target.position + Vector3.up * 1.5f); // Ajustez la hauteur si nécessaire
    }

    void HandleStamina()
    {
        if (stateManager == null || playerStats == null)
        {
            Debug.LogError("StateManager or PlayerStats is not initialized!");
            return;
        }

        stateManager.UpdateStamina(playerStats);

        if (playerStats.currentStamina <= 0)
        {
            stateManager.ForceStopSprinting();
        }
    }

    void HandleMovement()
    {
            if (movementInput.magnitude > 0.1f)
    {
        // Calculer la direction du mouvement par rapport à la caméra
        Vector3 moveDirection = cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x;
        moveDirection.y = 0f; // Ignorer le mouvement vertical
        moveDirection.Normalize();

        // Effectuer la rotation du joueur uniquement dans la direction du mouvement
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        // Appliquer la vitesse et déplacer le joueur
        float currentSpeed = stateManager.GetCurrentSpeed(walkSpeed, runSpeed, sprintSpeed);
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
        if (IsGrounded())
        {
            velocity.y = -2f;
            currentJumpCount = 0;

            if (jumpInput)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                currentJumpCount++;
                jumpInput = false;

                jumpVFX?.Play();
                animator?.SetTrigger("Jump");
            }

            animator?.SetBool("isFalling", false);
            animator?.SetBool("isLanding", true);
        }
        else
        {
            if (jumpInput && currentJumpCount < 2)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                currentJumpCount++;
                jumpInput = false;

                jumpVFX?.Play();
                animator?.SetTrigger("Jump");
            }

            velocity.y += gravity * Time.deltaTime;

            animator?.SetBool("isFalling", velocity.y < 0);
            animator?.SetBool("isLanding", false);
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
            // Vérification que l'entrée lookInput est significative avant d'appliquer une rotation
    if (lookInput.sqrMagnitude > 0.01f) // Ignorer les petites entrées pour éviter le bruit
    {
        // Rotation horizontale (Yaw) pour le joueur
        float horizontal = lookInput.x * cameraSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, horizontal);

        // Rotation verticale (Pitch) pour la caméra
        float vertical = lookInput.y * cameraSensitivity * Time.deltaTime;
        verticalAngle = Mathf.Clamp(verticalAngle - vertical, minVerticalAngle, maxVerticalAngle);

        // Appliquer la rotation verticale à la caméra
        Quaternion cameraRotation = Quaternion.Euler(verticalAngle, transform.eulerAngles.y, 0);
        cameraTransform.rotation = cameraRotation;
    }

    // Ajuster le champ de vision pour le mode visée
    float targetFOV = isAiming ? aimFOV : normalFOV;
    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime * aimSmoothSpeed);

    // Ajuster la position de la caméra (avec interpolation pour un mouvement fluide)
    Vector3 targetPosition = isAiming ? transform.position + aimOffset : transform.position + cameraOffset;
    cameraTransform.position = Vector3.Slerp(cameraTransform.position, targetPosition, Time.deltaTime * aimSmoothSpeed);
    }

    void HandleAnimations()
    {
        if (animator != null)
        {
            Vector2 normalizedInput = movementInput.magnitude > 0.1f ? movementInput.normalized : Vector2.zero;
            animator.SetFloat("PosX", normalizedInput.x);
            animator.SetFloat("PosY", normalizedInput.y);

            if (IsGrounded())
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", false);
                if (animator.GetBool("isLanding"))
                {
                    animator.SetBool("isLanding", false);
                }
            }
            else
            {
                animator.SetBool("isJumping", velocity.y > 0);
                animator.SetBool("isFalling", velocity.y < 0);

                if (velocity.y <= 0 && !IsGrounded())
                {
                    animator.SetBool("isLanding", true);
                }
            }

            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isSprinting", isSprinting);
            animator.SetBool("isCrouching", isCrouching);
            animator.SetBool("isRolling", isRolling);
        }
    }

    void InteractWithBoostItem()
    {
        if (nearbyBoostItem != null)
        {
            nearbyBoostItem.ApplyBoost(playerStats, this);
            Destroy(nearbyBoostItem.gameObject);
            nearbyBoostItem = null;
            uiManager?.ToggleInteractionPrompt(false);
        }
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