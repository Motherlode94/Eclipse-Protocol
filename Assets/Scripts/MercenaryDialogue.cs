using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class MercenaryDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    public string[] dialogueLines = {
        "Besoin d'un combattant ? 100 crédits et je suis à vous.",
        "Je n'ai pas peur de la police ou des rebelles. Vous me payez, je me bats.",
        "Pensez-y bien, mercenaire ou ennemi ?"
    };
    public DialogueSystem dialogueSystem;

    [Header("Interaction")]
    public float interactionRadius = 3f;
    private bool isTalking = false;

    [Header("Références")]
    public Animator animator;
    public NavMeshAgent agent;
    public Transform player;
    private PlayerInput playerInput;
    private Mercenary mercenary;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        playerInput = player.GetComponent<PlayerInput>();
        mercenary = GetComponent<Mercenary>();

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
            if (playerInput.actions["Interact"].WasPressedThisFrame())
            {
                StartDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        isTalking = true;
        animator.SetBool("isTalking", true);
        dialogueSystem.StartChoiceDialogue("Voulez-vous recruter ce mercenaire ?", new string[] { "Oui", "Non" }, OnChoiceSelected);
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (choiceIndex == 0) mercenary.Recruit();
        isTalking = false;
        animator.SetBool("isTalking", false);
    }
}
