using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Destination de téléportation (un autre portail ou monde)")]
    [SerializeField] private Transform teleportDestination;
    [Tooltip("Effet visuel pour la téléportation")]
    [SerializeField] private GameObject teleportEffect;
    [Tooltip("Son joué pendant la téléportation")]
    [SerializeField] private AudioClip teleportSound;
    [Tooltip("Délai avant que l'entité puisse utiliser le portail à nouveau")]
    [SerializeField] private float teleportCooldown = 2f;

    private bool canTeleport = true; // Indique si la téléportation est possible
    private AudioSource audioSource;

    private Renderer portalRenderer; // Pour changer la couleur pendant le cooldown
    private Color originalColor;

    [Header("World Management")]
    [Tooltip("Référence au système de gestion des mondes (Zone.cs)")]
    [SerializeField] private Zone currentZone;
    [Tooltip("Règles spécifiques au monde cible")]
    [SerializeField] private string targetWorldRules;

    private void Start()
    {
        // Ajout d'une source audio pour jouer les sons
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Enregistrement de la couleur d'origine pour feedback visuel
        portalRenderer = GetComponent<Renderer>();
        if (portalRenderer != null)
        {
            originalColor = portalRenderer.material.color;
        }

        // Vérifie si la destination est correctement configurée
        if (teleportDestination == null || !teleportDestination.GetComponent<Collider>()?.isTrigger == true)
        {
            Debug.LogError("Destination non configurée ou mal configurée !");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet qui entre est le joueur ou un autre type autorisé (PNJ, objet, etc.)
        if ((other.CompareTag("Player") || other.CompareTag("Ally") || other.CompareTag("Object")) && canTeleport)
        {
            TeleportEntity(other.gameObject);
        }
    }

    private void TeleportEntity(GameObject entity)
    {
        if (teleportDestination == null)
        {
            Debug.LogError("Aucune destination spécifiée pour ce portail !");
            return;
        }

        // Désactive temporairement la téléportation pour éviter les boucles infinies
        StartCoroutine(TeleportCooldown());

        // Synchronise avec le système de gestion des mondes
        if (currentZone != null)
        {
            currentZone.ApplyWorldRules(targetWorldRules);
        }

        // Joue l'effet visuel au départ
        PlayTeleportEffect(entity.transform.position);

        // Téléporte l'entité avec position et rotation
        entity.transform.SetParent(null); // Détache l'objet de tout parent
        entity.transform.position = teleportDestination.position;
        entity.transform.rotation = teleportDestination.rotation; // Aligne la rotation sur la destination

        // Joue l'effet visuel à l'arrivée
        PlayTeleportEffect(teleportDestination.position);

        // Joue le son de téléportation
        PlayTeleportSound();

        Debug.Log($"{entity.name} téléporté au portail de destination : {teleportDestination.name}");
    }

    private void PlayTeleportEffect(Vector3 position)
    {
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, position, Quaternion.identity);
        }
    }

    private void PlayTeleportSound()
    {
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }

    private IEnumerator TeleportCooldown()
    {
        // Active le cooldown et change la couleur
        canTeleport = false;
        if (portalRenderer != null)
        {
            portalRenderer.material.color = Color.red; // Couleur pendant le cooldown
        }

        yield return new WaitForSeconds(teleportCooldown);

        // Réinitialise l'état
        canTeleport = true;
        if (portalRenderer != null)
        {
            portalRenderer.material.color = originalColor;
        }
    }
}
