using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class VehicleInteraction : MonoBehaviour
{
    [System.Serializable]
    public class VehicleEvents
    {
        public UnityEvent onPlayerEnterVehicle;
        public UnityEvent onPlayerExitVehicle;
        public UnityEvent onPlayerApproachVehicle;
        public UnityEvent onPlayerLeaveArea;
    }

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform vehicleSeat;
    [SerializeField] private SpaceshipController spaceship;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InputAction interactAction;

    [Header("Settings")]
    [SerializeField] private float enterExitCooldown = 0.5f;
    [SerializeField] private Vector3 exitOffset = new Vector3(0, 1f, 2f);
    [SerializeField] private bool keepPlayerRotation = false;
    [SerializeField] private string interactionPrompt = "Appuyez sur E pour interagir";

    [Header("Events")]
    [SerializeField] private VehicleEvents events = new VehicleEvents();

    public bool IsPlayerInVehicle { get; private set; }
    public bool IsPlayerNearby { get; private set; }

    private float lastInteractionTime;
    private Collider vehicleCollider;
    private Rigidbody playerRigidbody;
    private Vector3 originalPlayerScale;
    private bool isInitialized;

    #region Unity Lifecycle

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.Enable();
            interactAction.performed += HandleInteractionInput;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.Disable();
            interactAction.performed -= HandleInteractionInput;
        }
    }

    private void OnDestroy()
    {
        if (IsPlayerInVehicle)
        {
            ForceExitVehicle();
        }
    }

    #endregion

    #region Initialization

    private void Initialize()
    {
        if (isInitialized) return;

        vehicleCollider = GetComponent<Collider>();
        if (vehicleCollider != null)
        {
            vehicleCollider.isTrigger = true;
        }

        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
            originalPlayerScale = player.transform.localScale;
        }

        ValidateComponents();
        isInitialized = true;
    }

    private void ValidateComponents()
    {
        if (player == null)
            Debug.LogError($"Player reference is missing on {gameObject.name}", this);
        if (vehicleSeat == null)
            Debug.LogError($"Vehicle Seat reference is missing on {gameObject.name}", this);
        if (spaceship == null)
            Debug.LogError($"Spaceship Controller reference is missing on {gameObject.name}", this);
        if (playerController == null)
            Debug.LogError($"Player Controller reference is missing on {gameObject.name}", this);
    }

    #endregion

    #region Trigger Detection

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered the interaction zone");
        IsPlayerNearby = true;
        events.onPlayerApproachVehicle?.Invoke();
        ShowInteractionPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player exited the interaction zone");
        IsPlayerNearby = false;
        events.onPlayerLeaveArea?.Invoke();
        HideInteractionPrompt();

        if (IsPlayerInVehicle)
        {
            ForceExitVehicle();
        }
    }

    #endregion

    #region Input Handling

    private void HandleInteractionInput(InputAction.CallbackContext context)
    {
        Debug.Log("Interact input received");

        if (!IsPlayerNearby || Time.time - lastInteractionTime < enterExitCooldown)
        {
            Debug.LogWarning("Cannot interact: Either not nearby or cooldown is active.");
            return;
        }

        if (IsPlayerInVehicle)
        {
            Debug.Log("Attempting to exit the vehicle");
            ExitVehicle();
        }
        else
        {
            Debug.Log("Attempting to enter the vehicle");
            EnterVehicle();
        }

        lastInteractionTime = Time.time;
    }

    #endregion

    #region Vehicle Entry/Exit

    private void EnterVehicle()
    {
        if (!CanEnterVehicle())
        {
            Debug.LogWarning("Cannot enter vehicle. Check conditions.");
            return;
        }

        Debug.Log("Player entering vehicle");
        DisablePlayerControl();
        AttachPlayerToVehicle();
        EnableVehicleControl();

        IsPlayerInVehicle = true;
        events.onPlayerEnterVehicle?.Invoke();
    }

    private void ExitVehicle()
    {
        if (!CanExitVehicle())
        {
            Debug.LogWarning("Cannot exit vehicle. Check conditions.");
            return;
        }

        Debug.Log("Player exiting vehicle");
        DisableVehicleControl();
        DetachPlayerFromVehicle();
        EnablePlayerControl();

        IsPlayerInVehicle = false;
        events.onPlayerExitVehicle?.Invoke();
    }

    private void ForceExitVehicle()
    {
        Debug.Log("Forcing player to exit vehicle");
        DisableVehicleControl();
        DetachPlayerFromVehicle();
        EnablePlayerControl();

        IsPlayerInVehicle = false;
        events.onPlayerExitVehicle?.Invoke();
    }

    #endregion

    #region Helper Methods

    private void EnableVehicleControl()
    {
        if (spaceship != null)
        {
            Debug.Log("Enabling spaceship control");
            spaceship.enabled = true;
            spaceship.ResetInputs();
            spaceship.AssignPlayerStats(player.GetComponent<PlayerStats>());
        }
        if (playerController != null)
            playerController.enabled = false; // Disable player controls
    }

    private void DisableVehicleControl()
    {
        if (spaceship != null)
        {
            Debug.Log("Disabling spaceship control");
            spaceship.ResetStatsToDefault();
            spaceship.enabled = false;
        }

        if (playerController != null)
            playerController.enabled = true; // Enable player controls
    }

    private bool CanEnterVehicle()
    {
        return !IsPlayerInVehicle &&
               player != null &&
               vehicleSeat != null &&
               spaceship != null &&
               playerController != null;
    }

    private bool CanExitVehicle()
    {
        return IsPlayerInVehicle &&
               player != null &&
               playerController != null;
    }

    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            Debug.Log("Disabling player control");
            playerController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            Debug.Log("Setting player Rigidbody to kinematic");
            playerRigidbody.isKinematic = true;
        }
    }

    private void EnablePlayerControl()
    {
        if (playerController != null)
        {
            Debug.Log("Enabling player control");
            playerController.enabled = true;
        }

        if (playerRigidbody != null)
        {
            Debug.Log("Resetting player Rigidbody");
            playerRigidbody.isKinematic = false;
            playerRigidbody.velocity = Vector3.zero;
        }
    }

    private void AttachPlayerToVehicle()
    {
        if (player == null || vehicleSeat == null) return;

        Debug.Log("Attaching player to vehicle");
        player.transform.SetParent(vehicleSeat);
        player.transform.localPosition = Vector3.zero;

        if (!keepPlayerRotation)
            player.transform.localRotation = Quaternion.identity;
    }

    private void DetachPlayerFromVehicle()
    {
        if (player == null) return;

        Debug.Log("Detaching player from vehicle");
        player.transform.SetParent(null);
        player.transform.position = transform.TransformPoint(exitOffset);
        player.transform.localScale = originalPlayerScale;
    }

    private void ShowInteractionPrompt()
    {
        Debug.Log(interactionPrompt);
    }

    private void HideInteractionPrompt()
    {
        Debug.Log("Interaction prompt hidden.");
    }

    #endregion

    #region Public Methods

    public void ForceExit()
    {
        if (IsPlayerInVehicle)
        {
            ForceExitVehicle();
        }
    }

    public Vector3 GetExitPosition()
    {
        return transform.TransformPoint(exitOffset);
    }

    #endregion
}
