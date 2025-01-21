using UnityEngine;
using UnityEngine.AI;

public class CompanionAI : MonoBehaviour
{
    public enum CompanionState { Idle, Follow, Attack }
    public CompanionState currentState = CompanionState.Idle;

    [Header("References")]
    public Transform player; // La cible que le compagnon doit suivre
    public Animator animator; // Référence à l'Animator
    public Transform attackTarget; // La cible actuelle d'attaque

    [Header("Settings")]
    public float followDistance = 5f; // Distance minimale pour suivre le joueur
    public float attackRange = 2f; // Distance pour attaquer une cible
    public float attackCooldown = 1.5f; // Temps entre les attaques

    private NavMeshAgent agent;
    private float nextAttackTime;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case CompanionState.Idle:
                IdleBehavior();
                break;

            case CompanionState.Follow:
                FollowPlayer();
                break;

            case CompanionState.Attack:
                AttackBehavior();
                break;
        }
    }

    /// <summary>
    /// Comportement lorsque le compagnon est en mode idle (ne fait rien).
    /// </summary>
    private void IdleBehavior()
    {
        agent.ResetPath();
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    /// <summary>
    /// Le compagnon suit le joueur lorsqu'il est trop éloigné.
    /// </summary>
    private void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > followDistance)
        {
            agent.SetDestination(player.position);
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
        else
        {
            agent.ResetPath();
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
        }
    }

    /// <summary>
    /// Comportement d'attaque lorsqu'une cible est assignée.
    /// </summary>
    private void AttackBehavior()
    {
        if (attackTarget == null)
        {
            currentState = CompanionState.Follow;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, attackTarget.position);

        if (distanceToTarget > attackRange)
        {
            agent.SetDestination(attackTarget.position);
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
        else
        {
            agent.ResetPath();
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                AttackTarget();
            }
        }
    }

    /// <summary>
    /// Définit une cible pour attaquer.
    /// </summary>
    /// <param name="target">La cible à attaquer.</param>
    public void AssignTarget(Transform target)
    {
        attackTarget = target;
        currentState = CompanionState.Attack;
    }

    /// <summary>
    /// Comportement d'attaque réel (animations, dégâts, etc.).
    /// </summary>
    private void AttackTarget()
    {
        Debug.Log("Compagnon attaque " + attackTarget.name);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Ajoutez ici la logique de dégâts si la cible a un script de santé
        if (attackTarget.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(10); // Exemple : inflige 10 points de dégâts
        }
    }

    /// <summary>
    /// Reprend le mode de suivi après avoir attaqué ou lorsque la cible n'existe plus.
    /// </summary>
    public void ReturnToFollow()
    {
        attackTarget = null;
        currentState = CompanionState.Follow;
    }

    /// <summary>
    /// Définir l'état en attente.
    /// </summary>
    public void SetIdle()
    {
        currentState = CompanionState.Idle;
    }

    /// <summary>
    /// Définir l'état de suivi.
    /// </summary>
    public void SetFollow()
    {
        currentState = CompanionState.Follow;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualiser les rayons pour le suivi et l'attaque
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
