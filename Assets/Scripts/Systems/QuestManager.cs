using UnityEngine;
using System.Collections.Generic;

public class Quest
{
    public string questName;
    public string description;
    public bool isCompleted;
    public int rewardXP;
    public int rewardMoney;

    public Quest(string name, string desc, int xp, int money)
    {
        questName = name;
        description = desc;
        rewardXP = xp;
        rewardMoney = money;
        isCompleted = false;
    }
}

public class QuestManager : MonoBehaviour
{
    public List<Quest> activeQuests = new List<Quest>();

    public void AddQuest(Quest newQuest)
    {
        activeQuests.Add(newQuest);
        Debug.Log($"Nouvelle quête ajoutée : {newQuest.questName}");
    }

    public void CompleteQuest(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            quest.isCompleted = true;
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            playerStats.GainXP(quest.rewardXP);
            playerStats.AddMoney(quest.rewardMoney);
            activeQuests.Remove(quest);
            Debug.Log($"Quête complétée : {quest.questName}");
        }
    }
}
