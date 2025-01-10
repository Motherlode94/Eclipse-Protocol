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

    public int currentMoney = 0;

    private UIManager uiManager;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        uiManager = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        uiManager.UpdateHealth(currentHealth);
    }

    public void RegenerateHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        uiManager.UpdateHealth(currentHealth);
    }

    public void UseStamina(int amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        uiManager.UpdateStamina(currentStamina);
    }

    public void RegenerateStamina(int amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        uiManager.UpdateStamina(currentStamina);
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= requiredXP)
        {
            LevelUp();
        }
        uiManager.UpdateXP(currentXP);
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        uiManager.UpdateMoney(currentMoney);
    }

    private void LevelUp()
    {
        currentXP -= requiredXP;
        requiredXP += 50; // Increases required XP for next level
        Debug.Log("Level Up!");
    }

    private void UpdateUI()
    {
        uiManager.UpdateHealth(currentHealth);
        uiManager.UpdateStamina(currentStamina);
        uiManager.UpdateXP(currentXP);
        uiManager.UpdateMoney(currentMoney);
    }
}