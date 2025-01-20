using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);

    [Header("Follow Settings")]
    [Tooltip("Vitesse de lissage du suivi de la caméra.")]
    public float smoothSpeed = 0.125f;

    [Header("Zoom Settings")]
    [Tooltip("Distance minimale et maximale pour le zoom.")]
    public float minZoom = 2f;
    public float maxZoom = 15f;
    public float zoomSpeed = 2f;

    [Header("Rotation Settings")]
    [Tooltip("Sensibilité de rotation avec la souris.")]
    public float rotationSpeed = 100f;

    private float currentZoom;
    private float currentYaw;
    private Vector2 rotationInput;
    private float zoomInput;

    private void Start()
    {
        currentZoom = offset.magnitude;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        HandleRotation();

        // Calcul de la position désirée
        Vector3 desiredPosition = target.position - offset.normalized * currentZoom;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Appliquer la position
        transform.position = smoothedPosition;

        // Faire regarder la caméra vers la cible
        transform.LookAt(target.position);
    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        zoomInput = context.ReadValue<float>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<Vector2>();
    }

    private void HandleZoom()
    {
        // Ajuster le zoom selon l'entrée utilisateur
        currentZoom -= zoomInput * zoomSpeed * Time.deltaTime;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    private void HandleRotation()
    {
        // Ajuster la rotation selon l'entrée utilisateur
        currentYaw += rotationInput.x * rotationSpeed * Time.deltaTime;

        // Appliquer la rotation orbitale
        offset = Quaternion.Euler(0, currentYaw, 0) * offset;
    }
}
