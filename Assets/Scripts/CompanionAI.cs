using UnityEngine;
using UnityEngine.AI;

public class CompanionAI : MonoBehaviour
{
    public enum CompanionState { Idle, Follow, Attack, Patrol }
    public CompanionState currentState = CompanionState.Idle;

    [Header("References")]
    public Transform player; // Cible que le compagnon doit suivre
    public Animator animator; // Référence à l'Animator
    public Transform attackTarget; // Cible actuelle d'attaque

    [Header("Settings")]
    public float followDistance = 5f; // Distance minimale pour suivre le joueur
    public float attackRange = 2f; // Distance pour attaquer une cible
    public float attackCooldown = 1.5f; // Temps entre les attaques
    public float patrolRadius = 10f; // Rayon de patrouille

    [Header("Effects")]
    public ParticleSystem attackEffect; // Effet visuel lors de l'attaque
    public AudioClip attackSound; // Son joué lors de l'attaque
    private AudioSource audioSource;

    private NavMeshAgent agent;
    private float nextAttackTime;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
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

            case CompanionState.Patrol:
                PatrolBehavior();
                break;
        }
    }

    private void IdleBehavior()
    {
        agent.ResetPath();
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

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

    private void AttackBehavior()
    {
        if (attackTarget == null)
        {
            ReturnToFollow();
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
                PerformAttack();
            }
        }
    }

    private void PatrolBehavior()
    {
        if (!agent.hasPath)
        {
            Vector3 randomPoint = Random.insideUnitSphere * patrolRadius + transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }
    }

    private void PerformAttack()
    {
        Debug.Log("Compagnon attaque " + attackTarget.name);

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackEffect != null)
        {
            attackEffect.Play();
        }

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        if (attackTarget.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(10); // Exemple : inflige 10 points de dégâts
        }
    }

    public void AssignTarget(Transform target)
    {
        attackTarget = target;
        currentState = CompanionState.Attack;
    }

    public void ReturnToFollow()
    {
        attackTarget = null;
        currentState = CompanionState.Follow;
    }

    public void SetIdle()
    {
        currentState = CompanionState.Idle;
    }

    public void SetFollow()
    {
        currentState = CompanionState.Follow;
    }

    public void SetPatrol()
    {
        currentState = CompanionState.Patrol;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
