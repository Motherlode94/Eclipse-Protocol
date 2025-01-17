using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    public int maxStamina = 100;
    public int currentStamina;

    public int currentXP = 0;
    public int requiredXP = 100;
    public int currentLevel = 1;

    public int currentMoney = 0;

    private UIManager uiManager;

    [Header("Regeneration Settings")]
    public int healthRegenRate = 5; // Health regenerated per second
    public int staminaRegenRate = 10; // Stamina regenerated per second
    public float regenDelay = 5f; // Delay before regeneration starts
    private float regenTimer = 0f;

    [Header("Movement Settings")]
    public int staminaRunDrainRate = 10; // Consommation de stamina par seconde lors du run
    public int staminaSprintDrainRate = 20; // Consommation de stamina par seconde lors du sprint
    public bool isRunning = false; // Indique si le joueur court
    public bool isSprinting = false; // Indique si le joueur sprinte

    [Header("Death Settings")]
    public GameObject deathEffect;
    public AudioClip deathSound;
    private AudioSource audioSource;

    [Header("Temporary Buffs")]
    private float buffTimer = 0f;
    private int temporaryMaxHealthBonus = 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Vérifie si le UIManager est présent
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found in the scene!");
        }

        // Prépare l'AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        HandleStamina(); // Gestion de la stamina pour les mouvements
        RegenerateStats(); // Régénération après un délai
        HandleBuffTimer(); // Gestion des buffs temporaires
    }

    private void HandleStamina()
    {
        if (isSprinting && currentStamina > 0)
        {
            // Consomme de la stamina pour le sprint
            currentStamina -= Mathf.RoundToInt(staminaSprintDrainRate * Time.deltaTime);
        }
        else if (isRunning && currentStamina > 0)
        {
            // Consomme de la stamina pour la course normale (run)
            currentStamina -= Mathf.RoundToInt(staminaRunDrainRate * Time.deltaTime);
        }

        if (currentStamina <= 0)
        {
            // Arrête le sprint ou la course normale si la stamina atteint 0
            isSprinting = false;
            isRunning = false;
        }

        // Empêche la régénération pendant le mouvement
        if (isSprinting || isRunning)
        {
            regenTimer = 0f;
        }

        // Mise à jour de la stamina dans l'UI
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        uiManager?.UpdateStamina(currentStamina);
    }

    public void StartRun()
    {
        if (currentStamina > 0)
        {
            isRunning = true;
        }
    }

    public void StopRun()
    {
        isRunning = false;
    }

    public void StartSprint()
    {
        if (currentStamina > 0)
        {
            isSprinting = true;
        }
    }

    public void StopSprint()
    {
        isSprinting = false;
    }

    private void RegenerateStats()
    {
        if (regenTimer < regenDelay)
        {
            regenTimer += Time.deltaTime;
            return;
        }

        if (currentStamina < maxStamina)
        {
            currentStamina += Mathf.RoundToInt(staminaRegenRate * Time.deltaTime);
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            uiManager?.UpdateStamina(currentStamina);
        }

        if (currentHealth < maxHealth)
        {
            currentHealth += Mathf.RoundToInt(healthRegenRate * Time.deltaTime);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            uiManager?.UpdateHealth(currentHealth);
        }
    }

    public void TakeDamage(int damage)
    {
            currentHealth -= damage;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    uiManager?.UpdateHealth(currentHealth); // Met à jour l'interface utilisateur
    uiManager?.ShowDamageText(damage, transform.position); // Affiche les dégâts au joueur

    regenTimer = 0f; // Réinitialise le timer de régénération

    if (currentHealth <= 0)
    {
        Die();
    }
    
    }

    private void DisplayDamage(int damage)
    {
        uiManager?.ShowDamageText(damage);
    }

    private void Die()
    {
        Debug.Log("Player has died!");

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void RegenerateHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        uiManager?.UpdateHealth(currentHealth);
    }

    public void UseStamina(float amount)
    {
        currentStamina -= Mathf.RoundToInt(amount);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        uiManager?.UpdateStamina(currentStamina);

        Debug.Log($"Stamina utilisée : {amount:F2}, Stamina actuelle : {currentStamina}");
    }

    public void RegenerateStamina(float amount)
    {
        currentStamina += Mathf.RoundToInt(amount);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        uiManager?.UpdateStamina(currentStamina);
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= requiredXP)
        {
            LevelUp();
        }
        uiManager?.UpdateXP(currentXP, requiredXP, currentLevel, amount);
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        uiManager?.UpdateMoney(currentMoney);
        Debug.Log($"Money gained: {amount}");
    }

    private void LevelUp()
    {
        currentXP -= requiredXP;
        currentLevel++;
        requiredXP += 50;
        maxHealth += 10;
        maxStamina += 5;
        Debug.Log("Level Up!");
        uiManager?.UpdateXP(currentXP, requiredXP, currentLevel, 0);
        UpdateUI();
    }

    private void UpdateUI()
    {
        uiManager?.UpdateHealth(currentHealth);
        uiManager?.UpdateStamina(currentStamina);
        uiManager?.UpdateXP(currentXP, requiredXP, currentLevel, 0);
        uiManager?.UpdateMoney(currentMoney);
    }

    public void ApplyTemporaryHealthBuff(int bonusAmount, float duration)
    {
        temporaryMaxHealthBonus = bonusAmount;
        maxHealth += bonusAmount;
        currentHealth += bonusAmount; // Augmente également la santé actuelle
        buffTimer = duration;

        Debug.Log($"Buff appliqué : +{bonusAmount} santé pour {duration} secondes");
    }

    private void HandleBuffTimer()
    {
        if (buffTimer > 0)
        {
            buffTimer -= Time.deltaTime;

            if (buffTimer <= 0)
            {
                maxHealth -= temporaryMaxHealthBonus;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                temporaryMaxHealthBonus = 0;

                Debug.Log("Buff expiré !");
            }
        }
    }

    public void SaveStats()
    {
        PlayerPrefs.SetInt("Health", currentHealth);
        PlayerPrefs.SetInt("Stamina", currentStamina);
        PlayerPrefs.SetInt("XP", currentXP);
        PlayerPrefs.SetInt("Level", currentLevel);
        PlayerPrefs.SetInt("Money", currentMoney);
        PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        currentHealth = PlayerPrefs.GetInt("Health", maxHealth);
        currentStamina = PlayerPrefs.GetInt("Stamina", maxStamina);
        currentXP = PlayerPrefs.GetInt("XP", 0);
        currentLevel = PlayerPrefs.GetInt("Level", 1);
        currentMoney = PlayerPrefs.GetInt("Money", 0);
        UpdateUI();
    }
}
