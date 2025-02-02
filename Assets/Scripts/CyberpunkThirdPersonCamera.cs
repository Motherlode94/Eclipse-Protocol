using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CyberpunkThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Cybernetic Zoom & Focus")]
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomSpeed = 2f;

    [Header("Combat & Targeting")]
    [SerializeField] private float targetLockSmoothTime = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Collision Settings")]
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private LayerMask collisionLayers;

    // Input Actions
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction zoomAction;
    private InputAction lockTargetAction;

    private Vector3 currentRotation;
    private float currentZoom;
    private Transform currentTarget;
    private Vector3 targetLockVelocity;

    void Awake()
    {
        // Initialize Input System
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing!");
            return;
        }

        // Attempt to find actions
        lookAction = playerInput.actions.FindAction("Look");
        if (lookAction == null)
        {
            Debug.LogError("Look action not found in PlayerInput!");
        }

        zoomAction = playerInput.actions.FindAction("Zoom");
        if (zoomAction == null)
        {
            Debug.LogWarning("Zoom action not found in PlayerInput! Zoom functionality will be disabled.");
        }

        lockTargetAction = playerInput.actions.FindAction("LockTarget");
        if (lockTargetAction == null)
        {
            Debug.LogWarning("LockTarget action not found in PlayerInput! Target locking will be disabled.");
        }

        currentRotation = transform.eulerAngles;
        currentZoom = distance;
    }

    void Update()
    {
        if (lookAction != null)
        {
            HandleCameraRotation();
        }

        if (zoomAction != null)
        {
            HandleZoom();
        }

        if (lockTargetAction != null)
        {
            HandleTargetLocking();
        }

        HandleCollision();
    }

    void HandleCameraRotation()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        currentRotation.y += lookInput.x * rotationSpeed;
        currentRotation.x -= lookInput.y * rotationSpeed;
        currentRotation.x = Mathf.Clamp(currentRotation.x, -45f, 45f);

        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentZoom);
        Vector3 position = rotation * negDistance + target.position + Vector3.up * height;

        transform.rotation = rotation;
        transform.position = position;
    }

    void HandleZoom()
    {
        float zoomInput = zoomAction.ReadValue<float>();
        currentZoom = Mathf.Clamp(currentZoom - zoomInput * zoomSpeed, minZoom, maxZoom);
    }

    void HandleTargetLocking()
    {
        if (lockTargetAction.triggered)
        {
            if (currentTarget != null)
            {
                currentTarget = null; // Unlock target if already locked
                return;
            }

            Collider[] nearbyEnemies = Physics.OverlapSphere(target.position, 20f, enemyLayer);
            if (nearbyEnemies.Length > 0)
            {
                currentTarget = GetClosestEnemy(nearbyEnemies);
            }
        }

        if (currentTarget != null)
        {
            Vector3 targetPosition = currentTarget.transform.position + Vector3.up * 1.5f;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref targetLockVelocity, targetLockSmoothTime);
            transform.LookAt(targetPosition);
        }
    }

    Transform GetClosestEnemy(Collider[] enemies)
    {
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(target.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    void HandleCollision()
    {
        Vector3 desiredPosition = transform.position;
        Vector3 direction = (transform.position - target.position).normalized;

        if (Physics.SphereCast(target.position, collisionOffset, direction, out RaycastHit hit, currentZoom, collisionLayers))
        {
            desiredPosition = hit.point + direction * collisionOffset;
        }

        transform.position = desiredPosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 20f);
    }
}
