using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainQuestGiver : MonoBehaviour
{
    [Header("Main Quests")]
    public List<Mission> mainQuests = new List<Mission>();
    public MissionManager missionManager; // Référence au MissionManager
    private int currentQuestIndex = 0;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI questTitleText;
    public TMPro.TextMeshProUGUI questDescriptionText;

    [Header("Settings")]
    public float secondaryQuestDelay = 2f; // Délai avant d'activer une quête secondaire

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GiveNextQuest();
        }
    }

    public void GiveNextQuest()
    {
        // Vérifie si toutes les quêtes ont été attribuées
        if (currentQuestIndex >= mainQuests.Count)
        {
            Debug.Log("Toutes les quêtes principales ont été attribuées !");
            return;
        }

        Mission nextQuest = mainQuests[currentQuestIndex];

        // Vérifie que la quête est valide et que les prérequis sont remplis
        if (nextQuest != null && nextQuest.ArePrerequisitesMet())
        {
            if (missionManager != null)
            {
                // Attribue la quête via MissionManager
                missionManager.AssignMission(nextQuest);

                // Met à jour l'interface utilisateur
                UpdateQuestUI(nextQuest);

                Debug.Log($"Nouvelle quête principale donnée : {nextQuest.missionName}");
                currentQuestIndex++;
            }
            else
            {
                Debug.LogError("MissionManager non trouvé dans la scène !");
            }
        }
        else
        {
            Debug.LogWarning($"Prérequis non remplis ou quête invalide pour : {nextQuest?.missionName}");
        }
    }

    public void ActivateSecondaryQuest(SecondaryQuestTrigger trigger)
    {
        // Active une quête secondaire avec un délai pour plus d'immersion
        if (trigger != null && trigger.secondaryQuest != null)
        {
            if (trigger.secondaryQuest.ArePrerequisitesMet())
            {
                StartCoroutine(ActivateQuestWithDelay(trigger));
            }
            else
            {
                Debug.LogWarning("Prérequis non remplis pour la quête secondaire.");
            }
        }
        else
        {
            Debug.LogWarning("Le déclencheur ou la quête secondaire est invalide.");
        }
    }

    private IEnumerator ActivateQuestWithDelay(SecondaryQuestTrigger trigger)
    {
        yield return new WaitForSeconds(secondaryQuestDelay);

        trigger.TriggerQuest();
        Debug.Log($"Quête secondaire activée : {trigger.secondaryQuest.missionName}");
    }

    private void UpdateQuestUI(Mission quest)
    {
        if (quest != null)
        {
            if (questTitleText != null)
            {
                questTitleText.text = quest.missionName;
            }

            if (questDescriptionText != null)
            {
                questDescriptionText.text = quest.missionDescription;
            }
        }
        else
        {
            Debug.LogWarning("Impossible de mettre à jour l'UI : la quête est nulle.");
        }
    }
}
