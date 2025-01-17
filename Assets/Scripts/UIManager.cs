using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI damageText; // Référence au texte de dégâts
    public GameObject interactionPrompt;
        public GameObject damageTextPrefab; // Préfabriqué pour le texte des dégâts
    public Canvas mainCanvas; // Canvas principal pour positionner le texte

    [Header("XP Display")]
    public GameObject xpLog; // GameObject affichant les gains d'XP
    public TextMeshProUGUI xpGainedText; // Texte pour le gain d'XP
    public TextMeshProUGUI xpTotalText; // Texte pour l'XP actuelle et requise

    [Header("Alert System")]
    public TextMeshProUGUI alertText;
    public GameObject levelUpPanel;

    [Header("Audio Feedback")]
    public AudioSource uiAudioSource;
    public AudioClip levelUpSound;
    public AudioClip questCompleteSound;
    private void Start()
    {
        if (damageText != null)
        {
            damageText.gameObject.SetActive(false);
        }
    }

    public void ShowDamageText(int damage, Vector3 position)
    {
        if (damageText != null)
        {
            damageText.text = $"-{damage}";
            damageText.transform.position = Camera.main.WorldToScreenPoint(position);
            damageText.gameObject.SetActive(true);

            // Masquer le texte après un court délai
            CancelInvoke(nameof(HideDamageText));
            Invoke(nameof(HideDamageText), 1.5f);
        }
    }

    private void Awake()
    {
        ValidateReferences();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (alertText != null)
        {
            alertText.gameObject.SetActive(false);
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        if (xpLog != null)
        {
            xpLog.SetActive(false);
        }
    }

    private void ValidateReferences()
    {
        if (healthText == null) Debug.LogError("healthText n'est pas assigné !");
        if (staminaText == null) Debug.LogError("staminaText n'est pas assigné !");
        if (xpText == null) Debug.LogError("xpText n'est pas assigné !");
        if (levelText == null) Debug.LogError("levelText n'est pas assigné !");
        if (moneyText == null) Debug.LogError("moneyText n'est pas assigné !");
        if (xpLog == null) Debug.LogWarning("xpLog n'est pas assigné !");
        if (alertText == null) Debug.LogWarning("alertText n'est pas assigné !");
    }

    public void UpdateHealth(int currentHealth)
    {
        healthText.text = $"HP : {currentHealth}";
        ApplyCriticalColor(healthText, currentHealth, 20);
    }

    public void UpdateStamina(int currentStamina)
    {
        staminaText.text = $"Stamina : {currentStamina}";
    }

    public void UpdateXP(int currentXP, int maxXP, int currentLevel, int xpGained)
    {
        // Affichage dans le panneau principal
        xpText.text = $"XP : {currentXP}/{maxXP}";
        levelText.text = $"Level : {currentLevel}";

        // Mise à jour du GameObject xpLog
        if (xpLog != null)
        {
            xpLog.SetActive(true);
            xpGainedText.text = $"+{xpGained} XP";
            xpTotalText.text = $"XP : {currentXP}/{maxXP}";

            // Masque le log après une courte durée
            CancelInvoke(nameof(HideXPLog));
            Invoke(nameof(HideXPLog), 2f);
        }

        if (currentXP >= maxXP)
        {
            TriggerLevelUp(currentLevel + 1);
        }
    }

    public void UpdateMoney(int currentMoney)
    {
        moneyText.text = $"Money : {currentMoney}";
        FlashText(moneyText, Color.green, 0.5f);
    }

    private void ApplyCriticalColor(TextMeshProUGUI textElement, int currentValue, int criticalThreshold)
    {
        textElement.color = currentValue < criticalThreshold ? Color.red : Color.white;
    }

    public void FlashText(TextMeshProUGUI textElement, Color flashColor, float duration)
    {
        StartCoroutine(FlashTextCoroutine(textElement, flashColor, duration));
    }

    private System.Collections.IEnumerator FlashTextCoroutine(TextMeshProUGUI textElement, Color flashColor, float duration)
    {
        if (textElement == null) yield break;

        Color originalColor = textElement.color;
        textElement.color = flashColor;

        yield return new WaitForSeconds(duration);

        textElement.color = originalColor;
    }

    public void ToggleInteractionPrompt(bool isActive, string message = "Press E")
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isActive);
            TextMeshProUGUI promptText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null)
            {
                promptText.text = message;
            }
        }
    }

    public void ShowAlert(string message, float duration = 2f)
    {
        if (alertText != null)
        {
            alertText.text = message;
            alertText.gameObject.SetActive(true);
            StartCoroutine(HideAlertAfterDuration(duration));
        }
    }

    private System.Collections.IEnumerator HideAlertAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (alertText != null) alertText.gameObject.SetActive(false);
    }

    private void TriggerLevelUp(int newLevel)
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            levelUpPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"LEVEL UP! Level {newLevel}";

            // Ajout d'une animation d'échelle
            StartCoroutine(AnimateLevelUpPanel(levelUpPanel.transform, 0.5f));

            if (uiAudioSource != null && levelUpSound != null)
            {
                uiAudioSource.PlayOneShot(levelUpSound);
            }

            Invoke(nameof(HideLevelUpPanel), 3f);
        }
    }

    private System.Collections.IEnumerator AnimateLevelUpPanel(Transform panelTransform, float duration)
    {
        Vector3 originalScale = panelTransform.localScale;
        Vector3 targetScale = originalScale * 1.2f; // Zoom 20%
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            panelTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            yield return null;
        }

        // Retour à la taille d'origine
        panelTransform.localScale = originalScale;
    }

    private void HideDamageText()
    {
        if (damageText != null)
        {
            damageText.gameObject.SetActive(false);
        }
    }

    private void HideLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    private void HideXPLog()
    {
        if (xpLog != null)
        {
            xpLog.SetActive(false);
        }
    }
}
