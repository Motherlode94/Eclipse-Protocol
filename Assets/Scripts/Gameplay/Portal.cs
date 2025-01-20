using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Destination de téléportation (un autre portail)")]
    [SerializeField] private Transform teleportDestination;
    [Tooltip("Effet visuel pour la téléportation")]
    [SerializeField] private GameObject teleportEffect;
    [Tooltip("Son joué pendant la téléportation")]
    [SerializeField] private AudioClip teleportSound;
    [Tooltip("Délai avant que le joueur puisse utiliser le portail à nouveau")]
    [SerializeField] private float teleportCooldown = 2f;

    private bool canTeleport = true; // Indique si la téléportation est possible
    private AudioSource audioSource;

    private Renderer portalRenderer; // Pour changer la couleur pendant le cooldown
    private Color originalColor;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet qui entre est le joueur
        if (other.CompareTag("Player") && canTeleport)
        {
            TeleportPlayer(other.gameObject);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        if (teleportDestination == null)
        {
            Debug.LogError("Aucune destination spécifiée pour ce portail !");
            return;
        }

        // Désactive temporairement la téléportation pour éviter les boucles infinies
        StartCoroutine(TeleportCooldown());

        // Joue l'effet visuel au départ
        PlayTeleportEffect(player.transform.position);

        // Téléporte le joueur avec position et rotation
        player.transform.SetParent(null); // Détache l'objet de tout parent
        player.transform.position = teleportDestination.position;
        player.transform.rotation = teleportDestination.rotation; // Aligne la rotation sur le portail de destination

        // Joue l'effet visuel à l'arrivée
        PlayTeleportEffect(teleportDestination.position);

        // Joue le son de téléportation
        PlayTeleportSound();

        Debug.Log($"Player téléporté au portail de destination : {teleportDestination.name}");
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
