using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboticArmController : MonoBehaviour
{
    [Header("Joints Configuration")]
    [SerializeField] private Transform baseJoint; // Base du bras
    [SerializeField] private Transform arm1Joint; // Premier segment du bras
    [SerializeField] private Transform arm2Joint; // Deuxième segment du bras
    [SerializeField] private Transform clawJoint; // Pince ou extrémité

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 20f; // Vitesse de rotation des joints
    [SerializeField] private float clawSpeed = 10f; // Vitesse de mouvement de la pince

    [Header("Claw Settings")]
    [SerializeField] private Transform clawLeft;
    [SerializeField] private Transform clawRight;
    [SerializeField] private float clawOpenDistance = 0.2f; // Distance d'ouverture de la pince

    [Header("Player Settings")]
    [SerializeField] private GameObject player; // Référence au joueur
    [SerializeField] private PlayerController playerController; 

    private bool isClawOpen = true; // État actuel de la pince
    private bool isControllingArm = false; // Indique si le joueur contrôle le bras

    private void Update()
    {
        if (isControllingArm)
        {
        HandleBaseRotation();
        HandleArmMovement();
        HandleClawControl();

            if (Input.GetKeyDown(KeyCode.Escape))
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
            playerController.enabled = false; // Désactive les contrôles du joueur
        }

        isControllingArm = true;
    }

    public void ExitControl()
    {
        if (playerController != null)
        {
            Debug.Log("Player stopped controlling the robotic arm.");
            playerController.enabled = true; // Réactive les contrôles du joueur
        }

        isControllingArm = false;
    }

    private void HandleBaseRotation()
    {
        // Rotation de la base avec les touches gauche/droite
        float horizontalInput = Input.GetAxis("Horizontal"); // Mapping standard (A/D ou Flèches gauche/droite)
        baseJoint.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
    }

    private void HandleArmMovement()
    {
        // Rotation du premier segment du bras avec W/S
        float verticalInput = Input.GetAxis("Vertical"); // Mapping standard (W/S ou Flèches haut/bas)
        arm1Joint.Rotate(Vector3.right, verticalInput * rotationSpeed * Time.deltaTime);

        // Ajustement du deuxième segment avec Q/E
        if (Input.GetKey(KeyCode.Q))
        {
            arm2Joint.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            arm2Joint.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleClawControl()
    {
        // Ouvrir/fermer la pince avec la barre d'espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleClaw();
        }
    }

    private void ToggleClaw()
    {
        if (isClawOpen)
        {
            CloseClaw();
        }
        else
        {
            OpenClaw();
        }
    }

    private void OpenClaw()
    {
        if (clawLeft != null && clawRight != null)
        {
            clawLeft.localPosition += Vector3.left * clawOpenDistance;
            clawRight.localPosition += Vector3.right * clawOpenDistance;
        }
        isClawOpen = true;
    }

    private void CloseClaw()
    {
        if (clawLeft != null && clawRight != null)
        {
            clawLeft.localPosition -= Vector3.left * clawOpenDistance;
            clawRight.localPosition -= Vector3.right * clawOpenDistance;
        }
        isClawOpen = false;
    }
}
