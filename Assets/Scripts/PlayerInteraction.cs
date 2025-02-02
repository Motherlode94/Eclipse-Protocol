using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Portée de l'interaction")]
    [SerializeField] private float interactionRange = 5f;
    [Tooltip("Couches des objets interactifs")]
    [SerializeField] private LayerMask interactionLayer;
    [Tooltip("Action d'interaction (ex. touche E)")]
    [SerializeField] private InputAction interactAction;

    private GameObject currentInteractable; // Objet interactif détecté
    private RaycastHit lastHit; // Dernier Raycast détecté

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

    /// <summary>
    /// Détecte les objets interactifs à portée via un Raycast.
    /// </summary>
    private void DetectInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward); // Raycast vers l'avant
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            GameObject detectedObject = hit.collider.gameObject;

            // Évite les répétitions inutiles
            if (currentInteractable != detectedObject)
            {
                currentInteractable = detectedObject;
                Debug.Log($"Nouveau interactable détecté : {currentInteractable.name}");

                // Affiche un message visuel (si applicable)
                DisplayInteractionMessage(currentInteractable);
            }
        }
        else
        {
            // Si aucun objet interactif n'est détecté
            if (currentInteractable != null)
            {
                Debug.Log("Aucun interactable détecté.");
                HideInteractionMessage(); // Cache le message visuel
                currentInteractable = null;
            }
        }
    }

    /// <summary>
    /// Gestion de l'interaction lorsque l'utilisateur appuie sur la touche d'interaction.
    /// </summary>
    private void HandleInteraction(InputAction.CallbackContext context)
    {
        if (currentInteractable == null)
        {
            Debug.Log("Aucun objet interactif à portée.");
            return;
        }

        // Tente d'accéder au script PickupItem
        PickupItem pickupItem = currentInteractable.GetComponent<PickupItem>();
        if (pickupItem != null)
        {
            pickupItem.Interact(); // Ramasse l'objet
            Debug.Log($"PickupItem détecté et ramassé : {currentInteractable.name}");
            return; // Pas besoin de continuer si c'est un PickupItem
        }

        // Tente d'accéder au script VehicleInteraction
        VehicleInteraction vehicle = currentInteractable.GetComponent<VehicleInteraction>();
        if (vehicle != null)
        {
            Debug.Log($"Interaction avec le véhicule : {currentInteractable.name}");
            vehicle.ForceExit(); // Appelle la méthode d'interaction
            return;
        }

        // Si aucun script compatible n'est trouvé
        Debug.Log($"L'objet {currentInteractable.name} n'est ni un PickupItem ni un véhicule.");
    }

    /// <summary>
    /// Affiche un message visuel d'interaction (par exemple, sur l'UI).
    /// </summary>
    /// <param name="interactable">Objet interactif détecté.</param>
    private void DisplayInteractionMessage(GameObject interactable)
    {
        // Implémentez la logique pour afficher un message dans l'UI
        Debug.Log($"[Message UI] Appuyez sur E pour interagir avec {interactable.name}.");
    }

    /// <summary>
    /// Cache le message visuel d'interaction.
    /// </summary>
    private void HideInteractionMessage()
    {
        // Implémentez la logique pour cacher le message dans l'UI
        Debug.Log("[Message UI] Interaction annulée.");
    }
}
