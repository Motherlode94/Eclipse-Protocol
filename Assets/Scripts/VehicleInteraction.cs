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
    }

    private void ValidateComponents()
    {
        if (player == null)
            Debug.LogError($"Player reference is missing on {gameObject.name}", this);
        if (vehicleSeat == null)
            Debug.LogError($"Vehicle Seat reference is missing on {gameObject.name}", this);
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
        if (!IsPlayerNearby || Time.time - lastInteractionTime < enterExitCooldown)
        {
            return;
        }

        if (IsPlayerInVehicle)
        {
            ExitVehicle();
        }
        else
        {
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
            return;
        }

        DisablePlayerControl();
        AttachPlayerToVehicle();
        IsPlayerInVehicle = true;
        events.onPlayerEnterVehicle?.Invoke();
    }

    private void ExitVehicle()
    {
        if (!CanExitVehicle())
        {
            return;
        }

        DetachPlayerFromVehicle();
        EnablePlayerControl();
        IsPlayerInVehicle = false;
        events.onPlayerExitVehicle?.Invoke();
    }

    private void ForceExitVehicle()
    {
        DetachPlayerFromVehicle();
        EnablePlayerControl();
        IsPlayerInVehicle = false;
        events.onPlayerExitVehicle?.Invoke();
    }

    public void ForceExit()
    {
        ForceExitVehicle();
    }

    #endregion

    #region Helper Methods

    private bool CanEnterVehicle()
    {
        return !IsPlayerInVehicle && player != null && vehicleSeat != null;
    }

    private bool CanExitVehicle()
    {
        return IsPlayerInVehicle && player != null;
    }

    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }
    }

    private void EnablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
            playerRigidbody.velocity = Vector3.zero;
        }
    }

    private void AttachPlayerToVehicle()
    {
        if (player == null || vehicleSeat == null) return;

        player.transform.SetParent(vehicleSeat);
        player.transform.localPosition = Vector3.zero;

        if (!keepPlayerRotation)
        {
            player.transform.localRotation = Quaternion.identity;
        }
    }

    private void DetachPlayerFromVehicle()
    {
        if (player == null) return;

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
}
