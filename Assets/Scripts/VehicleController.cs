using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour
{
    public enum VehicleType { Ground, Air, Water }
    public VehicleType vehicleType;

    [Header("Common Settings")]
    public float maxSpeed = 50f;
    public float acceleration = 10f;
    public float rotationSpeed = 5f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float throttleInput;
    private float yawInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnThrottle(InputValue value)
    {
        throttleInput = value.Get<float>();
    }

    public void OnYaw(InputValue value)
    {
        yawInput = value.Get<float>();
    }

    private void FixedUpdate()
    {
        switch (vehicleType)
        {
            case VehicleType.Ground:
                HandleGroundVehicle();
                break;
            case VehicleType.Air:
                HandleAirVehicle();
                break;
            case VehicleType.Water:
                HandleWaterVehicle();
                break;
        }
    }

    private void HandleGroundVehicle()
    {
        Vector3 forwardMove = transform.forward * throttleInput * acceleration * Time.fixedDeltaTime;
        rb.AddForce(forwardMove, ForceMode.Acceleration);

        float steer = moveInput.x * rotationSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, steer);

        // Limiter la vitesse
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    private void HandleAirVehicle()
    {
        // Portance (Lift)
        Vector3 lift = Vector3.up * throttleInput * acceleration * Time.fixedDeltaTime;
        rb.AddForce(lift, ForceMode.Acceleration);

        // Déplacement avant
        Vector3 forwardMove = transform.forward * moveInput.y * maxSpeed * Time.fixedDeltaTime;
        rb.AddForce(forwardMove, ForceMode.Acceleration);

        // Rotation (yaw pour tourner)
        float yaw = yawInput * rotationSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, yaw);

        // Inclinaison (roll/pitch)
        float roll = -moveInput.x * rotationSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.forward, roll);
    }

    private void HandleWaterVehicle()
    {
        // Flottabilité (force d'Archimède)
        if (transform.position.y < 0)
        {
            rb.AddForce(Vector3.up * Mathf.Abs(transform.position.y) * acceleration, ForceMode.Acceleration);
        }

        // Déplacement avant/arrière
        Vector3 forwardMove = transform.forward * throttleInput * acceleration * Time.fixedDeltaTime;
        rb.AddForce(forwardMove, ForceMode.Acceleration);

        // Rotation (gouvernail)
        float steer = moveInput.x * rotationSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, steer);

        // Limiter la vitesse
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }
}
