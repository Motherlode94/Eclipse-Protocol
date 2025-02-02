using System.Collections; // Pour IEnumerator
using UnityEngine;
using UnityEngine.AI;

public class Police : MonoBehaviour, IEnemy
{
    public enum PoliceState { Patrolling, Chasing, Arresting, Fighting, Investigating, Idle }
    private PoliceState currentState = PoliceState.Patrolling;

    [Header("Paramètres")]
    public int wantedThreshold = 50; // Niveau de recherche minimum pour poursuivre le joueur
    public float arrestRange = 2f; // Distance pour arrêter ou attaquer
    public float detectionRadius = 10f; // Rayon de détection
    public float patrolWaitTime = 3f; // Temps d'attente à un point de patrouille
    public float patrolSpeed = 2f; // Vitesse de patrouille
    public float chaseSpeed = 5f; // Vitesse de poursuite
    public float attackCooldown = 1.5f; // Temps entre les attaques
    private float nextAttackTime;

    [Header("Références")]
    public Transform[] patrolPoints;
    public Animator animator;
    public NavMeshAgent agent;
    public Transform player;
    public PlayerStats playerStats;

    [Header("Couches (Layer Masks)")]
    public LayerMask enemyLayer; // Rebelles considérés comme ennemis
    private Transform currentEnemy; // L'ennemi actuel (rebelle)

    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    public bool IsChasingPlayer { get; private set; }
    private bool isPlayerInRange; // Vérifie si le joueur est dans le rayon de détection


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
        public void StartChase()
    {
        Debug.Log("La police commence à poursuivre le joueur !");
        currentState = PoliceState.Chasing;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
        IsChasingPlayer = true;
    }

    private void Update()
    {
                if (playerStats == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= detectionRadius;

        FindEnemy();

        switch (currentState)
        {
            case PoliceState.Patrolling:
                Patrol();
                if (currentEnemy != null) currentState = PoliceState.Fighting;
                else if (isPlayerInRange && playerStats.WantedLevel >= wantedThreshold)
                    currentState = PoliceState.Chasing;
                break;

            case PoliceState.Chasing:
                ChasePlayer();
                if (distanceToPlayer <= arrestRange) currentState = PoliceState.Arresting;
                else if (playerStats.WantedLevel < wantedThreshold || distanceToPlayer > detectionRadius * 1.5f)
                    currentState = PoliceState.Patrolling;
                break;

            case PoliceState.Fighting:
                FightEnemy();
                break;

            case PoliceState.Arresting:
                ArrestPlayer();
                break;

            case PoliceState.Idle:
                IdleBehavior();
                break;
        }

        HandleCorruption();

    }

        private void HandleCorruption()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C))
        {
            Corrupt();
        }
    }

    public void Corrupt()
    {
        if (playerStats.currentMoney >= 50)
        {
            playerStats.currentMoney -= 50;
            playerStats.ModifyWantedLevel(-20);
            Debug.Log("Corruption réussie !");
        }
        else
        {
            Debug.Log("Pas assez d'argent pour corrompre !");
        }
    }
    private void FindEnemy()
    {
    Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
    Transform priorityEnemy = null;

    float maxThreat = 0f;
    foreach (Collider enemy in enemiesInRange)
    {
        IEnemy enemyComponent = enemy.GetComponent<IEnemy>();
        if (enemyComponent != null)
        {
            float threatLevel = enemyComponent.GetThreatLevel(); // Exemple : méthode GetThreatLevel
            if (threatLevel > maxThreat)
            {
                maxThreat = threatLevel;
                priorityEnemy = enemy.transform;
            }
        }
    }

    currentEnemy = priorityEnemy;
    }
    public float GetThreatLevel()
{
    // Exemple : la menace est basée sur le WantedLevel du joueur
    return playerStats != null ? playerStats.WantedLevel : 0;
}



    private void FightEnemy()
    {
        if (currentEnemy == null)
        {
            currentState = PoliceState.Patrolling;
            return;
        }

        agent.SetDestination(currentEnemy.position);
        float distanceToEnemy = Vector3.Distance(transform.position, currentEnemy.position);

        if (distanceToEnemy <= arrestRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            AttackEnemy();
        }

        animator.SetBool("isRunning", distanceToEnemy > arrestRange);
        animator.SetBool("isAttacking", distanceToEnemy <= arrestRange);
    }

    private void AttackEnemy()
    {
        Debug.Log("La police attaque " + currentEnemy.name);
        animator.SetTrigger("Attack");

        IEnemy enemy = currentEnemy.GetComponent<IEnemy>();
        enemy?.TakeDamage(20f);
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            IdleBehavior();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }

        agent.speed = patrolSpeed;
        agent.isStopped = false;
        animator.SetBool("isWalking", true);
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(patrolWaitTime);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        agent.isStopped = false;
        isWaiting = false;
    }

    private void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
        animator.SetBool("isRunning", true);
    }

    private void ArrestPlayer()
    {
    agent.isStopped = true;
    animator.SetTrigger("Arrest");
    Debug.Log("Le joueur est arrêté !");

    // Effet : réduire le WantedLevel et bloquer les mouvements du joueur
    if (playerStats != null)
    {
        playerStats.WantedLevel = 0;
        playerStats.TakeDamage(0); // Empêche les mouvements, ou réduit sa santé
    }

    StartCoroutine(ResumePatrolAfterArrest());
    }


    private IEnumerator ResumePatrolAfterArrest()
{
    yield return new WaitForSeconds(3f); // Temps de pause après arrestation
    currentState = PoliceState.Patrolling;
    agent.isStopped = false;
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
        // Logique pour gérer les dégâts (par exemple, réduire les points de vie)
    }
    public void AttackPlayer()
{
    Debug.Log("La police attaque le joueur !");
    playerStats.TakeDamage(10); // Inflige des dégâts au joueur
    animator.SetTrigger("Attack");
}

public void BecomeAggressive()
{
    Debug.Log("La police devient agressive !");
    currentState = PoliceState.Chasing;
    agent.speed = chaseSpeed;
    agent.SetDestination(player.position);
}
   
}
