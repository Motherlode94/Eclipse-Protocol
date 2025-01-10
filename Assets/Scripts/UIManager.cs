using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI moneyText;

    public void UpdateHealth(int currentHealth)
    {
        healthText.text = $"Health: {currentHealth}";
    }

    public void UpdateStamina(int currentStamina)
    {
        staminaText.text = $"Stamina: {currentStamina}";
    }

    public void UpdateXP(int currentXP)
    {
        xpText.text = $"XP: {currentXP}";
    }

    public void UpdateMoney(int currentMoney)
    {
        moneyText.text = $"Money: {currentMoney}";
    }
}