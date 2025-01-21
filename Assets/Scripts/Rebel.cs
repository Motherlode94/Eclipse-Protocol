using UnityEngine;
using UnityEngine.AI;

public class Rebel : MonoBehaviour
{
    [Header("Comportement")]
    public float attackRange = 2f; // Distance d'attaque
    public float detectionRadius = 10f; // Rayon de détection des cibles
    public float attackCooldown = 1.5f; // Temps entre deux attaques
    private float nextAttackTime;

    [Header("Patrouille")]
    public Transform[] patrolPoints; // Points de patrouille
    private int currentPatrolIndex = 0;

    [Header("Couches (Layer Masks)")]
    public LayerMask targetLayer; // Cibles potentielles (Player, Civils)

    [Header("Références")]
    private Transform target; // La cible actuelle
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        // Récupérer les composants nécessaires
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null) Debug.LogError("NavMeshAgent manquant sur " + gameObject.name);
        if (animator == null) Debug.LogError("Animator manquant sur " + gameObject.name);
    }

    void Update()
    {
        // Recherche d'une cible dans le rayon de détection
        FindTarget();

        if (target != null)
        {
            ChaseAndAttackTarget();
        }
        else
        {
            Patrol();
        }
    }

    void FindTarget()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        if (targetsInRange.Length > 0)
        {
            // Priorise la première cible détectée (peut être amélioré pour prioriser certains types)
            target = targetsInRange[0].transform;
        }
        else
        {
            target = null;
        }
    }

    void ChaseAndAttackTarget()
    {
        if (target == null) return;

        agent.SetDestination(target.position);
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            AttackTarget();
        }

        // Gestion des animations
        animator.SetBool("isRunning", distanceToTarget > attackRange);
        animator.SetBool("isAttacking", distanceToTarget <= attackRange);
    }

    void AttackTarget()
    {
        // Animation ou logique d'attaque
        Debug.Log("Le rebelle attaque " + target.name);
        animator.SetTrigger("Attack");

        // Ajouter des dégâts à la cible
        PlayerStats playerStats = target.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(10); // Inflige 10 points de dégâts
        }
        else
        {
            Debug.LogWarning("La cible n'a pas de script PlayerStats !");
        }
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
            // Passer au prochain point de patrouille
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", true);
    }

    void IdleBehavior()
    {
        agent.ResetPath();
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
    }

    void OnDrawGizmosSelected()
    {
        // Visualisation des rayons de détection et d'attaque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
