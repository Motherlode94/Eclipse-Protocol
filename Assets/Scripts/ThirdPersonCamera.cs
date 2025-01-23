using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Update()
    {
        // Gestion du zoom avec la molette de la souris
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Rotation horizontale avec la souris
        yawInput += Input.GetAxis("Mouse X") * rotationSpeed;
    }

    private void LateUpdate()
    {
        // Positionnement de la caméra
        Vector3 desiredPosition = target.position - transform.forward * currentZoom + Vector3.up * offset.y;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * rotationSpeed);

        // Faire regarder la caméra vers le joueur
        transform.LookAt(target.position + Vector3.up * offset.y);

        // Rotation horizontale autour du joueur
        transform.RotateAround(target.position, Vector3.up, yawInput);
    }
}
