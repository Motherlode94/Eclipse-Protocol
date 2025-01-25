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

    [Header("Camera Reference (pour le mouvement)")]
    // On garde la référence à la caméra uniquement pour avoir sa direction avant/droite
    public Transform cameraTransform;

    [Header("VFX Settings")]
    public ParticleSystem sprintVFX;
    public ParticleSystem jumpVFX;

    [Header("Inventory Settings")]
    public GameObject inventoryPanel;

    private CharacterController characterController;
    private StateManager stateManager;
    private Animator animator;
    private Vector3 velocity;
    private Vector2 movementInput;
    private Vector2 lookInput; // <-- Déclaration de la variable manquante
    private EclipseProtocol controls;
    private bool jumpInput;
    private bool isRunning, isSprinting, isCrouching;
    private bool isRolling = false;
    private bool isAiming = false;
    private InventoryManager inventoryManager;
    private PlayerStats playerStats;
    private BoostItem nearbyBoostItem;
    private UIManager uiManager;
    private PlayerInput playerInput; // Défini dans votre description
    private int currentJumpCount = 0; // Idem

    private bool previouslyInAir = false;

    // Pour déterminer si on est au sol
    private bool IsGrounded()
    {
        Vector3 groundCheck = transform.position
            + Vector3.down * (characterController.height / 2 + groundYOffset);
        bool grounded = Physics.CheckSphere(groundCheck, 0.2f, groundMask);
        return grounded;
    }

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

        // Pour la visée et le Look
        controls.Player.Aim.performed += ctx => OnAim(ctx);
        controls.Player.Aim.canceled += ctx => OnAim(ctx);
        controls.Player.Look.performed += ctx => OnLook(ctx);
        controls.Player.Look.canceled += ctx => OnLook(ctx);
    }

    // --------- Méthodes d'Input obligatoires pour IPlayerActions --------- //
    // (Seules celles effectivement appelées dans InitializeInputActions sont utiles.)
    public void OnLook(InputAction.CallbackContext context)
    {
        // On récupère le mouvement de la souris ou du stick droit (nouveau Input System)
        Vector2 rawInput = context.ReadValue<Vector2>();

        // Appliquer un seuil pour éviter le bruit
        const float threshold = 0.05f;
        lookInput = rawInput.magnitude > threshold ? rawInput : Vector2.zero;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Interact action triggered.");
            // Logique d’interaction
        }
    }
    public void OnLookHorizontal(InputAction.CallbackContext context) { }
    public void OnZoom(InputAction.CallbackContext context) { }
    public void OnMouseX(InputAction.CallbackContext context) { }
    public void OnMouseY(InputAction.CallbackContext context) { }
    public void OnMouseScroll(InputAction.CallbackContext context) { }
    // --------------------------------------------------------------------- //

    public void OnAim(InputAction.CallbackContext context)
    {
        isAiming = context.performed;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.performed ? context.ReadValue<Vector2>() : Vector2.zero;
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
            if (isSprinting) sprintVFX.Play();
            else sprintVFX.Stop();
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

                // Déplacement instantané sur cette frame
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
            // Logique de tir
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

    private void Update()
    {
        HandleMovement();
        HandleGravityAndJump();
        HandleStamina();
        HandleAnimations();

        // Exemple d'interaction : appuyer sur E pour interagir
        if (nearbyBoostItem != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            InteractWithBoostItem();
        }
    }

    void HandleMovement()
    {
        // On utilise la caméra pour orienter le déplacement
        if (cameraTransform != null && movementInput.magnitude > 0.1f)
        {
            // Direction avant/arrière = forward de la cam, gauche/droite = right de la cam
            Vector3 camForward = cameraTransform.forward; 
            Vector3 camRight   = cameraTransform.right;

            // On ignore la composante verticale (pour éviter de pencher le joueur)
            camForward.y = 0f;
            camRight.y   = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Calcul direction de déplacement
            Vector3 moveDirection = camForward * movementInput.y + camRight * movementInput.x;
            moveDirection.Normalize();

            // On pivote le joueur dans la direction de déplacement
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            // On détermine la vitesse (marche, course ou sprint)
            float currentSpeed = stateManager.GetCurrentSpeed(walkSpeed, runSpeed, sprintSpeed);

            // On applique le déplacement
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
    }

    void HandleGravityAndJump()
{
    // Check if the player is grounded
    bool isGrounded = IsGrounded();

    if (isGrounded)
    {
        // Reset vertical velocity when grounded
        velocity.y = -2f;
        currentJumpCount = 0;

        // Handle jump input
        if (jumpInput)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            currentJumpCount++;
            jumpInput = false;

            // Trigger jump animation
            animator?.SetTrigger("Jump");
        }
    }
    else
    {
        // Apply gravity when in the air
        velocity.y += gravity * Time.deltaTime;

        // Handle double jump
        if (jumpInput && currentJumpCount < 2)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            currentJumpCount++;
            jumpInput = false;

            animator?.SetTrigger("Jump");
        }
    }

    // Update falling or jumping animation states
    animator?.SetBool("isFalling", !isGrounded && velocity.y < 0);
    animator?.SetBool("isJumping", !isGrounded && velocity.y > 0);

    // Apply vertical movement
    characterController.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

    // Detect landing
    if (previouslyInAir && isGrounded)
    {
        animator?.SetTrigger("Landing");
    }

    previouslyInAir = !isGrounded;
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

    void HandleAnimations()
    {
            if (!animator) return;

    // Alimentation de la blend tree de locomotion
    Vector2 normalizedInput = (movementInput.magnitude > 0.1f) ? movementInput.normalized : Vector2.zero;
    animator.SetFloat("PosX", normalizedInput.x);
    animator.SetFloat("PosY", normalizedInput.y);

    // Gérer l’état "en l’air" si besoin
    // (Si vous n’utilisez pas un paramètre "isJumping" en bool, vous pouvez le supprimer)
    bool isGrounded = IsGrounded();
    animator.SetBool("isJumping", !isGrounded && velocity.y > 0);
    animator.SetBool("isFalling", !isGrounded && velocity.y < 0);

    // Courir, sprinter, etc.
    animator.SetBool("isRunning", isRunning);
    animator.SetBool("isSprinting", isSprinting);
    animator.SetBool("isCrouching", isCrouching);
    animator.SetBool("isRolling", isRolling);
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
    private void OnDrawGizmosSelected()
{
    if (!characterController) return;

    // Calculate the ground check position
    Vector3 groundCheck = transform.position + Vector3.down * (characterController.height / 2 + groundYOffset);

    // Draw a wire sphere for visualization
    Gizmos.color = IsGrounded() ? Color.green : Color.red;
    Gizmos.DrawWireSphere(groundCheck, 0.2f);
}


    // velocity doit être défini quelque part (manquant dans votre code original),
    // on le déclare ici en privé pour la gestion de la gravit
}

// -------------------------------------------------------------------------
// Classe séparée pour gérer l'état (course, sprint, etc.)
public class StateManager
{
    private bool isRunning;
    private bool isSprinting;
    private bool isAiming;

    public void SetRunning(bool value)   => isRunning = value;
    public void SetSprinting(bool value) => isSprinting = value;
    public void SetAiming(bool value)    => isAiming = value;

    public float GetCurrentSpeed(float walkSpeed, float runSpeed, float sprintSpeed)
    {
        if (isSprinting) return sprintSpeed;
        if (isRunning)   return runSpeed;
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
