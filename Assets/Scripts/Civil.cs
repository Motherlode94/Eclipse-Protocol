using UnityEngine;
using UnityEngine.AI;

public class Civil : MonoBehaviour
{
    [Header("Comportement")]
    public bool canHelp = true; // Peut aider
    public bool canFlee = true; // Peut fuir en cas de danger
    public int reputationThreshold = 50; // Minimum de réputation pour aider le joueur
    public float detectionRadius = 5f; // Rayon pour détecter le joueur
    public float fleeDistance = 10f; // Distance à laquelle fuir

    [Header("Patrouille")]
    public Transform[] patrolPoints; // Points de patrouille
    private int currentPatrolIndex = 0;

    [Header("Références")]
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private PlayerStats playerStats; // Référence au script PlayerStats

    [Header("Couches (Layer Masks)")]
    public LayerMask dangerLayer; // Pour détecter les menaces (ex. : rebelles)

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

        if (agent == null) Debug.LogError("NavMeshAgent manquant sur " + gameObject.name);
        if (animator == null) Debug.LogError("Animator manquant sur " + gameObject.name);
    }

    void Update()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats non assigné. Impossible de déterminer la réputation.");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Vérifie si une menace est proche
        bool isInDanger = Physics.CheckSphere(transform.position, detectionRadius, dangerLayer);

        if (isInDanger && canFlee)
        {
            FleeFromDanger();
        }
        else if (distanceToPlayer < detectionRadius)
        {
            if (playerStats.Reputation >= reputationThreshold && canHelp)
            {
                ProvideHelp();
            }
            else
            {
                Debug.Log("Le civil est méfiant ou ne peut pas aider.");
                IdleBehavior();
            }
        }
        else
        {
            Patrol();
        }
    }

    void ProvideHelp()
    {
        Debug.Log("Le civil aide le joueur !");
        canHelp = false; // Une seule interaction
        agent.ResetPath();
        animator.SetTrigger("Help"); // Animation d'aide
    }

    void FleeFromDanger()
    {
        Vector3 directionAwayFromDanger = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + directionAwayFromDanger * fleeDistance;

        agent.SetDestination(fleeTarget);
        animator.SetBool("isRunning", true);

        Debug.Log("Le civil fuit le danger !");
    }

    void IdleBehavior()
    {
        agent.ResetPath();
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        Debug.Log("Le civil reste sur place.");
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            IdleBehavior();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        animator.SetBool("isWalking", true);
        animator.SetBool("isRunning", false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
