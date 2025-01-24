using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Transform du joueur (pivot du personnage).")]
    public Transform target;

    [Tooltip("Réglage de l'offset vertical pour viser la tête/épaules.")]
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);

    [Header("Distances et Zoom")]
    public float distance = 3f;
    public float zoomSpeed = 2f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    [Header("Rotation caméra")]
    [Tooltip("Vitesse de rotation (sensibilité souris).")]
    public float rotationSpeed = 120f;
    [Tooltip("Angle min (caméra vers le haut).")]
    public float pitchMin = -20f;
    [Tooltip("Angle max (caméra vers le bas).")]
    public float pitchMax = 60f;

    [Header("Rotation joueur")]
    [Tooltip("Si true, le joueur pivote pour suivre le yaw de la caméra.")]
    public bool rotatePlayerWithCamera = true;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayers;
    [Tooltip("Distance pour éviter de coller la caméra au mur.")]
    public float obstaclePadding = 0.2f;

    [Header("Lissage")]
    [Tooltip("Vitesse de lerp pour la position de la caméra.")]
    public float positionLerpSpeed = 10f;

    // Angles actuels
    private float currentYaw;
    private float currentPitch;

    // Nouvelle Input System
    private EclipseProtocol controls;
    private Vector2 lookInput;  // (x,y) souris / stick droit
    private float zoomInput;     // molette ou axe Zoom

    private void Awake()
    {
        controls = new EclipseProtocol();
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        // On s'abonne aux actions
        controls.Player.Look.performed += OnLook;
        controls.Player.Look.canceled  += OnLook;

        controls.Player.Zoom.performed += OnZoom;
        controls.Player.Zoom.canceled  += OnZoom;
    }

    private void OnDisable()
    {
        controls.Player.Look.performed -= OnLook;
        controls.Player.Look.canceled  -= OnLook;

        controls.Player.Zoom.performed -= OnZoom;
        controls.Player.Zoom.canceled  -= OnZoom;

        controls.Player.Disable();
    }

    private void Start()
    {
        if (!target) return;

        // Pour partir caméra derrière le joueur (supposé faire face à +Z)
        // Si votre perso fait face +X, ajoutez +90f.
        currentYaw   = target.eulerAngles.y;
        currentPitch = 10f; // Légère inclinaison vers le bas
    }

    // Récupération du delta souris/manette
    private void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    // Récupération de la molette (ou axe) pour zoomer
    private void OnZoom(InputAction.CallbackContext ctx)
    {
        zoomInput = ctx.canceled ? 0f : ctx.ReadValue<float>();
    }

    private void Update()
    {
        if (!target) return;

        // 1) Zoom
        distance -= zoomInput * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // 2) Yaw / Pitch
        currentYaw   += lookInput.x * rotationSpeed * Time.deltaTime;
        currentPitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        currentPitch  = Mathf.Clamp(currentPitch, pitchMin, pitchMax);

        // 3) Faire pivoter le joueur pour qu'il regarde la même direction (optionnel)
        if (rotatePlayerWithCamera)
        {
            target.rotation = Quaternion.Euler(0f, currentYaw, 0f);
        }
    }

    private void LateUpdate()
    {
        if (!target) return;

        // a) Construire la rotation de la caméra
        //    => On applique pitch/yaw calculés
        Quaternion camRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // b) Point central de visée (ex. haut du torse)
        Vector3 targetCenter = target.position + offset;

        // c) Position idéale (on recule sur -forward de la rotation)
        Vector3 desiredPos = targetCenter - (camRotation * Vector3.forward * distance);

        // d) Obstacle Avoidance
        desiredPos = CheckObstacles(targetCenter, desiredPos);

        // e) Lissage de la position
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * positionLerpSpeed);

        // f) La caméra regarde le point central
        transform.LookAt(targetCenter);
    }

    /// <summary>
    /// Empêcher la caméra de traverser les murs par un raycast entre targetCenter et desiredPos.
    /// </summary>
    private Vector3 CheckObstacles(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        dir.Normalize();

        if (Physics.Raycast(from, dir, out RaycastHit hit, dist, obstacleLayers))
        {
            to = hit.point - dir * obstaclePadding;
        }
        return to;
    }
}
