using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Nouvelle méthode d'Input System

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target; // Le joueur ou l'objet à suivre
    public Vector3 offset = new Vector3(0, 1.5f, -3f);
    public float rotationSpeed = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    private float currentZoom = 5f;
    private float yawInput = 0f;

    private PlayerInput playerInput; // Référence au Player Input
    private InputAction mouseXAction; // Mouvement horizontal de la souris
    private InputAction mouseYAction; // Mouvement vertical de la souris
    private InputAction mouseScrollAction; // Molette de la souris

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Actions de la souris
        mouseXAction = playerInput.actions["MouseX"];
        mouseYAction = playerInput.actions["MouseY"];
        mouseScrollAction = playerInput.actions["MouseScroll"];
    }

    private void Update()
    {
        // Gestion du zoom avec la molette de la souris
        float scrollInput = mouseScrollAction.ReadValue<float>();
        currentZoom -= scrollInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Rotation horizontale avec la souris
        yawInput += mouseXAction.ReadValue<float>() * rotationSpeed;
    }

    private void LateUpdate()
    {
        // Positionnement de la caméra
        Vector3 desiredPosition = target.position + offset * currentZoom;
        transform.position = desiredPosition;

        // Rotation de la caméra autour du joueur
        transform.LookAt(target.position);
        transform.RotateAround(target.position, Vector3.up, yawInput);
    }
}
