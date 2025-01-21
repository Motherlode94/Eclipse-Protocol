using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    [Header("Missions")]
    public List<Mission> missions = new List<Mission>();
    private int currentMissionIndex = 0;

    [Header("Events")]
    public UnityEvent<Mission> OnMissionStarted;
    public UnityEvent<Mission> OnMissionCompleted;
    public UnityEvent<Mission> OnMissionFailed;

    private void Start()
    {
        if (missions.Count > 0)
        {
            StartMission(currentMissionIndex);
        }
        else
        {
            Debug.LogWarning("Aucune mission disponible !");
        }
    }

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

    public void CompleteObjective()
    {
        if (currentMissionIndex >= 0 && currentMissionIndex < missions.Count)
        {
            Mission mission = missions[currentMissionIndex];

            if (mission.currentState == Mission.MissionState.InProgress)
            {
                mission.AdvanceObjective();

                if (mission.isCompleted)
                {
                    CompleteMission(currentMissionIndex);
                }
            }
        }
    }

    public void CompleteMission(int index)
    {
        if (index >= 0 && index < missions.Count)
        {
            Mission mission = missions[index];

            if (!mission.isCompleted)
            {
                mission.CompleteMission();
                OnMissionCompleted?.Invoke(mission);

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
        }
    }

    public void SaveMissions()
    {
        Debug.Log("Sauvegarde des missions non implémentée.");
    }

    public void LoadMissions()
    {
        Debug.Log("Chargement des missions non implémenté.");
    }
}
