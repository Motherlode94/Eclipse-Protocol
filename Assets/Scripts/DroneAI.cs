using UnityEngine;
using UnityEngine.AI;

public class DroneAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 10f;
    public float attackRadius = 5f;
    public LayerMask playerLayer;
    public float fieldOfViewAngle = 120f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6f;
    public float alertDuration = 3f;

    [Header("Look Settings")]
    public Transform look; 

    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex = 0;
    private bool isAlerted = false;
    private float alertTimer = 0f;
    public bool isChasing { get; private set; }

    private float detectCooldown = 0.2f;
    private float detectTimer = 0f;
    private float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    // Propriété publique pour accéder au joueur
    public Transform Player => player;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        MoveToNextPatrolPoint();
    }

    private void Update()
    {
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
        foreach (var hit in hits)
        {
            Transform potentialPlayer = hit.transform;
            if (HasLineOfSight(potentialPlayer) && IsInFieldOfView(potentialPlayer))
            {
                player = potentialPlayer;
                isChasing = true;
                isAlerted = true;
                return;
            }
        }

        if (isAlerted)
        {
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertDuration)
            {
                isAlerted = false;
                alertTimer = 0f;
                player = null;
                isChasing = false;
            }
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

    private bool IsInFieldOfView(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= fieldOfViewAngle / 2f;
    }

    private void ChasePlayer()
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

    private void AttackPlayer()
    {
        Debug.Log("Drone is attacking the player!");
        // Effets d'attaque à implémenter
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * detectionRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, detectionRadius, NavMesh.AllAreas))
            {
                agent.destination = hit.position;
            }
        }
        else
        {
            agent.speed = patrolSpeed;
            agent.destination = patrolPoints[currentPatrolIndex].position;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void HandleAlertState()
    {
        alertTimer += Time.deltaTime;
        if (alertTimer >= alertDuration)
        {
            isAlerted = false;
            alertTimer = 0f;
            MoveToNextPatrolPoint();
        }
    }

    private void UpdateLookDirection()
    {
        if (look != null && player != null)
        {
            look.position = Vector3.Lerp(look.position, player.position, Time.deltaTime * 5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        if (look != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, look.position);
            Gizmos.DrawSphere(look.position, 0.2f);
        }

        Vector3 fovStart = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * transform.forward;
        Vector3 fovEnd = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, fovStart * detectionRadius);
        Gizmos.DrawRay(transform.position, fovEnd * detectionRadius);
    }
}
