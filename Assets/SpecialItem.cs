using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialItem : MonoBehaviour
{
    public int xpReward = 50; // XP donné par l'objet
    public AudioClip collectSound; // Son de collecte
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Vérifie si c'est le joueur
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.GainXP(xpReward); // Ajoute les XP au joueur

                // Joue un son de collecte (facultatif)
                if (collectSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(collectSound);
                }

                Debug.Log("Objet collecté : +" + xpReward + " XP !");
                Destroy(gameObject); // Détruit l'objet après collecte
            }
        }
    }
}
