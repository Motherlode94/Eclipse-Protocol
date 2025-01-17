using UnityEngine;

public class DeityDialogueTrigger : MonoBehaviour
{
    public DialogueSystem dialogueSystem; // Référence au système de dialogue
    public string[] deityDialogueLines; // Lignes de dialogue de la divinité
    public string question; // Question posée par la divinité
    public string[] choices; // Choix proposés au joueur

    private bool isPlayerInRange = false; // Indique si le joueur est dans la zone
    private bool isDialogueInProgress = false; // Empêche les interactions multiples

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isDialogueInProgress) // Touche "E" pour interagir
        {
            StartDeityDialogue();
        }
    }

    private void StartDeityDialogue()
    {
        isDialogueInProgress = true; // Marque le début du dialogue
        dialogueSystem.StartDialogue(deityDialogueLines); // Démarre les lignes de dialogue

        // Affiche les choix après un délai (lorsque le dialogue est terminé)
        Invoke(nameof(DisplayChoices), deityDialogueLines.Length * 2f); // Ajustez le délai si nécessaire
    }

    private void DisplayChoices()
    {
        dialogueSystem.StartChoiceDialogue(question, choices, OnChoiceSelected);
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0:
                Debug.Log("Choix 1 : Vous avez accepté la bénédiction de la divinité.");
                // Ajoutez ici les conséquences spécifiques (ex : bonus de stats)
                GrantBlessing();
                break;

            case 1:
                Debug.Log("Choix 2 : Vous avez défié la divinité !");
                // Ajoutez ici les conséquences spécifiques (ex : démarrer un combat)
                StartCombat();
                break;

            case 2:
                Debug.Log("Choix 3 : Vous avez ignoré la divinité.");
                // Comportement si le joueur refuse ou ignore
                IgnoreDeity();
                break;

            default:
                Debug.Log("Choix inconnu.");
                break;
        }

        EndDialogue();
    }

    private void GrantBlessing()
    {
        // Exemple de logique pour accorder une bénédiction
        Debug.Log("Vous avez reçu une bénédiction divine ! Vos stats ont augmenté.");
        // Ajoutez ici des modifications aux stats du joueur ou d'autres effets
    }

    private void StartCombat()
    {
        // Exemple de logique pour démarrer un combat
        Debug.Log("La divinité entre en colère et vous attaque !");
        // Ajoutez ici la logique pour déclencher un combat
    }

    private void IgnoreDeity()
    {
        // Exemple de logique pour ignorer la divinité
        Debug.Log("Vous avez ignoré la divinité et quitté son sanctuaire.");
        // Ajoutez ici un comportement spécifique (par exemple, un message ou une pénalité)
    }

    private void EndDialogue()
    {
        isDialogueInProgress = false; // Dialogue terminé
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
