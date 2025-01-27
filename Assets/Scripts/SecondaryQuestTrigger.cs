using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryQuestTrigger : MonoBehaviour
{
    [Header("Secondary Quest")]
    public Mission secondaryQuest;

    [Header("Trigger Settings")]
    public bool autoTrigger = false; // Si la quête doit être déclenchée automatiquement
    public bool oneTimeTrigger = true; // Si le déclenchement ne peut se produire qu'une fois
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (secondaryQuest != null && secondaryQuest.currentState == Mission.MissionState.NotStarted)
            {
                TriggerQuest();
                if (oneTimeTrigger)
                {
                    hasTriggered = true;
                }
            }
            else
            {
                Debug.LogWarning("Impossible de démarrer la quête secondaire (soit elle a déjà été démarrée, soit elle est terminée).");
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
}
