using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class RebelDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    public string[] dialogueLines = {
        "Rejoignez-nous pour renverser le système.",
        "Nous n'avons pas peur d'Eclipse.",
        "La résistance a besoin de vous."
    };
    public DialogueSystem dialogueSystem;

    [Header("Interaction")]
    public float interactionRadius = 3f; // Rayon d'interaction
    private bool isTalking = false;

    [Header("Références")]
    public Animator animator;
    public NavMeshAgent agent;
    public Transform player;
    private PlayerInput playerInput;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        playerInput = player.GetComponent<PlayerInput>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem non assigné !");
        }
    }

    private void Update()
    {
        if (player == null || isTalking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= interactionRadius)
        {
            StopAndFacePlayer();

            if (playerInput.actions["Interact"].WasPressedThisFrame())
            {
                StartDialogue();
            }
        }
    }

    private void StopAndFacePlayer()
    {
        agent.isStopped = true;
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        animator.SetBool("isWalking", false);
    }

    private void StartDialogue()
    {
        isTalking = true;

        animator.SetBool("isTalking", true);
        dialogueSystem.StartDialogue(dialogueLines);
    }

    private void EndDialogue()
    {
        isTalking = false;

        animator.SetBool("isTalking", false);
        agent.isStopped = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
