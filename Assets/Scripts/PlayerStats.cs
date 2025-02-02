using System.Collections;
using System.Collections.Generic;
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

    public int Money
{
    get { return currentMoney; }
    set { currentMoney = Mathf.Max(0, value); } // Empêche une valeur négative
}
public int currentMoney = 0;

    private UIManager uiManager;

    [Header("Système de Prime")]
public int bounty = 0; // Prime actuelle du joueur
public int bountyThreshold1 = 50; // Seuil où les ennemis deviennent plus agressifs
public int bountyThreshold2 = 100; // Seuil où tous les ennemis attaquent à vue


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
    public float SpeedMultiplier = 1.0f; // Modifie la vitesse max du vaisseau
    public float ThrustMultiplier = 1.0f; // Modifie la force de propulsion
    public float ThrustForce = 10f;
    public float MaxSpeed = 20f;
    public float RotationSpeed = 50f;
    public float BoostMultiplier = 2f;
    private List<float> healthBuffs = new List<float>();
    private List<float> staminaBuffs = new List<float>();
    public int Reputation = 50; // Valeur initiale
    public int WantedLevel = 0; // Niveau de criminalité initial


    [Header("Fatigue Settings")]
    public bool isFatigued = false; 
public float fatigueSpeedMultiplier = 0.5f; // Vitesse réduite de 50%
public float staminaFatigueThreshold = 5f; // Niveau de stamina où la fatigue commence



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
        currentStamina -= Mathf.RoundToInt(staminaSprintDrainRate * Time.deltaTime);
    }
    else if (isRunning && currentStamina > 0)
    {
        currentStamina -= Mathf.RoundToInt(staminaRunDrainRate * Time.deltaTime);
    }

    if (currentStamina <= 0)
    {
        isSprinting = false;
        isRunning = false;
        isFatigued = true; // Active la fatigue
        SpeedMultiplier = fatigueSpeedMultiplier;
    }

    if (currentStamina > staminaFatigueThreshold)
    {
        isFatigued = false; // Le joueur récupère
        SpeedMultiplier = 1.0f;
    }

    regenTimer = isRunning || isSprinting ? 0f : regenTimer;
    currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    uiManager?.UpdateStamina(currentStamina);
    }

    public void ModifyReputation(int amount)
    {
    Reputation = Mathf.Clamp(Reputation + amount, 0, 100);
    UpdateReputationEffects();
    }

    public void ModifyWantedLevel(int amount)
    {
    WantedLevel = Mathf.Max(0, WantedLevel + amount);
    UpdateWantedLevelEffects();
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

    // Ajout de la position actuelle du joueur pour afficher les dégâts
    uiManager?.ShowDamageText(damage, transform.position);

    regenTimer = 0f; // Réinitialise le timer de régénération

    if (currentHealth <= 0)
    {
        Die();
    }
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
        UpdateUI(); // Mets à jour l'interface utilisateur
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

    public void ApplyBuff(float healthBonus, float staminaBonus, float duration)
{
    if (healthBonus > 0)
    {
        maxHealth += Mathf.RoundToInt(healthBonus);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBuffs.Add(duration);
    }

    if (staminaBonus > 0)
    {
        maxStamina += Mathf.RoundToInt(staminaBonus);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaBuffs.Add(duration);
    }

    StartCoroutine(HandleBuffTimer());
}

public void UpdateReputationEffects()
{
    if (Reputation >= 80)
    {
        Debug.Log("Le joueur est très respecté. Les civils aident plus souvent.");
    }
    else if (Reputation < 30)
    {
        Debug.Log("Le joueur est mal vu. Certains NPCs refusent d’interagir.");
    }
}

public void UpdateWantedLevelEffects()
{
    if (WantedLevel >= 50)
    {
        Debug.Log("La police recherche activement le joueur !");
    }
    else if (WantedLevel >= 20)
    {
        Debug.Log("La police surveille le joueur de loin.");
    }
}

public void ModifyBounty(int amount)
{
    bounty += amount;
    bounty = Mathf.Max(0, bounty); // Empêche une valeur négative
    UpdateBountyEffects();
}

private void UpdateBountyEffects()
{
    if (bounty >= bountyThreshold2)
    {
        Debug.Log("Tous les ennemis attaquent à vue !");
    }
    else if (bounty >= bountyThreshold1)
    {
        Debug.Log("Les ennemis sont plus agressifs.");
    }
    else
    {
        Debug.Log("La prime du joueur est basse, comportement normal.");
    }
}

public void UpdateFactionReactions()
{
    // Police
    if (WantedLevel >= 50)
    {
        Debug.Log("La police tire à vue !");
    }
    else if (WantedLevel >= 20)
    {
        Debug.Log("La police vous surveille.");
    }

    // Rebelles
    if (Reputation <= 30)
    {
        Debug.Log("Les rebelles vous font confiance.");
    }
    else if (Reputation >= 70)
    {
        Debug.Log("Les rebelles vous considèrent comme un ennemi.");
    }

    // Mercenaires
    if (bounty >= 100)
    {
        Debug.Log("Les mercenaires veulent vous capturer pour la prime !");
    }
}




private IEnumerator HandleBuffTimer()
{
    while (healthBuffs.Count > 0 || staminaBuffs.Count > 0)
    {
        for (int i = healthBuffs.Count - 1; i >= 0; i--)
        {
            healthBuffs[i] -= Time.deltaTime;
            if (healthBuffs[i] <= 0)
            {
                maxHealth -= Mathf.RoundToInt(healthBuffs[i]);
                healthBuffs.RemoveAt(i);
            }
        }

        for (int i = staminaBuffs.Count - 1; i >= 0; i--)
        {
            staminaBuffs[i] -= Time.deltaTime;
            if (staminaBuffs[i] <= 0)
            {
                maxStamina -= Mathf.RoundToInt(staminaBuffs[i]);
                staminaBuffs.RemoveAt(i);
            }
        }

        yield return null;
    }
}

    public void ApplyTemporaryHealthBuff(int bonusAmount, float duration)
    {
        temporaryMaxHealthBonus = bonusAmount;
        maxHealth += bonusAmount;
        currentHealth += bonusAmount; // Augmente également la santé actuelle
        buffTimer = duration;

        Debug.Log($"Buff appliqué : +{bonusAmount} santé pour {duration} secondes");
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
