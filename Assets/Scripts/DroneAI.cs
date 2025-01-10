using UnityEngine;
using UnityEngine.AI;

public class DroneAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 10f;
    public float attackRadius = 5f;
    public LayerMask playerLayer;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6f;

    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex = 0;
    private bool isChasing = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        MoveToNextPatrolPoint();
    }

    private void Update()
    {
        DetectPlayer();

        if (isChasing && player != null)
        {
            ChasePlayer();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToNextPatrolPoint();
        }
    }

    private void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            player = hits[0].transform;
            isChasing = Vector3.Distance(transform.position, player.position) <= detectionRadius;
        }
        else
        {
            player = null;
            isChasing = false;
        }
    }

    private void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= attackRadius)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        Debug.Log("Drone is attacking the player!");
        // Implémentez ici les effets d'attaque (par exemple : réduire la santé du joueur)
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
