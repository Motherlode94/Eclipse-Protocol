using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Portée de l'interaction")]
    [SerializeField] private float interactionRange = 3f;
    [Tooltip("Couches des objets interactifs")]
    [SerializeField] private LayerMask interactionLayer;
    [Tooltip("Action d'interaction (ex. touche E)")]
    [SerializeField] private InputAction interactAction;

    private GameObject currentInteractable; // Objet interactif détecté

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
        // Vérifie si un objet interactif est à portée
        RaycastHit hit;
        bool hasHit = Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, interactionLayer);

        if (hasHit)
        {
            GameObject detectedObject = hit.collider.gameObject;

            // Evite les traitements inutiles si l'objet détecté est le même
            if (currentInteractable != detectedObject)
            {
                currentInteractable = detectedObject;
                Debug.Log($"Nouveau interactable détecté : {currentInteractable.name}");
            }
        }
        else if (currentInteractable != null)
        {
            Debug.Log("Aucun interactable détecté.");
            currentInteractable = null;
        }
    }

    private void HandleInteraction(InputAction.CallbackContext context)
    {
        // Vérifie qu'il y a un objet interactif
        if (currentInteractable == null)
        {
            Debug.Log("Aucun interactable à portée.");
            return;
        }

        // Tente d'accéder au script VehicleInteraction
        VehicleInteraction vehicle = currentInteractable.GetComponent<VehicleInteraction>();
        if (vehicle != null)
        {
            Debug.Log($"Interaction avec le véhicule : {currentInteractable.name}");
            vehicle.ForceExit(); // Appelle la méthode d'interaction
        }
        else
        {
            Debug.Log($"L'objet {currentInteractable.name} n'est pas un véhicule.");
        }
    }
}
