using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public List<Mission> missions;
    private int currentMissionIndex = 0;

    public UnityEvent<Mission> OnMissionStarted;
    public UnityEvent<Mission> OnMissionCompleted;

    void Start()
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
            mission.StartMission();
            OnMissionStarted?.Invoke(mission);
        }
        else
        {
            Debug.LogWarning("Index de mission invalide !");
        }
    }

    public void CompleteMission(int index)
    {
        if (index >= 0 && index < missions.Count)
        {
            Mission mission = missions[index];
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
        else
        {
            Debug.LogWarning("Index de mission invalide !");
        }
    }
}
