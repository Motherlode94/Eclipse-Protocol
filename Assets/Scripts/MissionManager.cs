using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    [Header("Missions")]
    public List<Mission> missions = new List<Mission>();
    private List<Mission> activeMissions = new List<Mission>();
    private int currentMissionIndex = 0;

    [Header("Events")]
    public UnityEvent<Mission> OnMissionStarted;
    public UnityEvent<Mission> OnMissionCompleted;
    public UnityEvent<Mission> OnMissionFailed;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI missionNameText;
    public TMPro.TextMeshProUGUI missionDescriptionText;

    private void Start()
    {
        Mission dynamicMission = CreateDynamicMission();
missionManager.AddMission(dynamicMission);


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
        if (index < 0 || index >= missions.Count)
        {
            Debug.LogError("Index de mission invalide !");
            return;
        }

        Mission mission = missions[index];
        if (!activeMissions.Contains(mission) && mission.ArePrerequisitesMet())
        {
            activeMissions.Add(mission);
            mission.StartMission();
            OnMissionStarted?.Invoke(mission);
            UpdateMissionUI(mission);
        }
        else
        {
            Debug.LogWarning($"Impossible de démarrer la mission : {mission.missionName}");
        }
    }

    public void CompleteMission(int index)
    {
        if (index >= 0 && index < missions.Count)
        {
            Mission mission = missions[index];
            if (mission.IsCompleted)
            {
                activeMissions.Remove(mission);
                OnMissionCompleted?.Invoke(mission);
                currentMissionIndex++;
                if (currentMissionIndex < missions.Count)
                {
                    StartMission(currentMissionIndex);
                }
            }
        }
    }

    public void UpdateMissionUI(Mission mission)
    {
        if (missionNameText != null && missionDescriptionText != null)
        {
            missionNameText.text = mission.missionName;
            missionDescriptionText.text = mission.GetCurrentObjective();
        }
    }

    public void SaveMissions()
    {
        // Implémentez la logique de sauvegarde
    }

    public void LoadMissions()
    {
        // Implémentez la logique de chargement
    }
}
