using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Rebel : MonoBehaviour, IEnemy
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
    public PlayerStats playerStats;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null) Debug.LogError("NavMeshAgent manquant !");
        if (animator == null) Debug.LogError("Animator manquant !");
    }

    private void Update()
    {
        // Recherche d'une cible
        FindTarget();

        // Comportement principal
        if (target != null)
        {
            ChaseAndAttackTarget();
        }
        else
        {
            Patrol();
        }

        // Comportement basé sur la prime
        if (playerStats.bounty >= playerStats.bountyThreshold2)
        {
            AttackPlayer();
        }
        else if (playerStats.bounty >= playerStats.bountyThreshold1)
        {
            BecomeAggressive();
        }
    }

    public void AttackPlayer()
    {
        Debug.Log("Le rebelle attaque le joueur !");
        playerStats.TakeDamage(15); // Inflige 15 points de dégâts au joueur
        animator.SetTrigger("Attack");
    }

    public void BecomeAggressive()
    {
        Debug.Log("Le rebelle devient agressif !");
        // Recherche la cible la plus proche
        FindTarget();
    }

    private void FindTarget()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        if (targetsInRange.Length > 0)
        {
            target = targetsInRange[0].transform; // Priorise la première cible
        }
        else
        {
            target = null;
        }
    }

    public float GetThreatLevel()
    {
        // Exemple : les rebelles sont toujours une menace constante
        return 50f; // Valeur arbitraire
    }

    private void ChaseAndAttackTarget()
    {
        if (target == null) return;

        agent.SetDestination(target.position);
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            AttackTarget();
        }

        animator.SetBool("isRunning", distanceToTarget > attackRange);
        animator.SetBool("isAttacking", distanceToTarget <= attackRange);
    }

    private void AttackTarget()
    {
        Debug.Log("Le rebelle attaque " + target.name);
        animator.SetTrigger("Attack");

        // Appliquer des dégâts
        PlayerStats targetStats = target.GetComponent<PlayerStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(10); // Inflige 10 points de dégâts
        }
        else
        {
            Debug.LogWarning("La cible n'a pas de PlayerStats !");
        }
    }

    private void Patrol()
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

        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", true);
    }

    private void IdleBehavior()
    {
        agent.ResetPath();
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
    }

    private void OnDrawGizmosSelected()
    {
        // Rayon d'attaque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Rayon de détection
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + " subit " + damage + " points de dégâts !");
    }
}
