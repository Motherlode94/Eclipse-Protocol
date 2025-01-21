using UnityEngine;

public class LevelUpSystem : MonoBehaviour
{
    [Header("Level Up Settings")]
    [SerializeField] private int baseXPRequired = 100; // XP de base pour le niveau 1
    [SerializeField] private float xpMultiplier = 1.5f; // Multiplicateur pour augmenter l'XP requis par niveau
    [SerializeField] private int healthBonusPerLevel = 10; // Bonus de santé max par niveau
    [SerializeField] private int staminaBonusPerLevel = 5; // Bonus d'endurance max par niveau
    [SerializeField] private GameObject levelUpEffect; // Effet visuel lors de la montée de niveau
    [SerializeField] private AudioClip levelUpSound; // Son joué lors de la montée de niveau

    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats; // Assigné via l'inspector

    private AudioSource audioSource;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("PlayerStats component is missing! Disabling LevelUpSystem.");
                enabled = false;
                return;
            }
        }

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // Initialisation des XP requis pour le premier niveau
        playerStats.requiredXP = baseXPRequired;
    }

    private void Update()
    {
        if (CanLevelUp())
        {
            LevelUp();
        }
    }

    private bool CanLevelUp()
    {
        return playerStats.currentXP >= playerStats.requiredXP;
    }

    private void LevelUp()
    {
        Debug.Log("Player leveled up!");

        // Augmente le niveau
        playerStats.currentLevel++;

        // Réinitialise l'XP et calcule les XP requis pour le prochain niveau
        playerStats.currentXP -= playerStats.requiredXP;
        playerStats.requiredXP = Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpMultiplier, playerStats.currentLevel - 1));

        // Ajoute les bonus
        playerStats.maxHealth += healthBonusPerLevel;
        playerStats.maxStamina += staminaBonusPerLevel;
        playerStats.currentHealth = playerStats.maxHealth; // Restaure la santé
        playerStats.currentStamina = playerStats.maxStamina; // Restaure l'endurance

        // Déclenche les effets visuels et sonores
        PlayLevelUpEffect();
        PlayLevelUpSound();

        // Sauvegarde les stats et notifie
        playerStats.SaveStats();
        Debug.Log($"Niveau actuel : {playerStats.currentLevel}, XP pour le prochain niveau : {playerStats.requiredXP}");
        NotifyLevelUp();
    }

    private void PlayLevelUpEffect()
    {
        if (levelUpEffect != null)
        {
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
        }
    }

    private void PlayLevelUpSound()
    {
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
    }

    private void NotifyLevelUp()
    {
        // Exemple : Affiche un message de montée de niveau dans l'UI
        Debug.Log("Nouvelle compétence débloquée !");
        // Vous pouvez ajouter ici un système pour notifier les nouvelles compétences ou améliorations débloquées
    }

    public void ResetLevel()
    {
        playerStats.currentLevel = 1;
        playerStats.requiredXP = baseXPRequired;
        playerStats.currentXP = 0;
        playerStats.maxHealth = 100;
        playerStats.maxStamina = 100;
        playerStats.SaveStats();
        Debug.Log("Le niveau et les stats ont été réinitialisés !");
    }
}
