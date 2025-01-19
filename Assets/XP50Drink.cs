using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XP50Drink : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int xpAmount = 50; // Quantité d'XP donnée
    [SerializeField] private AudioClip drinkSound; // Son joué lorsqu'on boit
    [SerializeField] private GameObject pickupEffect; // Effet visuel de consommation

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Ajoute de l'XP au joueur
                playerStats.GainXP(xpAmount);

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
