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
<<<<<<< Updated upstream
=======
        DetectPlayer();
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            Transform potentialPlayer = hits[0].transform;

            // Vérifie la ligne de vue
            if (HasLineOfSight(potentialPlayer))
            {
                player = potentialPlayer;
                isChasing = Vector3.Distance(transform.position, player.position) <= detectionRadius;
                isAlerted = false; // Stop alerte si on voit le joueur
                alertTimer = 0f;
                return;
            }
        }

        // Si aucune détection, passe en mode alerte
        if (isChasing)
        {
            isChasing = false;
            isAlerted = true;
            alertTimer = alertDuration;
=======
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
>>>>>>> Stashed changes
        }
        else
        {
            player = null;
            isChasing = false;
<<<<<<< Updated upstream
            isAlerted = false;
=======
            agent.speed = patrolSpeed; // Retour à la vitesse de patrouille
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
    private void HandleAlertState()
    {
        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0f)
        {
            isAlerted = false;
            MoveToNextPatrolPoint();
        }
        else
        {
            Debug.Log("Drone is in alert mode...");
            // Optionnel : Ajouter des animations ou des mouvements aléatoires pendant l'alerte
        }
    }

    private void UpdateLookDirection()
    {
        // Si le drone est immobile, ne pas ajuster la rotation
        if (agent.velocity.sqrMagnitude <= 0.01f) return;

        Vector3 lookDirection;

        // Détermine la direction à suivre
        if (isChasing && player != null)
        {
            lookDirection = player.position - transform.position;
        }
        else if (look != null)
        {
            lookDirection = look.position - transform.position;
        }
        else
        {
            lookDirection = agent.velocity; // Direction actuelle de déplacement
        }

        lookDirection.y = 0; // Ignore les différences de hauteur

        // Ajuste la rotation uniquement si la direction est significative
        if (lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
=======
    private bool HasLineOfSight(Transform target)
    {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, detectionRadius))
        {
            return hit.transform == target;
        }
        return false;
>>>>>>> Stashed changes
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
