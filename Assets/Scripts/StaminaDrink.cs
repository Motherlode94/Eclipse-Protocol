using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaDrink : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private int staminaAmount = 40; // Quantité d'endurance restaurée
    [SerializeField] private AudioClip drinkSound; // Son joué lorsqu'on boit
    [SerializeField] private GameObject pickupEffect; // Effet visuel de consommation

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Augmente l'endurance du joueur
                playerStats.RegenerateStamina(staminaAmount);

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
