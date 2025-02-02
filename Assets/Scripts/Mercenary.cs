using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Mercenary : MonoBehaviour, IRecruitable, IEnemy
{
    public enum MercenaryState { Patrolling, Fighting, Following, Idle }
    private MercenaryState currentState = MercenaryState.Patrolling;

    [Header("Paramètres")]
    public int recruitmentCost = 100; // Coût du recrutement
    public float detectionRadius = 10f; // Rayon pour détecter les ennemis
    public float attackRange = 2f; // Distance d'attaque
    public float attackCooldown = 1.5f; // Temps entre deux attaques
    private float nextAttackTime;
    private DialogueSystem dialogueSystem;
public float interactionRadius = 5f; // Rayon de l'interaction avec l'objet

    [Header("Patrouille")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Références")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;
    public PlayerStats playerStats;

    [Header("Couches (Layer Masks)")]
    public LayerMask enemyLayer;
    private Transform currentEnemy;

    private bool isRecruited = false;

    public bool IsRecruited => isRecruited;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

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
    }

    private void Start()
{
    dialogueSystem = FindObjectOfType<DialogueSystem>();
}

private void Update()
{
    if (playerStats == null) return;

    if (isRecruited)
    {
        FollowPlayer();
    }
    else
    {
        FindEnemy();
        switch (currentState)
        {
            case MercenaryState.Patrolling:
                Patrol();
                if (currentEnemy != null) currentState = MercenaryState.Fighting;
                break;

            case MercenaryState.Fighting:
                FightEnemy();
                break;

            case MercenaryState.Idle:
                IdleBehavior();
                break;
        }
    }

    // Vérification de la distance et fermeture du dialogue si nécessaire
    if (dialogueSystem != null)
    {
        CheckProximityAndCloseDialogue(player, transform, interactionRadius);
    }

    // Gestion de la prime du joueur
    if (playerStats.bounty >= playerStats.bountyThreshold2)
    {
        AttackPlayer(); // Devient hostile envers le joueur
    }
    else if (playerStats.bounty >= playerStats.bountyThreshold1)
    {
        BecomeAggressive(); // Adopte un comportement agressif
    }
}

public void CheckProximityAndCloseDialogue(Transform player, Transform dialogueTarget, float interactionRadius)
{
    float distance = Vector3.Distance(player.position, dialogueTarget.position);
    if (distance > interactionRadius && dialogueSystem != null)
    {
        Debug.Log("Le joueur est hors de portée. Fermeture du dialogue.");
        dialogueSystem.CloseDialogue();
    }
}

    public void AttackPlayer()
    {
        Debug.Log("Le mercenaire attaque le joueur !");
        playerStats.TakeDamage(20); // Inflige 20 points de dégâts au joueur
        animator.SetTrigger("Attack");
    }

    public void BecomeAggressive()
    {
        Debug.Log("Le mercenaire devient agressif envers le joueur !");
        currentState = MercenaryState.Fighting;
        agent.SetDestination(player.position);
    }

    public void Recruit()
    {
        if (playerStats.currentMoney >= recruitmentCost)
        {
            playerStats.currentMoney -= recruitmentCost;
            isRecruited = true;
            currentState = MercenaryState.Following;
            Debug.Log(gameObject.name + " a été recruté !");
        }
        else
        {
            Debug.Log("Pas assez d'argent pour recruter ce mercenaire !");
        }
    }


    public void Dismiss()
    {
        isRecruited = false;
        currentState = MercenaryState.Patrolling;
        Debug.Log(gameObject.name + " a quitté votre service !");
    }

    private void FollowPlayer()
    {
        agent.SetDestination(player.position);
        agent.isStopped = false;
        animator.SetBool("isWalking", true);
    }

    private void FindEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        if (enemiesInRange.Length > 0)
        {
            currentEnemy = enemiesInRange[0].transform;
        }
        else
        {
            currentEnemy = null;
        }
    }

    public float GetThreatLevel()
{
    // Exemple de calcul de menace basé sur l'état du mercenaire
    if (IsRecruited)
    {
        return 0f; // Si le mercenaire est recruté, il n'est pas une menace
    }

    switch (currentState)
    {
        case MercenaryState.Fighting:
            return 100f; // Très menaçant lorsqu'il combat
        case MercenaryState.Patrolling:
            return 20f; // Peu menaçant lorsqu'il patrouille
        case MercenaryState.Idle:
            return 10f; // Très peu menaçant lorsqu'il est inactif
        default:
            return 50f; // Valeur par défaut
    }
}


    private void FightEnemy()
    {
        if (currentEnemy == null)
        {
            currentState = MercenaryState.Patrolling;
            return;
        }

        agent.SetDestination(currentEnemy.position);
        float distanceToEnemy = Vector3.Distance(transform.position, currentEnemy.position);

        if (distanceToEnemy <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            AttackEnemy();
        }

        animator.SetBool("isRunning", distanceToEnemy > attackRange);
        animator.SetBool("isAttacking", distanceToEnemy <= attackRange);
    }

    private void AttackEnemy()
    {
        Debug.Log("Le mercenaire attaque " + currentEnemy.name);
        animator.SetTrigger("Attack");

        IEnemy enemy = currentEnemy.GetComponent<IEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(20f);
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

        animator.SetBool("isWalking", true);
    }

    private void IdleBehavior()
    {
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
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
