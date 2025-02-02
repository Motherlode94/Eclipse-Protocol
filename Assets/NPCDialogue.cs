using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    public string[] dialogueLines; // Lignes de dialogue linéaire
    public string question; // Question pour le choix
    public string[] choices; // Options pour le choix
    public DialogueSystem dialogueSystem; // Référence au système de dialogue

    [Header("Interaction")]
    public float interactionRadius = 5f; // Rayon de détection pour interagir
    public Animator animator; // Référence à l'Animator
    private bool isTalking = false; // Vérifie si un dialogue est en cours
    private bool isPlayerInRange = false; // Vérifie si le joueur est à portée
    private Transform player; // Référence au joueur
    private NavMeshAgent agent; // Référence au NavMeshAgent

    private PlayerInput playerInput; // Référence à la nouvelle Input System

    private void Start()
    {
        // Récupération des références
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        playerInput = player?.GetComponent<PlayerInput>(); // Récupère la configuration Input du joueur

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem non assigné dans l'inspecteur !");
        }
    }

    private void Update()
    {
        if (player == null || isTalking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Vérifie si le joueur est dans le rayon d'interaction
        if (distanceToPlayer <= interactionRadius)
        {
            isPlayerInRange = true;
            StopAndFacePlayer();

            // Vérifie si la touche d'interaction est pressée
            if (playerInput != null && playerInput.actions["Interact"].WasPressedThisFrame())
            {
                StartDialogue();
            }
        }
        else
        {
            isPlayerInRange = false;

            // Si le joueur sort du rayon et que le dialogue n'est pas actif, reprendre la patrouille
            if (!isTalking)
            {
                ResumeMovement();
            }
        }
    }

    private void StopAndFacePlayer()
    {
        if (agent != null)
        {
            agent.isStopped = true; // Stoppe immédiatement le NPC
            agent.velocity = Vector3.zero; // Évite un mouvement résiduel
        }

        // Oriente le NPC vers le joueur
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void StartDialogue()
    {
    isTalking = true;

    if (agent != null)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    if (animator != null)
    {
        animator.SetBool("isTalking", true);
    }

    if (dialogueSystem != null)
    {
        Debug.Log("Démarrage du dialogue !");
        if (choices != null && choices.Length > 0)
        {
            dialogueSystem.StartChoiceDialogue(question, choices, OnChoiceSelected);
        }
        else if (dialogueLines != null && dialogueLines.Length > 0)
        {
            dialogueSystem.StartDialogue(dialogueLines);
        }
    }
    else
    {
        Debug.LogError("DialogueSystem n'est pas assigné !");
    }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"Choix sélectionné : {choices[choiceIndex]}");
        EndDialogue();
    }

    private void EndDialogue()
    {
        isTalking = false;

        if (animator != null)
        {
            animator.SetBool("isTalking", false);
        }

        ResumeMovement();
    }

    private void ResumeMovement()
    {
        if (agent != null)
        {
            agent.isStopped = false;
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Affiche une sphère pour représenter le rayon d'interaction
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
