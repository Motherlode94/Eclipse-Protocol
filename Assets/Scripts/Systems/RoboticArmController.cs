using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class RoboticArmController : MonoBehaviour
{
    [Header("Joints Configuration")]
    [SerializeField] private Transform baseJoint;
    [SerializeField] private Transform arm1Joint;
    [SerializeField] private Transform arm2Joint;
    [SerializeField] private Transform clawJoint;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float clawSpeed = 10f;

    [Header("Claw Settings")]
    [SerializeField] private Transform clawLeft;
    [SerializeField] private Transform clawRight;
    [SerializeField] private float clawOpenDistance = 0.2f;
    [SerializeField] private float clawMovementSpeed = 0.05f;

    [Header("Rotation Limits")]
    [SerializeField] private Vector2 arm1RotationLimits = new Vector2(-45f, 45f); // Min/Max en degrés
    [SerializeField] private Vector2 arm2RotationLimits = new Vector2(-30f, 30f);

    [Header("Player Settings")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerController playerController;

    private bool isClawOpen = true;
    private bool isControllingArm = false;
    private float arm1CurrentRotation = 0f;
    private float arm2CurrentRotation = 0f;

    private void Update()
    {
        if (isControllingArm)
        {
            HandleBaseRotation();
            HandleArmMovement();
            HandleClawControl();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitControl();
            }
        }
    }

    public void StartControl()
    {
        if (playerController != null)
        {
            Debug.Log("Player started controlling the robotic arm.");
            playerController.enabled = false;
        }
        isControllingArm = true;
    }

    public void ExitControl()
    {
        if (playerController != null)
        {
            Debug.Log("Player stopped controlling the robotic arm.");
            playerController.enabled = true;
        }
        isControllingArm = false;
    }

    private void HandleBaseRotation()
    {
        float horizontalInput = Keyboard.current.leftArrowKey.isPressed ? -1 :
                                Keyboard.current.rightArrowKey.isPressed ? 1 : 0;

        if (horizontalInput != 0 && baseJoint != null)
        {
            baseJoint.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleArmMovement()
    {
        float verticalInput = Keyboard.current.upArrowKey.isPressed ? 1 :
                              Keyboard.current.downArrowKey.isPressed ? -1 : 0;

        if (arm1Joint != null)
        {
            arm1CurrentRotation += verticalInput * rotationSpeed * Time.deltaTime;
            arm1CurrentRotation = Mathf.Clamp(arm1CurrentRotation, arm1RotationLimits.x, arm1RotationLimits.y);
            arm1Joint.localRotation = Quaternion.Euler(arm1CurrentRotation, 0, 0);
        }

        float arm2Input = Keyboard.current.qKey.isPressed ? -1 :
                          Keyboard.current.eKey.isPressed ? 1 : 0;

        if (arm2Joint != null)
        {
            arm2CurrentRotation += arm2Input * rotationSpeed * Time.deltaTime;
            arm2CurrentRotation = Mathf.Clamp(arm2CurrentRotation, arm2RotationLimits.x, arm2RotationLimits.y);
            arm2Joint.localRotation = Quaternion.Euler(arm2CurrentRotation, 0, 0);
        }
    }

    private void HandleClawControl()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleClaw();
        }
    }

    private void ToggleClaw()
    {
        if (isClawOpen)
        {
            StartCoroutine(MoveClaw(Vector3.zero));
        }
        else
        {
            Vector3 openPosition = new Vector3(-clawOpenDistance, 0, 0);
            StartCoroutine(MoveClaw(openPosition));
        }

        isClawOpen = !isClawOpen;
    }

    private IEnumerator MoveClaw(Vector3 targetOffset)
    {
        float elapsedTime = 0f;
        Vector3 clawLeftStart = clawLeft.localPosition;
        Vector3 clawRightStart = clawRight.localPosition;

        Vector3 clawLeftTarget = clawLeftStart + targetOffset;
        Vector3 clawRightTarget = clawRightStart - targetOffset;

        while (elapsedTime < 1f)
        {
            clawLeft.localPosition = Vector3.Lerp(clawLeftStart, clawLeftTarget, elapsedTime);
            clawRight.localPosition = Vector3.Lerp(clawRightStart, clawRightTarget, elapsedTime);

            elapsedTime += Time.deltaTime * clawMovementSpeed;
            yield return null;
        }

        clawLeft.localPosition = clawLeftTarget;
        clawRight.localPosition = clawRightTarget;
    }
}
