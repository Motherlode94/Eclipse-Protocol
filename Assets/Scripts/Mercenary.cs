using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mercenary : MonoBehaviour
{
    public enum State { Ally, Enemy, Neutral }
    public State mercenaryState = State.Neutral;

    [Header("Reputation Thresholds")]
    public int allyThreshold = 70; // Réputation pour devenir allié
    public int enemyThreshold = 30; // Réputation pour devenir ennemi

    [Header("Patrol Points (Neutral State)")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private PlayerStats playerStats; // Référence au script PlayerStats

    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime;

    void Start()
    {
        // Récupérer la référence du joueur et du script PlayerStats
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (playerStats == null)
        {
            Debug.LogError("Le script PlayerStats n'est pas attaché au joueur !");
        }
    }

    void Update()
    {
        if (playerStats != null)
        {
            // Mettre à jour l'état du mercenaire en fonction de la réputation
            UpdateState(playerStats.Reputation);
            HandleBehavior();
        }
    }

    void UpdateState(int reputation)
    {
        if (reputation >= allyThreshold)
        {
            mercenaryState = State.Ally;
        }
        else if (reputation <= enemyThreshold)
        {
            mercenaryState = State.Enemy;
        }
        else
        {
            mercenaryState = State.Neutral;
        }
    }

    void HandleBehavior()
    {
        switch (mercenaryState)
        {
            case State.Ally:
                FollowAndProtectPlayer();
                break;
            case State.Enemy:
                AttackPlayer();
                break;
            case State.Neutral:
                Patrol();
                break;
        }
    }

    void FollowAndProtectPlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
            animator.SetBool("isWalking", true);

            // Rechercher des ennemis proches dans le rayon d'attaque
            Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
            if (enemies.Length > 0)
            {
                AttackTarget(enemies[0].transform);
            }
        }
    }

    void AttackPlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
            animator.SetBool("isRunning", true);

            if (Vector3.Distance(transform.position, player.position) <= attackRange && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                animator.SetTrigger("Attack");
                Debug.Log("Attaque le joueur !");
            }
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            animator.SetBool("isWalking", true);
        }
    }

    void AttackTarget(Transform target)
    {
        agent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            animator.SetTrigger("Attack");
            Debug.Log("Attaque un ennemi proche : " + target.name);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Afficher le rayon d'attaque pour visualisation dans l'éditeur
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
