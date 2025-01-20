using System;
using UnityEngine;

[System.Serializable]
public class Mission
{
    public enum MissionState
    {
        NotStarted,
        InProgress,
        Completed
    }

    [Header("Mission Details")]
    public string missionName;         // Nom de la mission
    public string missionDescription;  // Description de la mission
    public GameObject targetObject;    // Objet cible de la mission (optionnel)

    [Header("Rewards")]
    public int experienceReward;       // XP gagné en terminant la mission
    public int goldReward;             // Or gagné en terminant la mission

    [Header("Mission Status")]
    public MissionState currentState = MissionState.NotStarted;

    // Événements pour notifier les systèmes externes
    public event Action<Mission> OnMissionStarted;
    public event Action<Mission> OnMissionCompleted;
    public bool isCompleted => currentState == MissionState.Completed;

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

        currentState = MissionState.InProgress;
        Debug.Log($"Mission commencée : {missionName}");

        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }

        // Notifie les systèmes externes
        OnMissionStarted?.Invoke(this);
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

        // Notifie les systèmes externes
        OnMissionCompleted?.Invoke(this);

        // Gère les récompenses
        GrantRewards();
    }

    /// <summary>
    /// Accorde les récompenses au joueur.
    /// </summary>
    private void GrantRewards()
    {
        // Exemple de gestion des récompenses
        Debug.Log($"Récompenses : +{experienceReward} XP, +{goldReward} Gold");

        // Intégration potentielle avec un système de joueur
        PlayerStats playerStats = GameObject.FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.GainXP(experienceReward);
            playerStats.AddMoney(goldReward);
        }
    }
}
