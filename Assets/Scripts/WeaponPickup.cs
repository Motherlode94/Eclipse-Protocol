using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject weaponPrefab; // L'arme à ramasser
    public AudioClip pickupSound; // Son de ramassage
    public ParticleSystem pickupEffect; // Effet visuel de ramassage
    public string dialogueMessage = "Vous avez trouvé une arme ! Appuyez sur 'Interact' pour la ramasser."; // Message de dialogue

    private bool playerInRange = false; // Indique si le joueur est dans la zone
    private PlayerController playerController; // Référence au joueur
    private bool hasDisplayedDialogue = false; // Empêche le dialogue de se répéter

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Récupérer la référence au PlayerController
            playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                // Abonnez-vous à l'événement d'interaction
                playerController.OnInteractEvent += HandleInteraction;
            }

            // Affiche un dialogue uniquement une fois
            if (!hasDisplayedDialogue)
            {
                DialogueSystem dialogueSystem = FindObjectOfType<DialogueSystem>();
                if (dialogueSystem != null)
                {
                    dialogueSystem.StartDialogue(new string[] { dialogueMessage });
                }
                hasDisplayedDialogue = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Retirer l'abonnement à l'événement
            if (playerController != null)
            {
                playerController.OnInteractEvent -= HandleInteraction;
                playerController = null;
            }
        }
    }

    private void HandleInteraction()
    {
        if (playerInRange)
        {
            PickupWeapon();
        }
    }

    private void PickupWeapon()
    {
        WeaponSystem weaponSystem = FindObjectOfType<WeaponSystem>();
        if (weaponSystem != null)
        {
            // Vérifie si l'arme est déjà dans l'inventaire
            if (weaponSystem.weaponInventory.Contains(weaponPrefab))
            {
                Debug.Log("Cette arme est déjà dans l'inventaire.");
                return;
            }

            // Ajouter et équiper l'arme
            weaponSystem.weaponInventory.Add(weaponPrefab);
            weaponSystem.EquipWeapon(weaponPrefab, "right");

            // Effets visuels et sonores
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            Debug.Log("Arme ramassée : " + weaponPrefab.name);

            // Désactiver et détruire l'objet
            Destroy(gameObject, 0.5f);
        }
        else
        {
            Debug.LogWarning("Aucun système d'arme trouvé sur le joueur !");
        }
    }
}
