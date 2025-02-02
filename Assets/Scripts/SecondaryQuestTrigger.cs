using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryQuestTrigger : MonoBehaviour
{
    [Header("Secondary Quest")]
    public Mission secondaryQuest;
    public MissionManager missionManager; // Référence au MissionManager

    [Header("Trigger Settings")]
    public bool autoTrigger = false; // Si la quête doit être déclenchée automatiquement
    public bool oneTimeTrigger = true; // Si le déclenchement ne peut se produire qu'une fois
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (secondaryQuest != null && secondaryQuest.ArePrerequisitesMet())
            {
                missionManager.AddMission(secondaryQuest); // Ajout de la mission secondaire au MissionManager
                Debug.Log($"Quête secondaire déclenchée : {secondaryQuest.missionName}");
                hasTriggered = true;
            }
            else
            {
                Debug.LogWarning("Les prérequis pour cette quête secondaire ne sont pas remplis.");
            }
        }
    }

    public void TriggerQuest()
    {
        if (secondaryQuest.ArePrerequisitesMet())
        {
            secondaryQuest.StartMission();
            Debug.Log($"Quête secondaire déclenchée : {secondaryQuest.missionName}");
        }
        else
        {
            Debug.LogWarning("Les prérequis pour cette quête secondaire ne sont pas remplis.");
        }
    }
    public void NotifyMainQuestGiver(MainQuestGiver mainQuestGiver)
{
    if (mainQuestGiver != null)
    {
        mainQuestGiver.GiveNextQuest(); // Active la prochaine quête principale
    }
}

}
