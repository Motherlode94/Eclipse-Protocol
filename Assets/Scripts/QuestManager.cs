using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class Quest
{
    public enum QuestType { Main, Side, Repeatable }
    public QuestType questType;

    public string questName;
    public string description;
    public List<string> objectives;
    public bool isCompleted;
    public int rewardXP;
    public int rewardMoney;

    private int currentObjectiveIndex = 0;

    public Quest(string name, string desc, int xp, int money, QuestType type)
    {
        questName = name;
        description = desc;
        rewardXP = xp;
        rewardMoney = money;
        questType = type;
        isCompleted = false;
        objectives = new List<string>();
    }

    public string GetCurrentObjective()
    {
        if (currentObjectiveIndex < objectives.Count)
        {
            return objectives[currentObjectiveIndex];
        }
        return "Quête terminée.";
    }

    public void AdvanceObjective()
    {
        if (currentObjectiveIndex < objectives.Count)
        {
            currentObjectiveIndex++;
        }

        if (currentObjectiveIndex >= objectives.Count)
        {
            isCompleted = true;
        }
    }
}

public class QuestManager : MonoBehaviour
{
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();

    [Header("UI & Notifications")]
    public GameObject questNotificationUI;
    public GameObject questListUI; // Référence à une UI pour afficher la liste des quêtes
    public AudioClip questCompleteSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void AddQuest(Quest newQuest)
    {
        if (activeQuests.Exists(q => q.questName == newQuest.questName))
        {
            Debug.LogWarning($"La quête '{newQuest.questName}' est déjà active !");
            return;
        }

        activeQuests.Add(newQuest);
        DisplayQuestNotification($"Nouvelle quête ajoutée : {newQuest.questName}");
        Debug.Log($"Nouvelle quête ajoutée : {newQuest.questName}");
        UpdateQuestUI();
    }

    public void CompleteQuest(Quest quest)
    {
        if (activeQuests.Contains(quest) && quest.isCompleted)
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            playerStats.GainXP(quest.rewardXP);
            playerStats.AddMoney(quest.rewardMoney);

            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            DisplayQuestNotification($"Quête complétée : {quest.questName}");
            PlayQuestCompleteSound();
            Debug.Log($"Quête complétée : {quest.questName}");
            UpdateQuestUI();
        }
    }

    public void AdvanceQuest(Quest quest)
    {
        if (activeQuests.Contains(quest) && !quest.isCompleted)
        {
            quest.AdvanceObjective();
            if (quest.isCompleted)
            {
                CompleteQuest(quest);
            }
            else
            {
                DisplayQuestNotification($"Nouvel objectif : {quest.GetCurrentObjective()}");
                Debug.Log($"Nouvel objectif : {quest.GetCurrentObjective()}");
            }
        }
    }

    private void DisplayQuestNotification(string message)
    {
        if (questNotificationUI != null)
        {
            // Exemple : Affiche le message dans une Text UI
            Debug.Log($"Notification UI : {message}");
        }
    }

    private void PlayQuestCompleteSound()
    {
        if (questCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(questCompleteSound);
        }
    }

    private void UpdateQuestUI()
    {
        if (questListUI != null)
        {
            // Implémentez ici la logique pour mettre à jour l'interface des quêtes actives
            Debug.Log("Mise à jour de l'UI des quêtes.");
        }
    }

    public void SaveQuests()
    {
        string path = Application.persistentDataPath + "/quests.json";
        var questData = new QuestData { ActiveQuests = activeQuests, CompletedQuests = completedQuests };
        File.WriteAllText(path, JsonUtility.ToJson(questData, true));
        Debug.Log("Progression des quêtes sauvegardée !");
    }

    public void LoadQuests()
    {
        string path = Application.persistentDataPath + "/quests.json";
        if (File.Exists(path))
        {
            var questData = JsonUtility.FromJson<QuestData>(File.ReadAllText(path));
            activeQuests = questData.ActiveQuests;
            completedQuests = questData.CompletedQuests;
            UpdateQuestUI();
            Debug.Log("Progression des quêtes chargée !");
        }
        else
        {
            Debug.LogWarning("Aucune sauvegarde trouvée !");
        }
    }
}

[System.Serializable]
public class QuestData
{
    public List<Quest> ActiveQuests;
    public List<Quest> CompletedQuests;
}
