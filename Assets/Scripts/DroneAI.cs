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
    public float alertDuration = 3f; // Temps en alerte avant de revenir à la patrouille

    [Header("Look Settings")]
    public Transform look; // Transform pour définir la direction du regard

    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex = 0;
    private bool isAlerted = false;
    private float alertTimer = 0f;
    public bool isChasing { get; private set; } // Utilisation unique de `isChasing`

    // Cooldown pour la détection
    private float detectCooldown = 0.2f;
    private float detectTimer = 0f;
    public Transform Player => player;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

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
        else if (isAlerted)
        {
            HandleAlertState();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToNextPatrolPoint();
        }

        UpdateLookDirection();
    }

    private void FixedUpdate()
    {
        if (detectTimer > 0)
        {
            detectTimer -= Time.fixedDeltaTime;
        }
        else
        {
            DetectPlayer();
            detectTimer = detectCooldown;
        }
    }

    private void DetectPlayer()
    {
                Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length > 0)
        {
            Transform closestPlayer = null;
            float closestDistance = Mathf.Infinity;

            foreach (var hit in hits)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestPlayer = hit.transform;
                    closestDistance = distance;
                }
            }

            if (closestPlayer != null && HasLineOfSight(closestPlayer))
            {
                player = closestPlayer;
                isChasing = true;
            }
        }
        else
        {
            player = null;
            isChasing = false;
            isAlerted = false;
        }
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, detectionRadius))
        {
            return hit.transform == target;
        }
        return false;
    }

    public void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= attackRadius)
        {
                        if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                AttackPlayer();
            }
        }
    }

    public void AttackPlayer()
    {
        Debug.Log("Drone is attacking the player!");
        // Implémentez ici les effets d'attaque (par exemple : réduire la santé du joueur)
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.speed = patrolSpeed; // Réinitialise la vitesse
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private bool HasLineOfSight(Transform target)
    {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, detectionRadius))
        {
            return hit.transform == target;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // Détection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Attaque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Direction de regard
        if (look != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, look.position);
            Gizmos.DrawSphere(look.position, 0.2f);
        }
    }
}
