using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class Police : MonoBehaviour
{
    [Header("Paramètres")]
    public int wantedThreshold = 50;
    public float arrestRange = 2f;
    public float detectionRadius = 10f;
    public float patrolWaitTime = 3f;

    [Header("Références")]
    public Transform[] patrolPoints;
    public Animator animator;
    public NavMeshAgent agent;
    public Transform player;
    public PlayerStats playerStats;

    // Paramètres pour le Blend Tree
    [HideInInspector] public float posX;
    [HideInInspector] public float posY;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
        else
        {
            Debug.LogError("Aucun objet avec le tag 'Player' trouvé !");
        }
    }

    public void UpdateAnimatorParameters()
    {
        animator.SetFloat("PosX", posX);
        animator.SetFloat("PosY", posY);
    }
}
