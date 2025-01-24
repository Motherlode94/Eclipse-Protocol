using UnityEngine;
using UnityEngine.AI;

public class DroneAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 10f;
    public float attackRadius = 5f;
    public LayerMask playerLayer;
    public float fieldOfViewAngle = 120f;
    public Transform Player => player;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6f;
    public float alertDuration = 3f;

    [Header("Look Settings")]
    public Transform look;

    [Header("Audio & Visual Effects")]
    public AudioClip alertSound;
    public AudioClip attackSound;
    public ParticleSystem alertEffect;

    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex = 0;
    private bool isAlerted = false;
    private float alertTimer = 0f;
    public bool isChasing { get; private set; }

    private float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    private AudioSource audioSource;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        agent.speed = patrolSpeed;

        if (patrolPoints.Length > 0)
        {
            MoveToNextPatrolPoint();
        }
        else
        {
            Debug.LogWarning("No patrol points assigned. Drone will stay idle.");
        }
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

        DetectPlayer();
        UpdateLookDirection();
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

                if (alertEffect != null && !alertEffect.isPlaying)
                {
                    alertEffect.Play();
                }

                if (alertSound != null && audioSource != null && !isAlerted)
                {
                    audioSource.PlayOneShot(alertSound);
                }

                return;
            }
        }

        // If no player detected and in alert state, reset after the timer
        if (isAlerted)
        {
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertDuration)
            {
                ResetAlertState();
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
            if (HasLineOfSight(player) && Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                AttackPlayer();
            }
        }
    }

    private void AttackPlayer()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Debug.Log("Drone is attacking the player!");
        // Add effects or damage logic here
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            agent.ResetPath();
            Debug.LogWarning("No patrol points available. Drone is idle.");
            return;
        }

        agent.speed = patrolSpeed;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void HandleAlertState()
    {
        alertTimer += Time.deltaTime;
        if (alertTimer >= alertDuration)
        {
            ResetAlertState();
        }
    }

    private void ResetAlertState()
    {
        isAlerted = false;
        alertTimer = 0f;
        player = null;
        isChasing = false;
        MoveToNextPatrolPoint();
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

        if (look != null && player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player.position);
        }

        Vector3 fovStart = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * transform.forward;
        Vector3 fovEnd = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, fovStart * detectionRadius);
        Gizmos.DrawRay(transform.position, fovEnd * detectionRadius);
    }
}

