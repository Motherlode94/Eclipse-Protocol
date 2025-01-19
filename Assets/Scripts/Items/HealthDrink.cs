using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDrink : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int healthAmount = 30; // Quantité de santé restaurée
    [SerializeField] private AudioClip drinkSound; // Son joué lorsqu'on boit
    [SerializeField] private GameObject pickupEffect; // Effet visuel de consommation

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Augmente la santé du joueur
                playerStats.RegenerateHealth(healthAmount);

                // Joue un son de consommation
                if (drinkSound != null)
                {
                    AudioSource.PlayClipAtPoint(drinkSound, transform.position);
                }

                // Affiche un effet visuel
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // Détruit l'objet après utilisation
                Destroy(gameObject);
            }
        }
    }
}
