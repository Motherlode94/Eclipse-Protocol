using System.Collections;
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
    public UnityEvent<List<string>> OnQuestLogUpdated; // Événement pour mettre à jour le journal


    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI missionNameText;
    public TMPro.TextMeshProUGUI missionDescriptionText;

    [Header("Mission Element Settings")]
    public GameObject[] missionPrefabs; // Liste des objets à instancier pour les missions
    public Transform[] spawnPoints; // Points de spawn prédéfinis pour les objets
    [Header("Settings")]
    public float delayBetweenMissions = 5f; // Délai avant la prochaine mission
    public bool loopMissions = true; // Option pour boucler sur les missions

    private void Start()
    {
        StartMission(currentMissionIndex);

        Mission dynamicMission = CreateDynamicMission();
        AddMission(dynamicMission);

        if (missions.Count > 0)
        {
            StartMission(currentMissionIndex);
        }
        else
        {
            Debug.LogWarning("Aucune mission disponible !");
        }
    }

    public Mission CreateDynamicMission()
    {
        Mission newMission = ScriptableObject.CreateInstance<Mission>();

        // Génération des éléments de mission
        newMission.missionObjects = GenerateMissionElements();

        // Vérifie si des objets spécifiques sont générés
        bool hasKey = false;
        bool hasEnemies = false;

        foreach (GameObject obj in newMission.missionObjects)
        {
            if (obj.name.ToLower().Contains("key"))
            {
                hasKey = true;
            }
            else if (obj.name.ToLower().Contains("enemy"))
            {
                hasEnemies = true;
            }
        }

        // Définir le scénario de la mission
        if (hasKey)
        {
            newMission.missionName = "Trouver la clé sacrée";
            newMission.missionDescription = "Une clé essentielle pour ouvrir une porte légendaire est cachée quelque part. Trouvez-la !";
        }
        else if (hasEnemies)
        {
            newMission.missionName = "Défendez la forteresse";
            newMission.missionDescription = "Des ennemis attaquent la zone. Éliminez-les pour protéger votre base.";
        }
        else
        {
            string[] explorationTitles = { "Explorer une nouvelle zone", "Découverte de ruines anciennes", "Reconnaissance d'un territoire dangereux" };
            string[] explorationDescriptions = { "Explorez la zone et découvrez ses secrets.", "Trouvez les ruines cachées dans ce territoire inconnu.", "Naviguez prudemment pour éviter les pièges." };

            newMission.missionName = explorationTitles[Random.Range(0, explorationTitles.Length)];
            newMission.missionDescription = explorationDescriptions[Random.Range(0, explorationDescriptions.Length)];
        }

        // Définir les récompenses et objectifs par défaut (si non spécifiés dans le scénario)
        if (newMission.objectives == null || newMission.objectives.Count == 0)
        {
        newMission.objectives = new List<string> { "Compléter les objectifs listés dans cette mission." };
        }

        newMission.experienceReward = 100;
        newMission.goldReward = 50;
        newMission.itemRewards = new List<InventoryItem>();
        newMission.objectives = new List<string> { "Compléter les objectifs listés dans cette mission." };
        newMission.objectives = new List<string> { "Éliminer 5 drones ennemis." };
        newMission.requiredKills = 5;


        Debug.Log($"Mission créée : {newMission.missionName} - {newMission.missionDescription}");
        return newMission;
    }

    public void AddMission(Mission mission)
    {
        if (mission != null)
        {
            missions.Add(mission);
            Debug.Log("Mission ajoutée : " + mission.missionName);
        }
    }
    public List<string> GetActiveQuests()
    {
        List<string> questNames = new List<string>();
        foreach (var mission in activeMissions)
        {
            questNames.Add(mission.missionName);
        }
        return questNames;
    }
    public Mission GetActiveMission()
    {
        // Retourne la première mission en cours (ou null si aucune mission active)
        foreach (var mission in activeMissions)
        {
            if (mission.currentState == Mission.MissionState.InProgress)
            {
                return mission;
            }
        }

        Debug.LogWarning("Aucune mission active trouvée !");
        return null;
    }

    public void AssignMission(Mission mission)
    {
    if (mission != null && !missions.Contains(mission))
    {
        AddMission(mission);

        if (mission.ArePrerequisitesMet())
        {
            StartMission(missions.IndexOf(mission));
        }
        else
        {
            Debug.LogWarning($"Les prérequis pour la mission {mission.missionName} ne sont pas remplis.");
        }
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

                StartCoroutine(HandleMissionCompletion());
            }
        }
    }

private IEnumerator HandleMissionCompletion()
{
        // Affiche un message temporaire
        if (missionNameText != null)
        {
            missionNameText.text = "Mission terminée !";
        }

        if (missionDescriptionText != null)
        {
            missionDescriptionText.text = "Préparation de la prochaine mission...";
        }

        yield return new WaitForSeconds(delayBetweenMissions);

        // Passe à la mission suivante ou boucle sur les missions
        currentMissionIndex++;
        if (currentMissionIndex < missions.Count)
        {
            StartMission(currentMissionIndex);
        }
        else if (loopMissions)
        {
            currentMissionIndex = 0; // Réinitialise à la première mission
            StartMission(currentMissionIndex);
        }
        else
        {
            missionNameText.text = "Toutes les missions sont terminées.";
            missionDescriptionText.text = "Revenez bientôt pour de nouvelles quêtes.";
        }
    }

    public void NotifyKill()
{
    foreach (Mission mission in activeMissions)
    {
        mission.RegisterKill();
    }
}



    public void UpdateMissionUI(Mission mission)
    {
        if (missionNameText != null && missionDescriptionText != null)
        {
            if (mission.IsCompleted)
            {
                missionNameText.text = "Mission terminée !";
                missionDescriptionText.text = "";
            }
            else
            {
                missionNameText.text = mission.missionName;
                missionDescriptionText.text = mission.GetObjectiveProgress();
            }
        }
        else
        {
            Debug.LogError("UI non assignée dans l'Inspector !");
        }
    }

    private List<GameObject> GenerateMissionElements()
    {
        List<GameObject> missionElements = new List<GameObject>();

        for (int i = 0; i < missionPrefabs.Length; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject missionObject = Instantiate(missionPrefabs[i], spawnPoint.position, spawnPoint.rotation);
            missionElements.Add(missionObject);

            Debug.Log($"Objet généré : {missionObject.name} à la position {spawnPoint.position}");
        }

        return missionElements;
    }
    private void NotifyQuestLogUpdated()
{
    List<string> activeMissionNames = new List<string>();
    foreach (var mission in activeMissions)
    {
        activeMissionNames.Add(mission.missionName);
    }

    OnQuestLogUpdated?.Invoke(activeMissionNames);
}

    public void NotifyObjectiveCompleted(GameObject collectedObject)
    {
        foreach (Mission mission in activeMissions)
    {
        if (mission.missionObjects.Contains(collectedObject))
        {
            Debug.Log($"Objectif complété pour la mission : {mission.missionName}, objet : {collectedObject.name}");
            mission.CompleteObjective(collectedObject);
            UpdateMissionUI(mission);
            return;
        }
    }

    Debug.LogWarning($"L'objet {collectedObject.name} ne correspond à aucun objectif actif.");
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
