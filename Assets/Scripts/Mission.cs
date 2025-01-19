using System.Collections;
using UnityEngine;

[System.Serializable]
public class Mission
{
    public string missionName;         // Nom de la mission
    public string missionDescription;  // Description de la mission
    public GameObject targetObject;    // Objet cible de la mission (optionnel)
    public bool isCompleted;           // Statut de la mission

    public void StartMission()
    {
        Debug.Log($"Mission commencée : {missionName}");
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    public void CompleteMission()
    {
        Debug.Log($"Mission terminée : {missionName}");
        isCompleted = true;
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
}

