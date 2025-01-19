using System.Collections;
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

    private void Start()
    {
        // Ajout d'une source audio pour jouer les sons
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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

        // Téléporte le joueur
        player.transform.SetParent(null); // Détache l'objet de tout parent
        player.transform.position = teleportDestination.position;

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
        canTeleport = false;
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }
}

