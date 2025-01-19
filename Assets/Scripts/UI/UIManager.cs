using UnityEngine;
using TMPro;
using System.Collections.Generic; // Nécessaire pour utiliser List<>

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moneyText;
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

    [Header("Mission System")]
    public GameObject objectivesPanel; // Panneau des objectifs
    public TextMeshProUGUI missionText; // Texte pour afficher la mission active
    public List<TextMeshProUGUI> objectiveTexts; // Liste des textes pour les objectifs
    public TextMeshProUGUI chapterTitleText; // Texte pour le titre du chapitre
    public TextMeshProUGUI locationText; // Texte pour la localisation actuelle
    public TextMeshProUGUI countdownTimerText; // Texte pour le compte à rebours

    [Header("Audio Feedback")]
    public AudioSource uiAudioSource;
    public AudioClip levelUpSound;
    public AudioClip questCompleteSound;

    private void Start()
    {
        if (damageTextPrefab != null)
        {
            damageTextPrefab.SetActive(false);
        }
        if (objectivesPanel != null)
        {
            objectivesPanel.SetActive(false);
        }
    }

    public void ShowDamageText(int damage, Vector3 position)
    {
        if (damageTextPrefab == null || mainCanvas == null)
        {
            Debug.LogError("DamageTextPrefab ou MainCanvas n'est pas assigné dans l'inspecteur.");
            return;
        }

        // Instancie le texte des dégâts
        GameObject damageTextInstance = Instantiate(damageTextPrefab, mainCanvas.transform);

        // Convertit la position du monde en position écran
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);
        damageTextInstance.transform.position = screenPosition;

        // Ajoute le texte des dégâts
        TextMeshProUGUI damageInstanceText = damageTextInstance.GetComponent<TextMeshProUGUI>();
        if (damageInstanceText != null)
        {
            damageInstanceText.text = $"-{damage}";
        }

        // Détruit l'objet après 1.5 secondes
        Destroy(damageTextInstance, 1.5f);
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
        if (objectiveTexts.Count == 0) Debug.LogWarning("Aucun texte d'objectif assigné !");
        if (chapterTitleText == null) Debug.LogWarning("chapterTitleText n'est pas assigné !");
        if (locationText == null) Debug.LogWarning("locationText n'est pas assigné !");
        if (countdownTimerText == null) Debug.LogWarning("countdownTimerText n'est pas assigné !");
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
        xpText.text = $"XP : {currentXP}/{maxXP}";
        levelText.text = $"Level : {currentLevel}";

        if (xpLog != null)
        {
            xpLog.SetActive(true);
            xpGainedText.text = $"+{xpGained} XP";
            xpTotalText.text = $"XP : {currentXP}/{maxXP}";

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

        public void UpdateObjectives(List<string> objectives)
    {
        if (objectiveTexts.Count != objectives.Count)
        {
            Debug.LogWarning("Le nombre d'objectifs assignés ne correspond pas au nombre d'éléments dans la liste.");
            return;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            objectiveTexts[i].text = objectives[i];
        }

        objectivesPanel.SetActive(true);
    }

    public void UpdateChapterTitle(string title)
    {
        if (chapterTitleText != null)
        {
            chapterTitleText.text = title;
        }
    }

    public void UpdateLocation(string location)
    {
        if (locationText != null)
        {
            locationText.text = location;
        }
    }

    public void UpdateCountdown(float timeRemaining)
    {
        if (countdownTimerText != null)
        {
            countdownTimerText.text = timeRemaining.ToString("F1") + "s";
        }
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

    public void UpdateMission(string missionName, string missionDescription)
    {
        if (missionText != null)
        {
            missionText.text = $"Mission: {missionName}\n{missionDescription}";
        }
    }

    public void ShowMissionComplete(string reward)
    {
        ShowAlert($"Mission accomplie ! Récompense : {reward}", 3f);
        if (uiAudioSource != null && questCompleteSound != null)
        {
            uiAudioSource.PlayOneShot(questCompleteSound);
        }
    }

    private void TriggerLevelUp(int newLevel)
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            levelUpPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"LEVEL UP! Level {newLevel}";

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
        Vector3 targetScale = originalScale * 1.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            panelTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            yield return null;
        }

        panelTransform.localScale = originalScale;
    }

    private void HideXPLog()
    {
        if (xpLog != null)
        {
            xpLog.SetActive(false);
        }
    }

    private void HideLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }
}
