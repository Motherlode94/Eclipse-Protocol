using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public MissionManager missionManager;
    public QuestManager questManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialisation des managers si nécessaire
        if (missionManager == null) missionManager = GetComponentInChildren<MissionManager>();
        if (questManager == null) questManager = GetComponentInChildren<QuestManager>();
    }
}
