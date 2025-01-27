using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainQuestGiver : MonoBehaviour
{
    [Header("Main Quests")]
    public List<Mission> mainQuests = new List<Mission>();
    private int currentQuestIndex = 0;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI questTitleText;
    public TMPro.TextMeshProUGUI questDescriptionText;

    public void GiveNextQuest()
    {
        if (currentQuestIndex >= mainQuests.Count)
        {
            Debug.Log("Toutes les quêtes principales ont été attribuées !");
            return;
        }

        Mission nextQuest = mainQuests[currentQuestIndex];

        if (nextQuest.currentState == Mission.MissionState.NotStarted && nextQuest.ArePrerequisitesMet())
        {
            nextQuest.StartMission();
            UpdateQuestUI(nextQuest);
            Debug.Log($"Nouvelle quête principale donnée : {nextQuest.missionName}");
            currentQuestIndex++;
        }
        else
        {
            Debug.LogWarning("Cette quête principale ne peut pas encore être démarrée (prérequis non remplis).");
        }
    }

    private void UpdateQuestUI(Mission quest)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GiveNextQuest();
        }
    }
}
