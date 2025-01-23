using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Cible et offset")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Header("Distance et zoom")]
    public float distance = 3f;        // distance de base
    public float zoomSpeed = 2f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    [Header("Rotation")]
    public float rotationSpeed = 5f; // vitesse de rotation
    public float pitchMin = -20f;
    public float pitchMax = 60f;

    // Angles actuels
    private float currentYaw = 0f;
    private float currentPitch = 0f;

    private void Update()
    {
        if (!target) return;

        // Zoom (molette de la souris via l’ancien Input Manager)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calculer le Yaw (horizontal) et le Pitch (vertical)
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentYaw += mouseX * rotationSpeed;
        currentPitch -= mouseY * rotationSpeed;

        // Clamping du pitch pour ne pas retourner la caméra
        currentPitch = Mathf.Clamp(currentPitch, pitchMin, pitchMax);
    }

    private void LateUpdate()
    {
        if (!target) return;

        // Construire la rotation à partir de Yaw/Pitch
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // Position idéale derrière la cible, selon "distance"
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * distance);
        // On ajoute l’offset vertical
        desiredPosition.y += offset.y;

        // Placement progressif vers la position idéale
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * rotationSpeed);

        // Regarder la cible
        transform.LookAt(target.position + offset);
    }
}