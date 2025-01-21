using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Police : MonoBehaviour
{
    [Header("Paramètres")]
    public int wantedThreshold = 50; // Seuil de criminalité pour attaquer
    public float arrestRange = 2f; // Distance pour arrêter le joueur
    public float detectionRadius = 10f; // Rayon de détection pour patrouiller
    public float patrolWaitTime = 3f; // Temps d'attente entre les points de patrouille

    [Header("Références")]
    public Transform[] patrolPoints; // Points de patrouille prédéfinis
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private PlayerStats playerStats; // Référence au script PlayerStats

    private int currentPatrolIndex;
    private bool isWaiting;

    void Start()
    {
        // Trouver le joueur et récupérer les composants nécessaires
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

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (patrolPoints.Length > 0)
        {
            StartPatrol();
        }
        else
        {
            Debug.LogWarning("Aucun point de patrouille assigné !");
        }
    }

    void Update()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats non assigné. Impossible de déterminer le niveau de criminalité.");
            return;
        }

        // Si le joueur dépasse le seuil de criminalité, le policier le poursuit
        if (playerStats.WantedLevel >= wantedThreshold)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void StartPatrol()
    {
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            animator.SetBool("isWalking", true);
        }
    }

    void Patrol()
    {
        if (isWaiting || patrolPoints.Length == 0) return;

        // Si le policier atteint le point de patrouille
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(patrolWaitTime);

        // Aller au prochain point de patrouille
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        animator.SetBool("isWalking", true);
        isWaiting = false;
    }

    void ChasePlayer()
    {
        if (player == null) return;

        agent.SetDestination(player.position);
        animator.SetBool("isRunning", true);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= arrestRange)
        {
            ArrestPlayer();
        }
    }

    void ArrestPlayer()
    {
        // Logique d'arrestation
        Debug.Log("Le joueur est arrêté !");
        animator.SetTrigger("Arrest");

        // Réinitialiser le niveau de criminalité
        if (playerStats != null)
        {
            playerStats.WantedLevel = 0;
        }

        // Arrêter le policier après l'arrestation
        agent.ResetPath();
        animator.SetBool("isRunning", false);
    }

    void OnDrawGizmosSelected()
    {
        // Visualisation des rayons de détection et d'arrestation
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, arrestRange);
    }
}
