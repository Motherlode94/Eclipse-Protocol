using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f; // Distance maximale pour interagir
    [SerializeField] private LayerMask interactionLayer; // Masque des objets interactifs
    [SerializeField] private InputAction interactAction; // Action d'interaction (ex. "E")

    private GameObject currentInteractable; // L'objet interactif détecté

    private void OnEnable()
    {
        interactAction.Enable();
        interactAction.performed += HandleInteraction;
    }

    private void OnDisable()
    {
        interactAction.Disable();
        interactAction.performed -= HandleInteraction;
    }

    private void Update()
    {
        DetectInteractable();
    }

    private void DetectInteractable()
    {
        // Vérifie les objets interactifs proches du joueur
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, interactionLayer))
        {
            if (hit.collider != null)
            {
                currentInteractable = hit.collider.gameObject;
                Debug.Log($"Interactable detected: {currentInteractable.name}");
            }
        }
        else
        {
            currentInteractable = null; // Aucun objet interactif détecté
        }
    }

    private void HandleInteraction(InputAction.CallbackContext context)
    {
        if (currentInteractable == null)
        {
            Debug.Log("No interactable detected.");
            return;
        }

        // Vérifie si l'objet détecté comporte le script `VehicleInteraction`
        VehicleInteraction vehicle = currentInteractable.GetComponent<VehicleInteraction>();
        if (vehicle != null)
        {
            Debug.Log($"Interacting with vehicle: {currentInteractable.name}");
            vehicle.ForceExit(); // Ou une autre méthode d'interaction
        }
        else
        {
            Debug.Log($"No VehicleInteraction script found on {currentInteractable.name}");
        }
    }
}
