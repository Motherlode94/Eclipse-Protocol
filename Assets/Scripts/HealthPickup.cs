using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HealthPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int healthAmount = 50; // Points de santé restaurés
    public AudioClip pickupSound; // Son lors du ramassage
    public GameObject pickupEffect; // Effet visuel du ramassage
    public float pickupEffectDuration = 2f; // Durée de l'effet visuel

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Ajoute de la santé au joueur
                playerStats.RegenerateHealth(healthAmount);
                Debug.Log($"Santé restaurée : {healthAmount}");

                // Joue le son de ramassage
                PlayPickupSound();

                // Joue l'effet visuel
                PlayPickupEffect();

                // Détruit l'objet après le ramassage
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Joue le son de ramassage.
    /// </summary>
    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    /// <summary>
    /// Joue l'effet visuel de ramassage.
    /// </summary>
    private void PlayPickupEffect()
    {
        if (pickupEffect != null)
        {
            GameObject effectInstance = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effectInstance, pickupEffectDuration);
        }
    }
}

