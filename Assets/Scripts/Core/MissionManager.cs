using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    [Header("Missions")]
    [Tooltip("Liste des missions disponibles pour cette session.")]
    public List<Mission> missions = new List<Mission>();

    private int currentMissionIndex = 0; // Indice de la mission actuelle

    [Header("Events")]
    public UnityEvent<Mission> OnMissionStarted; // Appelé lorsqu'une mission commence
    public UnityEvent<Mission> OnMissionCompleted; // Appelé lorsqu'une mission se termine

    private void Start()
    {
        // Démarre la première mission si disponible
        if (missions.Count > 0)
        {
            StartMission(currentMissionIndex);
        }
        else
        {
            Debug.LogWarning("Aucune mission disponible !");
        }
    }

    /// <summary>
    /// Démarre une mission spécifique.
    /// </summary>
    /// <param name="index">Index de la mission dans la liste.</param>
    public void StartMission(int index)
    {
        if (index >= 0 && index < missions.Count)
        {
            Mission mission = missions[index];

            if (mission.isCompleted)
            {
                Debug.LogWarning($"Mission déjà complétée : {mission.missionName}");
                return;
            }

            mission.StartMission();
            OnMissionStarted?.Invoke(mission);
        }
        else
        {
            Debug.LogWarning("Index de mission invalide !");
        }
    }

    /// <summary>
    /// Termine une mission spécifique.
    /// </summary>
    /// <param name="index">Index de la mission dans la liste.</param>
    public void CompleteMission(int index)
    {
        if (index >= 0 && index < missions.Count)
        {
            Mission mission = missions[index];

            if (mission.isCompleted)
            {
                Debug.LogWarning($"Mission déjà complétée : {mission.missionName}");
                return;
            }

            mission.CompleteMission();
            OnMissionCompleted?.Invoke(mission);

            // Passe à la mission suivante
            currentMissionIndex++;
            if (currentMissionIndex < missions.Count)
            {
                StartMission(currentMissionIndex);
            }
            else
            {
                Debug.Log("Toutes les missions sont terminées !");
            }
        }
        else
        {
            Debug.LogWarning("Index de mission invalide !");
        }
    }

    /// <summary>
    /// Réinitialise toutes les missions.
    /// </summary>
    public void ResetMissions()
    {
        foreach (var mission in missions)
        {
            mission.currentState = Mission.MissionState.NotStarted;
        }

        currentMissionIndex = 0;
        Debug.Log("Toutes les missions ont été réinitialisées.");
        StartMission(currentMissionIndex);
    }
}
