using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HealthItem : MonoBehaviour
{
    public enum HealthItemType
    {
        Pickup, // Ramassable directement
        Consumable // Nécessite une action pour consommer
    }

    [Header("Health Item Settings")]
    public HealthItemType itemType = HealthItemType.Pickup; // Type d'objet
    [SerializeField] private int healthAmount = 50; // Quantité de santé restaurée
    [SerializeField] private AudioClip useSound; // Son joué lors de l'utilisation
    [SerializeField] private GameObject useEffect; // Effet visuel lors de l'utilisation
    [SerializeField] private KeyCode consumeKey = KeyCode.E; // Touche pour consommer un objet de type Consumable

    private bool isNearPlayer = false;
    private PlayerStats playerStats;

    private void Update()
    {
        if (itemType == HealthItemType.Consumable && isNearPlayer && Input.GetKeyDown(consumeKey))
        {
            UseItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                if (itemType == HealthItemType.Pickup)
                {
                    UseItem(); // Utilise l'objet immédiatement si c'est un Pickup
                }
                else
                {
                    isNearPlayer = true; // Permet d'utiliser l'objet avec une action
                    Debug.Log("Appuyez sur E pour consommer l'objet.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && itemType == HealthItemType.Consumable)
        {
            isNearPlayer = false;
            Debug.Log("Vous êtes trop loin pour consommer l'objet.");
        }
    }

    private void UseItem()
    {
        if (playerStats != null)
        {
            // Restaure la santé du joueur
            playerStats.RegenerateHealth(healthAmount);
            Debug.Log($"Santé restaurée : {healthAmount}");

            // Joue le son de l'objet
            if (useSound != null)
            {
                AudioSource.PlayClipAtPoint(useSound, transform.position);
            }

            // Affiche un effet visuel
            if (useEffect != null)
            {
                Instantiate(useEffect, transform.position, Quaternion.identity);
            }

            // Détruit l'objet après utilisation
            Destroy(gameObject);
        }
    }
}
