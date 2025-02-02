using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events; // ✅ Ajout du namespace UnityEvent

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

    [Header("Reputation and Crime UI")]
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI crimeText;

    [Header("References")]
    private PlayerStats playerStats; // Référence au script PlayerStats
    public MissionManager missionManager; // Référence au MissionManager

    [Header("Mission System")]
    public GameObject objectivesPanel; // Panneau des objectifs
    public TextMeshProUGUI missionText; // Texte pour afficher la mission active
    public List<TextMeshProUGUI> objectiveTexts; // Liste des textes pour les objectifs
    public TextMeshProUGUI chapterTitleText; // Texte pour le titre du chapitre
    public TextMeshProUGUI locationText; // Texte pour la localisation actuelle
    public TextMeshProUGUI countdownTimerText; // Texte pour le compte à rebours
    
    [Header("Quest Log")]
    public GameObject questLogPanel; // Panneau pour afficher les quêtes actives
    public List<TextMeshProUGUI> questLogTexts; // Liste des textes des quêtes actives

    [Header("Audio Feedback")]
    public AudioSource uiAudioSource;
    public AudioClip levelUpSound;
    public AudioClip questCompleteSound;

    [Header("Events")]
    public UnityEvent onLevelUp;
    public UnityEvent onMissionCompleted;
    public UnityEvent onHealthCritical;

    private void Awake()
    {
    ValidateReferences();

    if (interactionPrompt != null)
        interactionPrompt.SetActive(false);

    if (alertText != null)
        alertText.gameObject.SetActive(false);

    if (levelUpPanel != null)
        levelUpPanel.SetActive(false);

    if (xpLog != null)
        xpLog.SetActive(false);

    if (questLogPanel != null)
        questLogPanel.SetActive(false);
    }


    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            playerStats = playerObject.GetComponent<PlayerStats>();

        else
            Debug.LogError("Aucun objet avec le tag 'Player' trouvé !");

        if (missionManager == null)
        {
            Debug.LogError("MissionManager n'est pas assigné dans le UIManager !");
        }
    }

    private void ValidateReferences()
    {
        if (healthText == null) Debug.LogError("healthText non assigné !");
        if (staminaText == null) Debug.LogError("staminaText non assigné !");
        if (xpText == null) Debug.LogError("xpText non assigné !");
        if (levelText == null) Debug.LogError("levelText non assigné !");
        if (moneyText == null) Debug.LogError("moneyText non assigné !");
        if (questLogPanel == null) Debug.LogWarning("Journal des quêtes non assigné !");
    }

    private void Update()
    {
        if (playerStats != null)
        {
            reputationText.text = $"Réputation : {playerStats.Reputation}";
            crimeText.text = $"Niveau de criminalité : {playerStats.WantedLevel}";
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

    public void UpdateHealth(int currentHealth)
    {
        healthText.text = $"HP : {currentHealth}";
        ApplyCriticalColor(healthText, currentHealth, 20);
        if (currentHealth < 20)
            onHealthCritical?.Invoke();
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

            StartCoroutine(HideAfterDelay(xpLog, 2f));
        }

        if (currentXP >= maxXP)
            TriggerLevelUp(currentLevel + 1);
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
            Debug.LogWarning("Nombre d'objectifs incorrect !");
            return;
        }

        for (int i = 0; i < objectives.Count; i++)
            objectiveTexts[i].text = objectives[i];

        objectivesPanel.SetActive(true);
    }

    public void UpdateMission(string missionName, string missionDescription)
    {
        if (missionText != null)
            missionText.text = $"Mission: {missionName}\n{missionDescription}";
    }

    public void ShowMissionComplete(string reward)
    {
        ShowAlert($"Mission accomplie ! Récompense : {reward}", 3f);
        onMissionCompleted?.Invoke();
        if (uiAudioSource != null && questCompleteSound != null)
            uiAudioSource.PlayOneShot(questCompleteSound);
    }

    public void UpdateQuestLog()
    {
        if (missionManager == null || questLogTexts == null)
        {
            Debug.LogError("MissionManager ou questLogTexts n'est pas assigné !");
            return;
        }

        List<string> activeQuests = missionManager.GetActiveQuests(); // Méthode dans MissionManager pour obtenir les quêtes actives

        if (questLogTexts.Count < activeQuests.Count)
        {
            Debug.LogWarning("Pas assez d'emplacements pour afficher toutes les quêtes actives.");
            return;
        }

        // Mettre à jour les textes pour les quêtes actives
        for (int i = 0; i < activeQuests.Count; i++)
        {
            questLogTexts[i].text = activeQuests[i];
            questLogTexts[i].gameObject.SetActive(true);
        }

        // Désactiver les textes inutilisés
        for (int i = activeQuests.Count; i < questLogTexts.Count; i++)
        {
            questLogTexts[i].gameObject.SetActive(false);
        }
    }

private IEnumerator AnimateQuestLog(bool open)
{
    Vector3 startScale = questLogPanel.transform.localScale;
    Vector3 endScale = open ? Vector3.one : Vector3.zero;
    float duration = 0.3f;
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        questLogPanel.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
        yield return null;
    }

    questLogPanel.transform.localScale = endScale;
}



    public void ToggleInteractionPrompt(bool isActive, string message = "Press E")
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isActive);
            TextMeshProUGUI promptText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null)
                promptText.text = message;
        }
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
    public void ShowAlert(string message, float duration = 2f)
    {
        if (alertText != null)
        {
            alertText.text = message;
            alertText.gameObject.SetActive(true);
            StartCoroutine(HideAfterDelay(alertText.gameObject, duration));
        }
    }

    private IEnumerator HideAfterDelay(GameObject uiElement, float duration)
    {
        yield return new WaitForSeconds(duration);
        uiElement.SetActive(false);
    }

    private void TriggerLevelUp(int newLevel)
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            levelUpPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"LEVEL UP! Level {newLevel}";
            StartCoroutine(HideAfterDelay(levelUpPanel, 3f));
            onLevelUp?.Invoke();
            if (uiAudioSource != null && levelUpSound != null)
                uiAudioSource.PlayOneShot(levelUpSound);
        }
    }

    private IEnumerator AnimateLevelUpPanel(Transform panelTransform, float duration)
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
