using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpSystem : MonoBehaviour
{
    [Header("Level Up Settings")]
    [SerializeField] private int xpRequiredToLevelUp = 100; // XP nécessaire pour monter de niveau
    [SerializeField] private int levelUpXPIncrease = 50; // Augmentation du seuil d'XP à chaque niveau
    [SerializeField] private int healthBonusPerLevel = 10; // Bonus de santé max par niveau
    [SerializeField] private int staminaBonusPerLevel = 5; // Bonus d'endurance max par niveau
    [SerializeField] private GameObject levelUpEffect; // Effet visuel lors de la montée de niveau
    [SerializeField] private AudioClip levelUpSound; // Son joué lors de la montée de niveau

    [SerializeField] private PlayerStats playerStats; // Assigné via l'inspector

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component is missing on the Player!");
        }
    }

    private void Update()
    {
        if (playerStats != null && playerStats.currentXP >= playerStats.requiredXP)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Debug.Log("Player leveled up!");

        // Augmente le niveau du joueur
        playerStats.currentLevel++;

        // Réinitialise l'XP et augmente le seuil requis pour le prochain niveau
        playerStats.currentXP -= playerStats.requiredXP;
        playerStats.requiredXP += levelUpXPIncrease;

        // Ajoute les bonus de santé et d'endurance
        playerStats.maxHealth += healthBonusPerLevel;
        playerStats.maxStamina += staminaBonusPerLevel;
        playerStats.currentHealth = playerStats.maxHealth; // Restaure la santé
        playerStats.currentStamina = playerStats.maxStamina; // Restaure l'endurance

        // Affiche un effet visuel
        if (levelUpEffect != null)
        {
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
        }

        // Joue un son
        if (levelUpSound != null)
        {
            AudioSource.PlayClipAtPoint(levelUpSound, transform.position);
        }

        // Met à jour l'interface utilisateur via PlayerStats
        playerStats.SaveStats();
        Debug.Log($"New Level: {playerStats.currentLevel}, XP for next level: {playerStats.requiredXP}");
    }

    public void ResetLevel()
{
    playerStats.currentLevel = 1;
    playerStats.requiredXP = xpRequiredToLevelUp;
    playerStats.currentXP = 0;
    playerStats.maxHealth = 100;
    playerStats.maxStamina = 100;
    playerStats.SaveStats();
    Debug.Log("Le niveau et les stats ont été réinitialisés !");
}

}
