using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SavePoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats; // Référence aux statistiques du joueur
    [SerializeField] private Transform spawnPoint; // Point de réapparition
    [SerializeField] private GameObject saveEffect; // Effet visuel lors de la sauvegarde
    [SerializeField] private string interactionPrompt = "Appuyez sur E pour sauvegarder et avancer le temps";

    [Header("Time Settings")]
    [SerializeField] private int timeToAdvance = 6; // Nombre d'heures à avancer
    [SerializeField] private int dayLengthInHours = 24; // Durée d'une journée en heures

    private bool isPlayerNearby = false;

    #region Unity Methods

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            HideInteractionPrompt();
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SaveGame();
        }
    }

    #endregion

    #region Save System

    private void SaveGame()
    {
        Debug.Log("Sauvegarde effectuée !");
        SimulateTimeAdvance();
        RestorePlayerStats();
        SavePlayerData();

        // Ajouter un effet visuel
        if (saveEffect != null)
        {
            Instantiate(saveEffect, spawnPoint.position, Quaternion.identity);
        }
    }

    private void SimulateTimeAdvance()
    {
        // Simulation d'un cycle jour/nuit en avançant le temps
        int currentHour = PlayerPrefs.GetInt("GameHour", 8); // Heure actuelle, par défaut 8h
        int newHour = (currentHour + timeToAdvance) % dayLengthInHours;

        Debug.Log($"Temps avancé de {timeToAdvance} heures. Nouvelle heure : {newHour}");
        PlayerPrefs.SetInt("GameHour", newHour);
        PlayerPrefs.Save();
    }

    private void RestorePlayerStats()
    {
        if (playerStats != null)
        {
            playerStats.currentHealth = playerStats.maxHealth; // Restaure la santé
            playerStats.currentStamina = playerStats.maxStamina; // Restaure la stamina

            Debug.Log("Statistiques du joueur restaurées !");
        }
    }

    private void SavePlayerData()
    {
        if (playerStats != null)
        {
            playerStats.SaveStats(); // Sauvegarde les statistiques du joueur
        }

        // Vous pouvez également sauvegarder d'autres données spécifiques ici
        PlayerPrefs.SetFloat("PlayerPositionX", spawnPoint.position.x);
        PlayerPrefs.SetFloat("PlayerPositionY", spawnPoint.position.y);
        PlayerPrefs.SetFloat("PlayerPositionZ", spawnPoint.position.z);
        PlayerPrefs.Save();

        Debug.Log("Données du joueur sauvegardées !");
    }

    #endregion

    #region UI Management

    private void ShowInteractionPrompt()
    {
        Debug.Log(interactionPrompt);
        // Vous pouvez afficher un UI ici pour informer le joueur
    }

    private void HideInteractionPrompt()
    {
        Debug.Log("Interaction prompt caché.");
        // Vous pouvez cacher le UI ici
    }

    #endregion
}
