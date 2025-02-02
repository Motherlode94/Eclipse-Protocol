using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Civil : MonoBehaviour
{
    public enum CivilState { Patrolling, Fleeing, Helping, Crouching, Idle }
    private CivilState currentState = CivilState.Patrolling;

    [Header("Comportement")]
    public bool canHelp = true;
    public bool canFlee = true;
    public int reputationThreshold = 50;
    public float detectionRadius = 5f;
    public float interactionRadius = 7f;
    public float fleeDistance = 10f;

    [Header("Dénonciation")]
    public float reportChance = 0.5f; // 50% de chance de dénoncer
    public float reportCooldown = 10f; // Cooldown entre deux dénonciations
    private bool canReport = true; // Booléen pour limiter les dénonciations

    [Header("Signalement")]
    public float signalRadius = 15f; // Rayon pour détecter les policiers proches
    public LayerMask policeLayer; // Couches pour les policiers détectables

    [Header("Patrouille")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Références")]
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private PlayerStats playerStats;
    public DialogueSystem dialogueSystem; // Référence au système de dialogue

    [Header("Couches (Layer Masks)")]
    public LayerMask dangerLayer;

    private PlayerInput playerInput;
    private bool isPlayerInRange = false;

    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
            playerInput = playerObject.GetComponent<PlayerInput>();
        }

        dialogueSystem = FindObjectOfType<DialogueSystem>();

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (animator == null) Debug.LogError("Animator manquant sur " + gameObject.name);
        if (agent == null) Debug.LogError("NavMeshAgent manquant sur " + gameObject.name);
    }

    void Update()
    {
        if (playerStats == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isInDanger = Physics.CheckSphere(transform.position, detectionRadius, dangerLayer);

        // Mise à jour de la portée du joueur
        isPlayerInRange = distanceToPlayer <= interactionRadius;

        // Gestion de la dénonciation
        if (playerStats.bounty >= playerStats.bountyThreshold1 && canReport && Random.value < reportChance)
        {
            ReportToPolice();
        }

        if (dialogueSystem != null) 
{
    dialogueSystem.CheckProximityAndCloseDialogue(player, transform, interactionRadius);
}

        // Gestion de la corruption
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C))
        {
            Corrupt();
        }

        // Comportement selon l'état
        switch (currentState)
        {
            case CivilState.Patrolling:
                if (isInDanger && canFlee)
                {
                    currentState = CivilState.Fleeing;
                }
                else if (isPlayerInRange && canHelp && playerStats.Reputation >= reputationThreshold)
                {
                    StopAndWaitForInteraction();
                }
                else
                {
                    Patrol();
                }
                break;

            case CivilState.Fleeing:
                FleeFromDanger();
                if (!isInDanger)
                {
                    currentState = CivilState.Patrolling;
                }
                break;

            case CivilState.Helping:
                if (!isPlayerInRange)
                {
                    EndInteraction();
                }
                break;

            case CivilState.Idle:
                if (!isPlayerInRange)
                {
                    ResumePatrol();
                }
                else
                {
                    IdleBehavior();
                }
                break;
        }

        UpdateAnimator(); // Synchronisation des animations
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
        }

        agent.isStopped = false;
        animator.SetBool("isWalking", true);
    }

    void StopAndWaitForInteraction()
    {
        currentState = CivilState.Idle;

        // Arrête le Civil et le tourne vers le joueur
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        Debug.Log("En attente de l'interaction du joueur...");
    }

    void IdleBehavior()
    {
        if (isPlayerInRange && playerInput.actions["Interact"].WasPressedThisFrame())
        {
            StartHelping();
        }

        agent.isStopped = true;
        animator.SetBool("isWalking", false);
    }

    void StartHelping()
    {
        Debug.Log("Le Civil commence à aider le joueur !");
        currentState = CivilState.Helping;
        canHelp = false;

        animator.SetBool("isTalking", true);

        // Dialogue
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(new string[]
            {
                "Bonjour, aventurier !",
                "Peux-tu m'aider à récupérer une épée perdue ?",
                "Merci pour ton aide !"
            });
        }
    }

    void ResumePatrol()
    {
        Debug.Log("Le joueur est parti, le Civil reprend sa patrouille.");
        currentState = CivilState.Patrolling;
        animator.SetBool("isTalking", false);
    }

    void FleeFromDanger()
    {
        Vector3 directionAwayFromDanger = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + directionAwayFromDanger * fleeDistance;

        agent.SetDestination(fleeTarget);
        agent.isStopped = false;
        Debug.Log("Le Civil fuit le danger !");
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

    private void ReportToPolice()
    {
        if (!canReport || playerStats.bounty < playerStats.bountyThreshold1) return;

        Debug.Log("Un civil dénonce le joueur !");
        canReport = false;

        playerStats.ModifyWantedLevel(10);

        Collider[] nearbyPolice = Physics.OverlapSphere(transform.position, signalRadius, policeLayer);
        foreach (Collider policeCollider in nearbyPolice)
        {
            Police police = policeCollider.GetComponent<Police>();
            if (police != null)
            {
                police.StartChase();
                break;
            }
        }

        Invoke(nameof(ResetReportCooldown), reportCooldown);
    }

    private void ResetReportCooldown()
    {
        canReport = true;
    }

    void EndInteraction()
    {
        Debug.Log("Le joueur s'éloigne pendant l'interaction, retour à la patrouille.");
        ResumePatrol();
    }

    void UpdateAnimator()
    {
        if (animator != null && agent != null)
        {
            Vector3 velocity = agent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);

            animator.SetFloat("PosX", localVelocity.x);
            animator.SetFloat("PosY", localVelocity.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, signalRadius);
    }
}
