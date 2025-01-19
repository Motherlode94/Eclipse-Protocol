using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour
{
    [System.Serializable]
    public class MovementSettings
    {
        [Tooltip("Force de propulsion du vaisseau")]
        public float thrustForce = 10f;
        [Tooltip("Multiplicateur de vitesse lors du boost")]
        public float boostMultiplier = 2f;
        [Tooltip("Vitesse de rotation du vaisseau")]
        public float rotationSpeed = 50f;
        [Tooltip("Vitesse maximale en mode normal")]
        public float maxSpeed = 20f;
        [Tooltip("Force de freinage")]
        public float brakingForce = 5f;
    }

    [System.Serializable]
    public class HoverSettings
    {
        [Tooltip("Force maintenant le vaisseau en vol stationnaire")]
        public float hoverForce = 20f;
        [Tooltip("Hauteur de vol désirée")]
        public float targetHeight = 2f;
        [Tooltip("Force d'amortissement vertical")]
        public float hoverDamping = 10f;
        [Tooltip("Force de stabilisation de l'orientation")]
        public float stabilizationForce = 5f;
        [Tooltip("Distance maximale de détection du sol")]
        public float maxGroundDistance = 10f;
        [Tooltip("Angle maximum de tangage et roulis")]
        public float maxTiltAngle = 30f;
    }

    [Header("Configuration")]
    [SerializeField] private MovementSettings movement = new MovementSettings();
    [SerializeField] private HoverSettings hover = new HoverSettings();
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool debugMode;

    private Rigidbody rb;
    private Vector2 pitchYawInput;
    private float rollInput;
    private float thrustInput;
    private bool isBoosting;
    private bool isBraking;
    
    private Vector3 lastGroundNormal = Vector3.up;
    private float currentGroundDistance;
    private bool isGroundDetected;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
    }

    private void FixedUpdate()
    {
        UpdateGroundDetection();
        ApplyHoverPhysics();
        HandleMovement();
        if (debugMode) DrawDebugVisuals();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    #endregion

    #region Physics Handling

    private void HandleMovement()
    {
        ApplyThrust();
        ApplyRotation();
        HandleSpeedLimits();
        ApplyBraking();
    }

    public void AssignPlayerStats(PlayerStats playerStats)
{
    if (playerStats == null) return;

    // Exemple de transfert des statistiques du joueur au vaisseau
    movement.thrustForce = playerStats.ThrustForce;
    movement.maxSpeed = playerStats.MaxSpeed;
    movement.rotationSpeed = playerStats.RotationSpeed;
    movement.boostMultiplier = playerStats.BoostMultiplier;

    // D'autres stats peuvent être ajoutées ici
    Debug.Log("Stats transférées au vaisseau depuis le joueur.");
}


    private void ApplyThrust()
    {
        float currentThrust = thrustInput * movement.thrustForce;
        if (isBoosting) currentThrust *= movement.boostMultiplier;
        rb.AddForce(transform.forward * currentThrust, ForceMode.Acceleration);
    }

    private void ApplyRotation()
    {
        Vector3 targetRotation = new Vector3(
            -pitchYawInput.y,
            pitchYawInput.x,
            rollInput
        ) * movement.rotationSpeed * Time.fixedDeltaTime;

        targetRotation = ClampRotation(targetRotation);
        
        rb.MoveRotation(rb.rotation * Quaternion.Euler(targetRotation));
    }

    private Vector3 ClampRotation(Vector3 rotation)
    {
        rotation.x = Mathf.Clamp(rotation.x, -hover.maxTiltAngle, hover.maxTiltAngle);
        rotation.z = Mathf.Clamp(rotation.z, -hover.maxTiltAngle, hover.maxTiltAngle);
        return rotation;
    }

    private void HandleSpeedLimits()
    {
        float speedLimit = isBoosting ? movement.maxSpeed * movement.boostMultiplier : movement.maxSpeed;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
        
        if (horizontalVelocity.magnitude > speedLimit)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * speedLimit;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void ApplyBraking()
    {
        if (isBraking)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, movement.brakingForce * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Hover System

    private void UpdateGroundDetection()
    {
        RaycastHit hit;
        isGroundDetected = Physics.Raycast(
            transform.position,
            Vector3.down,
            out hit,
            hover.maxGroundDistance,
            groundLayer
        );

        if (isGroundDetected)
        {
            currentGroundDistance = hit.distance;
            lastGroundNormal = hit.normal;
        }
    }

    private void ApplyHoverPhysics()
    {
        if (!isGroundDetected)
        {
            rb.AddForce(Vector3.down * hover.hoverForce * 0.5f, ForceMode.Acceleration);
            return;
        }

        ApplyHoverForce();
        StabilizeOrientation();
    }

    private void ApplyHoverForce()
    {
        float heightError = hover.targetHeight - currentGroundDistance;
        Vector3 hoverForce = Vector3.up * heightError * hover.hoverForce;
        hoverForce -= rb.velocity.y * hover.hoverDamping * Vector3.up;
        
        rb.AddForce(hoverForce, ForceMode.Acceleration);
    }

    private void StabilizeOrientation()
    {
        Vector3 stabilizationTorque = Vector3.Cross(transform.up, lastGroundNormal) * hover.stabilizationForce;
        rb.AddTorque(stabilizationTorque, ForceMode.Acceleration);
    }

    #endregion

    #region Input Handling

    public void OnThrust(InputAction.CallbackContext context) =>
        thrustInput = context.ReadValue<float>();

    public void OnPitchYaw(InputAction.CallbackContext context) =>
        pitchYawInput = context.ReadValue<Vector2>();

    public void OnRoll(InputAction.CallbackContext context) =>
        rollInput = context.ReadValue<float>();

    public void OnBoost(InputAction.CallbackContext context) =>
        isBoosting = context.performed;

    public void OnBrake(InputAction.CallbackContext context) =>
        isBraking = context.performed;

    public void ResetInputs()
    {
        thrustInput = 0;
        pitchYawInput = Vector2.zero;
        rollInput = 0;
        isBoosting = false;
        isBraking = false;
    }
    public void ResetStatsToDefault()
{
    movement.thrustForce = 10f;
    movement.maxSpeed = 20f;
    movement.rotationSpeed = 50f;
    movement.boostMultiplier = 2f;

    Debug.Log("Stats du vaisseau réinitialisées.");
}


    #endregion

    #region Debug

    private void DrawDebugVisuals()
    {
        // Affiche la direction de poussée
        Debug.DrawRay(transform.position, transform.forward * thrustInput * movement.thrustForce, Color.blue);
        
        // Affiche le rayon de détection du sol
        Color groundRayColor = isGroundDetected ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector3.down * hover.maxGroundDistance, groundRayColor);
        
        // Affiche la normale du sol
        if (isGroundDetected)
        {
            Debug.DrawRay(transform.position, lastGroundNormal * 2f, Color.yellow);
        }
    }

    #endregion
}
