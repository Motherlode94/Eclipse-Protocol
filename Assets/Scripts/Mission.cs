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
    public List<Mission> prerequisites;

    [Header("Objectives")]
    public List<string> objectives;
    public int currentObjectiveIndex = 0;

    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;
    public List<InventoryItem> itemRewards;

    [Header("Mission Status")]
    public MissionState currentState = MissionState.NotStarted;

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

    public void Update(float deltaTime)
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
    public Mission CreateDynamicMission()
{
    Mission newMission = ScriptableObject.CreateInstance<Mission>();

    newMission.missionName = "Dynamic Mission";
    newMission.missionDescription = "This is a dynamically generated mission.";
    newMission.objectives = new List<string> { "Find the hidden key", "Open the treasure chest" };
    newMission.experienceReward = 100;
    newMission.goldReward = 50;

    return newMission;
}


    public string GetCurrentObjective()
    {
        return currentObjectiveIndex < objectives.Count ? objectives[currentObjectiveIndex] : "Aucun objectif actuel.";
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
        Debug.Log($"Récompenses : +{experienceReward} XP, +{goldReward} Gold");

        PlayerStats playerStats = GameObject.FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.GainXP(experienceReward);
            playerStats.AddMoney(goldReward);
        }

        foreach (var item in itemRewards)
        {
            InventoryManager.Instance.AddItem(item);
        }
    }
}
