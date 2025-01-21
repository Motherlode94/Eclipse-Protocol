using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mission
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

    [Header("Prerequisites")]
    public List<Mission> prerequisites; // Missions nécessaires avant de commencer celle-ci

    [Header("Objectives")]
    public List<string> objectives; // Liste des étapes/objectifs
    public int currentObjectiveIndex = 0;

    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;

    [Header("Mission Status")]
    public MissionState currentState = MissionState.NotStarted;

    public bool isCompleted => currentState == MissionState.Completed;

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

        OnMissionStarted?.Invoke(this);
    }

    /// <summary>
    /// Avance l'objectif de la mission.
    /// </summary>
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

    /// <summary>
    /// Termine la mission.
    /// </summary>
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

        OnMissionCompleted?.Invoke(this);
        GrantRewards();
    }

    /// <summary>
    /// Échoue la mission.
    /// </summary>
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
    }
}
