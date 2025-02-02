using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Quests/Mission")]
public class Mission : ScriptableObject
{
    public enum MissionState
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    [Header("Mission Details")]
    public string missionName;
    public string missionDescription;
    public GameObject targetObject;

    [Header("Timing")]
    public float missionDuration; // En secondes
    private float timeRemaining;

    [Header("Prerequisites")]
    public List<Mission> prerequisites = new List<Mission>();

    [Header("Objectives")]
    public List<string> objectives;
    public int currentObjectiveIndex = 0;
    public int requiredKills;
    private int currentKills = 0;

    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;
    public List<InventoryItem> itemRewards = new List<InventoryItem>(); // Initialisation automatique


    [Header("Mission Status")]
    public MissionState currentState = MissionState.NotStarted;

    [Header("Mission Elements")]
    public List<GameObject> missionObjects = new List<GameObject>();

    public bool IsCompleted => currentState == MissionState.Completed;

    public event Action<Mission> OnMissionStarted;
    public event Action<Mission> OnMissionCompleted;
    public event Action<Mission> OnMissionFailed;

    /// <summary>
    /// Vérifie si les prérequis sont remplis.
    /// </summary>
    public bool ArePrerequisitesMet()
    {
        foreach (var prerequisite in prerequisites)
        {
            if (prerequisite.currentState != MissionState.Completed)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Démarre la mission.
    /// </summary>
    public void StartMission()
    {
        if (currentState != MissionState.NotStarted)
        {
            Debug.LogWarning($"Mission déjà commencée ou terminée : {missionName}");
            return;
        }

        if (!ArePrerequisitesMet())
        {
            Debug.LogWarning($"Pré-requis non remplis pour la mission : {missionName}");
            return;
        }

        currentState = MissionState.InProgress;
        Debug.Log($"Mission commencée : {missionName}");

        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }

        timeRemaining = missionDuration > 0 ? missionDuration : 0;
        OnMissionStarted?.Invoke(this);
    }

    public void UpdateMission(float deltaTime)
    {
        if (currentState == MissionState.InProgress && missionDuration > 0)
        {
            timeRemaining -= deltaTime;
            if (timeRemaining <= 0)
            {
                FailMission();
            }
        }
    }

    public void AdvanceObjective()
    {
        if (currentState != MissionState.InProgress || currentObjectiveIndex >= objectives.Count)
        {
            Debug.LogWarning("Impossible d'avancer dans la mission.");
            return;
        }

        currentObjectiveIndex++;
        if (currentObjectiveIndex >= objectives.Count)
        {
            CompleteMission();
        }
        else
        {
            Debug.Log($"Nouvel objectif : {GetCurrentObjective()}");
        }
    }

    public string GetCurrentObjective()
    {
        return currentObjectiveIndex < objectives.Count ? objectives[currentObjectiveIndex] : "Aucun objectif actuel.";
    }
public string GetObjectiveProgress()
    {
        if (requiredKills > 0)
        return $"Éliminations : {currentKills}/{requiredKills}";
        return $"Objets restants : {missionObjects.Count}";
    }

    public void RegisterKill()
{
    currentKills++;
    if (currentKills >= requiredKills)
    {
        CompleteMission();
    }
}


    public void CompleteMission()
    {
        if (currentState != MissionState.InProgress)
        {
            Debug.LogWarning($"Mission non en cours : {missionName}");
            return;
        }

        currentState = MissionState.Completed;
        Debug.Log($"Mission terminée : {missionName}");

        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }

        GrantRewards();
        OnMissionCompleted?.Invoke(this);
    }
    public void CompleteObjective(GameObject collectedObject)
{
    if (missionObjects.Contains(collectedObject))
    {
        missionObjects.Remove(collectedObject); // Retire l'objet collecté de la liste
        Debug.Log($"Objectif complété : {collectedObject.name}");

        // Vérifie si tous les objets ont été collectés
        if (missionObjects.Count == 0)
        {
            CompleteMission(); // Termine la mission si tous les objectifs sont remplis
        }
    }
    else
    {
        Debug.LogWarning($"L'objet {collectedObject.name} ne fait pas partie des objectifs de la mission.");
    }
}


    public void FailMission()
    {
        if (currentState == MissionState.InProgress)
        {
            currentState = MissionState.Failed;
            Debug.Log($"Mission échouée : {missionName}");
            OnMissionFailed?.Invoke(this);
        }
    }
    private void GrantRewards()
    {
            Debug.Log($"Début de GrantRewards : +{experienceReward} XP, +{goldReward} Gold");

    PlayerStats playerStats = GameObject.FindObjectOfType<PlayerStats>();
    if (playerStats != null)
    {
        playerStats.GainXP(experienceReward);
        playerStats.AddMoney(goldReward);
    }
    else
    {
        Debug.LogError("PlayerStats non trouvé !");
    }

    if (InventoryManager.Instance == null)
    {
        Debug.LogError("InventoryManager.Instance est NULL !");
        return;
    }

    // ✅ Correction ici : cette ligne doit être dans une méthode
    if (itemRewards == null)
    {
        Debug.LogWarning("itemRewards était null, initialisation d'une liste vide.");
        itemRewards = new List<InventoryItem>(); // Empêche le crash
    }

    if (itemRewards.Count == 0)
    {
        Debug.LogWarning("Aucune récompense à ajouter.");
        return;
    }

    foreach (var item in itemRewards)
    {
        if (item != null)
        {
            bool success = InventoryManager.Instance.AddItem(item);
            if (success)
            {
                Debug.Log($"Ajout de {item.itemName} à l'inventaire.");
            }
            else
            {
                Debug.LogWarning($"Impossible d'ajouter {item.itemName}.");
            }
        }
        else
        {
            Debug.LogError("Un élément de itemRewards est NULL !");
        }
    }
    }
}
